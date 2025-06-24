using Mcp.Db.Contract.Interfaces;
using Mcp.Db.Infrastructure.Postgres;

namespace Mcp.Db.Application.Factory;

public static class McpSchemaFactory
{
    public static IMcpSchemaService Create(string connectionString)
    {
        return new PostgresMcpSchemaService(connectionString);
    }
}
