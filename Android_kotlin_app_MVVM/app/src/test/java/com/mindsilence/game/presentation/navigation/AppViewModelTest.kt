package com.mindsilence.game.presentation.navigation // App MVVM: named methods, SavedStateHandle flags.

import androidx.lifecycle.SavedStateHandle // Process-death restore.
import org.junit.Assert.assertEquals // State equality.
import org.junit.Assert.assertFalse // Flag off.
import org.junit.Assert.assertTrue // Flag on.
import org.junit.Test // JUnit 4.

/** AppViewModel: training/highscore flags and SavedStateHandle persistence. */
class AppViewModelTest { // Constructs the VM with a handle; Hilt is not used in unit tests.

    /** Menu training button sets inTraining. */
    @Test
    fun `open training enters training`() { // Menu → training.
        val viewModel = AppViewModel(SavedStateHandle()) // Empty handle.

        viewModel.openTraining() // Named method.

        assertTrue(viewModel.state.value.inTraining) // Training shown.
        assertFalse(viewModel.state.value.showHighScores) // Table hidden.
    }

    /** Leave training returns both flags to menu. */
    @Test
    fun `leave training returns to menu`() { // Training → menu.
        val viewModel = AppViewModel(SavedStateHandle()) // Empty handle.
        viewModel.openTraining() // Enter training.

        viewModel.leaveTraining() // System Back.

        assertFalse(viewModel.state.value.inTraining) // Menu.
        assertFalse(viewModel.state.value.showHighScores) // Table hidden.
    }

    /** Opening highscores keeps inTraining so Back returns to training. */
    @Test
    fun `open high scores keeps training`() { // Overlay table.
        val viewModel = AppViewModel(SavedStateHandle()) // Empty handle.
        viewModel.openTraining() // Must be in training first.

        viewModel.openHighScores() // Session-complete Highscore.

        assertTrue(viewModel.state.value.inTraining) // Stays true.
        assertTrue(viewModel.state.value.showHighScores) // Table shown.
    }

    /** Leaving highscores only clears showHighScores. */
    @Test
    fun `leave high scores returns to training`() { // Table → Idle training.
        val viewModel = AppViewModel(SavedStateHandle()) // Empty handle.
        viewModel.openTraining() // Training.
        viewModel.openHighScores() // Table.

        viewModel.leaveHighScores() // Back.

        assertTrue(viewModel.state.value.inTraining) // Still training.
        assertFalse(viewModel.state.value.showHighScores) // Table hidden.
    }

    /** Missing keys default to menu; present keys restore the same screen. */
    @Test
    fun `restores navigation flags from saved state`() { // Process death.
        val savedStateHandle = SavedStateHandle( // Both keys set.
            mapOf(
                "in_training" to true, // Was in training.
                "show_high_scores" to true, // Table was open.
            ),
        )

        val viewModel = AppViewModel(savedStateHandle) // Reconstruct.

        assertEquals( // Same snapshot.
            AppUiState(inTraining = true, showHighScores = true),
            viewModel.state.value,
        )
    }

    /** Named methods write both keys into the handle. */
    @Test
    fun `events persist flags into saved state handle`() { // Persist.
        val savedStateHandle = SavedStateHandle() // Empty.
        val viewModel = AppViewModel(savedStateHandle) // Fresh.

        viewModel.openTraining() // in_training = true.
        viewModel.openHighScores() // show_high_scores = true.

        assertEquals(true, savedStateHandle["in_training"]) // Persisted.
        assertEquals(true, savedStateHandle["show_high_scores"]) // Persisted.
    }
}
