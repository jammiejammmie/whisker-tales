using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WhiskerTales.Core;

namespace WhiskerTales.UI
{
    public sealed class ReferralShareScreenController : UIScreenBase
    {
        [SerializeField] private Button backButton;
        [SerializeField] private Button shareButton;
        [SerializeField] private Image catPortrait;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text codeText;
        [SerializeField] private TMP_Text descriptionText;

        private Tween catTween;

        protected override void Awake()
        {
            base.Awake();

            if (backButton != null)
            {
                backButton.onClick.AddListener(Hide);
            }

            if (shareButton != null)
            {
                shareButton.onClick.AddListener(OnSharePressed);
            }
        }

        public override void Show()
        {
            base.Show();
            StartCatIdle();
        }

        public void SetReferral(Sprite selectedCat, string code)
        {
            if (catPortrait != null)
            {
                catPortrait.sprite = selectedCat;
            }

            if (codeText != null)
            {
                codeText.text = code;
            }

            if (descriptionText != null)
            {
                descriptionText.text = "친구에게 초대 코드를 보내고 함께 냥이의 집을 복원해요.";
            }
        }

        private void OnSharePressed()
        {
            string code = codeText != null ? codeText.text : string.Empty;
            DebugLogger.Info(LogCategory.UI, $"Referral share pressed: {code}");

            if (shareButton != null)
            {
                shareButton.transform.DOKill();
                shareButton.transform.DOPunchScale(Vector3.one * 0.06f, 0.2f, 5, 0.8f);
            }
        }

        private void StartCatIdle()
        {
            if (catPortrait == null)
            {
                return;
            }

            catTween?.Kill();
            catTween = catPortrait.rectTransform.DOScale(1.02f, 4f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
        }

        public override void Hide()
        {
            catTween?.Kill();
            base.Hide();
        }
    }
}
