using UnityEngine;
using UnityEngine.SceneManagement;
using WhiskerTales.Assets;
using WhiskerTales.Feel;
using WhiskerTales.Pooling;
using WhiskerTales.Puzzle;
using WhiskerTales.Save;

namespace WhiskerTales.Core
{
    public sealed class SystemsBootstrap : MonoBehaviour
    {
        // V2 entry scenes opt out of auto-init. MainAppBootstrap calls EnsureInitialized() manually
        // so service lifetime is owned by GameRuntime (DontDestroyOnLoad in Boot_Persistent).
        private static readonly string[] V2EntryScenes = new string[]
        {
            "Boot_Persistent",
            "Main_App"
        };

        private static SystemsBootstrap instance;
        private SaveService saveService;
        private IAssetProvider assetProvider;

        public static SaveService SaveService
        {
            get { return instance != null ? instance.saveService : null; }
        }

        public static IAssetProvider AssetProvider
        {
            get { return instance != null ? instance.assetProvider : null; }
        }

        public static bool IsInitialized
        {
            get { return instance != null; }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeBeforeSceneLoad()
        {
            if (instance != null)
            {
                return;
            }

            if (IsV2BootEntryScene() == true)
            {
                DebugLogger.Info(LogCategory.UI, "[SystemsBootstrap] Auto-init skipped: V2 entry scene at build index 0. MainAppBootstrap will call EnsureInitialized().");
                return;
            }

            EnsureInitialized();
        }

        // V2 (MainAppBootstrap) calls this from its Awake to opt in explicitly.
        // V1 paths reach the same code via InitializeBeforeSceneLoad.
        public static void EnsureInitialized()
        {
            if (instance != null)
            {
                return;
            }

            GameObject root = new GameObject("WhiskerTales.SystemsBootstrap");
            DontDestroyOnLoad(root);
            instance = root.AddComponent<SystemsBootstrap>();
            instance.InitializeSystems(root);
        }

        private static bool IsV2BootEntryScene()
        {
            int sceneCount = SceneManager.sceneCountInBuildSettings;

            if (sceneCount <= 0)
            {
                return false;
            }

            string firstScenePath = SceneUtility.GetScenePathByBuildIndex(0);

            if (string.IsNullOrEmpty(firstScenePath) == true)
            {
                return false;
            }

            for (int i = 0; i < V2EntryScenes.Length; i++)
            {
                string marker = "/" + V2EntryScenes[i] + ".unity";

                if (firstScenePath.EndsWith(marker) == true)
                {
                    return true;
                }
            }

            return false;
        }

        private void InitializeSystems(GameObject root)
        {
            saveService = new SaveService();
            assetProvider = new AddressableAssetProvider();

            EnsureComponent<AudioService>(root, "AudioService");
            EnsureComponent<HapticManager>(root, "HapticManager");
            EnsureComponent<ParticlePoolManager>(root, "ParticlePoolManager");
            EnsureComponent<TilePool>(root, "TilePool");

            GameObject hintObject = new GameObject("HintSystem");
            hintObject.transform.SetParent(root.transform, false);
            hintObject.AddComponent<HintSystem>();

            DebugLogger.Info(LogCategory.UI, "SystemsBootstrap initialized without touching AppBootstrap.");
        }

        private static T EnsureComponent<T>(GameObject root, string name) where T : Component
        {
            T existing = FindObjectOfType<T>();

            if (existing != null)
            {
                return existing;
            }

            GameObject child = new GameObject(name);
            child.transform.SetParent(root.transform, false);
            return child.AddComponent<T>();
        }
    }
}
