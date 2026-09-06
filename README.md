# Mind Silence

A calm Android training game. Sit comfortably, tap **Start**, and do nothing while there is silence. When a thought appears, tap **Thought** — the session ends. Each next level lasts twice as long.

No ads, accounts, or trackers. Daily attempts, time, and best level stay on this device.


Privacy policy: [`legal/privacy-policy.md`](legal/privacy-policy.md).

## Run the app

Two Kotlin / Jetpack Compose Gradle roots. Open **one folder** in Android Studio, not the repository root.

Play Store / signing uses **MVI**:

```text
cd Android_kotlin_app_MVI
./gradlew :app:assembleDebug
```

MVVM + Clean Architecture + Hilt (same gameplay, package `com.mindsilence.game.mvvm`):

```text
cd Android_kotlin_app_MVVM
./gradlew :app:assembleDebug
```

On Windows: `.\gradlew.bat :app:assembleDebug`.

minSdk 26 · target/compile 36.

## Repository

| Path | Role |
|------|------|
| [`Android_kotlin_app_MVI/`](Android_kotlin_app_MVI/) | MVI Android app (open this for Play builds) |
| [`Android_kotlin_app_MVVM/`](Android_kotlin_app_MVVM/) | MVVM + Clean Architecture + Hilt copy |
| [`android_maui_app/`](android_maui_app/) | Empty stub — no MAUI project yet |
| [`handbook/`](handbook/) | Product and architecture canon |
| [`legal/`](legal/) | Privacy policy (published via GitHub Pages) |
