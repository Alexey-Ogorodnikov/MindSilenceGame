package com.mindsilence.game.feature.menu

import app.cash.turbine.test
import com.mindsilence.game.feature.game.MainDispatcherRule
import kotlinx.coroutines.test.runCurrent
import kotlinx.coroutines.test.runTest
import org.junit.Assert.assertEquals
import org.junit.Assert.assertFalse
import org.junit.Assert.assertTrue
import org.junit.Rule
import org.junit.Test

class MenuViewModelTest {

    @get:Rule
    val mainDispatcherRule = MainDispatcherRule()

    @Test
    fun `open how to train shows dialog`() = runTest {
        val viewModel = MenuViewModel()

        viewModel.onEvent(MenuUiEvent.OpenHowToTrain)
        runCurrent()

        assertTrue(viewModel.state.value.showHowToTrain)
    }

    @Test
    fun `dismiss how to train hides dialog`() = runTest {
        val viewModel = MenuViewModel()
        viewModel.onEvent(MenuUiEvent.OpenHowToTrain)
        runCurrent()

        viewModel.onEvent(MenuUiEvent.DismissHowToTrain)
        runCurrent()

        assertFalse(viewModel.state.value.showHowToTrain)
    }

    @Test
    fun `dismiss how to train when already hidden is no-op`() = runTest {
        val viewModel = MenuViewModel()

        viewModel.onEvent(MenuUiEvent.DismissHowToTrain)
        runCurrent()

        assertFalse(viewModel.state.value.showHowToTrain)
    }

    @Test
    fun `open training emits navigate effect`() = runTest {
        val viewModel = MenuViewModel()

        viewModel.effects.test {
            viewModel.onEvent(MenuUiEvent.OpenTraining)
            runCurrent()

            assertEquals(MenuUiEffect.NavigateToTraining, awaitItem())
            cancelAndIgnoreRemainingEvents()
        }
    }
}
