package com.mindsilence.game.presentation.game // Training UI snapshot for GameScreen.

import androidx.compose.runtime.Immutable // Stable for Compose equality.
import com.mindsilence.game.domain.model.GamePhase // Idle vs Running.
import com.mindsilence.game.domain.model.SessionSummary // Optional dialog payload.
import com.mindsilence.game.domain.usecase.durationForLevel // 4, 8, 16, … for the current level.

/** Training UI state: phase, current level timer, and optional end-of-session dialog data. */
@Immutable // Screen skips work when the snapshot is unchanged.
data class GameUiState( // Derived duration/progress live as getters.
    val phase: GamePhase = GamePhase.Idle, // Idle until Start.
    val level: Int = 0, // 0 in Idle; 1+ while Running.
    val elapsedSecAtLevel: Int = 0, // Seconds into the current level.
    val sessionSummary: SessionSummary? = null, // Non-null shows the complete dialog.
) { // Start derived properties.
    val requiredSecAtLevel: Int // Seconds to finish this level (4, 8, 16, …).
        get() = durationForLevel(level) // 0 when level is 0 (Idle).

    val progressFraction: Float // Linear bar for the current level only.
        get() { // Avoid divide-by-zero on Idle.
            val required = requiredSecAtLevel // Cached for the two uses below.
            if (required <= 0) return 0f // Idle: empty bar.
            return (elapsedSecAtLevel.toFloat() / required).coerceIn(0f, 1f) // 0..1 for LinearProgressIndicator.
        }
}
