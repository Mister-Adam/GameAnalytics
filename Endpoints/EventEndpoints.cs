using GameAnalytics.Models;
using GameAnalytics.Services;

namespace GameAnalytics.EndPoints;

public static class EventEndpoints
{
    public static void MapEventEndpoints(this WebApplication app)
    {
        app.MapGet("/events", async (EventService eventService) =>
        {
            var result = await eventService.GetEvents();

            return result;
        }).WithName("GetGameEvents");

        app.MapPost("/events", async (GameEvent gameEvent, EventService eventService) =>
        {
            await eventService.AddEvent(gameEvent);

            return Results.Ok();
        }).WithName("CreateGameEvent");
    }
}