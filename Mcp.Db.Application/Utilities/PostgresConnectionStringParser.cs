using System;

namespace Mcp.Db.Application.Factory;

public static class PostgresConnectionStringParser
{
    public static string Parse(string uri)
    {
        var uriObj = new Uri(uri);

        var userInfo = uriObj.UserInfo.Split(':');
        var username = Uri.UnescapeDataString(userInfo[0]);
        var password = Uri.UnescapeDataString(userInfo.Length > 1 ? userInfo[1] : "");
        var host = uriObj.Host;
        var port = uriObj.Port;
        var database = uriObj.AbsolutePath.TrimStart('/');

        return $"Host={host};Port={port};Username={username};Password={password};Database={database};";
    }
}