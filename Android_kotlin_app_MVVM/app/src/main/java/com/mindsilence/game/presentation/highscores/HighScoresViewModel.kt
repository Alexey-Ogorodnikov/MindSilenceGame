package com.mindsilence.game.presentation.highscores // Highscore MVVM: load list, Back is an effect.

import androidx.lifecycle.ViewModel // List survives config change while this screen is shown.
import androidx.lifecycle.viewModelScope // Send NavigateBack.
import com.mindsilence.game.domain.usecase.GetDailyStatsUseCase // Same store GameViewModel just wrote.
import dagger.hilt.android.lifecycle.HiltViewModel // Hilt constructs this when HighScoresRoute calls hiltViewModel().
import javax.inject.Inject // Constructor injection of the load-stats use case.
import kotlinx.coroutines.channels.Channel // One-shot Back.
import kotlinx.coroutines.flow.Flow // Collected by HighScoresRoute.
import kotlinx.coroutines.flow.MutableStateFlow // Private list.
import kotlinx.coroutines.flow.StateFlow // Screen list.
import kotlinx.coroutines.flow.asStateFlow // Hide mutation.
import kotlinx.coroutines.flow.receiveAsFlow // Route collects once.
import kotlinx.coroutines.flow.update // Load in init.
import kotlinx.coroutines.launch // Channel.send.

/**
 * Highscore MVVM: loads DailyStats on init and turns Back into a navigate-out effect.
 */
@HiltViewModel // Created when HighScoresRoute enters composition.
class HighScoresViewModel @Inject constructor( // Hilt supplies GetDailyStatsUseCase.
    private val getDailyStats: GetDailyStatsUseCase, // Same instance GameViewModel just wrote through RecordSessionUseCase.
) : ViewModel() { // Destroyed when leaving highscores if the store owner is cleared.

    private val _state = MutableStateFlow(HighScoresUiState()) // Empty until init load.
    val state: StateFlow<HighScoresUiState> = _state.asStateFlow() // LazyColumn content.

    private val _effects = Channel<HighScoresUiEffect>(Channel.BUFFERED) // Buffer Back.
    val effects: Flow<HighScoresUiEffect> = _effects.receiveAsFlow() // Maps to onBack.

    init { // Load once; list is not observed live.
        _state.update { it.copy(dailyStats = getDailyStats()) } // Newest first from the repo.
    }

    /** Top-bar Back: one-shot navigate to a new Idle training. */
    fun onBack() { // Effect, not a state flag.
        viewModelScope.launch { // Channel.send is suspending.
            _effects.send(HighScoresUiEffect.NavigateBack) // AppViewModel.leaveHighScores().
        }
    }
}
