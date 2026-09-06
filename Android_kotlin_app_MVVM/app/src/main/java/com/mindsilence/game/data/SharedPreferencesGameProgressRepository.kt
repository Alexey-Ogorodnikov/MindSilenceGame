package com.mindsilence.game.data // Device persistence; domain does not import this package.

import android.content.SharedPreferences // JSON blob lives in the game_progress prefs file.
import com.mindsilence.game.domain.model.DailyStats // Domain row written to and read from JSON.
import com.mindsilence.game.domain.repository.GameProgressRepository // Port this class implements.
import org.json.JSONArray // daily_stats value.
import org.json.JSONObject // One day in the array.
import java.time.LocalDate // Device-local day key.
import javax.inject.Inject // Hilt constructs this as the singleton repository impl.

/** Device persistence for [GameProgressRepository] via SharedPreferences JSON. */
class SharedPreferencesGameProgressRepository @Inject constructor( // Singleton from DataModule prefs.
    private val prefs: SharedPreferences, // File game_progress provided by Hilt.
) : GameProgressRepository { // Production store.

    init { // One-time move from old best_date/best_level keys.
        migrateLegacyIfNeeded() // No-op if daily_stats already exists.
    }

    override fun recordSession(levelReached: Int, totalSeconds: Int): Int { // Called on Thought.
        val today = LocalDate.now() // Device-local calendar day.
        val statsByDate = loadStats().toMutableMap() // Existing days.
        val current = statsByDate[today] // Null if first attempt today.
        val updated = if (current == null) { // First Thought of the day.
            DailyStats( // Seed the day.
                date = today, // Map key.
                attempts = 1, // This attempt.
                totalSeconds = totalSeconds, // This attempt’s length.
                bestLevel = levelReached, // This attempt’s level.
            )
        } else { // Another Thought the same day.
            current.copy( // Merge into today.
                attempts = current.attempts + 1, // Count attempts.
                totalSeconds = current.totalSeconds + totalSeconds, // Sum silence time.
                bestLevel = maxOf(current.bestLevel, levelReached), // Keep the day’s high.
            )
        }
        statsByDate[today] = updated // Replace today’s row.
        saveStats(statsByDate) // Write JSON and drop legacy keys.
        return updated.bestLevel // Session summary “best today”.
    }

    override fun getDailyStats(): List<DailyStats> = // Highscores list.
        loadStats().values.sortedByDescending { it.date } // Newest first.

    private fun loadStats(): Map<LocalDate, DailyStats> { // Parse daily_stats or empty.
        val json = prefs.getString(KEY_DAILY_STATS, null) ?: return emptyMap() // Missing key: no rows.
        return runCatching { // Corrupt JSON must not crash the app.
            val array = JSONArray(json) // Stored array of day objects.
            buildMap { // Date → DailyStats.
                for (index in 0 until array.length()) { // Each stored day.
                    val item = array.getJSONObject(index) // One object.
                    val date = LocalDate.parse(item.getString(JSON_DATE)) // ISO date string.
                    put( // Last write for a date wins if duplicates ever appear.
                        date, // Key.
                        DailyStats( // Fields match saveStats.
                            date = date, // Same as key.
                            attempts = item.getInt(JSON_ATTEMPTS), // Attempt count.
                            totalSeconds = item.getInt(JSON_TOTAL_SECONDS), // Summed seconds.
                            bestLevel = item.getInt(JSON_BEST_LEVEL), // Max level that day.
                        ),
                    )
                }
            }
        }.getOrDefault(emptyMap()) // On parse failure, show empty highscores.
    }

    private fun saveStats(statsByDate: Map<LocalDate, DailyStats>) { // Overwrite daily_stats.
        val array = JSONArray() // Build a stable array.
        statsByDate.values // All days.
            .sortedBy { it.date } // Oldest first on disk.
            .forEach { stat -> // One JSON object per day.
                array.put( // Append.
                    JSONObject().apply { // Keys used by loadStats.
                        put(JSON_DATE, stat.date.toString()) // ISO-8601 date.
                        put(JSON_ATTEMPTS, stat.attempts) // Int.
                        put(JSON_TOTAL_SECONDS, stat.totalSeconds) // Int.
                        put(JSON_BEST_LEVEL, stat.bestLevel) // Int.
                    },
                )
            }
        prefs.edit() // Single apply after JSON + legacy cleanup.
            .putString(KEY_DAILY_STATS, array.toString()) // New format.
            .remove(KEY_DATE) // Drop legacy best_date after a successful write.
            .remove(KEY_BEST) // Drop legacy best_level.
            .apply() // Async disk write.
    }

    private fun migrateLegacyIfNeeded() { // Old install: one best day, no JSON array.
        if (prefs.contains(KEY_DAILY_STATS)) return // Already migrated.

        val legacyDate = prefs.getString(KEY_DATE, null) ?: return // Nothing to migrate.
        val legacyBest = prefs.getInt(KEY_BEST, 0) // Old best level.
        if (legacyBest <= 0) return // Invalid legacy row.

        val date = runCatching { LocalDate.parse(legacyDate) }.getOrNull() ?: return // Bad date string.
        saveStats( // One day: attempts=1, totalSeconds=0 (unknown).
            mapOf( // Single entry.
                date to DailyStats( // Handbook: totalSeconds 0 for migrated rows.
                    date = date, // Parsed legacy date.
                    attempts = 1, // Treat as one attempt.
                    totalSeconds = 0, // Duration was not stored.
                    bestLevel = legacyBest, // Old best_level.
                ),
            ),
        )
    }

    private companion object { // Prefs JSON field names; file name lives in DataModule.
        const val KEY_DAILY_STATS = "daily_stats" // JSON array string.
        const val KEY_DATE = "best_date" // Legacy date key.
        const val KEY_BEST = "best_level" // Legacy level key.
        const val JSON_DATE = "date" // Object field.
        const val JSON_ATTEMPTS = "attempts" // Object field.
        const val JSON_TOTAL_SECONDS = "totalSeconds" // Object field.
        const val JSON_BEST_LEVEL = "bestLevel" // Object field.
    }
}
