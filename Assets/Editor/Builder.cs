using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;

public class Builder
{
    public static void BuildAndroid()
    {
        string buildPath = "build/Android/WhiskerTales.apk";
        string buildDirectory = Path.GetDirectoryName(buildPath);

        if (!Directory.Exists(buildDirectory))
        {
            Directory.CreateDirectory(buildDirectory);
        }

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new string[] { "Assets/Scenes/MainMenu.unity" };
        buildPlayerOptions.locationPathName = buildPath;
        buildPlayerOptions.target = BuildTarget.Android;
        buildPlayerOptions.options = BuildOptions.None;

        PlayerSettings.productName = "Whisker Tales";
        PlayerSettings.applicationIdentifier = "com.nyangstudio.whiskertales";
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel34;

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + buildPath);
        }
        else
        {
            Debug.LogError("Build failed!");
        }
    }
}
