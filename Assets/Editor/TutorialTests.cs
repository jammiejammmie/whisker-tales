using UnityEngine;
using UnityEditor;
using WhiskerTales.UI;

namespace WhiskerTales.EditorTests
{
    /// <summary>
    /// Editor PASS/FAIL — Tools/Whisker Tales/Test/Tutorial.
    /// 레벨별 PlayerPrefs gating, MarkSeen, 범위 검증.
    /// 백업/복원으로 사용자 PlayerPrefs 보호.
    /// </summary>
    public static class TutorialTests
    {
        [MenuItem("Tools/Whisker Tales/Test/Tutorial")]
        public static void TestAll()
        {
            int passed = 0, failed = 0;

            // 백업
            int b1 = PlayerPrefs.GetInt(TutorialOverlay.KeyForLevel(1), 0);
            int b2 = PlayerPrefs.GetInt(TutorialOverlay.KeyForLevel(2), 0);
            int b3 = PlayerPrefs.GetInt(TutorialOverlay.KeyForLevel(3), 0);

            try
            {
                TutorialOverlay.DebugClearAll();

                // ===== Test 1: 클린 상태에서 IsLevelSeen은 모두 false =====
                bool t1 = !TutorialOverlay.IsLevelSeen(1) &&
                          !TutorialOverlay.IsLevelSeen(2) &&
                          !TutorialOverlay.IsLevelSeen(3);
                if (t1)
                {
                    Debug.Log("  [PASS] Cleared state — Level 1/2/3 IsLevelSeen=false");
                    passed++;
                }
                else { Debug.LogError("  [FAIL] Clear state — some level still marked seen"); failed++; }

                // ===== Test 2: MarkLevelSeen 후 IsLevelSeen=true =====
                TutorialOverlay.MarkLevelSeen(2);
                bool t2 = TutorialOverlay.IsLevelSeen(2) &&
                          !TutorialOverlay.IsLevelSeen(1) &&
                          !TutorialOverlay.IsLevelSeen(3);
                if (t2)
                {
                    Debug.Log("  [PASS] MarkLevelSeen(2) — only Level 2 seen, others remain false");
                    passed++;
                }
                else { Debug.LogError("  [FAIL] MarkLevelSeen — wrong levels marked"); failed++; }

                // ===== Test 3: TryShowForLevel — unseen=true, seen=false =====
                GameObject go = new GameObject("TutorialTest_Overlay");
                try
                {
                    TutorialOverlay tut = go.AddComponent<TutorialOverlay>();

                    bool unseenShown = tut.DebugTryShow(1);   // not seen → true
                    bool seenShown   = tut.DebugTryShow(2);   // already seen → false

                    if (unseenShown && !seenShown)
                    {
                        Debug.Log("  [PASS] TryShow gating — unseen returns true, seen returns false");
                        passed++;
                    }
                    else { Debug.LogError($"  [FAIL] TryShow gating — unseen={unseenShown}, seen={seenShown}"); failed++; }

                    // ===== Test 4: 범위 밖 (level<1, level>3) 모두 false =====
                    bool oor1 = tut.DebugTryShow(0);
                    bool oor2 = tut.DebugTryShow(4);
                    bool oor3 = tut.DebugTryShow(99);
                    if (!oor1 && !oor2 && !oor3)
                    {
                        Debug.Log("  [PASS] Out-of-range levels (0/4/99) all return false");
                        passed++;
                    }
                    else { Debug.LogError($"  [FAIL] OOR — 0={oor1}, 4={oor2}, 99={oor3}"); failed++; }
                }
                finally
                {
                    Object.DestroyImmediate(go);
                }

                int total = passed + failed;
                string verdict = (failed == 0) ? "PASS" : "FAIL";
                Debug.Log($"[TEST] Tutorial: {verdict} ({passed}/{total})");
            }
            finally
            {
                // 백업 복원
                PlayerPrefs.SetInt(TutorialOverlay.KeyForLevel(1), b1);
                PlayerPrefs.SetInt(TutorialOverlay.KeyForLevel(2), b2);
                PlayerPrefs.SetInt(TutorialOverlay.KeyForLevel(3), b3);
                PlayerPrefs.Save();
            }
        }
    }
}
