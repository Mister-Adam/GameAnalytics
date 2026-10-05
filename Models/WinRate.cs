namespace GameAnalytics.Models;

public record WinRate(
    string Subject,
    long Victories,
    long Defeats,
    long GamesPlayed,
    double Ratio
);