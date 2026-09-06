package com.mindsilence.game.domain.model // Result of one Thought; presentation only displays it.

import androidx.compose.runtime.Immutable // Dialog content does not change after Thought.

/** Result of one Thought: level reached, today's best, and total seconds of the attempt. */
@Immutable // Dialog content does not change after Thought.
data class SessionSummary( // Shown until dismiss or OpenHighScores.
    val levelReached: Int, // Level at Thought.
    val bestToday: Int, // After recordSession.
    val totalSeconds: Int, // Full attempt length.
)
