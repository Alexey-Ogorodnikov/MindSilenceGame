package com.mindsilence.game.presentation.splash // Splash flags read by MainActivity and SplashScreen.

import androidx.compose.runtime.Immutable // Stable snapshot for Compose equality.

/** Whether Compose splash is on screen and whether the system splash must stay up. */
@Immutable // Collectors skip work when both flags are unchanged.
data class SplashUiState( // Two independent flags: Compose UI vs system splash condition.
    val showBrandedSplash: Boolean = true, // When false, MainActivity shows AppRoute instead.
    val keepSystemSplash: Boolean = true, // When false, setKeepOnScreenCondition lets the system splash go.
)
