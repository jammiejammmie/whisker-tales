using UnityEngine;
using WhiskerTales.Core;

namespace WhiskerTales.Platform
{
    [DisallowMultipleComponent]
    public sealed class AndroidRuntimeGuard : MonoBehaviour
    {
        public static readonly Color CreamBackground = new Color(0.961f, 0.945f, 0.910f);

        [SerializeField] private bool forceSixtyFps = true;
        [SerializeField] private bool disableVSync = true;
        [SerializeField] private bool paintCameraCream = true;

        private bool applied;

        private void Awake()
        {
            Apply();
        }

        public void Apply()
        {
            if (applied)
            {
                return;
            }

            if (disableVSync)
            {
                QualitySettings.vSyncCount = 0;
            }

            if (forceSixtyFps)
            {
                Application.targetFrameRate = 60;
            }

            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            if (paintCameraCream)
            {
                PaintMainCameraCream();
            }

            ApplySystemBarPolicy();

            applied = true;
            DebugLogger.Info(LogCategory.UI, "[AndroidRuntimeGuard] Applied. fps=" + Application.targetFrameRate + " vsync=" + QualitySettings.vSyncCount + " cream=" + paintCameraCream);
        }

        private void PaintMainCameraCream()
        {
            Camera main = Camera.main;
            if (main == null)
            {
                DebugLogger.Warning(LogCategory.UI, "[AndroidRuntimeGuard] Camera.main is null. Cream paint deferred.");
                return;
            }

            main.clearFlags = CameraClearFlags.SolidColor;
            main.backgroundColor = CreamBackground;
        }

        private void ApplySystemBarPolicy()
        {
            // Intentional no-op: transparent status bar is the known root cause of the magenta flash
            // on Galaxy S24 Ultra. Leave bars opaque per Player Settings; explicit warm-dark coloring
            // requires a native plugin and is deferred to Phase V2-7 polishing.
        }
    }
}
