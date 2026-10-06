namespace GameAnalytics.Features.Events;

using Npgsql;
using NpgsqlTypes;

public static class GetEvents
{
    // /events?playerId=xyz&limit=50&offset=0
    public readonly record struct Query(
        string? PlayerId = null,
        int Limit = 50,
        int Offset = 0
    );

    public readonly record struct EventRecord(
        long Id,
        string PlayerId,
        string EventType,
        string? PayloadJson,
        DateTime EventTime
    );

    private static readonly string Sql = SqlFiles.Load(typeof(GetEvents));

    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", async ([AsParameters] Query query, NpgsqlDataSource dataSource) =>
        {
            var limit = Math.Clamp(query.Limit, 1, 100);
            var offset = Math.Max(0, query.Offset);

            await using var command = dataSource.CreateCommand(Sql);
            command.Parameters.Add(new NpgsqlParameter("playerId", NpgsqlDbType.Text)
            {
                Value = (object?)query.PlayerId ?? DBNull.Value
            });
            command.Parameters.AddWithValue("limit", limit);
            command.Parameters.AddWithValue("offset", offset);

            var list = new List<EventRecord>(limit);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new EventRecord(
                    Id: reader.GetInt64(0),
                    PlayerId: reader.GetString(1),
                    EventType: reader.GetString(2),
                    PayloadJson: reader.IsDBNull(3) ? null : reader.GetString(3),
                    EventTime: reader.GetDateTime(4)
                ));
            }

            return Results.Ok(list);
        })
        .WithName("GetEvents");
    }
}