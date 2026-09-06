package com.mindsilence.game.feature.highscores // Highscore list snapshot.

import androidx.compose.runtime.Immutable // Stable list snapshot.
import com.mindsilence.game.feature.game.DailyStats // One row per day.

/** Highscore list state: daily stats, newest day first when loaded from the repository. */
@Immutable // Skip work when the list identity is unchanged.
data class HighScoresUiState( // Empty list shows “No records yet”.
    val dailyStats: List<DailyStats> = emptyList(), // Loaded in ViewModel init.
) // End HighScoresUiState.
