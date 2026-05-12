using UnityEngine;
using UnityEngine.UI;
using WhiskerTales.Core;
using WhiskerTales.Navigation;

namespace WhiskerTales.Bootstrap
{
    /// <summary>
    /// Main_App 씬의 진입점 — Canvas 4중 구조 검증 + ScreenNavigator 시동.
    /// Canvas: Background(0) / Screens(100) / Modals(500) / Toasts(1000).
    /// </summary>
    public sealed class MainAppBootstrap : MonoBehaviour
    {
        [SerializeField] private Canvas canvasBackground;
        [SerializeField] private Canvas canvasScreens;
        [SerializeField] private Canvas canvasModals;
        [SerializeField] private Canvas canvasToasts;
        [SerializeField] private ScreenNavigator screenNavigator;

        private void Awake()
        {
            ConfigureCanvas(canvasBackground, 0, false);
            ConfigureCanvas(canvasScreens, 100, true);
            ConfigureCanvas(canvasModals, 500, true);
            ConfigureCanvas(canvasToasts, 1000, true);

            if (screenNavigator == null)
            {
                screenNavigator = GetComponentInChildren<ScreenNavigator>(true);
            }

            if (screenNavigator == null)
            {
                DebugLogger.Warning(LogCategory.UI, "MainAppBootstrap: ScreenNavigator not assigned and not found in children.");
            }

            DebugLogger.Info(LogCategory.UI, "MainAppBootstrap: canvases configured (BG/Screens/Modals/Toasts).");
        }

        private void ConfigureCanvas(Canvas c, int sortingOrder, bool addRaycaster)
        {
            if (c == null)
            {
                return;
            }

            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = sortingOrder;

            CanvasScaler scaler = c.GetComponent<CanvasScaler>();

            if (scaler == null)
            {
                scaler = c.gameObject.AddComponent<CanvasScaler>();
            }

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            if (addRaycaster == true)
            {
                if (c.GetComponent<GraphicRaycaster>() == null)
                {
                    c.gameObject.AddComponent<GraphicRaycaster>();
                }
            }
        }
    }
}
