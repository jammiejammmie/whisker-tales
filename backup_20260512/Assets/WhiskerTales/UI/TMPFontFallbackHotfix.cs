using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace WhiskerTales.UI
{
    public static class TMPFontFallbackHotfix
    {
        private static bool installed;

        // DISABLED 2026-05-12 — Generated/NotoSansKR-Regular SDF.asset이 TMP_Settings에 정적 등록되어 있어
        // 한글 fallback은 이미 충족됨. 본 hotfix는 부팅 시마다 7개 dynamic SDF를 fallback list에 추가해서
        // 정적 SDF의 우선순위를 망가뜨리고 한글이 마젠타 글리프로 표시되는 원인이었음.
        // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Install()
        {
            if (installed) return;
            installed = true;

            try
            {
                List<TMP_FontAsset> list = EnsureFallbackList();
                if (list == null)
                {
                    Debug.LogWarning("[TMPFontFallbackHotfix] Could not access TMP_Settings.fallbackFontAssets — aborting");
                    return;
                }

                // Match AppBootstrap.InstallFontFallbacks naming so its later run becomes a no-op (skipped).
                string[] fontResourceNames =
                {
                    "NotoSansKR-Regular",
                    "NotoSansJP-Regular",
                    "NotoSansSC-Regular",
                    "NotoSansTC-Regular",
                    "NotoSans-Regular",
                    "NotoSansDevanagari-Regular",
                    "NotoSansThai-Regular",
                };

                int registered = 0, skipped = 0, failed = 0;
                foreach (string resName in fontResourceNames)
                {
                    string assetName = $"NotoFallback_{resName}";
                    if (HasFallbackNamed(list, assetName)) { skipped++; continue; }

                    Font font = Resources.Load<Font>(resName);
                    if (font == null)
                    {
                        Debug.LogWarning($"[TMPFontFallbackHotfix] Font resource missing: Resources/{resName}");
                        failed++;
                        continue;
                    }

                    TMP_FontAsset asset = TMP_FontAsset.CreateFontAsset(font);
                    if (asset == null)
                    {
                        Debug.LogWarning($"[TMPFontFallbackHotfix] CreateFontAsset returned null for {resName}");
                        failed++;
                        continue;
                    }

                    asset.name = assetName;
                    asset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
                    list.Add(asset);
                    registered++;
                }

                Debug.Log($"[TMPFontFallbackHotfix] Done. registered={registered}, skipped={skipped}, failed={failed}, totalFallbacks={list.Count}");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[TMPFontFallbackHotfix] Setup failed: {e.GetType().Name}: {e.Message}");
            }
        }

        private static List<TMP_FontAsset> EnsureFallbackList()
        {
            List<TMP_FontAsset> list = TMP_Settings.fallbackFontAssets;
            if (list != null) return list;

            TMP_Settings settings = null;
            try
            {
                PropertyInfo instProp = typeof(TMP_Settings).GetProperty(
                    "instance",
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
                if (instProp != null) settings = instProp.GetValue(null) as TMP_Settings;
            }
            catch { }

            if (settings == null)
            {
                settings = Resources.Load<TMP_Settings>("TMP Settings");
            }

            if (settings == null)
            {
                Debug.LogWarning("[TMPFontFallbackHotfix] TMP_Settings instance unavailable");
                return null;
            }

            FieldInfo fld = typeof(TMP_Settings).GetField(
                "m_fallbackFontAssets",
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (fld == null)
            {
                Debug.LogWarning("[TMPFontFallbackHotfix] m_fallbackFontAssets field not found via reflection");
                return null;
            }

            list = new List<TMP_FontAsset>();
            fld.SetValue(settings, list);
            Debug.Log("[TMPFontFallbackHotfix] Initialized m_fallbackFontAssets via reflection");
            return list;
        }

        private static bool HasFallbackNamed(List<TMP_FontAsset> list, string assetName)
        {
            foreach (var f in list)
            {
                if (f != null && f.name == assetName) return true;
            }
            return false;
        }
    }
}
