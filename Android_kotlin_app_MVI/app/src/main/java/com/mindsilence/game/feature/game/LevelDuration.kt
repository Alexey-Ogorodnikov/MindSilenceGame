package com.mindsilence.game.feature.game // Level length and full-attempt seconds.

/** Seconds for [level]: 5, 10, then 15, 30, 60, … (`15 shl (level - 3)`). Level ≤ 0 is 0. */
fun durationForLevel(level: Int): Int = when { // 5, 10, then doubling from 15. No max level.
    level <= 0 -> 0 // Idle uses 0.
    level == 1 -> 5 // First period.
    level == 2 -> 10 // Second period.
    else -> 15 shl (level - 3) // Level 3 is 15s; each later level doubles.
}

/** Full attempt length: sum of completed levels plus elapsed seconds on the current one. */
fun totalSessionSeconds(level: Int, elapsedSecAtLevel: Int): Int { // Used when recording Thought.
    if (level <= 0) return 0 // No session.

    var total = elapsedSecAtLevel // Time already spent on the current level.
    for (currentLevel in 1 until level) { // Add every fully finished level.
        total += durationForLevel(currentLevel) // 5 + 10 + … up to level-1.
    } // End for.
    return total // Persist this as the attempt length.
} // End totalSessionSeconds.
