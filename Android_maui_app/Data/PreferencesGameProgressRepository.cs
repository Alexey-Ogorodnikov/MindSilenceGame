using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Maui.Storage;
using MindSilence.Domain.Models;
using MindSilence.Domain.Repository;

namespace MindSilence.Data;

/// <summary>
/// Device JSON store for <see cref="IGameProgressRepository"/>.
/// Keys live in the <c>game_progress</c> Preferences container (Kotlin SharedPreferences file name).
/// </summary>
public sealed class PreferencesGameProgressRepository : IGameProgressRepository
{
	internal const string PrefsName = "game_progress";
	internal const string KeyDailyStats = "daily_stats";
	internal const string KeyLegacyDate = "best_date";
	internal const string KeyLegacyBest = "best_level";

	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		PropertyNamingPolicy = null,
		WriteIndented = false,
	};

	private readonly IPreferences _preferences;

	public PreferencesGameProgressRepository(IPreferences preferences)
	{
		_preferences = preferences;
		MigrateLegacyIfNeeded();
	}

	public int RecordSession(int levelReached, int totalSeconds)
	{
		var today = DateOnly.FromDateTime(DateTime.Today);
		var statsByDate = LoadStats();
		if (!statsByDate.TryGetValue(today, out var current))
			current = DailyStats.ForFirstAttempt(today, levelReached, totalSeconds);
		else
			current = current.AddAttempt(levelReached, totalSeconds);

		statsByDate[today] = current;
		SaveStats(statsByDate);
		return current.BestLevel;
	}

	public IReadOnlyList<DailyStats> GetDailyStats() =>
		LoadStats().Values.OrderByDescending(stat => stat.Date).ToList();

	private Dictionary<DateOnly, DailyStats> LoadStats()
	{
		if (!_preferences.ContainsKey(KeyDailyStats, PrefsName))
			return new Dictionary<DateOnly, DailyStats>();

		var json = _preferences.Get(KeyDailyStats, string.Empty, PrefsName);
		if (string.IsNullOrWhiteSpace(json))
			return new Dictionary<DateOnly, DailyStats>();

		try
		{
			var items = JsonSerializer.Deserialize<List<DailyStatsJson>>(json, JsonOptions);
			if (items is null)
				return new Dictionary<DateOnly, DailyStats>();

			var statsByDate = new Dictionary<DateOnly, DailyStats>();
			foreach (var item in items)
			{
				var date = DateOnly.ParseExact(item.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
				statsByDate[date] = new DailyStats(date, item.Attempts, item.TotalSeconds, item.BestLevel);
			}

			return statsByDate;
		}
		catch (JsonException)
		{
			return new Dictionary<DateOnly, DailyStats>();
		}
		catch (FormatException)
		{
			return new Dictionary<DateOnly, DailyStats>();
		}
		catch (ArgumentNullException)
		{
			return new Dictionary<DateOnly, DailyStats>();
		}
	}

	private void SaveStats(IReadOnlyDictionary<DateOnly, DailyStats> statsByDate)
	{
		var items = statsByDate.Values
			.OrderBy(stat => stat.Date)
			.Select(stat => new DailyStatsJson
			{
				Date = stat.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
				Attempts = stat.Attempts,
				TotalSeconds = stat.TotalSeconds,
				BestLevel = stat.BestLevel,
			})
			.ToList();

		var json = JsonSerializer.Serialize(items, JsonOptions);
		_preferences.Set(KeyDailyStats, json, PrefsName);
		_preferences.Remove(KeyLegacyDate, PrefsName);
		_preferences.Remove(KeyLegacyBest, PrefsName);
	}

	/// <summary>
	/// If <c>daily_stats</c> is not present yet, migrate legacy <c>best_date</c> + <c>best_level</c>
	/// into one day (<c>attempts = 1</c>, <c>totalSeconds = 0</c>). After a successful JSON write
	/// those old keys are removed. Matches architecture.md «Данные».
	/// </summary>
	private void MigrateLegacyIfNeeded()
	{
		if (_preferences.ContainsKey(KeyDailyStats, PrefsName))
			return;

		if (!_preferences.ContainsKey(KeyLegacyDate, PrefsName))
			return;

		var legacyDate = _preferences.Get(KeyLegacyDate, string.Empty, PrefsName);
		var legacyBest = _preferences.Get(KeyLegacyBest, 0, PrefsName);
		if (legacyBest <= 0)
			return;

		if (!DateOnly.TryParseExact(legacyDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
			return;

		SaveStats(new Dictionary<DateOnly, DailyStats>
		{
			[date] = DailyStats.ForFirstAttempt(date, levelReached: legacyBest, totalSeconds: 0),
		});
	}

	private sealed class DailyStatsJson
	{
		[JsonPropertyName("date")]
		public string Date { get; set; } = "";

		[JsonPropertyName("attempts")]
		public int Attempts { get; set; }

		[JsonPropertyName("totalSeconds")]
		public int TotalSeconds { get; set; }

		[JsonPropertyName("bestLevel")]
		public int BestLevel { get; set; }
	}
}
