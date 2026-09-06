package com.mindsilence.game.domain.repository // Persistence port; implementations live in data/.

import com.mindsilence.game.domain.model.DailyStats // Highscore rows returned newest-first.

/** Local daily training stats: record a finished attempt and read the list for highscores. */
interface GameProgressRepository { // Device prefs in production; in-memory fake in tests.
    /**
     * Writes today's day row and returns today's best level after the write.
     *
     * @param levelReached level at Thought for this attempt
     * @param totalSeconds full silence length of this attempt
     */
    fun recordSession(levelReached: Int, totalSeconds: Int): Int // Write today; return today's best level.

    /** Days newest-first for the highscore list. */
    fun getDailyStats(): List<DailyStats> // Empty when nothing has been recorded.
}
