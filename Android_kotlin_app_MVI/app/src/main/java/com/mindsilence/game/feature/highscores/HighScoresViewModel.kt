package com.mindsilence.game.feature.highscores // Highscore MVI: load list, Back is an effect.

import androidx.lifecycle.ViewModel // List survives config change while this screen is shown.
import androidx.lifecycle.viewModelScope // Send NavigateBack.
import com.mindsilence.game.feature.game.GameProgressRepository // Shared store from AppRoute.
import kotlinx.coroutines.channels.Channel // One-shot Back.
import kotlinx.coroutines.flow.Flow // Collected by HighScoresRoute.
import kotlinx.coroutines.flow.MutableStateFlow // Private list.
import kotlinx.coroutines.flow.StateFlow // Screen list.
import kotlinx.coroutines.flow.asStateFlow // Hide mutation.
import kotlinx.coroutines.flow.receiveAsFlow // Route collects once.
import kotlinx.coroutines.flow.update // Load in init.
import kotlinx.coroutines.launch // Channel.send.

/**
 * Highscore MVI: loads [DailyStats] on init and turns Back into a navigate-out effect.
 */
class HighScoresViewModel( // Created when HighScoresRoute enters composition.
    private val progressRepository: GameProgressRepository, // Same instance GameViewModel just wrote.
) : ViewModel() { // Destroyed when leaving highscores.

    private val _state = MutableStateFlow(HighScoresUiState()) // Empty until init load.
    val state: StateFlow<HighScoresUiState> = _state.asStateFlow() // LazyColumn content.

    private val _effects = Channel<HighScoresUiEffect>(Channel.BUFFERED) // Buffer Back.
    val effects: Flow<HighScoresUiEffect> = _effects.receiveAsFlow() // Maps to onBack.

    init { // Load once; list is not observed live.
        _state.update { it.copy(dailyStats = progressRepository.getDailyStats()) } // Newest first from the repo.
    } // End init.

    fun onEvent(event: HighScoresUiEvent) { // Only Back.
        when (event) { // Exhaustive.
            HighScoresUiEvent.Back -> viewModelScope.launch { // Effect, not a state flag.
                _effects.send(HighScoresUiEffect.NavigateBack) // AppUiEvent.LeaveHighScores.
            } // End Back launch.
        } // End when.
    } // End onEvent.
} // End HighScoresViewModel.
