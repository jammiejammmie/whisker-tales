using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WhiskerTales.Core;

namespace WhiskerTales.UI
{
    public sealed class LoadingScreenController : UIScreenBase
    {
        [Header("Background")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Logo")]
        [SerializeField] private RectTransform logoRoot;

        [Header("Spinner")]
        [SerializeField] private RectTransform spinnerRoot;

        [Header("Progress")]
        [SerializeField] private Image progressFill;
        [SerializeField] private TMP_Text progressText;

        private Tween spinnerTween;
        private Tween backgroundTween;

        protected override void Awake()
        {
            base.Awake();

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
        }

        public override void Show()
        {
            base.Show();

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.DOFade(1f, 0.35f);
            }

            if (spinnerRoot != null)
            {
                spinnerTween?.Kill();
                spinnerTween = spinnerRoot
                    .DORotate(new Vector3(0f, 0f, -360f), 2.8f, RotateMode.FastBeyond360)
                    .SetEase(Ease.Linear)
                    .SetLoops(-1, LoopType.Restart);
            }

            if (backgroundImage != null)
            {
                backgroundTween?.Kill();
                backgroundTween = backgroundImage.rectTransform
                    .DOScale(1.03f, 12f)
                    .From(1f)
                    .SetEase(Ease.Linear)
                    .SetLoops(-1, LoopType.Yoyo);
            }

            if (logoRoot != null)
            {
                logoRoot.localScale = Vector3.one * 0.96f;
                logoRoot.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
            }
        }

        public void SetProgress(float normalized)
        {
            normalized = Mathf.Clamp01(normalized);

            if (progressFill != null)
            {
                progressFill.fillAmount = normalized;
            }

            if (progressText != null)
            {
                progressText.text = $"{Mathf.RoundToInt(normalized * 100f)}%";
            }
        }

        public override void Hide()
        {
            spinnerTween?.Kill();
            backgroundTween?.Kill();

            base.Hide();
        }
    }
}
