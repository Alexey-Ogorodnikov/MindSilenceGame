package com.mindsilence.game.feature.menu

sealed interface MenuUiEffect {
    data object NavigateToTraining : MenuUiEffect
}
