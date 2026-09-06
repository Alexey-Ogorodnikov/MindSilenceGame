package com.mindsilence.game.presentation.splash // Wordmark shared by the branded splash.

import androidx.compose.material3.Text // Draws the app name.
import androidx.compose.runtime.Composable // Stateless title; sizes come from SplashDefaults.
import androidx.compose.ui.Modifier // Caller places this under the icon slot.
import androidx.compose.ui.graphics.Brush // Vertical gold-ish gradient on the letters.
import androidx.compose.ui.graphics.Shadow // Soft shadow so the title reads on white.
import androidx.compose.ui.res.colorResource // Gradient and shadow colors from XML.
import androidx.compose.ui.res.stringResource // app_name; no hardcoded title.
import androidx.compose.ui.text.TextStyle // Brush + shadow cannot be set as separate Text params.
import androidx.compose.ui.text.font.FontFamily // Serif mark, not body sans.
import androidx.compose.ui.text.font.FontWeight // Medium weight for the wordmark.
import com.mindsilence.game.R // app_name and splash_title_* colors.

/** App-name wordmark used on the branded splash (serif, gradient, shadow). */
@Composable // Layout-only; SplashScreen positions it.
fun MindSilenceTitle( // Same visual on system-matching Compose splash.
    modifier: Modifier = Modifier, // SplashScreen supplies TopCenter + top padding.
) { // Start MindSilenceTitle body.
    Text( // App name as a mark, not a heading in the app theme.
        text = stringResource(R.string.app_name), // English store name from strings.xml.
        modifier = modifier, // Positioned by the splash layout.
        fontSize = SplashDefaults.TitleFontSize, // 32.sp wordmark.
        fontWeight = FontWeight.Medium, // Heavier than regular body.
        letterSpacing = SplashDefaults.TitleLetterSpacing, // Slight tracking.
        fontFamily = FontFamily.Serif, // Distinct from Material body type.
        style = TextStyle( // Gradient fill + shadow (Text has no separate brush param).
            brush = Brush.verticalGradient( // Light-to-dark wash down the letters.
                colors = listOf( // Three XML stops so the gradient matches design.
                    colorResource(R.color.splash_title_gradient_start), // Top of the wash.
                    colorResource(R.color.splash_title_gradient_mid), // Mid stop.
                    colorResource(R.color.splash_title_gradient_end), // Bottom of the wash.
                ), // End colors.
            ), // End verticalGradient.
            shadow = Shadow( // Lift the title off the white field.
                color = colorResource(R.color.splash_title_shadow) // XML shadow color.
                    .copy(alpha = SplashDefaults.TitleShadowAlpha), // 0.55 so it is visible but not muddy.
                blurRadius = SplashDefaults.TitleShadowBlurPx, // 10px blur.
            ), // End Shadow.
        ), // End TextStyle.
    ) // End Text.
} // End MindSilenceTitle.
