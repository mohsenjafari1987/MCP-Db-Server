using Mcp.Db.Application.Factory;
using Mcp.Db.Contract.Interfaces;
using Mcp.Db.Infrastructure.Postgres;
using System;
using System.Threading.Tasks;

namespace Mcp.Db.Facade;

public class McpDbSdk
{
    public IMcpQueryService Query { get; }
    public IMcpSchemaService Schema { get; }

    private McpDbSdk(string uri)
    {        
        Query = McpQueryFactory.Create(uri);
        Schema = McpSchemaFactory.Create(uri);
    }

    public static async Task<McpDbSdk> CreateAsync(string uri)
    {
        if (string.IsNullOrWhiteSpace(uri))
            throw new ArgumentException("Connection string is required.", nameof(uri));

        if (!uri.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
            throw new NotSupportedException($"Unsupported MCP database engine in URI: {uri}");


        var connectionString = PostgresConnectionStringParser.Parse(uri);
        
        var isConnected = await PostgresConnectionTester.TryConnectAsync(connectionString);        
        if (!isConnected)
            throw new InvalidOperationException("Failed to connect...");

        return new McpDbSdk(connectionString);
    }
}