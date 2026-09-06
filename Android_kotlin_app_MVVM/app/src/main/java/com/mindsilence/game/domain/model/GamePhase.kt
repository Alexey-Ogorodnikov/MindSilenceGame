package com.mindsilence.game.domain.model // Session phase is a domain concept, not a Compose concern.

/** Session phase: waiting to start, or a silence attempt in progress. There is no pause. */
enum class GamePhase { // Only two phases; background keeps Running.
    Idle, // Start enabled; Thought ignored.
    Running, // Tick running; Thought ends the attempt.
}
