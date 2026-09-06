package com.mindsilence.game.feature.splash

import org.junit.Assert.assertEquals
import org.junit.Test

class SplashDefaultsTest {

    @Test
    fun `icon size matches system splash without icon background`() {
        assertEquals(288f, SplashDefaults.IconSize.value)
    }

    @Test
    fun `duration matches branded splash handoff`() {
        assertEquals(2_000L, SplashDefaults.DurationMs)
    }

    @Test
    fun `title metrics match branded splash wordmark`() {
        assertEquals(24f, SplashDefaults.TitleSpacing.value)
        assertEquals(32f, SplashDefaults.TitleFontSize.value)
        assertEquals(1f, SplashDefaults.TitleLetterSpacing.value)
        assertEquals(10f, SplashDefaults.TitleShadowBlurPx)
        assertEquals(0.55f, SplashDefaults.TitleShadowAlpha)
        assertEquals("splash_icon", SplashDefaults.IconTestTag)
    }
}
