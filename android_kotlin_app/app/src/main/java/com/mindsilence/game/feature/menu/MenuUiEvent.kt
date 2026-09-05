package com.mindsilence.game.feature.menu

sealed interface MenuUiEvent {
    data object OpenTraining : MenuUiEvent
    data object OpenHowToTrain : MenuUiEvent
    data object DismissHowToTrain : MenuUiEvent
}
