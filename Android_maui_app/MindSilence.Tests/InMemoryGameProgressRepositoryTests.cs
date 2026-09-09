using MindSilence.Data;
using MindSilence.Domain.Models;

namespace MindSilence.Tests;

public sealed class InMemoryGameProgressRepositoryTests
{
	[Fact]
	public void RecordSession_merges_today_and_returns_best()
	{
		var repository = new InMemoryGameProgressRepository();

		var bestAfterFirst = repository.RecordSession(levelReached: 2, totalSeconds: 3);
		var bestAfterSecond = repository.RecordSession(levelReached: 1, totalSeconds: 5);

		Assert.Equal(2, bestAfterFirst);
		Assert.Equal(2, bestAfterSecond);

		var stats = repository.GetDailyStats();
		Assert.Single(stats);
		Assert.Equal(
			new DailyStats(DateOnly.FromDateTime(DateTime.Today), Attempts: 2, TotalSeconds: 8, BestLevel: 2),
			stats[0]);
	}

	[Fact]
	public void GetDailyStats_returns_newest_first()
	{
		var repository = new InMemoryGameProgressRepository();
		repository.Seed(new DailyStats(new DateOnly(2026, 9, 1), Attempts: 1, TotalSeconds: 10, BestLevel: 2));
		repository.Seed(new DailyStats(new DateOnly(2026, 9, 3), Attempts: 2, TotalSeconds: 5, BestLevel: 4));

		var stats = repository.GetDailyStats();

		Assert.Equal(2, stats.Count);
		Assert.Equal(new DateOnly(2026, 9, 3), stats[0].Date);
		Assert.Equal(new DateOnly(2026, 9, 1), stats[1].Date);
	}
}
