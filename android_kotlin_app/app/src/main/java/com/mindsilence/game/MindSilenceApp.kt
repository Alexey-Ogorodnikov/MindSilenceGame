package com.mindsilence.game

import androidx.compose.foundation.isSystemInDarkTheme
import androidx.compose.runtime.Composable
import com.mindsilence.game.ui.theme.MindSilenceTheme

@Composable
fun MindSilenceApp(
    content: @Composable () -> Unit,
) {
    MindSilenceTheme(darkTheme = isSystemInDarkTheme()) {
        content()
    }
}
