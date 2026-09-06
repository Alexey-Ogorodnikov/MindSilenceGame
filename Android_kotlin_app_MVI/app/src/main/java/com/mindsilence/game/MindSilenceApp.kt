package com.mindsilence.game // Root application package.

import androidx.compose.foundation.isSystemInDarkTheme // Follows the device light/dark setting.
import androidx.compose.runtime.Composable // Marks this as a Compose entry wrapper.
import com.mindsilence.game.ui.theme.MindSilenceTheme // Calm Material 3 palette used after splash.

/** Root Compose wrapper: applies [MindSilenceTheme] using the system dark/light setting. */
@Composable // Compose entry; must be called from setContent.
fun MindSilenceApp( // Root wrapper so screens do not each pick a theme.
    content: @Composable () -> Unit, // Child UI (splash or AppRoute).
) { // Start MindSilenceApp body.
    MindSilenceTheme(darkTheme = isSystemInDarkTheme()) { // Match system dark/light; splash itself ignores this theme.
        content() // Draw whatever the caller passed in.
    } // End MindSilenceTheme.
} // End MindSilenceApp.
