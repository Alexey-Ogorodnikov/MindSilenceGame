package com.mindsilence.game.feature.game // Level length and full-attempt seconds.

/** Seconds needed to complete [level]: 4, 8, 16, … (`4 shl (level - 1)`). Level ≤ 0 is 0. */
fun durationForLevel(level: Int): Int = // Unbounded doubling; no max level.
    if (level <= 0) 0 else 4 shl (level - 1) // Idle uses 0; level 1 is 4s.

/** Full attempt length: sum of completed levels plus elapsed seconds on the current one. */
fun totalSessionSeconds(level: Int, elapsedSecAtLevel: Int): Int { // Used when recording Thought.
    if (level <= 0) return 0 // No session.

    var total = elapsedSecAtLevel // Time already spent on the current level.
    for (currentLevel in 1 until level) { // Add every fully finished level.
        total += durationForLevel(currentLevel) // 4 + 8 + … up to level-1.
    } // End for.
    return total // Persist this as the attempt length.
} // End totalSessionSeconds.
