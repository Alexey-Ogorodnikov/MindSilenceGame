package com.mindsilence.game.feature.splash

import com.mindsilence.game.feature.game.MainDispatcherRule
import kotlinx.coroutines.ExperimentalCoroutinesApi
import kotlinx.coroutines.test.advanceTimeBy
import kotlinx.coroutines.test.runCurrent
import kotlinx.coroutines.test.runTest
import org.junit.Assert.assertFalse
import org.junit.Assert.assertTrue
import org.junit.Rule
import org.junit.Test

@OptIn(ExperimentalCoroutinesApi::class)
class SplashViewModelTest {

    @get:Rule
    val mainDispatcherRule = MainDispatcherRule()

    @Test
    fun `cold start shows branded splash until duration elapses`() = runTest {
        val viewModel = SplashViewModel(startWithBrandedSplash = true)

        assertTrue(viewModel.state.value.showBrandedSplash)
        assertTrue(viewModel.state.value.keepSystemSplash)

        advanceTimeBy(SplashDefaults.DurationMs)
        runCurrent()

        assertFalse(viewModel.state.value.showBrandedSplash)
        assertFalse(viewModel.state.value.keepSystemSplash)
    }

    @Test
    fun `restore skips branded splash`() = runTest {
        val viewModel = SplashViewModel(startWithBrandedSplash = false)

        assertFalse(viewModel.state.value.showBrandedSplash)
        assertFalse(viewModel.state.value.keepSystemSplash)
    }

    @Test
    fun `content measured releases system splash`() = runTest {
        val viewModel = SplashViewModel(startWithBrandedSplash = true)

        viewModel.onEvent(SplashUiEvent.ContentMeasured)
        runCurrent()

        assertTrue(viewModel.state.value.showBrandedSplash)
        assertFalse(viewModel.state.value.keepSystemSplash)
    }
}
