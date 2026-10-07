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

    private static readonly string Sql = SqlFiles.Load(typeof(GetRunDuration), "GetRunDuration.sql");

    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/run-duration", async ([AsParameters] Query query, NpgsqlDataSource dataSource) =>
        {
            await using var connection = await dataSource.OpenConnectionAsync();
            await using var command = new NpgsqlCommand(Sql, connection);

            // Pass parameter names matching the SQL @names
            command.Parameters.AddWithValue("playerId", query.PlayerId);
            command.Parameters.AddWithValue("sessionId", query.SessionId);
            command.Parameters.AddWithValue("runId", query.RunId);

            await using var reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync() || reader.IsDBNull(1))
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