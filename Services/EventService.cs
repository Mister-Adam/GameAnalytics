using System.Text.Json;
using GameAnalytics.Models;
using Npgsql;

namespace GameAnalytics.Services;

public class EventService(string connectionString)
{
    private readonly string _connectionString = connectionString;

    public async Task<List<StoredGameEvent>> GetEvents()
    {
        using var connection =
            new NpgsqlConnection(_connectionString);

        await connection.OpenAsync();

        // TODO: Temporary when DB not online, to remove when finished
        var command = new NpgsqlCommand(
            "SELECT * FROM events",
            connection
        );

        var reader = await command.ExecuteReaderAsync();

        var results = new List<StoredGameEvent>();

        while (await reader.ReadAsync())
        {
            var gameEvent = new StoredGameEvent(
                (int)reader["id"],
                (string)reader["player_id"],
                (string)reader["session_id"],
               (string)reader["rund_id"],
                (string)reader["event_type"],
                (DateTime)reader["event_time"]
            );

            results.Add(gameEvent);
        }

        return results;
    }

    public async Task AddEvent(GameEvent gameEvent)
    {
        ValidatePayload(gameEvent);

        using var connection =
            new NpgsqlConnection(_connectionString);

        await connection.OpenAsync();

        var command = new NpgsqlCommand(
            "INSERT INTO events " +
            "(player_id, session_id, run_id, event_type, event_time, payload) " +
            "VALUES (@playerId, @sessionId,@runId, @eventType, @eventTime, @payload)",
            connection
        );

        command.Parameters.AddWithValue(
            "playerId",
            gameEvent.PlayerId
        );

        command.Parameters.AddWithValue(
            "sessionId",
            gameEvent.SessionId
        );

         command.Parameters.AddWithValue(
            "runId",
            gameEvent.RunId
        );


        command.Parameters.AddWithValue(
            "eventType",
            gameEvent.EventType.ToString()
        );

        command.Parameters.AddWithValue(
            "eventTime",
            gameEvent.EventTime
        );

        command.Parameters.AddWithValue(
            "payload",
            NpgsqlTypes.NpgsqlDbType.Json,
            gameEvent.Payload.ToString()
        );

        await command.ExecuteNonQueryAsync();
    }

    private static void ValidatePayload(GameEvent gameEvent)
    {
        if (gameEvent.EventType is GameEventType.GameStarted or GameEventType.GameClosed)
            return;

        if (!gameEvent.Payload.TryGetProperty("characters", out var characters))
            return;

        if (characters.ValueKind != JsonValueKind.Array)
        {
            throw new ArgumentException(
                "'characters' must be an array."
            );
        }

        if (characters.GetArrayLength() == 0)
        {
            throw new ArgumentException(
                "'characters' must contain at least one character."
            );
        }
    }
}