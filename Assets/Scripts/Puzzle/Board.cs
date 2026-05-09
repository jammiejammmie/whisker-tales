using System.Collections.Generic;
using UnityEngine;

namespace WhiskerTales.Puzzle
{
    /// <summary>
    /// 매치-3 게임의 8×8 보드를 관리하는 MonoBehaviour
    /// Stage 2: LevelGoal과 연동되어 이동 소진이 아닌 목표 달성 기반으로 완료 판정
    /// LevelGoal이 주어지지 않으면 레거시 이동 횟수 기반 경로로 폴백
    /// </summary>
    public class Board : MonoBehaviour
    {
        public static Board instance { get; private set; }

        private const int BOARD_SIZE = 8;

        private TileData[,] board;
        private LevelGoal levelGoal;

        // 레거시 폴백 상태
        private int moveLimit;
        private int movesUsed;
        private int starsEarned;
        private bool isLevelComplete;
        private bool isAnimating;

        public delegate void OnMatchFoundDelegate(List<TileData> matches);
        public event OnMatchFoundDelegate OnMatchFound;

        public delegate void OnLevelCompleteDelegate(int stars);
        public event OnLevelCompleteDelegate OnLevelComplete;

        public delegate void OnLevelFailedDelegate();
        public event OnLevelFailedDelegate OnLevelFailed;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
        }

        /// <summary>
        /// Stage 2: Level 데이터 + LevelGoal 컴포넌트 기반 초기화
        /// LevelGoal이 null이 아니면 이 보드의 완료/실패 판정은 LevelGoal에 위임됨
        /// </summary>
        public void Initialize(Level levelData, LevelGoal goal)
        {
            board = new TileData[BOARD_SIZE, BOARD_SIZE];
            levelGoal = goal;
            moveLimit = levelData != null ? levelData.moveLimit : 25;
            movesUsed = 0;
            starsEarned = 0;
            isLevelComplete = false;
            isAnimating = false;

            if (levelGoal != null && levelData != null)
            {
                levelGoal.Initialize(levelData);
            }

            SpawnTiles();

            if (levelData != null)
                Debug.Log($"[Board] Initialized goal={levelData.goalType} value={levelData.goalValue} moves={moveLimit}");
            else
                Debug.Log($"[Board] Initialized (no level data) moves={moveLimit}");
        }

        /// <summary>
        /// 레거시: 이동 횟수만 받는 초기화 (LevelGoal 미사용 경로)
        /// </summary>
        public void Initialize(int moveLimitValue = 25)
        {
            board = new TileData[BOARD_SIZE, BOARD_SIZE];
            levelGoal = null;
            moveLimit = moveLimitValue;
            movesUsed = 0;
            starsEarned = 0;
            isLevelComplete = false;
            isAnimating = false;

            SpawnTiles();
            Debug.Log($"[Board] Initialized (legacy) move limit: {moveLimit}");
        }

        public void SpawnTiles()
        {
            for (int y = 0; y < BOARD_SIZE; y++)
            {
                for (int x = 0; x < BOARD_SIZE; x++)
                {
                    TileType randomType = GetRandomTileType();
                    board[y, x] = new TileData(x, y, randomType);
                }
            }

            RemoveInitialMatches();
            Debug.Log("[Board] Tiles spawned");
        }

        /// <summary>
        /// 두 타일 스왑 시도. 매치가 발생하면 보드를 정리하고 진행도/이동을 LevelGoal에 통보
        /// </summary>
        public bool TrySwapTiles(int x1, int y1, int x2, int y2)
        {
            if (!IsValidPosition(x1, y1) || !IsValidPosition(x2, y2))
            {
                Debug.LogWarning($"[Board] Invalid position: ({x1},{y1}) or ({x2},{y2})");
                return false;
            }

            TileData tileA = board[y1, x1];
            TileData tileB = board[y2, x2];

            if (!MatchLogic.IsValidSwap(tileA, tileB))
            {
                Debug.LogWarning($"[Board] Invalid swap: ({x1},{y1}) and ({x2},{y2})");
                return false;
            }

            SwapTiles(tileA, tileB);

            List<List<TileData>> allMatches = MatchLogic.FindAllMatches(board);

            if (allMatches.Count > 0)
            {
                List<TileData> flatRemoved = new List<TileData>();
                foreach (List<TileData> matches in allMatches)
                {
                    OnMatchFound?.Invoke(matches);

                    SpecialItemType special = MatchLogic.GetSpecialItemType(matches);
                    if (special != SpecialItemType.None)
                        Debug.Log($"[Board] Special item created: {special}");

                    flatRemoved.AddRange(matches);
                }

                RemoveMatches(allMatches);
                ApplyGravity();
                FillEmpty();

                if (levelGoal != null)
                {
                    // Stage 2: 목표 기반 완료/실패
                    levelGoal.UpdateProgress(flatRemoved);
                    levelGoal.UseMove();

                    if (levelGoal.IsGoalAchieved())
                    {
                        isLevelComplete = true;
                        starsEarned = levelGoal.CalculateStars();
                        OnLevelComplete?.Invoke(starsEarned);
                        Debug.Log($"[Board] Goal achieved. Stars: {starsEarned}");
                    }
                    else if (levelGoal.IsMovesExceeded())
                    {
                        OnLevelFailed?.Invoke();
                        Debug.Log("[Board] Moves exhausted before goal achieved");
                    }
                }
                else
                {
                    // 레거시: 이동 소진 = 클리어
                    movesUsed++;
                    if (movesUsed >= moveLimit)
                    {
                        isLevelComplete = true;
                        starsEarned = CalculateStarsLegacy();
                        OnLevelComplete?.Invoke(starsEarned);
                        Debug.Log($"[Board] Level complete (legacy). Stars: {starsEarned}");
                    }
                }

                return true;
            }
            else
            {
                // 매치 없음 → 스왑 취소 (이동 횟수도 차감하지 않음)
                SwapTiles(tileA, tileB);
                Debug.Log("[Board] No match, swap cancelled");
                return false;
            }
        }

        public List<TileData> FindMatches()
        {
            List<List<TileData>> allMatches = MatchLogic.FindAllMatches(board);
            List<TileData> result = new List<TileData>();
            foreach (List<TileData> matches in allMatches)
                result.AddRange(matches);
            return result;
        }

        public void RemoveMatches(List<List<TileData>> allMatches)
        {
            if (allMatches == null || allMatches.Count == 0) return;
            foreach (List<TileData> matches in allMatches)
            {
                foreach (TileData tile in matches)
                {
                    tile.isMatched = true;
                    board[tile.y, tile.x] = null;
                }
            }
            Debug.Log($"[Board] {allMatches.Count} match groups removed");
        }

        public void ApplyGravity()
        {
            for (int x = 0; x < BOARD_SIZE; x++)
            {
                int writePos = BOARD_SIZE - 1;
                for (int y = BOARD_SIZE - 1; y >= 0; y--)
                {
                    if (board[y, x] != null)
                    {
                        if (y != writePos)
                        {
                            board[writePos, x] = board[y, x];
                            board[writePos, x].y = writePos;
                            board[y, x] = null;
                        }
                        writePos--;
                    }
                }
            }
            Debug.Log("[Board] Gravity applied");
        }

        public void FillEmpty()
        {
            for (int x = 0; x < BOARD_SIZE; x++)
            {
                for (int y = 0; y < BOARD_SIZE; y++)
                {
                    if (board[y, x] == null)
                    {
                        TileType randomType = GetRandomTileType();
                        board[y, x] = new TileData(x, y, randomType);
                    }
                }
            }
            Debug.Log("[Board] Empty spaces filled");
        }

        public void PrintBoardState()
        {
            string s = "[Board State]\n";
            for (int y = 0; y < BOARD_SIZE; y++)
            {
                for (int x = 0; x < BOARD_SIZE; x++)
                {
                    s += (board[y, x] != null) ? $"{(int)board[y, x].type} " : "X ";
                }
                s += "\n";
            }
            Debug.Log(s);
        }

        public int GetMovesUsed()
        {
            return levelGoal != null ? levelGoal.movesUsed : movesUsed;
        }

        public int GetMovesRemaining()
        {
            if (levelGoal != null) return levelGoal.GetRemainingMoves();
            return Mathf.Max(0, moveLimit - movesUsed);
        }

        public int GetStarsEarned() { return starsEarned; }
        public bool IsAnimating() { return isAnimating; }
        public bool IsLevelComplete() { return isLevelComplete; }

        public TileData GetTile(int x, int y)
        {
            if (IsValidPosition(x, y)) return board[y, x];
            return null;
        }

        // ===== Private =====

        private void SwapTiles(TileData tileA, TileData tileB)
        {
            int tempX = tileA.x;
            int tempY = tileA.y;

            tileA.x = tileB.x;
            tileA.y = tileB.y;
            tileB.x = tempX;
            tileB.y = tempY;

            board[tileA.y, tileA.x] = tileA;
            board[tileB.y, tileB.x] = tileB;
        }

        private bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < BOARD_SIZE && y >= 0 && y < BOARD_SIZE;
        }

        private TileType GetRandomTileType()
        {
            int randomIndex = Random.Range(0, 6);
            return (TileType)randomIndex;
        }

        /// <summary>
        /// 보드 생성 직후 우연히 매치가 형성된 타일을 인접 타일과 비교해 비매치 타입으로 즉시 교체.
        /// 기존 RemoveMatches→Gravity→FillEmpty 방식은 새로 채운 타일이 또 매치를 만들 수 있어 수렴 보장이 안 됐음.
        /// 이 방식은 매 iter마다 매치 타일만 교체하므로 빠르게 수렴.
        /// </summary>
        private void RemoveInitialMatches()
        {
            const int maxIterations = 100;
            int iter = 0;
            while (iter < maxIterations)
            {
                List<List<TileData>> matches = MatchLogic.FindAllMatches(board);
                if (matches == null || matches.Count == 0) break;

                HashSet<(int x, int y)> replaced = new HashSet<(int, int)>();
                foreach (List<TileData> group in matches)
                {
                    foreach (TileData tile in group)
                    {
                        if (tile == null) continue;
                        if (!replaced.Add((tile.x, tile.y))) continue;
                        tile.type = PickNonMatchingType(tile.x, tile.y);
                    }
                }
                iter++;
            }

            if (iter >= maxIterations)
                Debug.LogWarning($"[Board] Initial matches not fully resolved after {maxIterations} iterations");
            else if (iter > 0)
                Debug.Log($"[Board] Initial matches resolved in {iter} iteration(s)");
        }

        /// <summary>
        /// (x,y) 위치에 놓아도 가로/세로 3-매치를 만들지 않는 타일 타입 선택.
        /// 6개 타입 × 시도 4회 안에 거의 항상 비매치 후보를 찾을 수 있음 (확률적으로).
        /// </summary>
        private TileType PickNonMatchingType(int x, int y)
        {
            const int numTypes = 6;
            for (int attempt = 0; attempt < numTypes * 4; attempt++)
            {
                TileType candidate = (TileType)Random.Range(0, numTypes);
                if (!WouldCauseMatchAt(x, y, candidate)) return candidate;
            }
            return GetRandomTileType();
        }

        /// <summary>
        /// (x,y)에 type을 놓으면 가로/세로 3-매치(또는 그 일부)가 생기는지 검사.
        /// 좌2/우2/좌1우1, 위2/아래2/위1아래1 패턴 6가지를 모두 본다.
        /// </summary>
        private bool WouldCauseMatchAt(int x, int y, TileType type)
        {
            // 가로 좌측 2칸
            if (x >= 2 && SameType(board[y, x - 1], type) && SameType(board[y, x - 2], type)) return true;
            // 가로 우측 2칸
            if (x <= BOARD_SIZE - 3 && SameType(board[y, x + 1], type) && SameType(board[y, x + 2], type)) return true;
            // 가로 좌1 + 우1 (가운데)
            if (x >= 1 && x <= BOARD_SIZE - 2 && SameType(board[y, x - 1], type) && SameType(board[y, x + 1], type)) return true;
            // 세로 위쪽 2칸
            if (y >= 2 && SameType(board[y - 1, x], type) && SameType(board[y - 2, x], type)) return true;
            // 세로 아래쪽 2칸
            if (y <= BOARD_SIZE - 3 && SameType(board[y + 1, x], type) && SameType(board[y + 2, x], type)) return true;
            // 세로 위1 + 아래1 (가운데)
            if (y >= 1 && y <= BOARD_SIZE - 2 && SameType(board[y - 1, x], type) && SameType(board[y + 1, x], type)) return true;
            return false;
        }

        private static bool SameType(TileData t, TileType type) => t != null && t.type == type;

        // 레거시 별 계산: 이동 횟수 비율
        private int CalculateStarsLegacy()
        {
            int stars = 1;
            if (movesUsed <= moveLimit * 0.5f) stars = 3;
            else if (movesUsed <= moveLimit * 0.75f) stars = 2;
            return stars;
        }
    }
}
