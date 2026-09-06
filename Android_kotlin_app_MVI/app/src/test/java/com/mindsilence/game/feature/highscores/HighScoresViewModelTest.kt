package com.mindsilence.game.feature.highscores

import app.cash.turbine.test
import com.mindsilence.game.feature.game.DailyStats
import com.mindsilence.game.feature.game.InMemoryGameProgressRepository
import com.mindsilence.game.feature.game.MainDispatcherRule
import kotlinx.coroutines.ExperimentalCoroutinesApi
import kotlinx.coroutines.test.runCurrent
import kotlinx.coroutines.test.runTest
import org.junit.Assert.assertEquals
import org.junit.Rule
import org.junit.Test
import java.time.LocalDate

@OptIn(ExperimentalCoroutinesApi::class)
class HighScoresViewModelTest {

    @get:Rule
    val mainDispatcherRule = MainDispatcherRule()

    @Test
    fun `init loads daily stats from repository`() {
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

    @Test
    fun `back emits navigate back effect`() = runTest {
        val viewModel = HighScoresViewModel(InMemoryGameProgressRepository())

        viewModel.effects.test {
            viewModel.onEvent(HighScoresUiEvent.Back)
            runCurrent()

            assertEquals(HighScoresUiEffect.NavigateBack, awaitItem())
            cancelAndIgnoreRemainingEvents()
        }
    }
}
