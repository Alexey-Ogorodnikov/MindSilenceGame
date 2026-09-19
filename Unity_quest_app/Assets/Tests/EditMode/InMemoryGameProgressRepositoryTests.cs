using System;
using MindSilence.Data;
using NUnit.Framework;

namespace MindSilence.Tests.EditMode
{
    public sealed class InMemoryGameProgressRepositoryTests
    {
        [Test]
        public void RecordSession_MergesTodayAndReturnsBestToday()
        {
            var today = new DateTime(2026, 9, 13);
            var repository = new InMemoryGameProgressRepository(() => today);

            var bestAfterFirst = repository.RecordSession(levelReached: 2, totalSeconds: 3);
            var bestAfterSecond = repository.RecordSession(levelReached: 1, totalSeconds: 5);

            Assert.AreEqual(2, bestAfterFirst);
            Assert.AreEqual(2, bestAfterSecond);

            var stats = repository.GetDailyStats();
            Assert.AreEqual(1, stats.Count);
            Assert.AreEqual(today, stats[0].Date);
            Assert.AreEqual(2, stats[0].Attempts);
            Assert.AreEqual(8, stats[0].TotalSeconds);
            Assert.AreEqual(2, stats[0].BestLevel);
        }

        [Test]
        public void GetDailyStats_ReturnsNewestDayFirst()
        {
            var older = new DateTime(2026, 9, 1);
            var newer = new DateTime(2026, 9, 13);
            var current = older;
            var repository = new InMemoryGameProgressRepository(() => current);

            repository.RecordSession(levelReached: 1, totalSeconds: 10);
            current = newer;
            repository.RecordSession(levelReached: 4, totalSeconds: 5);

            var stats = repository.GetDailyStats();
            Assert.AreEqual(2, stats.Count);
            Assert.AreEqual(newer, stats[0].Date);
            Assert.AreEqual(4, stats[0].BestLevel);
            Assert.AreEqual(older, stats[1].Date);
            Assert.AreEqual(1, stats[1].BestLevel);
        }
    }
}
