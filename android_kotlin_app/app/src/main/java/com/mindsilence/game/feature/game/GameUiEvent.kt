package com.mindsilence.game.feature.game // Training intents for GameViewModel.onEvent.

/** Training intents: start, Thought, summary/highscores/back, and app background/foreground. */
sealed interface GameUiEvent { // Screen and lifecycle send these; no NavHost.
    data object Start : GameUiEvent // Idle → Running; ignored if already Running.
    data object Thought : GameUiEvent // End attempt; ignored if Idle.
    data object DismissSessionSummary : GameUiEvent // Close the complete dialog.
    data object OpenHighScores : GameUiEvent // Close summary and leave for the table.
    data object LeaveTraining : GameUiEvent // System Back → menu.
    data object AppBackgrounded : GameUiEvent // ON_STOP: pause tick, keep Running.
    data object AppForegrounded : GameUiEvent // ON_START: resume tick if it was Running.
} // End GameUiEvent.
