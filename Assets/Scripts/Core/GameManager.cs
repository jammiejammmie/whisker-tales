using UnityEngine;
using WhiskerTales.Utilities;

namespace WhiskerTales.Core
{
    /// <summary>
    /// 단일 씬 + 패널 전환 구조에서 화면 식별자.
    /// BottomNav가 발행하는 SwitchTo 이벤트와 GameManager.RequestNavigation을 통해 사용.
    /// </summary>
    public enum NavigationTarget
    {
        Title,      // 메인/타이틀 화면 (Home 탭)
        Shop,
        CatRoom,
        Gallery,
        Friends,
        Cafe,       // 카페 운영 모드
        Gameplay,   // 매치-3 보드
        Settings,
        Arcade,     // 미니게임 오락실 (§Phase B)
        MeditationGarden, // 명상 정원 (§3-2 Phase B-3)
        PhotoStudio,      // 고양이 포토 스튜디오 (§3-4 Phase B-3)
        ShareCard,        // 레퍼럴 공유 카드 (Phase C-2)
        Diagnostics       // 품질 진단 (Stage 4B 도구)
    }

    /// <summary>
    /// 게임 전체 상태 및 생명주기 관리
    /// 싱글톤 패턴 사용
    /// </summary>
    [DefaultExecutionOrder(-150)]
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        private GameState currentState = GameState.MainMenu;
        private UserProgress userProgress;
        private DataManager dataManager;

        public GameState CurrentState => currentState;
        public UserProgress UserProgress => userProgress;

        // 이벤트
        public delegate void GameStateChangedHandler(GameState newState);
        public event GameStateChangedHandler OnGameStateChanged;

        public delegate void LevelCompleteHandler(int levelId, int stars);
        public event LevelCompleteHandler OnLevelComplete;

        public delegate void LevelFailHandler(int levelId);
        public event LevelFailHandler OnLevelFail;

        public delegate void NavigationRequestedHandler(NavigationTarget target);
        public event NavigationRequestedHandler OnNavigationRequested;

        private void Awake()
        {
            // 싱글톤 패턴 구현
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 초기화
            dataManager = GetComponent<DataManager>();
            if (dataManager == null)
            {
                dataManager = gameObject.AddComponent<DataManager>();
            }

            LoadGame();
        }

        private void Start()
        {
            Debug.Log("[GameManager] Game initialized");
        }

        /// <summary>
        /// 게임 상태 변경
        /// </summary>
        public void SetGameState(GameState newState)
        {
            if (currentState == newState)
                return;

            currentState = newState;
            Debug.Log($"[GameManager] State changed to: {newState}");
            OnGameStateChanged?.Invoke(newState);
        }

        /// <summary>
        /// 레벨 시작
        /// </summary>
        public void StartLevel(int levelId)
        {
            if (levelId < 1 || levelId > Constants.TOTAL_LEVELS)
            {
                Debug.LogError($"[GameManager] Invalid level ID: {levelId}");
                return;
            }

            userProgress.currentLevel = levelId;
            SetGameState(GameState.Playing);
            Debug.Log($"[GameManager] Level {levelId} started");
        }

        /// <summary>
        /// 레벨 완료
        /// </summary>
        public void CompleteLevel(int levelId, int stars)
        {
            if (stars < 0 || stars > 3)
            {
                Debug.LogError($"[GameManager] Invalid stars: {stars}");
                return;
            }

            // 진행도 업데이트
            if (levelId == userProgress.currentLevel)
            {
                userProgress.completedLevels++;
                userProgress.stars += stars;

                // 다음 레벨 해금
                if (levelId < Constants.TOTAL_LEVELS)
                {
                    userProgress.currentLevel = levelId + 1;
                }
            }

            SetGameState(GameState.LevelComplete);
            OnLevelComplete?.Invoke(levelId, stars);
            Debug.Log($"[GameManager] Level {levelId} completed with {stars} stars");

            SaveGame();
        }

        /// <summary>
        /// 레벨 실패
        /// </summary>
        public void FailLevel(int levelId)
        {
            SetGameState(GameState.LevelFailed);
            OnLevelFail?.Invoke(levelId);
            Debug.Log($"[GameManager] Level {levelId} failed");
        }

        /// <summary>
        /// 게임 일시 정지
        /// </summary>
        public void PauseGame()
        {
            if (currentState == GameState.Playing)
            {
                SetGameState(GameState.Paused);
                Time.timeScale = 0f;
                Debug.Log("[GameManager] Game paused");
            }
        }

        /// <summary>
        /// 게임 재개
        /// </summary>
        public void ResumeGame()
        {
            if (currentState == GameState.Paused)
            {
                SetGameState(GameState.Playing);
                Time.timeScale = 1f;
                Debug.Log("[GameManager] Game resumed");
            }
        }

        /// <summary>
        /// 메인/타이틀 화면으로 복귀 (단일 씬 패널 전환).
        /// </summary>
        public void ReturnToMenu()
        {
            Time.timeScale = 1f;
            SetGameState(GameState.MainMenu);
            SaveGame();
            RequestNavigation(NavigationTarget.Title);
            Debug.Log("[GameManager] Returned to main menu");
        }

        /// <summary>
        /// 카페 화면으로 이동 (단일 씬 패널 전환).
        /// </summary>
        public void GoToCafe()
        {
            SetGameState(GameState.Cafe);
            RequestNavigation(NavigationTarget.Cafe);
            Debug.Log("[GameManager] Navigated to cafe");
        }

        /// <summary>
        /// 화면 전환 요청. 구독자(BottomNav 등)가 패널 활성화/비활성화 처리.
        /// </summary>
        public void RequestNavigation(NavigationTarget target)
        {
            OnNavigationRequested?.Invoke(target);
        }

        /// <summary>
        /// 게임 저장
        /// </summary>
        public void SaveGame()
        {
            if (dataManager != null)
            {
                dataManager.SaveUserProgress(userProgress);
                Debug.Log("[GameManager] Game saved");
            }
        }

        /// <summary>
        /// 게임 로드
        /// </summary>
        public void LoadGame()
        {
            if (dataManager != null)
            {
                userProgress = dataManager.LoadUserProgress();
                if (userProgress == null)
                {
                    userProgress = new UserProgress();
                    Debug.Log("[GameManager] New game started");
                }
                else
                {
                    Debug.Log("[GameManager] Game loaded");
                }
            }
        }

        /// <summary>
        /// 게임 리셋 (디버그용)
        /// </summary>
        public void ResetGame()
        {
            userProgress = new UserProgress();
            SaveGame();
            Debug.Log("[GameManager] Game reset");
        }

        /// <summary>
        /// 동전 추가
        /// </summary>
        public void AddCoins(int amount)
        {
            userProgress.coins += amount;
            Debug.Log($"[GameManager] Added {amount} coins. Total: {userProgress.coins}");
        }

        /// <summary>
        /// 동전 소비
        /// </summary>
        public bool SpendCoins(int amount)
        {
            if (userProgress.coins >= amount)
            {
                userProgress.coins -= amount;
                Debug.Log($"[GameManager] Spent {amount} coins. Remaining: {userProgress.coins}");
                return true;
            }
            else
            {
                Debug.LogWarning($"[GameManager] Not enough coins. Required: {amount}, Have: {userProgress.coins}");
                return false;
            }
        }

        /// <summary>
        /// 별 추가
        /// </summary>
        public void AddStars(int amount)
        {
            userProgress.stars += amount;
            Debug.Log($"[GameManager] Added {amount} stars. Total: {userProgress.stars}");
        }

        /// <summary>
        /// 별 소비
        /// </summary>
        public bool SpendStars(int amount)
        {
            if (userProgress.stars >= amount)
            {
                userProgress.stars -= amount;
                Debug.Log($"[GameManager] Spent {amount} stars. Remaining: {userProgress.stars}");
                return true;
            }
            else
            {
                Debug.LogWarning($"[GameManager] Not enough stars. Required: {amount}, Have: {userProgress.stars}");
                return false;
            }
        }

        /// <summary>
        /// 보석 추가
        /// </summary>
        public void AddGems(int amount)
        {
            userProgress.gems += amount;
            Debug.Log($"[GameManager] Added {amount} gems. Total: {userProgress.gems}");
        }

        /// <summary>
        /// 보석 소비
        /// </summary>
        public bool SpendGems(int amount)
        {
            if (userProgress.gems >= amount)
            {
                userProgress.gems -= amount;
                Debug.Log($"[GameManager] Spent {amount} gems. Remaining: {userProgress.gems}");
                return true;
            }
            else
            {
                Debug.LogWarning($"[GameManager] Not enough gems. Required: {amount}, Have: {userProgress.gems}");
                return false;
            }
        }

        /// <summary>
        /// 생명 추가
        /// </summary>
        public void AddLives(int amount)
        {
            userProgress.lives = Mathf.Min(userProgress.lives + amount, Constants.MAX_LIVES);
            Debug.Log($"[GameManager] Added {amount} lives. Total: {userProgress.lives}");
        }

        /// <summary>
        /// 생명 소비
        /// </summary>
        public bool SpendLives(int amount)
        {
            if (userProgress.lives >= amount)
            {
                userProgress.lives -= amount;
                Debug.Log($"[GameManager] Spent {amount} lives. Remaining: {userProgress.lives}");
                return true;
            }
            else
            {
                Debug.LogWarning($"[GameManager] Not enough lives. Required: {amount}, Have: {userProgress.lives}");
                return false;
            }
        }

        private void OnDestroy()
        {
            SaveGame();
        }
    }
}
