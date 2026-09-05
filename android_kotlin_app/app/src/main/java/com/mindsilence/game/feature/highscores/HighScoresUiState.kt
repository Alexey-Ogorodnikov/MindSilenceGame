package com.mindsilence.game.feature.highscores

import androidx.compose.runtime.Immutable
import com.mindsilence.game.feature.game.DailyStats

@Immutable
data class HighScoresUiState(
    val dailyStats: List<DailyStats> = emptyList(),
)
