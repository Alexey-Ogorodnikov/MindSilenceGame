package com.mindsilence.game.feature.splash

import com.mindsilence.game.feature.game.GameViewModel
import org.junit.Assert.assertEquals
import org.junit.Assert.assertFalse
import org.junit.Assert.assertTrue
import org.junit.Test

class SplashViewModelFactoryTest {

    @Test
    fun `create restores without branded splash`() {
        val viewModel = SplashViewModelFactory(startWithBrandedSplash = false)
            .create(SplashViewModel::class.java)

        assertFalse(viewModel.state.value.showBrandedSplash)
        assertFalse(viewModel.state.value.keepSystemSplash)
    }

    @Test
    fun `create rejects unknown class`() {
        val factory = SplashViewModelFactory(startWithBrandedSplash = true)
        val error = runCatching { factory.create(GameViewModel::class.java) }.exceptionOrNull()
        assertTrue(error is IllegalArgumentException)
        assertEquals(
            "Unknown ViewModel class: ${GameViewModel::class.java.name}",
            error?.message,
        )
    }
}
