namespace Mcp.Db.Contract.Models;

public class McpQueryRequest
{
    /// <summary>
    /// Raw SQL SELECT query to execute.
    /// </summary>
    public required string Query { get; init; }

    /// <summary>
    /// (Optional) user id or source agent, can be used for logging/routing
    /// </summary>
    public string? User { get; init; }
}
