package com.mindsilence.game.navigation

import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.saveable.rememberSaveable
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import com.mindsilence.game.feature.game.GameRoute
import com.mindsilence.game.feature.game.SharedPreferencesGameProgressRepository
import com.mindsilence.game.feature.highscores.HighScoresRoute
import com.mindsilence.game.feature.menu.MenuRoute

@Composable
fun AppRoute(
    modifier: Modifier = Modifier,
) {
    val context = LocalContext.current
    val progressRepository = remember(context) {
        SharedPreferencesGameProgressRepository(context.applicationContext)
    }
    var inTraining by rememberSaveable { mutableStateOf(false) }
    var showHighScores by rememberSaveable { mutableStateOf(false) }

    when {
        showHighScores -> {
            HighScoresRoute(
                progressRepository = progressRepository,
                onBack = { showHighScores = false },
                modifier = modifier,
            )
        }
        inTraining -> {
            GameRoute(
                progressRepository = progressRepository,
                onOpenHighScores = { showHighScores = true },
                onBack = { inTraining = false },
                modifier = modifier,
            )
        }
        else -> {
            MenuRoute(
                onOpenTraining = { inTraining = true },
                modifier = modifier,
            )
        }
    }
}
