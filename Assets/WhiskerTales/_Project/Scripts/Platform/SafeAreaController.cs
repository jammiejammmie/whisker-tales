using UnityEngine;
using WhiskerTales.Core;

namespace WhiskerTales.Platform
{
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    public sealed class SafeAreaController : MonoBehaviour
    {
        [SerializeField] private bool applyTop = true;
        [SerializeField] private bool applyBottom = true;
        [SerializeField] private bool applyLeft = true;
        [SerializeField] private bool applyRight = true;

        // V2-14: extra top inset in screen pixels. Galaxy S24 Ultra's Screen.safeArea accounts
        // for the status bar but the camera punch-hole still encroaches on top-left/right corners.
        // Adding ~80px of padding pushes top buttons (back / gear) clear of the hole + rounded
        // corner glass.
        [SerializeField] private float extraTopMarginPixels = 80f;

        private RectTransform target;
        private Rect lastSafeArea;
        private Vector2Int lastScreenSize;

        private void Awake()
        {
            target = GetComponent<RectTransform>();
            Apply();
        }

        private void Update()
        {
            bool changed = Screen.safeArea != lastSafeArea
                || Screen.width != lastScreenSize.x
                || Screen.height != lastScreenSize.y;
            if (changed)
            {
                Apply();
            }
        }

        public void Apply()
        {
            if (target == null)
            {
                target = GetComponent<RectTransform>();
            }
            if (target == null)
            {
                return;
            }

            Rect safe = Screen.safeArea;
            int w = Mathf.Max(1, Screen.width);
            int h = Mathf.Max(1, Screen.height);

            Vector2 anchorMin = new Vector2(safe.xMin / w, safe.yMin / h);
            Vector2 anchorMax = new Vector2(safe.xMax / w, safe.yMax / h);

            if (!applyLeft)
            {
                anchorMin.x = 0f;
            }
            if (!applyBottom)
            {
                anchorMin.y = 0f;
            }
            if (!applyRight)
            {
                anchorMax.x = 1f;
            }
            if (!applyTop)
            {
                anchorMax.y = 1f;
            }

            if (applyTop == true && extraTopMarginPixels > 0f)
            {
                float extraNormalized = extraTopMarginPixels / h;
                anchorMax.y = Mathf.Max(anchorMin.y, anchorMax.y - extraNormalized);
            }

            target.anchorMin = anchorMin;
            target.anchorMax = anchorMax;
            target.offsetMin = Vector2.zero;
            target.offsetMax = Vector2.zero;

            lastSafeArea = safe;
            lastScreenSize = new Vector2Int(w, h);

            DebugLogger.Info(LogCategory.UI, "[SafeAreaController] min=" + anchorMin + " max=" + anchorMax + " screen=" + w + "x" + h);
        }
    }
}
