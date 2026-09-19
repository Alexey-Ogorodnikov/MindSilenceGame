using MindSilence.Domain.Models;
using MindSilence.Domain.Repository;

namespace MindSilence.Data;

/// <summary>
/// In-memory <see cref="IGameProgressRepository"/> for tests. Same merge rules as the prod store.
/// Do not register this type in <c>MauiProgram</c> as the production implementation.
/// </summary>
public sealed class InMemoryGameProgressRepository : IGameProgressRepository
{
	private readonly Dictionary<DateOnly, DailyStats> _statsByDate = [];

	public int RecordSession(int levelReached, int totalSeconds)
	{
		var today = DateOnly.FromDateTime(DateTime.Today);
		if (!_statsByDate.TryGetValue(today, out var current))
			current = DailyStats.ForFirstAttempt(today, levelReached, totalSeconds);
		else
			current = current.AddAttempt(levelReached, totalSeconds);

		_statsByDate[today] = current;
		return current.BestLevel;
	}

	public IReadOnlyList<DailyStats> GetDailyStats() =>
		_statsByDate.Values.OrderByDescending(stat => stat.Date).ToList();

	public void Seed(DailyStats stats) => _statsByDate[stats.Date] = stats;
}
