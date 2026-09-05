package com.mindsilence.game.feature.splash

import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp

internal object SplashDefaults {
    /**
     * On-screen size of the system splash icon when
     * `windowSplashScreenIconBackgroundColor` is unset (240.dp slot × 1.2).
     */
    val IconSize = 288.dp

    val TitleSpacing = 24.dp

    val TitleFontSize = 32.sp

    val TitleLetterSpacing = 1.sp

    const val TitleShadowBlurPx = 10f

    const val TitleShadowAlpha = 0.55f

    const val IconTestTag = "splash_icon"
}
