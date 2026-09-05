package com.mindsilence.game.feature.highscores // Factory so HighScoresRoute can inject the repo.

import androidx.lifecycle.ViewModel // create() return type.
import androidx.lifecycle.ViewModelProvider // No Hilt.
import com.mindsilence.game.feature.game.GameProgressRepository // Shared with game.

/** Creates [HighScoresViewModel] with the shared [GameProgressRepository]. */
class HighScoresViewModelFactory( // HighScoresRoute passes AppRoute’s repo.
    private val progressRepository: GameProgressRepository, // Reads what Thought just wrote.
) : ViewModelProvider.Factory { // viewModel(factory = …).

    @Suppress("UNCHECKED_CAST") // Generic create() cast.
    override fun <T : ViewModel> create(modelClass: Class<T>): T { // Provider lookup.
        if (modelClass.isAssignableFrom(HighScoresViewModel::class.java)) { // Only this VM.
            return HighScoresViewModel(progressRepository) as T // Forward the repo.
        } // End type check.
        throw IllegalArgumentException("Unknown ViewModel class: ${modelClass.name}") // Wrong class.
    } // End create.
} // End HighScoresViewModelFactory.
