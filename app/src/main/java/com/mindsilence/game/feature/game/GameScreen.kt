package com.mindsilence.game.feature.game

import android.view.WindowManager
import androidx.activity.compose.BackHandler
import androidx.compose.foundation.Image
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.offset
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.requiredSize
import androidx.compose.foundation.layout.sizeIn
import androidx.compose.material3.AlertDialog
import androidx.compose.material3.Button
import androidx.compose.material3.ButtonDefaults
import androidx.compose.material3.LinearProgressIndicator
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
import androidx.compose.runtime.DisposableEffect
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.remember
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.geometry.Rect
import androidx.compose.ui.hapticfeedback.HapticFeedbackType
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.platform.LocalDensity
import androidx.compose.ui.platform.LocalHapticFeedback
import androidx.compose.ui.res.painterResource
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.semantics.contentDescription
import androidx.compose.ui.semantics.semantics
import androidx.compose.ui.text.TextLayoutResult
import androidx.compose.ui.text.rememberTextMeasurer
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.IntOffset
import androidx.compose.ui.unit.dp
import kotlin.math.max
import kotlin.math.min
import kotlin.math.roundToInt
import androidx.lifecycle.Lifecycle
import androidx.lifecycle.LifecycleEventObserver
import androidx.lifecycle.compose.LocalLifecycleOwner
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import androidx.lifecycle.viewmodel.compose.viewModel
import com.mindsilence.game.R

@Composable
fun GameRoute(
    progressRepository: GameProgressRepository,
    onOpenHighScores: () -> Unit,
    onBack: () -> Unit,
    modifier: Modifier = Modifier,
) {
    BackHandler(onBack = onBack)

    val viewModel: GameViewModel = viewModel(
        factory = GameViewModelFactory(progressRepository),
    )
    val state = viewModel.state.collectAsStateWithLifecycle().value
    val lifecycleOwner = LocalLifecycleOwner.current
    val context = LocalContext.current
    val haptic = LocalHapticFeedback.current

    DisposableEffect(lifecycleOwner) {
        val observer = LifecycleEventObserver { _, event ->
            when (event) {
                Lifecycle.Event.ON_STOP -> viewModel.onEvent(GameUiEvent.AppBackgrounded)
                Lifecycle.Event.ON_START -> viewModel.onEvent(GameUiEvent.AppForegrounded)
                else -> Unit
            }
        }
        lifecycleOwner.lifecycle.addObserver(observer)
        onDispose { lifecycleOwner.lifecycle.removeObserver(observer) }
    }

    LaunchedEffect(viewModel) {
        viewModel.effects.collect { effect ->
            when (effect) {
                GameUiEffect.HapticOnThought -> {
                    haptic.performHapticFeedback(HapticFeedbackType.LongPress)
                }
                is GameUiEffect.KeepScreenOn -> {
                    val activity = context.findActivity() ?: return@collect
                    if (effect.enabled) {
                        activity.window.addFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON)
                    } else {
                        activity.window.clearFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON)
                    }
                }
            }
        }
    }

    GameScreen(
        state = state,
        onStartClick = { viewModel.onEvent(GameUiEvent.Start) },
        onThoughtClick = { viewModel.onEvent(GameUiEvent.Thought) },
        onDismissSessionSummary = { viewModel.onEvent(GameUiEvent.DismissSessionSummary) },
        onOpenHighScores = {
            viewModel.onEvent(GameUiEvent.DismissSessionSummary)
            onOpenHighScores()
        },
        modifier = modifier,
    )
}

@Composable
fun GameScreen(
    state: GameUiState,
    onStartClick: () -> Unit,
    onThoughtClick: () -> Unit,
    onDismissSessionSummary: () -> Unit,
    onOpenHighScores: () -> Unit,
    modifier: Modifier = Modifier,
) {
    val startDescription = stringResource(R.string.start_content_description)
    val thoughtDescription = stringResource(R.string.thought_content_description)

    Scaffold(modifier = modifier.fillMaxSize()) { innerPadding ->
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(innerPadding)
                .padding(horizontal = 24.dp, vertical = 32.dp),
            horizontalAlignment = Alignment.CenterHorizontally,
            verticalArrangement = Arrangement.SpaceBetween,
        ) {
            Spacer(modifier = Modifier.height(48.dp))

            Column(
                horizontalAlignment = Alignment.CenterHorizontally,
                verticalArrangement = Arrangement.spacedBy(16.dp),
            ) {
                Text(
                    text = stringResource(R.string.level_label),
                    style = MaterialTheme.typography.titleLarge,
                    color = MaterialTheme.colorScheme.onBackground.copy(alpha = 0.8f),
                )
                LevelFocus(
                    text = if (state.phase == GamePhase.Running) {
                        state.level.toString()
                    } else {
                        stringResource(R.string.level_idle)
                    },
                )

                if (state.phase == GamePhase.Running) {
                    LinearProgressIndicator(
                        progress = { state.progressFraction },
                        modifier = Modifier
                            .fillMaxWidth()
                            .padding(top = 8.dp),
                    )
                    Text(
                        text = stringResource(
                            R.string.level_progress,
                            state.elapsedSecAtLevel,
                            state.requiredSecAtLevel,
                        ),
                        style = MaterialTheme.typography.bodyLarge,
                        color = MaterialTheme.colorScheme.onBackground.copy(alpha = 0.7f),
                    )
                }
            }

            Row(
                modifier = Modifier.fillMaxWidth(),
                horizontalArrangement = Arrangement.spacedBy(16.dp, Alignment.CenterHorizontally),
            ) {
                Button(
                    onClick = onStartClick,
                    enabled = state.phase == GamePhase.Idle,
                    modifier = Modifier
                        .sizeIn(minWidth = 120.dp, minHeight = 48.dp)
                        .semantics {
                            contentDescription = startDescription
                        },
                ) {
                    Text(text = stringResource(R.string.start))
                }
                Button(
                    onClick = onThoughtClick,
                    enabled = state.phase == GamePhase.Running,
                    colors = ButtonDefaults.buttonColors(
                        containerColor = MaterialTheme.colorScheme.secondary,
                    ),
                    modifier = Modifier
                        .sizeIn(minWidth = 120.dp, minHeight = 48.dp)
                        .semantics {
                            contentDescription = thoughtDescription
                        },
                ) {
                    Text(text = stringResource(R.string.thought))
                }
            }
        }
    }

    state.sessionSummary?.let { summary ->
        SessionSummaryDialog(
            summary = summary,
            onDismiss = onDismissSessionSummary,
            onOpenHighScores = onOpenHighScores,
        )
    }
}

@Composable
private fun LevelFocus(
    text: String,
    modifier: Modifier = Modifier,
) {
    val textMeasurer = rememberTextMeasurer()
    val displayLarge = MaterialTheme.typography.displayLarge
    val textStyle = displayLarge.copy(
        color = MaterialTheme.colorScheme.primary,
        fontSize = displayLarge.fontSize * 0.75f,
        lineHeight = displayLarge.lineHeight * 0.75f,
    )
    val textLayout = remember(text, textStyle, textMeasurer) {
        textMeasurer.measure(text = text, style = textStyle)
    }
    val glyph = textLayout.glyphUnionBounds()
    val ringSizePx = with(LocalDensity.current) { FocusRingSize.toPx() }
    val offsetX = ringSizePx * RingCenterXFraction - glyph.width / 2f - glyph.left
    val offsetY = ringSizePx * RingCenterYFraction - glyph.height / 2f - glyph.top

    Box(modifier = modifier.requiredSize(FocusRingSize)) {
        Image(
            painter = painterResource(R.drawable.circle),
            contentDescription = null,
            modifier = Modifier.fillMaxSize(),
            contentScale = ContentScale.Fit,
        )
        Text(
            text = text,
            style = textStyle,
            textAlign = TextAlign.Center,
            modifier = Modifier.offset {
                IntOffset(offsetX.roundToInt(), offsetY.roundToInt())
            },
        )
    }
}

@Composable
private fun SessionSummaryDialog(
    summary: SessionSummary,
    onDismiss: () -> Unit,
    onOpenHighScores: () -> Unit,
) {
    AlertDialog(
        onDismissRequest = onDismiss,
        title = {
            Text(text = stringResource(R.string.session_summary_title))
        },
        text = {
            Column(verticalArrangement = Arrangement.spacedBy(8.dp)) {
                Text(
                    text = stringResource(
                        R.string.session_level_reached,
                        summary.levelReached,
                    ),
                    style = MaterialTheme.typography.bodyLarge,
                )
                Text(
                    text = stringResource(
                        R.string.session_best_today,
                        summary.bestToday,
                    ),
                    style = MaterialTheme.typography.bodyLarge,
                )
                Text(
                    text = stringResource(
                        R.string.session_attempt_duration,
                        summary.totalSeconds / 60,
                        summary.totalSeconds % 60,
                    ),
                    style = MaterialTheme.typography.bodyLarge,
                )
            }
        },
        confirmButton = {
            TextButton(onClick = onDismiss) {
                Text(text = stringResource(R.string.ok))
            }
        },
        dismissButton = {
            TextButton(onClick = onOpenHighScores) {
                Text(text = stringResource(R.string.highscore))
            }
        },
    )
}

private fun android.content.Context.findActivity(): android.app.Activity? {
    var context = this
    while (context is android.content.ContextWrapper) {
        if (context is android.app.Activity) return context
        context = context.baseContext
    }
    return null
}

private fun TextLayoutResult.glyphUnionBounds(): Rect {
    val length = layoutInput.text.length
    if (length == 0) return Rect.Zero
    var bounds = getBoundingBox(0)
    for (index in 1 until length) {
        val other = getBoundingBox(index)
        bounds = Rect(
            left = min(bounds.left, other.left),
            top = min(bounds.top, other.top),
            right = max(bounds.right, other.right),
            bottom = max(bounds.bottom, other.bottom),
        )
    }
    return bounds
}

private val FocusRingSize = 480.dp

/** Visual center of the neon ring in `circle.png` (pixel centroid / image size). */
private const val RingCenterXFraction = 625.35f / 1254f
private const val RingCenterYFraction = 614.37f / 1254f
