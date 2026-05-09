using UnityEngine;
using UnityEditor;
using WhiskerTales.Currency;
using WhiskerTales.UI;

namespace WhiskerTales.EditorTests
{
    /// <summary>
    /// Editor PASS/FAIL — Tools/Whisker Tales/Test/Meditation Garden.
    /// 평화 포인트 적립 / 100 → 💝 +5 / 모래 픽셀 페인팅 검증.
    /// </summary>
    public static class MeditationGardenTests
    {
        [MenuItem("Tools/Whisker Tales/Test/Meditation Garden")]
        public static void TestAll()
        {
            int passed = 0, failed = 0;

            // Shared test scaffolding: 임시 매니저 + 컨트롤러
            GameObject mgrGo = new GameObject("MeditationTest_Managers");
            GameObject ctrlGo = new GameObject("MeditationTest_Controller");
            try
            {
                CurrencyManager cm = mgrGo.AddComponent<CurrencyManager>();
                // Edit 모드는 Awake가 자동으로 안 불리므로 명시 초기화 (싱글톤 + PlayerPrefs 로드)
                cm.EnsureInitialized();

                MeditationGardenController ctrl = ctrlGo.AddComponent<MeditationGardenController>();

                // 클린 시작
                ctrl.DebugResetPeacePoints();
                int beforeNH = cm.NyangiHeart;

                // ===== Test 1: 평화 포인트 단순 적립 =====
                int before = ctrl.PeacePoints;
                for (int i = 0; i < 5; i++) ctrl.DebugAddPeacePoint();
                int after = ctrl.PeacePoints;
                if (after - before == 5)
                {
                    Debug.Log("  [PASS] Peace point accumulation (+5)");
                    passed++;
                }
                else
                {
                    Debug.LogError($"  [FAIL] Peace point accumulation: expected +5, got +{after - before}");
                    failed++;
                }

                // ===== Test 2: 100 포인트 → 💝 +5 + reset =====
                ctrl.DebugResetPeacePoints();
                int rewardBefore = cm.NyangiHeart;
                for (int i = 0; i < 100; i++) ctrl.DebugAddPeacePoint();
                int rewardAfter = cm.NyangiHeart;
                int rewardGain = rewardAfter - rewardBefore;
                bool peaceReset = ctrl.PeacePoints < ctrl.PointsForReward;
                if (rewardGain >= 5 && peaceReset)
                {
                    Debug.Log($"  [PASS] 100 peace points → 💝 +{rewardGain}, peacePoints reset to {ctrl.PeacePoints}");
                    passed++;
                }
                else
                {
                    Debug.LogError($"  [FAIL] 100 peace points: rewardGain={rewardGain} (≥5?), peacePoints={ctrl.PeacePoints} (<100?)");
                    failed++;
                }

                // ===== Test 3: Drag draw — 픽셀 색상 변경 검증 =====
                ctrl.DebugInitTexture();
                int mid = ctrl.DebugTextureSize / 2;
                Color baseCol = ctrl.DebugSandBaseColor;
                Color targetCol = ctrl.DebugDrawColor;
                Color before3 = ctrl.DebugSampleSandPixel(mid, mid);
                ctrl.DebugDrawAt(new Vector2(0.5f, 0.5f));
                Color after3 = ctrl.DebugSampleSandPixel(mid, mid);
                bool changed = !ColorsApproxEqual(before3, after3, 0.01f);
                bool isDrawColor = ColorsApproxEqual(after3, targetCol, 0.05f);
                if (changed && isDrawColor)
                {
                    Debug.Log($"  [PASS] Drawing changes pixel: {ColorStr(before3)} → {ColorStr(after3)}");
                    passed++;
                }
                else
                {
                    Debug.LogError($"  [FAIL] Drawing: pixel before={ColorStr(before3)}, after={ColorStr(after3)}, expected drawColor={ColorStr(targetCol)}");
                    failed++;
                }

                int total = passed + failed;
                string verdict = (failed == 0) ? "PASS" : "FAIL";
                Debug.Log($"[TEST] Meditation Garden: {verdict} ({passed}/{total})");
            }
            finally
            {
                Object.DestroyImmediate(ctrlGo);
                Object.DestroyImmediate(mgrGo);
            }
        }

        private static bool ColorsApproxEqual(Color a, Color b, float tol)
        {
            return Mathf.Abs(a.r - b.r) < tol &&
                   Mathf.Abs(a.g - b.g) < tol &&
                   Mathf.Abs(a.b - b.b) < tol;
        }

        private static string ColorStr(Color c) => $"({c.r:F2},{c.g:F2},{c.b:F2})";
    }
}
