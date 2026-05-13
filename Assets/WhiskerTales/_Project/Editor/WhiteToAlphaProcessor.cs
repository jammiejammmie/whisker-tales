#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace WhiskerTales.EditorTools
{
    // Shared white-to-alpha sprite preprocessor. Reads a source PNG, samples each pixel, attenuates
    // alpha as RGB approaches pure white. Used by LogoAlphaProcessor and CatAlphaProcessor so both
    // paths share the same threshold math.
    //
    // Threshold model:
    //   brightness = max(r, g, b)
    //   alpha = 0          if brightness >= high
    //         = src.a      if brightness <= low
    //         = src.a*lerp between
    public static class WhiteToAlphaProcessor
    {
        public static bool ProcessFile(string srcPath, string dstPath, float low = 0.85f, float high = 0.97f)
        {
            if (File.Exists(srcPath) == false)
            {
                Debug.LogWarning("[WhiteToAlphaProcessor] Source missing: " + srcPath);
                return false;
            }

            string dstDirectory = Path.GetDirectoryName(dstPath);

            if (string.IsNullOrEmpty(dstDirectory) == false && Directory.Exists(dstDirectory) == false)
            {
                Directory.CreateDirectory(dstDirectory);
            }

            TextureImporter srcImporter = AssetImporter.GetAtPath(srcPath) as TextureImporter;
            bool restoreReadable = false;
            TextureImporterType originalType = TextureImporterType.Sprite;

            if (srcImporter != null)
            {
                originalType = srcImporter.textureType;

                if (srcImporter.isReadable == false)
                {
                    srcImporter.isReadable = true;
                    srcImporter.SaveAndReimport();
                    restoreReadable = true;
                }
            }

            try
            {
                Texture2D src = AssetDatabase.LoadAssetAtPath<Texture2D>(srcPath);

                if (src == null)
                {
                    Debug.LogWarning("[WhiteToAlphaProcessor] Could not load source texture: " + srcPath);
                    return false;
                }

                Texture2D processed = ApplyWhiteToAlpha(src, low, high);
                byte[] png = processed.EncodeToPNG();
                File.WriteAllBytes(dstPath, png);
                Object.DestroyImmediate(processed);

                AssetDatabase.ImportAsset(dstPath, ImportAssetOptions.ForceUpdate);

                TextureImporter outImporter = AssetImporter.GetAtPath(dstPath) as TextureImporter;

                if (outImporter != null)
                {
                    outImporter.textureType = TextureImporterType.Sprite;
                    outImporter.spriteImportMode = SpriteImportMode.Single;
                    outImporter.alphaIsTransparency = true;
                    outImporter.mipmapEnabled = false;
                    outImporter.SaveAndReimport();
                }

                Debug.Log("[WhiteToAlphaProcessor] " + srcPath + " -> " + dstPath);
                return true;
            }
            finally
            {
                if (restoreReadable == true && srcImporter != null)
                {
                    srcImporter.isReadable = false;
                    srcImporter.textureType = originalType;
                    srcImporter.SaveAndReimport();
                }
            }
        }

        private static Texture2D ApplyWhiteToAlpha(Texture2D src, float low, float high)
        {
            int w = src.width;
            int h = src.height;
            Color[] pixels = src.GetPixels();
            Color[] outPixels = new Color[pixels.Length];

            for (int i = 0; i < pixels.Length; i++)
            {
                Color c = pixels[i];
                float brightness = Mathf.Max(c.r, Mathf.Max(c.g, c.b));
                float alphaFactor;

                if (brightness >= high)
                {
                    alphaFactor = 0f;
                }
                else if (brightness <= low)
                {
                    alphaFactor = 1f;
                }
                else
                {
                    float t = (brightness - low) / (high - low);
                    alphaFactor = 1f - t;
                }

                outPixels[i] = new Color(c.r, c.g, c.b, c.a * alphaFactor);
            }

            Texture2D dst = new Texture2D(w, h, TextureFormat.RGBA32, false);
            dst.SetPixels(outPixels);
            dst.Apply(false, false);
            return dst;
        }
    }
}
#endif
