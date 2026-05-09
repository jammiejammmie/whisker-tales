using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WhiskerTales.Bootstrap;
using WhiskerTales.Core;
using WhiskerTales.Puzzle;
using WhiskerTales.Utilities;

namespace WhiskerTales.UI
{
    /// <summary>
    /// 매치-3 게임플레이 화면 (Stage 4 §4-3).
    /// 상단 배경 + 디톡스 카피, 좌측 Level/Moves/Goal HUD, 우측 BoosterPanel,
    /// 매치 발생 시 피드백 텍스트("나이스!", "COMBO x{N}"),
    /// 레벨 클리어 시 LevelClearPanel 표시.
    /// </summary>
    public class GameplayUI : MonoBehaviour
    {
        [Header("HUD (left)")]
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI movesText;
        [SerializeField] private TextMeshProUGUI goalText;
        [SerializeField] private Slider progressSlider;

        [Header("Top Bar")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TextMeshProUGUI detoxCopyText;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button menuButton;

        [Header("Feedback")]
        [SerializeField] private TextMeshProUGUI feedbackText;
        [SerializeField] private TextMeshProUGUI comboText;
        [SerializeField] private float feedbackHoldSeconds = 1.2f;

        [Header("Reward Per Level")]
        [SerializeField] private int defaultCoinReward = Constants.COIN_PER_LEVEL_CLEAR;

        [Header("Sub Panels")]
        [SerializeField] private LevelClearPanel levelClearPanel;
        [SerializeField] private GameObject levelFailPanel;

        private Board board;
        private LevelGoal levelGoal;
        private GameManager gameManager;
        private int currentScore;
        private int comboCounter;
        private float lastMatchTime;
        private const float COMBO_RESET_SECONDS = 1.5f;

        private static readonly string[] FeedbackKo =
        {
            "나이스!", "최고예요!", "고양이가 좋아해요!", "멋져요!", "환상적이에요!",
        };
        private static readonly string[] FeedbackEn =
        {
            "Nice!", "Awesome!", "The cats love it!", "Splendid!", "Fantastic!",
        };

        private void Start()
        {
            board = FindObjectOfType<Board>();
            levelGoal = FindObjectOfType<LevelGoal>();
            gameManager = GameManager.Instance;
            InitializeUI();
            SubscribeToEvents();
            ApplyBackground();
            ApplyDetoxCopy();
            TryShowTutorial();
        }

        private void OnEnable()
        {
            // Start만으로는 패널이 다시 활성화될 때 튜토리얼 트리거가 안 걸려서 OnEnable에서도 호출.
            // PlayerPrefs gate로 중복 노출 방지됨.
            TryShowTutorial();
        }

        private void TryShowTutorial()
        {
            if (gameManager == null) return;
            int level = gameManager.UserProgress?.currentLevel ?? 1;
            AppBootstrap boot = AppBootstrap.Instance;
            if (boot == null || boot.Tutorial == null) return;
            boot.Tutorial.TryShowForLevel(level);
        }

        private void InitializeUI()
        {
            if (levelText != null && gameManager != null)
            {
                levelText.text = $"Level {gameManager.UserProgress.currentLevel}";
            }
            if (pauseButton != null) pauseButton.onClick.AddListener(OnPauseClicked);
            if (menuButton != null) menuButton.onClick.AddListener(OnMenuClicked);
            if (feedbackText != null) feedbackText.text = string.Empty;
            if (comboText != null) comboText.text = string.Empty;
            if (levelFailPanel != null) levelFailPanel.SetActive(false);
            UpdateUI();
        }

        private void SubscribeToEvents()
        {
            if (board != null)
            {
                board.OnMatchFound += OnMatchFound;
            }
            if (levelGoal != null)
            {
                levelGoal.OnProgressChanged += OnProgressChanged;
                levelGoal.OnMovesChanged += OnMovesChanged;
                levelGoal.OnGoalAchieved += OnGoalAchieved;
                levelGoal.OnMovesExceeded += OnMovesExceeded;
            }
        }

        private void ApplyBackground()
        {
            if (backgroundImage == null || CafeRestorationManager.instance == null) return;
            Sprite sp = CafeRestorationManager.instance.GetCurrentBackground();
            if (sp != null) backgroundImage.sprite = sp;
        }

        private void ApplyDetoxCopy()
        {
            if (detoxCopyText == null) return;
            var entry = DailyCopy.GetToday();
            detoxCopyText.text = entry.Text;
            detoxCopyText.color = entry.Accent;
        }

        public void UpdateScore(int score)
        {
            currentScore = score;
            if (scoreText != null) scoreText.text = $"Score: {currentScore}";
        }

        private void OnMatchFound(List<TileData> matchedTiles)
        {
            if (matchedTiles == null) return;
            int matchScore = matchedTiles.Count * 100;
            currentScore += matchScore;
            if (scoreText != null) scoreText.text = $"Score: {currentScore}";

            BumpCombo();
            ShowFeedback();
            AudioManager.instance?.PlayMatchSuccess();
        }

        private void BumpCombo()
        {
            if (Time.unscaledTime - lastMatchTime > COMBO_RESET_SECONDS)
            {
                comboCounter = 0;
            }
            comboCounter++;
            lastMatchTime = Time.unscaledTime;

            if (comboText != null)
            {
                if (comboCounter >= 2)
                {
                    comboText.text = $"COMBO x{comboCounter}";
                    StopCoroutine(nameof(FadeComboLater));
                    StartCoroutine(nameof(FadeComboLater));
                }
            }
        }

        private IEnumerator FadeComboLater()
        {
            yield return new WaitForSecondsRealtime(COMBO_RESET_SECONDS);
            if (comboText != null) comboText.text = string.Empty;
        }

        private void ShowFeedback()
        {
            if (feedbackText == null) return;
            string msg = PickFeedback();
            feedbackText.text = msg;
            StopCoroutine(nameof(ClearFeedbackAfter));
            StartCoroutine(nameof(ClearFeedbackAfter));
        }

        private IEnumerator ClearFeedbackAfter()
        {
            yield return new WaitForSecondsRealtime(feedbackHoldSeconds);
            if (feedbackText != null) feedbackText.text = string.Empty;
        }

        private string PickFeedback()
        {
            bool ko = I18nManager.Instance != null && I18nManager.Instance.currentLanguage == SystemLanguage.Korean;
            string[] pool = ko ? FeedbackKo : FeedbackEn;
            return pool[Random.Range(0, pool.Length)];
        }

        private void OnProgressChanged(int newProgress) => UpdateUI();
        private void OnMovesChanged(int movesUsed, int moveLimit) => UpdateUI();

        private void OnGoalAchieved()
        {
            int stars = levelGoal != null ? levelGoal.CalculateStars() : 0;
            int reward = defaultCoinReward * Mathf.Max(1, stars);

            gameManager?.CompleteLevel(gameManager.UserProgress.currentLevel, stars);

            if (levelClearPanel != null)
            {
                levelClearPanel.Show(stars, reward);
            }
            Debug.Log($"[GameplayUI] Level complete with {stars} stars, +{reward} coins");
        }

        private void OnMovesExceeded()
        {
            gameManager?.FailLevel(gameManager.UserProgress.currentLevel);
            if (levelFailPanel != null) levelFailPanel.SetActive(true);
            Debug.Log("[GameplayUI] Level failed!");
        }

        private void UpdateUI()
        {
            if (scoreText != null) scoreText.text = $"Score: {currentScore}";
            if (levelGoal != null)
            {
                if (movesText != null) movesText.text = $"Moves: {levelGoal.GetRemainingMoves()}";
                if (goalText != null) goalText.text = $"{levelGoal.GetGoalDescription()}\n{levelGoal.GetProgressDescription()}";
                if (progressSlider != null) progressSlider.value = levelGoal.GetProgressPercentage() / 100f;
            }
        }

        private void OnPauseClicked() => gameManager?.PauseGame();
        private void OnMenuClicked() => gameManager?.ReturnToMenu();

        private void OnDestroy()
        {
            if (board != null)
            {
                board.OnMatchFound -= OnMatchFound;
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
