using UnityEngine;
using UnityEditor;
using WhiskerTales.Bootstrap;

namespace WhiskerTales.EditorTests
{
    /// <summary>
    /// Phase A §7 잔여 이슈 폴리시 테스트.
    /// Tools/Whisker Tales/Test/Detox Text Polish — §7-1 상수 (font/color/alpha) 검증.
    /// Tools/Whisker Tales/Test/Icons — §7-2 5개 PNG 아이콘 로드 검증.
    /// </summary>
    public static class PolishTests
    {
        [MenuItem("Tools/Whisker Tales/Test/Detox Text Polish")]
        public static void TestDetoxPolish()
        {
            int passed = 0, failed = 0;

            // 1. Font size — 14sp → 16sp 상향. point 36 → 42.
            if (Mathf.Approximately(AppBootstrap.DETOX_FONT_SIZE, 42f))
            {
                Debug.Log($"  [PASS] DETOX_FONT_SIZE = {AppBootstrap.DETOX_FONT_SIZE} (16sp 상향)");
                passed++;
            }
            else
            {
                Debug.LogError($"  [FAIL] DETOX_FONT_SIZE = {AppBootstrap.DETOX_FONT_SIZE}, expected 42");
                failed++;
            }

            // 2. Text color — #2C2C2C 차콜
            Color c = AppBootstrap.DETOX_TEXT_COLOR;
            bool charcoalOK = Mathf.Abs(c.r - 0.173f) < 0.01f
                           && Mathf.Abs(c.g - 0.173f) < 0.01f
                           && Mathf.Abs(c.b - 0.173f) < 0.01f;
            if (charcoalOK)
            {
                Debug.Log($"  [PASS] DETOX_TEXT_COLOR = ({c.r:F3},{c.g:F3},{c.b:F3}) ≈ #2C2C2C");
                passed++;
            }
            else
            {
                Debug.LogError($"  [FAIL] DETOX_TEXT_COLOR = ({c.r:F3},{c.g:F3},{c.b:F3}) — not #2C2C2C");
                failed++;
            }

            // 3. Backdrop alpha — 60%
            if (Mathf.Approximately(AppBootstrap.DETOX_BACKDROP_ALPHA, 0.6f))
            {
                Debug.Log($"  [PASS] DETOX_BACKDROP_ALPHA = {AppBootstrap.DETOX_BACKDROP_ALPHA} (60%)");
                passed++;
            }
            else
            {
                Debug.LogError($"  [FAIL] DETOX_BACKDROP_ALPHA = {AppBootstrap.DETOX_BACKDROP_ALPHA}, expected 0.6");
                failed++;
            }

            // 4. Backdrop RGB — #F5F1E8 한지 크림
            Color b = AppBootstrap.DETOX_BACKDROP_RGB;
            bool paperOK = Mathf.Abs(b.r - 0.961f) < 0.01f
                        && Mathf.Abs(b.g - 0.945f) < 0.01f
                        && Mathf.Abs(b.b - 0.910f) < 0.01f;
            if (paperOK)
            {
                Debug.Log($"  [PASS] DETOX_BACKDROP_RGB = ({b.r:F3},{b.g:F3},{b.b:F3}) ≈ #F5F1E8");
                passed++;
            }
            else
            {
                Debug.LogError($"  [FAIL] DETOX_BACKDROP_RGB = ({b.r:F3},{b.g:F3},{b.b:F3}) — not #F5F1E8");
                failed++;
            }

            int total = passed + failed;
            string verdict = (failed == 0) ? "PASS" : "FAIL";
            Debug.Log($"[TEST] Detox Text Polish: {verdict} ({passed}/{total})");
        }

        [MenuItem("Tools/Whisker Tales/Test/Icons")]
        public static void TestIcons()
        {
            int passed = 0, failed = 0;
            string[] names = { "icon_paw", "icon_lock", "icon_heart", "icon_star_filled", "icon_star_empty" };

            // Resources/Sprites/Icons/ 에 있어야 APK 빌드에 포함됨. 런타임 코드와 동일한 Resources.Load 경로로 검증.
            foreach (string name in names)
            {
                string resourcesPath = $"Sprites/Icons/{name}";
                Sprite sp = Resources.Load<Sprite>(resourcesPath);
                Texture2D tex = Resources.Load<Texture2D>(resourcesPath);
                bool exists = sp != null || tex != null;
                if (exists)
                {
                    string srcType = (sp != null) ? "Sprite" : "Texture2D fallback";
                    Debug.Log($"  [PASS] {name} — loaded from Resources/{resourcesPath} as {srcType}");
                    passed++;
                }
                else
                {
                    Debug.LogError($"  [FAIL] {name} — not found at Resources/{resourcesPath}");
                    failed++;
                }
            }

            int total = passed + failed;
            string verdict = (failed == 0) ? "PASS" : "FAIL";
            Debug.Log($"[TEST] Icons: {verdict} ({passed}/{total})");
        }
    }
}
