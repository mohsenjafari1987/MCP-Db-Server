using Mcp.Db.Contract.Interfaces;
using Mcp.Db.Contract.Models;
using Npgsql;
using System.Text.RegularExpressions;

namespace Mcp.Db.Infrastructure.Postgres
{
    public class PostgresSchemaService : ISchemaService
    {
        private readonly string _connectionString;

        public PostgresSchemaService(string connectionString)
        {
            _connectionString = connectionString;
        }

        private async Task<string?> GetTableDescriptionAsync(NpgsqlConnection conn, string tableName, CancellationToken ct)
        {
            var cmd = new NpgsqlCommand("SELECT obj_description(@table::regclass, 'pg_class')", conn);
            cmd.Parameters.AddWithValue("table", tableName);
            var result = await cmd.ExecuteScalarAsync(ct);
            return result == DBNull.Value ? null : result?.ToString();
        }

        private async Task<List<IndexSchema>> GetIndexesAsync(NpgsqlConnection conn, string tableName, CancellationToken ct)
        {
            var indexes = new List<IndexSchema>();

            var cmd = new NpgsqlCommand(@"
                    SELECT indexname, indexdef
                    FROM pg_indexes
                    WHERE schemaname = 'public' AND tablename = @table;
                ", conn);
            cmd.Parameters.AddWithValue("table", tableName);

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var indexName = reader.GetString(0);
                var indexDef = reader.GetString(1);

                var isUnique = indexDef.StartsWith("CREATE UNIQUE INDEX", StringComparison.OrdinalIgnoreCase);

                var match = Regex.Match(indexDef, @"\((.*?)\)");
                var columns = match.Success ? match.Groups[1].Value.Split(',').Select(s => s.Trim()).ToList() : new List<string>();

                indexes.Add(new IndexSchema
                {
                    IndexName = indexName,
                    IsUnique = isUnique,
                    Columns = columns
                });
            }

            return indexes;
        }

        private async Task<List<ForeignKeySchema>> GetForeignKeysAsync(NpgsqlConnection conn, string tableName, CancellationToken ct)
        {
            var fks = new List<ForeignKeySchema>();

            var cmd = new NpgsqlCommand(@"
                        SELECT
                            kcu.column_name,
                            ccu.table_name AS foreign_table,
                            ccu.column_name AS foreign_column,
                            rc.update_rule,
                            rc.delete_rule
                        FROM information_schema.table_constraints tc
                        JOIN information_schema.key_column_usage kcu
                            ON tc.constraint_name = kcu.constraint_name
                        JOIN information_schema.constraint_column_usage ccu
                            ON ccu.constraint_name = tc.constraint_name
                        JOIN information_schema.referential_constraints rc
                            ON rc.constraint_name = tc.constraint_name
                        WHERE tc.constraint_type = 'FOREIGN KEY'
                          AND tc.table_name = @table;
                    ", conn);
            cmd.Parameters.AddWithValue("table", tableName);

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                fks.Add(new ForeignKeySchema
                {
                    Column = reader.GetString(0),
                    ReferencesTable = reader.GetString(1),
                    ReferencesColumn = reader.GetString(2),
                    OnUpdate = reader.GetString(3),
                    OnDelete = reader.GetString(4)
                });
            }

            return fks;
        }
        public async Task<TableSchema> GetSchemaForTableAsync(string tableName, CancellationToken cancellationToken)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);

            var description = await GetTableDescriptionAsync(conn, tableName, cancellationToken);
            var indexes = await GetIndexesAsync(conn, tableName, cancellationToken);
            var foreignKeys = await GetForeignKeysAsync(conn, tableName, cancellationToken);

            var cmd = new NpgsqlCommand(@"
                    SELECT
                        a.attname AS column_name,
                        pg_catalog.format_type(a.atttypid, a.atttypmod) AS data_type,
                        NOT a.attnotnull AS is_nullable,
                        pg_get_expr(ad.adbin, ad.adrelid) AS default_value,
                        pgd.description AS column_description,
                        EXISTS (
                            SELECT 1 FROM pg_index i
                            WHERE i.indrelid = c.oid AND a.attnum = ANY(i.indkey) AND i.indisprimary
                        ) AS is_primary
                    FROM pg_attribute a
                    JOIN pg_class c ON c.oid = a.attrelid
                    JOIN pg_namespace n ON n.oid = c.relnamespace
                    LEFT JOIN pg_attrdef ad ON ad.adrelid = c.oid AND ad.adnum = a.attnum
                    LEFT JOIN pg_description pgd ON pgd.objoid = a.attrelid AND pgd.objsubid = a.attnum
                    WHERE c.relname = @table AND a.attnum > 0 AND NOT a.attisdropped AND n.nspname = 'public'
                    ORDER BY a.attnum;
    ", conn);
            cmd.Parameters.AddWithValue("table", tableName);

            var columns = new List<ColumnSchema>();
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                columns.Add(new ColumnSchema
                {
                    ColumnName = reader.GetString(0),
                    DataType = reader.GetString(1),
                    IsNullable = reader.GetBoolean(2),
                    DefaultValue = reader.IsDBNull(3) ? null : reader.GetString(3),
                    Description = reader.IsDBNull(4) ? null : reader.GetString(4),
                    IsPrimaryKey = reader.GetBoolean(5)
                });
            }

            return new TableSchema
            {
                TableName = tableName,
                Description = description,
                Columns = columns,
                Indexes = indexes,
                Relations = foreignKeys
            };
        }

        public async Task<List<TableSchema>> GetAllSchemasAsync(CancellationToken cancellationToken)
        {
            var tables = new List<TableSchema>();

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);

            var cmd = new NpgsqlCommand(@"
                    SELECT table_name
                    FROM information_schema.tables
                    WHERE table_schema = 'public'
                    ORDER BY table_name;
                ", conn);

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            var tableNames = new List<string>();
            while (await reader.ReadAsync(cancellationToken))
                tableNames.Add(reader.GetString(0));

            foreach (var tableName in tableNames)
            {
                var schema = await GetSchemaForTableAsync(tableName, cancellationToken);
                tables.Add(schema);
            }

            return tables;
        }


    }
}
