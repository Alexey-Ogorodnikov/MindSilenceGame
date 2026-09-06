package com.mindsilence.game.presentation.highscores // Compose UI tests for the stateless HighScoresScreen.

import androidx.compose.ui.test.assertIsDisplayed // Empty copy and day row.
import androidx.compose.ui.test.junit4.createComposeRule // Host Compose in instrumentation.
import androidx.compose.ui.test.onNodeWithText // Dates and Back.
import androidx.compose.ui.test.performClick // Back.
import androidx.test.ext.junit.runners.AndroidJUnit4 // Instrumentation runner.
import com.mindsilence.game.domain.model.DailyStats // Sample row.
import com.mindsilence.game.presentation.theme.MindSilenceTheme // App theme wrapper.
import org.junit.Assert.assertTrue // Back flag.
import org.junit.Rule // composeRule.
import org.junit.Test // JUnit 4.
import org.junit.runner.RunWith // AndroidJUnit4.
import java.time.LocalDate // Sample date.

/** HighScoresScreen empty copy, day row, and Back. */
@RunWith(AndroidJUnit4::class) // Device or emulator.
class HighScoresScreenTest { // Tests the stateless Screen, not HighScoresRoute/Hilt.

    @get:Rule
    val composeRule = createComposeRule() // Compose host.

    /** Empty list shows No records yet. */
    @Test
    fun emptyStateShowsCopy() { // Empty.
        composeRule.setContent { // Host the screen.
            MindSilenceTheme { // App palette.
                HighScoresScreen( // No rows.
                    state = HighScoresUiState(), // Empty list.
                    onBack = {}, // Unused.
                )
            }
        }

        composeRule.onNodeWithText("No records yet").assertIsDisplayed() // Empty copy.
    }

    /** A day row shows English date, attempts, best level, and Back reports a click. */
    @Test
    fun dayRowShowsAttemptsAndBack() { // Filled list.
        var backClicked = false // Back flag.
        composeRule.setContent { // Host the screen.
            MindSilenceTheme { // App palette.
                HighScoresScreen( // One day.
                    state = HighScoresUiState(
                        dailyStats = listOf(
                            DailyStats( // Sample row.
                                date = LocalDate.of(2026, 9, 2), // English long date.
                                attempts = 3, // Shown as Attempts: 3.
                                totalSeconds = 185, // Minutes/seconds in the Total line.
                                bestLevel = 5, // Shown as Best level: 5.
                            ),
                        ),
                    ),
                    onBack = { backClicked = true }, // Top bar.
                )
            }
        }

        composeRule.onNodeWithText("2 September 2026").assertIsDisplayed() // Locale.ENGLISH.
        composeRule.onNodeWithText("Attempts: 3").assertIsDisplayed() // Attempts line.
        composeRule.onNodeWithText("Best level: 5").assertIsDisplayed() // Best line.
        composeRule.onNodeWithText("Back").performClick() // Top bar.
        assertTrue(backClicked) // Callback fired.
    }
}
