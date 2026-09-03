package com.mindsilence.game.feature.splash

import androidx.compose.ui.res.stringResource
import androidx.compose.ui.test.assertHeightIsEqualTo
import androidx.compose.ui.test.assertIsDisplayed
import androidx.compose.ui.test.assertWidthIsEqualTo
import androidx.compose.ui.test.junit4.createComposeRule
import androidx.compose.ui.test.onNodeWithTag
import androidx.compose.ui.test.onNodeWithText
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
        composeRule.setContent {
            SplashScreen(iconSize = iconSize)
        }

        composeRule
            .onNodeWithTag(SplashDefaults.IconTestTag)
            .assertWidthIsEqualTo(iconSize)
            .assertHeightIsEqualTo(iconSize)
    }

    @Test
    fun appNameIsDisplayedBelowIcon() {
        lateinit var appName: String
        composeRule.setContent {
            appName = stringResource(R.string.app_name)
            SplashScreen()
        }

        composeRule
            .onNodeWithText(appName)
            .assertIsDisplayed()
    }
}
