using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WhiskerTales.Core;
using WhiskerTales.Puzzle;

namespace WhiskerTales.Integration
{
    // V2 → V1 bridge for the Match-3 stack. Owns the lifetime of a single playthrough:
    // creates Board + LevelGoal + BoardView under a host RectTransform, tears them down on Dispose.
    // Subscribes to Board.OnLevelComplete / OnLevelFailed and forwards them via plain delegates so
    // GameplayScreenController can route to LevelClear / GameFail without leaking V1 types upward.
    public sealed class Match3RuntimeAdapter
    {
        public delegate void LevelClearedHandler(int stars);
        public delegate void LevelFailedHandler();

        public event LevelClearedHandler LevelCleared;
        public event LevelFailedHandler LevelFailed;

        private readonly Transform host;
        private readonly RectTransform gridContainer;
        private readonly TextMeshProUGUI goalText;
        private readonly TextMeshProUGUI movesText;
        private readonly TextMeshProUGUI statusText;

        private GameObject boardGo;
        private GameObject levelGoalGo;
        private GameObject boardViewGo;
        private Board board;
        private bool active;

        public bool IsActive
        {
            get { return active; }
        }

        public Match3RuntimeAdapter(Transform host, RectTransform gridContainer,
            TextMeshProUGUI goalText, TextMeshProUGUI movesText, TextMeshProUGUI statusText)
        {
            this.host = host;
            this.gridContainer = gridContainer;
            this.goalText = goalText;
            this.movesText = movesText;
            this.statusText = statusText;
        }

        public void StartLevel(int levelId, int moveLimit, LevelGoalType goalType, int goalValue)
        {
            Dispose();

            if (gridContainer == null)
            {
                DebugLogger.Error(LogCategory.Puzzle, "[Match3RuntimeAdapter] gridContainer null; cannot start level.");
                return;
            }

            boardGo = new GameObject("Board");
            boardGo.transform.SetParent(host, false);
            board = boardGo.AddComponent<Board>();

            levelGoalGo = new GameObject("LevelGoal");
            levelGoalGo.transform.SetParent(host, false);
            LevelGoal levelGoal = levelGoalGo.AddComponent<LevelGoal>();

            // Fully qualified — WhiskerTales.Core.Level (DataModels) also exists and collides with
            // WhiskerTales.Puzzle.Level via the two using directives above.
            WhiskerTales.Puzzle.Level level = new WhiskerTales.Puzzle.Level
            {
                levelId = levelId,
                moveLimit = moveLimit,
                goalType = goalType,
                goalValue = goalValue
            };
            board.Initialize(level, levelGoal);

            boardViewGo = new GameObject("BoardView");
            boardViewGo.transform.SetParent(host, false);
            BoardView view = boardViewGo.AddComponent<BoardView>();
            view.board = board;
            view.levelGoal = levelGoal;
            view.gridContainer = gridContainer;
            view.goalText = goalText;
            view.movesText = movesText;
            view.statusText = statusText;
            view.BuildGrid();

            board.OnLevelComplete += HandleLevelComplete;
            board.OnLevelFailed += HandleLevelFailed;

            active = true;
            DebugLogger.Info(LogCategory.Puzzle, "[Match3RuntimeAdapter] Level " + levelId + " started (moves=" + moveLimit + " goal=" + goalType + ":" + goalValue + ").");
        }

        public void Dispose()
        {
            if (board != null)
            {
                board.OnLevelComplete -= HandleLevelComplete;
                board.OnLevelFailed -= HandleLevelFailed;
                board = null;
            }

            DestroyIfPresent(boardViewGo);
            DestroyIfPresent(levelGoalGo);
            DestroyIfPresent(boardGo);

            ClearGridChildren();

            boardGo = null;
            levelGoalGo = null;
            boardViewGo = null;
            active = false;
        }

        private void HandleLevelComplete(int stars)
        {
            LevelClearedHandler handler = LevelCleared;

            if (handler != null)
            {
                handler.Invoke(stars);
            }
        }

        private void HandleLevelFailed()
        {
            LevelFailedHandler handler = LevelFailed;

            if (handler != null)
            {
                handler.Invoke();
            }
        }

        private static void DestroyIfPresent(GameObject go)
        {
            if (go == null)
            {
                return;
            }

            if (Application.isPlaying == true)
            {
                Object.Destroy(go);
            }
            else
            {
                Object.DestroyImmediate(go);
            }
        }

        private void ClearGridChildren()
        {
            if (gridContainer == null)
            {
                return;
            }

            for (int i = gridContainer.childCount - 1; i >= 0; i--)
            {
                Transform child = gridContainer.GetChild(i);

                if (child == null)
                {
                    continue;
                }

                if (Application.isPlaying == true)
                {
                    Object.Destroy(child.gameObject);
                }
                else
                {
                    Object.DestroyImmediate(child.gameObject);
                }
            }
        }
    }
}
