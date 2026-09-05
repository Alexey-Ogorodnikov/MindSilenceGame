package com.mindsilence.game.feature.game

sealed interface GameUiEffect {
    data object HapticOnThought : GameUiEffect
    data class KeepScreenOn(val enabled: Boolean) : GameUiEffect
}
