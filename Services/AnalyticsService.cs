using GameAnalytics.Models;
using Npgsql;
using System.Reflection;

namespace GameAnalytics.Services;

public class AnalyticsService(string connectionString)
{
    private readonly string _connectionString = connectionString;

    private static string LoadSql(string fileName)
    {
        var path = Path.Combine(
            AppContext.BaseDirectory,
            "Sql",
            fileName
        );

        return File.ReadAllText(path);
    }

    public async Task<RunDuration> GetRunDuration(
        string playerId,
        string sessionId,
        string runId)
    {
        using var connection =
            new NpgsqlConnection(_connectionString);

        await connection.OpenAsync();

        var sql = LoadSql("GetRunDuration.sql");

        var command = new NpgsqlCommand(
            sql,
            connection
        );

        command.Parameters.AddWithValue(
            "playerId",
            playerId
        );

        command.Parameters.AddWithValue(
            "sessionId",
            sessionId
        );

        command.Parameters.AddWithValue(
            "runId",
            runId
        );

        var reader = await command.ExecuteReaderAsync();

        await reader.ReadAsync();

        if (reader["start_time"] is DBNull ||
            reader["end_time"] is DBNull ||
            reader["run_duration"] is DBNull)
        {
            throw new InvalidOperationException(
                "No completed run found for this player, session and run."
            );
        }

        var startTime = (DateTime)reader["start_time"];
        var endTime = (DateTime)reader["end_time"];
        var duration = (TimeSpan)reader["run_duration"];

        await reader.CloseAsync();

        // Player's average duration across all completed runs.

        var averageSql = LoadSql(
            "GetAverageRunDuration.sql"
        );

        var averageCommand = new NpgsqlCommand(
            averageSql,
            connection
        );

        averageCommand.Parameters.AddWithValue(
            "playerId",
            playerId
        );

        var averageReader =
            await averageCommand.ExecuteReaderAsync();

        await averageReader.ReadAsync();

        TimeSpan? averageDuration = null;

        if (averageReader["average_duration"] is not DBNull)
        {
            averageDuration =
                (TimeSpan)averageReader["average_duration"];
        }

        return new RunDuration(
            runId,
            startTime,
            endTime,
            duration,
            averageDuration
        );
    }

    public async Task<WinRate> GetWinRates()
    {
        using var connection =
            new NpgsqlConnection(_connectionString);

        await connection.OpenAsync();

        var sql = LoadSql("GetWinRate.sql");

        var command = new NpgsqlCommand(
            sql,
            connection
        );

        AddWinRateCommands(command);

        var reader = await command.ExecuteReaderAsync();

        await reader.ReadAsync();

        return CreateWinRate(
            "Global Winrate",
            reader
        );
    }

    public async Task<WinRate> GetPlayerWinRate(
        string playerId)
    {
        using var connection =
            new NpgsqlConnection(_connectionString);

        await connection.OpenAsync();

        var sql = LoadSql(
            "GetPlayerWinRate.sql"
        );

        var command = new NpgsqlCommand(
            sql,
            connection
        );

        command.Parameters.AddWithValue(
            "playerId",
            playerId
        );

        AddWinRateCommands(command);

        var reader = await command.ExecuteReaderAsync();

        await reader.ReadAsync();

        return CreateWinRate(
            playerId,
            reader
        );
    }

    public async Task<List<WinRate>> GetWinRateByCharacters()
    {
        using var connection =
            new NpgsqlConnection(_connectionString);

        await connection.OpenAsync();

        var sql = LoadSql(
            "GetCharacterWinRates.sql"
        );

        var command = new NpgsqlCommand(
            sql,
            connection
        );

        AddWinRateCommands(command);

        var reader = await command.ExecuteReaderAsync();

        var results = new List<WinRate>();

        while (await reader.ReadAsync())
        {
            var character =
                (string)reader["character"];

            results.Add(
                CreateWinRate(
                    character,
                    reader
                )
            );
        }

        return results;
    }

    private static void AddWinRateCommands(
        NpgsqlCommand command)
    {
        command.Parameters.AddWithValue(
            "victory",
            GameEventType.Victory.ToString()
        );

        command.Parameters.AddWithValue(
            "defeat",
            GameEventType.Defeat.ToString()
        );

        command.Parameters.AddWithValue(
            "surrender",
            GameEventType.Surrender.ToString()
        );
    }

    private static WinRate CreateWinRate(
        string name,
        NpgsqlDataReader reader)
    {
        var victories =
            (long)reader["victories"];

        var defeats =
            (long)reader["defeats"];

        var surrenders =
            (long)reader["surrenders"];

        return new WinRate(
            name,
            victories,
            defeats,
            victories + defeats + surrenders,
            CalculateWinRate(
                victories,
                defeats
            )
        );
    }

    private static double CalculateWinRate(
        long victories,
        long defeats)
    {
        if (victories + defeats == 0)
        {
            return 0;
        }

        return (double)victories /
               (victories + defeats);
    }
}