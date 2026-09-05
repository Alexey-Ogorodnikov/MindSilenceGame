package com.mindsilence.game.feature.menu

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.sizeIn
import androidx.compose.material3.AlertDialog
import androidx.compose.material3.Button
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.painterResource
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import androidx.lifecycle.viewmodel.compose.viewModel
import com.mindsilence.game.R
import com.mindsilence.game.ui.theme.MindSilenceTheme

@Composable
fun MenuRoute(
    onOpenTraining: () -> Unit,
    modifier: Modifier = Modifier,
    viewModel: MenuViewModel = viewModel(),
) {
    val state = viewModel.state.collectAsStateWithLifecycle().value

    LaunchedEffect(viewModel) {
        viewModel.effects.collect { effect ->
            when (effect) {
                MenuUiEffect.NavigateToTraining -> onOpenTraining()
            }
        }
    }

    MenuScreen(
        state = state,
        onTrainingClick = { viewModel.onEvent(MenuUiEvent.OpenTraining) },
        onHowToTrainClick = { viewModel.onEvent(MenuUiEvent.OpenHowToTrain) },
        onDismissHowToTrain = { viewModel.onEvent(MenuUiEvent.DismissHowToTrain) },
        modifier = modifier,
    )
}

@Composable
fun MenuScreen(
    state: MenuUiState,
    onTrainingClick: () -> Unit,
    onHowToTrainClick: () -> Unit,
    onDismissHowToTrain: () -> Unit,
    modifier: Modifier = Modifier,
) {
    Scaffold(modifier = modifier.fillMaxSize()) { innerPadding ->
        Box(
            modifier = Modifier
                .fillMaxSize()
                .padding(innerPadding)
                .padding(horizontal = 24.dp),
            contentAlignment = Alignment.Center,
        ) {
            Row(
                horizontalArrangement = Arrangement.spacedBy(8.dp),
                verticalAlignment = Alignment.CenterVertically,
            ) {
                Spacer(modifier = Modifier.size(MenuInfoButtonSize))
                Button(
                    onClick = onTrainingClick,
                    modifier = Modifier.sizeIn(minWidth = 120.dp, minHeight = 48.dp),
                ) {
                    Text(
                        text = stringResource(R.string.menu_training),
                        style = MaterialTheme.typography.labelLarge,
                    )
                }
                IconButton(
                    onClick = onHowToTrainClick,
                    modifier = Modifier.size(MenuInfoButtonSize),
                ) {
                    Icon(
                        painter = painterResource(R.drawable.ic_info),
                        contentDescription = stringResource(R.string.menu_how_to_train_cd),
                        tint = MaterialTheme.colorScheme.primary,
                    )
                }
            }
        }
    }

    if (state.showHowToTrain) {
        HowToTrainDialog(onDismiss = onDismissHowToTrain)
    }
}

@Composable
private fun HowToTrainDialog(
    onDismiss: () -> Unit,
) {
    AlertDialog(
        onDismissRequest = onDismiss,
        title = {
            Text(text = stringResource(R.string.how_to_train_title))
        },
        text = {
            Text(
                text = stringResource(R.string.how_to_train_body),
                style = MaterialTheme.typography.bodyLarge,
            )
        },
        confirmButton = {
            TextButton(onClick = onDismiss) {
                Text(text = stringResource(R.string.ok))
            }
        },
    )
}

private val MenuInfoButtonSize = 48.dp

@Preview(name = "Menu", showBackground = true, showSystemUi = true)
@Composable
private fun MenuScreenPreview() {
    MindSilenceTheme {
        MenuScreen(
            state = MenuUiState(),
            onTrainingClick = {},
            onHowToTrainClick = {},
            onDismissHowToTrain = {},
        )
    }
}

@Preview(name = "Menu — How to train", showBackground = true, showSystemUi = true)
@Composable
private fun MenuScreenHowToTrainPreview() {
    MindSilenceTheme {
        MenuScreen(
            state = MenuUiState(showHowToTrain = true),
            onTrainingClick = {},
            onHowToTrainClick = {},
            onDismissHowToTrain = {},
        )
    }
}
