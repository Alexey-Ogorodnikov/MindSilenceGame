using CommunityToolkit.Mvvm.ComponentModel;

namespace MindSilence.Presentation.Splash;

/// <summary>
/// Branded-splash timing. The host reads <see cref="ShowBrandedSplash"/>;
/// the Android activity reads <see cref="KeepSystemSplash"/>. No navigation effects.
/// </summary>
public sealed partial class SplashViewModel : ObservableObject
{
	private readonly Func<TimeSpan, Task> _delayAsync;
	private readonly Task? _dismissTask;

	[ObservableProperty]
	private bool showBrandedSplash;

	[ObservableProperty]
	private bool keepSystemSplash;

	public SplashViewModel(bool startWithBrandedSplash, TimeProvider? timeProvider = null)
		: this(startWithBrandedSplash, timeProvider, delayAsync: null)
	{
	}

	internal SplashViewModel(bool startWithBrandedSplash, TimeProvider? timeProvider, Func<TimeSpan, Task>? delayAsync)
	{
		var provider = timeProvider ?? TimeProvider.System;
		_delayAsync = delayAsync ?? (delay => Task.Delay(delay, provider));
		ShowBrandedSplash = startWithBrandedSplash;
		KeepSystemSplash = startWithBrandedSplash;

		if (startWithBrandedSplash)
		{
			_dismissTask = DismissAfterDelayAsync();
		}
	}

	internal Task DismissTask => _dismissTask ?? Task.CompletedTask;

	public void OnContentMeasured()
	{
		KeepSystemSplash = false;
	}

	private async Task DismissAfterDelayAsync()
	{
		await _delayAsync(TimeSpan.FromMilliseconds(SplashDefaults.DurationMs)).ConfigureAwait(false);
		RunOnUi(() =>
		{
			ShowBrandedSplash = false;
			KeepSystemSplash = false;
		});
	}

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
}

/// <summary>
/// Holds the process-wide splash ViewModel so rotation does not restart the timer.
/// The Android activity passes the cold-start flag on first create.
/// </summary>
public sealed class SplashViewModelFactory
{
	private SplashViewModel? _viewModel;

	public SplashViewModel? Current => _viewModel;

	public SplashViewModel GetOrCreate(bool startWithBrandedSplash) =>
		_viewModel ??= new SplashViewModel(startWithBrandedSplash);
}
