package com.mindsilence.game.presentation.splash // SplashDefaults must stay in sync with handbook pixels.

import org.junit.Assert.assertEquals // Metric equality.
import org.junit.Test // JUnit 4.

/** Guards splash sizes, duration, and the Compose test tag. */
class SplashDefaultsTest { // Same numbers as gameplay.md.

    /** 288.dp matches the system splash icon slot without an icon background color. */
    @Test
    fun `icon size matches system splash without icon background`() { // Slot size.
        assertEquals(288f, SplashDefaults.IconSize.value) // dp value.
    }

    /** Branded splash lasts 2000 ms on cold start. */
    @Test
    fun `duration matches branded splash handoff`() { // Timer.
        assertEquals(2_000L, SplashDefaults.DurationMs) // Handbook.
    }

    /** Title offset, type, shadow, and test tag. */
    @Test
    fun `title metrics match branded splash wordmark`() { // Wordmark.
        assertEquals(24f, SplashDefaults.TitleSpacing.value) // Gap under the icon.
        assertEquals(32f, SplashDefaults.TitleFontSize.value) // sp.
        assertEquals(1f, SplashDefaults.TitleLetterSpacing.value) // Tracking.
        assertEquals(10f, SplashDefaults.TitleShadowBlurPx) // Shadow blur.
        assertEquals(0.55f, SplashDefaults.TitleShadowAlpha) // Shadow alpha.
        assertEquals("splash_icon", SplashDefaults.IconTestTag) // UI test tag.
    }
}
