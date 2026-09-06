package com.mindsilence.game.domain.model // Domain models have no Android or Compose types besides @Immutable.

import androidx.compose.runtime.Immutable // List items are stable snapshots for highscore UI.
import java.time.LocalDate // Local calendar day, not a timestamp.

/** One day's recorded attempts: count, summed silence time, and best level reached. */
@Immutable // Highscore rows do not mutate in place.
data class DailyStats( // JSON fields: date, attempts, totalSeconds, bestLevel.
    val date: LocalDate, // Day key (device local).
    val attempts: Int, // Thoughts recorded that day.
    val totalSeconds: Int, // Sum of attempt lengths.
    val bestLevel: Int, // Max levelReached that day.
)
