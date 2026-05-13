using UnityEngine;
using UnityEngine.UI;
using WhiskerTales.Core;
using WhiskerTales.Feel;
using WhiskerTales.Save;

namespace WhiskerTales.UI.Screens
{
    public sealed class LevelClearScreenController : BackNavScreenBase
    {
        [Header("Buttons")]
        [SerializeField] private Button homeButton;
        [SerializeField] private Button nextButton;

        [Header("Targets")]
        [SerializeField] private string homeTargetId = "home";
        [SerializeField] private string nextTargetId = "gameplay";

        protected override void Awake()
        {
            base.Awake();

            if (homeButton != null)
            {
                homeButton.onClick.RemoveAllListeners();
                homeButton.onClick.AddListener(() => Go(homeTargetId, "Home"));
            }

            if (nextButton != null)
            {
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(() => Go(nextTargetId, "Next"));
            }
        }

        public override void Show(bool instant)
        {
            base.Show(instant);

            PlayerProgressService.MarkLevelCleared(GameplaySession.SelectedLevelId);

            if (AudioService.Instance != null)
            {
                AudioService.Instance.PlaySfx(SfxId.LevelClear);
            }
        }

        private void Go(string targetId, string label)
        {
            DebugLogger.Info(LogCategory.UI, "[LevelClearScreen] " + label + " -> " + targetId);

            if (navigator == null || string.IsNullOrEmpty(targetId) == true)
            {
                return;
            }

            navigator.Show(targetId);
        }
    }
}
