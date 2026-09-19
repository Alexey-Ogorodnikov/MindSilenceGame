using System;
using System.IO;
using MindSilence.Data;
using NUnit.Framework;

namespace MindSilence.Tests.EditMode
{
    public sealed class JsonGameProgressRepositoryTests
    {
        private string _directory;
        private string _filePath;

        [SetUp]
        public void SetUp()
        {
            _directory = Path.Combine(Path.GetTempPath(), "MindSilenceJsonRepoTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_directory);
            _filePath = Path.Combine(_directory, JsonGameProgressRepository.FileName);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_directory))
                Directory.Delete(_directory, recursive: true);
        }

        [Test]
        public void RecordSession_RoundTripsMergeAndReturnsBestToday()
        {
            var today = new DateTime(2026, 9, 13);
            var repository = new JsonGameProgressRepository(_filePath, () => today);

            var bestAfterFirst = repository.RecordSession(levelReached: 2, totalSeconds: 3);
            var bestAfterSecond = repository.RecordSession(levelReached: 1, totalSeconds: 5);

            Assert.AreEqual(2, bestAfterFirst);
            Assert.AreEqual(2, bestAfterSecond);

            var reloaded = new JsonGameProgressRepository(_filePath, () => today);
            var stats = reloaded.GetDailyStats();
            Assert.AreEqual(1, stats.Count);
            Assert.AreEqual(today, stats[0].Date);
            Assert.AreEqual(2, stats[0].Attempts);
            Assert.AreEqual(8, stats[0].TotalSeconds);
            Assert.AreEqual(2, stats[0].BestLevel);
            Assert.IsTrue(File.Exists(_filePath));
            Assert.AreEqual(_directory, Path.GetDirectoryName(_filePath));
        }

        [Test]
        public void GetDailyStats_ReturnsNewestDayFirst_WhenDiskIsOldestFirst()
        {
            File.WriteAllText(
                _filePath,
                "[" +
                "{\"date\":\"2026-09-01\",\"attempts\":1,\"totalSeconds\":10,\"bestLevel\":2}," +
                "{\"date\":\"2026-09-03\",\"attempts\":2,\"totalSeconds\":5,\"bestLevel\":4}" +
                "]");

            var stats = new JsonGameProgressRepository(_filePath).GetDailyStats();

            Assert.AreEqual(2, stats.Count);
            Assert.AreEqual(new DateTime(2026, 9, 3), stats[0].Date);
            Assert.AreEqual(2, stats[0].Attempts);
            Assert.AreEqual(5, stats[0].TotalSeconds);
            Assert.AreEqual(4, stats[0].BestLevel);
            Assert.AreEqual(new DateTime(2026, 9, 1), stats[1].Date);
            Assert.AreEqual(1, stats[1].Attempts);
            Assert.AreEqual(10, stats[1].TotalSeconds);
            Assert.AreEqual(2, stats[1].BestLevel);
        }

        [Test]
        public void RecordSession_WritesOldestDayFirstOnDisk()
        {
            var current = new DateTime(2026, 9, 1);
            var repository = new JsonGameProgressRepository(_filePath, () => current);
            repository.RecordSession(levelReached: 1, totalSeconds: 10);
            current = new DateTime(2026, 9, 13);
            repository.RecordSession(levelReached: 4, totalSeconds: 5);

            var onDisk = File.ReadAllText(_filePath);
            var olderIndex = onDisk.IndexOf("2026-09-01", StringComparison.Ordinal);
            var newerIndex = onDisk.IndexOf("2026-09-13", StringComparison.Ordinal);
            Assert.Greater(olderIndex, -1);
            Assert.Greater(newerIndex, olderIndex);

            var stats = new JsonGameProgressRepository(_filePath).GetDailyStats();
            Assert.AreEqual(2, stats.Count);
            Assert.AreEqual(new DateTime(2026, 9, 13), stats[0].Date);
            Assert.AreEqual(new DateTime(2026, 9, 1), stats[1].Date);
        }

        [Test]
        public void CorruptJson_YieldsEmptyStats()
        {
            File.WriteAllText(_filePath, "not-json");

            var stats = new JsonGameProgressRepository(_filePath).GetDailyStats();

            Assert.AreEqual(0, stats.Count);
        }

        [Test]
        public void MissingFile_YieldsEmptyStats()
        {
            var stats = new JsonGameProgressRepository(_filePath).GetDailyStats();

            Assert.AreEqual(0, stats.Count);
            Assert.IsFalse(File.Exists(_filePath));
        }
    }
}
