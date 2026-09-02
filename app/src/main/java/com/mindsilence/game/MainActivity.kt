package com.mindsilence.game

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.enableEdgeToEdge
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.saveable.rememberSaveable
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.layout.onGloballyPositioned
import androidx.core.splashscreen.SplashScreen.Companion.installSplashScreen
import com.mindsilence.game.feature.splash.SplashScreen
import com.mindsilence.game.navigation.AppRoute
import kotlinx.coroutines.delay

class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        val splashScreen = installSplashScreen()
        enableEdgeToEdge()
        super.onCreate(savedInstanceState)

        val startWithBrandedSplash = savedInstanceState == null
        var keepSystemSplash = true
        splashScreen.setKeepOnScreenCondition { keepSystemSplash }
        splashScreen.setOnExitAnimationListener { splashScreenView ->
            splashScreenView.remove()
        }

        setContent {
            var showBrandedSplash by rememberSaveable {
                mutableStateOf(startWithBrandedSplash)
            }

            LaunchedEffect(showBrandedSplash) {
                if (showBrandedSplash) {
                    delay(SPLASH_DURATION_MS)
                    showBrandedSplash = false
                } else {
                    keepSystemSplash = false
                }
            }

            MindSilenceApp {
                if (showBrandedSplash) {
                    SplashScreen(
                        modifier = Modifier.onGloballyPositioned {
                            keepSystemSplash = false
                        },
                    )
                } else {
                    AppRoute()
                }
            }
        }
    }

    private companion object {
        const val SPLASH_DURATION_MS = 2_000L
    }
}
