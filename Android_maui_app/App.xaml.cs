using Microsoft.Extensions.DependencyInjection;
using MindSilence.Presentation.Navigation;
using MindSilence.Presentation.Splash;

namespace MindSilence;

public partial class App : Application
{
	private readonly IServiceProvider _services;
	private readonly SplashViewModelFactory _splashFactory;
	private Window? _window;
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

		var splashPage = _services.GetRequiredService<SplashPage>();
		var hostPage = _services.GetRequiredService<AppHostPage>();

		if (!_listening)
		{
			splash.PropertyChanged += (_, e) =>
			{
				if (e.PropertyName is nameof(SplashViewModel.ShowBrandedSplash) or null)
				{
					MainThread.BeginInvokeOnMainThread(() => ApplyPage(splash, splashPage, hostPage));
				}
			};
			_listening = true;
		}

		_window = new Window(splash.ShowBrandedSplash ? (Page)splashPage : hostPage);
		return _window;
	}

	private void ApplyPage(SplashViewModel splash, SplashPage splashPage, AppHostPage hostPage)
	{
		if (_window is null)
		{
			return;
		}

		Page page = splash.ShowBrandedSplash ? splashPage : hostPage;
		if (!ReferenceEquals(_window.Page, page))
		{
			_window.Page = page;
		}
	}
}
