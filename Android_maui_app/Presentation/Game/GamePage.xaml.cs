using System.ComponentModel;
#if ANDROID
using Android.Widget;
#endif

namespace MindSilence.Presentation.Game;

public partial class GamePage : ContentView
{
	/// <summary>On-screen ring box; same 480dp slot as Kotlin <c>FocusRingSize</c>.</summary>
	private const double FocusRingSize = 480;

	/// <summary>Visual center of the neon ring in <c>circle.png</c> (pixel centroid / image size).</summary>
	private const double RingCenterXFraction = 625.35 / 1254d;

	/// <summary>Visual center of the neon ring in <c>circle.png</c> (pixel centroid / image size).</summary>
	private const double RingCenterYFraction = 614.37 / 1254d;

	public GameViewModel ViewModel { get; }

	public GamePage(GameViewModel viewModel)
	{
		InitializeComponent();
		ViewModel = viewModel;
		BindingContext = viewModel;
		viewModel.PropertyChanged += OnViewModelPropertyChanged;
		SizeChanged += (_, _) => PositionLevelGlyph();
		LevelLabel.SizeChanged += (_, _) => PositionLevelGlyph();
		Loaded += (_, _) => PositionLevelGlyph();
	}

	protected override void OnHandlerChanged()
	{
		base.OnHandlerChanged();
		PositionLevelGlyph();
	}

	private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is nameof(GameViewModel.LevelDisplayText) or null)
		{
			Dispatcher.Dispatch(PositionLevelGlyph);
		}
	}

	private void PositionLevelGlyph()
	{
		if (LevelLabel is null)
		{
			return;
		}

#if ANDROID
		if (TryPositionByInk(out var translationX, out var translationY))
		{
			LevelLabel.TranslationX = translationX;
			LevelLabel.TranslationY = translationY;
			return;
		}
#endif

		// Layout box until the platform TextView has a size (and on non-Android TFMs).
		if (LevelLabel.Width <= 0 || LevelLabel.Height <= 0)
		{
			return;
		}

		LevelLabel.TranslationX = FocusRingSize * RingCenterXFraction - LevelLabel.Width / 2;
		LevelLabel.TranslationY = FocusRingSize * RingCenterYFraction - LevelLabel.Height / 2;
	}

#if ANDROID
	/// <summary>
	/// Aligns glyph ink to the neon centroid. Requires a wrap-content label at the ring origin
	/// (not Fill + centered text — that math treated gravity-centered ink as if it sat at 0,0).
	/// </summary>
	private bool TryPositionByInk(out double translationX, out double translationY)
	{
		translationX = 0;
		translationY = 0;
		if (LevelLabel.Handler?.PlatformView is not TextView textView
			|| textView.Paint is null
			|| string.IsNullOrEmpty(textView.Text)
			|| textView.Width <= 0
			|| textView.Height <= 0)
		{
			return false;
		}

		var bounds = new Android.Graphics.Rect();
		var text = textView.Text;
		textView.Paint.GetTextBounds(text, 0, text.Length, bounds);
		var density = DeviceDisplay.MainDisplayInfo.Density;
		var ringPx = FocusRingSize * density;
		var inkLeft = textView.PaddingLeft + bounds.Left;
		var inkTop = textView.Baseline + bounds.Top;
		var inkCenterX = inkLeft + bounds.Width() / 2.0;
		var inkCenterY = inkTop + bounds.Height() / 2.0;
		translationX = (ringPx * RingCenterXFraction - inkCenterX) / density;
		translationY = (ringPx * RingCenterYFraction - inkCenterY) / density;
		return true;
	}
#endif
}
