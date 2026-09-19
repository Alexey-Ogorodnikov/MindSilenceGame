#if ANDROID
using Android.Graphics;
using Android.Views;
using Android.Widget;
using Microsoft.Maui.Graphics;
using Color = Microsoft.Maui.Graphics.Color;
#endif

namespace MindSilence.Presentation.Splash;

public partial class SplashPage : ContentPage
{
	public SplashPage(SplashViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
		SafeAreaEdges = SafeAreaEdges.None;
		Loaded += (_, _) =>
		{
			if (Height > 0)
			{
				NotifyMeasured();
			}
		};
	}

	protected override void OnHandlerChanged()
	{
		base.OnHandlerChanged();
#if ANDROID
		ApplyTitlePaint();
		ApplyLightStatusBars(enabled: true);
#endif
	}

	protected override void OnSizeAllocated(double width, double height)
	{
		base.OnSizeAllocated(width, height);
		if (height <= 0)
		{
			return;
		}

		var top = (height - SplashDefaults.IconSize) / 2d
			+ SplashDefaults.IconSize
			+ SplashDefaults.TitleSpacing;
		TitleLabel.Margin = new Thickness(0, top, 0, 0);

		NotifyMeasured();
#if ANDROID
		ApplyTitlePaint();
#endif
	}

	private void NotifyMeasured()
	{
		if (BindingContext is SplashViewModel viewModel)
		{
			viewModel.OnContentMeasured();
		}
	}

	protected override void OnDisappearing()
	{
#if ANDROID
		ApplyLightStatusBars(enabled: false);
#endif
		base.OnDisappearing();
	}

#if ANDROID
	private void ApplyTitlePaint()
	{
		if (TitleLabel.Handler?.PlatformView is not TextView textView || Application.Current is null)
		{
			return;
		}

		var start = ToArgb((Color)Application.Current.Resources["SplashTitleGradientStart"]);
		var mid = ToArgb((Color)Application.Current.Resources["SplashTitleGradientMid"]);
		var end = ToArgb((Color)Application.Current.Resources["SplashTitleGradientEnd"]);
		var shadow = (Color)Application.Current.Resources["SplashTitleShadow"];
		var shadowColor = Android.Graphics.Color.Argb(
			(int)Math.Round(SplashDefaults.TitleShadowAlpha * 255),
			(int)Math.Round(shadow.Red * 255),
			(int)Math.Round(shadow.Green * 255),
			(int)Math.Round(shadow.Blue * 255));

		if (OperatingSystem.IsAndroidVersionAtLeast(28))
		{
			textView.SetTypeface(Typeface.Create(Typeface.Serif, 500, italic: false), TypefaceStyle.Normal);
		}
		else
		{
			textView.SetTypeface(Typeface.Serif, TypefaceStyle.Normal);
		}

		textView.SetShadowLayer(SplashDefaults.TitleShadowBlurPx, 0f, 0f, shadowColor);
		var paint = textView.Paint;
		if (paint is null)
		{
			return;
		}

		paint.SetShader(new LinearGradient(
			0f,
			0f,
			0f,
			textView.TextSize,
			[start, mid, end],
			[0f, 0.5f, 1f],
			Shader.TileMode.Clamp!));
		textView.Invalidate();
	}

	private static void ApplyLightStatusBars(bool enabled)
	{
		if (!OperatingSystem.IsAndroidVersionAtLeast(30))
		{
			return;
		}

		var controller = Platform.CurrentActivity?.Window?.InsetsController;
		if (controller is null)
		{
			return;
		}

		var flag = (int)WindowInsetsControllerAppearance.LightStatusBars;
		controller.SetSystemBarsAppearance(enabled ? flag : 0, flag);
	}

	private static int ToArgb(Color color) =>
		Android.Graphics.Color.Argb(
			(int)Math.Round(color.Alpha * 255),
			(int)Math.Round(color.Red * 255),
			(int)Math.Round(color.Green * 255),
			(int)Math.Round(color.Blue * 255));
#endif
}
