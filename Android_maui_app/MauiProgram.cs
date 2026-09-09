using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;
using MindSilence.Data;
using MindSilence.Domain.Repository;
using MindSilence.Domain.UseCases;
using MindSilence.Presentation.Game;
using MindSilence.Presentation.HighScores;
using MindSilence.Presentation.Menu;
using MindSilence.Presentation.Navigation;
using MindSilence.Presentation.Splash;

namespace MindSilence;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		builder.Services.AddSingleton<IPreferences>(_ => Preferences.Default);
		builder.Services.AddSingleton<IGameProgressRepository, PreferencesGameProgressRepository>();
		builder.Services.AddTransient<RecordSessionUseCase>();
		builder.Services.AddTransient<GetDailyStatsUseCase>();
		builder.Services.AddSingleton<SplashViewModelFactory>();
		builder.Services.AddSingleton(sp =>
			sp.GetRequiredService<SplashViewModelFactory>().Current
			?? throw new InvalidOperationException("SplashViewModel requires the Android cold-start flag."));
		builder.Services.AddTransient<SplashPage>();
		builder.Services.AddSingleton<AppViewModel>();
		builder.Services.AddTransient<MenuViewModel>();
		builder.Services.AddTransient<MenuPage>();
		builder.Services.AddTransient<GameViewModel>();
		builder.Services.AddTransient<GamePage>();
		builder.Services.AddTransient<HighScoresViewModel>();
		builder.Services.AddTransient<HighScoresPage>();
		builder.Services.AddSingleton<AppHostPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
