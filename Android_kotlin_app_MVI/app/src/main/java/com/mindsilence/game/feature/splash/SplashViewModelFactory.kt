package com.mindsilence.game.feature.splash // Factory so MainActivity can pass cold-start vs restore.

import androidx.lifecycle.ViewModel // Factory create() returns this type.
import androidx.lifecycle.ViewModelProvider // Manual DI; the app has no Hilt.

/** Creates [SplashViewModel] with cold-start vs restore ([startWithBrandedSplash]). */
class SplashViewModelFactory( // Activity cannot use the default ViewModel constructor.
    private val startWithBrandedSplash: Boolean, // savedInstanceState == null in MainActivity.
) : ViewModelProvider.Factory { // ViewModelProvider looks up SplashViewModel by class.

    @Suppress("UNCHECKED_CAST") // create() is generic; we only ever return SplashViewModel.
    override fun <T : ViewModel> create(modelClass: Class<T>): T { // Called by ViewModelProvider.
        if (modelClass.isAssignableFrom(SplashViewModel::class.java)) { // Only this factory’s type.
            return SplashViewModel(startWithBrandedSplash) as T // Forward the cold-start flag.
        } // End type check.
        throw IllegalArgumentException("Unknown ViewModel class: ${modelClass.name}") // Wrong class asked of this factory.
    } // End create.
} // End SplashViewModelFactory.
