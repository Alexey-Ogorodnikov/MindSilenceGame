package com.mindsilence.game.navigation

import androidx.lifecycle.SavedStateHandle
import org.junit.Assert.assertEquals
import org.junit.Assert.assertFalse
import org.junit.Assert.assertTrue
import org.junit.Test

class AppViewModelTest {

    @Test
    fun `open training enters training`() {
        val viewModel = AppViewModel(SavedStateHandle())

        viewModel.onEvent(AppUiEvent.OpenTraining)

        assertTrue(viewModel.state.value.inTraining)
        assertFalse(viewModel.state.value.showHighScores)
    }

    @Test
    fun `leave training returns to menu`() {
        val viewModel = AppViewModel(SavedStateHandle())
        viewModel.onEvent(AppUiEvent.OpenTraining)

        viewModel.onEvent(AppUiEvent.LeaveTraining)

        assertFalse(viewModel.state.value.inTraining)
        assertFalse(viewModel.state.value.showHighScores)
    }

    @Test
    fun `open high scores keeps training`() {
        val viewModel = AppViewModel(SavedStateHandle())
        viewModel.onEvent(AppUiEvent.OpenTraining)

        viewModel.onEvent(AppUiEvent.OpenHighScores)

        assertTrue(viewModel.state.value.inTraining)
        assertTrue(viewModel.state.value.showHighScores)
    }

    @Test
    fun `leave high scores returns to training`() {
        val viewModel = AppViewModel(SavedStateHandle())
        viewModel.onEvent(AppUiEvent.OpenTraining)
        viewModel.onEvent(AppUiEvent.OpenHighScores)

        viewModel.onEvent(AppUiEvent.LeaveHighScores)

        assertTrue(viewModel.state.value.inTraining)
        assertFalse(viewModel.state.value.showHighScores)
    }

    @Test
    fun `restores navigation flags from saved state`() {
        val savedStateHandle = SavedStateHandle(
            mapOf(
                "in_training" to true,
                "show_high_scores" to true,
            ),
        )

        val viewModel = AppViewModel(savedStateHandle)

        assertEquals(
            AppUiState(inTraining = true, showHighScores = true),
            viewModel.state.value,
        )
    }

    @Test
    fun `events persist flags into saved state handle`() {
        val savedStateHandle = SavedStateHandle()
        val viewModel = AppViewModel(savedStateHandle)

        viewModel.onEvent(AppUiEvent.OpenTraining)
        viewModel.onEvent(AppUiEvent.OpenHighScores)

        assertEquals(true, savedStateHandle["in_training"])
        assertEquals(true, savedStateHandle["show_high_scores"])
    }
}
