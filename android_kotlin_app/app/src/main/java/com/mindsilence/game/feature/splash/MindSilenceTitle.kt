package com.mindsilence.game.feature.splash

import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Brush
import androidx.compose.ui.graphics.Shadow
import androidx.compose.ui.res.colorResource
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.text.TextStyle
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontWeight
import com.mindsilence.game.R

@Composable
fun MindSilenceTitle(
    modifier: Modifier = Modifier,
) {
    Text(
        text = stringResource(R.string.app_name),
        modifier = modifier,
        fontSize = SplashDefaults.TitleFontSize,
        fontWeight = FontWeight.Medium,
        letterSpacing = SplashDefaults.TitleLetterSpacing,
        fontFamily = FontFamily.Serif,
        style = TextStyle(
            brush = Brush.verticalGradient(
                colors = listOf(
                    colorResource(R.color.splash_title_gradient_start),
                    colorResource(R.color.splash_title_gradient_mid),
                    colorResource(R.color.splash_title_gradient_end),
                ),
            ),
            shadow = Shadow(
                color = colorResource(R.color.splash_title_shadow)
                    .copy(alpha = SplashDefaults.TitleShadowAlpha),
                blurRadius = SplashDefaults.TitleShadowBlurPx,
            ),
        ),
    )
}
