#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace WhiskerTales.EditorTools
{
    // Procedurally generates a soft circular glow sprite for the Home lantern. tutorial_bubble.png
    // was a passable stand-in but its speech-bubble tail breaks the lantern silhouette. This bakes
    // a clean radial-gradient PNG once and saves it as an importable Sprite at edit time.
    public static class LanternGlowTextureGenerator
    {
        public const string SpritePath = "Assets/WhiskerTales/_Project/Art/Generated/lantern_glow.png";
        private const string Directory = "Assets/WhiskerTales/_Project/Art/Generated";
        private const int TextureSize = 512;

        [InitializeOnLoadMethod]
        private static void AutoBuildOnEditorLoad()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode == true)
            {
                return;
            }
            if (File.Exists(SpritePath) == true)
            {
                return;
            }
            EditorApplication.delayCall += TryGenerate;
        }

        private static void TryGenerate()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode == true)
            {
                return;
            }
            if (EditorApplication.isCompiling == true || EditorApplication.isUpdating == true)
            {
                EditorApplication.delayCall += TryGenerate;
                return;
            }
            if (File.Exists(SpritePath) == true)
            {
                return;
            }
            Generate();
        }

        [MenuItem("Whisker Tales/V2/Generate Lantern Glow Texture")]
        public static void Generate()
        {
            if (System.IO.Directory.Exists(Directory) == false)
            {
                System.IO.Directory.CreateDirectory(Directory);
            }

            Texture2D tex = BuildRadialGradient(TextureSize);
            byte[] png = tex.EncodeToPNG();
            File.WriteAllBytes(SpritePath, png);
            Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(SpritePath, ImportAssetOptions.ForceUpdate);

            TextureImporter importer = AssetImporter.GetAtPath(SpritePath) as TextureImporter;

            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.SaveAndReimport();
            }

            Debug.Log("[LanternGlowTextureGenerator] Generated " + SpritePath);
        }

        private static Texture2D BuildRadialGradient(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color32[] pixels = new Color32[size * size];

            float center = (size - 1) * 0.5f;
            float maxRadius = center * 0.96f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float t = Mathf.Clamp01(dist / maxRadius);
                    // Smooth ease-out so the edge feathers rather than cutting off.
                    float alpha = Mathf.Pow(1f - t, 2.4f);
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)(alpha * 255f));
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply(false, false);
            return tex;
        }
    }
}
#endif
