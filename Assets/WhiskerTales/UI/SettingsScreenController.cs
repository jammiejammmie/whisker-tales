using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WhiskerTales.Core;

namespace WhiskerTales.UI
{
    public sealed class SettingsScreenController : UIScreenBase
    {
        [Header("Core")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private ScrollRect scrollRect;

        [Header("Toggles")]
        [SerializeField] private Toggle notificationToggle;
        [SerializeField] private Toggle detoxToggle;

        [Header("Sliders")]
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private Slider sfxSlider;

        [Header("Buttons")]
        [SerializeField] private Button privacyButton;
        [SerializeField] private Button contactButton;
        [SerializeField] private Button rateButton;
        [SerializeField] private Button diagnosticsButton;
        [SerializeField] private Button submitInviteButton;
        [SerializeField] private Button submitFeedbackButton;

        [Header("Fields")]
        [SerializeField] private TMP_InputField friendCodeInput;
        [SerializeField] private TMP_InputField feedbackInput;
        [SerializeField] private TMP_Text versionText;
        [SerializeField] private TMP_Text myCodeText;

        protected override void Awake()
        {
            base.Awake();
            BindButtons();
        }

        public override void Show()
        {
            base.Show();

            if (scrollRect != null)
            {
                scrollRect.verticalNormalizedPosition = 1f;
            }
        }

        public void SetVersion(string version)
        {
            if (versionText != null)
            {
                versionText.text = version;
            }
        }

        public void SetReferralCode(string code)
        {
            if (myCodeText != null)
            {
                myCodeText.text = code;
            }
        }

        private void BindButtons()
        {
            if (notificationToggle != null)
            {
                notificationToggle.onValueChanged.AddListener(value => DebugLogger.Info(LogCategory.UI, $"Notification toggle: {value}"));
            }

            if (detoxToggle != null)
            {
                detoxToggle.onValueChanged.AddListener(value => DebugLogger.Info(LogCategory.UI, $"Detox toggle: {value}"));
            }

            if (bgmSlider != null)
            {
                bgmSlider.onValueChanged.AddListener(value => DebugLogger.Info(LogCategory.Audio, $"BGM volume: {value}"));
            }

            if (sfxSlider != null)
            {
                sfxSlider.onValueChanged.AddListener(value => DebugLogger.Info(LogCategory.Audio, $"SFX volume: {value}"));
            }

            AddButtonLog(privacyButton, "Privacy");
            AddButtonLog(contactButton, "Contact");
            AddButtonLog(rateButton, "Rate");
            AddButtonLog(diagnosticsButton, "Diagnostics");
            AddButtonLog(submitInviteButton, "Submit Invite");
            AddButtonLog(submitFeedbackButton, "Submit Feedback");
        }

        private void AddButtonLog(Button button, string label)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.AddListener(() =>
            {
                DebugLogger.Info(LogCategory.UI, $"Settings button pressed: {label}");
                button.transform.DOKill();
                button.transform.DOPunchScale(Vector3.one * 0.05f, 0.18f, 5, 0.8f);
            });
        }
    }
}
