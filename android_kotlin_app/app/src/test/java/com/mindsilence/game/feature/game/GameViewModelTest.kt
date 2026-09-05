package com.mindsilence.game.feature.game

import app.cash.turbine.test
import androidx.lifecycle.ViewModelProvider
import androidx.lifecycle.ViewModelStore
import kotlinx.coroutines.ExperimentalCoroutinesApi
import kotlinx.coroutines.test.advanceTimeBy
import kotlinx.coroutines.test.runCurrent
import kotlinx.coroutines.test.runTest
import org.junit.Assert.assertEquals
import org.junit.Assert.assertTrue
import org.junit.Rule
import org.junit.Test

@OptIn(ExperimentalCoroutinesApi::class)
class GameViewModelTest {

    @get:Rule
    val mainDispatcherRule = MainDispatcherRule()

    @Test
    fun `start transitions to running level one`() = runTest {
        val viewModel = GameViewModel()

        viewModel.onEvent(GameUiEvent.Start)
        runCurrent()

        val state = viewModel.state.value
        assertEquals(GamePhase.Running, state.phase)
        assertEquals(1, state.level)
        assertEquals(0, state.elapsedSecAtLevel)
        assertEquals(4, state.requiredSecAtLevel)

        viewModel.onEvent(GameUiEvent.Thought)
        runCurrent()
    }

    @Test
    fun `four seconds of silence advances to level two`() = runTest {
        val viewModel = GameViewModel()
        viewModel.onEvent(GameUiEvent.Start)
        runCurrent()

        advanceTimeBy(4_000)
        runCurrent()

        assertEquals(2, viewModel.state.value.level)
        assertEquals(0, viewModel.state.value.elapsedSecAtLevel)

        viewModel.onEvent(GameUiEvent.Thought)
        runCurrent()
    }

    @Test
    fun `eight seconds on level two advances to level three`() = runTest {
        val viewModel = GameViewModel()
        viewModel.onEvent(GameUiEvent.Start)
        runCurrent()

        advanceTimeBy(4_000)
        runCurrent()
        advanceTimeBy(8_000)
        runCurrent()

        assertEquals(3, viewModel.state.value.level)

        viewModel.onEvent(GameUiEvent.Thought)
        runCurrent()
    }

    @Test
    fun `thought resets to idle and shows session summary`() = runTest {
        val viewModel = GameViewModel()
        viewModel.onEvent(GameUiEvent.Start)
        runCurrent()
        advanceTimeBy(500)
        runCurrent()

        viewModel.onEvent(GameUiEvent.Thought)
        runCurrent()

        val state = viewModel.state.value
        assertEquals(GamePhase.Idle, state.phase)
        assertEquals(0, state.level)
        assertEquals(0, state.elapsedSecAtLevel)
        assertEquals(SessionSummary(levelReached = 1, bestToday = 1, totalSeconds = 0), state.sessionSummary)
    }

    @Test
    fun `thought keeps best today across sessions`() = runTest {
        val repository = InMemoryGameProgressRepository()
        val viewModel = GameViewModel(repository)

        viewModel.onEvent(GameUiEvent.Start)
        runCurrent()
        advanceTimeBy(4_000)
        runCurrent()
        viewModel.onEvent(GameUiEvent.Thought)
        runCurrent()
        viewModel.onEvent(GameUiEvent.DismissSessionSummary)
        runCurrent()

        viewModel.onEvent(GameUiEvent.Start)
        runCurrent()
        viewModel.onEvent(GameUiEvent.Thought)
        runCurrent()

        assertEquals(SessionSummary(levelReached = 1, bestToday = 2, totalSeconds = 0), viewModel.state.value.sessionSummary)
    }

    @Test
    fun `dismiss session summary clears dialog state`() = runTest {
        val viewModel = GameViewModel()
        viewModel.onEvent(GameUiEvent.Start)
        runCurrent()
        viewModel.onEvent(GameUiEvent.Thought)
        runCurrent()

        viewModel.onEvent(GameUiEvent.DismissSessionSummary)
        runCurrent()

        assertEquals(null, viewModel.state.value.sessionSummary)
    }

    @Test
    fun `open high scores dismisses summary and navigates`() = runTest {
        val viewModel = GameViewModel()
        viewModel.onEvent(GameUiEvent.Start)
        runCurrent()
        viewModel.onEvent(GameUiEvent.Thought)
        runCurrent()

        viewModel.effects.test {
            skipItems(3)

            viewModel.onEvent(GameUiEvent.OpenHighScores)
            runCurrent()

            assertEquals(null, viewModel.state.value.sessionSummary)
            assertEquals(GameUiEffect.NavigateToHighScores, awaitItem())
            cancelAndIgnoreRemainingEvents()
        }
    }

    @Test
    fun `leave training emits navigate back to menu`() = runTest {
        val viewModel = GameViewModel()

        viewModel.effects.test {
            viewModel.onEvent(GameUiEvent.LeaveTraining)
            runCurrent()

            assertEquals(GameUiEffect.NavigateBackToMenu, awaitItem())
            cancelAndIgnoreRemainingEvents()
        }
    }

    @Test
    fun `thought in idle is no-op`() = runTest {
        val viewModel = GameViewModel()

        viewModel.onEvent(GameUiEvent.Thought)
        runCurrent()

        assertEquals(GamePhase.Idle, viewModel.state.value.phase)
        assertEquals(0, viewModel.state.value.level)
    }

    @Test
    fun `thought emits haptic effect`() = runTest {
        val viewModel = GameViewModel()
        viewModel.onEvent(GameUiEvent.Start)
        runCurrent()

        viewModel.effects.test {
            skipItems(1) // KeepScreenOn from Start

            viewModel.onEvent(GameUiEvent.Thought)
            runCurrent()

            val effects = mutableListOf<GameUiEffect>()
            repeat(2) {
                effects.add(awaitItem())
            }
            assertTrue(effects.any { it is GameUiEffect.HapticOnThought })
            cancelAndIgnoreRemainingEvents()
        }
    }

    @Test
    fun `totalSessionSeconds sums completed levels and current progress`() {
        assertEquals(0, totalSessionSeconds(level = 0, elapsedSecAtLevel = 5))
        assertEquals(0, totalSessionSeconds(level = 1, elapsedSecAtLevel = 0))
        assertEquals(4, totalSessionSeconds(level = 2, elapsedSecAtLevel = 0))
        assertEquals(12, totalSessionSeconds(level = 3, elapsedSecAtLevel = 0))
        assertEquals(13, totalSessionSeconds(level = 3, elapsedSecAtLevel = 1))
    }

    @Test
    fun `thought includes total session time in summary`() = runTest {
        val viewModel = GameViewModel()
        viewModel.onEvent(GameUiEvent.Start)
        runCurrent()

        advanceTimeBy(4_000)
        runCurrent()
        advanceTimeBy(8_000)
        runCurrent()

        viewModel.onEvent(GameUiEvent.Thought)
        runCurrent()

        assertEquals(
            SessionSummary(levelReached = 3, bestToday = 3, totalSeconds = 12),
            viewModel.state.value.sessionSummary,
        )
    }

    @Test
    fun `durationForLevel returns geometric progression`() {
        assertEquals(0, durationForLevel(0))
        assertEquals(0, durationForLevel(-1))
        assertEquals(4, durationForLevel(1))
        assertEquals(8, durationForLevel(2))
        assertEquals(16, durationForLevel(3))
        assertEquals(32, durationForLevel(4))
        assertEquals(64, durationForLevel(5))
    }

    @Test
    fun `start while running is no-op`() = runTest {
        val viewModel = GameViewModel()
        viewModel.onEvent(GameUiEvent.Start)
        runCurrent()
        advanceTimeBy(2_000)
        runCurrent()

        val elapsed = viewModel.state.value.elapsedSecAtLevel
        viewModel.onEvent(GameUiEvent.Start)
        runCurrent()

        assertEquals(GamePhase.Running, viewModel.state.value.phase)
        assertEquals(1, viewModel.state.value.level)
        assertEquals(elapsed, viewModel.state.value.elapsedSecAtLevel)

        viewModel.onEvent(GameUiEvent.Thought)
        runCurrent()
    }

    @Test
    fun `start emits keep screen on`() = runTest {
        val viewModel = GameViewModel()

        viewModel.effects.test {
            viewModel.onEvent(GameUiEvent.Start)
            runCurrent()

            assertEquals(GameUiEffect.KeepScreenOn(enabled = true), awaitItem())
            cancelAndIgnoreRemainingEvents()
        }

        viewModel.onEvent(GameUiEvent.Thought)
        runCurrent()
    }

    @Test
    fun `background while idle is no-op`() = runTest {
        val viewModel = GameViewModel()
        val before = viewModel.state.value

        viewModel.onEvent(GameUiEvent.AppBackgrounded)
        runCurrent()

        assertEquals(before, viewModel.state.value)
    }

    @Test
    fun `background while running pauses tick and keeps phase`() = runTest {
        val viewModel = GameViewModel()
        viewModel.onEvent(GameUiEvent.Start)
        runCurrent()

        viewModel.effects.test {
            skipItems(1)

            viewModel.onEvent(GameUiEvent.AppBackgrounded)
            runCurrent()

            assertEquals(GameUiEffect.KeepScreenOn(enabled = false), awaitItem())
            assertEquals(GamePhase.Running, viewModel.state.value.phase)
            val elapsed = viewModel.state.value.elapsedSecAtLevel
            advanceTimeBy(4_000)
            runCurrent()
            assertEquals(elapsed, viewModel.state.value.elapsedSecAtLevel)
            cancelAndIgnoreRemainingEvents()
        }

        viewModel.onEvent(GameUiEvent.Thought)
        runCurrent()
    }

    @Test
    fun `foreground resumes tick only after running background`() = runTest {
        val viewModel = GameViewModel()
        viewModel.onEvent(GameUiEvent.Start)
        runCurrent()

        viewModel.effects.test {
            skipItems(1)
            viewModel.onEvent(GameUiEvent.AppBackgrounded)
            runCurrent()
            skipItems(1)

            viewModel.onEvent(GameUiEvent.AppForegrounded)
            runCurrent()

            assertEquals(GameUiEffect.KeepScreenOn(enabled = true), awaitItem())
            advanceTimeBy(4_000)
            runCurrent()
            assertEquals(2, viewModel.state.value.level)
            cancelAndIgnoreRemainingEvents()
        }

        viewModel.onEvent(GameUiEvent.Thought)
        runCurrent()
    }

    @Test
    fun `foreground without background does not reset running session`() = runTest {
        val viewModel = GameViewModel()
        viewModel.onEvent(GameUiEvent.Start)
        runCurrent()

        viewModel.onEvent(GameUiEvent.AppForegrounded)
        runCurrent()
        advanceTimeBy(4_000)
        runCurrent()

        assertEquals(GamePhase.Running, viewModel.state.value.phase)
        assertEquals(2, viewModel.state.value.level)

        viewModel.onEvent(GameUiEvent.Thought)
        runCurrent()
    }

    @Test
    fun `idle progress fraction is zero`() {
        val idle = GameUiState()
        assertEquals(0, idle.requiredSecAtLevel)
        assertEquals(0f, idle.progressFraction, 0.0001f)
    }

    @Test
    fun `running progress fraction is elapsed over required`() {
        val running = GameUiState(
            phase = GamePhase.Running,
            level = 1,
            elapsedSecAtLevel = 2,
        )
        assertEquals(4, running.requiredSecAtLevel)
        assertEquals(0.5f, running.progressFraction, 0.0001f)
    }

    @Test
    fun `progress fraction is coerced to one`() {
        val overflowing = GameUiState(
            phase = GamePhase.Running,
            level = 1,
            elapsedSecAtLevel = 100,
        )
        assertEquals(1f, overflowing.progressFraction, 0.0001f)
    }

    @Test
    fun `onCleared stops ticking`() = runTest {
        val store = ViewModelStore()
        val viewModel = ViewModelProvider(
            store,
            GameViewModelFactory(InMemoryGameProgressRepository()),
        )[GameViewModel::class.java]

        viewModel.onEvent(GameUiEvent.Start)
        runCurrent()
        store.clear()

        val elapsed = viewModel.state.value.elapsedSecAtLevel
        advanceTimeBy(4_000)
        runCurrent()
        assertEquals(elapsed, viewModel.state.value.elapsedSecAtLevel)
    }
}
