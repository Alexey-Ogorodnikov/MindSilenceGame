package com.mindsilence.game.presentation.game // Session logic: timer, Thought, persist, effects.

import androidx.lifecycle.ViewModel // Training state survives config change.
import androidx.lifecycle.viewModelScope // Tick loop and Channel.send.
import com.mindsilence.game.domain.model.GamePhase // Idle vs Running.
import com.mindsilence.game.domain.model.SessionSummary // Dialog payload after Thought.
import com.mindsilence.game.domain.usecase.RecordSessionUseCase // Persist today; returns today's best.
import com.mindsilence.game.domain.usecase.totalSessionSeconds // Sum of completed levels + elapsed.
import dagger.hilt.android.lifecycle.HiltViewModel // Hilt constructs this when GameRoute calls hiltViewModel().
import javax.inject.Inject // Constructor injection of the record-session use case.
import kotlinx.coroutines.Job // Cancel the 1s tick when Thought or backgrounded.
import kotlinx.coroutines.channels.Channel // Haptic, keep-screen-on, navigation — not UiState.
import kotlinx.coroutines.delay // One-second tick, not Handler.postDelayed.
import kotlinx.coroutines.flow.Flow // Effects collected by GameRoute.
import kotlinx.coroutines.flow.MutableStateFlow // Private GameUiState.
import kotlinx.coroutines.flow.StateFlow // Screen reads phase/level/summary.
import kotlinx.coroutines.flow.asStateFlow // Hide mutation from Compose.
import kotlinx.coroutines.flow.receiveAsFlow // GameRoute collects once.
import kotlinx.coroutines.flow.update // Only allowed state mutation.
import kotlinx.coroutines.isActive // Stop the tick loop when the job is cancelled.
import kotlinx.coroutines.launch // Tick, haptic, keep-screen-on, navigate.

/**
 * Training session logic: Idle/Running, level timer, Thought end, progress write,
 * and effects (haptic, keep-screen-on, navigate). Composables do not tick or persist.
 * MVVM: named methods instead of a sealed UiEvent dispatcher.
 */
@HiltViewModel // Scoped to the ViewModelStoreOwner that GameRoute uses (Activity without NavHost).
class GameViewModel @Inject constructor( // All session rules live here, not in GameScreen.
    private val recordSession: RecordSessionUseCase, // Domain port; tests pass a fake repository through this.
) : ViewModel() { // Destroyed when the store is cleared (highscores replaces GameRoute in practice via Idle reset).

    private val _state = MutableStateFlow(GameUiState()) // Idle, no summary.
    val state: StateFlow<GameUiState> = _state.asStateFlow() // GameScreen renders this snapshot.

    private val _effects = Channel<GameUiEffect>(Channel.BUFFERED) // Buffer so ticks and clicks are not dropped.
    val effects: Flow<GameUiEffect> = _effects.receiveAsFlow() // GameRoute applies window flags and nav.

    private var tickJob: Job? = null // Null when not ticking.
    private var wasRunningBeforeBackground = false // Resume tick only if we left while Running.

    /** Idle → Running at level 1. Ignored while already Running. */
    fun onStart() { // Start is ignored while already Running.
        if (_state.value.phase == GamePhase.Running) return // Second Start does nothing.

        _state.updateToRunning(level = 1, elapsedSecAtLevel = 0) // Fresh attempt always starts at level 1.
        emitKeepScreenOn(enabled = true) // Keep the screen awake while the timer runs.
        startTicking() // 1s loop.
    }

    /** End the attempt; ignored if Idle. */
    fun onThought() { // Thought in Idle is ignored.
        if (_state.value.phase != GamePhase.Running) return // No session to end.

        val levelReached = _state.value.level // Current level at Thought.
        val elapsedSecAtLevel = _state.value.elapsedSecAtLevel // Partial time on this level.
        val totalSeconds = totalSessionSeconds(levelReached, elapsedSecAtLevel) // Sum of completed levels + elapsed.
        val bestToday = recordSession(levelReached, totalSeconds) // Persist today; returns today's best.

        stopTicking() // No more 1s updates.
        _state.update { // Idle + summary dialog; ring resets to empty.
            it.copy( // One snapshot for the end of the attempt.
                phase = GamePhase.Idle, // Waiting to Start again.
                level = 0, // Idle ring has no level.
                elapsedSecAtLevel = 0, // Clear the bar.
                sessionSummary = SessionSummary( // Dialog payload.
                    levelReached = levelReached, // This attempt.
                    bestToday = bestToday, // After recording.
                    totalSeconds = totalSeconds, // Silence length.
                ),
            )
        }
        emitKeepScreenOn(enabled = false) // Session is over; allow sleep.
        viewModelScope.launch { // Effect cannot live in state.
            _effects.send(GameUiEffect.HapticOnThought) // GameRoute performs LongPress haptic.
        }
    }

    /** Close the session-complete dialog without navigating. */
    fun onDismissSessionSummary() { // Close dialog without navigating.
        _state.update { it.copy(sessionSummary = null) } // Stay on Idle training.
    }

    /** Close the summary then navigate to highscores. */
    fun onOpenHighScores() { // Dialog Highscore button.
        _state.update { it.copy(sessionSummary = null) } // Clear before leaving so a new GameRoute starts clean.
        viewModelScope.launch { // Navigate after closing the summary.
            _effects.send(GameUiEffect.NavigateToHighScores) // AppRoute sets showHighScores.
        }
    }

    /** System Back on training: leave for the menu. */
    fun onLeaveTraining() { // System Back on training.
        viewModelScope.launch { // Parent clears inTraining.
            _effects.send(GameUiEffect.NavigateBackToMenu) // AppViewModel.leaveTraining().
        }
    }

    /** ON_STOP: pause tick, keep phase Running. */
    fun onAppBackgrounded() { // ON_STOP from GameRoute.
        if (_state.value.phase != GamePhase.Running) return // Idle: nothing to pause.

        wasRunningBeforeBackground = true // Remember to resume on ON_START.
        stopTicking() // Do not count time in the background.
        emitKeepScreenOn(enabled = false) // Drop FLAG_KEEP_SCREEN_ON while not visible.
    }

    /** ON_START: resume tick if we left while Running. */
    fun onAppForegrounded() { // ON_START from GameRoute.
        if (!wasRunningBeforeBackground || _state.value.phase != GamePhase.Running) { // Only resume a paused Running session.
            wasRunningBeforeBackground = false // Clear stale flag (e.g. Idle).
            return // Do not start a tick from Idle.
        }

        wasRunningBeforeBackground = false // Consume the flag.
        emitKeepScreenOn(enabled = true) // Screen on only while ticking in the foreground.
        startTicking() // Continue the same Running phase (no Pause UI).
    }

    private fun startTicking() { // One job at a time.
        if (tickJob?.isActive == true) return // Already ticking.

        tickJob = viewModelScope.launch { // Cancelled in stopTicking / onCleared.
            while (isActive) { // Until cancel.
                delay(TICK_INTERVAL_MS) // 1000 ms per handbook.
                advanceTick() // elapsed++ or level++.
            }
        }
    }

    private fun stopTicking() { // Thought, background, or VM cleared.
        tickJob?.cancel() // Stop the delay loop.
        tickJob = null // Allow startTicking to create a new job.
    }

    private fun advanceTick() { // One second of silence on the current level.
        _state.update { current -> // No-op if we are no longer Running.
            if (current.phase != GamePhase.Running) return@update current // Stale tick after Thought.

            val nextElapsed = current.elapsedSecAtLevel + 1 // Count this second.
            val required = current.requiredSecAtLevel // 4, 8, 16, … for this level.

            if (nextElapsed >= required) { // Level complete; no LevelComplete UI.
                current.copy( // Next level starts immediately.
                    level = current.level + 1, // Unbounded.
                    elapsedSecAtLevel = 0, // Fresh bar.
                )
            } else { // Still on this level.
                current.copy(elapsedSecAtLevel = nextElapsed) // Advance the bar.
            }
        }
    }

    private fun emitKeepScreenOn(enabled: Boolean) { // GameRoute sets/clears the window flag.
        viewModelScope.launch { // Channel send.
            _effects.send(GameUiEffect.KeepScreenOn(enabled)) // True only while foreground tick.
        }
    }

    private fun MutableStateFlow<GameUiState>.updateToRunning( // Shared Idle → Running write.
        level: Int, // Always 1 from Start.
        elapsedSecAtLevel: Int, // Always 0 from Start.
    ) { // Start helper body.
        update { // One snapshot.
            it.copy( // Keep sessionSummary null for a fresh run.
                phase = GamePhase.Running, // Timer is live.
                level = level, // Current level number.
                elapsedSecAtLevel = elapsedSecAtLevel, // Seconds into this level.
            )
        }
    }

    override fun onCleared() { // Highscores replaces GameRoute; stop the tick.
        stopTicking() // Do not leak the loop.
        super.onCleared() // ViewModel cleanup.
    }

    private companion object { // Tick constant.
        const val TICK_INTERVAL_MS = 1_000L // One second per handbook.
    }
}
