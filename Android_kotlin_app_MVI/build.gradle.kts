plugins { // plugins declared at the root so modules can apply them
    alias(libs.plugins.android.application) apply false // Android application plugin; applied in :app
    alias(libs.plugins.kotlin.android) apply false // Kotlin Android plugin; applied in :app
    alias(libs.plugins.kotlin.compose) apply false // Compose compiler plugin; applied in :app
    alias(libs.plugins.kover) apply false // Kover coverage plugin; applied in :app
}
