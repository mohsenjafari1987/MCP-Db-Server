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
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        // Explicit read-only transaction
        await using var tx = await conn.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted, cancellationToken);

        // Enforce read-only at session level for this transaction
        await using (var readOnlyCmd = new NpgsqlCommand("SET TRANSACTION READ ONLY", conn, tx))
        {
            await readOnlyCmd.ExecuteNonQueryAsync(cancellationToken);
        }

        // Execute the actual user query
        await using var cmd = new NpgsqlCommand(request.Query, conn, tx);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var result = new McpQueryResult
        {
            Columns = Enumerable.Range(0, reader.FieldCount)
                .Select(reader.GetName)
                .ToList(),
            Rows = new()
        };

        while (await reader.ReadAsync(cancellationToken))
        {
            var row = new List<object?>();
            for (var i = 0; i < reader.FieldCount; i++)
                row.Add(reader.IsDBNull(i) ? null : reader.GetValue(i));
            result.Rows.Add(row);
        }
        
        await reader.DisposeAsync();

        await tx.CommitAsync(cancellationToken); // safe to commit read-only
        return result;
    }

}
