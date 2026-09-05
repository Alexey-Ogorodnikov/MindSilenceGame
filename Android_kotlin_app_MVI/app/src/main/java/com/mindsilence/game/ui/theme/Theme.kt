package com.mindsilence.game.ui.theme // Theme types live off the feature packages.

import androidx.compose.material3.MaterialTheme // Applies colorScheme to descendants.
import androidx.compose.material3.darkColorScheme // Builds the default dark palette.
import androidx.compose.material3.lightColorScheme // Builds the optional light palette.
import androidx.compose.runtime.Composable // Theme is a Compose wrapper.
import androidx.compose.ui.graphics.Color // Hex palette tokens.

private val CalmBlue = Color(0xFF4A6FA5) // Primary: muted blue, not a loud accent.
private val CalmGreen = Color(0xFF5B8A72) // Secondary: muted green for supporting UI.
private val CalmBackgroundDark = Color(0xFF121820) // Dark canvas behind training/menu.
private val CalmSurfaceDark = Color(0xFF1A2330) // Dark cards/surfaces slightly above the canvas.
private val CalmBackgroundLight = Color(0xFFF2F5F8) // Light canvas if the system is in light mode.
private val CalmSurfaceLight = Color(0xFFFFFFFF) // Light surfaces; splash uses its own white, not this.

private val DarkColorScheme = darkColorScheme( // Default scheme: calm dark meditation look.
    primary = CalmBlue, // Buttons and emphasis.
    secondary = CalmGreen, // Secondary actions.
    background = CalmBackgroundDark, // Screen fill.
    surface = CalmSurfaceDark, // Cards and bars.
    onPrimary = Color.White, // Text/icons on primary.
    onSecondary = Color.White, // Text/icons on secondary.
    onBackground = Color(0xFFE8EEF4), // Readable light text on dark canvas.
    onSurface = Color(0xFFE8EEF4), // Readable light text on dark surfaces.
) // End DarkColorScheme.

private val LightColorScheme = lightColorScheme( // Used only when the system is light.
    primary = CalmBlue, // Same calm primary in light mode.
    secondary = CalmGreen, // Same calm secondary in light mode.
    background = CalmBackgroundLight, // Light canvas.
    surface = CalmSurfaceLight, // White surfaces.
    onPrimary = Color.White, // Text/icons on primary.
    onSecondary = Color.White, // Text/icons on secondary.
    onBackground = Color(0xFF1A2330), // Dark text on light canvas.
    onSurface = Color(0xFF1A2330), // Dark text on light surfaces.
) // End LightColorScheme.

/** Material 3 theme with the app's calm palette. Defaults to dark; splash does not use this. */
@Composable // Theme must wrap Compose content.
fun MindSilenceTheme( // App theme after splash; splash keeps its own white XML colors.
    darkTheme: Boolean = true, // Default dark; MindSilenceApp overrides from the system setting.
    content: @Composable () -> Unit, // Screens that should use this palette.
) { // Start MindSilenceTheme body.
    val colorScheme = if (darkTheme) DarkColorScheme else LightColorScheme // Pick palette from the darkTheme flag.

    MaterialTheme( // Publish the scheme to Material 3 descendants.
        colorScheme = colorScheme, // Colors only; no custom typography/shapes file.
        content = content, // Draw the caller’s UI inside the theme.
    ) // End MaterialTheme.
} // End MindSilenceTheme.
