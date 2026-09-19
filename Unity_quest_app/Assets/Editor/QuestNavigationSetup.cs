using System;
using MindSilence.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MindSilence.EditorTools
{
    /// <summary>
    /// Step 06: AppNavigator host + menu/training/highscore panels on the world-space panel.
    /// Step 07 replaces the menu stub with Training + (i).
    /// </summary>
    public static class QuestNavigationSetup
    {
        const string ScenePath = "Assets/Scenes/Bootstrap.unity";
        const string PanelPrefabPath = "Assets/UI/EmptyPanel.prefab";

        [MenuItem("MindSilence/Quest/Apply Navigation (Step 06)")]
        public static void Apply()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var panel = GameObject.Find("WorldSpacePanel");
            if (panel == null)
            {
                throw new InvalidOperationException("WorldSpacePanel is missing. Close steps 02 and 05 first.");
            }

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
            Debug.Log("Quest navigation setup applied (step 06).");
        }
    }
}
