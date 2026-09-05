package com.mindsilence.game.navigation // Top-level screen flags live here, not in a NavHost.

import androidx.lifecycle.SavedStateHandle // Survives process death for inTraining/showHighScores.
import androidx.lifecycle.ViewModel // Holds navigation flags for AppRoute.
import kotlinx.coroutines.flow.MutableStateFlow // Private mutable source of AppUiState.
import kotlinx.coroutines.flow.StateFlow // Read-only state for Compose.
import kotlinx.coroutines.flow.asStateFlow // Expose _state without letting UI mutate it.
import kotlinx.coroutines.flow.update // Atomic state writes only through update.

/**
 * Holds which screen is shown after splash (menu, training, or highscores).
 * Flags survive process death via [SavedStateHandle]; there is no navigation graph.
 */
class AppViewModel( // Post-splash host VM: two flags, no Channel effects.
    private val savedStateHandle: SavedStateHandle, // Restore the same screen after process death.
) : ViewModel() { // Survive configuration changes.

    private val _state = MutableStateFlow( // Single source of navigation flags.
        AppUiState( // Rebuild from handle so a killed process returns to the same screen.
            inTraining = savedStateHandle[KEY_IN_TRAINING] ?: false, // Missing key means menu, not training.
            showHighScores = savedStateHandle[KEY_SHOW_HIGH_SCORES] ?: false, // Missing key means highscores closed.
        ), // End initial AppUiState.
    ) // End MutableStateFlow.
    val state: StateFlow<AppUiState> = _state.asStateFlow() // Compose collects this; never mutates _state.

    fun onEvent(event: AppUiEvent) { // Only entry for changing which screen is shown.
        when (event) { // Map each intent to a flag change; OpenHighScores does not clear inTraining.
            AppUiEvent.OpenTraining -> reduce { it.copy(inTraining = true) } // Menu → training.
            AppUiEvent.LeaveTraining -> reduce { it.copy(inTraining = false) } // Training → menu.
            AppUiEvent.OpenHighScores -> reduce { it.copy(showHighScores = true) } // Keep inTraining so Back returns to a new Idle training.
            AppUiEvent.LeaveHighScores -> reduce { it.copy(showHighScores = false) } // Only hide the table; stay in training.
        } // End when.
    } // End onEvent.

    private fun reduce(transform: (AppUiState) -> AppUiState) { // Apply a copy and persist both keys together.
        _state.update { current -> // Atomic write so Compose sees one consistent snapshot.
            val next = transform(current) // New flags from the event.
            savedStateHandle[KEY_IN_TRAINING] = next.inTraining // Persist training so process death restores it.
            savedStateHandle[KEY_SHOW_HIGH_SCORES] = next.showHighScores // Persist highscores the same way.
            next // Publish the new state.
        } // End update.
    } // End reduce.

    private companion object { // SavedStateHandle keys; keep them stable.
        const val KEY_IN_TRAINING = "in_training" // Handle key for the training flag.
        const val KEY_SHOW_HIGH_SCORES = "show_high_scores" // Handle key for the highscores flag.
    } // End companion.
} // End AppViewModel.
