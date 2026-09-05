package com.mindsilence.game.feature.game

import androidx.compose.ui.test.assertIsDisplayed
import androidx.compose.ui.test.assertIsEnabled
import androidx.compose.ui.test.assertIsNotEnabled
import androidx.compose.ui.test.junit4.createComposeRule
import androidx.compose.ui.test.onNodeWithContentDescription
import androidx.compose.ui.test.onNodeWithText
import androidx.compose.ui.test.performClick
import androidx.test.ext.junit.runners.AndroidJUnit4
import com.mindsilence.game.ui.theme.MindSilenceTheme
import org.junit.Assert.assertTrue
import org.junit.Rule
import org.junit.Test
import org.junit.runner.RunWith

@RunWith(AndroidJUnit4::class)
class GameScreenTest {

    @get:Rule
    val composeRule = createComposeRule()

    @Test
    fun idleEnablesStartAndDisablesThought() {
        composeRule.setContent {
            MindSilenceTheme {
                GameScreen(
                    state = GameUiState(),
                    onStartClick = {},
                    onThoughtClick = {},
                    onDismissSessionSummary = {},
                    onOpenHighScores = {},
                )
            }
        }

        composeRule.onNodeWithContentDescription("Start session").assertIsEnabled()
        composeRule.onNodeWithContentDescription("Log a thought and end the session")
            .assertIsNotEnabled()
    }

    @Test
    fun runningEnablesThoughtAndDisablesStart() {
        composeRule.setContent {
            MindSilenceTheme {
                GameScreen(
                    state = GameUiState(
                        phase = GamePhase.Running,
                        level = 2,
                        elapsedSecAtLevel = 1,
                    ),
                    onStartClick = {},
                    onThoughtClick = {},
                    onDismissSessionSummary = {},
                    onOpenHighScores = {},
                )
            }
        }

        composeRule.onNodeWithContentDescription("Start session").assertIsNotEnabled()
        composeRule.onNodeWithContentDescription("Log a thought and end the session")
            .assertIsEnabled()
        composeRule.onNodeWithText("2").assertIsDisplayed()
    }

    @Test
    fun sessionSummaryShowsOkAndHighscore() {
        var dismissed = false
        var openedHighScores = false
        composeRule.setContent {
            MindSilenceTheme {
                GameScreen(
                    state = GameUiState(
                        sessionSummary = SessionSummary(
                            levelReached = 3,
                            bestToday = 4,
                            totalSeconds = 90,
                        ),
                    ),
                    onStartClick = {},
                    onThoughtClick = {},
                    onDismissSessionSummary = { dismissed = true },
                    onOpenHighScores = { openedHighScores = true },
                )
            }
        }

        composeRule.onNodeWithText("Session complete").assertIsDisplayed()
        composeRule.onNodeWithText("OK").performClick()
        assertTrue(dismissed)

        composeRule.onNodeWithText("Highscore").performClick()
        assertTrue(openedHighScores)
    }
}
