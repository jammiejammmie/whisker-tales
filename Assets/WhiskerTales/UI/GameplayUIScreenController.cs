using TMPro;
using UnityEngine;
using WhiskerTales.Core;

namespace WhiskerTales.UI
{
    public sealed class GameplayUIScreenController : UIScreenBase
    {
        [SerializeField] private RectTransform topHud;
        [SerializeField] private RectTransform puzzleBoardAnchor;
        [SerializeField] private RectTransform boosterRow;
        [SerializeField] private RectTransform goalPanel;

        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI movesText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI goalText;

        protected override void Awake()
        {
            base.Awake();
            ApplyLayout();
        }

        private void OnEnable()
        {
            GameEvents.OnGoalUpdated += HandleGoalUpdated;
            GameEvents.OnLevelStarted += HandleLevelStarted;
        }

        private void OnDisable()
        {
            GameEvents.OnGoalUpdated -= HandleGoalUpdated;
            GameEvents.OnLevelStarted -= HandleLevelStarted;
        }

        public void ApplyLayout()
        {
            if (topHud != null)
            {
                UIFactory.ApplyRect(topHud, new Vector2(0, -88), new Vector2(1000, 150), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            }

            if (puzzleBoardAnchor != null)
            {
                float boardSize = GameConstants.Board.Size * 96f;
                UIFactory.ApplyRect(puzzleBoardAnchor, new Vector2(0, -575), new Vector2(boardSize, boardSize), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            }

            if (boosterRow != null)
            {
                UIFactory.ApplyRect(boosterRow, new Vector2(0, 400), new Vector2(640, 130), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            }

            if (goalPanel != null)
            {
                UIFactory.ApplyRect(goalPanel, new Vector2(0, 170), new Vector2(820, 180), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            }
        }

        public void SetHud(int level, int moves, int score)
        {
            if (levelText != null)
            {
                levelText.text = "Level " + level;
            }

            if (movesText != null)
            {
                movesText.text = "Moves: " + moves;
            }

            if (scoreText != null)
            {
                scoreText.text = "Score: " + score;
            }
        }

        private void HandleLevelStarted(int level)
        {
            if (levelText != null)
            {
                levelText.text = "Level " + level;
            }
        }

        private void HandleGoalUpdated(int current, int target)
        {
            if (goalText != null)
            {
                goalText.text = "목표 " + current + "/" + target;
            }
        }
    }
}
