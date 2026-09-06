package com.mindsilence.game.presentation.menu // Menu UI snapshot.

import androidx.compose.runtime.Immutable // Stable for Compose equality.

/** Menu screen state: whether the How to train dialog is shown. */
@Immutable // Skip recomposition when the flag is unchanged.
data class MenuUiState( // Only dialog visibility; training navigation is an effect.
    val showHowToTrain: Boolean = false, // True after onOpenHowToTrain until dismiss.
)
