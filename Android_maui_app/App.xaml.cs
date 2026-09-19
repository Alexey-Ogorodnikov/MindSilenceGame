using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using MindSilence.Presentation.Navigation;
using MindSilence.Presentation.Splash;

namespace MindSilence;

public partial class App : Application
{
	private readonly IServiceProvider _services;
	private readonly SplashViewModelFactory _splashFactory;
	private Window? _window;
	private SplashPage? _splashPage;
	private AppHostPage? _hostPage;
	private bool _listening;

	public App(IServiceProvider services, SplashViewModelFactory splashFactory)
	{
		InitializeComponent();
		// Light calm palette after splash (Kotlin LightColorScheme). Do not follow OS dark.
		UserAppTheme = AppTheme.Light;
		_services = services;
		_splashFactory = splashFactory;
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		UserAppTheme = AppTheme.Light;
		var splash = _splashFactory.Current
			?? throw new InvalidOperationException("SplashViewModel must be created before the window.");

		_splashPage = _services.GetRequiredService<SplashPage>();
		_hostPage = _services.GetRequiredService<AppHostPage>();

		if (!_listening)
		{
			splash.PropertyChanged += OnSplashPropertyChanged;
			_listening = true;
		}

		_window = new Window(splash.ShowBrandedSplash ? (Page)_splashPage : _hostPage);
		return _window;
	}

	private void OnSplashPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is nameof(SplashViewModel.ShowBrandedSplash) or null)
		{
			MainThread.BeginInvokeOnMainThread(ApplyCurrentPage);
		}
	}

	private void ApplyCurrentPage()
	{
		if (_window is null || _splashFactory.Current is null || _splashPage is null || _hostPage is null)
		{
			return;
		}

		Page page = _splashFactory.Current.ShowBrandedSplash ? _splashPage : _hostPage;
		if (!ReferenceEquals(_window.Page, page))
		{
			_window.Page = page;
		}
	}
}
