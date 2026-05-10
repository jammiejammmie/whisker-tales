using UnityEngine;

using WhiskerTales.Core;
namespace WhiskerTales.UI
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaHandler : MonoBehaviour
    {
        [SerializeField] private bool applyTop = true;
        [SerializeField] private bool applyBottom = true;
        [SerializeField] private bool applyLeft = true;
        [SerializeField] private bool applyRight = true;

        private RectTransform rectTransform;
        private Rect lastSafeArea;
        private Vector2Int lastScreenSize;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            ApplySafeArea();
        }

        private void Update()
        {
            if (lastSafeArea != Screen.safeArea || lastScreenSize.x != Screen.width || lastScreenSize.y != Screen.height)
            {
                ApplySafeArea();
            }
        }

        public void ApplySafeArea()
        {
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }

            Rect safe = Screen.safeArea;
            lastSafeArea = safe;
            lastScreenSize = new Vector2Int(Screen.width, Screen.height);

            Vector2 anchorMin = safe.position;
            Vector2 anchorMax = safe.position + safe.size;

            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            if (applyLeft == false)
            {
                anchorMin.x = 0f;
            }

            if (applyBottom == false)
            {
                anchorMin.y = 0f;
            }

            if (applyRight == false)
            {
                anchorMax.x = 1f;
            }

            if (applyTop == false)
            {
                anchorMax.y = 1f;
            }

            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            DebugLogger.Info(LogCategory.UI, $"SafeArea applied: min={anchorMin}, max={anchorMax}");
        }
    }
}
