using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using WhiskerTales.Core;
using WhiskerTales.Feel;

namespace WhiskerTales.UI.Screens
{
    // V2-16: Home is a "돌아오는 장면" (a place to return to), not a menu.
    // Play button removed entirely. Per-screen tabs lifted to the scene-level TabBarController.
    // Only on-screen interactive element on Home is the top-right gear (Settings).
    // The center stays intentionally empty for V2-17+ assets (cat / hanok / wandering elements).
    public sealed class HomeScreenController : UIScreenBase
    {
        [Header("Top Right")]
        [SerializeField] private Button settingsButton;

        [Header("Navigation")]
        [SerializeField] private ScreenNavigator navigator;
        [SerializeField] private string settingsTargetId = "settings";

        protected override void Awake()
        {
            base.Awake();
            WireButton(settingsButton, OnSettingsPressed);
        }

        public override void Show(bool instant)
        {
            base.Show(instant);

            if (AudioService.Instance != null)
            {
                AudioService.Instance.PlayBgm(BgmId.HomeAmbience);
            }
        }

        public void SetNavigator(ScreenNavigator value)
        {
            navigator = value;
        }

        private void WireButton(Button button, UnityAction action)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }

        private void OnSettingsPressed()
        {
            Navigate(settingsTargetId, "Settings");
        }

        private void Navigate(string targetId, string label)
        {
            DebugLogger.Info(LogCategory.UI, "[HomeScreen] " + label + " -> " + targetId);

            if (navigator == null || string.IsNullOrEmpty(targetId) == true)
            {
                return;
            }

            navigator.Show(targetId);
        }
    }
}
