namespace MindSilence.Domain
{
    public static class LevelDuration
    {
        public static int DurationForLevel(int level)
        {
            if (level <= 0)
            {
                return 0;
            }

            return 4 << (level - 1);
        }

        public static int TotalSessionSeconds(int level, int elapsedSecAtLevel)
        {
            if (level <= 0)
            {
                return 0;
            }

            var total = elapsedSecAtLevel;
            for (var currentLevel = 1; currentLevel < level; currentLevel++)
            {
                total += DurationForLevel(currentLevel);
            }

            return total;
        }
    }
}
