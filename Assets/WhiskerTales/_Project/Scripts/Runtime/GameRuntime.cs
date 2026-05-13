using UnityEngine;
using UnityEngine.SceneManagement;
using WhiskerTales.Core;
using WhiskerTales.Platform;

namespace WhiskerTales.Runtime
{
    [DefaultExecutionOrder(-1000)]
    [DisallowMultipleComponent]
    public sealed class GameRuntime : MonoBehaviour
    {
        public static readonly Color CreamBackground = new Color(0.961f, 0.945f, 0.910f);
        public const int TargetFrameRate = 60;

        public static GameRuntime Instance { get; private set; }

        [SerializeField] private AndroidRuntimeGuard androidGuard;
        [SerializeField] private TouchInputGuard touchGuard;
        [SerializeField] private string mainAppSceneName = "Main_App";
        [SerializeField] private bool autoLoadMainApp = true;

        public bool IsBooted { get; private set; }
        public bool MainAppLoaded { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                DebugLogger.Warning(LogCategory.UI, "[GameRuntime] Duplicate RuntimeRoot detected. Destroying duplicate.", this);
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            Application.targetFrameRate = TargetFrameRate;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Application.runInBackground = false;

            EnsureGuards();
            ApplyPlatformGuards();
            ValidateInput();

            IsBooted = true;
            DebugLogger.Info(LogCategory.UI, "[GameRuntime] Boot complete. targetFps=" + Application.targetFrameRate + " platform=" + Application.platform);
        }

        private void Start()
        {
            if (autoLoadMainApp == false)
            {
                DebugLogger.Info(LogCategory.UI, "[GameRuntime] autoLoadMainApp disabled — skipping Main_App load.");
                return;
            }

            if (IsBooted == false)
            {
                DebugLogger.Warning(LogCategory.UI, "[GameRuntime] Start() before boot. Skipping Main_App load.");
                return;
            }

            if (string.IsNullOrEmpty(mainAppSceneName) == true)
            {
                DebugLogger.Warning(LogCategory.UI, "[GameRuntime] mainAppSceneName empty. Skipping additive load.");
                return;
            }

            Scene existing = SceneManager.GetSceneByName(mainAppSceneName);

            if (existing.IsValid() == true && existing.isLoaded == true)
            {
                MainAppLoaded = true;
                DebugLogger.Info(LogCategory.UI, "[GameRuntime] Main_App already loaded — skipping additive load.");
                return;
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
            DebugLogger.Info(LogCategory.UI, "[GameRuntime] Loading scene additively: " + mainAppSceneName);
            SceneManager.LoadScene(mainAppSceneName, LoadSceneMode.Additive);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != mainAppSceneName)
            {
                return;
            }

            SceneManager.sceneLoaded -= OnSceneLoaded;
            MainAppLoaded = true;

            SceneManager.SetActiveScene(scene);

            if (touchGuard != null)
            {
                // Main_App ships with its own EventSystem so it stays playable solo; prune duplicates now.
                touchGuard.ValidateEventSystem();
            }

            DebugLogger.Info(LogCategory.UI, "[GameRuntime] Main_App loaded and set active. rootGameObjects=" + scene.rootCount);
        }

        private void EnsureGuards()
        {
            if (androidGuard == null)
            {
                androidGuard = GetComponentInChildren<AndroidRuntimeGuard>(true);
                if (androidGuard == null)
                {
                    GameObject host = new GameObject("AndroidRuntimeGuard");
                    host.transform.SetParent(transform, false);
                    androidGuard = host.AddComponent<AndroidRuntimeGuard>();
                }
            }

            if (touchGuard == null)
            {
                touchGuard = GetComponentInChildren<TouchInputGuard>(true);
                if (touchGuard == null)
                {
                    GameObject host = new GameObject("TouchInputGuard");
                    host.transform.SetParent(transform, false);
                    touchGuard = host.AddComponent<TouchInputGuard>();
                }
            }
        }

        private void ApplyPlatformGuards()
        {
            if (androidGuard != null)
            {
                androidGuard.Apply();
            }
        }

        private void ValidateInput()
        {
            if (touchGuard != null)
            {
                touchGuard.ValidateEventSystem();
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
