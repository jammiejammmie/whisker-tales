using UnityEngine;
using WhiskerTales.Core;
using WhiskerTales.Detox;

namespace WhiskerTales.UI
{
    public sealed class PhaseABInstaller : MonoBehaviour
    {
        [SerializeField] private bool dontDestroyOnLoad = true;
        [SerializeField] private DetoxMomentService detoxMomentService;

        private static bool installed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInstall()
        {
            if (installed == true)
            {
                return;
            }

            GameObject go = new GameObject("WhiskerTales_PhaseABInstaller");
            PhaseABInstaller installer = go.AddComponent<PhaseABInstaller>();
            installer.Install();
        }

        private void Awake()
        {
            Install();
        }

        public void Install()
        {
            if (installed == true)
            {
                return;
            }

            installed = true;

            if (dontDestroyOnLoad == true)
            {
                DontDestroyOnLoad(gameObject);
            }

            EnsureDetoxService();
            DebugLogger.Info(LogCategory.UI, "Phase A+B UI foundation installed.");
        }

        private void EnsureDetoxService()
        {
            if (detoxMomentService != null)
            {
                return;
            }

            detoxMomentService = GetComponent<DetoxMomentService>();

            if (detoxMomentService == null)
            {
                detoxMomentService = gameObject.AddComponent<DetoxMomentService>();
            }
        }
    }
}
