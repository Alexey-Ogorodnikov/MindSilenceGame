package com.mindsilence.game // Root application package.

import android.os.Bundle // Bundle used to tell cold start from restore.
import androidx.activity.ComponentActivity // Base Activity that hosts Compose.
import androidx.activity.compose.setContent // Puts Compose UI into this Activity.
import androidx.activity.enableEdgeToEdge // Draws under system bars after splash.
import androidx.compose.ui.Modifier // Attaches splash layout callbacks.
import androidx.compose.ui.layout.onGloballyPositioned // Fires when Compose splash is measured.
import androidx.core.splashscreen.SplashScreen.Companion.installSplashScreen // System splash API; must run before super.onCreate.
import androidx.lifecycle.ViewModelProvider // Creates SplashViewModel with a factory.
import androidx.lifecycle.compose.collectAsStateWithLifecycle // Collects splash flags without leaking after stop.
import com.mindsilence.game.feature.splash.SplashScreen // Branded Compose splash that matches the system frame.
import com.mindsilence.game.feature.splash.SplashUiEvent // ContentMeasured tells the VM the Compose splash is laid out.
import com.mindsilence.game.feature.splash.SplashViewModel // Owns branded-splash timing and keepSystemSplash.
import com.mindsilence.game.feature.splash.SplashViewModelFactory // Passes cold-start vs restore into the VM.
import com.mindsilence.game.navigation.AppRoute // Post-splash menu/training/highscores host.

/**
 * App entry. Installs the system splash, then shows Compose splash or [AppRoute]
 * from [SplashViewModel] (cold start vs restore).
 */
class MainActivity : ComponentActivity() { // Android entry; splash API lives here, not in Compose.
    override fun onCreate(savedInstanceState: Bundle?) { // First callback; splash must be installed here.
        val splashScreen = installSplashScreen() // Before super.onCreate so the system splash can stay.
        enableEdgeToEdge() // Draw under system bars for the rest of the UI.
        super.onCreate(savedInstanceState) // Finish Activity setup after splash install.

        val splashViewModel = ViewModelProvider( // Scope the splash VM to this Activity.
            this, // Activity as ViewModelStoreOwner so restore keeps the same VM.
            SplashViewModelFactory(startWithBrandedSplash = savedInstanceState == null), // Cold start shows branded splash; restore skips it.
        )[SplashViewModel::class.java] // Resolve the typed SplashViewModel from the provider.

        splashScreen.setKeepOnScreenCondition { splashViewModel.state.value.keepSystemSplash } // Hold the system frame until Compose splash is ready.
        splashScreen.setOnExitAnimationListener { splashScreenView -> // Skip the default zoom so the icon does not jump.
            splashScreenView.remove() // Drop the system splash immediately.
        } // End exit-animation listener.

        setContent { // Compose tree: splash or the rest of the app.
            val splashState = splashViewModel.state.collectAsStateWithLifecycle().value // Latest splash flags, paused when stopped.

            MindSilenceApp { // Apply the app theme around whichever screen is shown.
                if (splashState.showBrandedSplash) { // Cold-start window: keep the matching Compose splash on screen.
                    SplashScreen( // White branded splash; same icon slot as the system splash.
                        modifier = Modifier.onGloballyPositioned { // First layout means Compose splash can replace the system frame.
                            splashViewModel.onEvent(SplashUiEvent.ContentMeasured) // Clear keepSystemSplash so the system splash can go.
                        }, // End onGloballyPositioned.
                    ) // End SplashScreen call.
                } else { // After splash (or restore): show menu/training/highscores.
                    AppRoute() // Post-splash navigation host.
                } // End splash vs app branch.
            } // End MindSilenceApp.
        } // End setContent.
    } // End onCreate.
} // End MainActivity.
