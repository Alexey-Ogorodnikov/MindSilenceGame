package com.mindsilence.game.feature.highscores // Highscore click intents.

/** Highscore user intents; [Back] leaves this screen for training. */
sealed interface HighScoresUiEvent { // Top bar Back only.
    data object Back : HighScoresUiEvent // Becomes NavigateBack.
} // End HighScoresUiEvent.
