namespace MindSilence.Domain.UseCases;

public static class LevelDuration
{
	public static int DurationForLevel(int level) =>
		level >= 1 ? 4 << (level - 1) : 0;

	public static int TotalSessionSeconds(int level, int elapsedSecAtLevel)
	{
		if (level <= 0)
			return 0;

		var total = elapsedSecAtLevel;
		for (var completedLevel = 1; completedLevel < level; completedLevel++)
			total += DurationForLevel(completedLevel);

		return total;
	}
}
