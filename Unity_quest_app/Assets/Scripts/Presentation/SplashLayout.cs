using UnityEngine;

namespace MindSilence.Presentation
{
    /// <summary>
    /// Icon stays geometrically centered. Title sits under the slot (not a centered icon+title column).
    /// </summary>
    public static class SplashLayout
    {
        public static float TitleOffsetFromTop(float panelHeight)
        {
            return ((panelHeight - SplashDefaults.IconSize) * 0.5f)
                + SplashDefaults.IconSize
                + SplashDefaults.TitleSpacing;
        }

        public static Vector2 TitleAnchoredPosition(float panelHeight)
        {
            return new Vector2(0f, -TitleOffsetFromTop(panelHeight));
        }
    }
}
