using System;
using MindSilence.Domain;
using MindSilence.Presentation;
using NUnit.Framework;

namespace MindSilence.Tests.EditMode
{
    public sealed class SplashControllerTests
    {
        [SetUp]
        public void SetUp()
        {
            SplashProcessGate.ResetForTests();
        }

        [TearDown]
        public void TearDown()
        {
            SplashProcessGate.ResetForTests();
        }

        [Test]
        public void ColdStart_ShowsBrandedSplashUntilDurationElapses()
        {
            var time = new FakeTimeSource();
            using var splash = new SplashController(startWithBrandedSplash: true, time);

            Assert.IsTrue(splash.ShowBrandedSplash);
            Assert.IsTrue(splash.KeepSystemSplash);

            time.Advance(TimeSpan.FromMilliseconds(SplashDefaults.DurationMs - 1));
            Assert.IsTrue(splash.ShowBrandedSplash);
            Assert.IsTrue(splash.KeepSystemSplash);

            time.Advance(TimeSpan.FromMilliseconds(1));
            Assert.IsFalse(splash.ShowBrandedSplash);
            Assert.IsFalse(splash.KeepSystemSplash);
        }

        [Test]
        public void Restore_SkipsBrandedSplash()
        {
            var time = new FakeTimeSource();
            using var splash = new SplashController(startWithBrandedSplash: false, time);

            Assert.IsFalse(splash.ShowBrandedSplash);
            Assert.IsFalse(splash.KeepSystemSplash);

            time.Advance(TimeSpan.FromMilliseconds(SplashDefaults.DurationMs));
            Assert.IsFalse(splash.ShowBrandedSplash);
        }

        [Test]
        public void ContentMeasured_ReleasesSystemSplash()
        {
            var time = new FakeTimeSource();
            using var splash = new SplashController(startWithBrandedSplash: true, time);

            splash.OnContentMeasured();

            Assert.IsTrue(splash.ShowBrandedSplash);
            Assert.IsFalse(splash.KeepSystemSplash);
        }

        [Test]
        public void RepeatSceneEntry_SkipsBrandedSplashImmediately()
        {
            var time = new FakeTimeSource();
            using var first = new SplashController(SplashProcessGate.ConsumeColdStart(), time);
            Assert.IsTrue(first.ShowBrandedSplash);

            using var second = new SplashController(SplashProcessGate.ConsumeColdStart(), time);
            Assert.IsFalse(second.ShowBrandedSplash);
            Assert.IsFalse(second.KeepSystemSplash);

            time.Advance(TimeSpan.FromMilliseconds(SplashDefaults.DurationMs));
            Assert.IsFalse(second.ShowBrandedSplash);
        }
    }
}
