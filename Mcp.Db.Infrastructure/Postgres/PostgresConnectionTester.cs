using Npgsql;

namespace Mcp.Db.Infrastructure.Postgres;

public static class PostgresConnectionTester
{
    public static async Task<bool> TryConnectAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
