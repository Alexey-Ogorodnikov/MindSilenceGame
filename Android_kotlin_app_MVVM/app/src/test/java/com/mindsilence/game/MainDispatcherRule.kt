package com.mindsilence.game // Shared test dispatcher so ViewModel coroutines run under runTest.

import kotlinx.coroutines.Dispatchers // Main dispatcher swapped for tests.
import kotlinx.coroutines.ExperimentalCoroutinesApi // setMain / resetMain.
import kotlinx.coroutines.test.StandardTestDispatcher // Virtual-time dispatcher for delay().
import kotlinx.coroutines.test.TestDispatcher // Injected into setMain.
import kotlinx.coroutines.test.resetMain // Restore Main after each test.
import kotlinx.coroutines.test.setMain // viewModelScope uses Main.
import org.junit.rules.TestWatcher // starting/finished hooks.
import org.junit.runner.Description // Unused except as TestWatcher params.

/** JUnit rule: Main dispatcher is a [TestDispatcher] so splash/game delays are virtual. */
@OptIn(ExperimentalCoroutinesApi::class) // setMain APIs.
class MainDispatcherRule( // Shared by splash, menu, game, and highscore ViewModel tests.
    private val dispatcher: TestDispatcher = StandardTestDispatcher(), // Virtual clock; advanceTimeBy in tests.
) : TestWatcher() { // Applied around each @Test.

    override fun starting(description: Description) { // Before the test body.
        Dispatchers.setMain(dispatcher) // viewModelScope.launch runs on this dispatcher.
    }

    override fun finished(description: Description) { // After the test body.
        Dispatchers.resetMain() // Do not leak the test dispatcher into the next test.
    }

    /** Exposes the test dispatcher when a test needs to advance it explicitly. */
    fun dispatcher(): TestDispatcher = dispatcher // Same instance passed to setMain.
}
