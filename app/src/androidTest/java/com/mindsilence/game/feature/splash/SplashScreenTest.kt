package com.mindsilence.game.feature.splash

import androidx.compose.ui.res.stringResource
import androidx.compose.ui.test.assertHeightIsEqualTo
import androidx.compose.ui.test.assertWidthIsEqualTo
import androidx.compose.ui.test.junit4.createComposeRule
import androidx.compose.ui.test.onNodeWithContentDescription
import androidx.compose.ui.unit.dp
import androidx.test.ext.junit.runners.AndroidJUnit4
import com.mindsilence.game.R
import org.junit.Rule
import org.junit.Test
import org.junit.runner.RunWith

@RunWith(AndroidJUnit4::class)
class SplashScreenTest {

    @get:Rule
    val composeRule = createComposeRule()

    @Test
    fun logoMatchesSystemSplashIconSize() {
        val iconSize = 288.dp
        lateinit var contentDescription: String
        composeRule.setContent {
            contentDescription = stringResource(R.string.splash_icon_content_description)
            SplashScreen(iconSize = iconSize)
        }

        composeRule
            .onNodeWithContentDescription(contentDescription)
            .assertWidthIsEqualTo(iconSize)
            .assertHeightIsEqualTo(iconSize)
    }
}
