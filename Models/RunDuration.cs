namespace GameAnalytics.Models;

public record RunDuration(
    string RunId,
    DateTime StartTime,
    DateTime EndTime,
    TimeSpan Duration,
    TimeSpan? AverageDuration
);