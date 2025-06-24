using Mcp.Db.Application.Factory;
using Mcp.Db.Application.Services;
using Mcp.Db.Contract.Interfaces;
using Mcp.Db.Infrastructure.Postgres;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);


builder.Configuration
                .AddEnvironmentVariables()
                .AddCommandLine(args);

var uri = builder.Configuration["DATABASE_URI"]
          ?? builder.Configuration["connection-string"]
          ?? throw new InvalidOperationException("Missing database connection string. Use --connection-string or set DATABASE_URI.");

var connectionString = uri.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase)
    ? PostgresConnectionStringParser.Parse(uri)
    : uri;

// ✅ Test DB connection before continuing
if (!await PostgresConnectionTester.TryConnectAsync(connectionString))
{
    Console.Error.WriteLine("❌ Unable to connect to PostgreSQL using provided connection string.");
    return;
}

builder.Services.AddSingleton<IMcpQueryService>(
   new PostgresMcpQueryService(connectionString));
builder.Services.AddScoped<McpQueryCoordinator>();
builder.Services.AddSingleton<ISchemaService>(new PostgresSchemaService(connectionString));
builder.Services.AddScoped<SchemaCoordinator>();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/query", async (
                McpQueryCoordinator coordinator,
                Mcp.Db.Contract.Models.McpQueryRequest request,
                CancellationToken cancellationToken) =>
{
    try
    {
        var result = await coordinator.ExecuteQueryAsync(request, cancellationToken);
        return Results.Ok(result);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (Exception ex)
    {
        // Log error if logger is registered
        return Results.Problem(title: "Internal Server Error", detail: ex.Message);
    }
})
    .WithName("ExecuteQuery")
    .WithOpenApi();


app.MapGet("/schema", async (SchemaCoordinator coordinator, CancellationToken ct) =>
{
    var result = await coordinator.GetAllSchemasAsync(ct);
    return Results.Ok(result);
});

app.MapGet("/schema/{tableName}", async (string tableName, SchemaCoordinator coordinator, CancellationToken ct) =>
{
    var result = await coordinator.GetSchemaForTableAsync(tableName, ct);
    return result is not null ? Results.Ok(result) : Results.NotFound();
});

app.Run();