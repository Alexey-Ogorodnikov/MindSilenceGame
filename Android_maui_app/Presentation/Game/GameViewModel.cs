using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MindSilence.Domain.Models;
using MindSilence.Domain.UseCases;
using MindSilence.Resources;

namespace MindSilence.Presentation.Game;

/// <summary>
/// Training session: Idle/Running, 1s tick, Thought persist, and one-shot effects.
/// Does not reference <c>AppViewModel</c> or write JSON itself.
/// </summary>
public sealed partial class GameViewModel : ObservableObject, IDisposable
{
	private static readonly TimeSpan TickInterval = TimeSpan.FromSeconds(1);
	private static readonly CultureInfo English = CultureInfo.GetCultureInfo("en");

	private readonly RecordSessionUseCase _recordSession;
	private readonly bool _autoTick;
	private CancellationTokenSource? _tickCts;
	private bool _isTicking;
	private int _tickEpoch;
	private bool _wasRunningBeforeBackground;
	private bool _disposed;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(RequiredSecAtLevel))]
	[NotifyPropertyChangedFor(nameof(ProgressFraction))]
	[NotifyPropertyChangedFor(nameof(IsIdle))]
	[NotifyPropertyChangedFor(nameof(IsRunning))]
	[NotifyPropertyChangedFor(nameof(ProgressOpacity))]
	[NotifyPropertyChangedFor(nameof(LevelDisplayText))]
	[NotifyPropertyChangedFor(nameof(LevelProgressText))]
	private GamePhase phase;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(RequiredSecAtLevel))]
	[NotifyPropertyChangedFor(nameof(ProgressFraction))]
	[NotifyPropertyChangedFor(nameof(LevelDisplayText))]
	[NotifyPropertyChangedFor(nameof(LevelProgressText))]
	private int level;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(ProgressFraction))]
	[NotifyPropertyChangedFor(nameof(LevelProgressText))]
	private int elapsedSecAtLevel;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(HasSessionSummary))]
	[NotifyPropertyChangedFor(nameof(SessionLevelReachedText))]
	[NotifyPropertyChangedFor(nameof(SessionBestTodayText))]
	[NotifyPropertyChangedFor(nameof(SessionAttemptDurationText))]
	private SessionSummary? sessionSummary;

	public int RequiredSecAtLevel =>
		Phase == GamePhase.Idle ? 0 : LevelDuration.DurationForLevel(Level);

	public double ProgressFraction
	{
		get
		{
			if (Phase == GamePhase.Idle)
				return 0;

			var required = RequiredSecAtLevel;
			if (required <= 0)
				return 0;

			return Math.Clamp((double)ElapsedSecAtLevel / required, 0, 1);
		}
	}

	public bool HasSessionSummary => SessionSummary is not null;

	public bool IsIdle => Phase == GamePhase.Idle;

	public bool IsRunning => Phase == GamePhase.Running;

	public double ProgressOpacity => IsRunning ? 1 : 0;

	public string LevelDisplayText =>
		IsRunning ? Level.ToString(English) : AppResources.level_idle;

	public string LevelProgressText =>
		ResxFormat.Format(AppResources.level_progress, ElapsedSecAtLevel, RequiredSecAtLevel);

	public string SessionLevelReachedText =>
		SessionSummary is null
			? string.Empty
			: ResxFormat.Format(AppResources.session_level_reached, SessionSummary.LevelReached);

	public string SessionBestTodayText =>
		SessionSummary is null
			? string.Empty
			: ResxFormat.Format(AppResources.session_best_today, SessionSummary.BestToday);

	public string SessionAttemptDurationText
	{
		get
		{
			if (SessionSummary is null)
				return string.Empty;

			var total = SessionSummary.TotalSeconds;
			return ResxFormat.Format(AppResources.session_attempt_duration, total / 60, total % 60);
		}
	}

	public event EventHandler? HapticOnThought;

	public event EventHandler<bool>? KeepScreenOn;

	public event EventHandler? NavigateToHighScores;

	public event EventHandler? NavigateBackToMenu;

	public GameViewModel(RecordSessionUseCase recordSession)
		: this(recordSession, autoTick: true)
	{
	}

	internal GameViewModel(RecordSessionUseCase recordSession, bool autoTick)
	{
		_recordSession = recordSession;
		_autoTick = autoTick;
	}

	[RelayCommand(CanExecute = nameof(CanStart))]
	public void OnStart()
	{
		if (Phase == GamePhase.Running || SessionSummary is not null)
			return;

		Phase = GamePhase.Running;
		Level = 1;
		ElapsedSecAtLevel = 0;
		RaiseKeepScreenOn(true);
		StartTicking();
	}

	[RelayCommand(CanExecute = nameof(CanThought))]
	public void OnThought()
	{
		if (Phase != GamePhase.Running)
			return;

		var levelReached = Level;
		var elapsed = ElapsedSecAtLevel;
		var totalSeconds = LevelDuration.TotalSessionSeconds(levelReached, elapsed);
		var bestToday = _recordSession.Invoke(levelReached, totalSeconds);

		StopTicking();
		Phase = GamePhase.Idle;
		Level = 0;
		ElapsedSecAtLevel = 0;
		SessionSummary = new SessionSummary(levelReached, bestToday, totalSeconds);
		RaiseKeepScreenOn(false);
		HapticOnThought?.Invoke(this, EventArgs.Empty);
	}

	[RelayCommand]
	public void OnDismissSessionSummary() => SessionSummary = null;

	[RelayCommand]
	public void OnOpenHighScores()
	{
		SessionSummary = null;
		NavigateToHighScores?.Invoke(this, EventArgs.Empty);
	}

	[RelayCommand]
	public void OnLeaveTraining() => NavigateBackToMenu?.Invoke(this, EventArgs.Empty);

	public void OnAppBackgrounded()
	{
		if (Phase != GamePhase.Running)
			return;

		_wasRunningBeforeBackground = true;
		StopTicking();
		RaiseKeepScreenOn(false);
	}

	public void OnAppForegrounded()
	{
		if (!_wasRunningBeforeBackground || Phase != GamePhase.Running)
		{
			_wasRunningBeforeBackground = false;
			return;
		}

		_wasRunningBeforeBackground = false;
		RaiseKeepScreenOn(true);
		StartTicking();
	}

	public void Dispose()
	{
		if (_disposed)
			return;

		_disposed = true;
		StopTicking();
		RaiseKeepScreenOn(false);
	}

	private void StartTicking()
	{
		if (_isTicking || _disposed)
			return;

		_isTicking = true;
		if (!_autoTick)
			return;

		_tickCts = new CancellationTokenSource();
		var epoch = _tickEpoch;
		_ = RunTickLoopAsync(epoch, _tickCts.Token);
	}

	private void StopTicking()
	{
		_isTicking = false;
		_tickEpoch++;
		if (_tickCts is null)
			return;

		_tickCts.Cancel();
		_tickCts.Dispose();
		_tickCts = null;
	}

	private async Task RunTickLoopAsync(int epoch, CancellationToken token)
	{
		try
		{
			while (!token.IsCancellationRequested)
			{
				await Task.Delay(TickInterval, token).ConfigureAwait(false);
				if (token.IsCancellationRequested)
					return;

				RunOnUi(() => AdvanceTick(epoch));
			}
		}
		catch (OperationCanceledException)
		{
		}
	}

	internal void StepTick(int count = 1)
	{
		var epoch = _tickEpoch;
		for (var i = 0; i < count; i++)
			AdvanceTick(epoch);
	}

	/// <summary>
	/// A tick posted before the previous stop, as if the UI dispatcher ran it after restart.
	/// </summary>
	internal void StepStaleTick() => AdvanceTick(_tickEpoch - 1);

	private void AdvanceTick(int epoch)
	{
		if (epoch != _tickEpoch || !_isTicking || Phase != GamePhase.Running)
			return;

		var nextElapsed = ElapsedSecAtLevel + 1;
		var required = RequiredSecAtLevel;
		if (nextElapsed >= required)
		{
			Level += 1;
			ElapsedSecAtLevel = 0;
		}
		else
		{
			ElapsedSecAtLevel = nextElapsed;
		}
	}

	private void RaiseKeepScreenOn(bool enabled) => KeepScreenOn?.Invoke(this, enabled);

	private static void RunOnUi(Action action)
	{
		var dispatcher = Application.Current?.Dispatcher;
		if (dispatcher is null)
		{
			action();
			return;
		}

		if (dispatcher.IsDispatchRequired)
			dispatcher.Dispatch(action);
		else
			action();
	}

	partial void OnPhaseChanged(GamePhase value)
	{
		StartCommand.NotifyCanExecuteChanged();
		ThoughtCommand.NotifyCanExecuteChanged();
	}

	private bool CanStart() => Phase == GamePhase.Idle && SessionSummary is null;

	private bool CanThought() => Phase == GamePhase.Running;

	partial void OnSessionSummaryChanged(SessionSummary? value) =>
		StartCommand.NotifyCanExecuteChanged();
}
