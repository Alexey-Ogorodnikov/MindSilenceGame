package com.mindsilence.game.feature.splash

import androidx.compose.foundation.Image
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.size
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.res.colorResource
import androidx.compose.ui.res.painterResource
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.unit.Dp
import com.mindsilence.game.R

@Composable
fun SplashScreen(
    iconSize: Dp = SplashDefaults.IconSize,
    modifier: Modifier = Modifier,
) {
    Box(
        modifier = modifier
            .fillMaxSize()
            .background(colorResource(R.color.splash_background)),
        contentAlignment = Alignment.Center,
    ) {
        // Same padded asset and slot as the system splash — avoids a size jump on handoff.
        Image(
            painter = painterResource(R.drawable.splash_icon),
            contentDescription = stringResource(R.string.splash_icon_content_description),
            modifier = Modifier.size(iconSize),
            contentScale = ContentScale.Fit,
        )
    }
}
