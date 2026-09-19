namespace MindSilence.Domain
{
    public enum GamePhase
    {
        Idle,
        Running,
    }

    public readonly struct SessionSummary : System.IEquatable<SessionSummary>
    {
        public SessionSummary(int levelReached, int bestToday, int totalSeconds)
        {
            LevelReached = levelReached;
            BestToday = bestToday;
            TotalSeconds = totalSeconds;
        }

        public int LevelReached { get; }
        public int BestToday { get; }
        public int TotalSeconds { get; }

        public bool Equals(SessionSummary other)
        {
            return LevelReached == other.LevelReached
                && BestToday == other.BestToday
                && TotalSeconds == other.TotalSeconds;
        }

        public override bool Equals(object obj)
        {
            return obj is SessionSummary other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (LevelReached * 397) ^ (BestToday * 17) ^ TotalSeconds;
        }

        public static bool operator ==(SessionSummary left, SessionSummary right) => left.Equals(right);

        public static bool operator !=(SessionSummary left, SessionSummary right) => !left.Equals(right);
    }

    public readonly struct GameSessionState
    {
        public GameSessionState(
            GamePhase phase = GamePhase.Idle,
            int level = 0,
            int elapsedSecAtLevel = 0,
            SessionSummary? sessionSummary = null)
        {
            Phase = phase;
            Level = level;
            ElapsedSecAtLevel = elapsedSecAtLevel;
            SessionSummary = sessionSummary;
        }

        public GamePhase Phase { get; }
        public int Level { get; }
        public int ElapsedSecAtLevel { get; }
        public SessionSummary? SessionSummary { get; }

        public int RequiredSecAtLevel => LevelDuration.DurationForLevel(Level);

        public float ProgressFraction
        {
            get
            {
                var required = RequiredSecAtLevel;
                if (required <= 0)
                {
                    return 0f;
                }

                var fraction = (float)ElapsedSecAtLevel / required;
                if (fraction < 0f)
                {
                    return 0f;
                }

                if (fraction > 1f)
                {
                    return 1f;
                }

                return fraction;
            }
        }
    }
}
