package com.mindsilence.game.feature.game // Factory so GameRoute can inject the shared repository.

import androidx.lifecycle.ViewModel // create() return type.
import androidx.lifecycle.ViewModelProvider // No Hilt.

/** Creates [GameViewModel] with a [GameProgressRepository] (device prefs or in-memory). */
class GameViewModelFactory( // GameRoute passes the AppRoute-owned repo.
    private val progressRepository: GameProgressRepository, // Same instance highscores will read.
) : ViewModelProvider.Factory { // viewModel(factory = …).

    @Suppress("UNCHECKED_CAST") // Generic create() cast.
    override fun <T : ViewModel> create(modelClass: Class<T>): T { // Provider lookup.
        if (modelClass.isAssignableFrom(GameViewModel::class.java)) { // Only this VM.
            return GameViewModel(progressRepository) as T // Forward the repo.
        } // End type check.
        throw IllegalArgumentException("Unknown ViewModel class: ${modelClass.name}") // Wrong class.
    } // End create.
} // End GameViewModelFactory.
