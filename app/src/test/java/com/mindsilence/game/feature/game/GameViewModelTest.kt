package com.mindsilence.game.feature.game

import app.cash.turbine.test
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
        assertEquals(1, state.requiredSecAtLevel)

        viewModel.onEvent(GameUiEvent.Thought)
        runCurrent()
    }

    @Test
    fun `one second of silence advances to level two`() = runTest {
        val viewModel = GameViewModel()
        viewModel.onEvent(GameUiEvent.Start)
        runCurrent()

        advanceTimeBy(1_000)
        runCurrent()

        assertEquals(2, viewModel.state.value.level)
        assertEquals(0, viewModel.state.value.elapsedSecAtLevel)

        viewModel.onEvent(GameUiEvent.Thought)
        runCurrent()
    }

    @Test
    fun `two seconds on level two advances to level three`() = runTest {
        val viewModel = GameViewModel()
        viewModel.onEvent(GameUiEvent.Start)
        runCurrent()

        advanceTimeBy(1_000)
        runCurrent()
        advanceTimeBy(2_000)
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
        advanceTimeBy(1_000)
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
        assertEquals(0, totalSessionSeconds(level = 1, elapsedSecAtLevel = 0))
        assertEquals(1, totalSessionSeconds(level = 2, elapsedSecAtLevel = 0))
        assertEquals(3, totalSessionSeconds(level = 3, elapsedSecAtLevel = 0))
        assertEquals(4, totalSessionSeconds(level = 3, elapsedSecAtLevel = 1))
    }

    @Test
    fun `thought includes total session time in summary`() = runTest {
        val viewModel = GameViewModel()
        viewModel.onEvent(GameUiEvent.Start)
        runCurrent()

        advanceTimeBy(1_000)
        runCurrent()
        advanceTimeBy(2_000)
        runCurrent()

        viewModel.onEvent(GameUiEvent.Thought)
        runCurrent()

        assertEquals(
            SessionSummary(levelReached = 3, bestToday = 3, totalSeconds = 3),
            viewModel.state.value.sessionSummary,
        )
    }

    @Test
    fun `durationForLevel returns geometric progression`() {
        assertEquals(0, durationForLevel(0))
        assertEquals(1, durationForLevel(1))
        assertEquals(2, durationForLevel(2))
        assertEquals(4, durationForLevel(3))
        assertEquals(8, durationForLevel(4))
        assertEquals(16, durationForLevel(5))
    }
}
