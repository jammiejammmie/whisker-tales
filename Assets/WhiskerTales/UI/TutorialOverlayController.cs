using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WhiskerTales.UI
{
    public sealed class TutorialOverlayController : UIScreenBase
    {
        [SerializeField] private CanvasGroup overlayGroup;
        [SerializeField] private RectTransform highlightRing;
        [SerializeField] private RectTransform hintTap;
        [SerializeField] private RectTransform hintSwipe;
        [SerializeField] private RectTransform arrowRight;
        [SerializeField] private RectTransform arrowDown;
        [SerializeField] private RectTransform bubbleRoot;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Button confirmButton;

        private Tween ringTween;
        private Tween tapTween;
        private Tween swipeTween;

        protected override void Awake()
        {
            base.Awake();

            if (confirmButton != null)
            {
                confirmButton.onClick.AddListener(Hide);
            }
        }

        public override void Show()
        {
            base.Show();

            if (overlayGroup != null)
            {
                overlayGroup.alpha = 0f;
                overlayGroup.DOFade(1f, 0.25f);
            }

            if (bubbleRoot != null)
            {
                bubbleRoot.localScale = Vector3.one * 0.96f;
                bubbleRoot.DOScale(1f, 0.22f).SetEase(Ease.OutBack);
            }

            StartLoopAnimations();
        }

        public void SetMessage(string value)
        {
            if (messageText != null)
            {
                messageText.text = value;
            }
        }

        public void SetHighlightPosition(Vector2 anchoredPosition)
        {
            if (highlightRing != null)
            {
                highlightRing.anchoredPosition = anchoredPosition;
            }
        }

        private void StartLoopAnimations()
        {
            if (highlightRing != null)
            {
                ringTween?.Kill();
                highlightRing.localScale = Vector3.one;
                ringTween = highlightRing.DOScale(1.08f, 1f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
            }

            if (hintTap != null)
            {
                tapTween?.Kill();
                Vector2 start = hintTap.anchoredPosition;
                tapTween = hintTap.DOAnchorPosY(start.y - 18f, 0.55f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
            }

            if (hintSwipe != null)
            {
                swipeTween?.Kill();
                Vector2 start = hintSwipe.anchoredPosition;
                swipeTween = hintSwipe.DOAnchorPosX(start.x + 120f, 0.9f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Restart);
            }
        }

        public override void Hide()
        {
            ringTween?.Kill();
            tapTween?.Kill();
            swipeTween?.Kill();
            base.Hide();
        }
    }
}
