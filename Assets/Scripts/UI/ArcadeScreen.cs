using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WhiskerTales.Core;

namespace WhiskerTales.UI
{
    /// <summary>
    /// 고양이 오락실 (Phase B 미니게임 허브). 3장의 ticket-style 카드:
    ///   1. 고양이 숨은그림찾기 (TODO 미니게임)
    ///   2. 고양이 두더지잡기 (TODO 미니게임)
    ///   3. Coming Soon (잠금)
    /// AppBootstrap이 카드 root + 자식 요소를 미리 빌드하고 ArcadeCard[]로 주입.
    /// </summary>
    public class ArcadeScreen : MonoBehaviour
    {
        public enum CardKind { HiddenPicture, WhackAMole, ComingSoon }

        [Serializable]
        public class ArcadeCard
        {
            public CardKind kind;
            public RectTransform root;
            public Button button;
            public TMP_Text titleText;
            public Image lockIcon;       // null when not Coming Soon
            public TMP_Text lockLabel;   // "준비중" — null when not Coming Soon
        }

        [Header("Top Bar")]
        [SerializeField] private Button backButton;

        [Header("Cards (built by AppBootstrap)")]
        [SerializeField] private ArcadeCard[] cards;

        private void OnEnable()
        {
            if (backButton != null) backButton.onClick.AddListener(HandleBack);

            if (cards != null)
            {
                foreach (var c in cards)
                {
                    if (c == null || c.button == null) continue;
                    ArcadeCard captured = c;
                    c.button.onClick.AddListener(() => HandleCardClicked(captured));
                    if (c.kind == CardKind.ComingSoon)
                    {
                        c.button.interactable = false;
                    }
                }
            }
        }

        private void OnDisable()
        {
            if (backButton != null) backButton.onClick.RemoveListener(HandleBack);
            if (cards != null)
            {
                foreach (var c in cards)
                {
                    if (c == null || c.button == null) continue;
                    c.button.onClick.RemoveAllListeners();
                }
            }
        }

        private void HandleCardClicked(ArcadeCard card)
        {
            AudioManager.instance?.PlayButtonClick();
            switch (card.kind)
            {
                case CardKind.HiddenPicture:
                    Debug.Log("[ArcadeScreen] TODO: launch 고양이 숨은그림찾기 mini-game");
                    break;
                case CardKind.WhackAMole:
                    Debug.Log("[ArcadeScreen] TODO: launch 고양이 두더지잡기 mini-game");
                    break;
                case CardKind.ComingSoon:
                    Debug.Log("[ArcadeScreen] Coming Soon — locked");
                    break;
            }
        }

        private void HandleBack()
        {
            AudioManager.instance?.PlayButtonClick();
            GameManager.Instance?.ReturnToMenu();
        }
    }
}
