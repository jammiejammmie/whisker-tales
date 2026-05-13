using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using WhiskerTales.EditorTools;

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

        // V2 runtime: Boot_Persistent (idx 0) -> Main_App (idx 1, additive via GameRuntime.Start).
        public const string OUTPUT_PATH_V2     = @"C:\whisker-tales-master\whisker-tales\WhiskerTalesV2.apk";
        public const string SCENE_PATH_V2_BOOT = "Assets/WhiskerTales/_Project/Scenes/Boot_Persistent.unity";
        public const string SCENE_PATH_V2_MAIN = "Assets/WhiskerTales/_Project/Scenes/Main_App.unity";

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

        [MenuItem("Tools/Whisker Tales/Build Android APK (V2)")]
        public static void BuildAndroidV2Menu()
        {
            DoBuildV2();
        }

        public static void BuildAndroidV2Batch()
        {
            BuildReport report = DoBuildV2();
            bool ok = report != null && report.summary.result == BuildResult.Succeeded;
            EditorApplication.Exit(ok ? 0 : 1);
        }

        private static BuildReport DoBuildV2()
        {
            try
            {
                ConfigurePlayerSettings();
                EnsureOutputDir(OUTPUT_PATH_V2);
                EnsureV2ScenesExist();

                BuildPlayerOptions opts = new BuildPlayerOptions
                {
                    scenes = new[] { SCENE_PATH_V2_BOOT, SCENE_PATH_V2_MAIN },
                    locationPathName = OUTPUT_PATH_V2,
                    target = BuildTarget.Android,
                    targetGroup = BuildTargetGroup.Android,
                    options = BuildOptions.None,
                };

                BuildReport report = BuildPipeline.BuildPlayer(opts);
                BuildSummary s = report.summary;

                long sizeMb = (long)(s.totalSize / (1024 * 1024));
                Debug.Log($"[BuildScript:V2] Result={s.result} time={s.totalTime} sizeApprox={sizeMb}MB errors={s.totalErrors} warnings={s.totalWarnings}");
                Debug.Log($"[BuildScript:V2] Output: {OUTPUT_PATH_V2}");

                if (s.result == BuildResult.Succeeded && File.Exists(OUTPUT_PATH_V2))
                {
                    long actualBytes = new FileInfo(OUTPUT_PATH_V2).Length;
                    Debug.Log($"[BuildScript:V2] APK on disk: {actualBytes / (1024.0 * 1024.0):F2} MB ({actualBytes} bytes)");
                }
                return report;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BuildScript:V2] Build threw: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
                return null;
            }
        }

        private static void EnsureV2ScenesExist()
        {
            EnsureV2BuildSettings();

            if (!File.Exists(SCENE_PATH_V2_BOOT))
            {
                Debug.Log("[BuildScript:V2] Boot_Persistent.unity missing — invoking builder.");
                BootPersistentSceneBuilder.Build();
            }

            // V2-10: ensure procedural lantern texture exists before HomeScreen prefab rebuild.
            if (!File.Exists(LanternGlowTextureGenerator.SpritePath))
            {
                Debug.Log("[BuildScript:V2] Generating lantern glow texture (V2-10).");
                LanternGlowTextureGenerator.Generate();
            }

            // V2-11: white-to-alpha processed logo (HomeScreen uses this if present).
            if (!File.Exists(LogoAlphaProcessor.OutputPath))
            {
                Debug.Log("[BuildScript:V2] Processing logo alpha (V2-11).");
                LogoAlphaProcessor.Process();
            }

            // V2-12: white-to-alpha processed cat (CatRoom uses this if present).
            if (!File.Exists(CatAlphaProcessor.OutputPath))
            {
                Debug.Log("[BuildScript:V2] Processing cat alpha (V2-12).");
                CatAlphaProcessor.Process();
            }

            // All V2 screen prefabs + Main_App scene are always rebuilt so the latest wiring
            // (registrations, navigator refs, controller fields) is baked into the APK.
            Debug.Log("[BuildScript:V2] Rebuilding HomeScreen prefab (V2-5).");
            HomeScreenPrefabBuilder.Build();

            Debug.Log("[BuildScript:V2] Rebuilding V2-6 screen prefabs (Gameplay/CatRoom/Cafe/LevelClear/GameFail).");
            V2ScreenPrefabBuilders.BuildAll();

            Debug.Log("[BuildScript:V2] Rebuilding Main_App.unity to re-wire screens.");
            MainAppSceneBuilder.Build();

            AssetDatabase.Refresh();
        }

        // Force EditorBuildSettings.scenes to exactly the two V2 scenes.
        // Guards Build And Run / Editor Play from picking up V1 scene residue
        // (e.g. Assets/Scenes/MainScenes.unity). DoBuildV2 itself overrides the
        // list via BuildPlayerOptions.scenes, so this is for the rest of the editor.
        private static void EnsureV2BuildSettings()
        {
            EditorBuildSettingsScene[] target = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene(SCENE_PATH_V2_BOOT, true),
                new EditorBuildSettingsScene(SCENE_PATH_V2_MAIN, true),
            };

            EditorBuildSettingsScene[] current = EditorBuildSettings.scenes;
            bool needsUpdate = false;

            if (current.Length != target.Length)
            {
                needsUpdate = true;
            }
            else
            {
                for (int i = 0; i < target.Length; i++)
                {
                    if (current[i].path != target[i].path || current[i].enabled != target[i].enabled)
                    {
                        needsUpdate = true;
                        break;
                    }
                }
            }

            if (needsUpdate == true)
            {
                Debug.Log("[BuildScript:V2] Resetting EditorBuildSettings.scenes to V2 (was " + current.Length + " entries, now " + target.Length + ").");
                EditorBuildSettings.scenes = target;
            }
        }

        private static BuildReport DoBuild()
        {
            try
            {
                ConfigurePlayerSettings();
                EnsureOutputDir(OUTPUT_PATH);

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

        private static void EnsureOutputDir(string targetPath)
        {
            string dir = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }
    }
}
