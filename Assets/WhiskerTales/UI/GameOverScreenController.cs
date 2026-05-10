using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WhiskerTales.UI
{
    public sealed class GameOverScreenController : UIScreenBase
    {
        [SerializeField] private CanvasGroup rootCanvas;
        [SerializeField] private RectTransform cardRoot;

        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text bodyText;

        [SerializeField] private Button retryButton;
        [SerializeField] private Button homeButton;

        protected override void Awake()
        {
            base.Awake();

            if (retryButton != null)
            {
                retryButton.onClick.AddListener(OnRetryPressed);
            }

            if (homeButton != null)
            {
                homeButton.onClick.AddListener(OnHomePressed);
            }
        }

        public override void Show()
        {
            base.Show();

            if (rootCanvas != null)
            {
                rootCanvas.alpha = 0f;
                rootCanvas.DOFade(1f, 0.24f);
            }

            if (cardRoot != null)
            {
                cardRoot.localScale = Vector3.one * 0.94f;
                cardRoot.DOScale(1f, 0.26f).SetEase(Ease.OutBack);
            }
        }

        private void OnRetryPressed()
        {
            Hide();
        }

        private void OnHomePressed()
        {
            Hide();
        }
    }
}
