using UnityEngine;
using UnityEngine.UI;
using WhiskerTales.Core;
using WhiskerTales.Save;

namespace WhiskerTales.UI.Screens
{
    public sealed class LevelSelectScreenController : BackNavScreenBase
    {
        [Header("Levels")]
        [SerializeField] private Button[] levelButtons;
        [SerializeField] private string playTargetId = "gameplay";

        [Header("Lock Visual")]
        [SerializeField] private Color unlockedTint = Color.white;
        [SerializeField] private Color lockedTint = new Color(0.65f, 0.62f, 0.58f, 0.85f);

        protected override void Awake()
        {
            base.Awake();

            if (levelButtons == null)
            {
                return;
            }

            for (int i = 0; i < levelButtons.Length; i++)
            {
                int capturedLevelId = i + 1;
                Button button = levelButtons[i];

                if (button == null)
                {
                    continue;
                }

                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => SelectLevel(capturedLevelId));
            }
        }

        public override void Show(bool instant)
        {
            base.Show(instant);
            RefreshLockState();
        }

        private void RefreshLockState()
        {
            if (levelButtons == null)
            {
                return;
            }

            int maxUnlocked = PlayerProgressService.MaxUnlockedLevel;

            for (int i = 0; i < levelButtons.Length; i++)
            {
                Button button = levelButtons[i];

                if (button == null)
                {
                    continue;
                }

                int levelId = i + 1;
                bool unlocked = levelId <= maxUnlocked;

                button.interactable = unlocked;

                Image image = button.targetGraphic as Image;

                if (image != null)
                {
                    image.color = unlocked == true ? unlockedTint : lockedTint;
                }
            }

            DebugLogger.Info(LogCategory.UI, "[LevelSelect] refreshed locks. maxUnlocked=" + maxUnlocked);
        }

        private void SelectLevel(int levelId)
        {
            if (PlayerProgressService.IsLevelUnlocked(levelId) == false)
            {
                DebugLogger.Info(LogCategory.UI, "[LevelSelect] level " + levelId + " locked; click ignored.");
                return;
            }

            GameplaySession.SelectedLevelId = levelId;
            DebugLogger.Info(LogCategory.UI, "[LevelSelect] -> level " + levelId);

            if (navigator == null || string.IsNullOrEmpty(playTargetId) == true)
            {
                return;
            }

            navigator.Show(playTargetId);
        }
    }
}
