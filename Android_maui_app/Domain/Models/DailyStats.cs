namespace MindSilence.Domain.Models;

public sealed record DailyStats(DateOnly Date, int Attempts, int TotalSeconds, int BestLevel);
