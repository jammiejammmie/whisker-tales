using UnityEngine;
using WhiskerTales.Assets;
using WhiskerTales.Feel;
using WhiskerTales.Pooling;
using WhiskerTales.Puzzle;
using WhiskerTales.Save;

namespace WhiskerTales.Core
{
    public sealed class SystemsBootstrap : MonoBehaviour
    {
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

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeBeforeSceneLoad()
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
