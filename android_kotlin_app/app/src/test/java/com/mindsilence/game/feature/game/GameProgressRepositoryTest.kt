package com.mindsilence.game.feature.game

import org.junit.Assert.assertEquals
import org.junit.Test
import java.time.LocalDate

class GameProgressRepositoryTest {

    @Test
    fun `recordSession stores attempts total time and best level for today`() {
        val repository = InMemoryGameProgressRepository()

        val bestAfterFirst = repository.recordSession(levelReached = 2, totalSeconds = 3)
        val bestAfterSecond = repository.recordSession(levelReached = 1, totalSeconds = 5)

        assertEquals(2, bestAfterFirst)
        assertEquals(2, bestAfterSecond)

        val stats = repository.getDailyStats()
        assertEquals(1, stats.size)
        assertEquals(LocalDate.now(), stats.first().date)
        assertEquals(2, stats.first().attempts)
        assertEquals(8, stats.first().totalSeconds)
        assertEquals(2, stats.first().bestLevel)
    }

    @Test
    fun `getDailyStats returns days in descending order`() {
        val repository = InMemoryGameProgressRepository()

        repository.recordSession(levelReached = 1, totalSeconds = 1)

        val stats = repository.getDailyStats()
        assertEquals(1, stats.size)
        assertEquals(LocalDate.now(), stats.first().date)
    }
}
