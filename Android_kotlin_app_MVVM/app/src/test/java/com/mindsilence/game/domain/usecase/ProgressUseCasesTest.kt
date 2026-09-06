package com.mindsilence.game.domain.usecase // Thin UseCase wrappers must still be covered at 100% line.

import com.mindsilence.game.domain.repository.InMemoryGameProgressRepository // Fake store.
import org.junit.Assert.assertEquals // Value equality.
import org.junit.Test // JUnit 4.
import java.time.LocalDate // Today key.

/** Unit tests for [RecordSessionUseCase] and [GetDailyStatsUseCase]. */
class ProgressUseCasesTest { // Delegates to the repository; still need coverage on the UseCase classes.

    /** RecordSessionUseCase writes through the port and returns today's best. */
    @Test
    fun `record session use case returns best today`() { // One attempt.
        val repository = InMemoryGameProgressRepository() // Empty fake.
        val recordSession = RecordSessionUseCase(repository) // Production constructor, no Hilt.

        val best = recordSession(levelReached = 3, totalSeconds = 12) // invoke operator.

        assertEquals(3, best) // First write’s best is 3.
        assertEquals(LocalDate.now(), repository.getDailyStats().first().date) // Persisted today.
    }

    /** GetDailyStatsUseCase returns the same newest-first list as the repository. */
    @Test
    fun `get daily stats use case returns repository list`() { // Load after a write.
        val repository = InMemoryGameProgressRepository() // Empty fake.
        repository.recordSession(levelReached = 2, totalSeconds = 4) // Seed.
        val getDailyStats = GetDailyStatsUseCase(repository) // Production constructor, no Hilt.

        val stats = getDailyStats() // invoke operator.

        assertEquals(1, stats.size) // One day.
        assertEquals(2, stats.first().bestLevel) // From the seed write.
        assertEquals(4, stats.first().totalSeconds) // From the seed write.
    }
}
