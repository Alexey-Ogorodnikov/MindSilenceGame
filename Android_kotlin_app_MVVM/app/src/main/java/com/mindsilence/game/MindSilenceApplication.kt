package com.mindsilence.game // Root application package.

import android.app.Application // Process-wide Android Application subclass.
import dagger.hilt.android.HiltAndroidApp // Generates the singleton Hilt component for this process.

/**
 * Process entry for Hilt. Must be named in the manifest so `@HiltViewModel` and
 * `@AndroidEntryPoint` can resolve the generated singleton graph.
 */
@HiltAndroidApp // Codegen creates the Application-scoped component used by Activities and ViewModels.
class MindSilenceApplication : Application() // No extra onCreate work; Hilt owns startup wiring.
