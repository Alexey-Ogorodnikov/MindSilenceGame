pluginManagement { // where Gradle resolves build plugins
    repositories { // repositories used only for plugins
        google { // Google Maven for Android/Google plugins
            content { // limit which groups are fetched from Google
                includeGroupByRegex("com\\.android.*") // Android Gradle Plugin artifacts
                includeGroupByRegex("com\\.google.*") // Google library and plugin artifacts
                includeGroupByRegex("androidx.*") // AndroidX library artifacts
            }
        }
        mavenCentral() // Maven Central for remaining plugins
        gradlePluginPortal() // official Gradle Plugin Portal
    }
}

dependencyResolutionManagement { // where project dependencies are resolved
    repositoriesMode.set(RepositoriesMode.FAIL_ON_PROJECT_REPOS) // forbid per-module repository blocks
    repositories { // repositories used for app libraries
        google() // Google Maven for AndroidX and Google libraries
        mavenCentral() // Maven Central for remaining libraries
    }
}

rootProject.name = "MindSilenceGameMVVM" // Gradle/Studio title so this root is distinct from MVI
include(":app") // include the single application module
