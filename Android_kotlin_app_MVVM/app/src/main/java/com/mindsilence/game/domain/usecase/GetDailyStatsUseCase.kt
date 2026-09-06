package com.mindsilence.game.domain.usecase // HighScoresViewModel loads the table through this port.

import com.mindsilence.game.domain.model.DailyStats // Newest-first rows for the highscore screen.
import com.mindsilence.game.domain.repository.GameProgressRepository // Port bound by Hilt to prefs.
import javax.inject.Inject // Hilt constructs this and injects it into HighScoresViewModel.

/**
 * Loads daily training stats for the highscore table.
 * Isolated so [com.mindsilence.game.presentation.highscores.HighScoresViewModel] never imports data/.
 */
class GetDailyStatsUseCase @Inject constructor( // Hilt can construct this from the bound repository.
    private val progressRepository: GameProgressRepository, // Production prefs or test fake.
) { // Start GetDailyStatsUseCase.

    /** Returns days newest-first; empty when nothing has been recorded. */
    operator fun invoke(): List<DailyStats> = // Callable as getDailyStats()
        progressRepository.getDailyStats() // Same sort order as the repository.
}
