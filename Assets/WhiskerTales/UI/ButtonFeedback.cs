using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WhiskerTales.Core;
#if WHISKER_DOTWEEN
using DG.Tweening;
#endif

namespace WhiskerTales.UI
{
    public sealed class ButtonFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField] private Button button;
        [SerializeField] private float pressedScale = 0.94f;
        [SerializeField] private float duration = 0.08f;

        private RectTransform rectTransform;
        private Coroutine scaleRoutine;
        private Vector3 originalScale = Vector3.one;

        public void Bind(Button targetButton)
        {
            button = targetButton;
        }

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();

            if (rectTransform != null)
            {
                originalScale = rectTransform.localScale;
            }
            else
            {
                DebugLogger.Warning(LogCategory.UI, "ButtonFeedback missing RectTransform.");
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (CanAnimate() == false)
            {
                return;
            }

            AnimateScale(originalScale * pressedScale);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (CanAnimate() == false)
            {
                return;
            }

            AnimateScale(originalScale);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (CanAnimate() == false)
            {
                return;
            }

            AnimateScale(originalScale);
        }

        private bool CanAnimate()
        {
            if (rectTransform == null)
            {
                return false;
            }

            if (button != null && button.interactable == false)
            {
                return false;
            }

            return true;
        }

        private void AnimateScale(Vector3 targetScale)
        {
#if WHISKER_DOTWEEN
            DG.Tweening.ShortcutExtensions.DOScale(rectTransform, targetScale, duration)
                .SetEase(DG.Tweening.Ease.OutQuad)
                .SetUpdate(true);
#else
            if (scaleRoutine != null)
            {
                StopCoroutine(scaleRoutine);
            }

            scaleRoutine = StartCoroutine(ScaleRoutine(targetScale));
#endif
        }

#if !WHISKER_DOTWEEN
        private IEnumerator ScaleRoutine(Vector3 targetScale)
        {
            Vector3 start = rectTransform.localScale;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                rectTransform.localScale = Vector3.Lerp(start, targetScale, t);
                yield return null;
            }

            rectTransform.localScale = targetScale;
            scaleRoutine = null;
        }
#endif
    }
}
