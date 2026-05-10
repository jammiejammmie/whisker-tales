using System.Collections;
using UnityEngine;
using WhiskerTales.Core;

namespace WhiskerTales.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIScreenBase : MonoBehaviour
    {
        [SerializeField] private string screenId;
        [SerializeField] private CanvasGroup canvasGroup;

        public string ScreenId
        {
            get { return screenId; }
        }

        protected virtual void Awake()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            if (canvasGroup == null)
            {
                DebugLogger.Warning(LogCategory.UI, name + " missing CanvasGroup.");
            }
        }

        public virtual void Show(bool instant)
        {
            gameObject.SetActive(true);

            if (canvasGroup == null)
            {
                return;
            }

            if (instant == true)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = true;
                return;
            }

            StopAllCoroutines();
            StartCoroutine(FadeRoutine(1f, true));
        }

        public virtual void Hide(bool instant)
        {
            if (canvasGroup == null)
            {
                gameObject.SetActive(false);
                return;
            }

            if (instant == true)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
                gameObject.SetActive(false);
                return;
            }

            StopAllCoroutines();
            StartCoroutine(FadeRoutine(0f, false));
        }

        private IEnumerator FadeRoutine(float target, bool interactableWhenDone)
        {
            float start = canvasGroup.alpha;
            float elapsed = 0f;
            float duration = UILayoutConstants.ScreenFadeSeconds;

            canvasGroup.blocksRaycasts = interactableWhenDone;
            canvasGroup.interactable = interactableWhenDone;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                canvasGroup.alpha = Mathf.Lerp(start, target, t);
                yield return null;
            }

            canvasGroup.alpha = target;
            canvasGroup.blocksRaycasts = interactableWhenDone;
            canvasGroup.interactable = interactableWhenDone;

            if (target <= 0.01f)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
