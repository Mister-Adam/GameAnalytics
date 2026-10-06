using GameAnalytics.EndPoints;
using GameAnalytics.Services;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Configure services
builder.Services.AddOpenApi();

// Connection string
var connectionString =
    $"Host={builder.Configuration["PGHOST"]};" +
    $"Port={builder.Configuration["PGPORT"]};" +
    $"Username={builder.Configuration["PGUSER"]};" +
    $"Password={builder.Configuration["PGPASSWORD"]};" +
    $"Database={builder.Configuration["PGDATABASE"]}";

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