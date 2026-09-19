using UnityEngine;

namespace MindSilence.Presentation
{
    /// <summary>
    /// Training ring geometry. Neon centroid matches GameScreen.kt / circle.png.
    /// </summary>
    public static class TrainingLayout
    {
        public const float RingSize = 1280f;
        public const float RingCenterXFraction = 625.35f / 1254f;
        public const float RingCenterYFraction = 614.37f / 1254f;
        public const float FocusWidth = 1280f;
        public const float FocusHeight = 1432f;
        public const float LevelLabelHeight = 48f;
        public const int GlyphFontSize = 128;
        public const float GlyphWidth = 640f / 3f;
        public const float GlyphHeight = 160f;
        public const float ProgressWidth = 1280f;
        public const float BackWidth = 200f;
        public const float BackHeight = 100f;
        public const float ActionWidth = 250f;
        public const float ActionHeight = 100f;
        public const float ActionSpacing = 50f;
        public const float ProgressHeight = 16f;
        public const string RingResourceName = "circle";

        public static Vector2 LevelGlyphAnchoredPosition(float ringSize = RingSize)
        {
            var x = (RingCenterXFraction - 0.5f) * ringSize;
            var y = (0.5f - RingCenterYFraction) * ringSize;
            return new Vector2(x, y);
        }
    }
}
