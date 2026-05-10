using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WhiskerTales.Core;

namespace WhiskerTales.UI
{
    public sealed class CafeRestorationScreenController : UIScreenBase
    {
        [Serializable]
        public sealed class RestoreCardView
        {
            public RectTransform root;
            public TMP_Text titleText;
            public TMP_Text progressText;
            public Image lockIcon;
            public Button restoreButton;
            public RectTransform starRoot;
        }

        [Header("Top")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text starTotalText;
        [SerializeField] private Image starIcon;

        [Header("Cards")]
        [SerializeField] private RectTransform contentRoot;
        [SerializeField] private List<RestoreCardView> cards = new List<RestoreCardView>();

        [Header("Zone Progress")]
        [SerializeField] private TMP_Text zone1Text;
        [SerializeField] private TMP_Text zone2Text;
        [SerializeField] private TMP_Text zone3Text;

        [Header("Navigation")]
        [SerializeField] private BottomNavController bottomNav;

        private Sequence entranceSequence;

        public override void Show()
        {
            base.Show();
            PlayEntrance();
        }

        public void SetTotalStars(int stars)
        {
            if (starTotalText != null)
            {
                starTotalText.text = stars.ToString();
            }

            if (starIcon != null)
            {
                starIcon.transform.DOKill();
                starIcon.transform.localScale = Vector3.one;
                starIcon.transform.DOPunchScale(Vector3.one * 0.18f, 0.28f, 6, 0.8f);
            }
        }

        public void SetZoneProgress(int zone1, int zone2, int zone3)
        {
            if (zone1Text != null)
            {
                zone1Text.text = $"1구역 {zone1}/5";
            }

            if (zone2Text != null)
            {
                zone2Text.text = $"2구역 {zone2}/5";
            }

            if (zone3Text != null)
            {
                zone3Text.text = $"3구역 {zone3}/5";
            }
        }

        public void ConfigureCard(int index, string title, string progress, bool locked, Action onRestore)
        {
            if (index < 0 || index >= cards.Count)
            {
                DebugLogger.Warning(LogCategory.UI, $"Restore card index out of range: {index}");
                return;
            }

            RestoreCardView card = cards[index];

            if (card == null)
            {
                DebugLogger.Warning(LogCategory.UI, $"Restore card is null: {index}");
                return;
            }

            if (card.titleText != null)
            {
                card.titleText.text = title;
            }

            if (card.progressText != null)
            {
                card.progressText.text = progress;
            }

            if (card.lockIcon != null)
            {
                card.lockIcon.gameObject.SetActive(locked);
            }

            if (card.restoreButton != null)
            {
                card.restoreButton.interactable = locked == false;
                card.restoreButton.onClick.RemoveAllListeners();
                card.restoreButton.onClick.AddListener(() =>
                {
                    onRestore?.Invoke();
                    PlayRestoreCardFeedback(card);
                });
            }
        }

        private void PlayEntrance()
        {
            entranceSequence?.Kill();
            entranceSequence = DOTween.Sequence();

            for (int i = 0; i < cards.Count; i++)
            {
                RestoreCardView card = cards[i];

                if (card == null || card.root == null)
                {
                    continue;
                }

                CanvasGroup group = card.root.GetComponent<CanvasGroup>();

                if (group == null)
                {
                    group = card.root.gameObject.AddComponent<CanvasGroup>();
                }

                group.alpha = 0f;
                card.root.anchoredPosition += new Vector2(0f, -22f);

                entranceSequence.Insert(i * 0.07f, group.DOFade(1f, 0.22f));
                entranceSequence.Insert(i * 0.07f, card.root.DOAnchorPosY(card.root.anchoredPosition.y + 22f, 0.22f).SetEase(Ease.OutCubic));
            }
        }

        private void PlayRestoreCardFeedback(RestoreCardView card)
        {
            if (card == null || card.root == null)
            {
                return;
            }

            card.root.DOKill();
            card.root.DOPunchScale(Vector3.one * 0.045f, 0.22f, 5, 0.75f);
        }

        public override void Hide()
        {
            entranceSequence?.Kill();
            base.Hide();
        }
    }
}
