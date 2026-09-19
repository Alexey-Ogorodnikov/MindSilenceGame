using System;
using System.IO;
using MindSilence.Presentation;
using MindSilence.XR;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MindSilence.EditorTools
{
    /// <summary>
    /// Step 08: training panel bound to GameSession. No Session complete dialog.
    /// </summary>
    public static class QuestTrainingSetup
    {
        const string ScenePath = "Assets/Scenes/Bootstrap.unity";
        const string PanelPrefabPath = "Assets/UI/EmptyPanel.prefab";
        const string CirclePath = "Assets/Art/circle.png";
        const string CircleResourcePath = "Assets/Resources/circle.png";
        const string KotlinCircle =
            "Android_kotlin_app_MVI/app/src/main/res/drawable-nodpi/circle.png";

        [MenuItem("MindSilence/Quest/Apply Training (Step 08)")]
        public static void Apply()
        {
            var ring = ImportCircle();
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var panel = GameObject.Find("WorldSpacePanel");
            if (panel == null)
            {
                throw new InvalidOperationException("WorldSpacePanel is missing. Close steps 02 and 07 first.");
            }

            if (panel.GetComponent<AppHost>() == null)
            {
                panel.AddComponent<AppHost>();
            }

            if (panel.GetComponent<LifecycleBridge>() == null)
            {
                panel.AddComponent<LifecycleBridge>();
            }

            if (panel.GetComponent<HapticBridge>() == null)
            {
                panel.AddComponent<HapticBridge>();
            }

            AppHost.BuildHierarchy(panel.GetComponent<RectTransform>());
            var training = panel.transform.Find(AppHost.TrainingStubName) as RectTransform;
            TrainingStub.BuildHierarchy(training, ring);
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
            Debug.Log("Quest training setup applied (step 08).");
        }

        static Sprite ImportCircle()
        {
            CopyKotlinCircle();
            ImportAsSprite(CirclePath);
            ImportAsSprite(CircleResourcePath);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(CirclePath)
                ?? AssetDatabase.LoadAssetAtPath<Sprite>(CircleResourcePath);
            if (sprite == null)
            {
                throw new InvalidOperationException("circle.png did not import as a Sprite.");
            }

            return sprite;
        }

        static void ImportAsSprite(string assetPath)
        {
            AssetDatabase.ImportAsset(assetPath);
            var importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            if (importer == null)
            {
                throw new InvalidOperationException("Missing " + assetPath);
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
        }

        static void CopyKotlinCircle()
        {
            var repoRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "..", ".."));
            var source = Path.Combine(repoRoot, KotlinCircle.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(source))
            {
                throw new FileNotFoundException("Kotlin circle.png not found.", source);
            }

            CopyCircle(source, Path.Combine(Application.dataPath, "Art", "circle.png"));
            CopyCircle(source, Path.Combine(Application.dataPath, "Resources", "circle.png"));
        }

        static void CopyCircle(string source, string destination)
        {
            var directory = Path.GetDirectoryName(destination);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.Copy(source, destination, overwrite: true);
        }
    }
}
