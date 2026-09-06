package com.mindsilence.game.feature.game // One-shot training effects for GameRoute.

/** One-shot training side effects: haptic, keep-screen-on, and navigation out of this screen. */
sealed interface GameUiEffect { // Window flags and nav cannot live in UiState.
    data object HapticOnThought : GameUiEffect // LongPress when Thought ends a run.
    data class KeepScreenOn(val enabled: Boolean) : GameUiEffect // FLAG_KEEP_SCREEN_ON while foreground tick.
    data object NavigateToHighScores : GameUiEffect // AppUiEvent.OpenHighScores.
    data object NavigateBackToMenu : GameUiEffect // AppUiEvent.LeaveTraining.
} // End GameUiEffect.
