namespace GameAnalytics.Features.Analytics;

using Npgsql;

public static class GetRunDuration
{
    // Bound from query string: /analytics/run-duration?playerId=...&sessionId=...&runId=...
    public readonly record struct Query(
        string PlayerId,
        string SessionId,
        string RunId
    );

    // Response record - strictly local to this feature
    public readonly record struct Result(
        string RunId,
        DateTime StartTimeUtc,
        DateTime EndTimeUtc,
        TimeSpan Duration,
        TimeSpan? AverageDuration
    );

    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/run-duration", async ([AsParameters] Query query, NpgsqlDataSource dataSource) =>
        {
            const string sql = """
                SELECT 
                    run_id, 
                    start_time, 
                    end_time, 
                    (end_time - start_time) AS duration
                FROM runs 
                WHERE player_id = $1 
                  AND session_id = $2 
                  AND run_id = $3
                LIMIT 1;
                """;

            await using var connection = await dataSource.OpenConnectionAsync();
            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue(query.PlayerId);
            command.Parameters.AddWithValue(query.SessionId);
            command.Parameters.AddWithValue(query.RunId);

            await using var reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                return Results.NotFound();
            }

            var start = reader.GetDateTime(1);
            var end = reader.GetDateTime(2);

            var result = new Result(
                RunId: reader.GetString(0),
                StartTimeUtc: start,
                EndTimeUtc: end,
                Duration: end - start,
                AverageDuration: null
            );

            return Results.Ok(result);
        })
        .WithName("GetRunDuration");
    }
}