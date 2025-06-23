using System.Data;
using Mcp.Db.Contract.Interfaces;
using Mcp.Db.Contract.Models;
using Npgsql;

namespace Mcp.Db.Infrastructure.Postgres;

public class PostgresMcpQueryService : IMcpQueryService
{
    private readonly string _connectionString;

    public PostgresMcpQueryService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<McpQueryResult> ExecuteQueryAsync(McpQueryRequest request, CancellationToken cancellationToken = default)
    {
        var sql = request.Query.Trim();

        if (!sql.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Only SELECT queries are supported in read-only mode.");

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        await using var cmd = new NpgsqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var result = new McpQueryResult
        {
            Columns = Enumerable.Range(0, reader.FieldCount)
                .Select(reader.GetName)
                .ToList(),
            Rows = new List<List<object?>>()
        };

        while (await reader.ReadAsync(cancellationToken))
        {
            var row = new List<object?>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                row.Add(reader.IsDBNull(i) ? null : reader.GetValue(i));
            }

            result.Rows.Add(row);
        }

        return result;
    }
}
