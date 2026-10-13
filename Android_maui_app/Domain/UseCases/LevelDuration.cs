namespace MindSilence.Domain.UseCases;

public static class LevelDuration
{
	public static int DurationForLevel(int level) => level switch
	{
		<= 0 => 0,
		1 => 5,
		2 => 10,
		_ => 15 << (level - 3),
	};

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
