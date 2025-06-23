using Mcp.Db.Application.Factory;
using Mcp.Db.Contract.Interfaces;

namespace Mcp.Db;

public static class Mcp
{
    public static IMcpQueryService Create(string uri)
        => McpQueryFactory.Create(uri);
}
