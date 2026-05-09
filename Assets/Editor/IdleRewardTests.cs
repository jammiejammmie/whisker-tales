using System;
using UnityEngine;
using UnityEditor;
using WhiskerTales.Sleep;

namespace WhiskerTales.EditorTests
{
    /// <summary>
    /// Editor PASS/FAIL — Tools/Whisker Tales/Test/Idle Reward.
    /// PREF_ENTRY_TIME 세팅 → ProcessPendingOfflineSleep → 보상 struct 검증.
    /// PlayerPrefs backup/restore.
    /// </summary>
    public static class IdleRewardTests
    {
        [MenuItem("Tools/Whisker Tales/Test/Idle Reward")]
        public static void TestAll()
        {
            int passed = 0, failed = 0;

            string backup = PlayerPrefs.GetString(SleepModeManager.PREF_ENTRY_TIME, "");
            GameObject mgrGo = new GameObject("IdleRewardTest_Manager");
            try
            {
                SleepModeManager mgr = mgrGo.AddComponent<SleepModeManager>();
                mgr.EnsureInitialized();

                // ===== Test 1: PREF 없을 때 default 반환 =====
                PlayerPrefs.DeleteKey(SleepModeManager.PREF_ENTRY_TIME);
                PlayerPrefs.Save();
                SleepModeManager.SleepReward t1 = mgr.ProcessPendingOfflineSleep();
                if (t1.hours == 0f && t1.anchovies == 0 && t1.hearts == 0)
                {
                    Debug.Log("  [PASS] No pending entry → default reward (hours=0)");
                    passed++;
                }
                else { Debug.LogError($"  [FAIL] No pending — got hours={t1.hours} anchovies={t1.anchovies} hearts={t1.hearts}"); failed++; }

                // ===== Test 2: 2시간 전 entry → 적정 보상 =====
                DateTime t2Start = DateTime.UtcNow.AddHours(-2);
                PlayerPrefs.SetString(SleepModeManager.PREF_ENTRY_TIME, t2Start.ToString("o"));
                PlayerPrefs.Save();
                SleepModeManager.SleepReward t2 = mgr.ProcessPendingOfflineSleep();
                bool t2OK = t2.hours >= 1.99f && t2.hours <= 2.01f
                         && t2.anchovies == 20    // 2h × 10
                         && t2.affinity == 4      // 120min / 30 = 4
                         && t2.hearts == SleepModeManager.HEARTS_PER_SESSION
                         && t2.nyangiHeart == 0;  // < 8h
                if (t2OK)
                {
                    Debug.Log($"  [PASS] 2h offline → {t2.anchovies} 멸치 / {t2.affinity} 호감도 / {t2.hearts} ❤ / {t2.nyangiHeart} 💝");
                    passed++;
                }
                else { Debug.LogError($"  [FAIL] 2h reward — hours={t2.hours} anchovies={t2.anchovies} affinity={t2.affinity} hearts={t2.hearts} 💝={t2.nyangiHeart}"); failed++; }

                // ===== Test 3: PREF cleared after process =====
                bool prefGone = string.IsNullOrEmpty(PlayerPrefs.GetString(SleepModeManager.PREF_ENTRY_TIME, ""));
                if (prefGone)
                {
                    Debug.Log("  [PASS] PREF_ENTRY_TIME cleared after process");
                    passed++;
                }
                else { Debug.LogError("  [FAIL] PREF_ENTRY_TIME still set after process"); failed++; }

                // ===== Test 4: 10시간 전 entry → 8h 캡 + 💝 +10 =====
                DateTime t4Start = DateTime.UtcNow.AddHours(-10);
                PlayerPrefs.SetString(SleepModeManager.PREF_ENTRY_TIME, t4Start.ToString("o"));
                PlayerPrefs.Save();
                SleepModeManager.SleepReward t4 = mgr.ProcessPendingOfflineSleep();
                bool t4OK = t4.hours >= 7.99f && t4.hours <= 8.01f   // capped
                         && t4.anchovies == 80                        // 8h × 10
                         && t4.affinity == 16                         // 480min / 30 = 16
                         && t4.hearts == SleepModeManager.HEARTS_PER_SESSION
                         && t4.nyangiHeart == SleepModeManager.NYANGI_HEART_FULL;
                if (t4OK)
                {
                    Debug.Log($"  [PASS] 10h offline (capped 8h) → {t4.anchovies} 멸치 / {t4.affinity} 호감도 / {t4.hearts} ❤ / {t4.nyangiHeart} 💝");
                    passed++;
                }
                else { Debug.LogError($"  [FAIL] 10h cap — hours={t4.hours} anchovies={t4.anchovies} affinity={t4.affinity} hearts={t4.hearts} 💝={t4.nyangiHeart}"); failed++; }

                // ===== Test 5: 잘못된 PREF → default + 정리 =====
                PlayerPrefs.SetString(SleepModeManager.PREF_ENTRY_TIME, "garbage_invalid_iso");
                PlayerPrefs.Save();
                SleepModeManager.SleepReward t5 = mgr.ProcessPendingOfflineSleep();
                bool t5Cleared = string.IsNullOrEmpty(PlayerPrefs.GetString(SleepModeManager.PREF_ENTRY_TIME, ""));
                if (t5.hours == 0f && t5Cleared)
                {
                    Debug.Log("  [PASS] Invalid PREF → default reward + key cleared");
                    passed++;
                }
                else { Debug.LogError($"  [FAIL] Invalid PREF — hours={t5.hours} cleared={t5Cleared}"); failed++; }

                int total = passed + failed;
                string verdict = (failed == 0) ? "PASS" : "FAIL";
                Debug.Log($"[TEST] Idle Reward: {verdict} ({passed}/{total})");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mgrGo);
                if (string.IsNullOrEmpty(backup))
                    PlayerPrefs.DeleteKey(SleepModeManager.PREF_ENTRY_TIME);
                else
                    PlayerPrefs.SetString(SleepModeManager.PREF_ENTRY_TIME, backup);
                PlayerPrefs.Save();
            }
        }
    }
}
