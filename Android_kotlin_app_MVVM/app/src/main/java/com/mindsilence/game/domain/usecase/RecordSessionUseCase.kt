package com.mindsilence.game.domain.usecase // GameViewModel persists Thought through this, not the impl.

import com.mindsilence.game.domain.repository.GameProgressRepository // Port bound by Hilt to prefs.
import javax.inject.Inject // Hilt constructs this and injects it into GameViewModel.

/**
 * Records a finished silence attempt and returns today's best level.
 * Isolated so [com.mindsilence.game.presentation.game.GameViewModel] never imports data/.
 */
class RecordSessionUseCase @Inject constructor( // Hilt can construct this from the bound repository.
    private val progressRepository: GameProgressRepository, // Production prefs or test fake.
) { // Start RecordSessionUseCase.

    /**
     * Writes the attempt and returns the best level reached today after the write.
     *
     * @param levelReached level at Thought
     * @param totalSeconds full attempt length from [totalSessionSeconds]
     */
    operator fun invoke(levelReached: Int, totalSeconds: Int): Int = // Callable as recordSession(...)
        progressRepository.recordSession(levelReached, totalSeconds) // Same merge rules as the repository.
}
