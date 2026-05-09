using System;
using UnityEngine;
using UnityEditor;
using WhiskerTales.Heart;

namespace WhiskerTales.EditorTests
{
    /// <summary>
    /// Editor PASS/FAIL — Tools/Whisker Tales/Test/Heart Recharge.
    /// 시간당 충전 / SNS 공유 1일 1회 / PREF persistence 검증. PlayerPrefs backup/restore.
    /// </summary>
    public static class HeartRechargeTests
    {
        [MenuItem("Tools/Whisker Tales/Test/Heart Recharge")]
        public static void TestAll()
        {
            int passed = 0, failed = 0;

            string bRecharge = PlayerPrefs.GetString(HeartRechargeManager.PREF_LAST_RECHARGE, "");
            string bShare    = PlayerPrefs.GetString(HeartRechargeManager.PREF_LAST_SHARE_REWARD, "");

            GameObject mgrGo = new GameObject("HeartTest_Manager");
            try
            {
                HeartRechargeManager mgr = mgrGo.AddComponent<HeartRechargeManager>();
                mgr.EnsureInitialized();

                // ===== Test 1: 첫 실행 (PREF 없음) → 0 hearts + PREF 저장 =====
                mgr.DebugReset();
                int t1 = mgr.ProcessOfflineRecharge();
                bool t1PrefSet = !string.IsNullOrEmpty(PlayerPrefs.GetString(HeartRechargeManager.PREF_LAST_RECHARGE, ""));
                if (t1 == 0 && t1PrefSet)
                {
                    Debug.Log("  [PASS] First run — 0 hearts, PREF stamped");
                    passed++;
                }
                else { Debug.LogError($"  [FAIL] First run — hearts={t1} prefSet={t1PrefSet}"); failed++; }

                // ===== Test 2: 3시간 전 PREF → +3 hearts (정수 시간만) =====
                mgr.DebugSetLastRechargeHoursAgo(3);
                int t2 = mgr.ProcessOfflineRecharge();
                if (t2 == 3 * HeartRechargeManager.HEARTS_PER_HOUR)
                {
                    Debug.Log($"  [PASS] 3h offline → {t2} hearts");
                    passed++;
                }
                else { Debug.LogError($"  [FAIL] 3h offline — got {t2}, expected 3"); failed++; }

                // ===== Test 3: 직후 다시 호출 → 0 (이미 정수 시간 소비됨) =====
                int t3 = mgr.ProcessOfflineRecharge();
                if (t3 == 0)
                {
                    Debug.Log("  [PASS] Immediate re-call → 0 hearts (already consumed)");
                    passed++;
                }
                else { Debug.LogError($"  [FAIL] Re-call should be 0 — got {t3}"); failed++; }

                // ===== Test 4: SNS 공유 첫 호출 → true, 두 번째 → false =====
                PlayerPrefs.DeleteKey(HeartRechargeManager.PREF_LAST_SHARE_REWARD);
                PlayerPrefs.Save();
                bool t4a = mgr.TryAwardShareHeart();
                bool t4b = mgr.TryAwardShareHeart();
                if (t4a && !t4b)
                {
                    Debug.Log("  [PASS] Share heart — first call true, second false (1/day cap)");
                    passed++;
                }
                else { Debug.LogError($"  [FAIL] Share heart — first={t4a}, second={t4b}"); failed++; }

                // ===== Test 5: PREF에 저장된 날짜와 오늘 일치 =====
                string today = DateTime.Now.ToString("yyyy-MM-dd");
                string stored = PlayerPrefs.GetString(HeartRechargeManager.PREF_LAST_SHARE_REWARD, "");
                if (stored == today)
                {
                    Debug.Log($"  [PASS] Share PREF stamped to today ({today})");
                    passed++;
                }
                else { Debug.LogError($"  [FAIL] Share PREF — got '{stored}', expected '{today}'"); failed++; }

                int total = passed + failed;
                string verdict = (failed == 0) ? "PASS" : "FAIL";
                Debug.Log($"[TEST] Heart Recharge: {verdict} ({passed}/{total})");
            }
            finally
            {
                Object.DestroyImmediate(mgrGo);
                if (string.IsNullOrEmpty(bRecharge))
                    PlayerPrefs.DeleteKey(HeartRechargeManager.PREF_LAST_RECHARGE);
                else
                    PlayerPrefs.SetString(HeartRechargeManager.PREF_LAST_RECHARGE, bRecharge);
                if (string.IsNullOrEmpty(bShare))
                    PlayerPrefs.DeleteKey(HeartRechargeManager.PREF_LAST_SHARE_REWARD);
                else
                    PlayerPrefs.SetString(HeartRechargeManager.PREF_LAST_SHARE_REWARD, bShare);
                PlayerPrefs.Save();
            }
        }
    }
}
