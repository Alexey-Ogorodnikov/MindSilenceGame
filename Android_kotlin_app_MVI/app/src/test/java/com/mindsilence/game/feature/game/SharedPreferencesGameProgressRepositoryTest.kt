package com.mindsilence.game.feature.game

import android.content.Context
import androidx.test.core.app.ApplicationProvider
import org.junit.Assert.assertEquals
import org.junit.Assert.assertTrue
import org.junit.Before
import org.junit.Test
import org.junit.runner.RunWith
import org.robolectric.RobolectricTestRunner
import java.time.LocalDate

@RunWith(RobolectricTestRunner::class)
class SharedPreferencesGameProgressRepositoryTest {

    private lateinit var context: Context

    @Before
    fun clearPrefs() {
        context = ApplicationProvider.getApplicationContext()
        context.getSharedPreferences(PREFS_NAME, Context.MODE_PRIVATE)
            .edit()
            .clear()
            .commit()
    }

    @Test
    fun `recordSession stores first day and merges later attempts`() {
        val repository = SharedPreferencesGameProgressRepository(context)

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
    fun `getDailyStats returns newest day first`() {
        prefs().edit()
            .putString(
                KEY_DAILY_STATS,
                """
                [
                  {"date":"2026-09-01","attempts":1,"totalSeconds":10,"bestLevel":2},
                  {"date":"2026-09-03","attempts":2,"totalSeconds":5,"bestLevel":4}
                ]
                """.trimIndent(),
            )
            .commit()

        val stats = SharedPreferencesGameProgressRepository(context).getDailyStats()

        assertEquals(2, stats.size)
        assertEquals(LocalDate.of(2026, 9, 3), stats[0].date)
        assertEquals(2, stats[0].attempts)
        assertEquals(5, stats[0].totalSeconds)
        assertEquals(4, stats[0].bestLevel)
        assertEquals(LocalDate.of(2026, 9, 1), stats[1].date)
    }

    @Test
    fun `corrupt json yields empty stats`() {
        prefs().edit().putString(KEY_DAILY_STATS, "not-json").commit()

        assertTrue(SharedPreferencesGameProgressRepository(context).getDailyStats().isEmpty())
    }

    @Test
    fun `missing json yields empty stats`() {
        assertTrue(SharedPreferencesGameProgressRepository(context).getDailyStats().isEmpty())
    }

    @Test
    fun `legacy keys migrate to one day with unknown duration`() {
        prefs().edit()
            .putString(KEY_DATE, "2026-01-15")
            .putInt(KEY_BEST, 4)
            .commit()

        val stats = SharedPreferencesGameProgressRepository(context).getDailyStats()

        assertEquals(
            listOf(
                DailyStats(
                    date = LocalDate.of(2026, 1, 15),
                    attempts = 1,
                    totalSeconds = 0,
                    bestLevel = 4,
                ),
            ),
            stats,
        )
        assertTrue(!prefs().contains(KEY_DATE))
        assertTrue(!prefs().contains(KEY_BEST))
        assertTrue(prefs().contains(KEY_DAILY_STATS))
    }

    @Test
    fun `legacy migration is skipped when daily stats already exist`() {
        prefs().edit()
            .putString(KEY_DAILY_STATS, "[]")
            .putString(KEY_DATE, "2026-01-15")
            .putInt(KEY_BEST, 9)
            .commit()

        val stats = SharedPreferencesGameProgressRepository(context).getDailyStats()

        assertTrue(stats.isEmpty())
        assertTrue(prefs().contains(KEY_DATE))
        assertEquals(9, prefs().getInt(KEY_BEST, 0))
    }

    @Test
    fun `legacy migration ignores missing date`() {
        prefs().edit().putInt(KEY_BEST, 3).commit()

        assertTrue(SharedPreferencesGameProgressRepository(context).getDailyStats().isEmpty())
    }

    @Test
    fun `legacy migration ignores non-positive best`() {
        prefs().edit()
            .putString(KEY_DATE, "2026-01-15")
            .putInt(KEY_BEST, 0)
            .commit()

        assertTrue(SharedPreferencesGameProgressRepository(context).getDailyStats().isEmpty())
    }

    @Test
    fun `legacy migration ignores unparsable date`() {
        prefs().edit()
            .putString(KEY_DATE, "not-a-date")
            .putInt(KEY_BEST, 3)
            .commit()

        assertTrue(SharedPreferencesGameProgressRepository(context).getDailyStats().isEmpty())
    }

    private fun prefs() = context.getSharedPreferences(PREFS_NAME, Context.MODE_PRIVATE)

    private companion object {
        const val PREFS_NAME = "game_progress"
        const val KEY_DAILY_STATS = "daily_stats"
        const val KEY_DATE = "best_date"
        const val KEY_BEST = "best_level"
    }
}
