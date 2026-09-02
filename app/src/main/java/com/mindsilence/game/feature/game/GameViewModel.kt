package com.mindsilence.game.feature.game

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import kotlinx.coroutines.Job
import kotlinx.coroutines.channels.Channel
import kotlinx.coroutines.delay
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.receiveAsFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.isActive
import kotlinx.coroutines.launch

class GameViewModel(
    private val progressRepository: GameProgressRepository = InMemoryGameProgressRepository(),
) : ViewModel() {

    private val _state = MutableStateFlow(GameUiState())
    val state: StateFlow<GameUiState> = _state.asStateFlow()

    private val _effects = Channel<GameUiEffect>(Channel.BUFFERED)
    val effects: Flow<GameUiEffect> = _effects.receiveAsFlow()

    private var tickJob: Job? = null
    private var wasRunningBeforeBackground = false

    fun onEvent(event: GameUiEvent) {
        when (event) {
            GameUiEvent.Start -> startSession()
            GameUiEvent.Thought -> onThought()
            GameUiEvent.DismissSessionSummary -> dismissSessionSummary()
            GameUiEvent.AppBackgrounded -> onAppBackgrounded()
            GameUiEvent.AppForegrounded -> onAppForegrounded()
        }
    }

    private fun startSession() {
        if (_state.value.phase == GamePhase.Running) return

        _state.updateToRunning(level = 1, elapsedSecAtLevel = 0)
        emitKeepScreenOn(enabled = true)
        startTicking()
    }

    private fun onThought() {
        if (_state.value.phase != GamePhase.Running) return

        val levelReached = _state.value.level
        val elapsedSecAtLevel = _state.value.elapsedSecAtLevel
        val totalSeconds = totalSessionSeconds(levelReached, elapsedSecAtLevel)
        val bestToday = progressRepository.recordSession(levelReached, totalSeconds)

        stopTicking()
        _state.update {
            it.copy(
                phase = GamePhase.Idle,
                level = 0,
                elapsedSecAtLevel = 0,
                sessionSummary = SessionSummary(
                    levelReached = levelReached,
                    bestToday = bestToday,
                    totalSeconds = totalSeconds,
                ),
            )
        }
        emitKeepScreenOn(enabled = false)
        viewModelScope.launch {
            _effects.send(GameUiEffect.HapticOnThought)
        }
    }

    private fun dismissSessionSummary() {
        _state.update { it.copy(sessionSummary = null) }
    }

    private fun onAppBackgrounded() {
        if (_state.value.phase != GamePhase.Running) return

        wasRunningBeforeBackground = true
        stopTicking()
        emitKeepScreenOn(enabled = false)
    }

    private fun onAppForegrounded() {
        if (!wasRunningBeforeBackground || _state.value.phase != GamePhase.Running) {
            wasRunningBeforeBackground = false
            return
        }

        wasRunningBeforeBackground = false
        emitKeepScreenOn(enabled = true)
        startTicking()
    }

    private fun startTicking() {
        if (tickJob?.isActive == true) return

        tickJob = viewModelScope.launch {
            while (isActive) {
                delay(TICK_INTERVAL_MS)
                advanceTick()
            }
        }
    }

    private fun stopTicking() {
        tickJob?.cancel()
        tickJob = null
    }

    private fun advanceTick() {
        _state.update { current ->
            if (current.phase != GamePhase.Running) return@update current

            val nextElapsed = current.elapsedSecAtLevel + 1
            val required = current.requiredSecAtLevel

            if (nextElapsed >= required) {
                current.copy(
                    level = current.level + 1,
                    elapsedSecAtLevel = 0,
                )
            } else {
                current.copy(elapsedSecAtLevel = nextElapsed)
            }
        }
    }

    private fun emitKeepScreenOn(enabled: Boolean) {
        viewModelScope.launch {
            _effects.send(GameUiEffect.KeepScreenOn(enabled))
        }
    }

    private fun MutableStateFlow<GameUiState>.updateToRunning(
        level: Int,
        elapsedSecAtLevel: Int,
    ) {
        update {
            it.copy(
                phase = GamePhase.Running,
                level = level,
                elapsedSecAtLevel = elapsedSecAtLevel,
            )
        }
    }

    override fun onCleared() {
        stopTicking()
        super.onCleared()
    }

    private companion object {
        const val TICK_INTERVAL_MS = 1_000L
    }
}
