package com.mindsilence.game.feature.game

import androidx.compose.runtime.Immutable
import java.time.LocalDate

@Immutable
data class DailyStats(
    val date: LocalDate,
    val attempts: Int,
    val totalSeconds: Int,
    val bestLevel: Int,
)
