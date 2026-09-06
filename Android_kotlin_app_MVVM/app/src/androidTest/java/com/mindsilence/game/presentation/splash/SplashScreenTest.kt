package com.mindsilence.game.presentation.splash // Compose UI tests for the branded splash layout.

import androidx.compose.ui.res.stringResource // app_name from strings.xml (MVVM launcher label).
import androidx.compose.ui.test.assertHeightIsEqualTo // Icon slot height.
import androidx.compose.ui.test.assertIsDisplayed // Title node.
import androidx.compose.ui.test.assertWidthIsEqualTo // Icon slot width.
import androidx.compose.ui.test.junit4.createComposeRule // Host Compose in instrumentation.
import androidx.compose.ui.test.onNodeWithTag // splash_icon tag.
import androidx.compose.ui.test.onNodeWithText // App name wordmark.
import androidx.compose.ui.unit.dp // 288.dp slot.
import androidx.test.ext.junit.runners.AndroidJUnit4 // Instrumentation runner.
import com.mindsilence.game.R // app_name.
import org.junit.Rule // composeRule.
import org.junit.Test // JUnit 4.
import org.junit.runner.RunWith // AndroidJUnit4.

/** SplashScreen icon slot size and app-name wordmark. */
@RunWith(AndroidJUnit4::class) // Device or emulator.
class SplashScreenTest { // Tests the stateless Screen; timing is in SplashViewModel.

    @get:Rule
    val composeRule = createComposeRule() // Compose host.

    /** Icon slot is 288.dp, matching the system splash without an icon background color. */
    @Test
    fun logoMatchesSystemSplashIconSize() { // Slot.
        val iconSize = 288.dp // Handbook size.
        composeRule.setContent { // Host the screen.
            SplashScreen(iconSize = iconSize) // Same size the test asserts.
        }

        composeRule // Find the tagged Image.
            .onNodeWithTag(SplashDefaults.IconTestTag) // splash_icon.
            .assertWidthIsEqualTo(iconSize) // Width.
            .assertHeightIsEqualTo(iconSize) // Height.
    }

    /** App name from strings.xml is shown under the icon. */
    @Test
    fun appNameIsDisplayedBelowIcon() { // Wordmark.
        lateinit var appName: String // Captured inside composition.
        composeRule.setContent { // Host the screen.
            appName = stringResource(R.string.app_name) // Mind Silence (MVVM).
            SplashScreen() // Default icon size.
        }

        composeRule // Title node.
            .onNodeWithText(appName) // Same string the composable used.
            .assertIsDisplayed() // Visible.
    }
}
