namespace GameAnalytics.Features.Events;

using System.Text.Json;
using Npgsql;

public static class TrackEvent
{
    // The incoming event payload
    public readonly record struct Request(
        string PlayerId,
        string EventName,
        JsonElement Payload
    );

    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/", async (Request request, NpgsqlDataSource dataSource) =>
        {
            const string sql = """
                INSERT INTO events (player_id, event_name, payload, event_time)
                VALUES (@playerId, @eventName, @payload, @eventTime);
                """;

            await using var connection = await dataSource.OpenConnectionAsync();
            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("playerId",request.PlayerId);
            command.Parameters.AddWithValue("eventName",request.EventName);
            command.Parameters.AddWithValue("payload",NpgsqlTypes.NpgsqlDbType.Json, request.Payload.ToString());
            command.Parameters.AddWithValue("eventTime",NpgsqlTypes.NpgsqlDbType.Date,DateTime.Now);

            await command.ExecuteNonQueryAsync();

            return Results.Accepted();
        })
        .WithName("TrackEvent");
    }
}