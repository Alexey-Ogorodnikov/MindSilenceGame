package com.mindsilence.game.feature.game

import com.mindsilence.game.feature.splash.SplashViewModel
import org.junit.Assert.assertEquals
import org.junit.Assert.assertTrue
import org.junit.Test

class GameViewModelFactoryTest {

    @Test
    fun `create returns game view model`() {
        val viewModel = GameViewModelFactory(InMemoryGameProgressRepository())
            .create(GameViewModel::class.java)

        assertEquals(GamePhase.Idle, viewModel.state.value.phase)
    }

    @Test
    fun `create rejects unknown class`() {
        val factory = GameViewModelFactory(InMemoryGameProgressRepository())
        val error = runCatching { factory.create(SplashViewModel::class.java) }.exceptionOrNull()
        assertTrue(error is IllegalArgumentException)
        assertEquals(
            "Unknown ViewModel class: ${SplashViewModel::class.java.name}",
            error?.message,
        )
    }
}
