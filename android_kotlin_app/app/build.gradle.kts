import java.util.Properties
import kotlinx.kover.gradle.plugin.dsl.AggregationType
import kotlinx.kover.gradle.plugin.dsl.CoverageUnit

plugins {
    alias(libs.plugins.android.application)
    alias(libs.plugins.kotlin.android)
    alias(libs.plugins.kotlin.compose)
    alias(libs.plugins.kover)
}

android {
    namespace = "com.mindsilence.game"
    compileSdk = 36

    defaultConfig {
        applicationId = "com.mindsilence.game"
        minSdk = 26
        targetSdk = 36
        versionCode = 1
        versionName = "1.0"

        testInstrumentationRunner = "androidx.test.runner.AndroidJUnitRunner"
    }

    val keystorePropertiesFile = rootProject.file("keystore.properties")
    val keystoreProperties = Properties()
    if (keystorePropertiesFile.exists()) {
        keystorePropertiesFile.inputStream().use { keystoreProperties.load(it) }
    }

    signingConfigs {
        if (keystorePropertiesFile.exists()) {
            create("release") {
                storeFile = rootProject.file(keystoreProperties.getProperty("storeFile"))
                storePassword = keystoreProperties.getProperty("storePassword")
                keyAlias = keystoreProperties.getProperty("keyAlias")
                keyPassword = keystoreProperties.getProperty("keyPassword")
            }
        }
    }

    buildTypes {
        release {
            isMinifyEnabled = true
            isShrinkResources = true
            proguardFiles(
                getDefaultProguardFile("proguard-android-optimize.txt"),
                "proguard-rules.pro",
            )
            if (keystorePropertiesFile.exists()) {
                signingConfig = signingConfigs.getByName("release")
            }
        }
    }

    compileOptions {
        sourceCompatibility = JavaVersion.VERSION_17
        targetCompatibility = JavaVersion.VERSION_17
    }

    androidResources {
        localeFilters += "en"
    }

    buildFeatures {
        compose = true
    }

    kotlinOptions {
        jvmTarget = "17"
    }

    testOptions {
        unitTests {
            isIncludeAndroidResources = true
            isReturnDefaultValues = true
        }
    }
}

dependencies {
    implementation(libs.androidx.core.ktx)
    implementation(libs.androidx.core.splashscreen)
    implementation(libs.androidx.lifecycle.runtime.ktx)
    implementation(libs.androidx.lifecycle.runtime.compose)
    implementation(libs.androidx.lifecycle.viewmodel.compose)
    implementation(libs.androidx.activity.compose)
    implementation(platform(libs.androidx.compose.bom))
    implementation(libs.androidx.compose.ui)
    implementation(libs.androidx.compose.ui.graphics)
    implementation(libs.androidx.compose.ui.tooling.preview)
    implementation(libs.androidx.compose.material3)
    implementation(libs.kotlinx.coroutines.android)

    testImplementation(libs.junit)
    testImplementation(libs.kotlinx.coroutines.test)
    testImplementation(libs.turbine)
    testImplementation(libs.androidx.lifecycle.runtime.testing)
    testImplementation(libs.robolectric)
    testImplementation(libs.androidx.test.core)

    androidTestImplementation(platform(libs.androidx.compose.bom))
    androidTestImplementation(libs.androidx.compose.ui.test.junit4)
    androidTestImplementation(libs.androidx.junit)

    debugImplementation(libs.androidx.compose.ui.tooling)
    debugImplementation(libs.androidx.compose.ui.test.manifest)
}

kover {
    reports {
        filters {
            includes {
                classes(
                    "com.mindsilence.game.feature.game.GameViewModel",
                    "com.mindsilence.game.feature.game.GameViewModelFactory",
                    "com.mindsilence.game.feature.game.GameUiState",
                    "com.mindsilence.game.feature.game.GamePhase",
                    "com.mindsilence.game.feature.game.SessionSummary",
                    "com.mindsilence.game.feature.game.GameUiEvent*",
                    "com.mindsilence.game.feature.game.GameUiEffect*",
                    "com.mindsilence.game.feature.game.DailyStats",
                    "com.mindsilence.game.feature.game.LevelDurationKt",
                    "com.mindsilence.game.feature.game.GameProgressRepository",
                    "com.mindsilence.game.feature.game.SharedPreferencesGameProgressRepository",
                    "com.mindsilence.game.feature.game.InMemoryGameProgressRepository",
                    "com.mindsilence.game.feature.splash.SplashViewModel",
                    "com.mindsilence.game.feature.splash.SplashViewModelFactory",
                    "com.mindsilence.game.feature.splash.SplashUiState",
                    "com.mindsilence.game.feature.splash.SplashUiEvent*",
                    "com.mindsilence.game.feature.splash.SplashDefaults",
                    "com.mindsilence.game.feature.menu.MenuViewModel",
                    "com.mindsilence.game.feature.menu.MenuUiState",
                    "com.mindsilence.game.feature.menu.MenuUiEvent*",
                    "com.mindsilence.game.feature.menu.MenuUiEffect*",
                    "com.mindsilence.game.feature.highscores.HighScoresViewModel",
                    "com.mindsilence.game.feature.highscores.HighScoresViewModelFactory",
                    "com.mindsilence.game.feature.highscores.HighScoresUiState",
                    "com.mindsilence.game.feature.highscores.HighScoresUiEvent*",
                    "com.mindsilence.game.feature.highscores.HighScoresUiEffect*",
                    "com.mindsilence.game.navigation.AppViewModel",
                    "com.mindsilence.game.navigation.AppUiState",
                    "com.mindsilence.game.navigation.AppUiEvent*",
                )
            }
        }
        verify {
            rule {
                bound {
                    aggregationForGroup = AggregationType.COVERED_PERCENTAGE
                    coverageUnits = CoverageUnit.LINE
                    minValue = 100
                }
            }
        }
    }
}
