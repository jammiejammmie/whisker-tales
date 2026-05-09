using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WhiskerTales.Audio;
using WhiskerTales.Core;
using WhiskerTales.Settings;

namespace WhiskerTales.UI
{
    /// <summary>
    /// Phase B §3-5. 설정 화면 + 인앱 피드백 폼.
    /// 모든 컨트롤은 AppBootstrap이 미리 빌드해서 SerializeField로 주입.
    /// SettingsManager에 즉시 read-back/write-through.
    /// </summary>
    public class SettingsScreen : MonoBehaviour
    {
        public const string APP_VERSION = "v1.0.0";
        public const string PRIVACY_URL  = "https://whiskertales-mwjyt48n.manus.space/privacy-policy";
        public const string SUPPORT_MAIL = "support@nyangstudio.com";
        public const string RATE_URL     = "https://play.google.com/store/apps/details?id=com.nyangstudio.whiskertales";

        [Header("Top Bar")]
        [SerializeField] private Button backButton;

        [Header("Notifications")]
        [SerializeField] private Button dailyNotificationToggleButton;
        [SerializeField] private TMP_Text dailyNotificationLabel;

        [Header("Sound")]
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Button soundModeNormalButton;
        [SerializeField] private Button soundModeCatButton;
        [SerializeField] private Button soundModeMuteButton;

        [Header("Detox")]
        [SerializeField] private Button detoxToggleButton;
        [SerializeField] private TMP_Text detoxLabel;

        [Header("Language")]
        [SerializeField] private Button langKoButton;
        [SerializeField] private Button langEnButton;

        [Header("Info / External")]
        [SerializeField] private Button privacyButton;
        [SerializeField] private Button mailButton;
        [SerializeField] private Button rateButton;
        [SerializeField] private TMP_Text versionLabel;

        [Header("Feedback")]
        [SerializeField] private Button[] starButtons; // 5개
        [SerializeField] private TMP_InputField feedbackInput;
        [SerializeField] private Button feedbackSubmitButton;

        private static readonly Color SelectedColor   = new Color(0.95f, 0.55f, 0.30f, 1f);
        private static readonly Color UnselectedColor = new Color(0.55f, 0.55f, 0.60f, 1f);
        private static readonly Color StarFilledColor = new Color(1f, 0.85f, 0.30f);
        private static readonly Color StarEmptyColor  = new Color(0.55f, 0.55f, 0.60f);

        private int feedbackStarRating;

        public int FeedbackStarRating => feedbackStarRating;

        private void OnEnable()
        {
            BindButtons();
            RefreshFromSettings();
        }

        private void OnDisable()
        {
            UnbindButtons();
        }

        private void BindButtons()
        {
            if (backButton != null) backButton.onClick.AddListener(HandleBack);

            if (dailyNotificationToggleButton != null)
                dailyNotificationToggleButton.onClick.AddListener(ToggleDailyNotification);
            if (detoxToggleButton != null)
                detoxToggleButton.onClick.AddListener(ToggleDetox);

            if (bgmSlider != null) bgmSlider.onValueChanged.AddListener(OnBgmSliderChanged);
            if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);

            if (soundModeNormalButton != null) soundModeNormalButton.onClick.AddListener(() => SetSoundMode(SoundMode.Normal));
            if (soundModeCatButton    != null) soundModeCatButton.onClick.AddListener(() => SetSoundMode(SoundMode.Cat));
            if (soundModeMuteButton   != null) soundModeMuteButton.onClick.AddListener(() => SetSoundMode(SoundMode.Mute));

            if (langKoButton != null) langKoButton.onClick.AddListener(() => SetLanguage(SettingsManager.LANG_KO));
            if (langEnButton != null) langEnButton.onClick.AddListener(() => SetLanguage(SettingsManager.LANG_EN));

            if (privacyButton != null) privacyButton.onClick.AddListener(() => OpenURL(PRIVACY_URL));
            if (mailButton    != null) mailButton.onClick.AddListener(() => OpenURL($"mailto:{SUPPORT_MAIL}"));
            if (rateButton    != null) rateButton.onClick.AddListener(() => OpenURL(RATE_URL));

            if (starButtons != null)
            {
                for (int i = 0; i < starButtons.Length; i++)
                {
                    if (starButtons[i] == null) continue;
                    int captured = i + 1;
                    starButtons[i].onClick.AddListener(() => SetFeedbackStars(captured));
                }
            }
            if (feedbackSubmitButton != null) feedbackSubmitButton.onClick.AddListener(HandleFeedbackSubmit);
        }

        private void UnbindButtons()
        {
            if (backButton != null) backButton.onClick.RemoveListener(HandleBack);
            if (dailyNotificationToggleButton != null) dailyNotificationToggleButton.onClick.RemoveListener(ToggleDailyNotification);
            if (detoxToggleButton != null) detoxToggleButton.onClick.RemoveListener(ToggleDetox);
            if (bgmSlider != null) bgmSlider.onValueChanged.RemoveListener(OnBgmSliderChanged);
            if (sfxSlider != null) sfxSlider.onValueChanged.RemoveListener(OnSfxSliderChanged);

            if (soundModeNormalButton != null) soundModeNormalButton.onClick.RemoveAllListeners();
            if (soundModeCatButton    != null) soundModeCatButton.onClick.RemoveAllListeners();
            if (soundModeMuteButton   != null) soundModeMuteButton.onClick.RemoveAllListeners();
            if (langKoButton != null) langKoButton.onClick.RemoveAllListeners();
            if (langEnButton != null) langEnButton.onClick.RemoveAllListeners();

            if (privacyButton != null) privacyButton.onClick.RemoveAllListeners();
            if (mailButton    != null) mailButton.onClick.RemoveAllListeners();
            if (rateButton    != null) rateButton.onClick.RemoveAllListeners();

            if (starButtons != null)
                foreach (var b in starButtons) if (b != null) b.onClick.RemoveAllListeners();
            if (feedbackSubmitButton != null) feedbackSubmitButton.onClick.RemoveListener(HandleFeedbackSubmit);
        }

        // ===== State refresh =====

        public void RefreshFromSettings()
        {
            SettingsManager s = SettingsManager.Instance;
            if (s == null) return;

            UpdateDailyNotificationLabel(s.DailyNotificationEnabled);
            UpdateDetoxLabel(s.DetoxModeEnabled);

            if (bgmSlider != null) bgmSlider.SetValueWithoutNotify(s.BgmVolume);
            if (sfxSlider != null) sfxSlider.SetValueWithoutNotify(s.SfxVolume);

            UpdateSoundModeVisual(SoundManager.Instance != null ? SoundManager.Instance.CurrentMode : SoundMode.Cat);
            UpdateLanguageVisual(s.Language);

            if (versionLabel != null) versionLabel.text = $"버전 {APP_VERSION}";

            UpdateStarsVisual();
        }

        // ===== Toggles =====

        private void ToggleDailyNotification()
        {
            SettingsManager s = SettingsManager.Instance;
            if (s == null) return;
            s.DailyNotificationEnabled = !s.DailyNotificationEnabled;
            UpdateDailyNotificationLabel(s.DailyNotificationEnabled);
            AudioManager.instance?.PlayButtonClick();
        }

        private void ToggleDetox()
        {
            SettingsManager s = SettingsManager.Instance;
            if (s == null) return;
            s.DetoxModeEnabled = !s.DetoxModeEnabled;
            UpdateDetoxLabel(s.DetoxModeEnabled);
            AudioManager.instance?.PlayButtonClick();
        }

        private void UpdateDailyNotificationLabel(bool on)
        {
            if (dailyNotificationLabel != null) dailyNotificationLabel.text = on ? "ON" : "OFF";
            if (dailyNotificationToggleButton != null)
            {
                Image img = dailyNotificationToggleButton.GetComponent<Image>();
                if (img != null) img.color = on ? new Color(0.483f, 0.722f, 0.553f) : UnselectedColor;
            }
        }

        private void UpdateDetoxLabel(bool on)
        {
            if (detoxLabel != null) detoxLabel.text = on ? "ON" : "OFF";
            if (detoxToggleButton != null)
            {
                Image img = detoxToggleButton.GetComponent<Image>();
                if (img != null) img.color = on ? new Color(0.483f, 0.722f, 0.553f) : UnselectedColor;
            }
        }

        // ===== Sliders =====

        private void OnBgmSliderChanged(float v)
        {
            SettingsManager s = SettingsManager.Instance;
            if (s == null) return;
            s.BgmVolume = v;
        }

        private void OnSfxSliderChanged(float v)
        {
            SettingsManager s = SettingsManager.Instance;
            if (s == null) return;
            s.SfxVolume = v;
        }

        // ===== Sound mode (3-way) =====

        private void SetSoundMode(SoundMode mode)
        {
            SoundManager.Instance?.SetMode(mode);
            UpdateSoundModeVisual(mode);
            AudioManager.instance?.PlayButtonClick();
        }

        private void UpdateSoundModeVisual(SoundMode active)
        {
            ColorButton(soundModeNormalButton, active == SoundMode.Normal);
            ColorButton(soundModeCatButton,    active == SoundMode.Cat);
            ColorButton(soundModeMuteButton,   active == SoundMode.Mute);
        }

        // ===== Language (2-way) =====

        private void SetLanguage(string code)
        {
            SettingsManager s = SettingsManager.Instance;
            if (s == null) return;
            s.Language = code;
            UpdateLanguageVisual(code);
            AudioManager.instance?.PlayButtonClick();
        }

        private void UpdateLanguageVisual(string lang)
        {
            ColorButton(langKoButton, lang == SettingsManager.LANG_KO);
            ColorButton(langEnButton, lang == SettingsManager.LANG_EN);
        }

        private static void ColorButton(Button btn, bool selected)
        {
            if (btn == null) return;
            Image img = btn.GetComponent<Image>();
            if (img != null) img.color = selected ? SelectedColor : UnselectedColor;
        }

        // ===== External =====

        private void OpenURL(string url)
        {
            AudioManager.instance?.PlayButtonClick();
            if (string.IsNullOrEmpty(url)) return;
            Application.OpenURL(url);
            Debug.Log($"[Settings] OpenURL: {url}");
        }

        // ===== Feedback =====

        private void SetFeedbackStars(int count)
        {
            feedbackStarRating = Mathf.Clamp(count, 0, 5);
            UpdateStarsVisual();
            AudioManager.instance?.PlayButtonClick();
        }

        private void UpdateStarsVisual()
        {
            if (starButtons == null) return;
            for (int i = 0; i < starButtons.Length; i++)
            {
                if (starButtons[i] == null) continue;
                Image img = starButtons[i].GetComponent<Image>();
                if (img != null) img.color = (i < feedbackStarRating) ? StarFilledColor : StarEmptyColor;
            }
        }

        private void HandleFeedbackSubmit()
        {
            AudioManager.instance?.PlayButtonClick();
            string text = feedbackInput != null ? feedbackInput.text ?? "" : "";
            int rating = feedbackStarRating;
            string subject = $"[Whisker Tales] User Feedback ({rating}/5 stars)";
            string body = $"별점: {rating}/5\n\n{text}\n\n--\nApp: {APP_VERSION}\nDevice: {SystemInfo.deviceModel}\nOS: {SystemInfo.operatingSystem}";
            string url = $"mailto:{SUPPORT_MAIL}?subject={Uri.EscapeDataString(subject)}&body={Uri.EscapeDataString(body)}";
            Application.OpenURL(url);
            Debug.Log($"[Settings] Feedback submitted: {rating}★ — \"{text}\"");

            if (feedbackInput != null) feedbackInput.text = "";
            SetFeedbackStars(0);
        }

        // ===== Back =====

        private void HandleBack()
        {
            AudioManager.instance?.PlayButtonClick();
            GameManager.Instance?.ReturnToMenu();
        }

#if UNITY_EDITOR
        public void DebugSetFeedbackStars(int n) => SetFeedbackStars(n);
#endif
    }
}
