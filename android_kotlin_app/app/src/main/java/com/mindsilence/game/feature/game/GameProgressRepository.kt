package com.mindsilence.game.feature.game

import android.content.Context
import org.json.JSONArray
import org.json.JSONObject
import java.time.LocalDate

interface GameProgressRepository {
    fun recordSession(levelReached: Int, totalSeconds: Int): Int

    fun getDailyStats(): List<DailyStats>
}

class SharedPreferencesGameProgressRepository(
    context: Context,
) : GameProgressRepository {

    private val prefs = context.getSharedPreferences(PREFS_NAME, Context.MODE_PRIVATE)

    init {
        migrateLegacyIfNeeded()
    }

    override fun recordSession(levelReached: Int, totalSeconds: Int): Int {
        val today = LocalDate.now()
        val statsByDate = loadStats().toMutableMap()
        val current = statsByDate[today]
        val updated = if (current == null) {
            DailyStats(
                date = today,
                attempts = 1,
                totalSeconds = totalSeconds,
                bestLevel = levelReached,
            )
        } else {
            current.copy(
                attempts = current.attempts + 1,
                totalSeconds = current.totalSeconds + totalSeconds,
                bestLevel = maxOf(current.bestLevel, levelReached),
            )
        }
        statsByDate[today] = updated
        saveStats(statsByDate)
        return updated.bestLevel
    }

    override fun getDailyStats(): List<DailyStats> =
        loadStats().values.sortedByDescending { it.date }

    private fun loadStats(): Map<LocalDate, DailyStats> {
        val json = prefs.getString(KEY_DAILY_STATS, null) ?: return emptyMap()
        return runCatching {
            val array = JSONArray(json)
            buildMap {
                for (index in 0 until array.length()) {
                    val item = array.getJSONObject(index)
                    val date = LocalDate.parse(item.getString(JSON_DATE))
                    put(
                        date,
                        DailyStats(
                            date = date,
                            attempts = item.getInt(JSON_ATTEMPTS),
                            totalSeconds = item.getInt(JSON_TOTAL_SECONDS),
                            bestLevel = item.getInt(JSON_BEST_LEVEL),
                        ),
                    )
                }
            }
        }.getOrDefault(emptyMap())
    }

    private fun saveStats(statsByDate: Map<LocalDate, DailyStats>) {
        val array = JSONArray()
        statsByDate.values
            .sortedBy { it.date }
            .forEach { stat ->
                array.put(
                    JSONObject().apply {
                        put(JSON_DATE, stat.date.toString())
                        put(JSON_ATTEMPTS, stat.attempts)
                        put(JSON_TOTAL_SECONDS, stat.totalSeconds)
                        put(JSON_BEST_LEVEL, stat.bestLevel)
                    },
                )
            }
        prefs.edit()
            .putString(KEY_DAILY_STATS, array.toString())
            .remove(KEY_DATE)
            .remove(KEY_BEST)
            .apply()
    }

    private fun migrateLegacyIfNeeded() {
        if (prefs.contains(KEY_DAILY_STATS)) return

        val legacyDate = prefs.getString(KEY_DATE, null) ?: return
        val legacyBest = prefs.getInt(KEY_BEST, 0)
        if (legacyBest <= 0) return

        val date = runCatching { LocalDate.parse(legacyDate) }.getOrNull() ?: return
        saveStats(
            mapOf(
                date to DailyStats(
                    date = date,
                    attempts = 1,
                    totalSeconds = 0,
                    bestLevel = legacyBest,
                ),
            ),
        )
    }

    private companion object {
        const val PREFS_NAME = "game_progress"
        const val KEY_DAILY_STATS = "daily_stats"
        const val KEY_DATE = "best_date"
        const val KEY_BEST = "best_level"
        const val JSON_DATE = "date"
        const val JSON_ATTEMPTS = "attempts"
        const val JSON_TOTAL_SECONDS = "totalSeconds"
        const val JSON_BEST_LEVEL = "bestLevel"
    }
}

class InMemoryGameProgressRepository : GameProgressRepository {

    private val statsByDate = linkedMapOf<LocalDate, DailyStats>()

    override fun recordSession(levelReached: Int, totalSeconds: Int): Int {
        val today = LocalDate.now()
        val current = statsByDate[today]
        val updated = if (current == null) {
            DailyStats(
                date = today,
                attempts = 1,
                totalSeconds = totalSeconds,
                bestLevel = levelReached,
            )
        } else {
            current.copy(
                attempts = current.attempts + 1,
                totalSeconds = current.totalSeconds + totalSeconds,
                bestLevel = maxOf(current.bestLevel, levelReached),
            )
        }
        statsByDate[today] = updated
        return updated.bestLevel
    }

    override fun getDailyStats(): List<DailyStats> =
        statsByDate.values.sortedByDescending { it.date }
}
