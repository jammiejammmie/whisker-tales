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

        private const int MAX_CASCADE_ITERATIONS = 10;

        /// <summary>
        /// 두 타일 스왑 시도. 매치가 발생하면 캐스케이드 루프(최대 MAX_CASCADE_ITERATIONS)로
        /// FillEmpty 후에도 새로 형성된 매치를 모두 처리. LevelGoal.UseMove()는 단 1회만 호출
        /// (캐스케이드는 같은 한 수의 결과로 취급).
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

            // 특수 타일 스왑 감지 (스왑 후 매치 0이어도 특수 타일이 있으면 발화)
            bool specialOnA = tileA.specialItem != SpecialItemType.None;
            bool specialOnB = tileB.specialItem != SpecialItemType.None;

            List<List<TileData>> currentMatches = MatchLogic.FindAllMatches(board);
            bool hasInitialMatch = currentMatches.Count > 0;

            if (!hasInitialMatch && !specialOnA && !specialOnB)
            {
                // 매치 없음 + 특수 아님 → 스왑 취소
                SwapTiles(tileA, tileB);
                Debug.Log("[Board] No match, swap cancelled");
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
                HashSet<TileData> removalSet = new HashSet<TileData>();

                // 특수-only 스왑으로 들어온 제거 후보 흡수
                foreach (var t in pendingSwapActivation) removalSet.Add(t);
                pendingSwapActivation.Clear();

                foreach (List<TileData> matchGroup in currentMatches)
                {
                    OnMatchFound?.Invoke(matchGroup);

                    SpecialItemType produced = MatchLogic.GetSpecialItemType(matchGroup);
                    TileData survivor = (produced != SpecialItemType.None)
                        ? ChooseSurvivor(matchGroup, swapOrigins)
                        : null;

                    if (produced != SpecialItemType.None && survivor != null)
                        Debug.Log($"[Board] Created {produced} at ({survivor.x},{survivor.y}) (cascade {cascade})");

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
                Debug.Log($"[Board] Cascade resolved in {cascade} iteration(s), {cumulativeRemoved.Count} tiles total");
            if (cascade >= MAX_CASCADE_ITERATIONS)
                Debug.LogWarning($"[Board] Cascade hit max iterations ({MAX_CASCADE_ITERATIONS})");

            if (levelGoal != null)
            {
                // Stage 2: 목표 기반 완료/실패. UseMove는 캐스케이드 횟수와 무관하게 1회.
                levelGoal.UpdateProgress(cumulativeRemoved);
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
                // Bomb / Rainbow → 후속 commit에서 추가
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
            while (anyActivated && safety < 50)
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
