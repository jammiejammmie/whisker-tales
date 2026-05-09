using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WhiskerTales.Cat;
using WhiskerTales.Core;

namespace WhiskerTales.UI
{
    /// <summary>
    /// 카페 복원 화면 (Stage 4 §4-4).
    /// 15개 stage 카드 + 3개 zone 진행 바 + 총 별 표시 + 현재 zone 배경.
    /// 카드 위젯은 AppBootstrap에서 미리 빌드되어 cards 배열로 주입됨.
    /// 유저가 [복원하기] 버튼을 누르면 CafeRestorationManager.TryRestoreCurrentStage가 별을 차감 + stage 진행.
    /// </summary>
    public class CafeRestorationScreen : MonoBehaviour
    {
        /// <summary>
        /// AppBootstrap에서 생성하는 stage 카드 한 장. CafeRestorationScreen은 이 데이터만 가지고
        /// 상태 갱신을 수행하므로 Inspector 와이어링이 필요 없음.
        /// </summary>
        [System.Serializable]
        public class CardWidget
        {
            public int areaId;          // 1-based
            public int stageIdx;        // 1-based
            public RectTransform root;
            public Image background;
            public TMP_Text descText;
            public TMP_Text starsText;
            public TMP_Text stateLabel;
            public Button restoreButton;
        }

        [Header("Top Bar")]
        [SerializeField] private Button backButton;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text totalStarsText;

        [Header("Background")]
        [SerializeField] private Image backgroundImage;

        [Header("Card List (built by AppBootstrap)")]
        [SerializeField] private CardWidget[] cards;

        [Header("Zone Progress (3 zones)")]
        [SerializeField] private Image[] zoneProgressBars;
        [SerializeField] private TMP_Text[] zoneProgressTexts;

        [Header("FX")]
        [SerializeField] private RectTransform completionBurstRoot;   // anchored to a card during burst
        [SerializeField] private TMP_Text completionBurstText;        // shows "+1" or "✓"
        [SerializeField] private float completionBurstSeconds = 1.2f;

        [Header("Visuals")]
        [SerializeField] private Color colorAvailable = new Color(1f, 1f, 1f, 1f);
        [SerializeField] private Color colorLocked    = new Color(0.45f, 0.45f, 0.50f, 1f);
        [SerializeField] private Color colorCompleted = new Color(0.55f, 0.85f, 0.55f, 1f);
        [SerializeField] private Color colorInsufficient = new Color(0.85f, 0.55f, 0.30f, 0.6f);

        private void OnEnable()
        {
            if (backButton != null) backButton.onClick.AddListener(HandleBack);

            if (cards != null)
            {
                foreach (var card in cards)
                {
                    if (card == null || card.restoreButton == null) continue;
                    CardWidget capture = card;
                    card.restoreButton.onClick.AddListener(() => HandleRestoreClicked(capture));
                }
            }

            if (CafeRestorationManager.instance != null)
                CafeRestorationManager.instance.OnStageRestoredManual += HandleStageRestored;

            ApplyStaticTexts();
            Refresh();
        }

        private void OnDisable()
        {
            if (backButton != null) backButton.onClick.RemoveListener(HandleBack);

            if (cards != null)
            {
                foreach (var card in cards)
                {
                    if (card == null || card.restoreButton == null) continue;
                    card.restoreButton.onClick.RemoveAllListeners();
                }
            }

            if (CafeRestorationManager.instance != null)
                CafeRestorationManager.instance.OnStageRestoredManual -= HandleStageRestored;
        }

        private void ApplyStaticTexts()
        {
            if (titleText == null) return;
            bool ko = I18nManager.Instance != null && I18nManager.Instance.currentLanguage == SystemLanguage.Korean;
            titleText.text = ko ? "카페 복원" : "Cafe Restoration";
        }

        public void Refresh()
        {
            CafeRestorationManager mgr = CafeRestorationManager.instance;
            if (mgr == null) return;

            // Total stars header
            int totalStars = GameManager.Instance?.UserProgress?.stars ?? 0;
            if (totalStarsText != null) totalStarsText.text = $"⭐ {totalStars}";

            // Background = current zone background (highest unlocked stage)
            if (backgroundImage != null)
            {
                Sprite bg = mgr.GetBackground(mgr.CurrentAreaId, Mathf.Max(1, mgr.CurrentStage));
                if (bg != null) { backgroundImage.sprite = bg; backgroundImage.color = Color.white; }
            }

            // Zone progress bars (3 zones)
            int zones = mgr.GetZoneCount();
            if (zoneProgressBars != null)
            {
                for (int i = 0; i < zoneProgressBars.Length; i++)
                {
                    if (zoneProgressBars[i] == null) continue;
                    int zone = i + 1;
                    if (zone > zones) { zoneProgressBars[i].fillAmount = 0f; continue; }
                    int total = mgr.GetZoneStageCount(zone);
                    int done = mgr.GetZoneStagesCompleted(zone);
                    zoneProgressBars[i].fillAmount = total > 0 ? (float)done / total : 0f;
                }
            }
            if (zoneProgressTexts != null)
            {
                for (int i = 0; i < zoneProgressTexts.Length; i++)
                {
                    if (zoneProgressTexts[i] == null) continue;
                    int zone = i + 1;
                    if (zone > zones) { zoneProgressTexts[i].text = ""; continue; }
                    int total = mgr.GetZoneStageCount(zone);
                    int done = mgr.GetZoneStagesCompleted(zone);
                    zoneProgressTexts[i].text = $"{zone}구역 {done}/{total}";
                }
            }

            // Cards
            if (cards == null) return;
            foreach (var card in cards)
            {
                if (card == null) continue;
                RefreshCard(card, mgr, totalStars);
            }
        }

        private void RefreshCard(CardWidget card, CafeRestorationManager mgr, int userStars)
        {
            var stage = mgr.GetStageData(card.areaId, card.stageIdx);
            if (stage == null) return;

            if (card.descText != null) card.descText.text = stage.description;
            if (card.starsText != null) card.starsText.text = $"⭐ {stage.starsRequired}";

            bool completed = mgr.IsStageCompleted(card.areaId, card.stageIdx);
            bool current   = mgr.IsStageCurrent(card.areaId, card.stageIdx);
            bool affordable = userStars >= stage.starsRequired;

            if (completed)
            {
                if (card.background != null) card.background.color = colorCompleted;
                if (card.stateLabel != null) card.stateLabel.text = "✓ Completed";
                if (card.restoreButton != null)
                {
                    card.restoreButton.gameObject.SetActive(false);
                }
            }
            else if (current)
            {
                if (card.background != null) card.background.color = affordable ? colorAvailable : colorInsufficient;
                if (card.stateLabel != null) card.stateLabel.text = affordable ? "" : "별 부족";
                if (card.restoreButton != null)
                {
                    card.restoreButton.gameObject.SetActive(true);
                    card.restoreButton.interactable = affordable;
                }
            }
            else
            {
                if (card.background != null) card.background.color = colorLocked;
                if (card.stateLabel != null) card.stateLabel.text = "🔒 Locked";
                if (card.restoreButton != null)
                {
                    card.restoreButton.gameObject.SetActive(false);
                }
            }
        }

        private void HandleRestoreClicked(CardWidget card)
        {
            AudioManager.instance?.PlayButtonClick();
            CafeRestorationManager mgr = CafeRestorationManager.instance;
            if (mgr == null) return;

            bool ok = mgr.TryRestoreCurrentStage(GameManager.Instance);
            if (!ok)
            {
                Debug.LogWarning("[CafeRestorationScreen] Restore failed (insufficient stars or no current stage)");
                Refresh();
                return;
            }

            AudioManager.instance?.PlayRewardGet();
            PlayCompletionBurst(card);
        }

        private void HandleStageRestored(int areaId, int stage)
        {
            // Manager has advanced state + faded background; just refresh card states.
            Refresh();

            // Zone milestone: when last stage of a zone completes, unlock the next cat.
            CafeRestorationManager mgr = CafeRestorationManager.instance;
            if (mgr == null) return;
            int zoneTotal = mgr.GetZoneStageCount(areaId);
            if (stage == zoneTotal)
            {
                TryUnlockCatForZone(areaId);
            }
        }

        private void TryUnlockCatForZone(int areaId)
        {
            // Zone 1 → Bella, Zone 2 → Sami, Zone 3 → Hodu
            int catId = 0;
            switch (areaId)
            {
                case 1: catId = WhiskerTales.Utilities.Constants.CAT_BELLA; break;
                case 2: catId = WhiskerTales.Utilities.Constants.CAT_SAMI;  break;
                case 3: catId = WhiskerTales.Utilities.Constants.CAT_HODU;  break;
                default: return;
            }
            if (CatManager.Instance == null) return;
            if (CatManager.Instance.IsCatUnlocked(catId)) return;
            CatManager.Instance.UnlockCat(catId);
            Debug.Log($"[CafeRestorationScreen] Zone {areaId} cleared — unlocked cat {catId}");
        }

        private void PlayCompletionBurst(CardWidget card)
        {
            if (completionBurstRoot == null || card == null || card.root == null) return;
            completionBurstRoot.SetParent(card.root, false);
            completionBurstRoot.anchoredPosition = Vector2.zero;
            completionBurstRoot.gameObject.SetActive(true);
            if (completionBurstText != null) completionBurstText.text = "✓";
            StopCoroutine(nameof(BurstAnim));
            StartCoroutine(nameof(BurstAnim));
        }

        private IEnumerator BurstAnim()
        {
            if (completionBurstRoot == null) yield break;
            float t = 0f;
            Vector3 startScale = Vector3.one * 0.5f;
            Vector3 endScale = Vector3.one * 1.8f;
            while (t < completionBurstSeconds)
            {
                t += Time.unscaledDeltaTime;
                float p = Mathf.Clamp01(t / completionBurstSeconds);
                completionBurstRoot.localScale = Vector3.Lerp(startScale, endScale, p);
                if (completionBurstText != null)
                {
                    Color c = completionBurstText.color;
                    c.a = 1f - p;
                    completionBurstText.color = c;
                }
                yield return null;
            }
            completionBurstRoot.gameObject.SetActive(false);
            completionBurstRoot.localScale = Vector3.one;
            if (completionBurstText != null)
            {
                Color c = completionBurstText.color; c.a = 1f; completionBurstText.color = c;
            }
        }

        private void HandleBack()
        {
            AudioManager.instance?.PlayButtonClick();
            GameManager.Instance?.ReturnToMenu();
        }
    }
}
