package com.mindsilence.game.presentation.game // Compose UI tests for the stateless GameScreen.

import androidx.compose.ui.test.assertIsDisplayed // Level number and dialog title.
import androidx.compose.ui.test.assertIsEnabled // Enabled Start or Thought.
import androidx.compose.ui.test.assertIsNotEnabled // Disabled Start or Thought.
import androidx.compose.ui.test.junit4.createComposeRule // Host Compose in instrumentation.
import androidx.compose.ui.test.onNodeWithContentDescription // Start / Thought a11y.
import androidx.compose.ui.test.onNodeWithText // Dialog copy.
import androidx.compose.ui.test.performClick // OK / Highscore.
import androidx.test.ext.junit.runners.AndroidJUnit4 // Instrumentation runner.
import com.mindsilence.game.domain.model.GamePhase // Running snapshot.
import com.mindsilence.game.domain.model.SessionSummary // Dialog payload.
import com.mindsilence.game.presentation.theme.MindSilenceTheme // App theme wrapper.
import org.junit.Assert.assertTrue // Click flags.
import org.junit.Rule // composeRule.
import org.junit.Test // JUnit 4.
import org.junit.runner.RunWith // AndroidJUnit4.

/** GameScreen idle/running controls and session-complete dialog actions. */
@RunWith(AndroidJUnit4::class) // Device or emulator.
class GameScreenTest { // Tests the stateless Screen, not GameRoute/Hilt.

    @get:Rule
    val composeRule = createComposeRule() // Compose host.

    /** Idle: Start enabled, Thought disabled. */
    @Test
    fun idleEnablesStartAndDisablesThought() { // Idle snapshot.
        composeRule.setContent { // Host the screen.
            MindSilenceTheme { // App palette.
                GameScreen( // Default Idle.
                    state = GameUiState(), // phase Idle.
                    onStartClick = {}, // Unused.
                    onThoughtClick = {}, // Unused.
                    onDismissSessionSummary = {}, // Unused.
                    onOpenHighScores = {}, // Unused.
                )
            }
        }

        composeRule.onNodeWithContentDescription("Start session").assertIsEnabled() // Start tap target.
        composeRule.onNodeWithContentDescription("Log a thought and end the session") // Thought a11y.
            .assertIsNotEnabled() // Disabled in Idle.
    }

    /** Running: Thought enabled, Start disabled, level number shown. */
    @Test
    fun runningEnablesThoughtAndDisablesStart() { // Running snapshot.
        composeRule.setContent { // Host the screen.
            MindSilenceTheme { // App palette.
                GameScreen( // Level 2 Running.
                    state = GameUiState(
                        phase = GamePhase.Running, // Tick UI.
                        level = 2, // Number in the ring.
                        elapsedSecAtLevel = 1, // Partial bar.
                    ),
                    onStartClick = {}, // Unused.
                    onThoughtClick = {}, // Unused.
                    onDismissSessionSummary = {}, // Unused.
                    onOpenHighScores = {}, // Unused.
                )
            }
        }

        composeRule.onNodeWithContentDescription("Start session").assertIsNotEnabled() // Disabled while Running.
        composeRule.onNodeWithContentDescription("Log a thought and end the session") // Thought a11y.
            .assertIsEnabled() // Enabled while Running.
        composeRule.onNodeWithText("2").assertIsDisplayed() // Level glyph.
    }

    /** Session-complete dialog shows OK and Highscore and reports both clicks. */
    @Test
    fun sessionSummaryShowsOkAndHighscore() { // Dialog.
        var dismissed = false // OK flag.
        var openedHighScores = false // Highscore flag.
        composeRule.setContent { // Host the screen.
            MindSilenceTheme { // App palette.
                GameScreen( // Summary over Idle.
                    state = GameUiState(
                        sessionSummary = SessionSummary( // Sample payload.
                            levelReached = 3, // This run.
                            bestToday = 4, // After record.
                            totalSeconds = 90, // 1:30.
                        ),
                    ),
                    onStartClick = {}, // Unused.
                    onThoughtClick = {}, // Unused.
                    onDismissSessionSummary = { dismissed = true }, // OK.
                    onOpenHighScores = { openedHighScores = true }, // Highscore.
                )
            }
        }

        composeRule.onNodeWithText("Session complete").assertIsDisplayed() // Title.
        composeRule.onNodeWithText("OK").performClick() // Dismiss.
        assertTrue(dismissed) // Callback fired.

        composeRule.onNodeWithText("Highscore").performClick() // Navigate.
        assertTrue(openedHighScores) // Callback fired.
    }
}
