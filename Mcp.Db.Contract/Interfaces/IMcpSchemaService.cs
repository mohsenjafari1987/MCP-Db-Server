using Mcp.Db.Contract.Models;

namespace Mcp.Db.Contract.Interfaces;

public interface IMcpSchemaService
{
    Task<TableSchema> GetSchemaForTableAsync(string tableName, CancellationToken cancellationToken);
    Task<List<TableSchema>> GetAllSchemasAsync(CancellationToken cancellationToken);
}
