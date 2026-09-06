package com.mindsilence.game.presentation.splash // Branded-splash timing lives here, not in MainActivity.

import androidx.lifecycle.ViewModel // Survives config change so the 2s timer is not restarted.
import androidx.lifecycle.viewModelScope // Cancels the splash delay if the Activity is finished.
import dagger.assisted.Assisted // Cold-start vs restore flag from MainActivity.
import dagger.assisted.AssistedFactory // Hilt-generated factory used after super.onCreate.
import dagger.assisted.AssistedInject // Mixes Hilt graph types with the assisted Boolean.
import kotlinx.coroutines.delay // Timed branded splash (SplashDefaults.DurationMs).
import kotlinx.coroutines.flow.MutableStateFlow // Private splash flags.
import kotlinx.coroutines.flow.StateFlow // Read-only flags for Activity/Compose.
import kotlinx.coroutines.flow.asStateFlow // Hide mutation from collectors.
import kotlinx.coroutines.flow.update // Atomic flag writes.
import kotlinx.coroutines.launch // Run the 2s delay off the main callback.

/**
 * Owns branded-splash timing and when to drop the system splash.
 * No Channel effects: [com.mindsilence.game.MainActivity] reads [SplashUiState.keepSystemSplash] for
 * `setKeepOnScreenCondition`. The cold-start Boolean is assisted because Hilt cannot know
 * `savedInstanceState == null`. Not `@HiltViewModel`: Hilt forbids injecting a
 * Hilt ViewModel assisted factory into the Activity; a plain assisted factory
 * plus [androidx.lifecycle.ViewModelProvider] still survives config change.
 */
class SplashViewModel @AssistedInject constructor( // Splash MVVM: state only; Activity reads keepSystemSplash.
    @Assisted startWithBrandedSplash: Boolean, // True on cold start; false after restore/rotation past splash.
) : ViewModel() { // Scoped to MainActivity.

    private val _state = MutableStateFlow( // Initial flags depend on cold start vs restore.
        if (startWithBrandedSplash) { // Cold start: show Compose splash and hold the system frame.
            SplashUiState(showBrandedSplash = true, keepSystemSplash = true) // Both true until measured + timer.
        } else { // Restore: skip branded splash so the user is not stuck on the logo.
            SplashUiState(showBrandedSplash = false, keepSystemSplash = false) // Let the system splash drop immediately.
        },
    )
    val state: StateFlow<SplashUiState> = _state.asStateFlow() // MainActivity and setContent collect this.

    init { // Start the 2s timer only when branded splash is actually shown.
        if (startWithBrandedSplash) { // Restore must not wait 2s on the logo.
            viewModelScope.launch { // Cancelled if the Activity is destroyed before the delay ends.
                delay(SplashDefaults.DurationMs) // Handbook: branded splash lasts 2000 ms.
                _state.update { // Hide Compose splash and release the system splash together.
                    it.copy(showBrandedSplash = false, keepSystemSplash = false) // Hand off to AppRoute.
                }
            }
        }
    }

    /** Compose splash is laid out; drop the system frame now. */
    fun onContentMeasured() { // Only ContentMeasured; no navigation effects.
        _state.update { it.copy(keepSystemSplash = false) } // System splash can go; branded splash stays until the timer.
    }

    /** Hilt-generated factory so [com.mindsilence.game.MainActivity] can pass the cold-start flag. */
    @AssistedFactory // Hilt implements this; unit tests still call the constructor directly.
    interface Factory { // One create method matching the @Assisted parameter.
        /**
         * Creates a [SplashViewModel] for cold start or restore.
         *
         * @param startWithBrandedSplash true when `savedInstanceState == null`
         */
        fun create(startWithBrandedSplash: Boolean): SplashViewModel // Assisted Boolean plus any future Hilt deps.
    }
}
