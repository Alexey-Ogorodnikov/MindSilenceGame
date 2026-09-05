package com.mindsilence.game.feature.highscores

import com.mindsilence.game.feature.game.DailyStats
import com.mindsilence.game.feature.game.InMemoryGameProgressRepository
import org.junit.Assert.assertEquals
import org.junit.Test
import java.time.LocalDate

class HighScoresViewModelTest {

    @Test
    fun `refresh loads daily stats from repository`() {
        val repository = InMemoryGameProgressRepository()
        repository.recordSession(levelReached = 3, totalSeconds = 10)
        repository.recordSession(levelReached = 2, totalSeconds = 4)

        val viewModel = HighScoresViewModel(repository)
        val state = viewModel.state.value

        assertEquals(1, state.dailyStats.size)
        assertEquals(
            DailyStats(
                date = LocalDate.now(),
                attempts = 2,
                totalSeconds = 14,
                bestLevel = 3,
            ),
            state.dailyStats.first(),
        )
    }
}
