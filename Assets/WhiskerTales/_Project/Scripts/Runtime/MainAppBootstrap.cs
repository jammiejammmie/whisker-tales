using UnityEngine;
using WhiskerTales.Audio;
using WhiskerTales.Core;
using WhiskerTales.UI;

namespace WhiskerTales.Runtime
{
    // Sits in Main_App scene. Awakens AFTER GameRuntime (which uses -1000) but before normal scripts
    // so that ScreenNavigator and screen controllers see services already wired.
    [DefaultExecutionOrder(-900)]
    [DisallowMultipleComponent]
    public sealed class MainAppBootstrap : MonoBehaviour
    {
        [SerializeField] private ScreenNavigator screenNavigator;
        [SerializeField] private Canvas backgroundCanvas;
        [SerializeField] private Canvas screensCanvas;
        [SerializeField] private Canvas modalsCanvas;
        [SerializeField] private Canvas toastsCanvas;

        public static MainAppBootstrap Instance { get; private set; }

        public bool ServicesReady { get; private set; }
        public ScreenNavigator Navigator
        {
            get { return screenNavigator; }
        }
        public Canvas BackgroundCanvas
        {
            get { return backgroundCanvas; }
        }
        public Canvas ScreensCanvas
        {
            get { return screensCanvas; }
        }
        public Canvas ModalsCanvas
        {
            get { return modalsCanvas; }
        }
        public Canvas ToastsCanvas
        {
            get { return toastsCanvas; }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                DebugLogger.Warning(LogCategory.UI, "[MainAppBootstrap] Duplicate detected. Destroying extra.", this);
                Destroy(gameObject);
                return;
            }

            Instance = this;

            TMPFontFallbackInstaller.EnsureInstalled();

            SystemsBootstrap.EnsureInitialized();
            V2AudioBindings.EnsureInstalled();
            V2SettingsApplier.ApplyFromSave();
            ServicesReady = SystemsBootstrap.IsInitialized
                && SystemsBootstrap.SaveService != null
                && SystemsBootstrap.AssetProvider != null;

            string status = ServicesReady == true ? "ok" : "FAIL";
            DebugLogger.Info(LogCategory.UI, "[MainAppBootstrap] Awake. services=" + status
                + " save=" + (SystemsBootstrap.SaveService != null)
                + " assets=" + (SystemsBootstrap.AssetProvider != null));
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
