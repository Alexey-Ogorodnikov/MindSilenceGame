using System;
using MindSilence.Presentation;
using MindSilence.XR;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MindSilence.EditorTools
{
    /// <summary>
    /// Step 09: Session complete overlay on training + highscore day list.
    /// </summary>
    public static class QuestHighscoreSetup
    {
        const string ScenePath = "Assets/Scenes/Bootstrap.unity";
        const string PanelPrefabPath = "Assets/UI/EmptyPanel.prefab";

        [MenuItem("MindSilence/Quest/Apply Highscore (Step 09)")]
        public static void Apply()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var panel = GameObject.Find("WorldSpacePanel");
            if (panel == null)
            {
                throw new InvalidOperationException("WorldSpacePanel is missing. Close steps 02 and 08 first.");
            }

            if (panel.GetComponent<AppHost>() == null)
            {
                panel.AddComponent<AppHost>();
            }

            AppHost.BuildHierarchy(panel.GetComponent<RectTransform>());
            var ring = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/circle.png");
            var training = panel.transform.Find(AppHost.TrainingStubName) as RectTransform;
            TrainingStub.BuildHierarchy(training, ring);
            var highScores = panel.transform.Find(AppHost.HighScoresStubName) as RectTransform;
            HighScoresStub.BuildHierarchy(highScores);
            UiInteractionRig.Ensure(panel.GetComponent<Canvas>());

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
            AssetDatabase.SaveAssets();
            Debug.Log("Quest highscore setup applied (step 09).");
        }
    }
}
