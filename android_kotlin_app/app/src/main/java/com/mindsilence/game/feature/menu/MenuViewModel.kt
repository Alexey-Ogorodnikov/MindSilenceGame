package com.mindsilence.game.feature.menu

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import kotlinx.coroutines.channels.Channel
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.receiveAsFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch

class MenuViewModel : ViewModel() {

    private val _state = MutableStateFlow(MenuUiState())
    val state: StateFlow<MenuUiState> = _state.asStateFlow()

    private val _effects = Channel<MenuUiEffect>(Channel.BUFFERED)
    val effects: Flow<MenuUiEffect> = _effects.receiveAsFlow()

    fun onEvent(event: MenuUiEvent) {
        when (event) {
            MenuUiEvent.OpenTraining -> viewModelScope.launch {
                _effects.send(MenuUiEffect.NavigateToTraining)
            }
            MenuUiEvent.OpenHowToTrain -> _state.update { it.copy(showHowToTrain = true) }
            MenuUiEvent.DismissHowToTrain -> _state.update { it.copy(showHowToTrain = false) }
        }
    }
}
