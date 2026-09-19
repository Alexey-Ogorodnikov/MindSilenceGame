using System;
using System.Collections.Generic;
using MindSilence.Domain;

namespace MindSilence.Data
{
    public sealed class InMemoryGameProgressRepository : IGameProgressRepository
    {
        private readonly Func<DateTime> _today;
        private readonly Dictionary<DateTime, DailyStats> _statsByDate = new Dictionary<DateTime, DailyStats>();

        public InMemoryGameProgressRepository(Func<DateTime> today = null)
        {
            _today = today ?? (() => DateTime.Now.Date);
        }

        public int RecordSession(int levelReached, int totalSeconds)
        {
            var today = _today().Date;
            DailyStats updated;
            if (!_statsByDate.TryGetValue(today, out var current))
            {
                updated = new DailyStats(today, attempts: 1, totalSeconds, levelReached);
            }
            else
            {
                var bestLevel = current.BestLevel > levelReached ? current.BestLevel : levelReached;
                updated = new DailyStats(
                    today,
                    current.Attempts + 1,
                    current.TotalSeconds + totalSeconds,
                    bestLevel);
            }

            _statsByDate[today] = updated;
            return updated.BestLevel;
        }

        public IReadOnlyList<DailyStats> GetDailyStats()
        {
            var stats = new List<DailyStats>(_statsByDate.Values);
            stats.Sort((left, right) => right.Date.CompareTo(left.Date));
            return stats;
        }
    }
}
