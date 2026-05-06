using UnityEngine;
using System.Collections.Generic;
using WhiskerTales.Puzzle;
using WhiskerTales.Core;
using WhiskerTales.UI;

namespace WhiskerTales.Game
{
    public class Match3Engine : MonoBehaviour
    {
        [SerializeField] private Board board;
        [SerializeField] private LevelGoal levelGoal;
        [SerializeField] private GameplayUI gameplayUI;

        private int currentScore = 0;

        public event System.Action<int> OnScoreChanged;
        public event System.Action<List<Tile>> OnTilesMatched;
        public event System.Action OnGameOver;

        private void Start()
        {
            if (board == null) board = GetComponent<Board>();
            if (levelGoal == null) levelGoal = GetComponent<LevelGoal>();
            if (gameplayUI == null) gameplayUI = GetComponent<GameplayUI>();

            currentScore = 0;
            if (board != null) board.Initialize();
        }

        public bool TrySwapTiles(int x1, int y1, int x2, int y2)
        {
            if (board == null) return false;

            Tile tile1 = board.GetTile(x1, y1);
            Tile tile2 = board.GetTile(x2, y2);

            if (tile1 == null || tile2 == null) return false;

            if (board.TrySwapTiles(tile1, tile2))
            {
                levelGoal?.UseMove();
                return true;
            }
            return false;
        }

        public void AddScore(int points)
        {
            currentScore += points;
            OnScoreChanged?.Invoke(currentScore);
            if (gameplayUI != null) gameplayUI.UpdateScore(currentScore);
        }

        public int GetCurrentScore() { return currentScore; }

        public void ResetGame()
        {
            currentScore = 0;
            if (board != null) board.Initialize();
        }

        public Board GetBoard() { return board; }
        public LevelGoal GetLevelGoal() { return levelGoal; }
    }
}
