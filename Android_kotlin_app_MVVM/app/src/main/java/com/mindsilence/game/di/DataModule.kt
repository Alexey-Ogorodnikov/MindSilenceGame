package com.mindsilence.game.di // Hilt providers for Android framework types used by data/.

import android.content.Context // Application context for opening SharedPreferences.
import android.content.SharedPreferences // Injected into the repository implementation.
import dagger.Module // Declares @Provides methods for the Hilt graph.
import dagger.Provides // Constructs SharedPreferences because it is not a class we @Inject.
import dagger.hilt.InstallIn // Install these providers in the process-wide component.
import dagger.hilt.android.qualifiers.ApplicationContext // Avoid leaking an Activity context.
import dagger.hilt.components.SingletonComponent // One prefs file for the whole app process.
import javax.inject.Singleton // Same SharedPreferences instance for game and highscores.

/** Provides the `game_progress` SharedPreferences file used by the data layer. */
@Module // Hilt reads @Provides methods from this object.
@InstallIn(SingletonComponent::class) // Lives as long as the Application.
object DataModule { // Stateless provider; no instance fields.

    /**
     * Opens the same prefs file the MVI app uses so JSON keys stay compatible.
     *
     * @param context application context from Hilt
     */
    @Provides // Hilt calls this when SharedPreferences is requested.
    @Singleton // One file handle for GameViewModel writes and HighScoresViewModel reads.
    fun provideGameProgressPreferences( // Named so the graph is readable in generated code.
        @ApplicationContext context: Context, // Process context; never an Activity.
    ): SharedPreferences = // Return type is what SharedPreferencesGameProgressRepository injects.
        context.getSharedPreferences(PREFS_NAME, Context.MODE_PRIVATE) // File game_progress.

    private const val PREFS_NAME = "game_progress" // Same file name as the MVI implementation.
}
