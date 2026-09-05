package com.mindsilence.game.feature.menu // Menu MVI: dialog flag + navigate-to-training effect.

import androidx.lifecycle.ViewModel // Menu state survives config change.
import androidx.lifecycle.viewModelScope // Send NavigateToTraining without blocking the click.
import kotlinx.coroutines.channels.Channel // One-shot navigation; not stored in UiState.
import kotlinx.coroutines.flow.Flow // Effects collected by MenuRoute.
import kotlinx.coroutines.flow.MutableStateFlow // Private How-to-train visibility.
import kotlinx.coroutines.flow.StateFlow // Read-only menu state.
import kotlinx.coroutines.flow.asStateFlow // Hide mutation from Compose.
import kotlinx.coroutines.flow.receiveAsFlow // MenuRoute collects effects once.
import kotlinx.coroutines.flow.update // Atomic showHowToTrain writes.
import kotlinx.coroutines.launch // Channel.send from onEvent.

/**
 * Menu MVI: How-to-train dialog visibility in state; opening training is a
 * one-shot [MenuUiEffect] so this screen never talks to [AppViewModel].
 */
class MenuViewModel : ViewModel() { // Default factory; no repository.

    private val _state = MutableStateFlow(MenuUiState()) // Dialog starts closed.
    val state: StateFlow<MenuUiState> = _state.asStateFlow() // MenuScreen reads showHowToTrain.

    private val _effects = Channel<MenuUiEffect>(Channel.BUFFERED) // Buffer so a click is not dropped.
    val effects: Flow<MenuUiEffect> = _effects.receiveAsFlow() // MenuRoute maps this to onOpenTraining.

    fun onEvent(event: MenuUiEvent) { // Single entry from MenuScreen clicks.
        when (event) { // Dialog is state; leaving the menu is an effect.
            MenuUiEvent.OpenTraining -> viewModelScope.launch { // Effect cannot be a state flag.
                _effects.send(MenuUiEffect.NavigateToTraining) // Parent AppRoute sets inTraining.
            } // End OpenTraining launch.
            MenuUiEvent.OpenHowToTrain -> _state.update { it.copy(showHowToTrain = true) } // Show the in-place dialog, not a route.
            MenuUiEvent.DismissHowToTrain -> _state.update { it.copy(showHowToTrain = false) } // OK, scrim, or Back.
        } // End when.
    } // End onEvent.
} // End MenuViewModel.
