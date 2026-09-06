package com.mindsilence.game.domain.repository // In-memory fake; production code never imports this.

import com.mindsilence.game.domain.model.DailyStats // Same row type as the prefs implementation.
import java.time.LocalDate // Test clock is the JVM default.

/** In-memory [GameProgressRepository] for unit tests; nothing is written to disk. */
class InMemoryGameProgressRepository : GameProgressRepository { // Used by ViewModel and UseCase tests.

    private val statsByDate = linkedMapOf<LocalDate, DailyStats>() // Insertion order; sorted on read.

    override fun recordSession(levelReached: Int, totalSeconds: Int): Int { // Same merge rules as prefs.
        val today = LocalDate.now() // Test clock is the JVM default.
        val current = statsByDate[today] // Null if first today.
        val updated = if (current == null) { // First attempt today.
            DailyStats( // New row.
                date = today, // Day key.
                attempts = 1, // This attempt.
                totalSeconds = totalSeconds, // This length.
                bestLevel = levelReached, // This level.
            )
        } else { // Same day again.
            current.copy( // Merge.
                attempts = current.attempts + 1, // Count.
                totalSeconds = current.totalSeconds + totalSeconds, // Sum.
                bestLevel = maxOf(current.bestLevel, levelReached), // Max level.
            )
        }
        statsByDate[today] = updated // Replace today’s row.
        return updated.bestLevel // Same return as the prefs impl.
    }

    override fun getDailyStats(): List<DailyStats> = // Newest first, like prefs.
        statsByDate.values.sortedByDescending { it.date } // Highscore order.
}
