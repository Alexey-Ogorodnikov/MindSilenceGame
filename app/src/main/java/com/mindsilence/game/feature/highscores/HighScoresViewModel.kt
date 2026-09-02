package com.mindsilence.game.feature.highscores

import androidx.lifecycle.ViewModel
import com.mindsilence.game.feature.game.GameProgressRepository
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow

class HighScoresViewModel(
    private val progressRepository: GameProgressRepository,
) : ViewModel() {

    private val _state = MutableStateFlow(HighScoresUiState())
    val state: StateFlow<HighScoresUiState> = _state.asStateFlow()

    init {
        refresh()
    }

    fun refresh() {
        _state.value = HighScoresUiState(
            dailyStats = progressRepository.getDailyStats(),
        )
    }
}
