package com.mindsilence.game.presentation.menu // Menu MVVM: named methods, not onEvent.

import app.cash.turbine.test // Collect NavigateToTraining.
import com.mindsilence.game.MainDispatcherRule // Virtual Main for Channel.send.
import kotlinx.coroutines.ExperimentalCoroutinesApi // runCurrent opt-in.
import kotlinx.coroutines.test.runCurrent // Flush launches.
import kotlinx.coroutines.test.runTest // Coroutine test scope.
import org.junit.Assert.assertEquals // Effect equality.
import org.junit.Assert.assertFalse // Dialog hidden.
import org.junit.Assert.assertTrue // Dialog shown.
import org.junit.Rule // MainDispatcherRule.
import org.junit.Test // JUnit 4.

/** MenuViewModel: How-to-train dialog flag and navigate-to-training effect. */
@OptIn(ExperimentalCoroutinesApi::class) // runCurrent.
class MenuViewModelTest { // Constructs the VM directly; Hilt is not used in unit tests.

    @get:Rule
    val mainDispatcherRule = MainDispatcherRule() // viewModelScope uses TestDispatcher.

    /** Info button shows the in-place dialog. */
    @Test
    fun `open how to train shows dialog`() = runTest { // Open dialog.
        val viewModel = MenuViewModel() // No-arg Hilt constructor.

        viewModel.onOpenHowToTrain() // Named method.
        runCurrent()

        assertTrue(viewModel.state.value.showHowToTrain) // Dialog on.
    }

    /** OK / scrim / Back hide the dialog. */
    @Test
    fun `dismiss how to train hides dialog`() = runTest { // Close dialog.
        val viewModel = MenuViewModel() // No-arg.
        viewModel.onOpenHowToTrain() // Open first.
        runCurrent()

        viewModel.onDismissHowToTrain() // Close.
        runCurrent()

        assertFalse(viewModel.state.value.showHowToTrain) // Dialog off.
    }

    /** Dismiss when already hidden stays hidden. */
    @Test
    fun `dismiss how to train when already hidden is no-op`() = runTest { // Idempotent.
        val viewModel = MenuViewModel() // Starts hidden.

        viewModel.onDismissHowToTrain() // Already false.
        runCurrent()

        assertFalse(viewModel.state.value.showHowToTrain) // Still false.
    }

    /** Training button emits NavigateToTraining once. */
    @Test
    fun `open training emits navigate effect`() = runTest { // Effect.
        val viewModel = MenuViewModel() // No-arg.

        viewModel.effects.test { // Collect.
            viewModel.onOpenTraining() // Training CTA.
            runCurrent()

            assertEquals(MenuUiEffect.NavigateToTraining, awaitItem()) // Parent opens training.
            cancelAndIgnoreRemainingEvents() // Done.
        }
    }
}
