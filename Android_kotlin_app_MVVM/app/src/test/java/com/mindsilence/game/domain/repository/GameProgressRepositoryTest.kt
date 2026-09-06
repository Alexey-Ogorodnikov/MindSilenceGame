package com.mindsilence.game.domain.repository // In-memory store merge rules.

import com.mindsilence.game.domain.model.DailyStats // Expected row after two attempts.
import org.junit.Assert.assertEquals // Value equality.
import org.junit.Test // JUnit 4.
import java.time.LocalDate // Today key.

/** Verifies [InMemoryGameProgressRepository] matches the handbook merge rules. */
class GameProgressRepositoryTest { // Same scenarios as the prefs Robolectric tests, without disk.

    /** Two Thoughts today: attempts++, seconds sum, bestLevel is the max. */
    @Test
    fun `recordSession stores attempts total time and best level for today`() { // Merge two attempts.
        val repository = InMemoryGameProgressRepository() // Empty store.

        val bestAfterFirst = repository.recordSession(levelReached = 2, totalSeconds = 3) // First Thought.
        val bestAfterSecond = repository.recordSession(levelReached = 1, totalSeconds = 5) // Second Thought, lower level.

        assertEquals(2, bestAfterFirst) // First write’s best is 2.
        assertEquals(2, bestAfterSecond) // Max stays 2.

        val stats = repository.getDailyStats() // Newest-first list.
        assertEquals(1, stats.size) // Only today.
        assertEquals(LocalDate.now(), stats.first().date) // Device-local today.
        assertEquals(2, stats.first().attempts) // Two Thoughts.
        assertEquals(8, stats.first().totalSeconds) // 3 + 5.
        assertEquals(2, stats.first().bestLevel) // max(2, 1).
        assertEquals( // Full row for documentation.
            DailyStats(date = LocalDate.now(), attempts = 2, totalSeconds = 8, bestLevel = 2),
            stats.first(),
        )
    }

    /** A single recorded day is returned newest-first (only one day exists). */
    @Test
    fun `getDailyStats returns days in descending order`() { // One day still sorts descending.
        val repository = InMemoryGameProgressRepository() // Empty store.

        repository.recordSession(levelReached = 1, totalSeconds = 1) // Seed today.

        val stats = repository.getDailyStats() // Newest first.
        assertEquals(1, stats.size) // One row.
        assertEquals(LocalDate.now(), stats.first().date) // Today.
    }
}
