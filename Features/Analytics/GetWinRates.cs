namespace GameAnalytics.Features.Analytics;

using GameAnalytics.Models;
using Npgsql;

public static class GetWinRates
{
    // /analytics/win-rates                    -> global
    // /analytics/win-rates?playerId=x         -> one player
    // /analytics/win-rates?byCharacter=true   -> one row per character
    public readonly record struct Query(
        string? PlayerId = null,
        bool ByCharacter = false
    );

    public readonly record struct WinRateResult(
        string Subject,
        long Victories,
        long Defeats,
        long GamesPlayed,
        double Ratio
    );
 
    private static readonly string GlobalSql = SqlFiles.Load(typeof(GetWinRates), "GetWinRate.sql");
    private static readonly string PlayerSql = SqlFiles.Load(typeof(GetWinRates), "GetPlayerWinRate.sql");
    private static readonly string CharacterSql = SqlFiles.Load(typeof(GetWinRates), "GetCharacterWinRates.sql");

    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/win-rates", async ([AsParameters] Query query, NpgsqlDataSource dataSource) =>
        {
            var (sql, defaultSubject) = query switch
            {
                { ByCharacter: true } => (CharacterSql, null),
                { PlayerId: not null } => (PlayerSql, query.PlayerId),
                _ => (GlobalSql, "Global Winrate")
            };

            await using var command = dataSource.CreateCommand(sql);
            command.Parameters.AddWithValue("victory", nameof(GameEventType.Victory));
            command.Parameters.AddWithValue("defeat", nameof(GameEventType.Defeat));
            command.Parameters.AddWithValue("surrender", nameof(GameEventType.Surrender));

            if (sql == PlayerSql)
                command.Parameters.AddWithValue("playerId", query.PlayerId!);

            var list = new List<WinRateResult>();
            await using var reader = await command.ExecuteReaderAsync();

            var victoriesCol = reader.GetOrdinal("victories");
            var defeatsCol = reader.GetOrdinal("defeats");
            var surrendersCol = reader.GetOrdinal("surrenders");

            while (await reader.ReadAsync())
            {
                var victories = reader.GetInt64(victoriesCol);
                var defeats = reader.GetInt64(defeatsCol);
                var surrenders = reader.GetInt64(surrendersCol);

                list.Add(new WinRateResult(
                    Subject: defaultSubject ?? reader.GetString(reader.GetOrdinal("character")),
                    Victories: victories,
                    Defeats: defeats,
                    GamesPlayed: victories + defeats + surrenders,
                    Ratio: victories + defeats == 0 ? 0 : (double)victories / (victories + defeats)
                ));
            }

            return Results.Ok(list);
        })
        .WithName("GetWinRates");
    }
}