package com.mindsilence.game.feature.splash // Compose splash that must pixel-match the system splash.

import androidx.compose.foundation.Image // Splash icon in the same slot as the system splash.
import androidx.compose.foundation.background // White splash_background, not MindSilenceTheme.
import androidx.compose.foundation.layout.BoxWithConstraints // Title offset needs maxHeight of the screen.
import androidx.compose.foundation.layout.fillMaxSize // Full-window splash.
import androidx.compose.foundation.layout.padding // Places the title under the icon slot.
import androidx.compose.foundation.layout.size // Icon slot size (SplashDefaults.IconSize).
import androidx.compose.runtime.Composable // Splash UI; no ViewModel in this composable.
import androidx.compose.ui.Alignment // Center icon; TopCenter title under the slot.
import androidx.compose.ui.Modifier // Layout and test tag.
import androidx.compose.ui.layout.ContentScale // Fit the padded asset inside the 288.dp slot.
import androidx.compose.ui.platform.testTag // Icon size test looks this up.
import androidx.compose.ui.res.colorResource // splash_background from XML so it matches the system splash.
import androidx.compose.ui.res.painterResource // splash_icon drawable.
import androidx.compose.ui.tooling.preview.Preview // Studio preview of the branded splash.
import androidx.compose.ui.unit.Dp // Overridable icon size for tests.
import com.mindsilence.game.R // splash_icon, splash_background.
import com.mindsilence.game.ui.theme.MindSilenceTheme // Preview only; runtime splash ignores this theme.

/**
 * Branded splash that matches the system splash (white, same icon slot) so the
 * handoff does not jump. Title sits under the icon, not a centered column.
 */
@Composable // Stateless layout; timing is in SplashViewModel.
fun SplashScreen( // Shown by MainActivity while showBrandedSplash is true.
    iconSize: Dp = SplashDefaults.IconSize, // Default matches system splash; tests may pass another size.
    modifier: Modifier = Modifier, // MainActivity adds onGloballyPositioned here.
) { // Start SplashScreen body.
    BoxWithConstraints( // Need maxHeight to sit the title just under the centered icon.
        modifier = modifier // Caller’s measure callback plus full-size white fill.
            .fillMaxSize() // Cover the window like the system splash.
            .background(colorResource(R.color.splash_background)), // XML white, not the app theme.
    ) { // Start splash box.
        // Same padded asset and slot as the system splash — avoids a size jump on handoff.
        Image( // Logo in the center slot.
            painter = painterResource(R.drawable.splash_icon), // Same asset as windowSplashScreenAnimatedIcon.
            contentDescription = null, // Decorative; the title carries the name.
            modifier = Modifier // Centered 288.dp slot with a test tag.
                .align(Alignment.Center) // Same placement as the system splash icon.
                .size(iconSize) // 288.dp unless a test overrides it.
                .testTag(SplashDefaults.IconTestTag), // UI test finds this node.
            contentScale = ContentScale.Fit, // Keep the padded asset from cropping.
        ) // End Image.
        MindSilenceTitle( // Wordmark under the icon, not a vertical column through the center.
            modifier = Modifier // Position from the top using the icon slot math.
                .align(Alignment.TopCenter) // Horizontal center; vertical offset is padding.
                .padding( // Push the title to just below the icon plus TitleSpacing.
                    top = (maxHeight - iconSize) / 2 + iconSize + SplashDefaults.TitleSpacing, // Icon top + icon height + gap.
                ), // End padding.
        ) // End MindSilenceTitle.
    } // End BoxWithConstraints.
} // End SplashScreen.

@Preview( // Studio preview of the branded splash on a white window.
    name = "Splash", // Preview label.
    showBackground = true, // Show a background in the preview pane.
    backgroundColor = 0xFFFFFFFF, // White like splash_background.
    showSystemUi = true, // Include status/nav bars in the preview.
) // End Preview.
@Composable // Preview composable; not used at runtime.
private fun SplashScreenPreview() { // Light theme wrapper; splash colors still come from XML.
    MindSilenceTheme(darkTheme = false) { // Preview chrome only; SplashScreen paints its own white.
        SplashScreen() // Default icon size.
    } // End theme.
} // End SplashScreenPreview.
