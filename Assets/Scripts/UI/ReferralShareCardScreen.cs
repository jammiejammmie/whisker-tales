using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WhiskerTales.Core;
using WhiskerTales.Referral;

namespace WhiskerTales.UI
{
    /// <summary>
    /// Phase C-2 레퍼럴 공유 카드 화면.
    /// 현재 고양이 이미지(MyCode의 cat name으로 결정) + 코드 large display + 공유 버튼.
    /// 공유는 NativeShare 미설치라 stub (path/text 콘솔 로그) — 통합 시 한 줄 활성화.
    /// </summary>
    public class ReferralShareCardScreen : MonoBehaviour
    {
        public const string SHARE_URL_BASE = "https://whiskertales-mwjyt48n.manus.space";

        [Header("Top Bar")]
        [SerializeField] private Button backButton;
        [SerializeField] private TMP_Text titleText;

        [Header("Card")]
        [SerializeField] private Image catImage;
        [SerializeField] private TMP_Text codeText;
        [SerializeField] private TMP_Text descriptionText;

        [Header("Action")]
        [SerializeField] private Button shareButton;

        [Header("Cat Sprites (injected by AppBootstrap, indexed by catId 1..5)")]
        [SerializeField] private Sprite[] catSpritesByCatId;

        private void OnEnable()
        {
            if (backButton != null) backButton.onClick.AddListener(HandleBack);
            if (shareButton != null) shareButton.onClick.AddListener(HandleShare);
            ApplyStaticTexts();
            Refresh();
        }

        private void OnDisable()
        {
            if (backButton != null) backButton.onClick.RemoveListener(HandleBack);
            if (shareButton != null) shareButton.onClick.RemoveListener(HandleShare);
        }

        private void ApplyStaticTexts()
        {
            if (titleText != null) titleText.text = "친구 초대";
            if (descriptionText != null) descriptionText.text = "이 코드로 친구를 초대하면\n친구가 하트 +3을 받아요 🐾";
        }

        public void Refresh()
        {
            string code = ReferralManager.Instance?.MyCode ?? "";
            if (codeText != null) codeText.text = code;

            int catId = ReferralManager.ParseCatIdFromCode(code);
            if (catImage != null && catSpritesByCatId != null
                && catId >= 0 && catId < catSpritesByCatId.Length
                && catSpritesByCatId[catId] != null)
            {
                catImage.sprite = catSpritesByCatId[catId];
            }
        }

        private void HandleShare()
        {
            AudioManager.instance?.PlayButtonClick();
            string code = ReferralManager.Instance?.MyCode ?? "";
            string shareText = BuildShareText(code);

            // NativeShare 플러그인 미설치 — 통합 시 다음 한 줄 활성화:
            //   new NativeShare().SetText(shareText).Share();
            Debug.Log($"[ShareCard] (Stub) Share text:\n{shareText}");
        }

        public static string BuildShareText(string code)
        {
            return $"냥이의 집에서 고양이들이 기다리고 있어요 🐾\n{SHARE_URL_BASE}?ref={code}";
        }

        private void HandleBack()
        {
            AudioManager.instance?.PlayButtonClick();
            GameManager.Instance?.ReturnToMenu();
        }
    }
}
