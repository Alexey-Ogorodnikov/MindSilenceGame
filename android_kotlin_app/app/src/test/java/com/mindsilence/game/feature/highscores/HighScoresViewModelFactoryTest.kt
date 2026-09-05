package com.mindsilence.game.feature.highscores

import com.mindsilence.game.feature.game.GameViewModel
import com.mindsilence.game.feature.game.InMemoryGameProgressRepository
import org.junit.Assert.assertEquals
import org.junit.Assert.assertTrue
import org.junit.Test

class HighScoresViewModelFactoryTest {

    @Test
    fun `create loads stats from repository`() {
        val repository = InMemoryGameProgressRepository()
        repository.recordSession(levelReached = 4, totalSeconds = 9)

        val viewModel = HighScoresViewModelFactory(repository)
            .create(HighScoresViewModel::class.java)

        assertEquals(1, viewModel.state.value.dailyStats.size)
        assertEquals(4, viewModel.state.value.dailyStats.first().bestLevel)
    }

    @Test
    fun `create rejects unknown class`() {
        val factory = HighScoresViewModelFactory(InMemoryGameProgressRepository())
        val error = runCatching { factory.create(GameViewModel::class.java) }.exceptionOrNull()
        assertTrue(error is IllegalArgumentException)
        assertEquals(
            "Unknown ViewModel class: ${GameViewModel::class.java.name}",
            error?.message,
        )
    }
}
