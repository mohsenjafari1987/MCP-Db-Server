using Mcp.Db.Facade;
using System.Threading.Tasks;

namespace Mcp.Db;

public static class Mcp
{
    public static async Task<McpDbSdk> Create(string uri)
            => await McpDbSdk.CreateAsync(uri);
}
