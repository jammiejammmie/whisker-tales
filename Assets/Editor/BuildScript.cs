using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace WhiskerTales.EditorBuild
{
    /// <summary>
    /// Android APK 빌드 스크립트.
    /// - 메뉴: Tools/Whisker Tales/Build Android APK (수동, EditorApplication.Exit 안 함)
    /// - Batch: -executeMethod WhiskerTales.EditorBuild.BuildScript.BuildAndroidBatch
    ///   (Unity.exe -batchmode -nographics -quit ...; 종료 코드 0/1로 결과 전달)
    /// </summary>
    public static class BuildScript
    {
        public const string PRODUCT_NAME  = "Whisker Tales";
        public const string COMPANY_NAME  = "NyangStudio";
        public const string PACKAGE_NAME  = "com.nyangstudio.whiskertales";
        public const string OUTPUT_PATH   = @"C:\Builds\WhiskerTales.apk";
        public const string SCENE_PATH    = "Assets/WhiskerTales/Scenes/Main.unity";

        [MenuItem("Tools/Whisker Tales/Build Android APK")]
        public static void BuildAndroidMenu()
        {
            DoBuild();
        }

        /// <summary>Batch mode 진입점 — 종료 코드 0(성공) / 1(실패)로 Unity 종료.</summary>
        public static void BuildAndroidBatch()
        {
            BuildReport report = DoBuild();
            bool ok = report != null && report.summary.result == BuildResult.Succeeded;
            EditorApplication.Exit(ok ? 0 : 1);
        }

        private static BuildReport DoBuild()
        {
            try
            {
                ConfigurePlayerSettings();
                EnsureOutputDir();

                BuildPlayerOptions opts = new BuildPlayerOptions
                {
                    scenes = new[] { SCENE_PATH },
                    locationPathName = OUTPUT_PATH,
                    target = BuildTarget.Android,
                    targetGroup = BuildTargetGroup.Android,
                    options = BuildOptions.None,
                };

                BuildReport report = BuildPipeline.BuildPlayer(opts);
                BuildSummary s = report.summary;

                long sizeMb = (long)(s.totalSize / (1024 * 1024));
                Debug.Log($"[BuildScript] Result={s.result} time={s.totalTime} sizeApprox={sizeMb}MB errors={s.totalErrors} warnings={s.totalWarnings}");
                Debug.Log($"[BuildScript] Output: {OUTPUT_PATH}");

                if (s.result == BuildResult.Succeeded && File.Exists(OUTPUT_PATH))
                {
                    long actualBytes = new FileInfo(OUTPUT_PATH).Length;
                    Debug.Log($"[BuildScript] APK on disk: {actualBytes / (1024.0 * 1024.0):F2} MB ({actualBytes} bytes)");
                }
                return report;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BuildScript] Build threw: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
                return null;
            }
        }

        private static void ConfigurePlayerSettings()
        {
            PlayerSettings.companyName = COMPANY_NAME;
            PlayerSettings.productName = PRODUCT_NAME;
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, PACKAGE_NAME);
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;

            // Google Play 출시 요건: IL2CPP + ARM64 (64-bit) 필수 (2019년 이후).
            // 첫 IL2CPP 빌드는 Android NDK 컴파일로 5~15분 추가 소요. Library 캐시 따뜻해지면 짧아짐.
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        }

        private static void EnsureOutputDir()
        {
            string dir = Path.GetDirectoryName(OUTPUT_PATH);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
        }
    }
}
