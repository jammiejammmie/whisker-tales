using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WhiskerTales.Core;

namespace WhiskerTales.UI
{
    public sealed class SleepModeScreenController : UIScreenBase
    {
        [SerializeField] private Image backgroundImage;
        [SerializeField] private RectTransform sleepyEmotion;

        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text subtitleText;
        [SerializeField] private TMP_Text timerText;

        [SerializeField] private Button enterButton;
        [SerializeField] private Button exitButton;

        private Tween floatTween;
        private Tween backgroundTween;

        protected override void Awake()
        {
            base.Awake();

            if (enterButton != null)
            {
                enterButton.onClick.AddListener(OnEnterPressed);
            }

            if (exitButton != null)
            {
                exitButton.onClick.AddListener(OnExitPressed);
            }
        }

        public override void Show()
        {
            base.Show();

            if (sleepyEmotion != null)
            {
                floatTween?.Kill();

                floatTween = sleepyEmotion
                    .DOAnchorPosY(sleepyEmotion.anchoredPosition.y + 12f, 3.8f)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo);
            }

            if (backgroundImage != null)
            {
                backgroundTween?.Kill();

                backgroundTween = backgroundImage.rectTransform
                    .DOScale(1.02f, 10f)
                    .From(1f)
                    .SetEase(Ease.Linear)
                    .SetLoops(-1, LoopType.Yoyo);
            }
        }

        public void SetTimer(string value)
        {
            if (timerText != null)
            {
                timerText.text = value;
            }
        }

        private void OnEnterPressed()
        {
            GameEvents.RaiseSleepModeEntered();
            Hide();
        }

        private void OnExitPressed()
        {
            Hide();
        }

        public override void Hide()
        {
            floatTween?.Kill();
            backgroundTween?.Kill();

            base.Hide();
        }
    }
}
