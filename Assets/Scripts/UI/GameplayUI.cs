using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WhiskerTales.Core;
using WhiskerTales.Puzzle;
using WhiskerTales.Utilities;

namespace WhiskerTales.UI
{
    public class GameplayUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI movesText;
        [SerializeField] private TextMeshProUGUI goalText;
        [SerializeField] private Slider progressSlider;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button menuButton;

        private Board board;
        private LevelGoal levelGoal;
        private GameManager gameManager;
        private int currentScore = 0;
        private Tile selectedTile = null;

        private void Start()
        {
            board = FindObjectOfType<Board>();
            levelGoal = FindObjectOfType<LevelGoal>();
            gameManager = GameManager.Instance;
            InitializeUI();
            SubscribeToEvents();
        }

        private void InitializeUI()
        {
            if (levelText != null && gameManager != null)
            {
                levelText.text = "Level " + gameManager.UserProgress.currentLevel;
            }
            if (pauseButton != null) pauseButton.onClick.AddListener(OnPauseClicked);
            if (menuButton != null) menuButton.onClick.AddListener(OnMenuClicked);
            UpdateUI();
        }

        private void SubscribeToEvents()
        {
            if (board != null)
            {
                board.OnTilesMatched += OnTilesMatched;
                board.OnBoardChanged += OnBoardChanged;
            }
            if (levelGoal != null)
            {
                levelGoal.OnProgressChanged += OnProgressChanged;
                levelGoal.OnMovesChanged += OnMovesChanged;
                levelGoal.OnGoalAchieved += OnGoalAchieved;
                levelGoal.OnMovesExceeded += OnMovesExceeded;
            }
        }

        public void OnTileClicked(Tile tile)
        {
            if (selectedTile == null)
            {
                selectedTile = tile;
                selectedTile.SetSelected(true);
            }
            else if (selectedTile == tile)
            {
                selectedTile.SetSelected(false);
                selectedTile = null;
            }
            else
            {
                if (board.TrySwapTiles(selectedTile, tile))
                {
                    if (levelGoal != null) levelGoal.UseMove();
                }
                selectedTile.SetSelected(false);
                selectedTile = null;
            }
        }

        public void UpdateScore(int score)
        {
            currentScore = score;
            if (scoreText != null) scoreText.text = "Score: " + currentScore;
        }

        private void OnTilesMatched(System.Collections.Generic.List<Tile> matchedTiles)
        {
            int matchScore = matchedTiles.Count * 100;
            currentScore += matchScore;
            if (scoreText != null) scoreText.text = "Score: " + currentScore;
            if (levelGoal != null) levelGoal.UpdateProgress(matchedTiles);
        }

        private void OnBoardChanged() { UpdateUI(); }
        private void OnProgressChanged(int newProgress) { UpdateUI(); }
        private void OnMovesChanged(int movesUsed, int moveLimit) { UpdateUI(); }

        private void OnGoalAchieved()
        {
            int stars = 0;
            if (levelGoal != null) stars = levelGoal.CalculateStars();
            if (gameManager != null) gameManager.CompleteLevel(gameManager.UserProgress.currentLevel, stars);
            Debug.Log("[GameplayUI] Level complete with " + stars + " stars!");
        }

        private void OnMovesExceeded()
        {
            if (gameManager != null) gameManager.FailLevel(gameManager.UserProgress.currentLevel);
            Debug.Log("[GameplayUI] Level failed!");
        }

        private void UpdateUI()
        {
            if (scoreText != null) scoreText.text = "Score: " + currentScore;
            if (levelGoal != null)
            {
                if (movesText != null) movesText.text = "Moves: " + levelGoal.GetRemainingMoves();
                if (goalText != null) goalText.text = levelGoal.GetGoalDescription() + "\n" + levelGoal.GetProgressDescription();
                if (progressSlider != null) progressSlider.value = levelGoal.GetProgressPercentage() / 100f;
            }
        }

        private void OnPauseClicked() { if (gameManager != null) gameManager.PauseGame(); }
        private void OnMenuClicked() { if (gameManager != null) gameManager.ReturnToMenu(); }

        private void OnDestroy()
        {
            if (board != null)
            {
                board.OnTilesMatched -= OnTilesMatched;
                board.OnBoardChanged -= OnBoardChanged;
            }
            if (levelGoal != null)
            {
                levelGoal.OnProgressChanged -= OnProgressChanged;
                levelGoal.OnMovesChanged -= OnMovesChanged;
                levelGoal.OnGoalAchieved -= OnGoalAchieved;
                levelGoal.OnMovesExceeded -= OnMovesExceeded;
            }
        }
    }
}
