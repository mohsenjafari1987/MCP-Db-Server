using Mcp.Db.Contract.Interfaces;
using Mcp.Db.Infrastructure.Postgres;

namespace Mcp.Db.Application.Factory;

public static class McpQueryFactory
{
    public static IMcpQueryService Create(string uri)
    {
        if (uri.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            var connectionString = PostgresConnectionStringParser.Parse(uri);
            return new PostgresMcpQueryService(connectionString);
        }

        throw new NotSupportedException($"Unsupported MCP database engine in URI: {uri}");
    }
}
