using Mcp.Db.Contract.Interfaces;
using Mcp.Db.Infrastructure.Postgres;

namespace Mcp.Db.Application.Factory;

public static class McpQueryFactory
{
    public static IMcpQueryService Create(string connectionString)
    {
        return new PostgresMcpQueryService(connectionString);
    }
}