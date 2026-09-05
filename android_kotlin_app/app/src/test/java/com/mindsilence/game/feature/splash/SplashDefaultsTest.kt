package com.mindsilence.game.feature.splash

import org.junit.Assert.assertEquals
import org.junit.Test

class SplashDefaultsTest {

    @Test
    fun `icon size matches system splash without icon background`() {
        assertEquals(288f, SplashDefaults.IconSize.value)
    }
}
