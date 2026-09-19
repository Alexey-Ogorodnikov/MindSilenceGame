using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using MindSilence.Presentation;
using MindSilence.XR;

namespace MindSilence.EditorTools
{
    /// <summary>
    /// Step 02: OpenXR + Meta XR Core, seated passthrough, empty world-space panel.
    /// Path A (sideload): no Platform SDK, no INTERNET.
    /// </summary>
    public static class QuestXrPassthroughSetup
    {
        private const string ScenePath = "Assets/Scenes/Bootstrap.unity";
        private const string WhiteSpritePath = "Assets/UI/WhitePixel.png";
        private const string PanelPrefabPath = "Assets/UI/EmptyPanel.prefab";
        private const string ManifestPath = "Assets/Plugins/Android/AndroidManifest.xml";

        [MenuItem("MindSilence/Quest/Apply XR Passthrough (Step 02)")]
        public static void Apply()
        {
            QuestAndroidIdentity.Apply();
            QuestAndroidIdentity.ApplyQuestUrpAndQuality();
            EnableOpenXrLoaders();
            EnableMetaOpenXrFeatures(BuildTargetGroup.Android);
            EnableMetaOpenXrFeatures(BuildTargetGroup.Standalone);
            ForceRemoveInternetPermission();
            RejectOculusXrPlugin();
            ConfigureSeatedPassthroughScene();
            ConfigureOvrProject();
            QuestSplashSetup.ConfigureEngineSplash();
            AssetDatabase.SaveAssets();
            FixProjectSetupTasks(BuildTargetGroup.Android);
            FixProjectSetupTasks(BuildTargetGroup.Standalone);
            IgnoreSkippedSetupTasks(BuildTargetGroup.Android);
            IgnoreSkippedSetupTasks(BuildTargetGroup.Standalone);
            RefreshSideloadManifest();
            AssetDatabase.SaveAssets();
            LogRemainingSetupTasks(BuildTargetGroup.Android);
            QuestSplashSetup.Apply();
            Debug.Log("Quest XR passthrough setup applied (step 02).");
        }

        public static void RefreshSideloadManifest()
        {
            GenerateAndSanitizeManifest();
        }

        [MenuItem("MindSilence/Quest/Build Development APK (Step 02)")]
        public static void BuildDevelopmentApk()
        {
            Apply();
            QuestAndroidIdentity.BuildDevelopmentApk();
        }

        private static void RejectOculusXrPlugin()
        {
            RemoveLoader(BuildTargetGroup.Android, "Unity.XR.Oculus.OculusLoader");
            RemoveLoader(BuildTargetGroup.Standalone, "Unity.XR.Oculus.OculusLoader");
            foreach (var package in UnityEditor.PackageManager.PackageInfo.GetAllRegisteredPackages())
            {
                if (package.name == "com.unity.xr.oculus")
                {
                    throw new InvalidOperationException(
                        "Oculus XR Plugin is present. Step 02 allows only Unity OpenXR Plugin.");
                }
            }
        }

        private static void EnableOpenXrLoaders()
        {
            EnsureAndroidXrSettings();
            AssignLoader(BuildTargetGroup.Standalone, "UnityEngine.XR.OpenXR.OpenXRLoader");
            AssignLoader(BuildTargetGroup.Android, "UnityEngine.XR.OpenXR.OpenXRLoader");
            var androidManager = ManagerFor(BuildTargetGroup.Android);
            Debug.Log(androidManager == null
                ? "Android XR manager still missing after AssignLoader."
                : "Android XR manager loaders: " + androidManager.activeLoaders.Count);
        }

        private static void EnsureAndroidXrSettings()
        {
            const string path = "Assets/XR/XRGeneralSettingsPerBuildTarget.asset";
            var perTarget = AssetDatabase.LoadAssetAtPath<XRGeneralSettingsPerBuildTarget>(path);
            if (perTarget == null)
            {
                Debug.LogWarning("XRGeneralSettingsPerBuildTarget.asset is missing.");
                return;
            }

            if (XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android) != null)
            {
                return;
            }

            var androidManager = ScriptableObject.CreateInstance<XRManagerSettings>();
            androidManager.name = "Android Providers";
            var androidGeneral = ScriptableObject.CreateInstance<XRGeneralSettings>();
            androidGeneral.name = "Android Settings";
            AssetDatabase.AddObjectToAsset(androidManager, perTarget);
            AssetDatabase.AddObjectToAsset(androidGeneral, perTarget);

            var generalSo = new SerializedObject(androidGeneral);
            generalSo.FindProperty("m_LoaderManagerInstance").objectReferenceValue = androidManager;
            generalSo.FindProperty("m_InitManagerOnStart").boolValue = true;
            generalSo.ApplyModifiedPropertiesWithoutUndo();

            var so = new SerializedObject(perTarget);
            var keys = so.FindProperty("Keys");
            var values = so.FindProperty("Values");
            var index = keys.arraySize;
            keys.InsertArrayElementAtIndex(index);
            values.InsertArrayElementAtIndex(index);
            keys.GetArrayElementAtIndex(index).intValue = (int)BuildTargetGroup.Android;
            values.GetArrayElementAtIndex(index).objectReferenceValue = androidGeneral;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(perTarget);
            AssetDatabase.SaveAssets();
            Debug.Log("Created Android XR General Settings.");
        }

        private static XRManagerSettings ManagerFor(BuildTargetGroup group)
        {
            var general = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(group);
            if (general == null)
            {
                return null;
            }

            return general.AssignedSettings;
        }

        private static void AssignLoader(BuildTargetGroup group, string loaderTypeName)
        {
            var manager = ManagerFor(group);
            if (manager == null)
            {
                Debug.LogWarning("XR manager settings missing for " + group + "; Project Setup Tool should create them.");
                return;
            }

            XRPackageMetadataStore.AssignLoader(manager, loaderTypeName, group);
        }

        private static void RemoveLoader(BuildTargetGroup group, string loaderTypeName)
        {
            var manager = ManagerFor(group);
            if (manager == null)
            {
                return;
            }

            XRPackageMetadataStore.RemoveLoader(manager, loaderTypeName, group);
        }

        private static void EnableMetaOpenXrFeatures(BuildTargetGroup group)
        {
            var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(group);
            if (settings == null)
            {
                Debug.LogWarning("OpenXR settings missing for " + group);
                return;
            }

            foreach (var feature in settings.GetFeatures())
            {
                if (feature == null)
                {
                    continue;
                }

                var typeName = feature.GetType().FullName ?? feature.GetType().Name;
                var enable = typeName.IndexOf("MetaQuestSupport.MetaQuestFeature", StringComparison.OrdinalIgnoreCase) >= 0
                    || typeName.IndexOf("MetaXRFeature", StringComparison.OrdinalIgnoreCase) >= 0
                    || typeName.IndexOf("MetaXRFoveation", StringComparison.OrdinalIgnoreCase) >= 0
                    || typeName.IndexOf("MetaXRSubsampled", StringComparison.OrdinalIgnoreCase) >= 0
                    || typeName.IndexOf("OculusTouchControllerProfile", StringComparison.OrdinalIgnoreCase) >= 0
                    || typeName.IndexOf("MetaQuestTouchPlus", StringComparison.OrdinalIgnoreCase) >= 0
                    || typeName.IndexOf("MetaQuestTouchPro", StringComparison.OrdinalIgnoreCase) >= 0;
                var disable = typeName.IndexOf("OculusQuestSupport.OculusQuestFeature", StringComparison.OrdinalIgnoreCase) >= 0
                    || typeName.IndexOf("SpaceWarp", StringComparison.OrdinalIgnoreCase) >= 0
                    || typeName.IndexOf("EyeTracked", StringComparison.OrdinalIgnoreCase) >= 0
                    || typeName.IndexOf("ApiLayersFeature", StringComparison.OrdinalIgnoreCase) >= 0;
                if (enable)
                {
                    feature.enabled = true;
                }

                if (disable)
                {
                    feature.enabled = false;
                }
            }

            EditorUtility.SetDirty(settings);
            ForceRemoveInternetPermission(settings);
        }

        private static void ForceRemoveInternetPermission()
        {
            ForceRemoveInternetPermission(OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android));
            ForceRemoveInternetPermission(OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Standalone));
        }

        private static void ForceRemoveInternetPermission(OpenXRSettings settings)
        {
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
                SetBoolIfPresent(so, "forceRemoveInternetPermission", true);
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(feature);
            }
        }

        private static void ConfigureSeatedPassthroughScene()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            DestroyNamed("Main Camera");
            DestroyNamed("Directional Light");
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = Color.black;

            var cameraRig = FindOrInstantiatePrefab("OVRCameraRig", "OVRCameraRig");
            if (cameraRig == null)
            {
                throw new InvalidOperationException("OVRCameraRig prefab was not found. Is Meta XR Core SDK imported?");
            }

            var passthrough = FindOrInstantiatePrefab("PassthroughUnderlay", "[BuildingBlock] Passthrough");
            if (passthrough == null)
            {
                var layerHost = GameObject.Find("[BuildingBlock] Passthrough");
                if (layerHost == null)
                {
                    layerHost = new GameObject("[BuildingBlock] Passthrough");
                }

                passthrough = layerHost;
                AddComponentByName(passthrough, "OVRPassthroughLayer");
            }

            ConfigureOvrManager(cameraRig);
            ConfigureEyeCameras();
            EnsureEmptyPanel();
            DestroyLocomotion();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void ConfigureOvrManager(GameObject cameraRig)
        {
            var manager = cameraRig.GetComponent("OVRManager") as Component
                ?? cameraRig.GetComponentInChildren(FindType("OVRManager"), true) as Component;
            if (manager == null)
            {
                throw new InvalidOperationException("OVRManager is missing on OVRCameraRig.");
            }

            var so = new SerializedObject(manager);
            var origin = so.FindProperty("_trackingOriginType") ?? so.FindProperty("trackingOriginType");
            if (origin != null)
            {
                origin.intValue = 0;
            }

            SetBoolIfPresent(so, "isInsightPassthroughEnabled", true);
            SetBoolIfPresent(so, "enableMixedReality", false);
            SetBoolIfPresent(so, "_disableBackups", true);
            SetBoolIfPresent(so, "disableBackups", true);
            so.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.RecordPrefabInstancePropertyModifications(manager);
        }

        private static void ConfigureEyeCameras()
        {
            foreach (var camera in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
                camera.nearClipPlane = Mathf.Min(camera.nearClipPlane, 0.1f);
            }
        }

        private static void EnsureEmptyPanel()
        {
            var existing = GameObject.Find("WorldSpacePanel");
            if (existing != null)
            {
                UnityEngine.Object.DestroyImmediate(existing);
            }

            var sprite = EnsureWhiteSprite();
            var panel = new GameObject("WorldSpacePanel");
            var rect = panel.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(AppHost.PanelWidth, AppHost.PanelHeight);
            panel.transform.position = new Vector3(0f, 1.15f, 1.35f);
            panel.transform.rotation = Quaternion.identity;
            panel.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);

            var canvas = panel.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
            panel.AddComponent<CanvasScaler>();
            panel.AddComponent<GraphicRaycaster>();

            var imageObject = new GameObject("PanelSurface", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            imageObject.transform.SetParent(panel.transform, false);
            var imageRect = imageObject.GetComponent<RectTransform>();
            imageRect.anchorMin = Vector2.zero;
            imageRect.anchorMax = Vector2.one;
            imageRect.offsetMin = Vector2.zero;
            imageRect.offsetMax = Vector2.zero;
            var image = imageObject.GetComponent<Image>();
            image.sprite = sprite;
            image.color = new Color(0.949f, 0.961f, 0.973f, 1f);
            image.raycastTarget = false;

            var placer = panel.AddComponent<PanelPlacer>();
            PrefabUtility.SaveAsPrefabAssetAndConnect(panel, PanelPrefabPath, InteractionMode.AutomatedAction);
            EditorUtility.SetDirty(placer);
        }

        private static Sprite EnsureWhiteSprite()
        {
            var directory = Path.GetDirectoryName(WhiteSpritePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (!File.Exists(WhiteSpritePath))
            {
                var texture = new Texture2D(4, 4, TextureFormat.RGBA32, false);
                var pixels = new Color[16];
                for (var i = 0; i < pixels.Length; i++)
                {
                    pixels[i] = Color.white;
                }

                texture.SetPixels(pixels);
                texture.Apply();
                File.WriteAllBytes(WhiteSpritePath, texture.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(texture);
            }

            AssetDatabase.ImportAsset(WhiteSpritePath);
            var importer = (TextureImporter)AssetImporter.GetAtPath(WhiteSpritePath);
            importer.textureType = TextureImporterType.Sprite;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(WhiteSpritePath);
        }

        private static void DestroyLocomotion()
        {
            foreach (var behaviour in UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (behaviour == null)
                {
                    continue;
                }

                if (UiInteractionRig.IsLocomotionType(behaviour.GetType().Name))
                {
                    UnityEngine.Object.DestroyImmediate(behaviour);
                }
            }
        }

        private static void ConfigureOvrProject()
        {
            var configType = FindType("OVRProjectConfig");
            if (configType == null)
            {
                Debug.LogWarning("OVRProjectConfig type not found.");
                return;
            }

            UnityEngine.Object config = null;
            var cached = configType.GetProperty("CachedProjectConfig") ?? configType.GetProperty("Instance");
            if (cached != null)
            {
                config = cached.GetValue(null) as UnityEngine.Object;
            }

            if (config == null)
            {
                var guids = AssetDatabase.FindAssets("t:OVRProjectConfig");
                if (guids.Length > 0)
                {
                    config = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AssetDatabase.GUIDToAssetPath(guids[0]));
                }
            }

            if (config == null)
            {
                Debug.LogWarning("OVRProjectConfig asset not found.");
                return;
            }

            var so = new SerializedObject(config);
            SetIntIfPresent(so, "_insightPassthroughSupport", 2);
            SetEnumIfPresent(so, "insightPassthroughSupport", "Required");
            SetEnumIfPresent(so, "handTrackingSupport", "ControllersOnly");
            SetBoolIfPresent(so, "disableBackups", true);
            SetBoolIfPresent(so, "enableNSCConfig", false);
            SetBoolIfPresent(so, "isPassthroughCameraAccessEnabled", false);
            DisableFeatureList(so, "passthroughCameraAccess", "anchorSupport", "sceneSupport",
                "bodyTrackingSupport", "faceTrackingSupport", "eyeTrackingSupport");
            SelectQuest3Targets(so);
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
        }

        private static void SelectQuest3Targets(SerializedObject so)
        {
            var targets = so.FindProperty("targetDeviceTypes") ?? so.FindProperty("targetDevices");
            if (targets == null || !targets.isArray)
            {
                Debug.LogWarning("OVRProjectConfig.targetDeviceTypes missing or not an array.");
                return;
            }

            targets.ClearArray();
            AddNamedEnum(targets, "Quest3", "Quest_3", "MetaQuest3");
            AddNamedEnum(targets, "Quest3S", "Quest_3S", "MetaQuest3S");
            if (targets.arraySize == 0)
            {
                targets.arraySize = 2;
                targets.GetArrayElementAtIndex(0).intValue = 4;
                targets.GetArrayElementAtIndex(1).intValue = 5;
            }

            Debug.Log("OVRProjectConfig targetDeviceTypes size=" + targets.arraySize);
        }

        private static void AddNamedEnum(SerializedProperty array, params string[] names)
        {
            var index = array.arraySize;
            array.InsertArrayElementAtIndex(index);
            var element = array.GetArrayElementAtIndex(index);
            if (element.propertyType == SerializedPropertyType.Enum)
            {
                var enumNames = element.enumNames;
                for (var i = 0; i < enumNames.Length; i++)
                {
                    foreach (var name in names)
                    {
                        if (string.Equals(enumNames[i], name, StringComparison.OrdinalIgnoreCase))
                        {
                            element.enumValueIndex = i;
                            return;
                        }
                    }
                }
            }

            array.DeleteArrayElementAtIndex(index);
        }

        private static void FixProjectSetupTasks(BuildTargetGroup group)
        {
            var setupType = FindType("OVRProjectSetup");
            if (setupType == null)
            {
                throw new InvalidOperationException("OVRProjectSetup not found. Meta XR Core SDK Editor scripts missing.");
            }

            var getRuntimeFields = typeof(System.Reflection.RuntimeReflectionExtensions).GetMethod("GetRuntimeFields");
            System.Reflection.FieldInfo registryField = null;
            foreach (var fieldObj in (IEnumerable)getRuntimeFields.Invoke(null, new object[] { setupType }))
            {
                var field = fieldObj as System.Reflection.FieldInfo;
                if (field != null && field.Name == "_principalRegistry")
                {
                    registryField = field;
                    break;
                }
            }

            if (registryField == null)
            {
                var fixAll = setupType.GetMethod("FixAllAsync");
                fixAll.Invoke(null, new object[] { group });
                PumpEditorUpdates(8000);
                return;
            }

            var registry = registryField.GetValue(null);
            System.Reflection.FieldInfo tasksField = null;
            foreach (var fieldObj in (IEnumerable)getRuntimeFields.Invoke(null, new object[] { registry.GetType() }))
            {
                var field = fieldObj as System.Reflection.FieldInfo;
                if (field != null && field.Name == "_tasks")
                {
                    tasksField = field;
                    break;
                }
            }

            var tasks = tasksField.GetValue(registry) as IList;
            if (tasks == null)
            {
                return;
            }

            foreach (var task in tasks)
            {
                if (task == null)
                {
                    continue;
                }

                var taskType = task.GetType();
                var platform = taskType.GetProperty("Platform")?.GetValue(task);
                if (platform != null && platform.ToString() != "Unknown" && platform.ToString() != group.ToString())
                {
                    continue;
                }

                if (!IsTaskValid(task, group) || IsTaskDone(task, group))
                {
                    continue;
                }

                var level = GetOptional(task, "Level", group);
                if (level != "Required" && level != "Recommended")
                {
                    continue;
                }

                var message = GetOptional(task, "Message", group) ?? taskType.Name;
                if (ShouldSkipSetupTask(message))
                {
                    IgnoreTask(task, group);
                    Debug.Log("Skipping Project Setup task: " + message);
                    continue;
                }

                var fixAction = taskType.GetProperty("FixAction")?.GetValue(task) as Delegate;
                if (fixAction != null)
                {
                    try
                    {
                        fixAction.DynamicInvoke(group);
                        Debug.Log("Fixed Project Setup (" + level + "): " + message);
                    }
                    catch (Exception exception)
                    {
                        Debug.LogWarning("Could not auto-fix '" + message + "': " + Unwrap(exception));
                    }
                }
            }

            PumpEditorUpdates(2000);
        }

        private static bool ShouldSkipSetupTask(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return false;
            }

            return message.IndexOf("Oculus XR", StringComparison.OrdinalIgnoreCase) >= 0
                || message.IndexOf("Platform SDK", StringComparison.OrdinalIgnoreCase) >= 0
                || message.IndexOf("App ID", StringComparison.OrdinalIgnoreCase) >= 0
                || message.IndexOf("application ID", StringComparison.OrdinalIgnoreCase) >= 0
                || message.IndexOf("package name", StringComparison.OrdinalIgnoreCase) >= 0
                || message.IndexOf("Data Use Checkup", StringComparison.OrdinalIgnoreCase) >= 0
                || message.IndexOf("internet", StringComparison.OrdinalIgnoreCase) >= 0
                || message.IndexOf("locomotion", StringComparison.OrdinalIgnoreCase) >= 0
                || message.IndexOf("Splash Screen", StringComparison.OrdinalIgnoreCase) >= 0
                || message.IndexOf("XR Simulator", StringComparison.OrdinalIgnoreCase) >= 0
                || message.IndexOf("Meta XR Operator", StringComparison.OrdinalIgnoreCase) >= 0
                || message.IndexOf("API Layers", StringComparison.OrdinalIgnoreCase) >= 0
                || message.IndexOf("SpaceWarp", StringComparison.OrdinalIgnoreCase) >= 0
                || message.IndexOf("Space Warp", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static void IgnoreSkippedSetupTasks(BuildTargetGroup group)
        {
            foreach (var task in EnumerateSetupTasks())
            {
                if (task == null)
                {
                    continue;
                }

                var platform = task.GetType().GetProperty("Platform")?.GetValue(task);
                if (platform != null && platform.ToString() != "Unknown" && platform.ToString() != group.ToString())
                {
                    continue;
                }

                if (!IsTaskValid(task, group) || IsTaskDone(task, group))
                {
                    continue;
                }

                var message = GetOptional(task, "Message", group);
                if (ShouldSkipSetupTask(message))
                {
                    IgnoreTask(task, group);
                }
            }
        }

        private static void IgnoreTask(object task, BuildTargetGroup group)
        {
            var setIgnored = task.GetType().GetMethod("SetIgnored");
            if (setIgnored == null)
            {
                return;
            }

            try
            {
                setIgnored.Invoke(task, new object[] { group, true });
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Could not ignore Project Setup task: " + Unwrap(exception));
            }
        }

        private static IList EnumerateSetupTasks()
        {
            var setupType = FindType("OVRProjectSetup");
            if (setupType == null)
            {
                return Array.Empty<object>();
            }

            var getRuntimeFields = typeof(System.Reflection.RuntimeReflectionExtensions).GetMethod("GetRuntimeFields");
            System.Reflection.FieldInfo registryField = null;
            foreach (var fieldObj in (IEnumerable)getRuntimeFields.Invoke(null, new object[] { setupType }))
            {
                var field = fieldObj as System.Reflection.FieldInfo;
                if (field != null && field.Name == "_principalRegistry")
                {
                    registryField = field;
                    break;
                }
            }

            if (registryField == null)
            {
                return Array.Empty<object>();
            }

            var registry = registryField.GetValue(null);
            System.Reflection.FieldInfo tasksField = null;
            foreach (var fieldObj in (IEnumerable)getRuntimeFields.Invoke(null, new object[] { registry.GetType() }))
            {
                var field = fieldObj as System.Reflection.FieldInfo;
                if (field != null && field.Name == "_tasks")
                {
                    tasksField = field;
                    break;
                }
            }

            return tasksField?.GetValue(registry) as IList ?? Array.Empty<object>();
        }

        private static void LogRemainingSetupTasks(BuildTargetGroup group)
        {
            var setupType = FindType("OVRProjectSetup");
            if (setupType == null)
            {
                return;
            }

            var getRuntimeFields = typeof(System.Reflection.RuntimeReflectionExtensions).GetMethod("GetRuntimeFields");
            System.Reflection.FieldInfo registryField = null;
            foreach (var fieldObj in (IEnumerable)getRuntimeFields.Invoke(null, new object[] { setupType }))
            {
                var field = fieldObj as System.Reflection.FieldInfo;
                if (field != null && field.Name == "_principalRegistry")
                {
                    registryField = field;
                    break;
                }
            }

            if (registryField == null)
            {
                return;
            }

            var registry = registryField.GetValue(null);
            System.Reflection.FieldInfo tasksField = null;
            foreach (var fieldObj in (IEnumerable)getRuntimeFields.Invoke(null, new object[] { registry.GetType() }))
            {
                var field = fieldObj as System.Reflection.FieldInfo;
                if (field != null && field.Name == "_tasks")
                {
                    tasksField = field;
                    break;
                }
            }

            var tasks = tasksField.GetValue(registry) as IList;
            var remaining = new StringBuilder();
            foreach (var task in tasks)
            {
                if (task == null)
                {
                    continue;
                }

                var platform = task.GetType().GetProperty("Platform")?.GetValue(task);
                if (platform != null && platform.ToString() != "Unknown" && platform.ToString() != group.ToString())
                {
                    continue;
                }

                if (!IsTaskValid(task, group) || IsTaskDone(task, group) || IsTaskIgnored(task, group))
                {
                    continue;
                }

                var level = GetOptional(task, "Level", group);
                if (level != "Required" && level != "Recommended")
                {
                    continue;
                }

                var message = GetOptional(task, "Message", group);
                if (ShouldSkipSetupTask(message))
                {
                    continue;
                }

                remaining.AppendLine(level + ": " + message);
            }

            if (remaining.Length == 0)
            {
                Debug.Log("Project Setup Tool: no remaining required/recommended Android tasks.");
            }
            else
            {
                Debug.LogWarning("Project Setup Tool remaining tasks:\n" + remaining);
            }
        }

        private static void GenerateAndSanitizeManifest()
        {
            var preprocessor = FindType("OVRManifestPreprocessor");
            if (preprocessor != null)
            {
                var method = preprocessor.GetMethod("GenerateOrUpdateAndroidManifest")
                    ?? preprocessor.GetMethod("GenerateAndroidManifest");
                if (method != null)
                {
                    try
                    {
                        var parameters = method.GetParameters();
                        if (parameters.Length == 1 && parameters[0].ParameterType == typeof(bool))
                        {
                            method.Invoke(null, new object[] { true });
                        }
                        else if (parameters.Length == 0)
                        {
                            method.Invoke(null, null);
                        }
                    }
                    catch (Exception exception)
                    {
                        Debug.LogWarning("Manifest generate via OVRManifestPreprocessor failed: " + Unwrap(exception));
                    }
                }
            }

            if (!File.Exists(ManifestPath))
            {
                WriteSideloadManifest();
            }
            else
            {
                SanitizeManifest(ManifestPath);
            }

            QuestSplashSetup.ConfigureEngineSplash();
            AssetDatabase.Refresh();
        }

        private static void WriteSideloadManifest()
        {
            var directory = Path.GetDirectoryName(ManifestPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(ManifestPath, SideloadManifestXml());
            AssetDatabase.ImportAsset(ManifestPath);
        }

        private static void SanitizeManifest(string path)
        {
            var text = File.ReadAllText(path);
            if (text.IndexOf("xmlns:tools", StringComparison.Ordinal) < 0)
            {
                text = text.Replace(
                    "<manifest",
                    "<manifest xmlns:tools=\"http://schemas.android.com/tools\"");
            }

            text = RemovePermission(text, "android.permission.INTERNET");
            text = RemovePermission(text, "android.permission.ACCESS_NETWORK_STATE");
            text = RemovePermission(text, "android.permission.CAMERA");
            text = RemovePermission(text, "horizonos.permission.HEADSET_CAMERA");
            text = RemovePermission(text, "com.oculus.permission.EYE_TRACKING");
            if (text.IndexOf("android:allowBackup", StringComparison.Ordinal) < 0)
            {
                text = text.Replace("<application", "<application android:allowBackup=\"false\"");
            }
            else
            {
                text = System.Text.RegularExpressions.Regex.Replace(
                    text,
                    "android:allowBackup=\"true\"",
                    "android:allowBackup=\"false\"");
            }

            if (text.IndexOf("tools:node=\"remove\"", StringComparison.Ordinal) < 0
                || text.IndexOf("android.permission.INTERNET", StringComparison.Ordinal) < 0)
            {
                text = text.Replace(
                    "</manifest>",
                    "    <uses-permission android:name=\"android.permission.INTERNET\" tools:node=\"remove\" />\n" +
                    "    <uses-permission android:name=\"android.permission.ACCESS_NETWORK_STATE\" tools:node=\"remove\" />\n" +
                    "</manifest>");
            }

            File.WriteAllText(path, text);
        }

        private static string RemovePermission(string manifest, string permission)
        {
            return System.Text.RegularExpressions.Regex.Replace(
                manifest,
                "\\s*<uses-permission[^>]*android:name=\"" + permission + "\"[^>]*/>",
                string.Empty);
        }

        private static string SideloadManifestXml()
        {
            return
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<manifest xmlns:android=\"http://schemas.android.com/apk/res/android\"\n" +
                "    xmlns:tools=\"http://schemas.android.com/tools\">\n" +
                "    <application android:allowBackup=\"false\" />\n" +
                "    <uses-permission android:name=\"android.permission.INTERNET\" tools:node=\"remove\" />\n" +
                "    <uses-permission android:name=\"android.permission.ACCESS_NETWORK_STATE\" tools:node=\"remove\" />\n" +
                "</manifest>\n";
        }

        private static GameObject FindOrInstantiatePrefab(string assetName, string instanceName)
        {
            var existing = GameObject.Find(instanceName);
            if (existing != null)
            {
                return existing;
            }

            foreach (var found in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (found.name == instanceName || found.name.Contains(assetName))
                {
                    return found.gameObject;
                }
            }

            var guids = AssetDatabase.FindAssets(assetName + " t:Prefab");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.IndexOf("PassthroughCamera", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    continue;
                }

                if (!Path.GetFileNameWithoutExtension(path).Equals(assetName, StringComparison.OrdinalIgnoreCase)
                    && path.IndexOf(assetName, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                {
                    continue;
                }

                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                instance.name = instanceName;
                return instance;
            }

            return null;
        }

        private static void DestroyNamed(string name)
        {
            var found = GameObject.Find(name);
            if (found != null)
            {
                UnityEngine.Object.DestroyImmediate(found);
            }
        }

        private static void AddComponentByName(GameObject host, string typeName)
        {
            if (host.GetComponent(typeName) != null)
            {
                return;
            }

            var type = FindType(typeName);
            if (type != null)
            {
                host.AddComponent(type);
            }
        }

        private static Type FindType(string fullOrShortName)
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

        private static void SetIntIfPresent(SerializedObject so, string property, int value)
        {
            var found = so.FindProperty(property);
            if (found != null && (found.propertyType == SerializedPropertyType.Integer || found.propertyType == SerializedPropertyType.Enum))
            {
                found.intValue = value;
            }
        }

        private static void SetBoolIfPresent(SerializedObject so, string property, bool value)
        {
            var found = so.FindProperty(property);
            if (found != null && found.propertyType == SerializedPropertyType.Boolean)
            {
                found.boolValue = value;
            }
        }

        private static void SetEnumIfPresent(SerializedObject so, string property, params string[] names)
        {
            var found = so.FindProperty(property);
            if (found == null)
            {
                return;
            }

            if (found.propertyType == SerializedPropertyType.Enum)
            {
                var namesOnProp = found.enumNames;
                foreach (var name in names)
                {
                    for (var i = 0; i < namesOnProp.Length; i++)
                    {
                        if (string.Equals(namesOnProp[i], name, StringComparison.OrdinalIgnoreCase))
                        {
                            found.enumValueIndex = i;
                            return;
                        }
                    }
                }
            }
            else if (found.propertyType == SerializedPropertyType.Integer)
            {
                foreach (var name in names)
                {
                    var index = EnumIndex(found.type, names);
                    if (index >= 0)
                    {
                        found.intValue = index;
                        return;
                    }
                }
            }
        }

        private static void DisableFeatureList(SerializedObject so, params string[] names)
        {
            foreach (var name in names)
            {
                var property = so.FindProperty(name);
                if (property == null)
                {
                    continue;
                }

                if (property.propertyType == SerializedPropertyType.Boolean)
                {
                    property.boolValue = false;
                }
                else if (property.propertyType == SerializedPropertyType.Enum)
                {
                    property.enumValueIndex = 0;
                }
            }
        }

        private static int EnumIndex(string typeName, params string[] names)
        {
            var type = FindType(typeName);
            if (type == null || !type.IsEnum)
            {
                return -1;
            }

            foreach (var name in names)
            {
                foreach (var value in Enum.GetNames(type))
                {
                    if (string.Equals(value, name, StringComparison.OrdinalIgnoreCase))
                    {
                        return (int)Enum.Parse(type, value);
                    }
                }
            }

            return -1;
        }

        private static bool IsTaskValid(object task, BuildTargetGroup group)
        {
            var valid = task.GetType().GetProperty("Valid")?.GetValue(task);
            if (valid == null)
            {
                return true;
            }

            var getValue = valid.GetType().GetMethod("GetValue");
            if (getValue == null)
            {
                return true;
            }

            var result = getValue.Invoke(valid, new object[] { group });
            if (result is bool flag)
            {
                return flag;
            }

            return result == null || Convert.ToBoolean(result);
        }

        private static bool IsTaskDone(object task, BuildTargetGroup group)
        {
            var isDone = task.GetType().GetProperty("IsDone")?.GetValue(task);
            if (isDone == null)
            {
                return false;
            }

            if (isDone is Delegate del)
            {
                return (bool)del.DynamicInvoke(group);
            }

            var invoke = isDone.GetType().GetMethod("Invoke");
            return invoke != null && (bool)invoke.Invoke(isDone, new object[] { group });
        }

        private static bool IsTaskIgnored(object task, BuildTargetGroup group)
        {
            var isIgnored = task.GetType().GetMethod("IsIgnored");
            if (isIgnored == null)
            {
                return false;
            }

            try
            {
                return (bool)isIgnored.Invoke(task, new object[] { group });
            }
            catch
            {
                return false;
            }
        }

        private static string GetOptional(object task, string property, BuildTargetGroup group)
        {
            var value = task.GetType().GetProperty(property)?.GetValue(task);
            if (value == null)
            {
                return null;
            }

            var getValue = value.GetType().GetMethod("GetValue");
            if (getValue != null)
            {
                return getValue.Invoke(value, new object[] { group })?.ToString();
            }

            return value.ToString();
        }

        private static void PumpEditorUpdates(int milliseconds)
        {
            var tick = typeof(EditorApplication).GetMethod(
                "Internal_CallUpdateFunctions",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var until = DateTime.UtcNow.AddMilliseconds(milliseconds);
            while (DateTime.UtcNow < until)
            {
                tick?.Invoke(null, null);
                System.Threading.Thread.Sleep(50);
            }
        }

        private static Exception Unwrap(Exception exception)
        {
            return exception is System.Reflection.TargetInvocationException tie && tie.InnerException != null
                ? tie.InnerException
                : exception;
        }
    }
}
