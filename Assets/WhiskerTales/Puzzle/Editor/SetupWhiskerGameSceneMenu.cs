#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameVanilla.Core;
using GameVanilla.Game.Common;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WhiskerTales.EditorTools
{
    public static class SetupWhiskerGameSceneMenu
    {
        private const string KitGameScenePath = "Assets/Vendor/CandyMatch3Kit/Scenes/GameScene.unity";
        private const string WhiskerScenePath = "Assets/WhiskerTales/Puzzle/Skin/Scenes/WhiskerGameScene.unity";
        private const string WhiskerVariantDir = "Assets/WhiskerTales/Puzzle/Skin/WhiskerTilePrefabs";

        [MenuItem("WhiskerTales/Puzzle/Setup Whisker Game Scene")]
        public static void SetupWhiskerGameScene()
        {
            EnsureFolder(Path.GetDirectoryName(WhiskerScenePath).Replace('\\', '/'));

            if (!File.Exists(KitGameScenePath))
            {
                Debug.LogError($"[WhiskerGameScene] Kit GameScene not found at {KitGameScenePath}");
                return;
            }

            bool sceneExists = File.Exists(WhiskerScenePath);
            if (sceneExists)
            {
                bool overwrite = EditorUtility.DisplayDialog(
                    "WhiskerGameScene already exists",
                    $"{WhiskerScenePath} already exists.\n\n[Overwrite] re-copies from Kit and re-applies swaps (loses any manual scene edits).\n[Re-swap only] keeps the existing scene and only re-applies the 6 prefab swaps.",
                    "Overwrite",
                    "Re-swap only");

                if (overwrite)
                {
                    AssetDatabase.DeleteAsset(WhiskerScenePath);
                    sceneExists = false;
                }
            }

            if (!sceneExists)
            {
                if (!AssetDatabase.CopyAsset(KitGameScenePath, WhiskerScenePath))
                {
                    Debug.LogError($"[WhiskerGameScene] CopyAsset failed: {KitGameScenePath} -> {WhiskerScenePath}");
                    return;
                }
                Debug.Log($"[WhiskerGameScene] Copied Kit GameScene to {WhiskerScenePath}");
            }

            var scene = EditorSceneManager.OpenScene(WhiskerScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"[WhiskerGameScene] Failed to open scene at {WhiskerScenePath}");
                return;
            }

            var tilePool = FindTilePoolInScene(scene);
            if (tilePool == null)
            {
                Debug.LogError("[WhiskerGameScene] TilePool component not found in scene");
                return;
            }

            int swapped = SwapBasicCandyPools(tilePool);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            int buildIndex = AddSceneToBuildSettings(WhiskerScenePath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[WhiskerGameScene] Done. swapped={swapped}/6, buildIndex={buildIndex}, scene={WhiskerScenePath}");
        }

        private static TilePool FindTilePoolInScene(Scene scene)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                var tp = root.GetComponentInChildren<TilePool>(includeInactive: true);
                if (tp != null) return tp;
            }
            return null;
        }

        private static int SwapBasicCandyPools(TilePool tilePool)
        {
            var swaps = new (ObjectPool pool, string variantName)[]
            {
                (tilePool.blueCandyPool,   "FishTile"),
                (tilePool.greenCandyPool,  "MilkTile"),
                (tilePool.orangeCandyPool, "YarnTile"),
                (tilePool.purpleCandyPool, "CatnipTile"),
                (tilePool.redCandyPool,    "PawprintTile"),
                (tilePool.yellowCandyPool, "FishboneTile"),
            };

            int count = 0;
            foreach (var (pool, variantName) in swaps)
            {
                if (pool == null)
                {
                    Debug.LogWarning($"[WhiskerGameScene] {variantName}: ObjectPool reference is null in TilePool — skipped");
                    continue;
                }

                string variantPath = $"{WhiskerVariantDir}/{variantName}.prefab";
                var variant = AssetDatabase.LoadAssetAtPath<GameObject>(variantPath);
                if (variant == null)
                {
                    Debug.LogWarning($"[WhiskerGameScene] {variantName}: Variant prefab not found at {variantPath} — skipped (run 'Create Cat Tile Variants' first)");
                    continue;
                }

                Undo.RecordObject(pool, $"Swap {variantName}");
                pool.prefab = variant;
                EditorUtility.SetDirty(pool);
                count++;
                Debug.Log($"[WhiskerGameScene] Swapped {pool.name}.prefab -> {variantName}");
            }
            return count;
        }

        private static int AddSceneToBuildSettings(string scenePath)
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            int existing = scenes.FindIndex(s => s.path == scenePath);
            if (existing >= 0)
            {
                if (!scenes[existing].enabled)
                {
                    scenes[existing] = new EditorBuildSettingsScene(scenePath, true);
                    EditorBuildSettings.scenes = scenes.ToArray();
                }
                return existing;
            }

            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            return scenes.Count - 1;
        }

        private static void EnsureFolder(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath)) return;

            string parent = Path.GetDirectoryName(assetPath).Replace('\\', '/');
            string leaf = Path.GetFileName(assetPath);

            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
#endif
