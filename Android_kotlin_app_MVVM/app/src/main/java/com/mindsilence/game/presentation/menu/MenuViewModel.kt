package com.mindsilence.game.presentation.menu // Menu MVVM: dialog flag + navigate-to-training effect.

import androidx.lifecycle.ViewModel // Menu state survives config change.
import androidx.lifecycle.viewModelScope // Send NavigateToTraining without blocking the click.
import dagger.hilt.android.lifecycle.HiltViewModel // Hilt constructs this when MenuRoute calls hiltViewModel().
import javax.inject.Inject // No-arg graph constructor.
import kotlinx.coroutines.channels.Channel // One-shot navigation; not stored in UiState.
import kotlinx.coroutines.flow.Flow // Effects collected by MenuRoute.
import kotlinx.coroutines.flow.MutableStateFlow // Private How-to-train visibility.
import kotlinx.coroutines.flow.StateFlow // Read-only menu state.
import kotlinx.coroutines.flow.asStateFlow // Hide mutation from Compose.
import kotlinx.coroutines.flow.receiveAsFlow // MenuRoute collects effects once.
import kotlinx.coroutines.flow.update // Atomic showHowToTrain writes.
import kotlinx.coroutines.launch // Channel.send from onOpenTraining.

/**
 * Menu MVVM: How-to-train dialog visibility in state; opening training is a
 * one-shot [MenuUiEffect] so this screen never talks to AppViewModel.
 */
@HiltViewModel // Default Hilt factory; no repository.
class MenuViewModel @Inject constructor() : ViewModel() { // No constructor args.

    private val _state = MutableStateFlow(MenuUiState()) // Dialog starts closed.
    val state: StateFlow<MenuUiState> = _state.asStateFlow() // MenuScreen reads showHowToTrain.

    private val _effects = Channel<MenuUiEffect>(Channel.BUFFERED) // Buffer so a click is not dropped.
    val effects: Flow<MenuUiEffect> = _effects.receiveAsFlow() // MenuRoute maps this to onOpenTraining.

    /** Training button: one-shot navigate; does not change menu state. */
    fun onOpenTraining() { // Effect cannot be a state flag.
        viewModelScope.launch { // Channel.send is suspending.
            _effects.send(MenuUiEffect.NavigateToTraining) // Parent AppRoute sets inTraining.
        }
    }

    /** Info button: show the in-place How to train dialog. */
    fun onOpenHowToTrain() { // Show the in-place dialog, not a route.
        _state.update { it.copy(showHowToTrain = true) } // Dialog overlay.
    }

    /** OK, scrim, or Back: hide the How to train dialog. */
    fun onDismissHowToTrain() { // Close dialog.
        _state.update { it.copy(showHowToTrain = false) } // OK, scrim, or Back.
    }
}
