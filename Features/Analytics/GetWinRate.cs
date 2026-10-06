namespace GameAnalytics.Features.Analytics;

using Npgsql;

public static class GetWinRates
{
    // Query params: /analytics/win-rate?playerId=x OR /analytics/win-rate?characterId=y
    public readonly record struct Query(
        string? PlayerId = null,
        string? CharacterId = null
    );

    // Polymorphic result record
    public readonly record struct WinRateResult(
        string Subject,
        long Victories,
        long Defeats,
        long GamesPlayed,
        double Ratio
    );

    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/win-rates", async ([AsParameters] Query query, NpgsqlDataSource dataSource) =>
        {
            // Dynamic grouping based on which filter was passed
            string groupByColumn = query switch
            {
                { CharacterId: not null } => "character_id",
                { PlayerId: not null } => "player_id",
                _ => "'Global'"
            };

            var sql = $"""
                SELECT 
                    {groupByColumn}::text AS subject,
                    COUNT(*) FILTER (WHERE won) AS victories,
                    COUNT(*) FILTER (WHERE NOT won) AS defeats,
                    COUNT(*) AS games_played,
                    ROUND(COUNT(*) FILTER (WHERE won)::numeric / NULLIF(COUNT(*), 0), 4)::float8 AS ratio
                FROM matches
                WHERE ($1::text IS NULL OR player_id = $1)
                  AND ($2::text IS NULL OR character_id = $2)
                GROUP BY {groupByColumn};
                """;

            await using var connection = await dataSource.OpenConnectionAsync();
            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue(query.PlayerId is null ? DBNull.Value : query.PlayerId);
            command.Parameters.AddWithValue(query.CharacterId is null ? DBNull.Value : query.CharacterId);

            var list = new List<WinRateResult>();
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new WinRateResult(
                    Subject: reader.GetString(0),
                    Victories: reader.GetInt64(1),
                    Defeats: reader.GetInt64(2),
                    GamesPlayed: reader.GetInt64(3),
                    Ratio: reader.IsDBNull(4) ? 0.0 : reader.GetDouble(4)
                ));
            }

            return Results.Ok(list);
        })
        .WithName("GetWinRates");
    }
}