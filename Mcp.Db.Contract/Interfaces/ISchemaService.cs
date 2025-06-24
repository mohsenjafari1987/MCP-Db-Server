using Mcp.Db.Contract.Models;

namespace Mcp.Db.Contract.Interfaces;

public interface ISchemaService
{
    /// <summary>
    /// Returns the schema details for a single table.
    /// </summary>
    /// <param name="tableName">Name of the table (case-sensitive in some DBs)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Schema of the specified table</returns>
    Task<TableSchema> GetSchemaForTableAsync(string tableName, CancellationToken cancellationToken);

    /// <summary>
    /// Returns the schema for all user-defined tables in the connected database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of table schemas</returns>
    Task<List<TableSchema>> GetAllSchemasAsync(CancellationToken cancellationToken);
}
