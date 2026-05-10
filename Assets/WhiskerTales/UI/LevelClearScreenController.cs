using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WhiskerTales.Core;

namespace WhiskerTales.UI
{
    public sealed class LevelClearScreenController : UIScreenBase
    {
        [SerializeField] private CanvasGroup rootCanvas;

        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text scoreText;

        [SerializeField] private RectTransform star1;
        [SerializeField] private RectTransform star2;
        [SerializeField] private RectTransform star3;

        [SerializeField] private Button continueButton;

        private Sequence starSequence;

        protected override void Awake()
        {
            base.Awake();

            if (continueButton != null)
            {
                continueButton.onClick.AddListener(OnContinuePressed);
            }
        }

        public override void Show()
        {
            base.Show();

            if (rootCanvas != null)
            {
                rootCanvas.alpha = 0f;
                rootCanvas.DOFade(1f, 0.32f);
            }

            PlayStarSequence();
        }

        public void SetResult(int level, int score)
        {
            if (levelText != null)
            {
                levelText.text = $"LEVEL {level}";
            }

            if (scoreText != null)
            {
                scoreText.text = score.ToString("N0");
            }
        }

        private void PlayStarSequence()
        {
            starSequence?.Kill();

            RectTransform[] stars = { star1, star2, star3 };

            starSequence = DOTween.Sequence();

            foreach (RectTransform star in stars)
            {
                if (star == null)
                {
                    continue;
                }

                star.localScale = Vector3.zero;

                starSequence.Append(star.DOScale(1f, 0.18f).SetEase(Ease.OutBack));
                starSequence.AppendInterval(0.06f);
            }
        }

        private void OnContinuePressed()
        {
            Hide();
        }

        public override void Hide()
        {
            starSequence?.Kill();
            base.Hide();
        }
    }
}
