using System;
using System.IO;
using System.Text;
using MindSilence.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
using UnityEngine.XR.OpenXR;

namespace MindSilence.EditorTools
{
    /// <summary>
    /// Step 05: AppStrings-backed splash on the world-space panel, Unity + Meta system splash.
    /// Path A: no Platform SDK. System splash uses Contextual Passthrough (no fade-through-black).
    /// </summary>
    public static class QuestSplashSetup
    {
        const string ScenePath = "Assets/Scenes/Bootstrap.unity";
        const string PanelPrefabPath = "Assets/UI/EmptyPanel.prefab";
        const string SplashIconPath = "Assets/Art/splash_icon.png";
        const string KotlinSplashIcon =
            "Android_kotlin_app_MVI/app/src/main/res/drawable-nodpi/splash_icon.png";

        [MenuItem("MindSilence/Quest/Apply Splash (Step 05)")]
        public static void Apply()
        {
            var sprite = ImportSplashIcon();
            ConfigureEngineSplash(sprite);
            ConfigurePanel(sprite);
            AssetDatabase.SaveAssets();
            Debug.Log("Quest splash setup applied (step 05).");
        }

        public static void ConfigureEngineSplash()
        {
            ConfigureEngineSplash(ImportSplashIcon());
        }

        [MenuItem("MindSilence/Quest/Run Edit Mode Tests")]
        public static void RunEditModeTests()
        {
            var api = ScriptableObject.CreateInstance<TestRunnerApi>();
            api.RegisterCallbacks(new EditModeTestCallbacks());
            api.Execute(new ExecutionSettings(new Filter
            {
                testMode = TestMode.EditMode,
                assemblyNames = new[] { "MindSilence.Tests" }
            }));
        }

        sealed class EditModeTestCallbacks : ICallbacks
        {
            public void RunStarted(ITestAdaptor testsToRun)
            {
                Debug.Log("Edit Mode tests started: " + testsToRun.TestCaseCount);
            }

            public void RunFinished(ITestResultAdaptor result)
            {
                var summary = new StringBuilder();
                summary.AppendLine("result=" + result.TestStatus);
                summary.AppendLine("passed=" + result.PassCount);
                summary.AppendLine("failed=" + result.FailCount);
                summary.AppendLine("skipped=" + result.SkipCount);
                CollectFailures(result, summary);
                var path = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Logs", "editmode-summary.txt"));
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, summary.ToString());
                Debug.Log("Edit Mode tests finished:\n" + summary);
                EditorApplication.Exit(result.FailCount == 0 ? 0 : 1);
            }

            public void TestStarted(ITestAdaptor test)
            {
            }

            public void TestFinished(ITestResultAdaptor result)
            {
            }

            static void CollectFailures(ITestResultAdaptor result, StringBuilder summary)
            {
                if (!result.HasChildren && result.TestStatus == TestStatus.Failed)
                {
                    summary.AppendLine("FAIL " + result.FullName);
                    summary.AppendLine(result.Message);
                }

                if (!result.HasChildren)
                {
                    return;
                }

                foreach (var child in result.Children)
                {
                    CollectFailures(child, summary);
                }
            }
        }

        static Sprite ImportSplashIcon()
        {
            CopyKotlinSplashIcon();
            AssetDatabase.ImportAsset(SplashIconPath);
            var importer = (TextureImporter)AssetImporter.GetAtPath(SplashIconPath);
            if (importer == null)
            {
                throw new InvalidOperationException("Missing " + SplashIconPath);
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SplashIconPath);
            if (sprite == null)
            {
                throw new InvalidOperationException("splash_icon did not import as a Sprite.");
            }

            return sprite;
        }

        static void CopyKotlinSplashIcon()
        {
            var repoRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "..", ".."));
            var source = Path.Combine(repoRoot, KotlinSplashIcon.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(source))
            {
                throw new FileNotFoundException("Kotlin splash_icon PNG not found.", source);
            }

            var destination = Path.Combine(Application.dataPath, "Art", "splash_icon.png");
            var directory = Path.GetDirectoryName(destination);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.Copy(source, destination, overwrite: true);
        }

        static void ConfigureEngineSplash(Sprite sprite)
        {
            var texture = sprite.texture;
            PlayerSettings.SplashScreen.show = true;
            PlayerSettings.SplashScreen.backgroundColor = Color.white;
            PlayerSettings.SplashScreen.animationMode = PlayerSettings.SplashScreen.AnimationMode.Static;
            PlayerSettings.SplashScreen.drawMode = PlayerSettings.SplashScreen.DrawMode.AllSequential;
            PlayerSettings.SplashScreen.overlayOpacity = 1f;
            PlayerSettings.SplashScreen.blurBackgroundImage = false;
            try
            {
                PlayerSettings.SplashScreen.showUnityLogo = false;
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Could not hide Made with Unity logo (license): " + exception.Message);
            }

            PlayerSettings.SplashScreen.logos = new[]
            {
                PlayerSettings.SplashScreenLogo.Create(0.15f, sprite)
            };
            PlayerSettings.virtualRealitySplashScreen = texture;
            PlayerSettings.Android.splashScreenScale = AndroidSplashScreenScale.Center;

            AssignMetaSystemSplash(texture);
        }

        static void AssignMetaSystemSplash(Texture2D texture)
        {
            var configType = FindType("OVRProjectConfig");
            UnityEngine.Object config = null;
            if (configType != null)
            {
                var cached = configType.GetProperty("CachedProjectConfig") ?? configType.GetProperty("Instance");
                config = cached?.GetValue(null) as UnityEngine.Object;
            }

            if (config == null)
            {
                var guids = AssetDatabase.FindAssets("t:OVRProjectConfig");
                if (guids.Length > 0)
                {
                    config = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AssetDatabase.GUIDToAssetPath(guids[0]));
                }
            }

            if (config != null)
            {
                var so = new SerializedObject(config);
                SetObjectIfPresent(so, "systemSplashScreen", texture);
                SetIntIfPresent(so, "systemSplashScreenType", 0);
                SetIntIfPresent(so, "_systemLoadingScreenBackground", 1);
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(config);
            }
            else
            {
                Debug.LogWarning("OVRProjectConfig not found; Meta system splash was not assigned.");
            }

            AssignOpenXrSystemSplash(BuildTargetGroup.Android, texture);
            AssignOpenXrSystemSplash(BuildTargetGroup.Standalone, texture);
        }

        static void AssignOpenXrSystemSplash(BuildTargetGroup group, Texture2D texture)
        {
            var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(group);
            if (settings == null)
            {
                return;
            }

            foreach (var feature in settings.GetFeatures())
            {
                if (feature == null)
                {
                    continue;
                }

                var so = new SerializedObject(feature);
                if (so.FindProperty("systemSplashScreen") == null)
                {
                    continue;
                }

                SetObjectIfPresent(so, "systemSplashScreen", texture);
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(feature);
            }
        }

        static void ConfigurePanel(Sprite sprite)
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var panel = GameObject.Find("WorldSpacePanel");
            if (panel == null)
            {
                throw new InvalidOperationException("WorldSpacePanel is missing. Close step 02 first.");
            }

            var host = panel.GetComponent<SplashHost>() ?? panel.AddComponent<SplashHost>();
            var hostSo = new SerializedObject(host);
            hostSo.FindProperty("splashIcon").objectReferenceValue = sprite;
            hostSo.ApplyModifiedPropertiesWithoutUndo();
            SplashHost.BuildHierarchy(panel.GetComponent<RectTransform>(), sprite);
            if (panel.GetComponent<AppHost>() == null)
            {
                panel.AddComponent<AppHost>();
            }

            AppHost.BuildHierarchy(panel.GetComponent<RectTransform>());

            if (PrefabUtility.IsPartOfPrefabInstance(panel))
            {
                PrefabUtility.ApplyPrefabInstance(panel, InteractionMode.AutomatedAction);
            }
            else
            {
                PrefabUtility.SaveAsPrefabAssetAndConnect(panel, PanelPrefabPath, InteractionMode.AutomatedAction);
            }

            EditorUtility.SetDirty(panel);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PanelPrefabPath);
            if (prefab != null)
            {
                EditorUtility.SetDirty(prefab);
            }
        }

        static void SetObjectIfPresent(SerializedObject so, string property, UnityEngine.Object value)
        {
            var found = so.FindProperty(property);
            if (found != null && found.propertyType == SerializedPropertyType.ObjectReference)
            {
                found.objectReferenceValue = value;
            }
        }

        static void SetIntIfPresent(SerializedObject so, string property, int value)
        {
            var found = so.FindProperty(property);
            if (found != null && (found.propertyType == SerializedPropertyType.Integer
                || found.propertyType == SerializedPropertyType.Enum))
            {
                found.intValue = value;
            }
        }

        static Type FindType(string fullOrShortName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = null;
                try
                {
                    type = assembly.GetType(fullOrShortName);
                    if (type == null)
                    {
                        foreach (var candidate in assembly.GetTypes())
                        {
                            if (candidate.Name == fullOrShortName || candidate.FullName == fullOrShortName)
                            {
                                type = candidate;
                                break;
                            }
                        }
                    }
                }
                catch
                {
                    // Some assemblies throw on GetTypes.
                }

                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }
    }
}
