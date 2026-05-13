#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WhiskerTales.Platform;
using WhiskerTales.Runtime;

namespace WhiskerTales.EditorTools
{
    public static class BootPersistentSceneBuilder
    {
        private const string ScenePath = "Assets/WhiskerTales/_Project/Scenes/Boot_Persistent.unity";
        private const string SceneDirectory = "Assets/WhiskerTales/_Project/Scenes";
        private static readonly Color CreamBackground = new Color(0.961f, 0.945f, 0.910f);

        [InitializeOnLoadMethod]
        private static void AutoBuildOnEditorLoad()
        {
            if (IsPlayModeActive())
            {
                return;
            }
            if (File.Exists(ScenePath))
            {
                return;
            }
            // Defer to first idle frame so we don't run during asset import or domain reload.
            EditorApplication.delayCall += TryAutoBuild;
        }

        private static void TryAutoBuild()
        {
            if (IsPlayModeActive())
            {
                return;
            }
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += TryAutoBuild;
                return;
            }
            if (File.Exists(ScenePath))
            {
                return;
            }
            BuildInternal("Auto-generated");
        }

        [MenuItem("Whisker Tales/V2/Build Boot_Persistent Scene")]
        public static void Build()
        {
            if (IsPlayModeActive())
            {
                Debug.LogWarning("[BootPersistentSceneBuilder] Skipped: cannot build scene while in Play Mode. Stop playing first.");
                return;
            }
            BuildInternal("Rebuilt");
        }

        private static bool IsPlayModeActive()
        {
            if (EditorApplication.isPlaying)
            {
                return true;
            }
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return true;
            }
            return false;
        }

        private static void BuildInternal(string verb)
        {
            if (!Directory.Exists(SceneDirectory))
            {
                Directory.CreateDirectory(SceneDirectory);
            }

            // Additive so the user's currently open scene is preserved untouched.
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);

            CreateMainCamera(scene);
            CreateRuntimeRoot(scene);
            CreateEventSystem(scene);
            CreateGlobalOverlayCanvas(scene);

            bool saved = EditorSceneManager.SaveScene(scene, ScenePath);
            EditorSceneManager.CloseScene(scene, true);

            if (!saved)
            {
                Debug.LogError("[BootPersistentSceneBuilder] Failed to save scene at " + ScenePath);
                return;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[BootPersistentSceneBuilder] " + verb + " Boot_Persistent.unity at " + ScenePath
                + ". Open File > Build Settings and drag this scene to index 0 before building for Android.");
        }

        private static void CreateMainCamera(Scene targetScene)
        {
            GameObject cameraGo = new GameObject("MainCamera");
            cameraGo.tag = "MainCamera";
            Camera cam = cameraGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = CreamBackground;
            cam.orthographic = false;
            cam.depth = 0f;
            cam.allowHDR = false;
            cam.allowMSAA = false;
            cameraGo.AddComponent<AudioListener>();
            SceneManager.MoveGameObjectToScene(cameraGo, targetScene);
        }

        private static void CreateRuntimeRoot(Scene targetScene)
        {
            GameObject root = new GameObject("RuntimeRoot");
            root.AddComponent<GameRuntime>();

            GameObject androidGuardGo = new GameObject("AndroidRuntimeGuard");
            androidGuardGo.transform.SetParent(root.transform, false);
            androidGuardGo.AddComponent<AndroidRuntimeGuard>();

            GameObject touchGuardGo = new GameObject("TouchInputGuard");
            touchGuardGo.transform.SetParent(root.transform, false);
            touchGuardGo.AddComponent<TouchInputGuard>();

            SceneManager.MoveGameObjectToScene(root, targetScene);
        }

        private static void CreateEventSystem(Scene targetScene)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
            SceneManager.MoveGameObjectToScene(es, targetScene);
        }

        private static void CreateGlobalOverlayCanvas(Scene targetScene)
        {
            GameObject canvasGo = new GameObject("GlobalOverlayCanvas");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;

            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 2400f);
            scaler.matchWidthOrHeight = 0.5f;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

            SceneManager.MoveGameObjectToScene(canvasGo, targetScene);
        }
    }
}
#endif
