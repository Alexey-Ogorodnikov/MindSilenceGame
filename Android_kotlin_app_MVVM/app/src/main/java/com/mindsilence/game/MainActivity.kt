package com.mindsilence.game // Root application package.

import android.os.Bundle // Bundle used to tell cold start from restore.
import androidx.activity.ComponentActivity // Base Activity that hosts Compose.
import androidx.activity.compose.setContent // Puts Compose UI into this Activity.
import androidx.activity.enableEdgeToEdge // Draws under system bars after splash.
import androidx.compose.ui.Modifier // Attaches splash layout callbacks.
import androidx.compose.ui.layout.onGloballyPositioned // Fires when Compose splash is measured.
import androidx.core.splashscreen.SplashScreen.Companion.installSplashScreen // System splash API; must run before super.onCreate.
import androidx.lifecycle.ViewModel // Type token for the assisted factory wrapper.
import androidx.lifecycle.ViewModelProvider // Creates SplashViewModel with the Hilt assisted factory.
import androidx.lifecycle.compose.collectAsStateWithLifecycle // Collects splash flags without leaking after stop.
import com.mindsilence.game.presentation.navigation.AppRoute // Post-splash menu/training/highscores host.
import com.mindsilence.game.presentation.splash.SplashScreen // Branded Compose splash that matches the system frame.
import com.mindsilence.game.presentation.splash.SplashViewModel // Owns branded-splash timing and keepSystemSplash.
import dagger.hilt.android.AndroidEntryPoint // Hilt injects splashViewModelFactory after super.onCreate.
import javax.inject.Inject // Assisted factory from the Hilt graph.

/**
 * App entry. Installs the system splash, then shows Compose splash or [AppRoute]
 * from [SplashViewModel] (cold start vs restore). Hilt injects the assisted factory
 * only after [super.onCreate], so splash install still happens first.
 */
@AndroidEntryPoint // Required so hiltViewModel() in child routes can find the Activity component.
class MainActivity : ComponentActivity() { // Android entry; splash API lives here, not in Compose.

    @Inject // Populated in super.onCreate by Hilt.
    lateinit var splashViewModelFactory: SplashViewModel.Factory // Assisted create(startWithBrandedSplash).

    override fun onCreate(savedInstanceState: Bundle?) { // First callback; splash must be installed here.
        val splashScreen = installSplashScreen() // Before super.onCreate so the system splash can stay.
        enableEdgeToEdge() // Draw under system bars for the rest of the UI.
        super.onCreate(savedInstanceState) // Finish Activity setup; Hilt injects splashViewModelFactory here.

        val startWithBrandedSplash = savedInstanceState == null // Cold start shows branded splash; restore skips it.
        val splashViewModel = ViewModelProvider( // Scope the splash VM to this Activity.
            this, // Activity as ViewModelStoreOwner so restore keeps the same VM.
            splashViewModelFactory.asViewModelProviderFactory(startWithBrandedSplash), // Wrap assisted create in a Factory.
        )[SplashViewModel::class.java] // Resolve the typed SplashViewModel from the provider.

        splashScreen.setKeepOnScreenCondition { splashViewModel.state.value.keepSystemSplash } // Hold the system frame until Compose splash is ready.
        splashScreen.setOnExitAnimationListener { splashScreenView -> // Skip the default zoom so the icon does not jump.
            splashScreenView.remove() // Drop the system splash immediately.
        }

        setContent { // Compose tree: splash or the rest of the app.
            val splashState = splashViewModel.state.collectAsStateWithLifecycle().value // Latest splash flags, paused when stopped.

            MindSilenceApp { // Apply the app theme around whichever screen is shown.
                if (splashState.showBrandedSplash) { // Cold-start window: keep the matching Compose splash on screen.
                    SplashScreen( // White branded splash; same icon slot as the system splash.
                        modifier = Modifier.onGloballyPositioned { // First layout means Compose splash can replace the system frame.
                            splashViewModel.onContentMeasured() // Clear keepSystemSplash so the system splash can go.
                        },
                    )
                } else { // After splash (or restore): show menu/training/highscores.
                    AppRoute() // Post-splash navigation host.
                }
            }
        }
    }
}

/**
 * Adapts [SplashViewModel.Factory] to [ViewModelProvider.Factory] so the Activity can
 * keep the VM in its store across config changes.
 */
private fun SplashViewModel.Factory.asViewModelProviderFactory( // Extension keeps MainActivity.onCreate short.
    startWithBrandedSplash: Boolean, // Forwarded into assisted create.
): ViewModelProvider.Factory { // Anonymous factory used only for this Activity.
    val assistedFactory = this // Capture the Hilt factory so create() is not the ViewModelProvider override.
    return object : ViewModelProvider.Factory { // ViewModelProvider looks up SplashViewModel::class.java.
        override fun <T : ViewModel> create(modelClass: Class<T>): T { // Called once per store key.
            @Suppress("UNCHECKED_CAST") // Factory is only used to obtain SplashViewModel.
            return assistedFactory.create(startWithBrandedSplash) as T // Assisted Hilt factory, not a handwritten VM factory class.
        }
    }
}
