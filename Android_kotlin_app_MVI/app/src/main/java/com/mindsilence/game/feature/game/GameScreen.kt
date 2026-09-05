package com.mindsilence.game.feature.game // Training Route + Screen: ring, Start/Thought, summary dialog.

import android.view.WindowManager // FLAG_KEEP_SCREEN_ON on the Activity window.
import androidx.activity.compose.BackHandler // System Back → LeaveTraining.
import androidx.compose.foundation.Image // circle.png ring.
import androidx.compose.foundation.layout.Arrangement // Column/Row spacing.
import androidx.compose.foundation.layout.Box // Ring + level number overlay.
import androidx.compose.foundation.layout.Column // Vertical training layout.
import androidx.compose.foundation.layout.Row // Start and Thought.
import androidx.compose.foundation.layout.Spacer // Top breathing room.
import androidx.compose.foundation.layout.fillMaxSize // Full pane.
import androidx.compose.foundation.layout.fillMaxWidth // Progress and button row.
import androidx.compose.foundation.layout.height // Top spacer height.
import androidx.compose.foundation.layout.offset // Place the level glyph on the neon centroid.
import androidx.compose.foundation.layout.padding // Insets.
import androidx.compose.foundation.layout.requiredSize // Fixed ring box.
import androidx.compose.foundation.layout.sizeIn // Button min tap target.
import androidx.compose.material3.AlertDialog // Session complete.
import androidx.compose.material3.Button // Start / Thought.
import androidx.compose.material3.ButtonDefaults // Thought uses secondary color.
import androidx.compose.material3.LinearProgressIndicator // Current-level bar (Running only).
import androidx.compose.material3.MaterialTheme // Type and colors.
import androidx.compose.material3.Scaffold // Insets.
import androidx.compose.material3.Text // Labels and dialog copy.
import androidx.compose.material3.TextButton // Dialog OK / Highscore.
import androidx.compose.runtime.Composable // Route vs stateless Screen.
import androidx.compose.runtime.DisposableEffect // Lifecycle observer.
import androidx.compose.runtime.LaunchedEffect // Collect effects.
import androidx.compose.runtime.remember // Measure the level string once per text/style.
import androidx.compose.ui.Alignment // Center ring and buttons.
import androidx.compose.ui.Modifier // Layout and semantics.
import androidx.compose.ui.draw.alpha // Hide Idle progress without dropping layout.
import androidx.compose.ui.geometry.Rect // Glyph union for centering.
import androidx.compose.ui.hapticfeedback.HapticFeedbackType // LongPress on Thought.
import androidx.compose.ui.layout.ContentScale // Fit circle.png.
import androidx.compose.ui.platform.LocalContext // Window flags.
import androidx.compose.ui.platform.LocalDensity // dp ring → px for offset math.
import androidx.compose.ui.platform.LocalHapticFeedback // Thought haptic.
import androidx.compose.ui.res.painterResource // circle.png.
import androidx.compose.ui.res.stringResource // English strings.
import androidx.compose.ui.semantics.clearAndSetSemantics // Hide Idle progress from a11y.
import androidx.compose.ui.semantics.contentDescription // Start/Thought a11y names.
import androidx.compose.ui.semantics.semantics // Apply contentDescription.
import androidx.compose.ui.text.TextLayoutResult // Glyph boxes.
import androidx.compose.ui.text.rememberTextMeasurer // Measure level text for ring centering.
import androidx.compose.ui.text.style.TextAlign // Center the level string.
import androidx.compose.ui.tooling.preview.Preview // Idle / Running / summary.
import androidx.compose.ui.unit.IntOffset // Pixel offset of the level text.
import androidx.compose.ui.unit.dp // Spacing and ring size.
import kotlin.math.max // Glyph union.
import kotlin.math.min // Glyph union.
import kotlin.math.roundToInt // Offset to IntOffset.
import androidx.lifecycle.Lifecycle // ON_STOP / ON_START.
import androidx.lifecycle.LifecycleEventObserver // Forward to VM.
import androidx.lifecycle.compose.LocalLifecycleOwner // Observer target.
import androidx.lifecycle.compose.collectAsStateWithLifecycle // GameUiState.
import androidx.lifecycle.viewmodel.compose.viewModel // GameViewModelFactory.
import com.mindsilence.game.R // strings and circle.
import com.mindsilence.game.ui.theme.MindSilenceTheme // Previews.

/**
 * Training container: wires [GameViewModel], Back, lifecycle pause/resume of the tick,
 * haptic/keep-screen-on, and maps navigation effects to parent callbacks.
 */
@Composable // Owns the VM; GameScreen is stateless.
fun GameRoute( // AppRoute supplies repo and nav lambdas.
    progressRepository: GameProgressRepository, // Persist Thought.
    onOpenHighScores: () -> Unit, // OpenHighScores on the app VM.
    onBack: () -> Unit, // LeaveTraining on the app VM.
    modifier: Modifier = Modifier, // Host fill.
) { // Start GameRoute body.
    val viewModel: GameViewModel = viewModel( // Destroyed when highscores replaces this route.
        factory = GameViewModelFactory(progressRepository), // Shared prefs repo.
    ) // End viewModel().
    val state = viewModel.state.collectAsStateWithLifecycle().value // Phase, level, summary.

    BackHandler { viewModel.onEvent(GameUiEvent.LeaveTraining) } // System Back → menu, not finish Activity.
    val lifecycleOwner = LocalLifecycleOwner.current // ON_STOP / ON_START.
    val context = LocalContext.current // Activity window for keep-screen-on.
    val haptic = LocalHapticFeedback.current // Thought haptic.

    DisposableEffect(lifecycleOwner) { // Register while this route is composed.
        val observer = LifecycleEventObserver { _, event -> // Map lifecycle to GameUiEvents.
            when (event) { // Only stop/start; pause UI does not exist.
                Lifecycle.Event.ON_STOP -> viewModel.onEvent(GameUiEvent.AppBackgrounded) // Pause tick, keep Running.
                Lifecycle.Event.ON_START -> viewModel.onEvent(GameUiEvent.AppForegrounded) // Resume tick if needed.
                else -> Unit // Ignore other events.
            } // End when.
        } // End observer.
        lifecycleOwner.lifecycle.addObserver(observer) // Start listening.
        onDispose { lifecycleOwner.lifecycle.removeObserver(observer) } // Avoid leaks when leaving training.
    } // End DisposableEffect.

    LaunchedEffect(viewModel) { // Collect effects for this VM instance.
        viewModel.effects.collect { effect -> // Haptic, window flag, nav.
            when (effect) { // Exhaustive GameUiEffect.
                GameUiEffect.HapticOnThought -> { // After Thought.
                    haptic.performHapticFeedback(HapticFeedbackType.LongPress) // Confirm the press.
                } // End haptic branch.
                is GameUiEffect.KeepScreenOn -> { // Tick vs idle/background.
                    val activity = context.findActivity() ?: return@collect // Need the Activity window.
                    if (effect.enabled) { // Foreground Running.
                        activity.window.addFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON) // Stay awake while ticking.
                    } else { // Idle, Thought, or background.
                        activity.window.clearFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON) // Allow sleep.
                    } // End enabled branch.
                } // End KeepScreenOn.
                GameUiEffect.NavigateToHighScores -> onOpenHighScores() // After closing summary in the VM.
                GameUiEffect.NavigateBackToMenu -> onBack() // LeaveTraining.
            } // End when.
        } // End collect.
    } // End LaunchedEffect.

    GameScreen( // Stateless layout.
        state = state, // Snapshot.
        onStartClick = { viewModel.onEvent(GameUiEvent.Start) }, // Idle only (VM ignores Running).
        onThoughtClick = { viewModel.onEvent(GameUiEvent.Thought) }, // Running only (VM ignores Idle).
        onDismissSessionSummary = { viewModel.onEvent(GameUiEvent.DismissSessionSummary) }, // Close dialog.
        onOpenHighScores = { viewModel.onEvent(GameUiEvent.OpenHighScores) }, // Dialog Highscore.
        modifier = modifier, // Fill the host.
    ) // End GameScreen.
} // End GameRoute.

/** Stateless training layout: ring, level, Start/Thought, session-complete dialog. */
@Composable // No ViewModel in params.
fun GameScreen( // Ring + buttons + optional summary.
    state: GameUiState, // Phase, level, progress, summary.
    onStartClick: () -> Unit, // Start.
    onThoughtClick: () -> Unit, // Thought.
    onDismissSessionSummary: () -> Unit, // Dialog OK / scrim.
    onOpenHighScores: () -> Unit, // Dialog Highscore.
    modifier: Modifier = Modifier, // From Route.
) { // Start GameScreen body.
    val startDescription = stringResource(R.string.start_content_description) // A11y for Start.
    val thoughtDescription = stringResource(R.string.thought_content_description) // A11y for Thought.

    Scaffold(modifier = modifier.fillMaxSize()) { innerPadding -> // Insets.
        Column( // Top spacer, focus, buttons.
            modifier = Modifier // Full pane.
                .fillMaxSize() // Use the window.
                .padding(innerPadding) // System bars.
                .padding(horizontal = 24.dp, vertical = 32.dp), // Training inset.
            horizontalAlignment = Alignment.CenterHorizontally, // Center ring and row.
            verticalArrangement = Arrangement.SpaceBetween, // Spacer / focus / buttons.
        ) { // Start main column.
            Spacer(modifier = Modifier.height(48.dp)) // Push the ring down from the status bar.

            Column( // Label, ring, progress.
                horizontalAlignment = Alignment.CenterHorizontally, // Center children.
                verticalArrangement = Arrangement.spacedBy(16.dp), // Gap.
            ) { // Start focus column.
                Text( // "Level" heading.
                    text = stringResource(R.string.level_label), // English label.
                    style = MaterialTheme.typography.titleLarge, // Heading.
                    color = MaterialTheme.colorScheme.onBackground.copy(alpha = 0.8f), // Softer than full onBackground.
                ) // End level label.
                LevelFocus( // Ring + number or idle placeholder.
                    text = if (state.phase == GamePhase.Running) { // Show the live level.
                        state.level.toString() // Current level.
                    } else { // Idle: no number in the ring.
                        stringResource(R.string.level_idle) // Placeholder (e.g. "—").
                    }, // End text branch.
                ) // End LevelFocus.

                LevelProgress( // Bar stays in layout so the ring does not jump on Start.
                    progressFraction = state.progressFraction, // 0..1 of this level.
                    elapsedSecAtLevel = state.elapsedSecAtLevel, // Elapsed seconds.
                    requiredSecAtLevel = state.requiredSecAtLevel, // Level duration.
                    visible = state.phase == GamePhase.Running, // Invisible in Idle but still takes space.
                ) // End LevelProgress.
            } // End focus column.

            Row( // Start left, Thought right.
                modifier = Modifier.fillMaxWidth(), // Spread across the pane.
                horizontalArrangement = Arrangement.spacedBy(16.dp, Alignment.CenterHorizontally), // Centered pair.
            ) { // Start button row.
                Button( // Start a silence attempt.
                    onClick = onStartClick, // GameUiEvent.Start.
                    enabled = state.phase == GamePhase.Idle, // Disabled while Running.
                    modifier = Modifier // Tap target + a11y name.
                        .sizeIn(minWidth = 120.dp, minHeight = 48.dp) // Comfortable hit area.
                        .semantics { // Override default button speech.
                            contentDescription = startDescription // Explicit Start description.
                        }, // End semantics.
                ) { // Start label.
                    Text(text = stringResource(R.string.start)) // "Start".
                } // End Start Button.
                Button( // End the attempt when a thought appears.
                    onClick = onThoughtClick, // GameUiEvent.Thought.
                    enabled = state.phase == GamePhase.Running, // Disabled in Idle.
                    colors = ButtonDefaults.buttonColors( // Distinct from Start.
                        containerColor = MaterialTheme.colorScheme.secondary, // Calm green.
                    ), // End colors.
                    modifier = Modifier // Tap target + a11y name.
                        .sizeIn(minWidth = 120.dp, minHeight = 48.dp) // Comfortable hit area.
                        .semantics { // Override default button speech.
                            contentDescription = thoughtDescription // Explicit Thought description.
                        }, // End semantics.
                ) { // Thought label.
                    Text(text = stringResource(R.string.thought)) // "Thought".
                } // End Thought Button.
            } // End Row.
        } // End main Column.
    } // End Scaffold.

    state.sessionSummary?.let { summary -> // Dialog only after Thought.
        SessionSummaryDialog( // Level, best today, duration.
            summary = summary, // Payload from the VM.
            onDismiss = onDismissSessionSummary, // Stay on Idle training.
            onOpenHighScores = onOpenHighScores, // Leave for the table.
        ) // End SessionSummaryDialog.
    } // End summary let.
} // End GameScreen.

@Composable // Current-level bar; kept in tree while Idle (alpha 0) so layout stays put.
private fun LevelProgress( // Hidden from a11y when not Running.
    progressFraction: Float, // 0..1.
    elapsedSecAtLevel: Int, // Elapsed.
    requiredSecAtLevel: Int, // Required.
    visible: Boolean, // Running vs Idle.
    modifier: Modifier = Modifier, // Optional outer modifier.
) { // Start LevelProgress body.
    Column( // Bar + "elapsed / required" text.
        modifier = modifier // Width, fade, a11y.
            .fillMaxWidth() // Full column width.
            .alpha(if (visible) 1f else 0f) // Invisible in Idle, still occupies space.
            .then(if (visible) Modifier else Modifier.clearAndSetSemantics { }), // Idle: not read by TalkBack.
        horizontalAlignment = Alignment.CenterHorizontally, // Center the caption.
        verticalArrangement = Arrangement.spacedBy(16.dp), // Gap under the bar.
    ) { // Start column.
        LinearProgressIndicator( // Determinate bar for this level only.
            progress = { if (visible) progressFraction else 0f }, // Idle draws empty.
            modifier = Modifier // Full width with a little top gap.
                .fillMaxWidth() // Stretch.
                .padding(top = 8.dp), // Separate from the ring.
        ) // End LinearProgressIndicator.
        Text( // Numeric progress.
            text = stringResource( // Format elapsed and required.
                R.string.level_progress, // e.g. "%d / %d s".
                elapsedSecAtLevel, // Elapsed arg.
                requiredSecAtLevel, // Required arg.
            ), // End stringResource.
            style = MaterialTheme.typography.bodyLarge, // Caption.
            color = MaterialTheme.colorScheme.onBackground.copy(alpha = 0.7f), // Softer caption.
        ) // End progress Text.
    } // End Column.
} // End LevelProgress.

@Composable // Neon ring with the level glyph centered on the artwork’s visual center.
private fun LevelFocus( // circle.png is not optically centered in the PNG.
    text: String, // Level number or idle placeholder.
    modifier: Modifier = Modifier, // Optional outer modifier.
) { // Start LevelFocus body.
    val textMeasurer = rememberTextMeasurer() // Measure glyph bounds.
    val displayLarge = MaterialTheme.typography.displayLarge // Base type for the number.
    val textStyle = displayLarge.copy( // Slightly smaller so it sits in the ring.
        color = MaterialTheme.colorScheme.primary, // Calm blue number.
        fontSize = displayLarge.fontSize * 0.75f, // Fit inside the neon.
        lineHeight = displayLarge.lineHeight * 0.75f, // Match the smaller size.
    ) // End textStyle.
    val textLayout = remember(text, textStyle, textMeasurer) { // Re-measure when the level changes.
        textMeasurer.measure(text = text, style = textStyle) // Layout for glyph union.
    } // End remember measure.
    val glyph = textLayout.glyphUnionBounds() // Tight box around ink, not the line box.
    val ringSizePx = with(LocalDensity.current) { FocusRingSize.toPx() } // 480.dp in px.
    val offsetX = ringSizePx * RingCenterXFraction - glyph.width / 2f - glyph.left // Align glyph center to neon X.
    val offsetY = ringSizePx * RingCenterYFraction - glyph.height / 2f - glyph.top // Align glyph center to neon Y.

    Box(modifier = modifier.requiredSize(FocusRingSize)) { // Fixed square so the ring does not resize.
        Image( // Neon circle artwork.
            painter = painterResource(R.drawable.circle), // circle.png.
            contentDescription = null, // Decorative; level text is the a11y content.
            modifier = Modifier.fillMaxSize(), // Fill the 480.dp box.
            contentScale = ContentScale.Fit, // Keep aspect.
        ) // End Image.
        Text( // Level (or idle mark) on the visual center.
            text = text, // Number or placeholder.
            style = textStyle, // Scaled displayLarge.
            textAlign = TextAlign.Center, // Center inside the offset.
            modifier = Modifier.offset { // Pixel offset from the Box origin.
                IntOffset(offsetX.roundToInt(), offsetY.roundToInt()) // Snap to pixels.
            }, // End offset.
        ) // End Text.
    } // End Box.
} // End LevelFocus.

@Composable // End-of-attempt dialog; not a route.
private fun SessionSummaryDialog( // OK stays; Highscore navigates.
    summary: SessionSummary, // From Thought.
    onDismiss: () -> Unit, // DismissSessionSummary.
    onOpenHighScores: () -> Unit, // OpenHighScores.
) { // Start SessionSummaryDialog body.
    AlertDialog( // Material dialog over Idle training.
        onDismissRequest = onDismiss, // Scrim / Back close the dialog only.
        title = { // Title slot.
            Text(text = stringResource(R.string.session_summary_title)) // "Session complete".
        }, // End title.
        text = { // Three stats.
            Column(verticalArrangement = Arrangement.spacedBy(8.dp)) { // Stack lines.
                Text( // Level reached this attempt.
                    text = stringResource( // Format level.
                        R.string.session_level_reached, // "Level reached: %d".
                        summary.levelReached, // This run.
                    ), // End stringResource.
                    style = MaterialTheme.typography.bodyLarge, // Body.
                ) // End level Text.
                Text( // Best level today after recording.
                    text = stringResource( // Format best.
                        R.string.session_best_today, // "Best today: %d".
                        summary.bestToday, // From the repo.
                    ), // End stringResource.
                    style = MaterialTheme.typography.bodyLarge, // Body.
                ) // End best Text.
                Text( // Attempt length as m:ss.
                    text = stringResource( // Minutes and leftover seconds.
                        R.string.session_attempt_duration, // Duration format.
                        summary.totalSeconds / 60, // Minutes.
                        summary.totalSeconds % 60, // Seconds.
                    ), // End stringResource.
                    style = MaterialTheme.typography.bodyLarge, // Body.
                ) // End duration Text.
            } // End Column.
        }, // End text.
        confirmButton = { // Stay on training.
            TextButton(onClick = onDismiss) { // DismissSessionSummary.
                Text(text = stringResource(R.string.ok)) // "OK".
            } // End TextButton.
        }, // End confirmButton.
        dismissButton = { // Leave for highscores (VM clears summary first).
            TextButton(onClick = onOpenHighScores) { // OpenHighScores.
                Text(text = stringResource(R.string.highscore)) // "Highscore".
            } // End TextButton.
        }, // End dismissButton.
    ) // End AlertDialog.
} // End SessionSummaryDialog.

private fun android.content.Context.findActivity(): android.app.Activity? { // Walk wrappers to the Activity for window flags.
    var context = this // Start from LocalContext.
    while (context is android.content.ContextWrapper) { // Unwrap until Activity or base.
        if (context is android.app.Activity) return context // Found the window owner.
        context = context.baseContext // Next wrapper.
    } // End while.
    return null // Preview or non-Activity context.
} // End findActivity.

private fun TextLayoutResult.glyphUnionBounds(): Rect { // Union of per-glyph boxes so "—" and digits center on ink.
    val length = layoutInput.text.length // Character count.
    if (length == 0) return Rect.Zero // Empty string: no offset math.
    var bounds = getBoundingBox(0) // First glyph.
    for (index in 1 until length) { // Expand to include the rest.
        val other = getBoundingBox(index) // Next glyph.
        bounds = Rect( // Axis-aligned union.
            left = min(bounds.left, other.left), // Leftmost.
            top = min(bounds.top, other.top), // Topmost.
            right = max(bounds.right, other.right), // Rightmost.
            bottom = max(bounds.bottom, other.bottom), // Bottommost.
        ) // End Rect.
    } // End for.
    return bounds // Tight ink box.
} // End glyphUnionBounds.

private val FocusRingSize = 480.dp // On-screen ring box; artwork is fitted inside.

/** Visual center of the neon ring in `circle.png` (pixel centroid / image size). */
private const val RingCenterXFraction = 625.35f / 1254f // Neon centroid X / PNG width.
private const val RingCenterYFraction = 614.37f / 1254f // Neon centroid Y / PNG height.

@Preview(name = "Game — Idle", showBackground = true, showSystemUi = true) // Start enabled.
@Composable // Preview: Idle.
private fun GameScreenIdlePreview() { // Default GameUiState.
    MindSilenceTheme { // App theme.
        GameScreen( // Empty Idle.
            state = GameUiState(), // phase Idle.
            onStartClick = {}, // Preview stub.
            onThoughtClick = {}, // Preview stub.
            onDismissSessionSummary = {}, // Preview stub.
            onOpenHighScores = {}, // Preview stub.
        ) // End GameScreen.
    } // End theme.
} // End GameScreenIdlePreview.

@Preview(name = "Game — Running", showBackground = true, showSystemUi = true) // Thought enabled.
@Composable // Preview: Running level 3.
private fun GameScreenRunningPreview() { // Mid-level tick.
    MindSilenceTheme { // App theme.
        GameScreen( // Running snapshot.
            state = GameUiState( // Sample Running.
                phase = GamePhase.Running, // Tick UI.
                level = 3, // Number in the ring.
                elapsedSecAtLevel = 2, // Partial bar.
            ), // End GameUiState.
            onStartClick = {}, // Preview stub.
            onThoughtClick = {}, // Preview stub.
            onDismissSessionSummary = {}, // Preview stub.
            onOpenHighScores = {}, // Preview stub.
        ) // End GameScreen.
    } // End theme.
} // End GameScreenRunningPreview.

@Preview(name = "Game — Session summary", showBackground = true, showSystemUi = true) // Dialog.
@Composable // Preview: summary over Idle.
private fun GameScreenSessionSummaryPreview() { // After Thought.
    MindSilenceTheme { // App theme.
        GameScreen( // Dialog shown.
            state = GameUiState( // Summary only.
                sessionSummary = SessionSummary( // Sample payload.
                    levelReached = 4, // This run.
                    bestToday = 5, // After record.
                    totalSeconds = 90, // 1:30.
                ), // End SessionSummary.
            ), // End GameUiState.
            onStartClick = {}, // Preview stub.
            onThoughtClick = {}, // Preview stub.
            onDismissSessionSummary = {}, // Preview stub.
            onOpenHighScores = {}, // Preview stub.
        ) // End GameScreen.
    } // End theme.
} // End GameScreenSessionSummaryPreview.
