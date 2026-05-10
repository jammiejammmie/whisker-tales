using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WhiskerTales.Core;

namespace WhiskerTales.UI
{
    public sealed class CatBondingScreenController : UIScreenBase
    {
        [Header("Cat")]
        [SerializeField] private Image catPortrait;
        [SerializeField] private TMP_Text catNameText;
        [SerializeField] private TMP_Text levelText;

        [Header("Affinity")]
        [SerializeField] private Image affinityFill;
        [SerializeField] private TMP_Text affinityText;
        [SerializeField] private TMP_Text rewardAffinityText;
        [SerializeField] private TMP_Text rewardCoinText;

        [Header("Buttons")]
        [SerializeField] private Button cameraButton;
        [SerializeField] private Button helpButton;
        [SerializeField] private Button petButton;
        [SerializeField] private Button treatButton;
        [SerializeField] private Button playButton;

        [Header("Tip")]
        [SerializeField] private TMP_Text tipText;

        private Tween catIdleTween;

        protected override void Awake()
        {
            base.Awake();

            if (petButton != null)
            {
                petButton.onClick.AddListener(() => OnInteractionPressed("pet", 5, 0));
            }

            if (treatButton != null)
            {
                treatButton.onClick.AddListener(() => OnInteractionPressed("treat", 8, 3));
            }

            if (playButton != null)
            {
                playButton.onClick.AddListener(() => OnInteractionPressed("play", 10, 5));
            }
        }

        public override void Show()
        {
            base.Show();
            StartCatIdle();
        }

        public void SetCat(Sprite portrait, string catName, int level, float affinityNormalized, string tip)
        {
            if (catPortrait != null)
            {
                catPortrait.sprite = portrait;
                catPortrait.SetNativeSize();
                catPortrait.rectTransform.sizeDelta = new Vector2(720f, 780f);
            }

            if (catNameText != null)
            {
                catNameText.text = catName;
            }

            if (levelText != null)
            {
                levelText.text = $"Lv.{level}";
            }

            SetAffinity(affinityNormalized, false);

            if (tipText != null)
            {
                tipText.text = tip;
            }
        }

        public void SetAffinity(float normalized, bool animate)
        {
            normalized = Mathf.Clamp01(normalized);

            if (affinityFill != null)
            {
                if (animate == true)
                {
                    affinityFill.DOFillAmount(normalized, 0.45f).SetEase(Ease.OutCubic);
                }
                else
                {
                    affinityFill.fillAmount = normalized;
                }
            }

            if (affinityText != null)
            {
                affinityText.text = $"{Mathf.RoundToInt(normalized * 100f)}%";
            }
        }

        private void OnInteractionPressed(string interactionType, int affinityAmount, int coins)
        {
            GameEvents.RaiseCatAffinityChanged(0, affinityAmount);

            PlayFloatingReward(rewardAffinityText, $"+{affinityAmount} Affinity");

            if (coins > 0)
            {
                GameEvents.RaiseCoinEarned(coins);
                PlayFloatingReward(rewardCoinText, $"+{coins} Coins");
            }

            if (catPortrait != null)
            {
                catPortrait.rectTransform.DOKill();
                catPortrait.rectTransform.DOPunchScale(Vector3.one * 0.05f, 0.32f, 6, 0.7f);
            }

            DebugLogger.Info(LogCategory.UI, $"Cat interaction pressed: {interactionType}");
        }

        private void PlayFloatingReward(TMP_Text target, string value)
        {
            if (target == null)
            {
                return;
            }

            target.text = value;
            target.alpha = 1f;
            RectTransform rt = target.rectTransform;
            Vector2 original = rt.anchoredPosition;

            rt.DOKill();
            rt.anchoredPosition = original;
            Sequence seq = DOTween.Sequence();
            seq.Append(rt.DOAnchorPosY(original.y + 70f, 0.9f).SetEase(Ease.OutCubic));
            seq.Join(target.DOFade(0f, 0.9f));
            seq.OnComplete(() =>
            {
                rt.anchoredPosition = original;
            });
        }

        private void StartCatIdle()
        {
            if (catPortrait == null)
            {
                return;
            }

            catIdleTween?.Kill();
            catPortrait.rectTransform.localScale = Vector3.one;
            catIdleTween = catPortrait.rectTransform
                .DOScale(1.02f, 4f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        public override void Hide()
        {
            catIdleTween?.Kill();
            base.Hide();
        }
    }
}
