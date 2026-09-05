package com.mindsilence.game.navigation // Navigation flags for AppViewModel / AppRoute.

import androidx.compose.runtime.Immutable // Stable snapshot for Compose equality.

/** Which top-level screen to show: training and/or highscores over the menu. */
@Immutable // Compose can skip work when both flags are unchanged.
data class AppUiState( // Two flags; not a back stack.
    val inTraining: Boolean = false, // True after OpenTraining; stays true under highscores.
    val showHighScores: Boolean = false, // True after OpenHighScores; takes precedence in AppRoute.
) // End AppUiState.
