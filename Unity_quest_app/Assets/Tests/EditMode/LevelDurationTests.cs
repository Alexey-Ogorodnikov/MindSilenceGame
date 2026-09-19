using MindSilence.Domain;
using NUnit.Framework;

namespace MindSilence.Tests.EditMode
{
    public sealed class LevelDurationTests
    {
        [Test]
        public void DurationForLevel_MatchesGeometricProgression()
        {
            Assert.AreEqual(0, LevelDuration.DurationForLevel(0));
            Assert.AreEqual(0, LevelDuration.DurationForLevel(-1));
            Assert.AreEqual(4, LevelDuration.DurationForLevel(1));
            Assert.AreEqual(8, LevelDuration.DurationForLevel(2));
            Assert.AreEqual(16, LevelDuration.DurationForLevel(3));
            Assert.AreEqual(32, LevelDuration.DurationForLevel(4));
            Assert.AreEqual(64, LevelDuration.DurationForLevel(5));
        }

        [Test]
        public void TotalSessionSeconds_SumsCompletedLevelsAndElapsed()
        {
            Assert.AreEqual(0, LevelDuration.TotalSessionSeconds(0, 5));
            Assert.AreEqual(0, LevelDuration.TotalSessionSeconds(1, 0));
            Assert.AreEqual(4, LevelDuration.TotalSessionSeconds(2, 0));
            Assert.AreEqual(12, LevelDuration.TotalSessionSeconds(3, 0));
            Assert.AreEqual(13, LevelDuration.TotalSessionSeconds(3, 1));
        }
    }
}
