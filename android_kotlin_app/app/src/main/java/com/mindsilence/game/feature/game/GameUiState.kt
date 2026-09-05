package com.mindsilence.game.feature.game

import androidx.compose.runtime.Immutable

enum class GamePhase {
    Idle,
    Running,
}

@Immutable
data class SessionSummary(
    val levelReached: Int,
    val bestToday: Int,
    val totalSeconds: Int,
)

@Immutable
data class GameUiState(
    val phase: GamePhase = GamePhase.Idle,
    val level: Int = 0,
    val elapsedSecAtLevel: Int = 0,
    val sessionSummary: SessionSummary? = null,
) {
    val requiredSecAtLevel: Int
        get() = durationForLevel(level)

    val progressFraction: Float
        get() {
            val required = requiredSecAtLevel
            if (required <= 0) return 0f
            return (elapsedSecAtLevel.toFloat() / required).coerceIn(0f, 1f)
        }
}
