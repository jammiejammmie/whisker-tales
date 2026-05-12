using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WhiskerTales.Bootstrap;
using WhiskerTales.Navigation;
using WhiskerTales.Platform;
using WhiskerTales.Runtime;

namespace WhiskerTales.EditorTools
{
    /// <summary>
    /// V2-2: Boot_Persistent.unity + Main_App.unity 자동 생성 + Build Settings 등록.
    /// 메뉴: Whisker Tales/Setup/Generate V2 Scenes
    /// 기존 씬은 절대 미터치. 새 씬 두 개만 생성.
    /// </summary>
    public static class GenerateV2Scenes
    {
        private const string BootScenePath = "Assets/WhiskerTales/_Project/Scenes/Boot_Persistent.unity";
        private const string MainScenePath = "Assets/WhiskerTales/_Project/Scenes/Main_App.unity";

        [MenuItem("Whisker Tales/Setup/Generate V2 Scenes")]
        public static void Run()
        {
            try
            {
                // 현재 작업 중인 씬 보존을 위해 dirty 체크
                if (EditorSceneManager.GetActiveScene().isDirty == true)
                {
                    bool save = EditorUtility.DisplayDialog("Generate V2 Scenes",
                        "현재 씬에 저장되지 않은 변경이 있습니다. 저장 후 진행할까요?",
                        "저장 후 진행", "취소");

                    if (save == false)
                    {
                        return;
                    }

                    EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
                }

                EditorUtility.DisplayProgressBar("Generate V2 Scenes", "Boot_Persistent…", 0.1f);
                CreateBootScene();

                EditorUtility.DisplayProgressBar("Generate V2 Scenes", "Main_App…", 0.5f);
                CreateMainAppScene();

                EditorUtility.DisplayProgressBar("Generate V2 Scenes", "Build Settings…", 0.85f);
                UpdateBuildSettings();

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorUtility.ClearProgressBar();

                EditorUtility.DisplayDialog("Generate V2 Scenes",
                    "완료\n\n" +
                    "✓ " + BootScenePath + "\n" +
                    "✓ " + MainScenePath + "\n" +
                    "✓ Build Settings: idx 0 = Boot_Persistent, idx 1 = Main_App\n\n" +
                    "기존 씬들은 idx 2 이상으로 보존되었습니다.",
                    "확인");

                Debug.Log("[Generate V2 Scenes] Done.");
            }
            catch (System.Exception e)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError("[Generate V2 Scenes] " + e);
                EditorUtility.DisplayDialog("Generate V2 Scenes — 실패", e.Message, "확인");
            }
        }

        private static void EnsureDir(string assetPath)
        {
            string dir = Path.GetDirectoryName(assetPath);

            if (Directory.Exists(dir) == false)
            {
                Directory.CreateDirectory(dir);
            }
        }

        private static void CreateBootScene()
        {
            EnsureDir(BootScenePath);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Camera (크림색 배경)
            GameObject camGo = new GameObject("MainCamera");
            Camera cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.961f, 0.945f, 0.910f);
            camGo.tag = "MainCamera";

            // EventSystem (단일성은 TouchInputGuard가 강제)
            GameObject evGo = new GameObject("EventSystem");
            evGo.AddComponent<EventSystem>();
            evGo.AddComponent<StandaloneInputModule>();

            // RuntimeRoot — V2 시스템 컴포넌트 묶음
            GameObject root = new GameObject("RuntimeRoot");
            root.AddComponent<GameRuntime>();
            root.AddComponent<AndroidRuntimeGuard>();
            root.AddComponent<AndroidSystemBars>();
            root.AddComponent<TouchInputGuard>();
            root.AddComponent<AppLifecycleController>();

            EditorSceneManager.SaveScene(scene, BootScenePath);
        }

        private static void CreateMainAppScene()
        {
            EnsureDir(MainScenePath);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject bootGo = new GameObject("MainAppBootstrap");
            MainAppBootstrap boot = bootGo.AddComponent<MainAppBootstrap>();

            Canvas canvasBG = CreateCanvas("Canvas_Background", 0, false);
            Canvas canvasScreens = CreateCanvas("Canvas_Screens", 100, true);
            Canvas canvasModals = CreateCanvas("Canvas_Modals", 500, true);
            Canvas canvasToasts = CreateCanvas("Canvas_Toasts", 1000, true);

            // SafeAreaRoot (Canvas_Screens 자식)
            GameObject safeArea = new GameObject("SafeAreaRoot", typeof(RectTransform), typeof(SafeAreaController));
            safeArea.transform.SetParent(canvasScreens.transform, false);
            RectTransform sa = safeArea.GetComponent<RectTransform>();
            sa.anchorMin = Vector2.zero;
            sa.anchorMax = Vector2.one;
            sa.offsetMin = Vector2.zero;
            sa.offsetMax = Vector2.zero;

            // ScreenNavigator (SafeAreaRoot 자식)
            GameObject navGo = new GameObject("ScreenNavigator", typeof(RectTransform));
            navGo.transform.SetParent(safeArea.transform, false);
            ScreenNavigator nav = navGo.AddComponent<ScreenNavigator>();
            RectTransform navRect = navGo.GetComponent<RectTransform>();
            navRect.anchorMin = Vector2.zero;
            navRect.anchorMax = Vector2.one;
            navRect.offsetMin = Vector2.zero;
            navRect.offsetMax = Vector2.zero;

            // MainAppBootstrap 와이어링
            SerializedObject so = new SerializedObject(boot);
            so.FindProperty("canvasBackground").objectReferenceValue = canvasBG;
            so.FindProperty("canvasScreens").objectReferenceValue = canvasScreens;
            so.FindProperty("canvasModals").objectReferenceValue = canvasModals;
            so.FindProperty("canvasToasts").objectReferenceValue = canvasToasts;
            so.FindProperty("screenNavigator").objectReferenceValue = nav;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, MainScenePath);
        }

        private static Canvas CreateCanvas(string name, int sortingOrder, bool addRaycaster)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            Canvas c = go.GetComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = sortingOrder;

            CanvasScaler scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            if (addRaycaster == true)
            {
                go.AddComponent<GraphicRaycaster>();
            }

            return c;
        }

        private static void UpdateBuildSettings()
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();
            scenes.Add(new EditorBuildSettingsScene(BootScenePath, true));
            scenes.Add(new EditorBuildSettingsScene(MainScenePath, true));

            EditorBuildSettingsScene[] existing = EditorBuildSettings.scenes;

            for (int i = 0; i < existing.Length; i++)
            {
                if (existing[i] == null)
                {
                    continue;
                }

                if (existing[i].path == BootScenePath || existing[i].path == MainScenePath)
                {
                    continue;
                }

                scenes.Add(existing[i]);
            }

            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
