using Mcp.Db.Contract.Interfaces;
using Mcp.Db.Contract.Models;

namespace Mcp.Db.Application.Services;

public class McpQueryCoordinator
{
    private readonly IMcpQueryService _queryService;

    public McpQueryCoordinator(IMcpQueryService queryService)
    {
        _queryService = queryService;
    }

    public Task<McpQueryResult> ExecuteQueryAsync(McpQueryRequest request, CancellationToken cancellationToken = default)
    {
        // You could add pre-validation, logging, metrics, caching, etc. here
        return _queryService.ExecuteQueryAsync(request, cancellationToken);
    }
}
