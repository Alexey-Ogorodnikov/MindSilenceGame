package com.mindsilence.game.feature.highscores // Highscore Route + Screen: daily list, Back to training.

import androidx.compose.foundation.layout.Arrangement // Gaps between day cards.
import androidx.compose.foundation.layout.Column // Card internals.
import androidx.compose.foundation.layout.PaddingValues // LazyColumn insets.
import androidx.compose.foundation.layout.fillMaxSize // List or empty state fills the pane.
import androidx.compose.foundation.layout.fillMaxWidth // Cards stretch.
import androidx.compose.foundation.layout.padding // Scaffold and card padding.
import androidx.compose.foundation.lazy.LazyColumn // Scrolling day rows.
import androidx.compose.foundation.lazy.items // Keyed by date.
import androidx.compose.material3.Card // One day.
import androidx.compose.material3.CardDefaults // Surface container color.
import androidx.compose.material3.ExperimentalMaterial3Api // TopAppBar.
import androidx.compose.material3.MaterialTheme // Type and colors.
import androidx.compose.material3.Scaffold // Top bar + content.
import androidx.compose.material3.Text // Title, empty, and row copy.
import androidx.compose.material3.TextButton // Back in the top bar.
import androidx.compose.material3.TopAppBar // Highscore title + Back.
import androidx.compose.runtime.Composable // Route collects VM; Screen is stateless.
import androidx.compose.runtime.LaunchedEffect // Collect NavigateBack.
import androidx.compose.ui.Modifier // Layout.
import androidx.compose.ui.res.stringResource // English strings.
import androidx.compose.ui.tooling.preview.Preview // Empty and filled lists.
import androidx.compose.ui.unit.dp // Padding and gaps.
import androidx.lifecycle.compose.collectAsStateWithLifecycle // List snapshot.
import androidx.lifecycle.viewmodel.compose.viewModel // Factory-backed VM.
import com.mindsilence.game.R // strings.
import com.mindsilence.game.feature.game.DailyStats // Preview rows.
import com.mindsilence.game.feature.game.GameProgressRepository // Injected from AppRoute.
import com.mindsilence.game.ui.theme.MindSilenceTheme // Previews.
import java.time.LocalDate // Preview dates.
import java.time.format.DateTimeFormatter // d MMMM yyyy.
import java.util.Locale // ENGLISH dates.

/** Highscore container: collects [HighScoresViewModel] and maps Back out to the parent. */
@Composable // Owns the VM; Screen does not take a ViewModel.
fun HighScoresRoute( // AppRoute passes repo and onBack.
    progressRepository: GameProgressRepository, // Same store Thought just wrote.
    onBack: () -> Unit, // LeaveHighScores.
    modifier: Modifier = Modifier, // Host fill.
    viewModel: HighScoresViewModel = viewModel( // Default: factory with this repo.
        factory = HighScoresViewModelFactory(progressRepository), // Load getDailyStats in init.
    ), // End viewModel().
) { // Start HighScoresRoute body.
    val state = viewModel.state.collectAsStateWithLifecycle().value // dailyStats list.

    LaunchedEffect(viewModel) { // Bind effects to this VM.
        viewModel.effects.collect { effect -> // One-shot Back.
            when (effect) { // Only NavigateBack.
                HighScoresUiEffect.NavigateBack -> onBack() // Parent clears showHighScores.
            } // End when.
        } // End collect.
    } // End LaunchedEffect.

    HighScoresScreen( // Stateless list.
        state = state, // Rows or empty copy.
        onBack = { viewModel.onEvent(HighScoresUiEvent.Back) }, // Top bar Back.
        modifier = modifier, // Fill the host.
    ) // End HighScoresScreen.
} // End HighScoresRoute.

/** Stateless highscore list: per-day attempts, total time, and best level. */
@OptIn(ExperimentalMaterial3Api::class) // TopAppBar API.
@Composable // Dumb UI.
fun HighScoresScreen( // Empty vs LazyColumn of DailyStatsCard.
    state: HighScoresUiState, // Loaded list.
    onBack: () -> Unit, // Back event.
    modifier: Modifier = Modifier, // From Route.
) { // Start HighScoresScreen body.
    val dateFormatter = DateTimeFormatter.ofPattern("d MMMM yyyy", Locale.ENGLISH) // Handbook: English month names.

    Scaffold( // Top bar stays while the list scrolls.
        modifier = modifier.fillMaxSize(), // Fill the window.
        topBar = { // Title + Back; not a nav graph.
            TopAppBar( // Material 3 bar.
                title = { Text(text = stringResource(R.string.high_scores_title)) }, // "Highscore".
                navigationIcon = { // Leading Back.
                    TextButton(onClick = onBack) { // HighScoresUiEvent.Back.
                        Text(text = stringResource(R.string.back)) // "Back".
                    } // End TextButton.
                }, // End navigationIcon.
            ) // End TopAppBar.
        }, // End topBar.
    ) { innerPadding -> // Content below the bar.
        if (state.dailyStats.isEmpty()) { // No attempts yet.
            Text( // Centered empty copy would need extra layout; padding is enough.
                text = stringResource(R.string.high_scores_empty), // "No records yet".
                style = MaterialTheme.typography.bodyLarge, // Body.
                color = MaterialTheme.colorScheme.onBackground.copy(alpha = 0.7f), // Softer than primary text.
                modifier = Modifier // Fill under the bar.
                    .fillMaxSize() // Use the pane.
                    .padding(innerPadding) // Below TopAppBar.
                    .padding(24.dp), // Inset the sentence.
            ) // End empty Text.
        } else { // At least one day.
            LazyColumn( // Scroll days; newest first from the VM.
                modifier = Modifier // Fill under the bar.
                    .fillMaxSize() // Use the pane.
                    .padding(innerPadding), // Below TopAppBar.
                contentPadding = PaddingValues(horizontal = 24.dp, vertical = 16.dp), // List insets.
                verticalArrangement = Arrangement.spacedBy(12.dp), // Gap between cards.
            ) { // Start items.
                items( // Stable keys by ISO date.
                    items = state.dailyStats, // Order from getDailyStats().
                    key = { it.date.toString() }, // One card per calendar day.
                ) { dailyStats -> // One row.
                    DailyStatsCard( // Attempts, time, best level.
                        dailyStats = dailyStats, // Numbers.
                        formattedDate = dailyStats.date.format(dateFormatter), // English long date.
                    ) // End DailyStatsCard.
                } // End items.
            } // End LazyColumn.
        } // End empty vs list.
    } // End Scaffold.
} // End HighScoresScreen.

@Composable // One day’s card.
private fun DailyStatsCard( // Three stats under the date.
    dailyStats: DailyStats, // Source numbers.
    formattedDate: String, // Preformatted English date.
    modifier: Modifier = Modifier, // Optional outer modifier.
) { // Start DailyStatsCard body.
    Card( // Surface container.
        modifier = modifier.fillMaxWidth(), // Stretch to the list width.
        colors = CardDefaults.cardColors( // Slightly lifted surface.
            containerColor = MaterialTheme.colorScheme.surfaceContainerHigh, // Contrast on the canvas.
        ), // End cardColors.
    ) { // Start card content.
        Column( // Date then three lines.
            modifier = Modifier // Inner padding.
                .fillMaxWidth() // Stretch.
                .padding(16.dp), // Card inset.
            verticalArrangement = Arrangement.spacedBy(8.dp), // Line gap.
        ) { // Start column.
            Text( // Day heading.
                text = formattedDate, // e.g. 2 September 2026.
                style = MaterialTheme.typography.titleMedium, // Emphasize the day.
                color = MaterialTheme.colorScheme.primary, // Calm primary.
            ) // End date Text.
            Text( // Attempt count.
                text = stringResource(R.string.daily_attempts, dailyStats.attempts), // "Attempts: N".
                style = MaterialTheme.typography.bodyLarge, // Body.
            ) // End attempts Text.
            Text( // Summed silence as m:ss via string args.
                text = stringResource( // Minutes and seconds split for the format string.
                    R.string.daily_total_time, // "Total time: %d:%02d".
                    dailyStats.totalSeconds / 60, // Minutes.
                    dailyStats.totalSeconds % 60, // Seconds.
                ), // End stringResource.
                style = MaterialTheme.typography.bodyLarge, // Body.
            ) // End time Text.
            Text( // Best level that day.
                text = stringResource(R.string.daily_best_level, dailyStats.bestLevel), // "Best level: N".
                style = MaterialTheme.typography.bodyLarge, // Body.
            ) // End best Text.
        } // End Column.
    } // End Card.
} // End DailyStatsCard.

@Preview(name = "High scores — Empty", showBackground = true, showSystemUi = true) // Empty copy.
@Composable // Preview: no rows.
private fun HighScoresScreenEmptyPreview() { // Default UiState.
    MindSilenceTheme { // App theme.
        HighScoresScreen( // Empty list.
            state = HighScoresUiState(), // dailyStats empty.
            onBack = {}, // Preview stub.
        ) // End HighScoresScreen.
    } // End theme.
} // End HighScoresScreenEmptyPreview.

@Preview(name = "High scores", showBackground = true, showSystemUi = true) // Two sample days.
@Composable // Preview: filled list.
private fun HighScoresScreenPreview() { // Newest day first.
    MindSilenceTheme { // App theme.
        HighScoresScreen( // Two DailyStats.
            state = HighScoresUiState( // Sample rows.
                dailyStats = listOf( // 2 Sep then 1 Sep.
                    DailyStats( // Newer day.
                        date = LocalDate.of(2026, 9, 2), // Sample date.
                        attempts = 3, // Sample count.
                        totalSeconds = 185, // Sample sum.
                        bestLevel = 5, // Sample best.
                    ), // End first DailyStats.
                    DailyStats( // Older day.
                        date = LocalDate.of(2026, 9, 1), // Sample date.
                        attempts = 1, // Sample count.
                        totalSeconds = 45, // Sample sum.
                        bestLevel = 3, // Sample best.
                    ), // End second DailyStats.
                ), // End listOf.
            ), // End HighScoresUiState.
            onBack = {}, // Preview stub.
        ) // End HighScoresScreen.
    } // End theme.
} // End HighScoresScreenPreview.
