using MindSilence.Presentation.Splash;

namespace MindSilence.Tests;

public sealed class SplashViewModelTests
{
	[Fact]
	public async Task Cold_start_shows_branded_splash_until_duration_elapses()
	{
		var delay = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		var viewModel = new SplashViewModel(startWithBrandedSplash: true, TimeProvider.System, _ => delay.Task);

		Assert.True(viewModel.ShowBrandedSplash);
		Assert.True(viewModel.KeepSystemSplash);

		delay.SetResult();
		await viewModel.DismissTask;

		Assert.False(viewModel.ShowBrandedSplash);
		Assert.False(viewModel.KeepSystemSplash);
	}

	[Fact]
	public void Restore_skips_branded_splash()
	{
		var viewModel = new SplashViewModel(startWithBrandedSplash: false);

		Assert.False(viewModel.ShowBrandedSplash);
		Assert.False(viewModel.KeepSystemSplash);
	}

	[Fact]
	public void Content_measured_releases_system_splash()
	{
		var viewModel = new SplashViewModel(startWithBrandedSplash: true);

		viewModel.OnContentMeasured();

		Assert.True(viewModel.ShowBrandedSplash);
		Assert.False(viewModel.KeepSystemSplash);
	}
}

public sealed class SplashDefaultsTests
{
	[Fact]
	public void Icon_size_matches_system_splash_slot() =>
		Assert.Equal(288d, SplashDefaults.IconSize);

	[Fact]
	public void Duration_matches_branded_splash_handoff() =>
		Assert.Equal(2000, SplashDefaults.DurationMs);

	[Fact]
	public void Title_metrics_match_branded_splash_wordmark()
	{
		Assert.Equal(24d, SplashDefaults.TitleSpacing);
		Assert.Equal(32d, SplashDefaults.TitleFontSize);
		Assert.Equal(1d, SplashDefaults.TitleLetterSpacing);
		Assert.Equal(10f, SplashDefaults.TitleShadowBlurPx);
		Assert.Equal(0.55f, SplashDefaults.TitleShadowAlpha);
		Assert.Equal("splash_icon", SplashDefaults.IconTestTag);
	}
}
