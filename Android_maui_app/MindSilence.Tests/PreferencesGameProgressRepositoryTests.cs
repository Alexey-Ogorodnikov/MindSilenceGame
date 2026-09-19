using MindSilence.Data;
using MindSilence.Domain.Models;

namespace MindSilence.Tests;

public sealed class PreferencesGameProgressRepositoryTests
{
	[Fact]
	public void RecordSession_round_trips_json_and_merges_today()
	{
		var store = new MemoryPreferences();
		var first = new PreferencesGameProgressRepository(store);

		var bestAfterFirst = first.RecordSession(levelReached: 2, totalSeconds: 3);
		var bestAfterSecond = first.RecordSession(levelReached: 1, totalSeconds: 5);

		Assert.Equal(2, bestAfterFirst);
		Assert.Equal(2, bestAfterSecond);

		var json = store.Get(PreferencesGameProgressRepository.KeyDailyStats, string.Empty, PreferencesGameProgressRepository.PrefsName);
		Assert.Contains("\"attempts\":2", json, StringComparison.Ordinal);
		Assert.Contains("\"totalSeconds\":8", json, StringComparison.Ordinal);
		Assert.Contains("\"bestLevel\":2", json, StringComparison.Ordinal);

		var reloaded = new PreferencesGameProgressRepository(store).GetDailyStats();
		Assert.Single(reloaded);
		Assert.Equal(
			new DailyStats(DateOnly.FromDateTime(DateTime.Today), Attempts: 2, TotalSeconds: 8, BestLevel: 2),
			reloaded[0]);
	}

	[Fact]
	public void GetDailyStats_returns_newest_first()
	{
		var store = new MemoryPreferences();
		store.Set(
			PreferencesGameProgressRepository.KeyDailyStats,
			"""
			[
			  {"date":"2026-09-01","attempts":1,"totalSeconds":10,"bestLevel":2},
			  {"date":"2026-09-03","attempts":2,"totalSeconds":5,"bestLevel":4}
			]
			""",
			PreferencesGameProgressRepository.PrefsName);

		var stats = new PreferencesGameProgressRepository(store).GetDailyStats();

		Assert.Equal(2, stats.Count);
		Assert.Equal(new DateOnly(2026, 9, 3), stats[0].Date);
		Assert.Equal(2, stats[0].Attempts);
		Assert.Equal(5, stats[0].TotalSeconds);
		Assert.Equal(4, stats[0].BestLevel);
		Assert.Equal(new DateOnly(2026, 9, 1), stats[1].Date);
	}

	[Fact]
	public void Corrupt_json_yields_empty_stats()
	{
		var store = new MemoryPreferences();
		store.Set(PreferencesGameProgressRepository.KeyDailyStats, "not-json", PreferencesGameProgressRepository.PrefsName);

		Assert.Empty(new PreferencesGameProgressRepository(store).GetDailyStats());
	}

	[Fact]
	public void Legacy_keys_migrate_to_one_day_with_unknown_duration()
	{
		var store = new MemoryPreferences();
		store.Set(PreferencesGameProgressRepository.KeyLegacyDate, "2026-01-15", PreferencesGameProgressRepository.PrefsName);
		store.Set(PreferencesGameProgressRepository.KeyLegacyBest, 4, PreferencesGameProgressRepository.PrefsName);

		var stats = new PreferencesGameProgressRepository(store).GetDailyStats();

		Assert.Equal(
			[
				new DailyStats(new DateOnly(2026, 1, 15), Attempts: 1, TotalSeconds: 0, BestLevel: 4),
			],
			stats);
		Assert.False(store.ContainsKey(PreferencesGameProgressRepository.KeyLegacyDate, PreferencesGameProgressRepository.PrefsName));
		Assert.False(store.ContainsKey(PreferencesGameProgressRepository.KeyLegacyBest, PreferencesGameProgressRepository.PrefsName));
		Assert.True(store.ContainsKey(PreferencesGameProgressRepository.KeyDailyStats, PreferencesGameProgressRepository.PrefsName));
	}

	[Fact]
	public void Legacy_migration_is_skipped_when_daily_stats_already_exist()
	{
		var store = new MemoryPreferences();
		store.Set(PreferencesGameProgressRepository.KeyDailyStats, "[]", PreferencesGameProgressRepository.PrefsName);
		store.Set(PreferencesGameProgressRepository.KeyLegacyDate, "2026-01-15", PreferencesGameProgressRepository.PrefsName);
		store.Set(PreferencesGameProgressRepository.KeyLegacyBest, 9, PreferencesGameProgressRepository.PrefsName);

		var stats = new PreferencesGameProgressRepository(store).GetDailyStats();

		Assert.Empty(stats);
		Assert.True(store.ContainsKey(PreferencesGameProgressRepository.KeyLegacyDate, PreferencesGameProgressRepository.PrefsName));
		Assert.Equal(9, store.Get(PreferencesGameProgressRepository.KeyLegacyBest, 0, PreferencesGameProgressRepository.PrefsName));
	}

	[Fact]
	public void Legacy_migration_ignores_missing_date()
	{
		var store = new MemoryPreferences();
		store.Set(PreferencesGameProgressRepository.KeyLegacyBest, 3, PreferencesGameProgressRepository.PrefsName);

		Assert.Empty(new PreferencesGameProgressRepository(store).GetDailyStats());
	}

	[Fact]
	public void Legacy_migration_ignores_non_positive_best()
	{
		var store = new MemoryPreferences();
		store.Set(PreferencesGameProgressRepository.KeyLegacyDate, "2026-01-15", PreferencesGameProgressRepository.PrefsName);
		store.Set(PreferencesGameProgressRepository.KeyLegacyBest, 0, PreferencesGameProgressRepository.PrefsName);

		Assert.Empty(new PreferencesGameProgressRepository(store).GetDailyStats());
	}

	[Fact]
	public void Legacy_migration_ignores_unparsable_date()
	{
		var store = new MemoryPreferences();
		store.Set(PreferencesGameProgressRepository.KeyLegacyDate, "not-a-date", PreferencesGameProgressRepository.PrefsName);
		store.Set(PreferencesGameProgressRepository.KeyLegacyBest, 3, PreferencesGameProgressRepository.PrefsName);

		Assert.Empty(new PreferencesGameProgressRepository(store).GetDailyStats());
	}
}
