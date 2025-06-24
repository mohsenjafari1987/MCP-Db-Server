using Mcp.Db.Contract.Interfaces;
using Mcp.Db.Contract.Models;

namespace Mcp.Db.Application.Services;

public class SchemaCoordinator
{
    private readonly IMcpSchemaService _schemaService;

    public SchemaCoordinator(IMcpSchemaService schemaService)
    {
        _schemaService = schemaService;
    }

    public Task<TableSchema> GetSchemaForTableAsync(string tableName, CancellationToken ct = default)
        => _schemaService.GetSchemaForTableAsync(tableName, ct);

    public Task<List<TableSchema>> GetAllSchemasAsync(CancellationToken ct = default)
        => _schemaService.GetAllSchemasAsync(ct);
}
