package com.mindsilence.game.feature.game

sealed interface GameUiEvent {
    data object Start : GameUiEvent
    data object Thought : GameUiEvent
    data object DismissSessionSummary : GameUiEvent
    data object AppBackgrounded : GameUiEvent
    data object AppForegrounded : GameUiEvent
}
