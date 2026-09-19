using System;
using MindSilence.Presentation;
using MindSilence.XR;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR.OpenXR;

namespace MindSilence.EditorTools
{
    /// <summary>
    /// Step 07: menu Training + (i), How to train, poke and ray UI, no locomotion.
    /// </summary>
    public static class QuestMenuSetup
    {
        const string ScenePath = "Assets/Scenes/Bootstrap.unity";
        const string PanelPrefabPath = "Assets/UI/EmptyPanel.prefab";

        [MenuItem("MindSilence/Quest/Apply Menu (Step 07)")]
        public static void Apply()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var panel = GameObject.Find("WorldSpacePanel");
            if (panel == null)
            {
                throw new InvalidOperationException("WorldSpacePanel is missing. Close steps 02 and 06 first.");
            }

            if (panel.GetComponent<AppHost>() == null)
            {
                panel.AddComponent<AppHost>();
            }

            AppHost.BuildHierarchy(panel.GetComponent<RectTransform>());
            UiInteractionRig.Ensure(panel.GetComponent<Canvas>());
            EnableHandsAndControllers();
            EnableOpenXrHandTracking(BuildTargetGroup.Android);
            EnableOpenXrHandTracking(BuildTargetGroup.Standalone);

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
            Debug.Log("Quest menu setup applied (step 07).");
        }

        static void EnableHandsAndControllers()
        {
            var configType = FindType("OVRProjectConfig");
            if (configType == null)
            {
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
                return;
            }

            var so = new SerializedObject(config);
            SetEnumIfPresent(so, "handTrackingSupport", "ControllersAndHands", "ControllersAndHands");
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
        }

        static void EnableOpenXrHandTracking(BuildTargetGroup group)
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

                var typeName = feature.GetType().FullName ?? feature.GetType().Name;
                if (typeName.IndexOf("HandTracking", StringComparison.OrdinalIgnoreCase) >= 0
                    || typeName.IndexOf("MetaHand", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    feature.enabled = true;
                    EditorUtility.SetDirty(feature);
                }
            }

            EditorUtility.SetDirty(settings);
        }

        static void SetEnumIfPresent(SerializedObject so, string property, params string[] names)
        {
            var found = so.FindProperty(property);
            if (found == null || found.propertyType != SerializedPropertyType.Enum)
            {
                return;
            }

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
                    // Dynamic assemblies can throw on GetTypes.
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
