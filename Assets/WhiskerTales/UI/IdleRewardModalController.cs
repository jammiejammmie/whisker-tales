using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WhiskerTales.Core;

namespace WhiskerTales.UI
{
    public sealed class IdleRewardModalController : UIScreenBase
    {
        [SerializeField] private CanvasGroup overlay;
        [SerializeField] private RectTransform cardRoot;
        [SerializeField] private RectTransform happyEmotion;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text offlineTimeText;
        [SerializeField] private TMP_Text coinRewardText;
        [SerializeField] private TMP_Text heartRewardText;
        [SerializeField] private Button confirmButton;

        private int targetCoins;
        private int targetHearts;

        protected override void Awake()
        {
            base.Awake();

            if (confirmButton != null)
            {
                confirmButton.onClick.AddListener(OnConfirmPressed);
            }
        }

        public override void Show()
        {
            base.Show();

            if (overlay != null)
            {
                overlay.alpha = 0f;
                overlay.DOFade(1f, 0.22f);
            }

            if (cardRoot != null)
            {
                cardRoot.localScale = Vector3.one * 0.92f;
                cardRoot.DOScale(1f, 0.26f).SetEase(Ease.OutBack);
            }

            if (happyEmotion != null)
            {
                happyEmotion.DOPunchScale(Vector3.one * 0.16f, 0.38f, 8, 0.75f);
            }

            CountRewards();
        }

        public void SetReward(float offlineHours, int coins, int hearts)
        {
            targetCoins = Mathf.Max(0, coins);
            targetHearts = Mathf.Max(0, hearts);

            if (offlineTimeText != null)
            {
                offlineTimeText.text = $"그동안 {offlineHours:0.#}시간 자리를 비웠어요";
            }
        }

        private void CountRewards()
        {
            if (coinRewardText != null)
            {
                DOVirtual.Int(0, targetCoins, 0.8f, value =>
                {
                    coinRewardText.text = $"멸치 +{value}";
                });
            }

            if (heartRewardText != null)
            {
                DOVirtual.Int(0, targetHearts, 0.8f, value =>
                {
                    heartRewardText.text = $"하트 +{value}";
                });
            }
        }

        private void OnConfirmPressed()
        {
            if (targetCoins > 0)
            {
                GameEvents.RaiseCoinEarned(targetCoins);
            }

            Hide();
        }
    }
}
