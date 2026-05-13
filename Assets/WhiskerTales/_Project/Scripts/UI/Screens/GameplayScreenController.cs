using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WhiskerTales.Core;
using WhiskerTales.Feel;
using WhiskerTales.Integration;
using WhiskerTales.Puzzle;

namespace WhiskerTales.UI.Screens
{
    public sealed class GameplayScreenController : BackNavScreenBase
    {
        [Header("Match3 Mount Points")]
        [SerializeField] private RectTransform boardArea;
        [SerializeField] private Transform match3Host;
        [SerializeField] private TextMeshProUGUI goalText;
        [SerializeField] private TextMeshProUGUI movesText;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private TextMeshProUGUI levelTitle;

        [Header("Targets")]
        [SerializeField] private string levelClearTargetId = "levelclear";
        [SerializeField] private string gameFailTargetId = "gamefail";

        [Header("Level Defaults (until JSON loader is wired)")]
        [SerializeField] private int defaultMoveLimit = 25;
        [SerializeField] private int defaultGoalValue = 30;

        private Match3RuntimeAdapter adapter;

        public override void Show(bool instant)
        {
            base.Show(instant);

            if (AudioService.Instance != null)
            {
                AudioService.Instance.PlayBgm(BgmId.GameplayLoop);
            }

            EnsureAdapter();
            StartLevel();
        }

        public override void Hide(bool instant)
        {
            DisposeAdapter();
            base.Hide(instant);
        }

        protected override void OnBackPressed()
        {
            DisposeAdapter();
            base.OnBackPressed();
        }

        private void EnsureAdapter()
        {
            if (adapter != null)
            {
                return;
            }

            Transform host = match3Host != null ? match3Host : transform;
            adapter = new Match3RuntimeAdapter(host, boardArea, goalText, movesText, statusText);
            adapter.LevelCleared += OnLevelCleared;
            adapter.LevelFailed += OnLevelFailed;
        }

        private void StartLevel()
        {
            if (adapter == null)
            {
                return;
            }

            int levelId = GameplaySession.SelectedLevelId;

            if (levelTitle != null)
            {
                levelTitle.text = "Level " + levelId;
            }

            adapter.StartLevel(levelId, defaultMoveLimit, LevelGoalType.RemoveBlocks, defaultGoalValue);
        }

        private void DisposeAdapter()
        {
            if (adapter == null)
            {
                return;
            }

            adapter.LevelCleared -= OnLevelCleared;
            adapter.LevelFailed -= OnLevelFailed;
            adapter.Dispose();
            adapter = null;
        }

        private void OnLevelCleared(int stars)
        {
            DebugLogger.Info(LogCategory.Puzzle, "[GameplayScreen] LevelCleared stars=" + stars);
            DisposeAdapter();

            if (navigator != null)
            {
                navigator.Show(levelClearTargetId);
            }
        }

        private void OnLevelFailed()
        {
            DebugLogger.Info(LogCategory.Puzzle, "[GameplayScreen] LevelFailed");
            DisposeAdapter();

            if (navigator != null)
            {
                navigator.Show(gameFailTargetId);
            }
        }
    }
}
