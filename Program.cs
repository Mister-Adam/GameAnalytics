using GameAnalytics.Endpoints;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

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

// Create and register NpgsqlDataSource singleton
var dataSource = NpgsqlDataSource.Create(connectionString);
builder.Services.AddSingleton(dataSource);

var app = builder.Build();

app.Lifetime.ApplicationStopping.Register(() => dataSource.Dispose());

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Wire up the feature slices
app.MapEventEndpoints();
app.MapAnalyticsEndpoints();

app.Run();