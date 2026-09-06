package com.mindsilence.game.presentation.game // Session MVVM: named methods, not onEvent.

import androidx.lifecycle.ViewModel // Type for the onCleared factory.
import androidx.lifecycle.ViewModelProvider // Puts GameViewModel in a store we can clear.
import androidx.lifecycle.ViewModelStore // store.clear() calls onCleared.
import app.cash.turbine.test // Collect effects.
import com.mindsilence.game.MainDispatcherRule // Virtual Main for delay().
import com.mindsilence.game.domain.model.GamePhase // Idle vs Running.
import com.mindsilence.game.domain.model.SessionSummary // Dialog payload.
import com.mindsilence.game.domain.repository.InMemoryGameProgressRepository // Fake store.
import com.mindsilence.game.domain.usecase.RecordSessionUseCase // Injected into the VM in tests.
import com.mindsilence.game.domain.usecase.durationForLevel // Pure duration helper.
import com.mindsilence.game.domain.usecase.totalSessionSeconds // Pure attempt-length helper.
import kotlinx.coroutines.ExperimentalCoroutinesApi // advanceTimeBy.
import kotlinx.coroutines.test.advanceTimeBy // Virtual seconds.
import kotlinx.coroutines.test.runCurrent // Flush launched jobs.
import kotlinx.coroutines.test.runTest // Coroutine test scope.
import org.junit.Assert.assertEquals // Value equality.
import org.junit.Assert.assertTrue // Effect contains haptic.
import org.junit.Rule // MainDispatcherRule.
import org.junit.Test // JUnit 4.

/** Training ViewModel: Start/Thought, level timer, background, keep-screen-on, navigation effects. */
@OptIn(ExperimentalCoroutinesApi::class) // Virtual time.
class GameViewModelTest { // Same scenarios as the MVI tests, calling named methods.

    @get:Rule
    val mainDispatcherRule = MainDispatcherRule() // viewModelScope uses TestDispatcher.

    /** Start from Idle goes to Running level 1 with a 4s requirement. */
    @Test
    fun `start transitions to running level one`() = runTest { // Idle → Running.
        val viewModel = gameViewModel() // In-memory store.

        viewModel.onStart() // Named MVVM method.
        runCurrent() // Flush KeepScreenOn launch.

        val state = viewModel.state.value // Snapshot.
        assertEquals(GamePhase.Running, state.phase) // Session live.
        assertEquals(1, state.level) // Always starts at 1.
        assertEquals(0, state.elapsedSecAtLevel) // Fresh bar.
        assertEquals(4, state.requiredSecAtLevel) // durationForLevel(1).

        viewModel.onThought() // Stop the tick so the test does not leak.
        runCurrent()
    }

    /** Four seconds of silence complete level 1 and open level 2. */
    @Test
    fun `four seconds of silence advances to level two`() = runTest { // Level-up.
        val viewModel = gameViewModel() // In-memory store.
        viewModel.onStart() // Running.
        runCurrent()

        advanceTimeBy(4_000) // One level duration.
        runCurrent()

        assertEquals(2, viewModel.state.value.level) // Next level.
        assertEquals(0, viewModel.state.value.elapsedSecAtLevel) // Fresh bar.

        viewModel.onThought() // Cleanup.
        runCurrent()
    }

    /** Level 2 lasts 8s; after that the session is on level 3. */
    @Test
    fun `eight seconds on level two advances to level three`() = runTest { // Two level-ups.
        val viewModel = gameViewModel() // In-memory store.
        viewModel.onStart() // Level 1.
        runCurrent()

        advanceTimeBy(4_000) // Finish level 1.
        runCurrent()
        advanceTimeBy(8_000) // Finish level 2.
        runCurrent()

        assertEquals(3, viewModel.state.value.level) // Unbounded.

        viewModel.onThought() // Cleanup.
        runCurrent()
    }

    /** Thought returns Idle, clears the ring, and shows a summary. */
    @Test
    fun `thought resets to idle and shows session summary`() = runTest { // End attempt.
        val viewModel = gameViewModel() // In-memory store.
        viewModel.onStart() // Running.
        runCurrent()
        advanceTimeBy(500) // Less than 1s; elapsed stays 0.
        runCurrent()

        viewModel.onThought() // End.
        runCurrent()

        val state = viewModel.state.value // Snapshot.
        assertEquals(GamePhase.Idle, state.phase) // Waiting to Start.
        assertEquals(0, state.level) // Idle ring.
        assertEquals(0, state.elapsedSecAtLevel) // Cleared bar.
        assertEquals(SessionSummary(levelReached = 1, bestToday = 1, totalSeconds = 0), state.sessionSummary) // Dialog.
    }

    /** A later shorter attempt still reports the higher bestToday. */
    @Test
    fun `thought keeps best today across sessions`() = runTest { // Two attempts, shared repo.
        val repository = InMemoryGameProgressRepository() // Shared fake.
        val viewModel = gameViewModel(repository) // Same store.

        viewModel.onStart() // First run.
        runCurrent()
        advanceTimeBy(4_000) // Reach level 2.
        runCurrent()
        viewModel.onThought() // Record level 2.
        runCurrent()
        viewModel.onDismissSessionSummary() // Close dialog.
        runCurrent()

        viewModel.onStart() // Second run.
        runCurrent()
        viewModel.onThought() // Record level 1 immediately.
        runCurrent()

        assertEquals(SessionSummary(levelReached = 1, bestToday = 2, totalSeconds = 0), viewModel.state.value.sessionSummary) // Best stays 2.
    }

    /** OK on the dialog only clears sessionSummary. */
    @Test
    fun `dismiss session summary clears dialog state`() = runTest { // Dismiss.
        val viewModel = gameViewModel() // In-memory store.
        viewModel.onStart() // Running.
        runCurrent()
        viewModel.onThought() // Show dialog.
        runCurrent()

        viewModel.onDismissSessionSummary() // OK.
        runCurrent()

        assertEquals(null, viewModel.state.value.sessionSummary) // Dialog gone.
    }

    /** Highscore from the dialog clears summary and emits NavigateToHighScores. */
    @Test
    fun `open high scores dismisses summary and navigates`() = runTest { // Nav effect.
        val viewModel = gameViewModel() // In-memory store.
        viewModel.onStart() // Running.
        runCurrent()
        viewModel.onThought() // KeepScreenOn false + haptic already queued.
        runCurrent()

        viewModel.effects.test { // Collect remaining effects.
            skipItems(3) // Start keep-on, Thought keep-off, haptic.

            viewModel.onOpenHighScores() // Highscore button.
            runCurrent()

            assertEquals(null, viewModel.state.value.sessionSummary) // Cleared first.
            assertEquals(GameUiEffect.NavigateToHighScores, awaitItem()) // Then navigate.
            cancelAndIgnoreRemainingEvents() // Done.
        }
    }

    /** System Back emits NavigateBackToMenu. */
    @Test
    fun `leave training emits navigate back to menu`() = runTest { // Back.
        val viewModel = gameViewModel() // Idle.

        viewModel.effects.test { // Collect.
            viewModel.onLeaveTraining() // BackHandler.
            runCurrent()

            assertEquals(GameUiEffect.NavigateBackToMenu, awaitItem()) // Parent leaves training.
            cancelAndIgnoreRemainingEvents() // Done.
        }
    }

    /** Thought while Idle does nothing. */
    @Test
    fun `thought in idle is no-op`() = runTest { // Illegal Thought.
        val viewModel = gameViewModel() // Idle.

        viewModel.onThought() // Ignored.
        runCurrent()

        assertEquals(GamePhase.Idle, viewModel.state.value.phase) // Unchanged.
        assertEquals(0, viewModel.state.value.level) // Unchanged.
    }

    /** Thought emits HapticOnThought. */
    @Test
    fun `thought emits haptic effect`() = runTest { // Haptic.
        val viewModel = gameViewModel() // In-memory store.
        viewModel.onStart() // KeepScreenOn queued.
        runCurrent()

        viewModel.effects.test { // Collect.
            skipItems(1) // KeepScreenOn from Start

            viewModel.onThought() // End.
            runCurrent()

            val effects = mutableListOf<GameUiEffect>() // KeepScreenOn false + haptic, order not guaranteed as a pair beyond collect order.
            repeat(2) { // Two effects after Thought.
                effects.add(awaitItem()) // Collect both.
            }
            assertTrue(effects.any { it is GameUiEffect.HapticOnThought }) // Haptic present.
            cancelAndIgnoreRemainingEvents() // Done.
        }
    }

    /** totalSessionSeconds is the sum of finished levels plus current elapsed. */
    @Test
    fun `totalSessionSeconds sums completed levels and current progress`() { // Pure function.
        assertEquals(0, totalSessionSeconds(level = 0, elapsedSecAtLevel = 5)) // No session.
        assertEquals(0, totalSessionSeconds(level = 1, elapsedSecAtLevel = 0)) // Just started.
        assertEquals(4, totalSessionSeconds(level = 2, elapsedSecAtLevel = 0)) // Finished level 1.
        assertEquals(12, totalSessionSeconds(level = 3, elapsedSecAtLevel = 0)) // 4+8.
        assertEquals(13, totalSessionSeconds(level = 3, elapsedSecAtLevel = 1)) // 4+8+1.
    }

    /** Thought at level 3 with 0 elapsed stores 12 seconds. */
    @Test
    fun `thought includes total session time in summary`() = runTest { // Duration in dialog.
        val viewModel = gameViewModel() // In-memory store.
        viewModel.onStart() // Level 1.
        runCurrent()

        advanceTimeBy(4_000) // Level 2.
        runCurrent()
        advanceTimeBy(8_000) // Level 3.
        runCurrent()

        viewModel.onThought() // Record.
        runCurrent()

        assertEquals( // 4+8+0.
            SessionSummary(levelReached = 3, bestToday = 3, totalSeconds = 12),
            viewModel.state.value.sessionSummary,
        )
    }

    /** durationForLevel doubles from 4 with no upper bound. */
    @Test
    fun `durationForLevel returns geometric progression`() { // Pure function.
        assertEquals(0, durationForLevel(0)) // Idle.
        assertEquals(0, durationForLevel(-1)) // Invalid.
        assertEquals(4, durationForLevel(1)) // Level 1.
        assertEquals(8, durationForLevel(2)) // Level 2.
        assertEquals(16, durationForLevel(3)) // Level 3.
        assertEquals(32, durationForLevel(4)) // Level 4.
        assertEquals(64, durationForLevel(5)) // Level 5.
    }

    /** A second Start while Running does not reset elapsed. */
    @Test
    fun `start while running is no-op`() = runTest { // Ignore Start.
        val viewModel = gameViewModel() // In-memory store.
        viewModel.onStart() // Running.
        runCurrent()
        advanceTimeBy(2_000) // Two ticks.
        runCurrent()

        val elapsed = viewModel.state.value.elapsedSecAtLevel // Snapshot.
        viewModel.onStart() // Ignored.
        runCurrent()

        assertEquals(GamePhase.Running, viewModel.state.value.phase) // Still running.
        assertEquals(1, viewModel.state.value.level) // Same level.
        assertEquals(elapsed, viewModel.state.value.elapsedSecAtLevel) // Not reset.

        viewModel.onThought() // Cleanup.
        runCurrent()
    }

    /** Start emits KeepScreenOn(true). */
    @Test
    fun `start emits keep screen on`() = runTest { // Window flag.
        val viewModel = gameViewModel() // In-memory store.

        viewModel.effects.test { // Collect.
            viewModel.onStart() // Running.
            runCurrent()

            assertEquals(GameUiEffect.KeepScreenOn(enabled = true), awaitItem()) // Flag on.
            cancelAndIgnoreRemainingEvents() // Ignore later ticks.
        }

        viewModel.onThought() // Cleanup.
        runCurrent()
    }

    /** Background while Idle does not change state. */
    @Test
    fun `background while idle is no-op`() = runTest { // Idle ON_STOP.
        val viewModel = gameViewModel() // Idle.
        val before = viewModel.state.value // Snapshot.

        viewModel.onAppBackgrounded() // Ignored.
        runCurrent()

        assertEquals(before, viewModel.state.value) // Unchanged.
    }

    /** Background while Running stops the tick but keeps phase Running. */
    @Test
    fun `background while running pauses tick and keeps phase`() = runTest { // ON_STOP.
        val viewModel = gameViewModel() // In-memory store.
        viewModel.onStart() // Running.
        runCurrent()

        viewModel.effects.test { // Collect.
            skipItems(1) // Start keep-on.

            viewModel.onAppBackgrounded() // Pause tick.
            runCurrent()

            assertEquals(GameUiEffect.KeepScreenOn(enabled = false), awaitItem()) // Flag off.
            assertEquals(GamePhase.Running, viewModel.state.value.phase) // No Pause UI.
            val elapsed = viewModel.state.value.elapsedSecAtLevel // Frozen.
            advanceTimeBy(4_000) // Would have leveled up if ticking.
            runCurrent()
            assertEquals(elapsed, viewModel.state.value.elapsedSecAtLevel) // Frozen.
            cancelAndIgnoreRemainingEvents() // Done.
        }

        viewModel.onThought() // Cleanup.
        runCurrent()
    }

    /** Foreground after a Running background resumes the tick. */
    @Test
    fun `foreground resumes tick only after running background`() = runTest { // ON_START.
        val viewModel = gameViewModel() // In-memory store.
        viewModel.onStart() // Running.
        runCurrent()

        viewModel.effects.test { // Collect.
            skipItems(1) // Start keep-on.
            viewModel.onAppBackgrounded() // Pause.
            runCurrent()
            skipItems(1) // Keep-off.

            viewModel.onAppForegrounded() // Resume.
            runCurrent()

            assertEquals(GameUiEffect.KeepScreenOn(enabled = true), awaitItem()) // Flag on.
            advanceTimeBy(4_000) // Level up once resumed.
            runCurrent()
            assertEquals(2, viewModel.state.value.level) // Tick ran.
            cancelAndIgnoreRemainingEvents() // Done.
        }

        viewModel.onThought() // Cleanup.
        runCurrent()
    }

    /** Foreground without a prior Running background does not reset the session. */
    @Test
    fun `foreground without background does not reset running session`() = runTest { // Spurious ON_START.
        val viewModel = gameViewModel() // In-memory store.
        viewModel.onStart() // Running.
        runCurrent()

        viewModel.onAppForegrounded() // No-op resume.
        runCurrent()
        advanceTimeBy(4_000) // Still ticking from Start.
        runCurrent()

        assertEquals(GamePhase.Running, viewModel.state.value.phase) // Still running.
        assertEquals(2, viewModel.state.value.level) // Tick continued.

        viewModel.onThought() // Cleanup.
        runCurrent()
    }

    /** Idle derived progress is 0. */
    @Test
    fun `idle progress fraction is zero`() { // Derived getters.
        val idle = GameUiState() // Defaults.
        assertEquals(0, idle.requiredSecAtLevel) // durationForLevel(0).
        assertEquals(0f, idle.progressFraction, 0.0001f) // Empty bar.
    }

    /** Running progress is elapsed / required. */
    @Test
    fun `running progress fraction is elapsed over required`() { // Half bar.
        val running = GameUiState( // Level 1, 2 of 4s.
            phase = GamePhase.Running, // Tick UI.
            level = 1, // 4s required.
            elapsedSecAtLevel = 2, // Halfway.
        )
        assertEquals(4, running.requiredSecAtLevel) // Level 1.
        assertEquals(0.5f, running.progressFraction, 0.0001f) // 2/4.
    }

    /** Overflow elapsed is coerced to 1f. */
    @Test
    fun `progress fraction is coerced to one`() { // Guard.
        val overflowing = GameUiState( // Impossible elapsed, still must not exceed 1.
            phase = GamePhase.Running, // Tick UI.
            level = 1, // 4s required.
            elapsedSecAtLevel = 100, // Past the end.
        )
        assertEquals(1f, overflowing.progressFraction, 0.0001f) // coerceIn.
    }

    /** Clearing the ViewModelStore stops the tick loop. */
    @Test
    fun `onCleared stops ticking`() = runTest { // onCleared.
        val store = ViewModelStore() // Owner we can clear.
        val viewModel = ViewModelProvider( // Put the VM in the store.
            store, // Cleared below.
            object : ViewModelProvider.Factory { // Tests do not use Hilt.
                override fun <T : ViewModel> create(modelClass: Class<T>): T { // One type.
                    @Suppress("UNCHECKED_CAST") // Only GameViewModel is requested.
                    return gameViewModel() as T // In-memory UseCase.
                }
            },
        )[GameViewModel::class.java] // Typed VM.

        viewModel.onStart() // Start tick.
        runCurrent()
        store.clear() // onCleared → stopTicking.

        val elapsed = viewModel.state.value.elapsedSecAtLevel // Frozen.
        advanceTimeBy(4_000) // Would level up if still ticking.
        runCurrent()
        assertEquals(elapsed, viewModel.state.value.elapsedSecAtLevel) // Frozen.
    }

    private fun gameViewModel( // Shared constructor without Hilt.
        repository: InMemoryGameProgressRepository = InMemoryGameProgressRepository(), // Default empty fake.
    ): GameViewModel = // Production VM with a test UseCase.
        GameViewModel(RecordSessionUseCase(repository)) // Same constructor Hilt uses.
}
