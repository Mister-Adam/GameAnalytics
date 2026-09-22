var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var events = new List<GameEvent>();

app.MapPost("/events", (GameEvent gameEvent) =>
{
    events.Add(gameEvent);
    return gameEvent;
});

app.MapGet("/events", () => events.ToArray())
    .WithName("GetGameEvents");


app.Run();



record GameEvent(string EventType, int Level);


