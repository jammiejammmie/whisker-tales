using UnityEditor;
using UnityEngine;
using System.IO;

public class BuildScript
{
    public static void BuildAndroid()
    {
        Debug.Log("Starting Android APK build...");
        
        // Set build settings
        EditorBuildSettings.scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/SampleScene.unity", true)
        };
        
        // Set player settings
        PlayerSettings.productName = "Whisker Tales";
        PlayerSettings.bundleIdentifier = "com.jammiejammmie.whiskertales";
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel33;
        
        // Create output directory
        string buildPath = "build/outputs/apk/debug";
        if (!Directory.Exists(buildPath))
        {
            Directory.CreateDirectory(buildPath);
        }
        
        // Build APK
        string apkPath = Path.Combine(buildPath, "app-debug.apk");
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = EditorBuildSettings.scenes;
        buildPlayerOptions.locationPathName = apkPath;
        buildPlayerOptions.target = BuildTarget.Android;
        buildPlayerOptions.options = BuildOptions.None;
        
        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;
        
        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Android APK build succeeded!");
            Debug.Log("APK saved to: " + apkPath);
        }
        else
        {
            Debug.LogError("Android APK build failed!");
        }
    }
}
