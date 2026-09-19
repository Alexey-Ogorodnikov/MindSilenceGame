namespace MindSilence.Domain.Models;

public sealed record DailyStats(DateOnly Date, int Attempts, int TotalSeconds, int BestLevel)
{
	public static DailyStats ForFirstAttempt(DateOnly date, int levelReached, int totalSeconds) =>
		new(date, Attempts: 1, totalSeconds, levelReached);

	public DailyStats AddAttempt(int levelReached, int totalSeconds) => this with
	{
		Attempts = Attempts + 1,
		TotalSeconds = TotalSeconds + totalSeconds,
		BestLevel = Math.Max(BestLevel, levelReached),
	};
}
