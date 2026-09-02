package com.mindsilence.game.feature.highscores

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material3.Card
import androidx.compose.material3.CardDefaults
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.material3.TopAppBar
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.unit.dp
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import androidx.lifecycle.viewmodel.compose.viewModel
import com.mindsilence.game.R
import com.mindsilence.game.feature.game.DailyStats
import com.mindsilence.game.feature.game.GameProgressRepository
import java.time.format.DateTimeFormatter
import java.util.Locale

@Composable
fun HighScoresRoute(
    progressRepository: GameProgressRepository,
    onBack: () -> Unit,
    modifier: Modifier = Modifier,
    viewModel: HighScoresViewModel = viewModel(
        factory = HighScoresViewModelFactory(progressRepository),
    ),
) {
    val state = viewModel.state.collectAsStateWithLifecycle().value

    HighScoresScreen(
        state = state,
        onBack = onBack,
        modifier = modifier,
    )
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun HighScoresScreen(
    state: HighScoresUiState,
    onBack: () -> Unit,
    modifier: Modifier = Modifier,
) {
    val dateFormatter = DateTimeFormatter.ofPattern("d MMMM yyyy", Locale("ru"))

    Scaffold(
        modifier = modifier.fillMaxSize(),
        topBar = {
            TopAppBar(
                title = { Text(text = stringResource(R.string.high_scores_title)) },
                navigationIcon = {
                    TextButton(onClick = onBack) {
                        Text(text = stringResource(R.string.back))
                    }
                },
            )
        },
    ) { innerPadding ->
        if (state.dailyStats.isEmpty()) {
            Text(
                text = stringResource(R.string.high_scores_empty),
                style = MaterialTheme.typography.bodyLarge,
                color = MaterialTheme.colorScheme.onBackground.copy(alpha = 0.7f),
                modifier = Modifier
                    .fillMaxSize()
                    .padding(innerPadding)
                    .padding(24.dp),
            )
        } else {
            LazyColumn(
                modifier = Modifier
                    .fillMaxSize()
                    .padding(innerPadding),
                contentPadding = PaddingValues(horizontal = 24.dp, vertical = 16.dp),
                verticalArrangement = Arrangement.spacedBy(12.dp),
            ) {
                items(
                    items = state.dailyStats,
                    key = { it.date.toString() },
                ) { dailyStats ->
                    DailyStatsCard(
                        dailyStats = dailyStats,
                        formattedDate = dailyStats.date.format(dateFormatter),
                    )
                }
            }
        }
    }
}

@Composable
private fun DailyStatsCard(
    dailyStats: DailyStats,
    formattedDate: String,
    modifier: Modifier = Modifier,
) {
    Card(
        modifier = modifier.fillMaxWidth(),
        colors = CardDefaults.cardColors(
            containerColor = MaterialTheme.colorScheme.surfaceContainerHigh,
        ),
    ) {
        Column(
            modifier = Modifier
                .fillMaxWidth()
                .padding(16.dp),
            verticalArrangement = Arrangement.spacedBy(8.dp),
        ) {
            Text(
                text = formattedDate,
                style = MaterialTheme.typography.titleMedium,
                color = MaterialTheme.colorScheme.primary,
            )
            Text(
                text = stringResource(R.string.daily_attempts, dailyStats.attempts),
                style = MaterialTheme.typography.bodyLarge,
            )
            Text(
                text = stringResource(
                    R.string.daily_total_time,
                    dailyStats.totalSeconds / 60,
                    dailyStats.totalSeconds % 60,
                ),
                style = MaterialTheme.typography.bodyLarge,
            )
            Text(
                text = stringResource(R.string.daily_best_level, dailyStats.bestLevel),
                style = MaterialTheme.typography.bodyLarge,
            )
        }
    }
}
