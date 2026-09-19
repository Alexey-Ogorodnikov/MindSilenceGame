using System;

namespace MindSilence.Domain
{
    public readonly struct DailyStats
    {
        public DailyStats(DateTime date, int attempts, int totalSeconds, int bestLevel)
        {
            Date = date.Date;
            Attempts = attempts;
            TotalSeconds = totalSeconds;
            BestLevel = bestLevel;
        }

        public DateTime Date { get; }
        public int Attempts { get; }
        public int TotalSeconds { get; }
        public int BestLevel { get; }
    }
}
