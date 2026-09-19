using Microsoft.Extensions.DependencyInjection;
using MindSilence.Presentation.Game;
using MindSilence.Presentation.HighScores;
using MindSilence.Presentation.Menu;
#if ANDROID
using Android.Views;
#endif

namespace MindSilence.Presentation.Navigation;

public partial class AppHostPage : ContentPage
{
	private readonly AppViewModel _viewModel;
	private readonly MenuPage _menuPage;
	private readonly IServiceProvider _services;
	private GamePage? _gamePage;
	private HighScoresPage? _highScoresPage;
	private bool _windowEventsBound;
	private bool _themeListening;

	public AppHostPage(AppViewModel viewModel, MenuPage menuPage, IServiceProvider services)
	{
		InitializeComponent();
		_viewModel = viewModel;
		_menuPage = menuPage;
		_services = services;
		BindingContext = viewModel;
		MenuSlot.Content = menuPage;
		menuPage.BindingContext = menuPage.ViewModel;
		_menuPage.ViewModel.NavigateToTraining += OnNavigateToTraining;
		_viewModel.PropertyChanged += OnAppStateChanged;
		Loaded += OnHostLoaded;
		SyncGamePage();
		SyncHighScoresPage();
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		ApplySystemBarContrast();
		if (Application.Current is not null && !_themeListening)
		{
			Application.Current.RequestedThemeChanged += OnRequestedThemeChanged;
			_themeListening = true;
		}
	}

	private void OnRequestedThemeChanged(object? sender, AppThemeChangedEventArgs e) =>
		ApplySystemBarContrast();

	protected override bool OnBackButtonPressed()
	{
		if (_viewModel.ShowMenu && _menuPage.ViewModel.ShowHowToTrain)
		{
			_menuPage.ViewModel.OnDismissHowToTrain();
			return true;
		}

		if (_viewModel.ShowHighScores)
		{
			_viewModel.LeaveHighScores();
			return true;
		}

		if (_viewModel.InTraining)
		{
			if (_gamePage is not null && _gamePage.ViewModel.HasSessionSummary)
			{
				_gamePage.ViewModel.OnDismissSessionSummary();
				return true;
			}

			if (_gamePage is not null)
			{
				_gamePage.ViewModel.OnLeaveTraining();
			}
			else
			{
				_viewModel.LeaveTraining();
			}

			return true;
		}

		return base.OnBackButtonPressed();
	}

	private void OnHostLoaded(object? sender, EventArgs e)
	{
		if (Window is null || _windowEventsBound)
			return;

		Window.Stopped += OnWindowStopped;
		Window.Resumed += OnWindowResumed;
		_windowEventsBound = true;
	}

	private void OnAppStateChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
	{
		if (e.PropertyName is nameof(AppViewModel.InTraining) or nameof(AppViewModel.ShowHighScores) or null)
		{
			SyncGamePage();
			SyncHighScoresPage();
		}
	}

	private void SyncGamePage()
	{
		if (_viewModel.InTraining)
		{
			EnsureGamePage();
		}
		else
		{
			ReleaseGamePage();
		}
	}

	private void EnsureGamePage()
	{
		if (_gamePage is not null)
			return;

		_gamePage = _services.GetRequiredService<GamePage>();
		TrainingSlot.Content = _gamePage;
		SubscribeGameEffects(_gamePage.ViewModel);
	}

	private void ReleaseGamePage()
	{
		if (_gamePage is null)
			return;

		var viewModel = _gamePage.ViewModel;
		viewModel.Dispose();
		UnsubscribeGameEffects(viewModel);
		TrainingSlot.Content = null;
		_gamePage = null;
	}

	private void SubscribeGameEffects(GameViewModel viewModel)
	{
		viewModel.HapticOnThought += OnHapticOnThought;
		viewModel.KeepScreenOn += OnKeepScreenOn;
		viewModel.NavigateToHighScores += OnNavigateToHighScores;
		viewModel.NavigateBackToMenu += OnNavigateBackToMenu;
	}

	private void UnsubscribeGameEffects(GameViewModel viewModel)
	{
		viewModel.HapticOnThought -= OnHapticOnThought;
		viewModel.KeepScreenOn -= OnKeepScreenOn;
		viewModel.NavigateToHighScores -= OnNavigateToHighScores;
		viewModel.NavigateBackToMenu -= OnNavigateBackToMenu;
	}

	private void OnWindowStopped(object? sender, EventArgs e) =>
		_gamePage?.ViewModel.OnAppBackgrounded();

	private void OnWindowResumed(object? sender, EventArgs e) =>
		_gamePage?.ViewModel.OnAppForegrounded();

	private void OnNavigateToTraining(object? sender, EventArgs e) => _viewModel.OpenTraining();

	private void SyncHighScoresPage()
	{
		if (_viewModel.ShowHighScores)
			EnsureHighScoresPage();
		else
			ReleaseHighScoresPage();
	}

	private void EnsureHighScoresPage()
	{
		if (_highScoresPage is not null)
			return;

		_highScoresPage = _services.GetRequiredService<HighScoresPage>();
		_highScoresPage.ViewModel.NavigateBack += OnNavigateBackFromHighScores;
		HighScoresSlot.Content = _highScoresPage;
	}

	private void ReleaseHighScoresPage()
	{
		if (_highScoresPage is null)
			return;

		_highScoresPage.ViewModel.NavigateBack -= OnNavigateBackFromHighScores;
		HighScoresSlot.Content = null;
		_highScoresPage = null;
	}

	private void OnNavigateToHighScores(object? sender, EventArgs e) => _viewModel.OpenHighScores();

	private void OnNavigateBackToMenu(object? sender, EventArgs e) => _viewModel.LeaveTraining();

	private void OnNavigateBackFromHighScores(object? sender, EventArgs e) => _viewModel.LeaveHighScores();

	private static void OnHapticOnThought(object? sender, EventArgs e) =>
		HapticFeedback.Default.Perform(HapticFeedbackType.LongPress);

	private static void OnKeepScreenOn(object? sender, bool enabled) =>
		DeviceDisplay.Current.KeepScreenOn = enabled;

	private static void ApplySystemBarContrast()
	{
#if ANDROID
		if (!OperatingSystem.IsAndroidVersionAtLeast(30))
		{
			return;
		}

		var controller = Platform.CurrentActivity?.Window?.InsetsController;
		if (controller is null)
		{
			return;
		}

		var lightBackground = Application.Current?.RequestedTheme != AppTheme.Dark;
		var flags = (int)(WindowInsetsControllerAppearance.LightStatusBars
			| WindowInsetsControllerAppearance.LightNavigationBars);
		controller.SetSystemBarsAppearance(lightBackground ? flags : 0, flags);
#endif
	}
}
