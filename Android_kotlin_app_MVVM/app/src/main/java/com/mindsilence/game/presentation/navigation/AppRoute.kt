package com.mindsilence.game.presentation.navigation // Composes the post-splash screen from AppViewModel flags.

import androidx.compose.runtime.Composable // AppRoute is the post-splash Compose host.
import androidx.compose.ui.Modifier // Forwarded to the visible child screen.
import androidx.hilt.navigation.compose.hiltViewModel // Hilt AppViewModel without a handwritten factory.
import androidx.lifecycle.compose.collectAsStateWithLifecycle // Collect flags without leaking after stop.
import com.mindsilence.game.presentation.game.GameRoute // Training screen when inTraining.
import com.mindsilence.game.presentation.highscores.HighScoresRoute // Daily stats table.
import com.mindsilence.game.presentation.menu.MenuRoute // First screen after splash.

/**
 * Post-splash host: renders menu, training, or highscores from [AppViewModel] flags.
 * Child screens never see this ViewModel. The progress repository is a Hilt singleton,
 * not remembered here.
 */
@Composable // Host composable; children get lambdas, not this ViewModel.
fun AppRoute( // Chooses menu / training / highscores from two flags.
    modifier: Modifier = Modifier, // Passed through to the visible child.
    viewModel: AppViewModel = hiltViewModel(), // SavedStateHandle-backed flags from Hilt.
) { // Start AppRoute body.
    val state = viewModel.state.collectAsStateWithLifecycle().value // Latest flags; highscores wins over training.

    when { // Order matters: highscores first so GameRoute (and its VM) leave composition.
        state.showHighScores -> { // Table on top of training; GameViewModel is not composed.
            HighScoresRoute( // Daily stats; Back returns to a new Idle training.
                onBack = { viewModel.leaveHighScores() }, // Clear only showHighScores.
                modifier = modifier, // Fill the host.
            )
        }
        state.inTraining -> { // Training after menu (or after leaving highscores).
            GameRoute( // Session UI; navigation effects come back as AppViewModel methods.
                onOpenHighScores = { viewModel.openHighScores() }, // From the session-complete dialog.
                onBack = { viewModel.leaveTraining() }, // System Back or Leave training.
                modifier = modifier, // Fill the host.
            )
        }
        else -> { // Default after splash: menu.
            MenuRoute( // Training button and How to train; no highscores from here.
                onOpenTraining = { viewModel.openTraining() }, // Only menu effect that leaves this screen.
                modifier = modifier, // Fill the host.
            )
        }
    }
}
