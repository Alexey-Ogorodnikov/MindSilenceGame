package com.mindsilence.game.di // Binds domain ports to data implementations.

import com.mindsilence.game.data.SharedPreferencesGameProgressRepository // Device JSON store.
import com.mindsilence.game.domain.repository.GameProgressRepository // Port UseCases depend on.
import dagger.Binds // Interface-to-impl mapping generates less code than @Provides.
import dagger.Module // Abstract module of @Binds methods.
import dagger.hilt.InstallIn // Install in the process-wide component.
import dagger.hilt.components.SingletonComponent // One repository for the app process.
import javax.inject.Singleton // Same instance so Thought writes are visible on highscores.

/** Binds [GameProgressRepository] to the SharedPreferences implementation. */
@Module // Hilt reads the @Binds method from this abstract class.
@InstallIn(SingletonComponent::class) // Lives as long as the Application.
abstract class RepositoryModule { // Abstract because @Binds must not have a method body.

    /**
     * Exposes the prefs repository as the domain port so UseCases never import data/.
     *
     * @param impl Hilt-constructed SharedPreferences implementation
     */
    @Binds // Map the interface to this single implementation.
    @Singleton // One store for GameViewModel and HighScoresViewModel.
    abstract fun bindGameProgressRepository( // Method name is documentation for the generated factory.
        impl: SharedPreferencesGameProgressRepository, // Concrete type Hilt already knows how to build.
    ): GameProgressRepository // Type UseCases and tests against the port.
}
