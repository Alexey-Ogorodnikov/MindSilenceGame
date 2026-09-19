using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace MindSilence.EditorTools
{
    public static class QuestAndroidIdentity
    {
        private const string ScenePath = "Assets/Scenes/Bootstrap.unity";
        private const string PackageName = "com.mindsilence.game.quest";

        public static void Apply()
        {
            ApplyPlayerSettings();
            ApplyQuestUrpAndQuality();
            EnsureBootstrapScene();
            var switched = EditorUserBuildSettings.SwitchActiveBuildTarget(
                NamedBuildTarget.Android,
                BuildTarget.Android);
            if (!switched)
            {
                throw new System.InvalidOperationException(
                    "Could not switch the active build target to Android.");
            }

            AssetDatabase.SaveAssets();
            Debug.Log(
                "Quest Android identity applied: " + PackageName +
                ", IL2CPP ARM64, min API 32, scene " + ScenePath);
        }

        public static void BuildDevelopmentApk()
        {
            Apply();
            var shortTemp = @"C:\u-tmp";
            var shortGradle = @"C:\u-gradle";
            Directory.CreateDirectory(shortTemp);
            Directory.CreateDirectory(shortGradle);
            Environment.SetEnvironmentVariable("TEMP", shortTemp);
            Environment.SetEnvironmentVariable("TMP", shortTemp);
            Environment.SetEnvironmentVariable("GRADLE_USER_HOME", shortGradle);
            const string apkPath = "Builds/MindSilence.apk";
            var directory = Path.GetDirectoryName(apkPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = apkPath,
                target = BuildTarget.Android,
                options = BuildOptions.Development
            });
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new System.InvalidOperationException(
                    "Android APK build failed: " + report.summary.result);
            }

            Debug.Log("Android APK built: " + Path.GetFullPath(apkPath));
        }

        private static void ApplyPlayerSettings()
        {
            PlayerSettings.companyName = "MindSilence";
            PlayerSettings.productName = "Mind Silence";
            PlayerSettings.bundleVersion = "1.0";
            PlayerSettings.Android.bundleVersionCode = 1;
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, PackageName);
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel32;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.preferredInstallLocation = AndroidPreferredInstallLocation.Auto;
            PlayerSettings.Android.forceInternetPermission = false;
            PlayerSettings.insecureHttpOption = InsecureHttpOption.NotAllowed;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.SplashScreen.backgroundColor = Color.white;
            PlayerSettings.SplashScreen.animationMode = PlayerSettings.SplashScreen.AnimationMode.Static;
            QuestSplashSetup.ConfigureEngineSplash();
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.Vulkan });
        }

        public static void ApplyQuestUrpAndQuality()
        {
            const string pipelinePath = "Assets/Settings/URP-Pipeline.asset";
            var pipeline = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(pipelinePath);
            if (pipeline == null)
            {
                throw new System.InvalidOperationException("Missing " + pipelinePath);
            }

            GraphicsSettings.defaultRenderPipeline = pipeline;
            var names = QualitySettings.names;
            var questIndex = 2;
            for (var i = 0; i < names.Length; i++)
            {
                if (names[i] == "Medium")
                {
                    questIndex = i;
                    break;
                }
            }

            QualitySettings.SetQualityLevel(questIndex, applyExpensiveChanges: true);
            QualitySettings.renderPipeline = pipeline;
            QualitySettings.vSyncCount = 0;
            QualitySettings.shadows = ShadowQuality.Disable;
            QualitySettings.antiAliasing = 0;
            QualitySettings.particleRaycastBudget = 4;
            EditorUtility.SetDirty(pipeline);
            AssetDatabase.SaveAssets();
            Debug.Log("Quest URP assigned; quality '" + names[questIndex] + "' shadows off, vSync 0.");
        }

        private static void EnsureBootstrapScene()
        {
            Scene scene;
            if (System.IO.File.Exists(ScenePath))
            {
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }
            else
            {
                scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                var directory = System.IO.Path.GetDirectoryName(ScenePath);
                if (!string.IsNullOrEmpty(directory) && !System.IO.Directory.Exists(directory))
                {
                    System.IO.Directory.CreateDirectory(directory);
                }

                EditorSceneManager.SaveScene(scene, ScenePath);
            }

            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            EditorSceneManager.SaveScene(scene);
        }
    }

    /// <summary>
    /// Path A sideload: do not ship Meta XR Operator. Core SDK still packs the
    /// implicit OpenXR layer into Development APKs; that aborts LoadOpenXRLibrary
    /// on Quest. callbackOrder 1 replaces the SDK's Development-only include.
    /// </summary>
    internal sealed class ExcludeMetaXrOperatorAar : IPreprocessBuildWithReport
    {
        public int callbackOrder => 1;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.Android)
            {
                return;
            }

            foreach (var importer in PluginImporter.GetAllImporters())
            {
                if (importer == null ||
                    importer.assetPath.IndexOf(
                        "XrApiLayer_METAX_operator_unity_android",
                        StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                importer.SetIncludeInBuildDelegate(_ => false);
                Debug.Log("Excluded Meta XR Operator AAR from Android player.");
                break;
            }
        }
    }
}
