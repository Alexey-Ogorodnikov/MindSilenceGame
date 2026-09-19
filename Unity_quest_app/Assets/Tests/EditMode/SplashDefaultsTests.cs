using MindSilence.Presentation;
using NUnit.Framework;

namespace MindSilence.Tests.EditMode
{
    public sealed class SplashDefaultsTests
    {
        [Test]
        public void IconSize_MatchesSystemSplashSlot()
        {
            Assert.AreEqual(288f, SplashDefaults.IconSize);
        }

        [Test]
        public void Duration_MatchesBrandedSplashHandoff()
        {
            Assert.AreEqual(2000, SplashDefaults.DurationMs);
        }

        [Test]
        public void TitleMetrics_MatchBrandedSplashWordmark()
        {
            Assert.AreEqual(24f, SplashDefaults.TitleSpacing);
            Assert.AreEqual(32f, SplashDefaults.TitleFontSize);
            Assert.AreEqual(1f, SplashDefaults.TitleLetterSpacing);
            Assert.AreEqual(10f, SplashDefaults.TitleShadowBlurPx);
            Assert.AreEqual(0.55f, SplashDefaults.TitleShadowAlpha);
            Assert.AreEqual("splash_icon", SplashDefaults.IconTestTag);
        }
    }

    public sealed class SplashLayoutTests
    {
        [Test]
        public void TitleSitsUnderCenteredIconSlot_NotInCenteredColumn()
        {
            const float panelHeight = 1800f;
            var expected = ((panelHeight - 288f) * 0.5f) + 288f + 24f;
            Assert.AreEqual(1068f, expected);
            Assert.AreEqual(expected, SplashLayout.TitleOffsetFromTop(panelHeight));
            Assert.AreEqual(0f, SplashLayout.TitleAnchoredPosition(panelHeight).x);
            Assert.AreEqual(-expected, SplashLayout.TitleAnchoredPosition(panelHeight).y);
        }
    }
}
