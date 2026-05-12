using System.Collections;
using UnityEngine;

namespace WhiskerTales.Navigation
{
    /// <summary>
    /// V2 화면 베이스 — CanvasGroup 기반 fade in/out.
    /// 기존 WhiskerTales.UI.UIScreenBase와 별개 (V2 격리).
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class BaseScreen : MonoBehaviour
    {
        [SerializeField] private ScreenId screenId = ScreenId.None;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float fadeSeconds = 0.28f;

        public ScreenId Id
        {
            get { return screenId; }
        }

        protected virtual void Awake()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
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

        private IEnumerator FadeRoutine(float target, bool interactable)
        {
            float start = canvasGroup.alpha;
            float elapsed = 0f;

            canvasGroup.blocksRaycasts = interactable;
            canvasGroup.interactable = interactable;

            while (elapsed < fadeSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / fadeSeconds);
                canvasGroup.alpha = Mathf.Lerp(start, target, t);
                yield return null;
            }

            canvasGroup.alpha = target;

            if (target <= 0.01f)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
