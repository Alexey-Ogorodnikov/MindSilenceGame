using MindSilence.Data;
using MindSilence.Domain.Models;
using MindSilence.Domain.UseCases;
using MindSilence.Presentation.Game;

namespace MindSilence.Tests;

public sealed class GameViewModelTests : IDisposable
{
	private readonly List<GameViewModel> _created = [];

	[Fact]
	public void Start_transitions_to_running_level_one()
	{
		var viewModel = CreateGame();
		var keepScreen = new List<bool>();
		viewModel.KeepScreenOn += (_, enabled) => keepScreen.Add(enabled);

		viewModel.OnStart();

		Assert.Equal(GamePhase.Running, viewModel.Phase);
		Assert.Equal(1, viewModel.Level);
		Assert.Equal(0, viewModel.ElapsedSecAtLevel);
		Assert.Equal(4, viewModel.RequiredSecAtLevel);
		Assert.Equal([true], keepScreen);
	}

	[Fact]
	public void Four_ticks_of_silence_advance_to_level_two()
	{
		var viewModel = CreateGame();
		viewModel.OnStart();

		viewModel.StepTick(4);

		Assert.Equal(2, viewModel.Level);
		Assert.Equal(0, viewModel.ElapsedSecAtLevel);
	}

	[Fact]
	public void Completing_level_two_advances_to_level_three()
	{
		var viewModel = CreateGame();
		viewModel.OnStart();

		viewModel.StepTick(4);
		viewModel.StepTick(8);

		Assert.Equal(3, viewModel.Level);
	}

	[Fact]
	public void Thought_resets_to_idle_writes_use_case_and_shows_summary()
	{
		var repository = new InMemoryGameProgressRepository();
		var viewModel = CreateGame(repository);
		var haptic = 0;
		viewModel.HapticOnThought += (_, _) => haptic++;
		viewModel.OnStart();
		viewModel.StepTick(1);

		viewModel.OnThought();

		Assert.Equal(GamePhase.Idle, viewModel.Phase);
		Assert.Equal(0, viewModel.Level);
		Assert.Equal(0, viewModel.ElapsedSecAtLevel);
		Assert.Equal(new SessionSummary(LevelReached: 1, BestToday: 1, TotalSeconds: 1), viewModel.SessionSummary);
		Assert.Equal(1, haptic);
		Assert.Single(repository.GetDailyStats());
		Assert.Equal(1, repository.GetDailyStats()[0].Attempts);
		Assert.Equal(1, repository.GetDailyStats()[0].TotalSeconds);
	}

	[Fact]
	public void Thought_keeps_best_today_across_sessions()
	{
		var repository = new InMemoryGameProgressRepository();
		var viewModel = CreateGame(repository);
		viewModel.OnStart();
		viewModel.StepTick(4);
		viewModel.OnThought();
		viewModel.OnDismissSessionSummary();

		viewModel.OnStart();
		viewModel.OnThought();

		Assert.Equal(new SessionSummary(LevelReached: 1, BestToday: 2, TotalSeconds: 0), viewModel.SessionSummary);
	}

	[Fact]
	public void Thought_includes_total_session_time_in_summary()
	{
		var viewModel = CreateGame();
		viewModel.OnStart();
		viewModel.StepTick(4);
		viewModel.StepTick(8);

		viewModel.OnThought();

		Assert.Equal(new SessionSummary(LevelReached: 3, BestToday: 3, TotalSeconds: 12), viewModel.SessionSummary);
	}

	[Fact]
	public void Thought_in_idle_is_ignored()
	{
		var repository = new InMemoryGameProgressRepository();
		var viewModel = CreateGame(repository);

		viewModel.OnThought();

		Assert.Equal(GamePhase.Idle, viewModel.Phase);
		Assert.Equal(0, viewModel.Level);
		Assert.Null(viewModel.SessionSummary);
		Assert.Empty(repository.GetDailyStats());
	}

	[Fact]
	public void Start_while_running_is_ignored()
	{
		var viewModel = CreateGame();
		viewModel.OnStart();
		viewModel.StepTick(2);

		viewModel.OnStart();

		Assert.Equal(GamePhase.Running, viewModel.Phase);
		Assert.Equal(1, viewModel.Level);
		Assert.Equal(2, viewModel.ElapsedSecAtLevel);
	}

	[Fact]
	public void Start_while_session_summary_is_ignored()
	{
		var viewModel = CreateGame();
		viewModel.OnStart();
		viewModel.StepTick(1);
		viewModel.OnThought();
		var summary = viewModel.SessionSummary;

		viewModel.OnStart();

		Assert.Equal(GamePhase.Idle, viewModel.Phase);
		Assert.Equal(0, viewModel.Level);
		Assert.Equal(summary, viewModel.SessionSummary);
		Assert.False(viewModel.StartCommand.CanExecute(null));
		Assert.False(viewModel.ThoughtCommand.CanExecute(null));
	}

	[Fact]
	public void Dismiss_session_summary_clears_dialog_and_enables_start()
	{
		var viewModel = CreateGame();
		viewModel.OnStart();
		viewModel.OnThought();

		viewModel.OnDismissSessionSummary();

		Assert.Null(viewModel.SessionSummary);
		Assert.True(viewModel.StartCommand.CanExecute(null));
	}

	[Fact]
	public void Stale_tick_after_restart_is_ignored()
	{
		var viewModel = CreateGame();
		viewModel.OnStart();
		viewModel.StepTick(1);
		viewModel.OnAppBackgrounded();
		viewModel.OnAppForegrounded();

		viewModel.StepStaleTick();

		Assert.Equal(1, viewModel.ElapsedSecAtLevel);
		viewModel.StepTick(1);
		Assert.Equal(2, viewModel.ElapsedSecAtLevel);
	}

	[Fact]
	public void Background_while_running_stops_tick_and_keeps_phase()
	{
		var viewModel = CreateGame();
		var keepScreen = new List<bool>();
		viewModel.KeepScreenOn += (_, enabled) => keepScreen.Add(enabled);
		viewModel.OnStart();
		viewModel.StepTick(1);

		viewModel.OnAppBackgrounded();
		var elapsed = viewModel.ElapsedSecAtLevel;
		viewModel.StepTick(4);

		Assert.Equal(GamePhase.Running, viewModel.Phase);
		Assert.Equal(elapsed, viewModel.ElapsedSecAtLevel);
		Assert.Equal([true, false], keepScreen);
	}

	[Fact]
	public void Foreground_resumes_tick_only_after_running_background()
	{
		var viewModel = CreateGame();
		viewModel.OnStart();
		viewModel.OnAppBackgrounded();

		viewModel.OnAppForegrounded();
		viewModel.StepTick(4);

		Assert.Equal(2, viewModel.Level);
	}

	[Fact]
	public void Background_while_idle_is_noop()
	{
		var viewModel = CreateGame();
		var keepScreen = 0;
		viewModel.KeepScreenOn += (_, _) => keepScreen++;

		viewModel.OnAppBackgrounded();

		Assert.Equal(GamePhase.Idle, viewModel.Phase);
		Assert.Equal(0, keepScreen);
	}

	[Fact]
	public void OpenHighScores_clears_summary_and_navigates()
	{
		var viewModel = CreateGame();
		var navigated = 0;
		viewModel.NavigateToHighScores += (_, _) => navigated++;
		viewModel.OnStart();
		viewModel.OnThought();

		viewModel.OnOpenHighScores();

		Assert.Null(viewModel.SessionSummary);
		Assert.Equal(1, navigated);
	}

	[Fact]
	public void LeaveTraining_emits_navigate_back_to_menu()
	{
		var viewModel = CreateGame();
		var navigated = 0;
		viewModel.NavigateBackToMenu += (_, _) => navigated++;

		viewModel.OnLeaveTraining();

		Assert.Equal(1, navigated);
	}

	[Fact]
	public void Dispose_stops_ticking()
	{
		var viewModel = CreateGame();
		viewModel.OnStart();
		viewModel.StepTick(1);

		viewModel.Dispose();
		var elapsed = viewModel.ElapsedSecAtLevel;
		viewModel.StepTick(4);

		Assert.Equal(elapsed, viewModel.ElapsedSecAtLevel);
	}

	public void Dispose()
	{
		foreach (var viewModel in _created)
			viewModel.Dispose();
	}

	private GameViewModel CreateGame(InMemoryGameProgressRepository? repository = null)
	{
		var viewModel = new GameViewModel(
			new RecordSessionUseCase(repository ?? new InMemoryGameProgressRepository()),
			autoTick: false);
		_created.Add(viewModel);
		return viewModel;
	}
}
