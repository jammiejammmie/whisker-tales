using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;

public class Builder
{
    public static void BuildAndroid()
    {
        // 빌드 경로 설정
        string buildPath = "build/Android/Whisker Tales.apk";
        string buildDirectory = Path.GetDirectoryName(buildPath);
        
        // 디렉토리 생성
        if (!Directory.Exists(buildDirectory))
        {
            Directory.CreateDirectory(buildDirectory);
        }

        // 빌드 설정
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes);
        buildPlayerOptions.locationPathName = buildPath;
        buildPlayerOptions.target = BuildTarget.Android;
        buildPlayerOptions.options = BuildOptions.None;

        // 플레이어 설정
        PlayerSettings.productName = "Whisker Tales";
        PlayerSettings.bundleIdentifier = "com.nyangstudio.whiskertales";
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel34;
        PlayerSettings.Android.useCustomKeystore = true;

        // 빌드 실행
        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + buildPath);
        }
        else if (summary.result == BuildResult.Failed)
        {
            Debug.LogError("Build failed!");
        }
    }
}
