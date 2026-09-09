using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MindSilence.Domain.Models;
using MindSilence.Domain.UseCases;
using MindSilence.Resources;

namespace MindSilence.Presentation.HighScores;

/// <summary>
/// Daily highscore table. Loads newest-first stats in the constructor.
/// Back is a one-shot effect; this screen never talks to <c>AppViewModel</c>.
/// </summary>
public sealed partial class HighScoresViewModel : ObservableObject
{
	public HighScoresViewModel(GetDailyStatsUseCase getDailyStats)
	{
		DailyStats = getDailyStats.Invoke();
		Days = DailyStats.Select(HighScoreDayItem.From).ToList();
	}

	public IReadOnlyList<DailyStats> DailyStats { get; }

	public IReadOnlyList<HighScoreDayItem> Days { get; }

	public bool IsEmpty => DailyStats.Count == 0;

	public bool HasRecords => DailyStats.Count > 0;

	public event EventHandler? NavigateBack;

	[RelayCommand]
	public void OnBack() => NavigateBack?.Invoke(this, EventArgs.Empty);
}

public sealed record HighScoreDayItem(
	string DateText,
	string AttemptsText,
	string TotalTimeText,
	string BestLevelText)
{
	private static readonly CultureInfo English = CultureInfo.GetCultureInfo("en");

	public static HighScoreDayItem From(DailyStats stats)
	{
		var minutes = stats.TotalSeconds / 60;
		var seconds = stats.TotalSeconds % 60;
		return new HighScoreDayItem(
			stats.Date.ToString("d MMMM yyyy", English),
			FormatResx(AppResources.daily_attempts, stats.Attempts),
			FormatResx(AppResources.daily_total_time, minutes, seconds),
			FormatResx(AppResources.daily_best_level, stats.BestLevel));
	}

	private static string FormatResx(string androidPattern, params object[] args)
	{
		var format = androidPattern
			.Replace("%1$d", "{0}", StringComparison.Ordinal)
			.Replace("%2$d", "{1}", StringComparison.Ordinal);
		return string.Format(English, format, args);
	}
}
