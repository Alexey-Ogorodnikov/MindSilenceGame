using UnityEngine;

namespace MindSilence.Presentation
{
    /// <summary>
    /// Handbook palette. Splash is always white; panels after splash stay light.
    /// </summary>
    public static class AppPalette
    {
        public static readonly Color SplashBackground = Color.white;
        public static readonly Color CalmBackgroundLight = new Color32(0xF2, 0xF5, 0xF8, 0xFF);
        public static readonly Color CalmSurfaceLight = Color.white;
        public static readonly Color CalmBlue = new Color32(0x4A, 0x6F, 0xA5, 0xFF);
        public static readonly Color CalmGreen = new Color32(0x5B, 0x8A, 0x72, 0xFF);
        public static readonly Color OnBackground = new Color32(0x1A, 0x23, 0x30, 0xFF);
        public static readonly Color TitleGradientStart = new Color32(0x1F, 0xC0, 0xE0, 0xFF);
        public static readonly Color TitleGradientMid = new Color32(0x1A, 0x9A, 0xD4, 0xFF);
        public static readonly Color TitleGradientEnd = new Color32(0x0E, 0x72, 0xB8, 0xFF);
        public static readonly Color TitleShadow = new Color(0f, 0f, 0f, SplashDefaults.TitleShadowAlpha);
    }
}
