package com.mindsilence.game.feature.splash // Splash intents; no navigation Channel.

/** Splash intents; [ContentMeasured] means Compose splash is laid out and the system frame can go. */
sealed interface SplashUiEvent { // Single event: layout happened.
    data object ContentMeasured : SplashUiEvent // onGloballyPositioned on SplashScreen.
} // End SplashUiEvent.
