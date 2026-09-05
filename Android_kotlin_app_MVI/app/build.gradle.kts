import java.util.Properties // Java Properties for optional keystore.properties
import kotlinx.kover.gradle.plugin.dsl.AggregationType // Kover aggregation enum for coverage rules
import kotlinx.kover.gradle.plugin.dsl.CoverageUnit // Kover unit enum (line vs branch)

plugins { // plugins applied to the :app module
    alias(libs.plugins.android.application) // Android application plugin for this module
    alias(libs.plugins.kotlin.android) // Kotlin Android compilation
    alias(libs.plugins.kotlin.compose) // Compose compiler plugin
    alias(libs.plugins.kover) // Kover code-coverage reports and verification
}

android { // Android application configuration
    namespace = "com.mindsilence.game" // R and BuildConfig package
    compileSdk = 36 // Android API used to compile

    defaultConfig { // shared settings for all build types
        applicationId = "com.mindsilence.game" // Play Store / device package name
        minSdk = 26 // lowest supported Android version (8.0)
        targetSdk = 36 // behavior target API
        versionCode = 1 // integer version for Play updates
        versionName = "1.0" // user-visible version string

        testInstrumentationRunner = "androidx.test.runner.AndroidJUnitRunner" // runner for androidTest
    }

    val keystorePropertiesFile = rootProject.file("keystore.properties") // optional local signing file
    val keystoreProperties = Properties() // holder for storeFile / passwords / alias
    if (keystorePropertiesFile.exists()) { // skip signing config when the file is absent
        keystorePropertiesFile.inputStream().use { keystoreProperties.load(it) } // load keystore keys from disk
    }

    signingConfigs { // named signing configs for build types
        if (keystorePropertiesFile.exists()) { // only define release signing when properties exist
            create("release") { // named signing config used by the release build type
                storeFile = rootProject.file(keystoreProperties.getProperty("storeFile")) // path to the .jks / .keystore
                storePassword = keystoreProperties.getProperty("storePassword") // password for the keystore file
                keyAlias = keystoreProperties.getProperty("keyAlias") // alias of the signing key
                keyPassword = keystoreProperties.getProperty("keyPassword") // password for the key alias
            }
        }
    }

    buildTypes { // debug vs release variants
        release { // Play/store build type
            isMinifyEnabled = true // enable R8 shrinking and obfuscation
            isShrinkResources = true // remove unused resources in release
            proguardFiles( // R8/ProGuard rule files for the release build
                getDefaultProguardFile("proguard-android-optimize.txt"), // Android default optimize rules
                "proguard-rules.pro", // app-specific keep rules
            )
            if (keystorePropertiesFile.exists()) { // attach release signing when a keystore is configured
                signingConfig = signingConfigs.getByName("release") // use the named release signing config
            }
        }
    }

    compileOptions { // javac language level
        sourceCompatibility = JavaVersion.VERSION_17 // javac source level
        targetCompatibility = JavaVersion.VERSION_17 // javac bytecode target
    }

    androidResources { // packaged resource filters
        localeFilters += "en" // keep only English resources in the APK
    }

    buildFeatures { // optional Android build features
        compose = true // enable Jetpack Compose
    }

    kotlinOptions { // Kotlin compiler options
        jvmTarget = "17" // Kotlin JVM bytecode target
    }

    testOptions { // unit and instrumented test options
        unitTests { // JVM unit-test options
            isIncludeAndroidResources = true // allow Robolectric to see Android resources
            isReturnDefaultValues = true // stub Android APIs that would otherwise throw in unit tests
        }
    }
}

dependencies { // module classpath
    implementation(libs.androidx.core.ktx) // Kotlin extensions for Android core
    implementation(libs.androidx.core.splashscreen) // AndroidX SplashScreen API
    implementation(libs.androidx.lifecycle.runtime.ktx) // Lifecycle runtime for process/activity
    implementation(libs.androidx.lifecycle.runtime.compose) // collectAsStateWithLifecycle and Compose lifecycle
    implementation(libs.androidx.lifecycle.viewmodel.compose) // viewModel() for Compose
    implementation(libs.androidx.activity.compose) // ComponentActivity setContent
    implementation(platform(libs.androidx.compose.bom)) // Compose Bill of Materials (aligns Compose versions)
    implementation(libs.androidx.compose.ui) // Compose UI toolkit
    implementation(libs.androidx.compose.ui.graphics) // Compose graphics primitives
    implementation(libs.androidx.compose.ui.tooling.preview) // @Preview annotations
    implementation(libs.androidx.compose.material3) // Material 3 components
    implementation(libs.kotlinx.coroutines.android) // coroutines on Android main dispatcher

    testImplementation(libs.junit) // JUnit 4 for unit tests
    testImplementation(libs.kotlinx.coroutines.test) // TestDispatcher and runTest
    testImplementation(libs.turbine) // Flow/StateFlow assertions in tests
    testImplementation(libs.androidx.lifecycle.runtime.testing) // TestLifecycleOwner
    testImplementation(libs.robolectric) // JVM Android framework stubs
    testImplementation(libs.androidx.test.core) // ApplicationProvider and test core

    androidTestImplementation(platform(libs.androidx.compose.bom)) // same Compose versions in androidTest
    androidTestImplementation(libs.androidx.compose.ui.test.junit4) // Compose UI test APIs
    androidTestImplementation(libs.androidx.junit) // AndroidX JUnit extensions for instrumented tests

    debugImplementation(libs.androidx.compose.ui.tooling) // interactive Compose preview tooling (debug)
    debugImplementation(libs.androidx.compose.ui.test.manifest) // test Activity manifest for Compose tests
}

kover { // Kover coverage plugin configuration
    reports { // coverage reports and verification
        filters { // which classes enter the coverage gate
            includes { // allowlist of classes that must be covered
                classes( // MVI/ViewModel types required to meet the line-coverage gate
                    "com.mindsilence.game.feature.game.GameViewModel", // game session ViewModel
                    "com.mindsilence.game.feature.game.GameViewModelFactory", // game ViewModel factory
                    "com.mindsilence.game.feature.game.GameUiState", // game screen state
                    "com.mindsilence.game.feature.game.GamePhase", // in-session phase enum
                    "com.mindsilence.game.feature.game.SessionSummary", // post-session summary model
                    "com.mindsilence.game.feature.game.GameUiEvent*", // game user events
                    "com.mindsilence.game.feature.game.GameUiEffect*", // game one-shot effects
                    "com.mindsilence.game.feature.game.DailyStats", // daily stats model
                    "com.mindsilence.game.feature.game.LevelDurationKt", // level duration helpers
                    "com.mindsilence.game.feature.game.GameProgressRepository", // progress persistence port
                    "com.mindsilence.game.feature.game.SharedPreferencesGameProgressRepository", // SharedPreferences impl
                    "com.mindsilence.game.feature.game.InMemoryGameProgressRepository", // in-memory test impl
                    "com.mindsilence.game.feature.splash.SplashViewModel", // splash ViewModel
                    "com.mindsilence.game.feature.splash.SplashViewModelFactory", // splash ViewModel factory
                    "com.mindsilence.game.feature.splash.SplashUiState", // splash screen state
                    "com.mindsilence.game.feature.splash.SplashUiEvent*", // splash user events
                    "com.mindsilence.game.feature.splash.SplashDefaults", // splash timing defaults
                    "com.mindsilence.game.feature.menu.MenuViewModel", // menu ViewModel
                    "com.mindsilence.game.feature.menu.MenuUiState", // menu screen state
                    "com.mindsilence.game.feature.menu.MenuUiEvent*", // menu user events
                    "com.mindsilence.game.feature.menu.MenuUiEffect*", // menu one-shot effects
                    "com.mindsilence.game.feature.highscores.HighScoresViewModel", // high scores ViewModel
                    "com.mindsilence.game.feature.highscores.HighScoresViewModelFactory", // high scores factory
                    "com.mindsilence.game.feature.highscores.HighScoresUiState", // high scores screen state
                    "com.mindsilence.game.feature.highscores.HighScoresUiEvent*", // high scores user events
                    "com.mindsilence.game.feature.highscores.HighScoresUiEffect*", // high scores one-shot effects
                    "com.mindsilence.game.navigation.AppViewModel", // root navigation ViewModel
                    "com.mindsilence.game.navigation.AppUiState", // root navigation state
                    "com.mindsilence.game.navigation.AppUiEvent*", // root navigation events
                )
            }
        }
        verify { // fail the build when coverage is too low
            rule { // one verification rule
                bound { // numeric coverage bound
                    aggregationForGroup = AggregationType.COVERED_PERCENTAGE // fail on covered-line percentage
                    coverageUnits = CoverageUnit.LINE // measure line coverage, not branches
                    minValue = 100 // require 100% line coverage on included classes
                }
            }
        }
    }
}
