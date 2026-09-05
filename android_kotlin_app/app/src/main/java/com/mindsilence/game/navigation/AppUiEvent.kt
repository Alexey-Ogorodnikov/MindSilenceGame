package com.mindsilence.game.navigation // Intents that change AppUiState flags.

/** User or child-screen intents that change which top-level screen is visible. */
sealed interface AppUiEvent { // No Channel: these events are the navigation.
    data object OpenTraining : AppUiEvent // Menu → training (inTraining = true).
    data object LeaveTraining : AppUiEvent // Training → menu (inTraining = false).
    data object OpenHighScores : AppUiEvent // Show the table; does not clear inTraining.
    data object LeaveHighScores : AppUiEvent // Hide the table only (showHighScores = false).
} // End AppUiEvent.
