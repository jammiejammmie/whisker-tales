using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WhiskerTales.Core;
using WhiskerTales.Puzzle;
using WhiskerTales.Utilities;

namespace WhiskerTales.UI
{
    /// <summary>
    /// 게임 플레이 화면 UI 관리
    /// 점수, 이동, 목표, 버튼 등 관리
    /// </summary>
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

        /// <summary>
        /// UI 초기화
        /// </summary>
        private void InitializeUI()
        {
            if (levelText != null)
            {
                levelText.text = $"Level {gameManager.UserProgress.currentLevel}";
            }

            if (pauseButton != null)
            {
                pauseButton.onClick.AddListener(OnPauseClicked);
            }

            if (menuButton != null)
            {
                menuButton.onClick.AddListener(OnMenuClicked);
            }

            UpdateUI();
        }

        /// <summary>
        /// 이벤트 구독
        /// </summary>
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

        /// <summary>
        /// 타일 클릭 처리
        /// </summary>
        public void OnTileClicked(Tile tile)
        {
            if (selectedTile == null)
            {
                // 첫 번째 타일 선택
                selectedTile = tile;
                selectedTile.SetSelected(true);
                Debug.Log($"[GameplayUI] First tile selected: ({tile.x}, {tile.y})");
            }
            else if (selectedTile == tile)
            {
                // 같은 타일 다시 클릭 - 선택 해제
                selectedTile.SetSelected(false);
                selectedTile = null;
                Debug.Log("[GameplayUI] Selection cancelled");
            }
            else
            {
                // 두 번째 타일 선택 - 스왑 시도
                if (board.TrySwapTiles(selectedTile, tile))
                {
                    levelGoal.UseMove();
                    Debug.Log($"[GameplayUI] Swap attempted: ({selectedTile.x}, {selectedTile.y}) <-> ({tile.x}, {tile.y})");
                }

                selectedTile.SetSelected(false);
                selectedTile = null;
            }
        }

        /// <summary>
        /// 타일 매치 이벤트 처리
        /// </summary>
        private void OnTilesMatched(System.Collections.Generic.List<Tile> matchedTiles)
        {
            // 점수 계산
            int matchScore = matchedTiles.Count * 100;
            currentScore += matchScore;

            // 레벨 목표 업데이트
            levelGoal.UpdateProgress(matchedTiles);

            Debug.Log($"[GameplayUI] Tiles matched: {matchedTiles.Count}, Score: +{matchScore}");
        }

        /// <summary>
        /// 보드 변경 이벤트 처리
        /// </summary>
        private void OnBoardChanged()
        {
            UpdateUI();
        }

        /// <summary>
        /// 진행도 변경 이벤트 처리
        /// </summary>
        private void OnProgressChanged(int newProgress)
        {
            UpdateUI();
        }

        /// <summary>
        /// 이동 변경 이벤트 처리
        /// </summary>
        private void OnMovesChanged(int movesUsed, int moveLimit)
        {
            UpdateUI();
        }

        /// <summary>
        /// 목표 달성 이벤트 처리
        /// </summary>
        private void OnGoalAchieved()
        {
            Debug.Log("[GameplayUI] Goal achieved!");
            
            // 별 계산
            int stars = levelGoal.CalculateStars();
            
            // 게임 완료
            gameManager.CompleteLevel(gameManager.UserProgress.currentLevel, stars);
            
            // 결과 화면으로 전환
            ShowLevelCompleteScreen(stars);
        }

        /// <summary>
        /// 이동 초과 이벤트 처리
        /// </summary>
        private void OnMovesExceeded()
        {
            Debug.LogWarning("[GameplayUI] Moves exceeded!");
            
            // 게임 실패
            gameManager.FailLevel(gameManager.UserProgress.currentLevel);
            
            // 실패 화면 표시
            ShowLevelFailedScreen();
        }

        /// <summary>
        /// UI 업데이트
        /// </summary>
        private void UpdateUI()
        {
            if (scoreText != null)
            {
                scoreText.text = $"Score: {currentScore}";
            }

            if (levelGoal != null)
            {
                if (movesText != null)
                {
                    movesText.text = $"Moves: {levelGoal.GetRemainingMoves()}";
                }

                if (goalText != null)
                {
                    goalText.text = $"{levelGoal.GetGoalDescription()}\n{levelGoal.GetProgressDescription()}";
                }

                if (progressSlider != null)
                {
                    progressSlider.value = levelGoal.GetProgressPercentage() / 100f;
                }
            }
        }

        /// <summary>
        /// 일시정지 버튼 클릭
        /// </summary>
        private void OnPauseClicked()
        {
            gameManager.PauseGame();
            ShowPauseMenu();
            Debug.Log("[GameplayUI] Pause button clicked");
        }

        /// <summary>
        /// 메뉴 버튼 클릭
        /// </summary>
        private void OnMenuClicked()
        {
            gameManager.ReturnToMenu();
            Debug.Log("[GameplayUI] Menu button clicked");
        }

        /// <summary>
        /// 레벨 완료 화면 표시
        /// </summary>
        private void ShowLevelCompleteScreen(int stars)
        {
            // TODO: 레벨 완료 UI 구현
            Debug.Log($"[GameplayUI] Level complete with {stars} stars!");
        }

        /// <summary>
        /// 레벨 실패 화면 표시
        /// </summary>
        private void ShowLevelFailedScreen()
        {
            // TODO: 레벨 실패 UI 구현
            Debug.Log("[GameplayUI] Level failed!");
        }

        /// <summary>
        /// 일시정지 메뉴 표시
        /// </summary>
        private void ShowPauseMenu()
        {
            // TODO: 일시정지 메뉴 UI 구현
            Debug.Log("[GameplayUI] Pause menu shown");
        }

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
