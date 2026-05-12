using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WhiskerTales.Editor
{
    /// <summary>
    /// Android 빌드 진입점 — 메뉴 + Codemagic CLI 공통.
    /// 메뉴: "Whisker Tales/Build/Android APK"
    /// CLI: -executeMethod WhiskerTales.Editor.BuildScript.BuildAndroid
    /// </summary>
    public static class BuildScript
    {
        private const string DefaultOutputDir = "Build/Android";
        private const string DefaultApkName = "WhiskerTalesV2.apk";

        // -----------------------------------------------------------------
        // 메뉴 진입점 (Editor)
        // -----------------------------------------------------------------
        [MenuItem("Whisker Tales/Build/Android APK")]
        public static void BuildAndroidFromMenu()
        {
            BuildResult result = BuildAndroidInternal(out string outputPath, out string message);

            if (Application.isBatchMode == true)
            {
                return;
            }

            if (result == BuildResult.Succeeded)
            {
                bool reveal = EditorUtility.DisplayDialog(
                    "Build Android APK",
                    "✅ 빌드 성공\n\n" + message + "\n\n경로:\n" + outputPath,
                    "산출 폴더 열기",
                    "확인");

                if (reveal == true)
                {
                    EditorUtility.RevealInFinder(outputPath);
                }
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "Build Android APK — 실패",
                    "❌ " + result + "\n\n" + message + "\n\n콘솔 로그를 확인하세요.",
                    "확인");
            }
        }

        // -----------------------------------------------------------------
        // CLI 진입점 (Codemagic / Unity batchmode)
        // -----------------------------------------------------------------
        public static void BuildAndroid()
        {
            BuildResult result = BuildAndroidInternal(out string _, out string _);

            if (Application.isBatchMode == true)
            {
                EditorApplication.Exit(result == BuildResult.Succeeded ? 0 : 1);
            }
        }

        // -----------------------------------------------------------------
        // 공통 빌드 로직
        // -----------------------------------------------------------------
        private static BuildResult BuildAndroidInternal(out string outputPath, out string message)
        {
            outputPath = ResolveOutputPath();
            string outputDir = Path.GetDirectoryName(outputPath);

            if (Directory.Exists(outputDir) == false)
            {
                Directory.CreateDirectory(outputDir);
            }

            string[] scenes = ResolveScenes(out string sceneSummary);

            if (scenes.Length == 0)
            {
                message = "빌드할 씬이 없습니다. Build Settings에 씬을 추가하거나, 빌드할 씬을 활성 상태로 두세요.";
                Debug.LogError("[BuildScript] " + message);
                return BuildResult.Failed;
            }

            Debug.Log("[BuildScript] Scenes to build: " + sceneSummary);
            Debug.Log("[BuildScript] Output: " + outputPath);

            ConfigureAndroidPlayerSettings();

            BuildPlayerOptions opts = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(opts);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                long sizeMb = (long)(summary.totalSize / (1024UL * 1024UL));
                message = $"size={sizeMb}MB, time={summary.totalTime.TotalSeconds:F1}s, scenes={scenes.Length}";
                Debug.Log("[BuildScript] ✅ Build succeeded — " + message);
            }
            else
            {
                message = $"errors={summary.totalErrors}, warnings={summary.totalWarnings}";
                Debug.LogError("[BuildScript] ❌ Build " + summary.result + " — " + message);
            }

            return summary.result;
        }

        // -----------------------------------------------------------------
        // Android Player Settings (보수적 설정 — 기존 ProjectSettings 존중)
        // -----------------------------------------------------------------
        private static void ConfigureAndroidPlayerSettings()
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            }

            // APK 산출 (AAB 아님)
            EditorUserBuildSettings.buildAppBundle = false;

            // ARM64 보장 (Google Play 요구사항)
            if ((PlayerSettings.Android.targetArchitectures & AndroidArchitecture.ARM64) == 0)
            {
                PlayerSettings.Android.targetArchitectures |= AndroidArchitecture.ARM64;
            }
        }

        // -----------------------------------------------------------------
        // 씬 결정: EditorBuildSettings의 enabled 우선, 없으면 활성 씬
        // -----------------------------------------------------------------
        private static string[] ResolveScenes(out string summary)
        {
            List<string> list = new List<string>();

            for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
            {
                EditorBuildSettingsScene s = EditorBuildSettings.scenes[i];

                if (s == null)
                {
                    continue;
                }

                if (s.enabled == false)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(s.path) == true)
                {
                    continue;
                }

                list.Add(s.path);
            }

            if (list.Count > 0)
            {
                summary = "EditorBuildSettings (" + list.Count + " enabled): " + string.Join(", ", list);
                return list.ToArray();
            }

            Scene active = SceneManager.GetActiveScene();

            if (string.IsNullOrEmpty(active.path) == false)
            {
                summary = "fallback to active scene: " + active.path;
                return new string[] { active.path };
            }

            summary = "(none)";
            return new string[0];
        }

        // -----------------------------------------------------------------
        // 산출 경로: <projectRoot>/Build/Android/WhiskerTalesV2.apk
        // -----------------------------------------------------------------
        private static string ResolveOutputPath()
        {
            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            string combined = Path.Combine(projectRoot, DefaultOutputDir, DefaultApkName);
            return combined.Replace('\\', '/');
        }
    }
}
