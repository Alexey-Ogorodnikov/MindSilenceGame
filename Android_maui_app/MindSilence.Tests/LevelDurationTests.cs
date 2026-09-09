using MindSilence.Domain.UseCases;

namespace MindSilence.Tests;

public sealed class LevelDurationTests
{
	[Theory]
	[InlineData(0, 0)]
	[InlineData(-1, 0)]
	[InlineData(1, 4)]
	[InlineData(2, 8)]
	[InlineData(3, 16)]
	[InlineData(4, 32)]
	[InlineData(5, 64)]
	public void DurationForLevel_follows_geometric_progression(int level, int expected) =>
		Assert.Equal(expected, LevelDuration.DurationForLevel(level));

	[Theory]
	[InlineData(0, 5, 0)]
	[InlineData(1, 0, 0)]
	[InlineData(2, 0, 4)]
	[InlineData(3, 0, 12)]
	[InlineData(3, 1, 13)]
	public void TotalSessionSeconds_sums_completed_levels_and_elapsed(int level, int elapsed, int expected) =>
		Assert.Equal(expected, LevelDuration.TotalSessionSeconds(level, elapsed));
}
