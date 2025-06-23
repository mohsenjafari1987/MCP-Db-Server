using Mcp.Db.Contract.Models;

namespace Mcp.Db.Contract.Interfaces
{

    public interface IMcpQueryService
    {
        Task<McpQueryResult> ExecuteQueryAsync(McpQueryRequest request, CancellationToken cancellationToken = default);
    }
}
