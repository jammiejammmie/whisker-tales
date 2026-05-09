using UnityEngine;
using UnityEditor;
using WhiskerTales.Referral;

namespace WhiskerTales.EditorTests
{
    /// <summary>
    /// Editor PASS/FAIL — Tools/Whisker Tales/Test/Referral.
    /// 코드 생성/형식/persistence + 친구 코드 redeem 흐름 검증.
    /// PlayerPrefs backup/restore.
    /// </summary>
    public static class ReferralTests
    {
        [MenuItem("Tools/Whisker Tales/Test/Referral")]
        public static void TestAll()
        {
            int passed = 0, failed = 0;

            // 백업
            string bMy   = PlayerPrefs.GetString(ReferralManager.PREF_MY_CODE, "");
            int    bRdmd = PlayerPrefs.GetInt(ReferralManager.PREF_FRIEND_REDEEMED, 0);
            string bUsed = PlayerPrefs.GetString(ReferralManager.PREF_FRIEND_CODE_USED, "");

            GameObject mgrGo = new GameObject("ReferralTest_Manager");
            try
            {
                ReferralManager r = mgrGo.AddComponent<ReferralManager>();
                r.EnsureInitialized();
                r.DebugReset();

                // ===== Test 1: IsValidFormat 유효/무효 케이스 =====
                bool t1a = ReferralManager.IsValidFormat("NABI-1234");
                bool t1b = ReferralManager.IsValidFormat("BELLA-9999");
                bool t1c = ReferralManager.IsValidFormat("HODU-0000");
                bool t1d = !ReferralManager.IsValidFormat("nabi-1234");      // 소문자 → invalid
                bool t1e = !ReferralManager.IsValidFormat("UNKNOWN-1234");   // 모르는 이름
                bool t1f = !ReferralManager.IsValidFormat("NABI-12");        // 짧은 숫자
                bool t1g = !ReferralManager.IsValidFormat("NABI-12345");     // 긴 숫자
                bool t1h = !ReferralManager.IsValidFormat("");               // 빈 문자열
                bool t1i = !ReferralManager.IsValidFormat("NABI1234");       // 하이픈 누락
                if (t1a && t1b && t1c && t1d && t1e && t1f && t1g && t1h && t1i)
                {
                    Debug.Log("  [PASS] IsValidFormat — 5종 cat name + 4-digit + 하이픈 매칭");
                    passed++;
                }
                else { Debug.LogError($"  [FAIL] IsValidFormat — a={t1a} b={t1b} c={t1c} d={t1d} e={t1e} f={t1f} g={t1g} h={t1h} i={t1i}"); failed++; }

                // ===== Test 2: MyCode 생성 + 형식 + persistence =====
                string code1 = r.MyCode;
                bool fmt = ReferralManager.IsValidFormat(code1);
                string code2 = r.MyCode; // 두 번째 호출
                bool same = code1 == code2;
                if (fmt && same)
                {
                    Debug.Log($"  [PASS] MyCode generated and persisted: {code1}");
                    passed++;
                }
                else { Debug.LogError($"  [FAIL] MyCode — code={code1} fmt={fmt} stable={same}"); failed++; }

                // ===== Test 3: 새 인스턴스에서도 같은 코드 read =====
                Object.DestroyImmediate(mgrGo);
                GameObject mgr2 = new GameObject("ReferralTest_Manager2");
                ReferralManager r2 = mgr2.AddComponent<ReferralManager>();
                r2.EnsureInitialized();
                string code3 = r2.MyCode;
                if (code3 == code1)
                {
                    Debug.Log($"  [PASS] Cross-instance — second manager reads same MyCode={code3}");
                    passed++;
                }
                else { Debug.LogError($"  [FAIL] Cross-instance — first={code1}, second={code3}"); failed++; }

                // ===== Test 4: TryRedeem — own code 거부 =====
                string fail4;
                bool t4 = !r2.TryRedeemFriendCode(code1, out fail4);
                if (t4 && !string.IsNullOrEmpty(fail4))
                {
                    Debug.Log($"  [PASS] Reject own code — \"{fail4}\"");
                    passed++;
                }
                else { Debug.LogError($"  [FAIL] Own code should reject — returned={!t4}, reason={fail4}"); failed++; }

                // ===== Test 5: TryRedeem — invalid format 거부 =====
                string fail5;
                bool t5 = !r2.TryRedeemFriendCode("garbage", out fail5);
                if (t5)
                {
                    Debug.Log($"  [PASS] Reject invalid format — \"{fail5}\"");
                    passed++;
                }
                else { Debug.LogError($"  [FAIL] Invalid format should reject"); failed++; }

                // ===== Test 6: TryRedeem — 유효한 외부 코드 redeem =====
                string foreign = (code1.StartsWith("NABI")) ? "BELLA-7777" : "NABI-7777";
                string fail6;
                bool t6 = r2.TryRedeemFriendCode(foreign, out fail6);
                bool t6b = r2.IsFriendCodeRedeemed;
                if (t6 && t6b)
                {
                    Debug.Log($"  [PASS] Foreign code accepted: {foreign} → IsFriendCodeRedeemed=true");
                    passed++;
                }
                else { Debug.LogError($"  [FAIL] Foreign code should accept — ok={t6} redeemed={t6b} reason={fail6}"); failed++; }

                // ===== Test 7: 두 번째 redeem 시도는 거부 =====
                string fail7;
                bool t7 = !r2.TryRedeemFriendCode("HODU-1111", out fail7);
                if (t7)
                {
                    Debug.Log($"  [PASS] Second redeem rejected — \"{fail7}\"");
                    passed++;
                }
                else { Debug.LogError($"  [FAIL] Second redeem should reject"); failed++; }

                Object.DestroyImmediate(mgr2);

                int total = passed + failed;
                string verdict = (failed == 0) ? "PASS" : "FAIL";
                Debug.Log($"[TEST] Referral: {verdict} ({passed}/{total})");
            }
            finally
            {
                if (mgrGo != null) Object.DestroyImmediate(mgrGo);
                // 백업 복원
                if (string.IsNullOrEmpty(bMy)) PlayerPrefs.DeleteKey(ReferralManager.PREF_MY_CODE);
                else PlayerPrefs.SetString(ReferralManager.PREF_MY_CODE, bMy);
                PlayerPrefs.SetInt(ReferralManager.PREF_FRIEND_REDEEMED, bRdmd);
                if (string.IsNullOrEmpty(bUsed)) PlayerPrefs.DeleteKey(ReferralManager.PREF_FRIEND_CODE_USED);
                else PlayerPrefs.SetString(ReferralManager.PREF_FRIEND_CODE_USED, bUsed);
                PlayerPrefs.Save();
            }
        }
    }
}
