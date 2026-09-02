package com.mindsilence.game.feature.highscores

import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import com.mindsilence.game.feature.game.GameProgressRepository

class HighScoresViewModelFactory(
    private val progressRepository: GameProgressRepository,
) : ViewModelProvider.Factory {

    @Suppress("UNCHECKED_CAST")
    override fun <T : ViewModel> create(modelClass: Class<T>): T {
        if (modelClass.isAssignableFrom(HighScoresViewModel::class.java)) {
            return HighScoresViewModel(progressRepository) as T
        }
        throw IllegalArgumentException("Unknown ViewModel class: ${modelClass.name}")
    }
}
