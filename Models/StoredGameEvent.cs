namespace GameAnalytics.Models;

public record StoredGameEvent(
    int Id,
    string PlayerId,
    string SessionId,
    string RunId,
    string EventType,
    DateTime EventTime
);