#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace WhiskerTales.EditorTools
{
    public static class CreateWhiskerTileVariantsMenu
    {
        private const string KitCandyDir = "Assets/Vendor/CandyMatch3Kit/Prefabs/Candies";
        private const string TileSpriteDir = "Assets/WhiskerTales/Art/Tiles";
        private const string OutputDir = "Assets/WhiskerTales/Puzzle/Skin/WhiskerTilePrefabs";

        private struct TileMapping
        {
            public string kitPrefabName;
            public string tileSpriteName;
            public string variantName;
        }

        private static readonly TileMapping[] Mappings = new[]
        {
            new TileMapping { kitPrefabName = "BlueCandy",   tileSpriteName = "tile_fish",     variantName = "FishTile" },
            new TileMapping { kitPrefabName = "GreenCandy",  tileSpriteName = "tile_milk",     variantName = "MilkTile" },
            new TileMapping { kitPrefabName = "OrangeCandy", tileSpriteName = "tile_yarn",     variantName = "YarnTile" },
            new TileMapping { kitPrefabName = "PurpleCandy", tileSpriteName = "tile_catnip",   variantName = "CatnipTile" },
            new TileMapping { kitPrefabName = "RedCandy",    tileSpriteName = "tile_pawprint", variantName = "PawprintTile" },
            new TileMapping { kitPrefabName = "YellowCandy", tileSpriteName = "tile_fishbone", variantName = "FishboneTile" },
        };

        [MenuItem("WhiskerTales/Puzzle/Create Cat Tile Variants")]
        public static void CreateVariants()
        {
            EnsureFolder(OutputDir);

            var created = new List<string>();
            var skipped = new List<string>();
            var failed = new List<string>();

            foreach (var m in Mappings)
            {
                string parentPath = $"{KitCandyDir}/{m.kitPrefabName}.prefab";
                string spritePath = $"{TileSpriteDir}/{m.tileSpriteName}.png";
                string variantPath = $"{OutputDir}/{m.variantName}.prefab";

                var parentPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(parentPath);
                if (parentPrefab == null)
                {
                    failed.Add($"{m.variantName}: parent prefab not found at {parentPath}");
                    continue;
                }

                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                if (sprite == null)
                {
                    failed.Add($"{m.variantName}: sprite not found at {spritePath}");
                    continue;
                }

                if (File.Exists(variantPath))
                {
                    skipped.Add($"{m.variantName}: already exists at {variantPath}");
                    continue;
                }

                var instance = (GameObject)PrefabUtility.InstantiatePrefab(parentPrefab);
                if (instance == null)
                {
                    failed.Add($"{m.variantName}: failed to instantiate parent prefab");
                    continue;
                }

                try
                {
                    var sr = instance.GetComponent<SpriteRenderer>();
                    if (sr == null)
                    {
                        failed.Add($"{m.variantName}: parent has no SpriteRenderer");
                        continue;
                    }

                    sr.sprite = sprite;

                    var variant = PrefabUtility.SaveAsPrefabAsset(instance, variantPath, out bool success);
                    if (success && variant != null)
                    {
                        created.Add($"{m.variantName} → {variantPath}");
                    }
                    else
                    {
                        failed.Add($"{m.variantName}: SaveAsPrefabAsset reported failure");
                    }
                }
                finally
                {
                    Object.DestroyImmediate(instance);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(BuildReport(created, skipped, failed));
        }

        private static void EnsureFolder(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath)) return;

            string parent = Path.GetDirectoryName(assetPath).Replace('\\', '/');
            string leaf = Path.GetFileName(assetPath);

            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);

            AssetDatabase.CreateFolder(parent, leaf);
        }

        private static string BuildReport(List<string> created, List<string> skipped, List<string> failed)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"[WhiskerTilesVariants] created={created.Count} skipped={skipped.Count} failed={failed.Count}");
            foreach (var s in created) sb.AppendLine($"  + {s}");
            foreach (var s in skipped) sb.AppendLine($"  · {s}");
            foreach (var s in failed) sb.AppendLine($"  ! {s}");
            return sb.ToString();
        }
    }
}
#endif
