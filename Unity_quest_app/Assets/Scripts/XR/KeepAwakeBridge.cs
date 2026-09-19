using UnityEngine;

namespace MindSilence.XR
{
    /// <summary>
    /// Analog of FLAG_KEEP_SCREEN_ON. NeverSleep only while the session tick
    /// runs in the foreground. Menu and highscore stay on SystemSetting.
    /// </summary>
    public static class KeepAwakeBridge
    {
        public static void SetEnabled(bool enabled)
        {
            Screen.sleepTimeout = enabled ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting;
        }
    }
}
