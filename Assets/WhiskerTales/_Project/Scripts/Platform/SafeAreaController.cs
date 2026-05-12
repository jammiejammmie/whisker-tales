using UnityEngine;

namespace WhiskerTales.Platform
{
    /// <summary>
    /// V2 SafeArea 컨트롤러 — RectTransform을 Screen.safeArea에 맞춰 자동 조정.
    /// 기존 WhiskerTales.UI.SafeAreaHandler와 별개 (V2 격리).
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaController : MonoBehaviour
    {
        [SerializeField] private bool applyTop = true;
        [SerializeField] private bool applyBottom = true;
        [SerializeField] private bool applyLeft = true;
        [SerializeField] private bool applyRight = true;

        private RectTransform rect;
        private Rect lastSafe;
        private ScreenOrientation lastOrientation;

        private void Awake()
        {
            rect = GetComponent<RectTransform>();
            ApplySafeArea();
        }

        private void Update()
        {
            if (Screen.safeArea != lastSafe || Screen.orientation != lastOrientation)
            {
                ApplySafeArea();
            }
        }

        public void ApplySafeArea()
        {
            if (rect == null)
            {
                rect = GetComponent<RectTransform>();
            }

            Rect safe = Screen.safeArea;
            lastSafe = safe;
            lastOrientation = Screen.orientation;

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

            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
