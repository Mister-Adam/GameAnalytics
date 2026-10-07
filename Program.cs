using GameAnalytics.Endpoints;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var connectionString =
    builder.Configuration.GetConnectionString("GameAnalytics")
    ?? throw new InvalidOperationException(
        "Connection string 'GameAnalytics' is not configured."
    );

var dataSource = NpgsqlDataSource.Create(connectionString);
builder.Services.AddSingleton(dataSource);

var app = builder.Build();

app.Lifetime.ApplicationStopping.Register(() => dataSource.Dispose());

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapEventEndpoints();
app.MapAnalyticsEndpoints();

app.Run();