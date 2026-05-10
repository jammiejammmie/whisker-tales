using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WhiskerTales.Core;

namespace WhiskerTales.UI
{
    public sealed class ArcadeScreenController : UIScreenBase
    {
        [Serializable]
        public sealed class ArcadeCardView
        {
            public RectTransform root;
            public Image catImage;
            public Image lockIcon;
            public TMP_Text titleText;
            public TMP_Text stateText;
            public Button button;
        }

        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text bottomText;
        [SerializeField] private List<ArcadeCardView> cards = new List<ArcadeCardView>();

        private Sequence entranceSequence;

        public override void Show()
        {
            base.Show();
            PlayCardEntrance();
        }

        public void ConfigureCard(int index, Sprite catSprite, string title, string state, bool locked, Action onClick)
        {
            if (index < 0 || index >= cards.Count)
            {
                DebugLogger.Warning(LogCategory.UI, $"Arcade card index out of range: {index}");
                return;
            }

            ArcadeCardView card = cards[index];

            if (card == null)
            {
                return;
            }

            if (card.catImage != null)
            {
                card.catImage.sprite = catSprite;
            }

            if (card.titleText != null)
            {
                card.titleText.text = title;
            }

            if (card.stateText != null)
            {
                card.stateText.text = state;
            }

            if (card.lockIcon != null)
            {
                card.lockIcon.gameObject.SetActive(locked);
            }

            if (card.button != null)
            {
                card.button.interactable = locked == false;
                card.button.onClick.RemoveAllListeners();
                card.button.onClick.AddListener(() =>
                {
                    onClick?.Invoke();
                    DebugLogger.Info(LogCategory.UI, $"Arcade card pressed: {title}");
                });
            }

            if (card.root != null)
            {
                CanvasGroup group = card.root.GetComponent<CanvasGroup>();

                if (group == null)
                {
                    group = card.root.gameObject.AddComponent<CanvasGroup>();
                }

                group.alpha = locked == true ? 0.6f : 1f;
            }
        }

        private void PlayCardEntrance()
        {
            entranceSequence?.Kill();
            entranceSequence = DOTween.Sequence();

            for (int i = 0; i < cards.Count; i++)
            {
                ArcadeCardView card = cards[i];

                if (card == null || card.root == null)
                {
                    continue;
                }

                CanvasGroup group = card.root.GetComponent<CanvasGroup>();

                if (group == null)
                {
                    group = card.root.gameObject.AddComponent<CanvasGroup>();
                }

                Vector2 original = card.root.anchoredPosition;
                card.root.anchoredPosition = original + new Vector2(0f, -24f);
                group.alpha = 0f;

                entranceSequence.Insert(i * 0.12f, group.DOFade(1f, 0.24f));
                entranceSequence.Insert(i * 0.12f, card.root.DOAnchorPos(original, 0.24f).SetEase(Ease.OutCubic));
            }
        }

        public override void Hide()
        {
            entranceSequence?.Kill();
            base.Hide();
        }
    }
}
