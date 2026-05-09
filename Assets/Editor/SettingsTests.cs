using UnityEngine;
using UnityEditor;
using WhiskerTales.Settings;

namespace WhiskerTales.EditorTests
{
    /// <summary>
    /// Editor PASS/FAIL — Tools/Whisker Tales/Test/Settings.
    /// SettingsManager 속성 set → PlayerPrefs 영구 저장 + 동일/새 인스턴스가 같은 값 read.
    /// </summary>
    public static class SettingsTests
    {
        [MenuItem("Tools/Whisker Tales/Test/Settings")]
        public static void TestAll()
        {
            int passed = 0, failed = 0;

            // 기존 PlayerPrefs 값 백업 (테스트 후 복원)
            int   bDetox       = PlayerPrefs.GetInt(SettingsManager.PREF_DETOX_ENABLED, 1);
            int   bNotif       = PlayerPrefs.GetInt(SettingsManager.PREF_DAILY_NOTIFICATION, 1);
            float bBgm         = PlayerPrefs.GetFloat(SettingsManager.PREF_BGM_VOLUME, 0.5f);
            float bSfx         = PlayerPrefs.GetFloat(SettingsManager.PREF_SFX_VOLUME, 0.7f);
            string bLang       = PlayerPrefs.GetString(SettingsManager.PREF_LANGUAGE, SettingsManager.LANG_KO);

            GameObject mgrGo = new GameObject("SettingsTest_Manager");
            try
            {
                SettingsManager s = mgrGo.AddComponent<SettingsManager>();
                s.EnsureInitialized();

                // Test 1: DetoxModeEnabled write/read round-trip
                s.DetoxModeEnabled = false;
                bool prefDetox = PlayerPrefs.GetInt(SettingsManager.PREF_DETOX_ENABLED, 99) == 0;
                bool getDetox  = s.DetoxModeEnabled == false;
                if (prefDetox && getDetox)
                {
                    Debug.Log("  [PASS] DetoxModeEnabled persists (set false → PlayerPrefs=0 → getter=false)");
                    passed++;
                }
                else { Debug.LogError($"  [FAIL] DetoxModeEnabled — pref={prefDetox}, get={getDetox}"); failed++; }

                // Test 2: BgmVolume float
                s.BgmVolume = 0.42f;
                float prefBgm = PlayerPrefs.GetFloat(SettingsManager.PREF_BGM_VOLUME, -1f);
                float getBgm  = s.BgmVolume;
                if (Mathf.Abs(prefBgm - 0.42f) < 0.001f && Mathf.Abs(getBgm - 0.42f) < 0.001f)
                {
                    Debug.Log($"  [PASS] BgmVolume persists (set 0.42 → PlayerPrefs={prefBgm:F3} → getter={getBgm:F3})");
                    passed++;
                }
                else { Debug.LogError($"  [FAIL] BgmVolume — pref={prefBgm}, get={getBgm}"); failed++; }

                // Test 3: Language (string + invalid → ko fallback)
                s.Language = SettingsManager.LANG_EN;
                string prefLang1 = PlayerPrefs.GetString(SettingsManager.PREF_LANGUAGE, "?");
                string getLang1  = s.Language;
                bool t3a = prefLang1 == SettingsManager.LANG_EN && getLang1 == SettingsManager.LANG_EN;

                s.Language = "garbage_xx";
                string getLang2 = s.Language;
                bool t3b = getLang2 == SettingsManager.LANG_KO; // fallback

                if (t3a && t3b)
                {
                    Debug.Log($"  [PASS] Language persists (en round-trip OK, invalid → ko fallback)");
                    passed++;
                }
                else { Debug.LogError($"  [FAIL] Language — en pref={prefLang1}, get={getLang1}, fallback get={getLang2}"); failed++; }

                // Test 4: 새 SettingsManager 인스턴스도 같은 값 읽음 (PlayerPrefs 진짜 영구화 검증)
                s.BgmVolume = 0.13f;
                Object.DestroyImmediate(mgrGo);
                GameObject mgr2 = new GameObject("SettingsTest_Manager2");
                SettingsManager s2 = mgr2.AddComponent<SettingsManager>();
                s2.EnsureInitialized();
                float reread = s2.BgmVolume;
                Object.DestroyImmediate(mgr2);
                if (Mathf.Abs(reread - 0.13f) < 0.001f)
                {
                    Debug.Log($"  [PASS] Cross-instance persistence (new SettingsManager reads BgmVolume={reread:F3})");
                    passed++;
                }
                else { Debug.LogError($"  [FAIL] Cross-instance — re-read={reread}, expected 0.13"); failed++; }

                int total = passed + failed;
                string verdict = (failed == 0) ? "PASS" : "FAIL";
                Debug.Log($"[TEST] Settings: {verdict} ({passed}/{total})");
            }
            finally
            {
                if (mgrGo != null) Object.DestroyImmediate(mgrGo);
                // 백업 복원
                PlayerPrefs.SetInt(SettingsManager.PREF_DETOX_ENABLED, bDetox);
                PlayerPrefs.SetInt(SettingsManager.PREF_DAILY_NOTIFICATION, bNotif);
                PlayerPrefs.SetFloat(SettingsManager.PREF_BGM_VOLUME, bBgm);
                PlayerPrefs.SetFloat(SettingsManager.PREF_SFX_VOLUME, bSfx);
                PlayerPrefs.SetString(SettingsManager.PREF_LANGUAGE, bLang);
                PlayerPrefs.Save();
            }
        }
    }
}
