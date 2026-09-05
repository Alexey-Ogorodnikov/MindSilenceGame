package com.mindsilence.game.feature.menu

import androidx.compose.ui.test.assertIsDisplayed
import androidx.compose.ui.test.junit4.createComposeRule
import androidx.compose.ui.test.onNodeWithContentDescription
import androidx.compose.ui.test.onNodeWithText
import androidx.compose.ui.test.performClick
import androidx.test.ext.junit.runners.AndroidJUnit4
import com.mindsilence.game.ui.theme.MindSilenceTheme
import org.junit.Assert.assertEquals
import org.junit.Assert.assertTrue
import org.junit.Rule
import org.junit.Test
import org.junit.runner.RunWith

@RunWith(AndroidJUnit4::class)
class MenuScreenTest {

    @get:Rule
    val composeRule = createComposeRule()

    @Test
    fun trainingClickIsReported() {
        var trainingClicks = 0
        composeRule.setContent {
            MindSilenceTheme {
                MenuScreen(
                    state = MenuUiState(),
                    onTrainingClick = { trainingClicks++ },
                    onHowToTrainClick = {},
                    onDismissHowToTrain = {},
                )
            }
        }

        composeRule.onNodeWithText("Mind Silence Training").performClick()
        assertEquals(1, trainingClicks)
    }

    @Test
    fun howToTrainOpensAndOkDismisses() {
        var howToTrainClicks = 0
        var dismissClicks = 0
        composeRule.setContent {
            MindSilenceTheme {
                MenuScreen(
                    state = MenuUiState(showHowToTrain = howToTrainClicks > 0 && dismissClicks == 0),
                    onTrainingClick = {},
                    onHowToTrainClick = { howToTrainClicks++ },
                    onDismissHowToTrain = { dismissClicks++ },
                )
            }
        }

        composeRule.onNodeWithContentDescription("How to train").performClick()
        assertEquals(1, howToTrainClicks)
    }

    @Test
    fun howToTrainDialogShowsCopyAndOk() {
        var dismissed = false
        composeRule.setContent {
            MindSilenceTheme {
                MenuScreen(
                    state = MenuUiState(showHowToTrain = true),
                    onTrainingClick = {},
                    onHowToTrainClick = {},
                    onDismissHowToTrain = { dismissed = true },
                )
            }
        }

        composeRule.onNodeWithText("How to train").assertIsDisplayed()
        composeRule.onNodeWithText("OK").performClick()
        assertTrue(dismissed)
    }
}
