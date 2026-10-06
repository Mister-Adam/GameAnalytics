namespace GameAnalytics.Features.Events;

using Npgsql;

public static class GetEvents
{
    // Query parameters: /events?playerId=xyz&limit=50&offset=0
    public readonly record struct Query(
        string? PlayerId = null,
        int Limit = 50,
        int Offset = 0
    );

    // Event model local to this feature slice
    public readonly record struct EventRecord(
        long Id,
        string PlayerId,
        string EventName,
        string? PayloadJson,
        DateTime CreatedAt
    );

    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", async ([AsParameters] Query query, NpgsqlDataSource dataSource) =>
        {
            // Clamp limit to prevent massive accidental dumps
            var limit = Math.Clamp(query.Limit, 1, 100);
            var offset = Math.Max(0, query.Offset);

            var sql = """
                SELECT 
                    id, 
                    player_id, 
                    event_name, 
                    payload::text, 
                    created_at
                FROM events
                WHERE ($1::text IS NULL OR player_id = $1)
                ORDER BY created_at DESC
                LIMIT $2 OFFSET $3;
                """;

            await using var connection = await dataSource.OpenConnectionAsync();
            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue(query.PlayerId is null ? DBNull.Value : query.PlayerId);
            command.Parameters.AddWithValue(limit);
            command.Parameters.AddWithValue(offset);

            var list = new List<EventRecord>();
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new EventRecord(
                    Id: reader.GetInt64(0),
                    PlayerId: reader.GetString(1),
                    EventName: reader.GetString(2),
                    PayloadJson: reader.IsDBNull(3) ? null : reader.GetString(3),
                    CreatedAt: reader.GetDateTime(4)
                ));
            }

            return Results.Ok(list);
        })
        .WithName("GetEvents");
    }
}