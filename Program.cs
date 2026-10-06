using GameAnalytics.EndPoints;
using GameAnalytics.Services;
using Npgsql;
var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Configure services
builder.Services.AddOpenApi();

// Connection string
string Require(string key) =>
    builder.Configuration[key]
    ?? throw new InvalidOperationException($"Environment variable '{key}' is not set.");

var connectionString = new NpgsqlConnectionStringBuilder
{
    Host = Require("PGHOST"),
    Port = int.Parse(Require("PGPORT")),
    Username = Require("PGUSER"),
    Password = Require("PGPASSWORD"),
    Database = Require("PGDATABASE")
}.ConnectionString;

// Register application services
builder.Services.AddScoped(
    serviceProvider => new AnalyticsService(connectionString)
);

builder.Services.AddScoped(
    serviceProvider => new EventService(connectionString)
);

// Build application
var app = builder.Build();

// Configure application
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Register endpoints
app.MapEventEndpoints();
app.MapAnalyticsEndpoints();

// Start application
app.Run();