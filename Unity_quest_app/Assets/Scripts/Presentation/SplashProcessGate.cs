namespace MindSilence.Presentation
{
    /// <summary>
    /// Branded splash is a process-lifetime cold start. Re-entering the scene or resume skips it.
    /// </summary>
    public static class SplashProcessGate
    {
        static bool _coldStartConsumed;

        public static bool ConsumeColdStart()
        {
            if (_coldStartConsumed)
            {
                return false;
            }

            _coldStartConsumed = true;
            return true;
        }

        public static void ResetForTests()
        {
            _coldStartConsumed = false;
        }
    }
}
