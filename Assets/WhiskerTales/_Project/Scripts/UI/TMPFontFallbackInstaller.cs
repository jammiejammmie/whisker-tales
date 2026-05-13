using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using WhiskerTales.Core;

namespace WhiskerTales.UI
{
    // Mirrors AppBootstrap.InstallFontFallbacks for the V2 boot path. LiberationSans SDF (TMP default)
    // only ships Latin glyphs, so Korean/CJK render as □ until Noto fallbacks are registered.
    // Idempotent — safe to call multiple times.
    public static class TMPFontFallbackInstaller
    {
        private const string AssetNamePrefix = "NotoFallback_";

        private static readonly string[] ResourceNames = new string[]
        {
            "NotoSansKR-Regular",
            "NotoSansJP-Regular",
            "NotoSansSC-Regular",
            "NotoSansTC-Regular",
            "NotoSans-Regular",
            "NotoSansDevanagari-Regular",
            "NotoSansThai-Regular"
        };

        private static bool installed;

        public static void EnsureInstalled()
        {
            if (installed == true)
            {
                return;
            }

            try
            {
                List<TMP_FontAsset> fallbacks = TMP_Settings.fallbackFontAssets;

                if (fallbacks == null)
                {
                    DebugLogger.Warning(LogCategory.UI, "[TMPFontFallbackInstaller] TMP_Settings.fallbackFontAssets is null — fallbacks skipped.");
                    return;
                }

                int registered = 0;
                int skipped = 0;
                int failed = 0;

                for (int i = 0; i < ResourceNames.Length; i++)
                {
                    string resName = ResourceNames[i];
                    string assetName = AssetNamePrefix + resName;

                    if (HasFallbackNamed(fallbacks, assetName) == true)
                    {
                        skipped++;
                        continue;
                    }

                    Font font = Resources.Load<Font>(resName);

                    if (font == null)
                    {
                        failed++;
                        continue;
                    }

                    TMP_FontAsset asset = TMP_FontAsset.CreateFontAsset(font);

                    if (asset == null)
                    {
                        failed++;
                        continue;
                    }

                    asset.name = assetName;
                    asset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
                    fallbacks.Add(asset);
                    registered++;
                }

                installed = true;
                DebugLogger.Info(LogCategory.UI, "[TMPFontFallbackInstaller] registered=" + registered + " skipped=" + skipped + " failed=" + failed);
            }
            catch (Exception e)
            {
                DebugLogger.Warning(LogCategory.UI, "[TMPFontFallbackInstaller] Setup failed: " + e.GetType().Name + ": " + e.Message);
            }
        }

        private static bool HasFallbackNamed(List<TMP_FontAsset> list, string assetName)
        {
            for (int i = 0; i < list.Count; i++)
            {
                TMP_FontAsset f = list[i];

                if (f != null && f.name == assetName)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
