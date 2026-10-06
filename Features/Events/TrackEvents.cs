namespace GameAnalytics.Features.Events;

using Npgsql;

public static class TrackEvent
{
    // The incoming event payload
    public readonly record struct Request(
        string PlayerId,
        string EventName,
        string? PayloadJson
    );

    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/", async (Request request, NpgsqlDataSource dataSource) =>
        {
            const string sql = """
                INSERT INTO events (player_id, event_name, payload, event_time)
                VALUES ($1, $2, $3::jsonb, NOW());
                """;

            await using var connection = await dataSource.OpenConnectionAsync();
            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue(request.PlayerId);
            command.Parameters.AddWithValue(request.EventName);
            command.Parameters.AddWithValue(request.PayloadJson ?? "{}");

            await command.ExecuteNonQueryAsync();

            return Results.Accepted();
        })
        .WithName("TrackEvent");
    }
}