package com.mindsilence.game.data // Robolectric tests for the JSON SharedPreferences store.

import android.app.Application // Plain Application so Hilt’s generated subclass is not started.
import android.content.Context // Prefs access.
import androidx.test.core.app.ApplicationProvider // Robolectric application context.
import com.mindsilence.game.domain.model.DailyStats // Expected migrated row.
import org.junit.Assert.assertEquals // Value equality.
import org.junit.Assert.assertTrue // Boolean / contains checks.
import org.junit.Before // Clear prefs before each test.
import org.junit.Test // JUnit 4.
import org.junit.runner.RunWith // Robolectric runner.
import org.robolectric.RobolectricTestRunner // JVM Android stubs.
import org.robolectric.annotation.Config // Avoid instantiating @HiltAndroidApp.
import java.time.LocalDate // Day keys in JSON.

/** Device persistence: JSON daily_stats, merge rules, and legacy best_date/best_level migration. */
@RunWith(RobolectricTestRunner::class) // Needs Android SharedPreferences.
@Config(application = Application::class) // Do not construct MindSilenceApplication (Hilt) in JVM tests.
class SharedPreferencesGameProgressRepositoryTest { // Same cases as the MVI prefs tests.

    private lateinit var context: Context // Application context from Robolectric.

    /** Wipe game_progress so tests do not share JSON. */
    @Before
    fun clearPrefs() { // Isolation.
        context = ApplicationProvider.getApplicationContext() // Robolectric app.
        context.getSharedPreferences(PREFS_NAME, Context.MODE_PRIVATE) // Same file name as DataModule.
            .edit() // Editor.
            .clear() // Drop all keys.
            .commit() // Sync so the next repository sees empty prefs.
    }

    /** First Thought seeds the day; a second Thought merges attempts, time, and best. */
    @Test
    fun `recordSession stores first day and merges later attempts`() { // Two writes today.
        val repository = SharedPreferencesGameProgressRepository(prefs()) // Inject prefs, not Context.

        val bestAfterFirst = repository.recordSession(levelReached = 2, totalSeconds = 3) // Seed.
        val bestAfterSecond = repository.recordSession(levelReached = 1, totalSeconds = 5) // Merge.

        assertEquals(2, bestAfterFirst) // First best.
        assertEquals(2, bestAfterSecond) // Max stays 2.

        val stats = repository.getDailyStats() // Newest first.
        assertEquals(1, stats.size) // One day.
        assertEquals(LocalDate.now(), stats.first().date) // Today.
        assertEquals(2, stats.first().attempts) // Two attempts.
        assertEquals(8, stats.first().totalSeconds) // 3 + 5.
        assertEquals(2, stats.first().bestLevel) // max(2, 1).
    }

    /** JSON days are returned newest calendar date first. */
    @Test
    fun `getDailyStats returns newest day first`() { // Two stored days.
        prefs().edit() // Seed JSON without going through recordSession.
            .putString( // daily_stats array.
                KEY_DAILY_STATS, // Production key.
                """
                [
                  {"date":"2026-09-01","attempts":1,"totalSeconds":10,"bestLevel":2},
                  {"date":"2026-09-03","attempts":2,"totalSeconds":5,"bestLevel":4}
                ]
                """.trimIndent(),
            )
            .commit() // Sync.

        val stats = SharedPreferencesGameProgressRepository(prefs()).getDailyStats() // Parse + sort.

        assertEquals(2, stats.size) // Two days.
        assertEquals(LocalDate.of(2026, 9, 3), stats[0].date) // Newest first.
        assertEquals(2, stats[0].attempts) // 3 Sep attempts.
        assertEquals(5, stats[0].totalSeconds) // 3 Sep seconds.
        assertEquals(4, stats[0].bestLevel) // 3 Sep best.
        assertEquals(LocalDate.of(2026, 9, 1), stats[1].date) // Older day second.
    }

    /** Corrupt JSON must not crash; highscores show empty. */
    @Test
    fun `corrupt json yields empty stats`() { // Bad payload.
        prefs().edit().putString(KEY_DAILY_STATS, "not-json").commit() // Invalid JSON.

        assertTrue(SharedPreferencesGameProgressRepository(prefs()).getDailyStats().isEmpty()) // Empty list.
    }

    /** Missing daily_stats key means no rows. */
    @Test
    fun `missing json yields empty stats`() { // Fresh prefs.
        assertTrue(SharedPreferencesGameProgressRepository(prefs()).getDailyStats().isEmpty()) // Empty list.
    }

    /** Old best_date + best_level become one day with unknown duration. */
    @Test
    fun `legacy keys migrate to one day with unknown duration`() { // Migration path.
        prefs().edit() // Legacy keys only.
            .putString(KEY_DATE, "2026-01-15") // Old date.
            .putInt(KEY_BEST, 4) // Old best level.
            .commit() // Sync.

        val stats = SharedPreferencesGameProgressRepository(prefs()).getDailyStats() // init migrates.

        assertEquals( // Handbook: attempts=1, totalSeconds=0.
            listOf(
                DailyStats(
                    date = LocalDate.of(2026, 1, 15), // Parsed legacy date.
                    attempts = 1, // One migrated attempt.
                    totalSeconds = 0, // Duration was not stored.
                    bestLevel = 4, // Old best_level.
                ),
            ),
            stats,
        )
        assertTrue(!prefs().contains(KEY_DATE)) // Legacy date removed after write.
        assertTrue(!prefs().contains(KEY_BEST)) // Legacy best removed after write.
        assertTrue(prefs().contains(KEY_DAILY_STATS)) // JSON now exists.
    }

    /** If JSON already exists, leftover legacy keys are left alone. */
    @Test
    fun `legacy migration is skipped when daily stats already exist`() { // Skip path.
        prefs().edit() // Both formats present.
            .putString(KEY_DAILY_STATS, "[]") // Empty JSON array counts as “already migrated”.
            .putString(KEY_DATE, "2026-01-15") // Leftover.
            .putInt(KEY_BEST, 9) // Leftover.
            .commit() // Sync.

        val stats = SharedPreferencesGameProgressRepository(prefs()).getDailyStats() // No migrate.

        assertTrue(stats.isEmpty()) // Empty JSON array.
        assertTrue(prefs().contains(KEY_DATE)) // Legacy date still there.
        assertEquals(9, prefs().getInt(KEY_BEST, 0)) // Legacy best still there.
    }

    /** Legacy best without a date is ignored. */
    @Test
    fun `legacy migration ignores missing date`() { // No date key.
        prefs().edit().putInt(KEY_BEST, 3).commit() // Best only.

        assertTrue(SharedPreferencesGameProgressRepository(prefs()).getDailyStats().isEmpty()) // Nothing to migrate.
    }

    /** Non-positive legacy best is ignored. */
    @Test
    fun `legacy migration ignores non-positive best`() { // best_level = 0.
        prefs().edit() // Date with invalid best.
            .putString(KEY_DATE, "2026-01-15") // Valid date.
            .putInt(KEY_BEST, 0) // Invalid best.
            .commit() // Sync.

        assertTrue(SharedPreferencesGameProgressRepository(prefs()).getDailyStats().isEmpty()) // No row.
    }

    /** Unparsable legacy date is ignored. */
    @Test
    fun `legacy migration ignores unparsable date`() { // Bad date string.
        prefs().edit() // Unparsable date + valid best.
            .putString(KEY_DATE, "not-a-date") // Parse fails.
            .putInt(KEY_BEST, 3) // Would migrate if the date parsed.
            .commit() // Sync.

        assertTrue(SharedPreferencesGameProgressRepository(prefs()).getDailyStats().isEmpty()) // No row.
    }

    private fun prefs() = context.getSharedPreferences(PREFS_NAME, Context.MODE_PRIVATE) // Same file as DataModule.

    private companion object { // Prefs file and keys matching production.
        const val PREFS_NAME = "game_progress" // SharedPreferences file.
        const val KEY_DAILY_STATS = "daily_stats" // JSON array string.
        const val KEY_DATE = "best_date" // Legacy date key.
        const val KEY_BEST = "best_level" // Legacy level key.
    }
}
