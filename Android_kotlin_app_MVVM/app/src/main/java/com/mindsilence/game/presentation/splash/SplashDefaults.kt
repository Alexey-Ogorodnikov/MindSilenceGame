package com.mindsilence.game.presentation.splash // Shared splash numbers so layout and tests stay in sync.

import androidx.compose.ui.unit.dp // Icon slot and title offset.
import androidx.compose.ui.unit.sp // Title type size and tracking.

/** Shared splash sizes, title styling, test tag, and branded-splash duration. */
internal object SplashDefaults { // One place for splash metrics; not a public API.
    /**
     * On-screen size of the system splash icon when
     * `windowSplashScreenIconBackgroundColor` is unset (240.dp slot × 1.2).
     */
    val IconSize = 288.dp // Match the system splash icon so the handoff does not jump.

    val TitleSpacing = 24.dp // Gap between the icon slot and the wordmark.

    val TitleFontSize = 32.sp // Wordmark size on the branded splash.

    val TitleLetterSpacing = 1.sp // Slight tracking so the title reads as a mark, not body text.

    const val TitleShadowBlurPx = 10f // Soft shadow so the gradient title stays readable on white.

    const val TitleShadowAlpha = 0.55f // Shadow opacity paired with splash_title_shadow.

    const val IconTestTag = "splash_icon" // Compose test tag for the icon size assertion.

    const val DurationMs = 2_000L // Branded splash length on cold start (handbook).
}
