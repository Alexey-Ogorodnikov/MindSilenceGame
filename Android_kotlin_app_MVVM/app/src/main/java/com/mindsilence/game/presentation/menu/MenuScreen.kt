package com.mindsilence.game.presentation.menu // Menu Route + Screen: training button and How to train.

import androidx.compose.foundation.layout.Arrangement // Gap between training button and info.
import androidx.compose.foundation.layout.Box // Centers the row on the screen.
import androidx.compose.foundation.layout.Row // Training button + balancing spacers/info.
import androidx.compose.foundation.layout.Spacer // Same width as the info button so the training label stays centered.
import androidx.compose.foundation.layout.fillMaxSize // Menu fills the window.
import androidx.compose.foundation.layout.padding // Horizontal inset and scaffold padding.
import androidx.compose.foundation.layout.size // Info button and matching spacer.
import androidx.compose.foundation.layout.sizeIn // Minimum training-button tap target.
import androidx.compose.material3.AlertDialog // How to train copy; not a separate route.
import androidx.compose.material3.Button // Mind Silence Training.
import androidx.compose.material3.Icon // Info glyph.
import androidx.compose.material3.IconButton // Opens How to train.
import androidx.compose.material3.MaterialTheme // Label/body styles and primary tint.
import androidx.compose.material3.Scaffold // Edge-to-edge padding for the menu.
import androidx.compose.material3.Text // Button and dialog strings from resources.
import androidx.compose.material3.TextButton // Dialog OK.
import androidx.compose.runtime.Composable // Route collects VM; Screen is stateless.
import androidx.compose.runtime.LaunchedEffect // Collect NavigateToTraining once per VM.
import androidx.compose.ui.Alignment // Center the row.
import androidx.compose.ui.Modifier // Layout modifiers.
import androidx.compose.ui.res.painterResource // ic_info.
import androidx.compose.ui.res.stringResource // English UI strings.
import androidx.compose.ui.tooling.preview.Preview // Idle and How-to-train previews.
import androidx.compose.ui.unit.dp // Spacing and min sizes.
import androidx.hilt.navigation.compose.hiltViewModel // Hilt MenuViewModel; no handwritten factory.
import androidx.lifecycle.compose.collectAsStateWithLifecycle // Menu flags, paused when stopped.
import com.mindsilence.game.R // strings and ic_info.
import com.mindsilence.game.presentation.theme.MindSilenceTheme // Previews.

/** Menu container: collects [MenuViewModel] state/effects and maps navigation out. */
@Composable // Owns the VM; MenuScreen does not take a ViewModel.
fun MenuRoute( // AppRoute passes onOpenTraining; this screen never sees AppViewModel.
    onOpenTraining: () -> Unit, // Becomes AppViewModel.openTraining() in AppRoute.
    modifier: Modifier = Modifier, // Host fill.
    viewModel: MenuViewModel = hiltViewModel(), // Hilt constructs MenuViewModel.
) { // Start MenuRoute body.
    val state = viewModel.state.collectAsStateWithLifecycle().value // showHowToTrain.

    LaunchedEffect(viewModel) { // Bind effects to this VM instance.
        viewModel.effects.collect { effect -> // One-shot navigation.
            when (effect) { // Only NavigateToTraining exists.
                MenuUiEffect.NavigateToTraining -> onOpenTraining() // Leave menu for training.
            } // End when.
        } // End collect.
    } // End LaunchedEffect.

    MenuScreen( // Stateless layout + lambdas.
        state = state, // Dialog visibility.
        onTrainingClick = { viewModel.onOpenTraining() }, // Effect, not a state flag.
        onHowToTrainClick = { viewModel.onOpenHowToTrain() }, // Open dialog.
        onDismissHowToTrain = { viewModel.onDismissHowToTrain() }, // Close dialog.
        modifier = modifier, // Fill the host.
    ) // End MenuScreen.
} // End MenuRoute.

/** Stateless menu layout: training button, How to train dialog, no ViewModel in params. */
@Composable // Dumb UI; events go out as lambdas.
fun MenuScreen( // Centered training CTA plus info.
    state: MenuUiState, // Whether to show HowToTrainDialog.
    onTrainingClick: () -> Unit, // Training button.
    onHowToTrainClick: () -> Unit, // Info button.
    onDismissHowToTrain: () -> Unit, // Dialog dismiss.
    modifier: Modifier = Modifier, // From MenuRoute.
) { // Start MenuScreen body.
    Scaffold(modifier = modifier.fillMaxSize()) { innerPadding -> // Respect system insets.
        Box( // Center the controls.
            modifier = Modifier // Full pane inside the scaffold.
                .fillMaxSize() // Use the window.
                .padding(innerPadding) // Status/nav bars.
                .padding(horizontal = 24.dp), // Side inset so the row is not edge-to-edge.
            contentAlignment = Alignment.Center, // Vertical and horizontal center.
        ) { // Start centered box.
            Row( // Spacer + Training + info so the label stays visually centered.
                horizontalArrangement = Arrangement.spacedBy(8.dp), // Gap between the three slots.
                verticalAlignment = Alignment.CenterVertically, // Align button and icon.
            ) { // Start row.
                Spacer(modifier = Modifier.size(MenuInfoButtonSize)) // Balance the info button on the right.
                Button( // Primary CTA: start a session.
                    onClick = onTrainingClick, // OpenTraining event.
                    modifier = Modifier.sizeIn(minWidth = 120.dp, minHeight = 48.dp), // Comfortable tap target.
                ) { // Start button content.
                    Text( // Label from strings.xml.
                        text = stringResource(R.string.menu_training), // "Mind Silence Training".
                        style = MaterialTheme.typography.labelLarge, // Button type.
                    ) // End Text.
                } // End Button.
                IconButton( // How to train; not a separate screen.
                    onClick = onHowToTrainClick, // OpenHowToTrain.
                    modifier = Modifier.size(MenuInfoButtonSize), // Match the left spacer.
                ) { // Start icon button content.
                    Icon( // Info glyph.
                        painter = painterResource(R.drawable.ic_info), // Vector in res.
                        contentDescription = stringResource(R.string.menu_how_to_train_cd), // A11y name.
                        tint = MaterialTheme.colorScheme.primary, // Calm primary, not on-surface.
                    ) // End Icon.
                } // End IconButton.
            } // End Row.
        } // End Box.
    } // End Scaffold.

    if (state.showHowToTrain) { // Dialog is overlay state, not a route.
        HowToTrainDialog(onDismiss = onDismissHowToTrain) // OK / scrim / Back all dismiss.
    } // End How to train branch.
} // End MenuScreen.

@Composable // Private dialog; only MenuScreen shows it.
private fun HowToTrainDialog( // Copy from strings.xml; English only.
    onDismiss: () -> Unit, // DismissHowToTrain.
) { // Start HowToTrainDialog body.
    AlertDialog( // Material dialog; not a new destination.
        onDismissRequest = onDismiss, // Scrim and system Back.
        title = { // Dialog title slot.
            Text(text = stringResource(R.string.how_to_train_title)) // "How to train".
        }, // End title.
        text = { // Body slot.
            Text( // Session rules in English.
                text = stringResource(R.string.how_to_train_body), // Silence / Thought copy.
                style = MaterialTheme.typography.bodyLarge, // Readable body.
            ) // End Text.
        }, // End text.
        confirmButton = { // Single OK; no extra actions.
            TextButton(onClick = onDismiss) { // Same dismiss as scrim.
                Text(text = stringResource(R.string.ok)) // "OK".
            } // End TextButton.
        }, // End confirmButton.
    ) // End AlertDialog.
} // End HowToTrainDialog.

private val MenuInfoButtonSize = 48.dp // Info button and left spacer share this so the CTA stays centered.

@Preview(name = "Menu", showBackground = true, showSystemUi = true) // Idle menu.
@Composable // Preview: dialog closed.
private fun MenuScreenPreview() { // Default MenuUiState.
    MindSilenceTheme { // App theme for preview chrome.
        MenuScreen( // No-op clicks.
            state = MenuUiState(), // Dialog hidden.
            onTrainingClick = {}, // Preview stub.
            onHowToTrainClick = {}, // Preview stub.
            onDismissHowToTrain = {}, // Preview stub.
        ) // End MenuScreen.
    } // End theme.
} // End MenuScreenPreview.

@Preview(name = "Menu — How to train", showBackground = true, showSystemUi = true) // Dialog open.
@Composable // Preview: dialog shown.
private fun MenuScreenHowToTrainPreview() { // showHowToTrain true.
    MindSilenceTheme { // App theme for preview chrome.
        MenuScreen( // Dialog visible.
            state = MenuUiState(showHowToTrain = true), // Opens HowToTrainDialog.
            onTrainingClick = {}, // Preview stub.
            onHowToTrainClick = {}, // Preview stub.
            onDismissHowToTrain = {}, // Preview stub.
        ) // End MenuScreen.
    } // End theme.
} // End MenuScreenHowToTrainPreview.
