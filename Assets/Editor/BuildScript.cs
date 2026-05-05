using UnityEditor;
using UnityEngine;
using System.IO;

public class BuildScript
{
    public static void BuildAndroid()
    {
        Debug.Log("=== Starting Android APK build ===");
        
        try
        {
            // Set build settings
            EditorBuildSettings.scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/SampleScene.unity", true)
            };
            Debug.Log("✓ Build scenes configured");
            
            // Set player settings
            PlayerSettings.productName = "Whisker Tales";
            PlayerSettings.bundleIdentifier = "com.jammiejammmie.whiskertales";
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel33;
            Debug.Log("✓ Player settings configured");
            
            // Create output directory using absolute path
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string buildPath = Path.Combine(projectRoot, "build", "outputs", "apk", "release");
            
            if (!Directory.Exists(buildPath))
            {
                Directory.CreateDirectory(buildPath);
                Debug.Log("✓ Created build directory: " + buildPath);
            }
            else
            {
                Debug.Log("✓ Build directory exists: " + buildPath);
            }
            
            // Build APK with release configuration
            string apkPath = Path.Combine(buildPath, "app-release.apk");
            Debug.Log("APK output path: " + apkPath);
            
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = EditorBuildSettings.scenes;
            buildPlayerOptions.locationPathName = apkPath;
            buildPlayerOptions.target = BuildTarget.Android;
            buildPlayerOptions.options = BuildOptions.None;
            
            Debug.Log("Starting BuildPipeline...");
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;
            
            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log("✓ Android APK build SUCCEEDED!");
                Debug.Log("✓ APK saved to: " + apkPath);
                Debug.Log("✓ Build time: " + summary.totalBuildTime.ToString("F2") + " seconds");
                Debug.Log("✓ Build size: " + (summary.totalSize / 1024 / 1024).ToString("F2") + " MB");
            }
            else if (summary.result == BuildResult.Failed)
            {
                Debug.LogError("✗ Android APK build FAILED!");
                Debug.LogError("Build errors: " + summary.totalErrors);
                Debug.LogError("Build warnings: " + summary.totalWarnings);
            }
            else
            {
                Debug.LogWarning("⚠ Android APK build was CANCELLED!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("✗ Exception during build: " + e.Message);
            Debug.LogError("Stack trace: " + e.StackTrace);
        }
        
        Debug.Log("=== Build process completed ===");
    }
}
