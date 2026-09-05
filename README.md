# Mind Silence

A calm Android training game. Sit comfortably, tap **Start**, and do nothing while there is silence. When a thought appears, tap **Thought** — the session ends. Each next level lasts twice as long.

No ads, accounts, or trackers. Daily attempts, time, and best level stay on this device.


Privacy policy: [`legal/privacy-policy.md`](legal/privacy-policy.md).

## Run the app

The playable build is Kotlin and Jetpack Compose. In Android Studio open **`android_kotlin_app/`**, not the repository root.

```text
cd android_kotlin_app
./gradlew :app:assembleDebug
```

On Windows: `.\gradlew.bat :app:assembleDebug`.

Package `com.mindsilence.game` · minSdk 26 · target/compile 36.

## Repository

| Path | Role |
|------|------|
| [`android_kotlin_app/`](android_kotlin_app/) | The Android app (open this in Android Studio) |
| [`android_maui_app/`](android_maui_app/) | Empty stub — no MAUI project yet |
| [`legal/`](legal/) | Privacy policy (published via GitHub Pages) |

