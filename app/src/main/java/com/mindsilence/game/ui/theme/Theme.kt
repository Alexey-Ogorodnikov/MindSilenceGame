package com.mindsilence.game.ui.theme

import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.darkColorScheme
import androidx.compose.material3.lightColorScheme
import androidx.compose.runtime.Composable
import androidx.compose.ui.graphics.Color

private val CalmBlue = Color(0xFF4A6FA5)
private val CalmGreen = Color(0xFF5B8A72)
private val CalmBackgroundDark = Color(0xFF121820)
private val CalmSurfaceDark = Color(0xFF1A2330)
private val CalmBackgroundLight = Color(0xFFF2F5F8)
private val CalmSurfaceLight = Color(0xFFFFFFFF)

private val DarkColorScheme = darkColorScheme(
    primary = CalmBlue,
    secondary = CalmGreen,
    background = CalmBackgroundDark,
    surface = CalmSurfaceDark,
    onPrimary = Color.White,
    onSecondary = Color.White,
    onBackground = Color(0xFFE8EEF4),
    onSurface = Color(0xFFE8EEF4),
)

private val LightColorScheme = lightColorScheme(
    primary = CalmBlue,
    secondary = CalmGreen,
    background = CalmBackgroundLight,
    surface = CalmSurfaceLight,
    onPrimary = Color.White,
    onSecondary = Color.White,
    onBackground = Color(0xFF1A2330),
    onSurface = Color(0xFF1A2330),
)

@Composable
fun MindSilenceTheme(
    darkTheme: Boolean = true,
    content: @Composable () -> Unit,
) {
    val colorScheme = if (darkTheme) DarkColorScheme else LightColorScheme

    MaterialTheme(
        colorScheme = colorScheme,
        content = content,
    )
}
