package com.mindsilence.game.feature.highscores

import androidx.compose.ui.test.assertIsDisplayed
import androidx.compose.ui.test.junit4.createComposeRule
import androidx.compose.ui.test.onNodeWithText
import androidx.compose.ui.test.performClick
import androidx.test.ext.junit.runners.AndroidJUnit4
import com.mindsilence.game.feature.game.DailyStats
import com.mindsilence.game.ui.theme.MindSilenceTheme
import org.junit.Assert.assertTrue
import org.junit.Rule
import org.junit.Test
import org.junit.runner.RunWith
import java.time.LocalDate

@RunWith(AndroidJUnit4::class)
class HighScoresScreenTest {

    @get:Rule
    val composeRule = createComposeRule()

    @Test
    fun emptyStateShowsCopy() {
        composeRule.setContent {
            MindSilenceTheme {
                HighScoresScreen(
                    state = HighScoresUiState(),
                    onBack = {},
                )
            }
        }

        composeRule.onNodeWithText("No records yet").assertIsDisplayed()
    }

    @Test
    fun dayRowShowsAttemptsAndBack() {
        var backClicked = false
        composeRule.setContent {
            MindSilenceTheme {
                HighScoresScreen(
                    state = HighScoresUiState(
                        dailyStats = listOf(
                            DailyStats(
                                date = LocalDate.of(2026, 9, 2),
                                attempts = 3,
                                totalSeconds = 185,
                                bestLevel = 5,
                            ),
                        ),
                    ),
                    onBack = { backClicked = true },
                )
            }
        }

        composeRule.onNodeWithText("2 September 2026").assertIsDisplayed()
        composeRule.onNodeWithText("Attempts: 3").assertIsDisplayed()
        composeRule.onNodeWithText("Best level: 5").assertIsDisplayed()
        composeRule.onNodeWithText("Back").performClick()
        assertTrue(backClicked)
    }
}
