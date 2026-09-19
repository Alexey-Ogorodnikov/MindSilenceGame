using System.Runtime.Versioning;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Window;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using MindSilence.Presentation.Navigation;
using MindSilence.Presentation.Splash;

namespace MindSilence;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
	protected override void OnCreate(Bundle? savedInstanceState)
	{
		AndroidX.AppCompat.App.AppCompatDelegate.DefaultNightMode =
			AndroidX.AppCompat.App.AppCompatDelegate.ModeNightNo;

		var services = IPlatformApplication.Current!.Services;
		var factory = services.GetRequiredService<SplashViewModelFactory>();
		var splashViewModel = factory.GetOrCreate(savedInstanceState is null);

		var appViewModel = services.GetRequiredService<AppViewModel>();
		if (savedInstanceState is null)
		{
			appViewModel.ResetToMenu();
		}
		else
		{
			appViewModel.RestoreFromStore();
		}

		if (OperatingSystem.IsAndroidVersionAtLeast(31))
		{
			SplashScreen.SetOnExitAnimationListener(new ImmediateSplashExit());
		}

		base.OnCreate(savedInstanceState);

		Window!.DecorView.ViewTreeObserver!.AddOnPreDrawListener(
			new KeepSystemSplashPreDraw(splashViewModel, Window.DecorView));
	}

	private sealed class KeepSystemSplashPreDraw : Java.Lang.Object, ViewTreeObserver.IOnPreDrawListener
	{
		private readonly SplashViewModel _splashViewModel;
		private readonly Android.Views.View _content;

		public KeepSystemSplashPreDraw(SplashViewModel splashViewModel, Android.Views.View content)
		{
			_splashViewModel = splashViewModel;
			_content = content;
		}

		public bool OnPreDraw()
		{
			if (_splashViewModel.KeepSystemSplash)
			{
				return false;
			}

			_content.ViewTreeObserver?.RemoveOnPreDrawListener(this);
			return true;
		}
	}

	[SupportedOSPlatform("android31.0")]
	private sealed class ImmediateSplashExit : Java.Lang.Object, ISplashScreenOnExitAnimationListener
	{
		public void OnSplashScreenExit(SplashScreenView view)
		{
			view.Remove();
		}
	}
}
