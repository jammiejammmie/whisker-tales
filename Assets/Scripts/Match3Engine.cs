using UnityEngine;
using System.Collections.Generic;
using WhiskerTales.Puzzle;
using WhiskerTales.Core;

namespace WhiskerTales.Game
{
    /// <summary>
    /// 매치-3 게임 엔진 - Board.cs와 통합된 래퍼
    /// 게임플레이 로직, 점수 계산, 특수 아이템 처리
    /// </summary>
    public class Match3Engine : MonoBehaviour
    {
        [SerializeField] private Board board;
        [SerializeField] private LevelGoal levelGoal;
        [SerializeField] private GameplayUI gameplayUI;

        private bool isProcessing = false;
        private int currentScore = 0;

        // 이벤트
        public event System.Action<int> OnScoreChanged;
        public event System.Action<List<Tile>> OnTilesMatched;
        public event System.Action OnGameOver;

        private void Start()
        {
            if (board == null)
                board = GetComponent<Board>();

            if (levelGoal == null)
                levelGoal = GetComponent<LevelGoal>();

            if (gameplayUI == null)
                gameplayUI = GetComponent<GameplayUI>();

            InitializeEngine();
        }

        /// <summary>
        /// 엔진 초기화
        /// </summary>
        private void InitializeEngine()
        {
            currentScore = 0;
            isProcessing = false;

            // 보드 초기화
            if (board != null)
            {
                board.Initialize();
            }

            Debug.Log("[Match3Engine] 엔진 초기화 완료");
        }

        /// <summary>
        /// 타일 스왑 시도
        /// </summary>
        public bool TrySwapTiles(Vector2Int pos1, Vector2Int pos2)
        {
            if (isProcessing || board == null)
                return false;

            // 보드에서 타일 스왑
            if (board.TrySwapTiles(pos1, pos2))
            {
                levelGoal?.UseMove();
                StartCoroutine(ProcessMatches());
                return true;
            }

            return false;
        }

        /// <summary>
        /// 매치 처리 코루틴
        /// </summary>
        private System.Collections.IEnumerator ProcessMatches()
        {
            isProcessing = true;

            while (true)
            {
                // 매치 감지
                List<Tile> matchedTiles = board.FindMatches();

                if (matchedTiles.Count == 0)
                    break;

                // 점수 계산 및 업데이트
                int matchScore = CalculateScore(matchedTiles);
                AddScore(matchScore);

                // 레벨 목표 업데이트
                levelGoal?.UpdateProgress(matchedTiles);

                // 타일 제거 애니메이션
                yield return StartCoroutine(board.RemoveTilesWithAnimation(matchedTiles));

                // 중력 적용
                yield return StartCoroutine(board.ApplyGravity());

                // 빈 공간 채우기
                yield return StartCoroutine(board.FillEmptySpaces());

                // 이벤트 발생
                OnTilesMatched?.Invoke(matchedTiles);
            }

            isProcessing = false;

            // 게임 오버 확인
            if (levelGoal != null && levelGoal.IsMovesExceeded() && !levelGoal.IsGoalAchieved())
            {
                OnGameOver?.Invoke();
            }
        }

        /// <summary>
        /// 점수 계산
        /// </summary>
        private int CalculateScore(List<Tile> matchedTiles)
        {
            int score = 0;

            foreach (Tile tile in matchedTiles)
            {
                // 기본 점수: 타일 수 × 100
                score += 100;

                // 특수 아이템 보너스
                if (tile.specialItem != SpecialItemType.None)
                {
                    score += 50; // 특수 아이템 보너스
                }
            }

            // 연쇄 매치 보너스 (매치 수에 따라)
            if (matchedTiles.Count >= 5)
            {
                score += (matchedTiles.Count - 4) * 50; // 5개 이상일 때 추가 보너스
            }

            return score;
        }

        /// <summary>
        /// 점수 추가
        /// </summary>
        public void AddScore(int points)
        {
            currentScore += points;
            OnScoreChanged?.Invoke(currentScore);

            if (gameplayUI != null)
            {
                gameplayUI.UpdateScore(currentScore);
            }

            Debug.Log($"[Match3Engine] 점수 추가: +{points} (총: {currentScore})");
        }

        /// <summary>
        /// 현재 점수
        /// </summary>
        public int GetCurrentScore()
        {
            return currentScore;
        }

        /// <summary>
        /// 게임 상태 확인
        /// </summary>
        public bool IsProcessing()
        {
            return isProcessing;
        }

        /// <summary>
        /// 게임 리셋
        /// </summary>
        public void ResetGame()
        {
            currentScore = 0;
            isProcessing = false;

            if (board != null)
            {
                board.Initialize();
            }

            Debug.Log("[Match3Engine] 게임 리셋");
        }

        /// <summary>
        /// 보드 조회
        /// </summary>
        public Board GetBoard()
        {
            return board;
        }

        /// <summary>
        /// 레벨 목표 조회
        /// </summary>
        public LevelGoal GetLevelGoal()
        {
            return levelGoal;
        }
    }
}
