using GameAnalytics.EndPoints;
using GameAnalytics.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.AddOpenApi();

// Connection string
var connectionString =
    builder.Configuration.GetConnectionString("GameAnalytics")
    ?? throw new InvalidOperationException(
        "Connection string 'GameAnalytics' not found."
    );

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