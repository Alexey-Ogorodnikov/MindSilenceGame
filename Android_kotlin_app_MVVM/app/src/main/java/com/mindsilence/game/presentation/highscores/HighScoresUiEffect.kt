package com.mindsilence.game.presentation.highscores // One-shot highscore effects for HighScoresRoute.

/** One-shot highscore side effects; [NavigateBack] is forwarded by [HighScoresRoute] to the parent. */
sealed interface HighScoresUiEffect { // AppRoute clears showHighScores only.
    data object NavigateBack : HighScoresUiEffect // leaveHighScores(); new Idle GameRoute.
}
