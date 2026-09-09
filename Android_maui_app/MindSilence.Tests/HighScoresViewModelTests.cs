using MindSilence.Data;
using MindSilence.Domain.Models;
using MindSilence.Domain.UseCases;
using MindSilence.Presentation.HighScores;

namespace MindSilence.Tests;

public sealed class HighScoresViewModelTests
{
	[Fact]
	public void Constructor_loads_daily_stats_newest_first()
	{
		var repository = new InMemoryGameProgressRepository();
		repository.RecordSession(levelReached: 3, totalSeconds: 10);
		repository.RecordSession(levelReached: 2, totalSeconds: 4);

		var viewModel = new HighScoresViewModel(new GetDailyStatsUseCase(repository));

		Assert.Single(viewModel.DailyStats);
		Assert.Equal(
			new DailyStats(DateOnly.FromDateTime(DateTime.Today), Attempts: 2, TotalSeconds: 14, BestLevel: 3),
			viewModel.DailyStats[0]);
		Assert.False(viewModel.IsEmpty);
		Assert.True(viewModel.HasRecords);
	}

	[Fact]
	public void Empty_repository_shows_empty_state()
	{
		var viewModel = new HighScoresViewModel(new GetDailyStatsUseCase(new InMemoryGameProgressRepository()));

		Assert.Empty(viewModel.DailyStats);
		Assert.True(viewModel.IsEmpty);
		Assert.False(viewModel.HasRecords);
	}

	[Fact]
	public void Day_row_formats_english_date_and_totals()
	{
		var repository = new InMemoryGameProgressRepository();
		repository.Seed(new DailyStats(new DateOnly(2026, 9, 5), Attempts: 2, TotalSeconds: 125, BestLevel: 3));

		var viewModel = new HighScoresViewModel(new GetDailyStatsUseCase(repository));

		var row = Assert.Single(viewModel.Days);
		Assert.Equal("5 September 2026", row.DateText);
		Assert.Equal("Attempts: 2", row.AttemptsText);
		Assert.Equal("Total: 2 min 5 s", row.TotalTimeText);
		Assert.Equal("Best level: 3", row.BestLevelText);
	}

	[Fact]
	public void OnBack_emits_navigate_back_effect()
	{
		var viewModel = new HighScoresViewModel(new GetDailyStatsUseCase(new InMemoryGameProgressRepository()));
		var raised = 0;
		viewModel.NavigateBack += (_, _) => raised++;

		viewModel.OnBack();

		Assert.Equal(1, raised);
	}
}
