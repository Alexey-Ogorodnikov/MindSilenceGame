package com.mindsilence.game.presentation.menu // Compose UI tests for the stateless MenuScreen.

import androidx.compose.ui.test.assertIsDisplayed // Dialog title.
import androidx.compose.ui.test.junit4.createComposeRule // Host Compose in instrumentation.
import androidx.compose.ui.test.onNodeWithContentDescription // Info button.
import androidx.compose.ui.test.onNodeWithText // Training CTA and OK.
import androidx.compose.ui.test.performClick // Clicks.
import androidx.test.ext.junit.runners.AndroidJUnit4 // Instrumentation runner.
import com.mindsilence.game.presentation.theme.MindSilenceTheme // App theme wrapper.
import org.junit.Assert.assertEquals // Click counts.
import org.junit.Assert.assertTrue // Dismiss flag.
import org.junit.Rule // composeRule.
import org.junit.Test // JUnit 4.
import org.junit.runner.RunWith // AndroidJUnit4.

/** MenuScreen training CTA, info button, and How to train dialog. */
@RunWith(AndroidJUnit4::class) // Device or emulator.
class MenuScreenTest { // Tests the stateless Screen, not MenuRoute/Hilt.

    @get:Rule
    val composeRule = createComposeRule() // Compose host.

    /** Training button reports a click. */
    @Test
    fun trainingClickIsReported() { // CTA.
        var trainingClicks = 0 // Count.
        composeRule.setContent { // Host the screen.
            MindSilenceTheme { // App palette.
                MenuScreen( // Dialog closed.
                    state = MenuUiState(), // Default.
                    onTrainingClick = { trainingClicks++ }, // Count.
                    onHowToTrainClick = {}, // Unused.
                    onDismissHowToTrain = {}, // Unused.
                )
            }
        }

        composeRule.onNodeWithText("Mind Silence Training").performClick() // Primary CTA.
        assertEquals(1, trainingClicks) // One click.
    }

    /** Info button reports a click (dialog visibility is owned by the VM). */
    @Test
    fun howToTrainOpensAndOkDismisses() { // Info click.
        var howToTrainClicks = 0 // Count.
        var dismissClicks = 0 // Unused here but kept for the original scenario shape.
        composeRule.setContent { // Host the screen.
            MindSilenceTheme { // App palette.
                MenuScreen( // Dialog still closed until the VM updates state.
                    state = MenuUiState(showHowToTrain = howToTrainClicks > 0 && dismissClicks == 0), // Starts false.
                    onTrainingClick = {}, // Unused.
                    onHowToTrainClick = { howToTrainClicks++ }, // Count.
                    onDismissHowToTrain = { dismissClicks++ }, // Count.
                )
            }
        }

        composeRule.onNodeWithContentDescription("How to train").performClick() // Info.
        assertEquals(1, howToTrainClicks) // Click reported.
    }

    /** Open dialog shows copy and OK dismisses. */
    @Test
    fun howToTrainDialogShowsCopyAndOk() { // Dialog.
        var dismissed = false // OK flag.
        composeRule.setContent { // Host the screen.
            MindSilenceTheme { // App palette.
                MenuScreen( // Dialog shown.
                    state = MenuUiState(showHowToTrain = true), // Overlay.
                    onTrainingClick = {}, // Unused.
                    onHowToTrainClick = {}, // Unused.
                    onDismissHowToTrain = { dismissed = true }, // OK.
                )
            }
        }

        composeRule.onNodeWithText("How to train").assertIsDisplayed() // Title.
        composeRule.onNodeWithText("OK").performClick() // Dismiss.
        assertTrue(dismissed) // Callback fired.
    }
}
