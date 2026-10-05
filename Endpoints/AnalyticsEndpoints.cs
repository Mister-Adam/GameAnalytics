using GameAnalytics.Services;

namespace GameAnalytics.EndPoints;

public static class AnalyticsEndpoints
{
    public static void MapAnalyticsEndpoints(this WebApplication app)
    {
        app.MapGet("/analytics/run-duration", async (
            string playerId,
            string sessionId,
            string runId,
            AnalyticsService analyticsService) =>
        {
            var result = await analyticsService.GetRunDuration(
                playerId,
                sessionId,
                runId
            );

            return result;
        }).WithName("GetRunDuration");


        app.MapGet("/analytics/win-rate", async (
            AnalyticsService analyticsService) =>
        {
            var result = await analyticsService.GetWinRates();

            return result;
        }).WithName("GetWinRates");


        app.MapGet("/analytics/player-win-rate", async (
            string playerId,
            AnalyticsService analyticsService) =>
        {
            var result = await analyticsService.GetPlayerWinRate(playerId);

            return result;
        }).WithName("GetPlayerWinRate");


        app.MapGet("/analytics/characters-win-rate", async (
            AnalyticsService analyticsService) =>
        {
            var result = await analyticsService.GetWinRateByCharacters();

            return result;
        }).WithName("GetCharactersWinRate");
    }
}