using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace WhiskerTales.Editor
{
    /// <summary>
    /// 한글 SDF Font Asset 자동 생성 + TMP_Settings 정적 등록.
    /// V2_ARCHITECTURE 원칙: "TMP Settings에 NotoSansKR SDF 고정 등록 (런타임 주입 X)" 충족.
    ///
    /// 처리 항목:
    ///   1. NotoSansKR-Regular.ttf 로드
    ///   2. SDF Font Asset 생성 (8192x8192, multi-atlas)
    ///   3. ASCII + 한글 11,172자 + 부호 사전 베이크
    ///   4. atlasPopulationMode = Static (런타임 추가 차단)
    ///   5. TMP_Settings.fallbackFontAssets 정적 등록 (중복 체크)
    ///
    /// Idempotent — 다시 클릭하면 기존 .asset 삭제 후 재생성.
    /// </summary>
    public static class GenerateKoreanSDFFont
    {
        private const string SourceFontPath = "Assets/Fonts/Resources/NotoSansKR-Regular.ttf";
        private const string OutputAssetPath = "Assets/Fonts/NotoSansKR_SDF.asset";
        private const string TmpSettingsPath = "Assets/TextMesh Pro/Resources/TMP Settings.asset";

        // SDF 품질/사이즈 균형 — 한글 11,172자 + 부호 + ASCII는 multi-atlas로 자동 분할됨.
        // 8192x8192/90pt → ~33MB SDF, 4096x4096/60pt → ~15MB SDF (multi-atlas 4~5장).
        // 60pt는 일반 본문(36~54pt)에서 충분. 큰 타이틀(>72pt)은 약간 흐려짐.
        private const int SamplingPointSize = 60;
        private const int AtlasPadding = 4;
        private const int AtlasWidth = 4096;
        private const int AtlasHeight = 4096;

        [MenuItem("Whisker Tales/Setup/Generate Korean SDF Font")]
        public static void Run()
        {
            EditorUtility.DisplayProgressBar("Generate Korean SDF", "Loading source font…", 0.05f);

            try
            {
                Font font = AssetDatabase.LoadAssetAtPath<Font>(SourceFontPath);

                if (font == null)
                {
                    Fail("Source font not found: " + SourceFontPath);
                    return;
                }

                EditorUtility.DisplayProgressBar("Generate Korean SDF", "Creating SDF font asset (8192x8192)…", 0.20f);

                TMP_FontAsset asset = TMP_FontAsset.CreateFontAsset(
                    font,
                    SamplingPointSize,
                    AtlasPadding,
                    GlyphRenderMode.SDFAA,
                    AtlasWidth,
                    AtlasHeight,
                    AtlasPopulationMode.Dynamic,
                    true);

                if (asset == null)
                {
                    Fail("TMP_FontAsset.CreateFontAsset returned null.");
                    return;
                }

                asset.name = "NotoSansKR_SDF";

                EditorUtility.DisplayProgressBar("Generate Korean SDF", "Baking ASCII + Hangul (~12K glyphs) — 1~2분 소요…", 0.40f);

                uint[] codePoints = BuildCharacterSet();
                asset.TryAddCharacters(codePoints, out uint[] missingUnicodes);
                int missingCount = missingUnicodes != null ? missingUnicodes.Length : 0;
                int bakedCount = codePoints.Length - missingCount;

                // V2 ARCH 원칙: 런타임 추가 차단
                asset.atlasPopulationMode = AtlasPopulationMode.Static;

                EditorUtility.DisplayProgressBar("Generate Korean SDF", "Saving asset + sub-assets…", 0.70f);

                if (File.Exists(OutputAssetPath) == true)
                {
                    AssetDatabase.DeleteAsset(OutputAssetPath);
                }

                AssetDatabase.CreateAsset(asset, OutputAssetPath);

                if (asset.atlasTexture != null)
                {
                    asset.atlasTexture.name = asset.name + "_Atlas";
                    AssetDatabase.AddObjectToAsset(asset.atlasTexture, asset);
                }

                if (asset.material != null)
                {
                    asset.material.name = asset.name + "_Material";
                    AssetDatabase.AddObjectToAsset(asset.material, asset);
                }

                EditorUtility.SetDirty(asset);
                AssetDatabase.SaveAssetIfDirty(asset);
                AssetDatabase.ImportAsset(OutputAssetPath);

                EditorUtility.DisplayProgressBar("Generate Korean SDF", "Registering in TMP_Settings.fallbackFontAssets…", 0.90f);

                bool registered = RegisterInTmpSettings(asset);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorUtility.ClearProgressBar();

                string regNote = registered == true
                    ? "✓ TMP_Settings.fallbackFontAssets 등록"
                    : "⚠ TMP_Settings 등록 실패 — 수동 등록 필요";

                EditorUtility.DisplayDialog(
                    "Generate Korean SDF Font",
                    "완료\n\n" +
                    "✓ " + OutputAssetPath + "\n" +
                    "  baked=" + bakedCount + " / requested=" + codePoints.Length + " (missing=" + missingCount + ")\n" +
                    regNote + "\n\n" +
                    "이제 재빌드하면 한글이 정상 표시됩니다.\n" +
                    "추정 빌드 사이즈 증가: 10~30MB (SDF atlas + 압축)",
                    "확인");

                Debug.Log("[Generate Korean SDF] Done. baked=" + bakedCount + ", missing=" + missingCount + ", registered=" + registered);
            }
            catch (System.Exception e)
            {
                EditorUtility.ClearProgressBar();
                Fail("예외: " + e.GetType().Name + ": " + e.Message + "\n" + e.StackTrace);
            }
        }

        // -----------------------------------------------------------------
        // 베이크 대상 글리프 — Unicode 범위 기반
        // -----------------------------------------------------------------
        private static uint[] BuildCharacterSet()
        {
            List<uint> list = new List<uint>(13000);

            // ASCII printable (0x20-0x7E)
            for (uint c = 0x0020; c <= 0x007E; c++) { list.Add(c); }

            // Latin-1 Supplement printable (0xA0-0xFF) — 일부 부호
            for (uint c = 0x00A0; c <= 0x00FF; c++) { list.Add(c); }

            // CJK Symbols and Punctuation (3000-303F) — 한국어/일본어 부호, 따옴표
            for (uint c = 0x3000; c <= 0x303F; c++) { list.Add(c); }

            // Hangul Compatibility Jamo (3130-318F)
            for (uint c = 0x3130; c <= 0x318F; c++) { list.Add(c); }

            // Hangul Syllables (AC00-D7A3) — 11,172자 전체
            for (uint c = 0xAC00; c <= 0xD7A3; c++) { list.Add(c); }

            // Halfwidth and Fullwidth Forms (FF00-FFEF) — 전각 부호/숫자
            for (uint c = 0xFF00; c <= 0xFFEF; c++) { list.Add(c); }

            return list.ToArray();
        }

        // -----------------------------------------------------------------
        // TMP_Settings.fallbackFontAssets에 정적 등록 (중복 체크)
        // -----------------------------------------------------------------
        private static bool RegisterInTmpSettings(TMP_FontAsset newAsset)
        {
            TMP_Settings settings = AssetDatabase.LoadAssetAtPath<TMP_Settings>(TmpSettingsPath);

            if (settings == null)
            {
                Debug.LogWarning("[Generate Korean SDF] TMP_Settings asset not found at " + TmpSettingsPath);
                return false;
            }

            SerializedObject so = new SerializedObject(settings);
            SerializedProperty list = so.FindProperty("m_fallbackFontAssets");

            if (list == null)
            {
                Debug.LogWarning("[Generate Korean SDF] m_fallbackFontAssets property not found.");
                return false;
            }

            for (int i = 0; i < list.arraySize; i++)
            {
                Object existing = list.GetArrayElementAtIndex(i).objectReferenceValue;

                if (existing == newAsset)
                {
                    Debug.Log("[Generate Korean SDF] Already in TMP_Settings.fallbackFontAssets — kept.");
                    return true;
                }
            }

            list.arraySize++;
            list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = newAsset;
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(settings);
            Debug.Log("[Generate Korean SDF] Registered NotoSansKR_SDF in TMP_Settings.fallbackFontAssets.");
            return true;
        }

        private static void Fail(string message)
        {
            EditorUtility.ClearProgressBar();
            Debug.LogError("[Generate Korean SDF] " + message);
            EditorUtility.DisplayDialog("Generate Korean SDF Font — 실패", message, "확인");
        }
    }
}
