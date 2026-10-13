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

            if (level == 1)
            {
                return 5;
            }

            if (level == 2)
            {
                return 10;
            }

            return 15 << (level - 3);
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
