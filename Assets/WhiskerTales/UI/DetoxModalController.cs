using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WhiskerTales.Core;

namespace WhiskerTales.UI
{
    public sealed class DetoxModalController : UIScreenBase
    {
        [SerializeField] private CanvasGroup overlay;
        [SerializeField] private RectTransform cardRoot;

        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text bodyText;

        [SerializeField] private Button confirmButton;
        [SerializeField] private Button skipButton;

        private Sequence pulseSequence;

        protected override void Awake()
        {
            base.Awake();

            if (confirmButton != null)
            {
                confirmButton.onClick.AddListener(OnConfirmPressed);
            }

            if (skipButton != null)
            {
                skipButton.onClick.AddListener(OnSkipPressed);
            }
        }

        public override void Show()
        {
            base.Show();

            GameEvents.RaiseDetoxModalShown();

            if (overlay != null)
            {
                overlay.alpha = 0f;
                overlay.DOFade(1f, 0.25f);
            }

            if (cardRoot != null)
            {
                cardRoot.localScale = Vector3.one * 0.92f;
                cardRoot.DOScale(1f, 0.28f).SetEase(Ease.OutBack);
            }

            pulseSequence?.Kill();

            if (confirmButton != null)
            {
                pulseSequence = DOTween.Sequence();
                pulseSequence.Append(confirmButton.transform.DOScale(1.04f, 1.4f));
                pulseSequence.Append(confirmButton.transform.DOScale(1f, 1.4f));
                pulseSequence.SetLoops(-1);
            }
        }

        public void SetMessage(string title, string body)
        {
            if (titleText != null)
            {
                titleText.text = title;
            }

            if (bodyText != null)
            {
                bodyText.text = body;
            }
        }

        private void OnConfirmPressed()
        {
            Hide();
        }

        private void OnSkipPressed()
        {
            Hide();
        }

        public override void Hide()
        {
            pulseSequence?.Kill();

            if (overlay != null)
            {
                overlay.DOFade(0f, 0.16f);
            }

            if (cardRoot != null)
            {
                cardRoot.DOScale(0.96f, 0.16f).SetEase(Ease.InQuad);
            }

            base.Hide();
        }
    }
}
