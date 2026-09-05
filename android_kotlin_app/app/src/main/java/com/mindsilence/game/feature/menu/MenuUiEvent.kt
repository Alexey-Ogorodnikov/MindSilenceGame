package com.mindsilence.game.feature.menu // Menu click intents.

/** Menu user intents: start training or open/close the How to train dialog. */
sealed interface MenuUiEvent { // Screen sends these; it does not call AppViewModel.
    data object OpenTraining : MenuUiEvent // Training button; becomes NavigateToTraining.
    data object OpenHowToTrain : MenuUiEvent // Info button; sets showHowToTrain.
    data object DismissHowToTrain : MenuUiEvent // Close the How to train dialog.
} // End MenuUiEvent.
