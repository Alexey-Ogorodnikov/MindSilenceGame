package com.mindsilence.game.presentation.splash // Splash MVVM: named onContentMeasured, assisted Boolean.

import com.mindsilence.game.MainDispatcherRule // Virtual Main for delay().
import kotlinx.coroutines.ExperimentalCoroutinesApi // advanceTimeBy.
import kotlinx.coroutines.test.advanceTimeBy // Virtual 2s.
import kotlinx.coroutines.test.runCurrent // Flush the timer job.
import kotlinx.coroutines.test.runTest // Coroutine test scope.
import org.junit.Assert.assertFalse // Restore / after-timer flags.
import org.junit.Assert.assertTrue // Cold-start flags.
import org.junit.Rule // MainDispatcherRule.
import org.junit.Test // JUnit 4.

/** SplashViewModel: cold-start timer, restore skip, and system-splash handoff. */
@OptIn(ExperimentalCoroutinesApi::class) // Virtual time.
class SplashViewModelTest { // Constructs the VM directly; Hilt is not used in unit tests.

    @get:Rule
    val mainDispatcherRule = MainDispatcherRule() // viewModelScope uses TestDispatcher.

    /** Cold start keeps both flags until DurationMs elapses. */
    @Test
    fun `cold start shows branded splash until duration elapses`() = runTest { // Timer.
        val viewModel = SplashViewModel(startWithBrandedSplash = true) // Assisted Boolean, no Hilt.

        assertTrue(viewModel.state.value.showBrandedSplash) // Compose splash on.
        assertTrue(viewModel.state.value.keepSystemSplash) // System frame held.

        advanceTimeBy(SplashDefaults.DurationMs) // 2000 ms.
        runCurrent() // Apply the update.

        assertFalse(viewModel.state.value.showBrandedSplash) // Hand off to AppRoute.
        assertFalse(viewModel.state.value.keepSystemSplash) // System splash can go.
    }

    /** Restore must not show the branded splash or hold the system frame. */
    @Test
    fun `restore skips branded splash`() = runTest { // savedInstanceState != null.
        val viewModel = SplashViewModel(startWithBrandedSplash = false) // Restore.

        assertFalse(viewModel.state.value.showBrandedSplash) // Skip logo.
        assertFalse(viewModel.state.value.keepSystemSplash) // Drop system splash now.
    }

    /** First Compose layout releases the system splash while branded splash stays. */
    @Test
    fun `content measured releases system splash`() = runTest { // onGloballyPositioned.
        val viewModel = SplashViewModel(startWithBrandedSplash = true) // Cold start.

        viewModel.onContentMeasured() // Named MVVM method.
        runCurrent()

        assertTrue(viewModel.state.value.showBrandedSplash) // Timer still running.
        assertFalse(viewModel.state.value.keepSystemSplash) // System frame gone.
    }
}
