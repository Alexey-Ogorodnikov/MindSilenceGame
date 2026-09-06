package com.mindsilence.game.presentation.highscores // Highscore MVVM: named onBack, UseCase load.

import app.cash.turbine.test // Collect NavigateBack.
import com.mindsilence.game.MainDispatcherRule // Virtual Main for Channel.send.
import com.mindsilence.game.domain.model.DailyStats // Expected row.
import com.mindsilence.game.domain.repository.InMemoryGameProgressRepository // Fake store.
import com.mindsilence.game.domain.usecase.GetDailyStatsUseCase // Injected into the VM in tests.
import kotlinx.coroutines.ExperimentalCoroutinesApi // runCurrent.
import kotlinx.coroutines.test.runCurrent // Flush launches.
import kotlinx.coroutines.test.runTest // Coroutine test scope.
import org.junit.Assert.assertEquals // Value equality.
import org.junit.Rule // MainDispatcherRule.
import org.junit.Test // JUnit 4.
import java.time.LocalDate // Today key.

/** HighScoresViewModel: loads daily stats in init and emits Back as an effect. */
@OptIn(ExperimentalCoroutinesApi::class) // runCurrent.
class HighScoresViewModelTest { // Constructs the VM with a UseCase; no Hilt.

    @get:Rule
    val mainDispatcherRule = MainDispatcherRule() // viewModelScope uses TestDispatcher.

    /** init copies getDailyStats() into UiState. */
    @Test
    fun `init loads daily stats from repository`() { // Load.
        val repository = InMemoryGameProgressRepository() // Empty fake.
        repository.recordSession(levelReached = 3, totalSeconds = 10) // First attempt.
        repository.recordSession(levelReached = 2, totalSeconds = 4) // Merge today.

        val viewModel = HighScoresViewModel(GetDailyStatsUseCase(repository)) // Same constructor Hilt uses.
        val state = viewModel.state.value // Snapshot after init.

        assertEquals(1, state.dailyStats.size) // One day.
        assertEquals( // Merged row.
            DailyStats(
                date = LocalDate.now(), // Today.
                attempts = 2, // Two Thoughts.
                totalSeconds = 14, // 10 + 4.
                bestLevel = 3, // max(3, 2).
            ),
            state.dailyStats.first(),
        )
    }

    /** Top-bar Back emits NavigateBack. */
    @Test
    fun `back emits navigate back effect`() = runTest { // Effect.
        val viewModel = HighScoresViewModel(GetDailyStatsUseCase(InMemoryGameProgressRepository())) // Empty list.

        viewModel.effects.test { // Collect.
            viewModel.onBack() // Named method.
            runCurrent()

            assertEquals(HighScoresUiEffect.NavigateBack, awaitItem()) // Parent leaves highscores.
            cancelAndIgnoreRemainingEvents() // Done.
        }
    }
}
