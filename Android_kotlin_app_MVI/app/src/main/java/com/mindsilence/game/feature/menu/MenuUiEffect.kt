package com.mindsilence.game.feature.menu // One-shot menu effects for MenuRoute.

/** One-shot menu side effects; [NavigateToTraining] is forwarded by [MenuRoute] to the parent. */
sealed interface MenuUiEffect { // Not stored in UiState so it fires once.
    data object NavigateToTraining : MenuUiEffect // AppRoute turns this into OpenTraining.
} // End MenuUiEffect.
