using UnityEngine;
using UnityEngine.SceneManagement;
using WhiskerTales.Core;

namespace WhiskerTales.Runtime
{
    /// <summary>
    /// V2 런타임 코어. Boot_Persistent 씬에 거주, DontDestroyOnLoad.
    /// Main_App 씬을 Additive로 자동 로드 + ServiceRegistry 보유.
    /// </summary>
    public sealed class GameRuntime : MonoBehaviour
    {
        public static GameRuntime Instance { get; private set; }
        public ServiceRegistry Services { get; private set; }
        public bool MainAppLoaded { get; private set; }

        [SerializeField] private string mainAppSceneName = "Main_App";
        [SerializeField] private int targetFrameRate = 60;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                DebugLogger.Warning(LogCategory.UI, "GameRuntime duplicate destroyed.");
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            Application.targetFrameRate = targetFrameRate;
            Services = new ServiceRegistry();

            DebugLogger.Info(LogCategory.UI, "GameRuntime initialized (target FPS=" + targetFrameRate + ").");
        }

        private void Start()
        {
            if (MainAppLoaded == false)
            {
                LoadMainApp();
            }
        }

        public void LoadMainApp()
        {
            if (MainAppLoaded == true)
            {
                return;
            }

            Scene existing = SceneManager.GetSceneByName(mainAppSceneName);
            if (existing.IsValid() == true && existing.isLoaded == true)
            {
                MainAppLoaded = true;
                DebugLogger.Info(LogCategory.UI, "Main_App already loaded.");
                return;
            }

            DebugLogger.Info(LogCategory.UI, "GameRuntime: loading " + mainAppSceneName + " (additive)...");
            AsyncOperation op = SceneManager.LoadSceneAsync(mainAppSceneName, LoadSceneMode.Additive);

            if (op == null)
            {
                DebugLogger.Warning(LogCategory.UI, mainAppSceneName + " not found in Build Settings — add it as scene #1.");
                return;
            }

            op.completed += _ =>
            {
                MainAppLoaded = true;
                DebugLogger.Info(LogCategory.UI, "Main_App loaded.");
            };
        }
    }
}
