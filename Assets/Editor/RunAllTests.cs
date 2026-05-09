using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace WhiskerTales.EditorTests
{
    /// <summary>
    /// Tools/Whisker Tales/Test/Run All Tests — 모든 테스트를 순차 실행하고 마지막에 합계 출력.
    /// 각 테스트가 emit하는 "[TEST] &lt;name&gt;: PASS|FAIL ..." 라인을 캡처해서 합계 계산.
    /// </summary>
    public static class RunAllTests
    {
        [MenuItem("Tools/Whisker Tales/Test/Run All Tests", priority = 0)]
        public static void RunAll()
        {
            var tests = new List<(Action action, string name)>
            {
                (PuzzleTests.TestCascade,                "Cascade"),
                (PuzzleTests.TestSpecialTileCreation,    "Special Tile Creation"),
                (MeditationGardenTests.TestAll,          "Meditation Garden"),
                (PhotoStudioTests.TestAll,               "Photo Studio"),
                (SettingsTests.TestAll,                  "Settings"),
                (TutorialTests.TestAll,                  "Tutorial"),
                (ReferralTests.TestAll,                  "Referral"),
                (IdleRewardTests.TestAll,                "Idle Reward"),
                (HeartRechargeTests.TestAll,             "Heart Recharge"),
                (PolishTests.TestDetoxPolish,            "Detox Text Polish"),
                (PolishTests.TestIcons,                  "Icons"),
            };

            // 각 테스트가 끝낼 때 emit하는 "[TEST] <name>: PASS|FAIL ..." 라인만 캡처.
            // 서브-테스트의 "  [PASS] xxx" 줄은 prefix가 다르므로 제외됨.
            var results = new List<(string name, bool passed, string detail)>();
            Application.LogCallback handler = (msg, trace, type) =>
            {
                if (string.IsNullOrEmpty(msg)) return;
                if (!msg.StartsWith("[TEST] ")) return;

                int colonIdx = msg.IndexOf(':', 7);
                if (colonIdx < 0) return;
                string name = msg.Substring(7, colonIdx - 7).Trim();
                string after = msg.Substring(colonIdx + 1).TrimStart();
                bool passed = after.StartsWith("PASS");
                results.Add((name, passed, after));
            };

            Debug.Log("===== Run All Tests — start =====");
            Application.logMessageReceived += handler;
            try
            {
                foreach (var (action, name) in tests)
                {
                    try
                    {
                        action.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[TEST] {name}: FAIL — exception {ex.GetType().Name}: {ex.Message}");
                    }
                }
            }
            finally
            {
                Application.logMessageReceived -= handler;
            }

            // 요약 출력
            int passedCount = 0;
            Debug.Log("===== Run All Tests — results =====");
            foreach (var (name, passed, detail) in results)
            {
                string mark = passed ? "✓" : "✗";
                if (passed)
                {
                    Debug.Log($"  {mark} {name}: {detail}");
                    passedCount++;
                }
                else
                {
                    Debug.LogError($"  {mark} {name}: {detail}");
                }
            }

            int total = results.Count;
            int expected = tests.Count;
            if (total < expected)
            {
                Debug.LogWarning($"[SUMMARY] only {total}/{expected} test summaries captured — some tests may have failed silently or didn't emit [TEST] line");
            }

            string verdict = passedCount == expected ? "PASS" : "FAIL";
            Debug.Log($"[SUMMARY] {passedCount}/{expected} {verdict}");
        }
    }
}
