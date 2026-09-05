package com.mindsilence.game.feature.splash

import androidx.compose.foundation.Image
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.BoxWithConstraints
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.res.colorResource
import androidx.compose.ui.res.painterResource
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.Dp
import com.mindsilence.game.R
import com.mindsilence.game.ui.theme.MindSilenceTheme

@Composable
fun SplashScreen(
    iconSize: Dp = SplashDefaults.IconSize,
    modifier: Modifier = Modifier,
) {
    BoxWithConstraints(
        modifier = modifier
            .fillMaxSize()
            .background(colorResource(R.color.splash_background)),
    ) {
        // Same padded asset and slot as the system splash — avoids a size jump on handoff.
        Image(
            painter = painterResource(R.drawable.splash_icon),
            contentDescription = null,
            modifier = Modifier
                .align(Alignment.Center)
                .size(iconSize)
                .testTag(SplashDefaults.IconTestTag),
            contentScale = ContentScale.Fit,
        )
        MindSilenceTitle(
            modifier = Modifier
                .align(Alignment.TopCenter)
                .padding(
                    top = (maxHeight - iconSize) / 2 + iconSize + SplashDefaults.TitleSpacing,
                ),
        )
    }
}

@Preview(
    name = "Splash",
    showBackground = true,
    backgroundColor = 0xFFFFFFFF,
    showSystemUi = true,
)
@Composable
private fun SplashScreenPreview() {
    MindSilenceTheme(darkTheme = false) {
        SplashScreen()
    }
}
