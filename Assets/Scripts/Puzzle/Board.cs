using System.Collections.Generic;
using UnityEngine;
using WhiskerTales.Core;

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

        private const int BOARD_SIZE = GameConstants.Board.Size;

        private TileData[,] board;
        private LevelGoal levelGoal;

        // 레거시 폴백 상태
        private int moveLimit;
        private int movesUsed;
        private int starsEarned;
        private bool isLevelComplete;
        private bool isAnimating;
        private int currentLevelId;

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
            currentLevelId = levelData != null ? levelData.levelId : 0;
            moveLimit = levelData != null ? levelData.moveLimit : GameConstants.Board.DefaultMoveLimit;
            movesUsed = 0;
            starsEarned = 0;
            isLevelComplete = false;
            isAnimating = false;

            if (levelGoal != null && levelData != null)
            {
                levelGoal.Initialize(levelData);
            }

            SpawnTiles();
            GameEvents.RaiseLevelStarted(currentLevelId);

            if (levelData != null)
                DebugLogger.Info(LogCategory.Puzzle, $"[Board] Initialized goal={levelData.goalType} value={levelData.goalValue} moves={moveLimit}", this);
            else
                DebugLogger.Info(LogCategory.Puzzle, $"[Board] Initialized (no level data) moves={moveLimit}", this);
        }

        /// <summary>
        /// 레거시: 이동 횟수만 받는 초기화 (LevelGoal 미사용 경로)
        /// </summary>
        public void Initialize(int moveLimitValue = 25)
        {
            board = new TileData[BOARD_SIZE, BOARD_SIZE];
            levelGoal = null;
            currentLevelId = 0;
            moveLimit = moveLimitValue;
            movesUsed = 0;
            starsEarned = 0;
            isLevelComplete = false;
            isAnimating = false;

            SpawnTiles();
            DebugLogger.Info(LogCategory.Puzzle, $"[Board] Initialized (legacy) move limit: {moveLimit}", this);
            GameEvents.RaiseLevelStarted(currentLevelId);
        }

        public void SpawnTiles()
        {
            if (board == null || board.GetLength(0) != BOARD_SIZE || board.GetLength(1) != BOARD_SIZE)
            {
                DebugLogger.Warning(LogCategory.Puzzle, "[Board] SpawnTiles called before board allocation; allocating now", this);
                board = new TileData[BOARD_SIZE, BOARD_SIZE];
            }
            for (int y = 0; y < BOARD_SIZE; y++)
            {
                for (int x = 0; x < BOARD_SIZE; x++)
                {
                    TileType randomType = GetRandomTileType();
                    board[y, x] = new TileData(x, y, randomType);
                }
            }

            RemoveInitialMatches();
            DebugLogger.Info(LogCategory.Puzzle, "[Board] Tiles spawned", this);
        }

        private const int MAX_CASCADE_ITERATIONS = GameConstants.Board.MaxCascadeIterations;

        /// <summary>
        /// 두 타일 스왑 시도. 매치가 발생하면 캐스케이드 루프(최대 MAX_CASCADE_ITERATIONS)로
        /// FillEmpty 후에도 새로 형성된 매치를 모두 처리. LevelGoal.UseMove()는 단 1회만 호출
        /// (캐스케이드는 같은 한 수의 결과로 취급).
        /// </summary>
        public bool TrySwapTiles(int x1, int y1, int x2, int y2)
        {
            if (!EnsureBoardReady()) return false;
            if (isAnimating)
            {
                DebugLogger.Warning(LogCategory.Puzzle, "[Board] Swap ignored while board is animating", this);
                return false;
            }
            if (!IsValidPosition(x1, y1) || !IsValidPosition(x2, y2))
            {
                DebugLogger.Warning(LogCategory.Puzzle, $"[Board] Invalid position: ({x1},{y1}) or ({x2},{y2})", this);
                return false;
            }

            TileData tileA = GetTileSafe(x1, y1);
            TileData tileB = GetTileSafe(x2, y2);

            if (tileA == null || tileB == null)
            {
                DebugLogger.Warning(LogCategory.Puzzle, $"[Board] Swap failed because one tile is null: A=({x1},{y1}) B=({x2},{y2})", this);
                return false;
            }

            if (!MatchLogic.IsValidSwap(tileA, tileB))
            {
                DebugLogger.Warning(LogCategory.Puzzle, $"[Board] Invalid swap: ({x1},{y1}) and ({x2},{y2})", this);
                return false;
            }

            SwapTiles(tileA, tileB);
            GameEvents.RaiseTileSwapped(x1, y1, x2, y2);

            // 특수 타일 스왑 감지 (스왑 후 매치 0이어도 특수 타일이 있으면 발화)
            bool specialOnA = tileA.specialItem != SpecialItemType.None;
            bool specialOnB = tileB.specialItem != SpecialItemType.None;

            List<List<TileData>> currentMatches = MatchLogic.FindAllMatches(board);
            bool hasInitialMatch = currentMatches.Count > 0;

            if (!hasInitialMatch && !specialOnA && !specialOnB)
            {
                // 매치 없음 + 특수 아님 → 스왑 취소
                SwapTiles(tileA, tileB);
                DebugLogger.Info(LogCategory.Puzzle, "[Board] No match, swap cancelled", this);
                return false;
            }

            HashSet<TileData> swapOrigins = new HashSet<TileData> { tileA, tileB };
            List<TileData> cumulativeRemoved = new List<TileData>();
            HashSet<TileData> pendingSwapActivation = new HashSet<TileData>();

            // 특수-only 스왑: 매치 없는데 특수 타일 swap → 즉시 발화
            if (!hasInitialMatch)
            {
                if (specialOnA && specialOnB)
                {
                    foreach (var t in SpecialItem.ActivateCombo(tileA, tileB, board)) pendingSwapActivation.Add(t);
                    tileA.specialItem = SpecialItemType.None;
                    tileB.specialItem = SpecialItemType.None;
                }
                else
                {
                    TileData spec = specialOnA ? tileA : tileB;
                    TileData partner = specialOnA ? tileB : tileA;
                    ActivateSpecial(spec, partner.type, pendingSwapActivation);
                }
            }

            int cascade = 0;
            while ((currentMatches.Count > 0 || pendingSwapActivation.Count > 0) && cascade < MAX_CASCADE_ITERATIONS)
            {
                GameEvents.RaiseCascadeStarted(cascade + 1);
                HashSet<TileData> removalSet = new HashSet<TileData>();

                // 특수-only 스왑으로 들어온 제거 후보 흡수
                foreach (var t in pendingSwapActivation) removalSet.Add(t);
                pendingSwapActivation.Clear();

                foreach (List<TileData> matchGroup in currentMatches)
                {
                    OnMatchFound?.Invoke(matchGroup);
                    GameEvents.RaiseMatchFound(matchGroup != null ? matchGroup.Count : 0);

                    SpecialItemType produced = MatchLogic.GetSpecialItemType(matchGroup);
                    TileData survivor = (produced != SpecialItemType.None)
                        ? ChooseSurvivor(matchGroup, swapOrigins)
                        : null;

                    if (produced != SpecialItemType.None && survivor != null)
                        DebugLogger.Info(LogCategory.Puzzle, $"[Board] Created {produced} at ({survivor.x},{survivor.y}) (cascade {cascade})", this);
                        GameEvents.RaiseSpecialTileCreated(produced);

                    foreach (TileData tile in matchGroup)
                    {
                        if (tile == survivor) continue;

                        // 매치로 제거되는 타일이 기존 특수 → 체인 발화
                        if (tile.specialItem != SpecialItemType.None)
                            ActivateSpecial(tile, tile.type, removalSet);

                        removalSet.Add(tile);
                    }

                    if (survivor != null)
                    {
                        survivor.specialItem = produced;
                        survivor.isMatched = false;
                    }
                }

                // removalSet 안에 아직 활성화 안 된 특수 (체인 결과로 추가된 것 등) 처리
                ResolveChainedSpecials(removalSet);

                // 보드에서 실제 제거
                foreach (TileData t in removalSet)
                {
                    if (t == null) continue;
                    if (t.x < 0 || t.x >= BOARD_SIZE || t.y < 0 || t.y >= BOARD_SIZE) continue;
                    if (board[t.y, t.x] == t)
                    {
                        t.isMatched = true;
                        board[t.y, t.x] = null;
                    }
                    cumulativeRemoved.Add(t);
                }

                ApplyGravity();
                FillEmpty();
                cascade++;

                // 첫 iter 이후 swapOrigins 의미 없음 (캐스케이드 매치는 중앙 타일을 survivor로)
                swapOrigins.Clear();

                currentMatches = MatchLogic.FindAllMatches(board);
            }

            if (cascade > 1)
                DebugLogger.Info(LogCategory.Puzzle, $"[Board] Cascade resolved in {cascade} iteration(s), {cumulativeRemoved.Count} tiles total", this);
            if (cascade >= MAX_CASCADE_ITERATIONS)
                DebugLogger.Warning(LogCategory.Puzzle, $"[Board] Cascade hit max iterations ({MAX_CASCADE_ITERATIONS})", this);

            GameEvents.RaiseCascadeEnded(cascade);

            if (levelGoal != null)
            {
                // Stage 2: 목표 기반 완료/실패. UseMove는 캐스케이드 횟수와 무관하게 1회.
                levelGoal.UpdateProgress(cumulativeRemoved);
                levelGoal.UseMove();
                GameEvents.RaiseGoalUpdated(levelGoal.currentProgress, levelGoal.goalValue);

                if (levelGoal.IsGoalAchieved())
                {
                    isLevelComplete = true;
                    starsEarned = levelGoal.CalculateStars();
                    OnLevelComplete?.Invoke(starsEarned);
                    GameEvents.RaiseLevelCompleted(currentLevelId, starsEarned);
                    DebugLogger.Info(LogCategory.Puzzle, $"[Board] Goal achieved. Stars: {starsEarned}", this);
                }
                else if (levelGoal.IsMovesExceeded())
                {
                    OnLevelFailed?.Invoke();
                    GameEvents.RaiseLevelFailed(currentLevelId);
                    DebugLogger.Info(LogCategory.Puzzle, "[Board] Moves exhausted before goal achieved", this);
                }
            }
            else
            {
                movesUsed++;
                if (movesUsed >= moveLimit)
                {
                    isLevelComplete = true;
                    starsEarned = CalculateStarsLegacy();
                    OnLevelComplete?.Invoke(starsEarned);
                    GameEvents.RaiseLevelCompleted(currentLevelId, starsEarned);
                    DebugLogger.Info(LogCategory.Puzzle, $"[Board] Level complete (legacy). Stars: {starsEarned}", this);
                }
            }

            return true;
        }

        public List<TileData> FindMatches()
        {
            List<TileData> result = new List<TileData>();
            if (!EnsureBoardReady()) return result;
            List<List<TileData>> allMatches = MatchLogic.FindAllMatches(board);
            foreach (List<TileData> matches in allMatches)
                result.AddRange(matches);
            return result;
        }

        public void RemoveMatches(List<List<TileData>> allMatches)
        {
            if (!EnsureBoardReady()) return;
            if (allMatches == null || allMatches.Count == 0) return;
            foreach (List<TileData> matches in allMatches)
            {
                foreach (TileData tile in matches)
                {
                    if (tile == null || !IsValidPosition(tile.x, tile.y)) continue;
                    tile.isMatched = true;
                    if (board[tile.y, tile.x] == tile) board[tile.y, tile.x] = null;
                }
            }
            DebugLogger.Info(LogCategory.Puzzle, $"[Board] {allMatches.Count} match groups removed", this);
        }

        public void ApplyGravity()
        {
            if (!EnsureBoardReady()) return;
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
            DebugLogger.Verbose(LogCategory.Puzzle, "[Board] Gravity applied", this);
        }

        public void FillEmpty()
        {
            if (!EnsureBoardReady()) return;
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
            DebugLogger.Verbose(LogCategory.Puzzle, "[Board] Empty spaces filled", this);
        }

        public void PrintBoardState()
        {
            if (!EnsureBoardReady()) return;
            string s = "[Board State]\n";
            for (int y = 0; y < BOARD_SIZE; y++)
            {
                for (int x = 0; x < BOARD_SIZE; x++)
                {
                    s += (board[y, x] != null) ? $"{(int)board[y, x].type} " : "X ";
                }
                s += "\n";
            }
            DebugLogger.Verbose(LogCategory.Puzzle, s, this);
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
            return GetTileSafe(x, y);
        }

        private bool EnsureBoardReady()
        {
            if (board == null)
            {
                DebugLogger.Warning(LogCategory.Puzzle, "[Board] Board array is null", this);
                return false;
            }
            if (board.GetLength(0) != BOARD_SIZE || board.GetLength(1) != BOARD_SIZE)
            {
                DebugLogger.Error(LogCategory.Puzzle, $"[Board] Board array size mismatch: {board.GetLength(1)}x{board.GetLength(0)}", this);
                return false;
            }
            return true;
        }

        private TileData GetTileSafe(int x, int y)
        {
            if (!EnsureBoardReady()) return null;
            if (!IsValidPosition(x, y))
            {
                DebugLogger.Warning(LogCategory.Puzzle, $"[Board] Out-of-bounds tile access: ({x},{y})", this);
                return null;
            }
            return board[y, x];
        }

        // ===== Private =====

        private void SwapTiles(TileData tileA, TileData tileB)
        {
            if (!EnsureBoardReady()) return;
            if (tileA == null || tileB == null)
            {
                DebugLogger.Warning(LogCategory.Puzzle, "[Board] SwapTiles received null tile", this);
                return;
            }
            int tempX = tileA.x;
            int tempY = tileA.y;

            tileA.x = tileB.x;
            tileA.y = tileB.y;
            tileB.x = tempX;
            tileB.y = tempY;

            if (!IsValidPosition(tileA.x, tileA.y) || !IsValidPosition(tileB.x, tileB.y))
            {
                DebugLogger.Warning(LogCategory.Puzzle, "[Board] SwapTiles produced invalid coordinates", this);
                return;
            }

            board[tileA.y, tileA.x] = tileA;
            board[tileB.y, tileB.x] = tileB;
        }

        private bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < BOARD_SIZE && y >= 0 && y < BOARD_SIZE;
        }

        private TileType GetRandomTileType()
        {
            int randomIndex = Random.Range(0, GameConstants.Board.TileTypeCount);
            return (TileType)randomIndex;
        }

        /// <summary>
        /// 매치 그룹에서 특수 아이템이 살아남을 위치 선택.
        /// 스왑 origin이 매치에 포함되어 있으면 그것을, 아니면 매치 그룹의 중간 타일.
        /// </summary>
        private TileData ChooseSurvivor(List<TileData> matchGroup, HashSet<TileData> swapOrigins)
        {
            if (matchGroup == null || matchGroup.Count == 0) return null;
            if (swapOrigins != null)
            {
                foreach (TileData t in matchGroup)
                {
                    if (swapOrigins.Contains(t)) return t;
                }
            }
            return matchGroup[matchGroup.Count / 2];
        }

        /// <summary>
        /// 특수 타일 발화. 발화 결과 제거 대상을 removalSet에 추가.
        /// 체인 방지를 위해 발화한 타일의 specialItem은 즉시 None으로 마킹.
        /// 발화 결과에 또 다른 특수 타일이 있으면 재귀적으로 발화 (체인 폭발).
        /// </summary>
        private void ActivateSpecial(TileData special, TileType partnerColor, HashSet<TileData> removalSet)
        {
            if (special == null) return;
            SpecialItemType type = special.specialItem;
            if (type == SpecialItemType.None) return;

            // 체인 무한 루프 방지 — 즉시 소비 마킹
            special.specialItem = SpecialItemType.None;

            List<TileData> activated = null;
            switch (type)
            {
                case SpecialItemType.RocketHorizontal:
                    activated = SpecialItem.ActivateRocket(special, board, true);
                    break;
                case SpecialItemType.RocketVertical:
                    activated = SpecialItem.ActivateRocket(special, board, false);
                    break;
                case SpecialItemType.Rocket:
                    // legacy — 가로로 폴백
                    activated = SpecialItem.ActivateRocket(special, board, true);
                    break;
                case SpecialItemType.Bomb:
                    activated = SpecialItem.ActivateBomb(special, board);
                    break;
                case SpecialItemType.Rainbow:
                    // partnerColor: 스왑 발화 시 상대 타일 색, 체인 발화 시 자기 타일 색
                    activated = SpecialItem.ActivateRainbow(board, partnerColor);
                    break;
                default:
                    return;
            }

            if (activated == null) return;
            foreach (TileData t in activated)
            {
                removalSet.Add(t);
                if (t != null && t.specialItem != SpecialItemType.None && t != special)
                {
                    ActivateSpecial(t, t.type, removalSet);
                }
            }
        }

        /// <summary>
        /// removalSet에 들어와 있지만 아직 활성화되지 않은 특수 타일 처리.
        /// 특수-only 스왑 콤보 결과나 매치 처리 중 직접 추가된 케이스.
        /// </summary>
        private void ResolveChainedSpecials(HashSet<TileData> removalSet)
        {
            int safety = 0;
            bool anyActivated = true;
            while (anyActivated && safety < GameConstants.Board.SpecialChainSafetyLimit)
            {
                anyActivated = false;
                List<TileData> toActivate = new List<TileData>();
                foreach (TileData t in removalSet)
                {
                    if (t != null && t.specialItem != SpecialItemType.None) toActivate.Add(t);
                }
                foreach (TileData t in toActivate)
                {
                    ActivateSpecial(t, t.type, removalSet);
                    anyActivated = true;
                }
                safety++;
            }
        }

        /// <summary>
        /// 보드 생성 직후 우연히 매치가 형성된 타일을 인접 타일과 비교해 비매치 타입으로 즉시 교체.
        /// 기존 RemoveMatches→Gravity→FillEmpty 방식은 새로 채운 타일이 또 매치를 만들 수 있어 수렴 보장이 안 됐음.
        /// 이 방식은 매 iter마다 매치 타일만 교체하므로 빠르게 수렴.
        /// </summary>
        private void RemoveInitialMatches()
        {
            const int maxIterations = GameConstants.Board.InitialMatchResolveMaxIterations;
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
                DebugLogger.Warning(LogCategory.Puzzle, $"[Board] Initial matches not fully resolved after {maxIterations} iterations", this);
            else if (iter > 0)
                DebugLogger.Info(LogCategory.Puzzle, $"[Board] Initial matches resolved in {iter} iteration(s)", this);
        }

        /// <summary>
        /// (x,y) 위치에 놓아도 가로/세로 3-매치를 만들지 않는 타일 타입 선택.
        /// 6개 타입 × 시도 4회 안에 거의 항상 비매치 후보를 찾을 수 있음 (확률적으로).
        /// </summary>
        private TileType PickNonMatchingType(int x, int y)
        {
            if (!EnsureBoardReady()) return GetRandomTileType();
            const int numTypes = GameConstants.Board.TileTypeCount;
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
            if (!EnsureBoardReady() || !IsValidPosition(x, y)) return false;
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

#if UNITY_EDITOR
        // ===== Editor-only debug API for tests (Tools/Whisker Tales/Test/*) =====

        /// <summary>Test 전용: 빈 보드로 초기화 (random spawn 없음).</summary>
        public void DebugSetupEmpty(int moveLimitValue = 99)
        {
            board = new TileData[BOARD_SIZE, BOARD_SIZE];
            levelGoal = null;
            currentLevelId = 0;
            moveLimit = moveLimitValue;
            movesUsed = 0;
            starsEarned = 0;
            isLevelComplete = false;
            isAnimating = false;
        }

        /// <summary>Test 전용: 특정 좌표에 타일 직접 배치.</summary>
        public void DebugSetTile(int x, int y, TileType type, SpecialItemType special = SpecialItemType.None)
        {
            if (board == null) DebugSetupEmpty();
            if (!IsValidPosition(x, y)) return;
            TileData t = new TileData(x, y, type);
            t.specialItem = special;
            board[y, x] = t;
        }

        /// <summary>Test 전용: 보드 직접 읽기.</summary>
        public TileData[,] DebugBoard => board;

        /// <summary>
        /// Test 전용: 현재 보드 상태에서 캐스케이드 루프를 직접 실행.
        /// TrySwapTiles와 동일한 매치 처리/특수 발화 로직을 사용하지만 swap 없음.
        /// 반환값: 실행된 캐스케이드 iter 수.
        /// </summary>
        public int DebugProcessExistingMatches(int maxIterations = MAX_CASCADE_ITERATIONS)
        {
            if (board == null) return 0;
            int cascade = 0;
            while (cascade < maxIterations)
            {
                List<List<TileData>> currentMatches = MatchLogic.FindAllMatches(board);
                if (currentMatches.Count == 0) break;

                HashSet<TileData> removalSet = new HashSet<TileData>();
                foreach (List<TileData> matchGroup in currentMatches)
                {
                    SpecialItemType produced = MatchLogic.GetSpecialItemType(matchGroup);
                    TileData survivor = (produced != SpecialItemType.None)
                        ? ChooseSurvivor(matchGroup, null)
                        : null;

                    foreach (TileData tile in matchGroup)
                    {
                        if (tile == survivor) continue;
                        if (tile.specialItem != SpecialItemType.None)
                            ActivateSpecial(tile, tile.type, removalSet);
                        removalSet.Add(tile);
                    }

                    if (survivor != null)
                    {
                        survivor.specialItem = produced;
                        survivor.isMatched = false;
                    }
                }

                ResolveChainedSpecials(removalSet);

                foreach (TileData t in removalSet)
                {
                    if (t == null) continue;
                    if (t.x < 0 || t.x >= BOARD_SIZE || t.y < 0 || t.y >= BOARD_SIZE) continue;
                    if (board[t.y, t.x] == t) board[t.y, t.x] = null;
                }

                ApplyGravity();
                FillEmpty();
                cascade++;
            }
            return cascade;
        }
#endif

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
