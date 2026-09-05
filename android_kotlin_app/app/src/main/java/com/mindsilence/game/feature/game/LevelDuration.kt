package com.mindsilence.game.feature.game

fun durationForLevel(level: Int): Int =
    if (level <= 0) 0 else 4 shl (level - 1)

fun totalSessionSeconds(level: Int, elapsedSecAtLevel: Int): Int {
    if (level <= 0) return 0

    var total = elapsedSecAtLevel
    for (currentLevel in 1 until level) {
        total += durationForLevel(currentLevel)
    }
    return total
}
