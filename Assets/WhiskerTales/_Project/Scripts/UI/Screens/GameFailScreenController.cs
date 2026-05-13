using UnityEngine;
using UnityEngine.UI;
using WhiskerTales.Core;
using WhiskerTales.Feel;

namespace WhiskerTales.UI.Screens
{
    public sealed class GameFailScreenController : BackNavScreenBase
    {
        [Header("Buttons")]
        [SerializeField] private Button homeButton;
        [SerializeField] private Button retryButton;

        [Header("Targets")]
        [SerializeField] private string homeTargetId = "home";
        [SerializeField] private string retryTargetId = "gameplay";

        protected override void Awake()
        {
            base.Awake();

            if (homeButton != null)
            {
                homeButton.onClick.RemoveAllListeners();
                homeButton.onClick.AddListener(() => Go(homeTargetId, "Home"));
            }

            if (retryButton != null)
            {
                retryButton.onClick.RemoveAllListeners();
                retryButton.onClick.AddListener(() => Go(retryTargetId, "Retry"));
            }
        }

        public override void Show(bool instant)
        {
            base.Show(instant);

            if (AudioService.Instance != null)
            {
                AudioService.Instance.PlaySfx(SfxId.LevelFail);
            }
        }

        private void Go(string targetId, string label)
        {
            DebugLogger.Info(LogCategory.UI, "[GameFailScreen] " + label + " -> " + targetId);

            if (navigator == null || string.IsNullOrEmpty(targetId) == true)
            {
                return;
            }

            navigator.Show(targetId);
        }
    }
}
