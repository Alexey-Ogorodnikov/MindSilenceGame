package com.mindsilence.game.navigation // Composes the post-splash screen from AppViewModel flags.

import androidx.compose.runtime.Composable // AppRoute is the post-splash Compose host.
import androidx.compose.runtime.remember // One SharedPreferences repository per composition.
import androidx.compose.ui.Modifier // Forwarded to the visible child screen.
import androidx.compose.ui.platform.LocalContext // Application context for prefs.
import androidx.lifecycle.compose.collectAsStateWithLifecycle // Collect flags without leaking after stop.
import androidx.lifecycle.createSavedStateHandle // Process-death restore for AppViewModel.
import androidx.lifecycle.viewmodel.compose.viewModel // Create AppViewModel in composition.
import com.mindsilence.game.feature.game.GameRoute // Training screen when inTraining.
import com.mindsilence.game.feature.game.SharedPreferencesGameProgressRepository // Device persistence shared by game and highscores.
import com.mindsilence.game.feature.highscores.HighScoresRoute // Daily stats table.
import com.mindsilence.game.feature.menu.MenuRoute // First screen after splash.

/**
 * Post-splash host: owns the progress repository and renders menu, training, or
 * highscores from [AppViewModel] flags. Child screens never see this ViewModel.
 */
@Composable // Host composable; children get lambdas, not this ViewModel.
fun AppRoute( // Chooses menu / training / highscores from two flags.
    modifier: Modifier = Modifier, // Passed through to the visible child.
    viewModel: AppViewModel = viewModel { // Default factory: SavedStateHandle-backed flags.
        AppViewModel(createSavedStateHandle()) // Restore inTraining/showHighScores after process death.
    }, // End viewModel factory.
) { // Start AppRoute body.
    val context = LocalContext.current // Needed to open SharedPreferences.
    val progressRepository = remember(context) { // One repo instance so game writes are visible on highscores.
        SharedPreferencesGameProgressRepository(context.applicationContext) // Application context avoids leaking the Activity.
    } // End remember repository.
    val state = viewModel.state.collectAsStateWithLifecycle().value // Latest flags; highscores wins over training.

    when { // Order matters: highscores first so GameRoute (and its VM) leave composition.
        state.showHighScores -> { // Table on top of training; GameViewModel is destroyed on purpose.
            HighScoresRoute( // Daily stats; Back returns to a new Idle training.
                progressRepository = progressRepository, // Same store the session just wrote.
                onBack = { viewModel.onEvent(AppUiEvent.LeaveHighScores) }, // Clear only showHighScores.
                modifier = modifier, // Fill the host.
            ) // End HighScoresRoute.
        } // End highscores branch.
        state.inTraining -> { // Training after menu (or after leaving highscores).
            GameRoute( // Session UI; navigation effects come back as AppUiEvents.
                progressRepository = progressRepository, // Persist Thought results on device.
                onOpenHighScores = { viewModel.onEvent(AppUiEvent.OpenHighScores) }, // From the session-complete dialog.
                onBack = { viewModel.onEvent(AppUiEvent.LeaveTraining) }, // System Back or Leave training.
                modifier = modifier, // Fill the host.
            ) // End GameRoute.
        } // End training branch.
        else -> { // Default after splash: menu.
            MenuRoute( // Training button and How to train; no highscores from here.
                onOpenTraining = { viewModel.onEvent(AppUiEvent.OpenTraining) }, // Only menu effect that leaves this screen.
                modifier = modifier, // Fill the host.
            ) // End MenuRoute.
        } // End menu branch.
    } // End when.
} // End AppRoute.
