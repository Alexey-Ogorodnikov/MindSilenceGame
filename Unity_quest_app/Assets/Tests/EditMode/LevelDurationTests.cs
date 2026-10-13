using MindSilence.Domain;
using NUnit.Framework;

namespace MindSilence.Tests.EditMode
{
    public sealed class LevelDurationTests
    {
        [Test]
        public void DurationForLevel_MatchesLevelPeriods()
        {
            Assert.AreEqual(0, LevelDuration.DurationForLevel(0));
            Assert.AreEqual(0, LevelDuration.DurationForLevel(-1));
            Assert.AreEqual(5, LevelDuration.DurationForLevel(1));
            Assert.AreEqual(10, LevelDuration.DurationForLevel(2));
            Assert.AreEqual(15, LevelDuration.DurationForLevel(3));
            Assert.AreEqual(30, LevelDuration.DurationForLevel(4));
            Assert.AreEqual(60, LevelDuration.DurationForLevel(5));
            Assert.AreEqual(120, LevelDuration.DurationForLevel(6));
            Assert.AreEqual(240, LevelDuration.DurationForLevel(7));
        }

        [Test]
        public void TotalSessionSeconds_SumsCompletedLevelsAndElapsed()
        {
            Assert.AreEqual(0, LevelDuration.TotalSessionSeconds(0, 5));
            Assert.AreEqual(0, LevelDuration.TotalSessionSeconds(1, 0));
            Assert.AreEqual(5, LevelDuration.TotalSessionSeconds(2, 0));
            Assert.AreEqual(15, LevelDuration.TotalSessionSeconds(3, 0));
            Assert.AreEqual(16, LevelDuration.TotalSessionSeconds(3, 1));
        }
    }
}
