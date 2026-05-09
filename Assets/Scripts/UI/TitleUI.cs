using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WhiskerTales.Core;
using WhiskerTales.Utilities;

namespace WhiskerTales.UI
{
    /// <summary>
    /// 타이틀/메인 화면 (Stage 4 §4-1).
    /// 상단 HUD(하트/멸치/별/설정), 동적 배경, 로고+오늘의 빛깔 카피,
    /// 우상단 이벤트 배너 카운트다운, 하단 네비게이션 연동.
    /// </summary>
    public class TitleUI : MonoBehaviour
    {
        [Header("Top HUD")]
        [SerializeField] private TMP_Text livesText;
        [SerializeField] private TMP_Text livesTimerText;
        [SerializeField] private Button livesPlusButton;
        [SerializeField] private TMP_Text anchoviesText;
        [SerializeField] private Button anchoviesPlusButton;
        [SerializeField] private TMP_Text starsText;
        [SerializeField] private Button settingsButton;

        [Header("Center")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TMP_Text logoText;
        [SerializeField] private TMP_Text dailyCopyText;

        [Header("Event Banner (top-right)")]
        [SerializeField] private GameObject eventBannerRoot;
        [SerializeField] private TMP_Text eventCountdownText;
        [SerializeField] private int eventDurationDays = 3;
        [SerializeField] private int eventDurationHours = 12;

        [Header("Panels")]
        [SerializeField] private GameObject titlePanel;
        [SerializeField] private GameObject settingsPanel;

        private DateTime eventEndTime;

        private void OnEnable()
        {
            if (HeartSystem.Instance != null)
            {
                HeartSystem.Instance.OnLivesChanged += HandleLivesChanged;
                HeartSystem.Instance.OnTickRefreshTime += HandleLivesTick;
            }
            if (livesPlusButton != null) livesPlusButton.onClick.AddListener(HandleLivesPlusClicked);
            if (anchoviesPlusButton != null) anchoviesPlusButton.onClick.AddListener(HandleAnchoviesPlusClicked);
            if (settingsButton != null) settingsButton.onClick.AddListener(HandleSettingsClicked);
        }

        private void OnDisable()
        {
            if (HeartSystem.Instance != null)
            {
                HeartSystem.Instance.OnLivesChanged -= HandleLivesChanged;
                HeartSystem.Instance.OnTickRefreshTime -= HandleLivesTick;
            }
            if (livesPlusButton != null) livesPlusButton.onClick.RemoveListener(HandleLivesPlusClicked);
            if (anchoviesPlusButton != null) anchoviesPlusButton.onClick.RemoveListener(HandleAnchoviesPlusClicked);
            if (settingsButton != null) settingsButton.onClick.RemoveListener(HandleSettingsClicked);
        }

        private void Start()
        {
            InitEventEndTime();
            ApplyDailyCopy();
            ApplyLogoText();
            RefreshAll();
            StartCoroutine(BannerTickRoutine());
        }

        private void InitEventEndTime()
        {
            const string key = "title.eventEndTimeBinary";
            if (PlayerPrefs.HasKey(key))
            {
                long bin = Convert.ToInt64(PlayerPrefs.GetString(key));
                eventEndTime = DateTime.FromBinary(bin);
                if (eventEndTime <= DateTime.Now)
                {
                    eventEndTime = DateTime.Now.AddDays(eventDurationDays).AddHours(eventDurationHours);
                    PlayerPrefs.SetString(key, eventEndTime.ToBinary().ToString());
                }
            }
            else
            {
                eventEndTime = DateTime.Now.AddDays(eventDurationDays).AddHours(eventDurationHours);
                PlayerPrefs.SetString(key, eventEndTime.ToBinary().ToString());
            }
        }

        private void RefreshAll()
        {
            RefreshLives();
            RefreshAnchovies();
            RefreshStars();
            RefreshBackground();
            RefreshEventCountdown();
        }

        private void RefreshLives()
        {
            int lives = HeartSystem.Instance != null ? HeartSystem.Instance.CurrentLives : Constants.MAX_LIVES;
            if (livesText != null)
            {
                livesText.text = lives >= Constants.MAX_LIVES ? "FULL" : lives.ToString();
            }
            RefreshLivesTimer();
        }

        private void RefreshLivesTimer()
        {
            if (livesTimerText == null) return;
            if (HeartSystem.Instance == null) { livesTimerText.text = string.Empty; return; }

            if (HeartSystem.Instance.IsFull)
            {
                livesTimerText.text = string.Empty;
                return;
            }

            TimeSpan t = HeartSystem.Instance.TimeUntilNextLife;
            livesTimerText.text = $"{t.Minutes:D2}:{t.Seconds:D2}";
        }

        private void RefreshAnchovies()
        {
            if (anchoviesText == null) return;
            int amount = GameManager.Instance?.UserProgress?.coins ?? 0;
            anchoviesText.text = amount.ToString("N0");
        }

        private void RefreshStars()
        {
            if (starsText == null) return;
            int amount = GameManager.Instance?.UserProgress?.stars ?? 0;
            starsText.text = amount.ToString();
        }

        private void RefreshBackground()
        {
            if (backgroundImage == null) return;
            if (CafeRestorationManager.instance == null) return;

            Sprite sp = CafeRestorationManager.instance.GetCurrentBackground();
            if (sp != null) backgroundImage.sprite = sp;
        }

        private void RefreshEventCountdown()
        {
            if (eventCountdownText == null) return;
            TimeSpan remain = eventEndTime - DateTime.Now;
            if (remain <= TimeSpan.Zero)
            {
                eventCountdownText.text = "00h 00m";
                if (eventBannerRoot != null) eventBannerRoot.SetActive(false);
                return;
            }
            if (eventBannerRoot != null) eventBannerRoot.SetActive(true);

            if (remain.TotalDays >= 1)
            {
                eventCountdownText.text = $"{(int)remain.TotalDays}d {remain.Hours}h";
            }
            else
            {
                eventCountdownText.text = $"{remain.Hours:D2}h {remain.Minutes:D2}m";
            }
        }

        private void ApplyDailyCopy()
        {
            if (dailyCopyText == null) return;
            var entry = DailyCopy.GetToday();
            dailyCopyText.text = entry.Text;
            dailyCopyText.color = entry.Accent;
        }

        private void ApplyLogoText()
        {
            if (logoText == null) return;
            logoText.text = I18nManager.Instance != null
                ? I18nManager.Instance.GetLocalizedText("game_title")
                : "Whisker Tales";
        }

        private IEnumerator BannerTickRoutine()
        {
            var wait = new WaitForSecondsRealtime(1f);
            while (true)
            {
                RefreshEventCountdown();
                RefreshLivesTimer();
                yield return wait;
            }
        }

        private void HandleLivesChanged(int _) => RefreshLives();
        private void HandleLivesTick(TimeSpan _) => RefreshLivesTimer();

        private void HandleLivesPlusClicked()
        {
            // 구매/광고 시트는 §4-9 / IAP 통합 시 연결. 지금은 hook만.
            AudioManager.instance?.PlayButtonClick();
            Debug.Log("[TitleUI] Lives + button clicked (TODO: IAP / ad sheet)");
        }

        private void HandleAnchoviesPlusClicked()
        {
            AudioManager.instance?.PlayButtonClick();
            Debug.Log("[TitleUI] Anchovies + button clicked (TODO: shop)");
        }

        private void HandleSettingsClicked()
        {
            AudioManager.instance?.PlayButtonClick();
            if (titlePanel != null) titlePanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(true);
        }
    }
}
