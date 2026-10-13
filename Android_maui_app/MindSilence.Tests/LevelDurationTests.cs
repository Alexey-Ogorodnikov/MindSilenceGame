using MindSilence.Domain.UseCases;

namespace MindSilence.Tests;

public sealed class LevelDurationTests
{
	[Theory]
	[InlineData(0, 0)]
	[InlineData(-1, 0)]
	[InlineData(1, 5)]
	[InlineData(2, 10)]
	[InlineData(3, 15)]
	[InlineData(4, 30)]
	[InlineData(5, 60)]
	[InlineData(6, 120)]
	[InlineData(7, 240)]
	public void DurationForLevel_follows_5_10_then_doubles_from_15(int level, int expected) =>
		Assert.Equal(expected, LevelDuration.DurationForLevel(level));

	[Theory]
	[InlineData(0, 5, 0)]
	[InlineData(1, 0, 0)]
	[InlineData(2, 0, 5)]
	[InlineData(3, 0, 15)]
	[InlineData(3, 1, 16)]
	public void TotalSessionSeconds_sums_completed_levels_and_elapsed(int level, int elapsed, int expected) =>
		Assert.Equal(expected, LevelDuration.TotalSessionSeconds(level, elapsed));
}
