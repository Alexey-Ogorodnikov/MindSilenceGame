using Android.App;
using Android.Runtime;
using AndroidX.AppCompat.App;

namespace MindSilence;

[Application]
public class MainApplication : MauiApplication
{
	public MainApplication(IntPtr handle, JniHandleOwnership ownership)
		: base(handle, ownership)
	{
	}

	public override void OnCreate()
	{
		// Native DayNight must match MAUI Light, or Android keeps the OS dark theme.
		AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightNo;
		base.OnCreate();
	}

	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
