Write-Host "=== Whisker Tales Stage 2 Setup ===" -ForegroundColor Cyan
Write-Host ""

$projectRoot = Get-Location
$puzzlePath = Join-Path $projectRoot "Assets\Scripts\Puzzle"

# ----------------------------------------------------------------------
# 1. Verify Stage 1 outputs exist
# ----------------------------------------------------------------------
Write-Host "1] Verifying Stage 1 prerequisites..." -ForegroundColor Yellow
$required = @("TileData.cs", "MatchLogic.cs", "Board.cs")
$missing = @()
foreach ($f in $required) {
    if (-not (Test-Path (Join-Path $puzzlePath $f))) { $missing += $f }
}
if ($missing.Count -gt 0) {
    Write-Host "   [X] Stage 1 has not been completed. Missing: $($missing -join ', ')" -ForegroundColor Red
    Write-Host "   Please run setup_stage1.ps1 first." -ForegroundColor Red
    exit 1
}
Write-Host "   [OK] Stage 1 files present" -ForegroundColor Green
Write-Host ""

# ----------------------------------------------------------------------
# 2. Create SpecialItem.cs
# ----------------------------------------------------------------------
Write-Host "2] Creating SpecialItem.cs..." -ForegroundColor Yellow
$specialItemPath = Join-Path $puzzlePath "SpecialItem.cs"
$specialItemContent = @"
using System.Collections.Generic;
using UnityEngine;

namespace WhiskerTales.Puzzle
{
    /// <summary>
    /// 매치-3 특수 아이템 활성화 로직 (로켓, 폭탄, 무지개, 망치)
    /// 모두 보드의 TileData 배열에 작용하는 static 메서드로 구성
    /// 제거 대상 타일들을 List로 반환하며, 실제 보드 수정은 Board가 담당
    /// </summary>
    public static class SpecialItem
    {
        /// <summary>
        /// 로켓 활성화: 한 줄(가로 또는 세로) 전체 제거 대상 반환
        /// </summary>
        /// <param name="tile">발화 위치 타일</param>
        /// <param name="board">8×8 타일 데이터 배열</param>
        /// <param name="horizontal">true면 가로(같은 행), false면 세로(같은 열)</param>
        public static List<TileData> ActivateRocket(TileData tile, TileData[,] board, bool horizontal)
        {
            List<TileData> removed = new List<TileData>();
            if (tile == null || board == null) return removed;

            int rows = board.GetLength(0);
            int cols = board.GetLength(1);

            if (horizontal)
            {
                if (tile.y < 0 || tile.y >= rows) return removed;
                for (int x = 0; x < cols; x++)
                {
                    TileData t = board[tile.y, x];
                    if (t != null && !t.isLocked)
                    {
                        removed.Add(t);
                    }
                }
            }
            else
            {
                if (tile.x < 0 || tile.x >= cols) return removed;
                for (int y = 0; y < rows; y++)
                {
                    TileData t = board[y, tile.x];
                    if (t != null && !t.isLocked)
                    {
                        removed.Add(t);
                    }
                }
            }

            Debug.Log($"[SpecialItem] Rocket activated at ({tile.x},{tile.y}) horizontal={horizontal} -> {removed.Count} tiles");
            return removed;
        }

        /// <summary>
        /// 폭탄 활성화: 발화 위치 중심 3×3 영역 제거 대상 반환
        /// </summary>
        public static List<TileData> ActivateBomb(TileData tile, TileData[,] board)
        {
            List<TileData> removed = new List<TileData>();
            if (tile == null || board == null) return removed;

            int rows = board.GetLength(0);
            int cols = board.GetLength(1);

            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    int nx = tile.x + dx;
                    int ny = tile.y + dy;
                    if (nx < 0 || nx >= cols || ny < 0 || ny >= rows) continue;
                    TileData neighbor = board[ny, nx];
                    if (neighbor != null && !neighbor.isLocked)
                    {
                        removed.Add(neighbor);
                    }
                }
            }

            Debug.Log($"[SpecialItem] Bomb activated at ({tile.x},{tile.y}) -> {removed.Count} tiles");
            return removed;
        }

        /// <summary>
        /// 무지개 활성화: 보드에서 같은 색(targetColor)의 모든 타일 제거 대상 반환
        /// </summary>
        public static List<TileData> ActivateRainbow(TileData[,] board, TileType targetColor)
        {
            List<TileData> removed = new List<TileData>();
            if (board == null) return removed;

            int rows = board.GetLength(0);
            int cols = board.GetLength(1);

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    TileData t = board[y, x];
                    if (t != null && !t.isLocked && t.type == targetColor)
                    {
                        removed.Add(t);
                    }
                }
            }

            Debug.Log($"[SpecialItem] Rainbow activated targetColor={targetColor} -> {removed.Count} tiles");
            return removed;
        }

        /// <summary>
        /// 망치 활성화: 지정 위치 타일 1개 제거 대상 반환
        /// 잠긴 타일은 제거되지 않음
        /// </summary>
        public static List<TileData> ActivateHammer(TileData tile, TileData[,] board)
        {
            List<TileData> removed = new List<TileData>();
            if (tile == null || board == null) return removed;

            int rows = board.GetLength(0);
            int cols = board.GetLength(1);
            if (tile.x < 0 || tile.x >= cols || tile.y < 0 || tile.y >= rows) return removed;

            TileData target = board[tile.y, tile.x];
            if (target != null && !target.isLocked)
            {
                removed.Add(target);
            }

            Debug.Log($"[SpecialItem] Hammer activated at ({tile.x},{tile.y}) -> {removed.Count} tiles");
            return removed;
        }

        /// <summary>
        /// 두 특수 아이템의 콤보 효과를 계산
        /// 예: 로켓+로켓 = 가로/세로 두 줄, 로켓+폭탄 = 한 줄 + 3×3
        /// 단순 합집합으로 처리하며 중복은 자동 제거됨
        /// </summary>
        public static List<TileData> ActivateCombo(TileData tileA, TileData tileB, TileData[,] board)
        {
            HashSet<TileData> combined = new HashSet<TileData>();
            if (tileA == null || tileB == null || board == null) return new List<TileData>(combined);

            // tileA 효과
            switch (tileA.specialItem)
            {
                case SpecialItemType.Rocket:
                    foreach (var t in ActivateRocket(tileA, board, true)) combined.Add(t);
                    foreach (var t in ActivateRocket(tileA, board, false)) combined.Add(t);
                    break;
                case SpecialItemType.Bomb:
                    foreach (var t in ActivateBomb(tileA, board)) combined.Add(t);
                    break;
                case SpecialItemType.Rainbow:
                    foreach (var t in ActivateRainbow(board, tileB.type)) combined.Add(t);
                    break;
                case SpecialItemType.Hammer:
                    foreach (var t in ActivateHammer(tileA, board)) combined.Add(t);
                    break;
            }

            // tileB 효과
            switch (tileB.specialItem)
            {
                case SpecialItemType.Rocket:
                    foreach (var t in ActivateRocket(tileB, board, true)) combined.Add(t);
                    foreach (var t in ActivateRocket(tileB, board, false)) combined.Add(t);
                    break;
                case SpecialItemType.Bomb:
                    foreach (var t in ActivateBomb(tileB, board)) combined.Add(t);
                    break;
                case SpecialItemType.Rainbow:
                    foreach (var t in ActivateRainbow(board, tileA.type)) combined.Add(t);
                    break;
                case SpecialItemType.Hammer:
                    foreach (var t in ActivateHammer(tileB, board)) combined.Add(t);
                    break;
            }

            Debug.Log($"[SpecialItem] Combo {tileA.specialItem}+{tileB.specialItem} -> {combined.Count} tiles");
            return new List<TileData>(combined);
        }
    }
}
"@

if (Test-Path $specialItemPath) {
    Write-Host "   [..] SpecialItem.cs exists, overwriting" -ForegroundColor DarkGray
}
Set-Content -Path $specialItemPath -Value $specialItemContent -Encoding UTF8
Write-Host "   [OK] SpecialItem.cs created" -ForegroundColor Green
Write-Host ""

# ----------------------------------------------------------------------
# 3. Insert TileData overload into LevelGoal.cs (idempotent)
# ----------------------------------------------------------------------
Write-Host "3] Patching LevelGoal.cs (TileData overload)..." -ForegroundColor Yellow
$levelGoalPath = Join-Path $puzzlePath "LevelGoal.cs"

if (-not (Test-Path $levelGoalPath)) {
    Write-Host "   [!] LevelGoal.cs not found at $levelGoalPath" -ForegroundColor Yellow
    Write-Host "       Skipping patch step." -ForegroundColor Yellow
} else {
    $lgContent = Get-Content -Path $levelGoalPath -Raw -Encoding UTF8

    if ($lgContent -match "UpdateProgress\s*\(\s*List<TileData>") {
        Write-Host "   [..] TileData overload already present, skipping" -ForegroundColor DarkGray
    } else {
        $overload = @"
        /// <summary>
        /// 진행도 업데이트 (TileData 기반 — Stage 2에서 추가됨)
        /// Board가 TileData 배열을 사용하므로 Tile 오버로드와 별개로 제공
        /// </summary>
        public void UpdateProgress(List<TileData> removedTiles)
        {
            if (removedTiles == null || removedTiles.Count == 0)
                return;

            int previousProgress = currentProgress;

            switch (goalType)
            {
                case LevelGoalType.RemoveBlocks:
                    currentProgress += removedTiles.Count;
                    break;

                case LevelGoalType.CollectItems:
                    foreach (TileData tile in removedTiles)
                    {
                        if (tile.specialItem != SpecialItemType.None)
                            currentProgress++;
                    }
                    break;

                case LevelGoalType.ReachScore:
                    currentProgress += removedTiles.Count * 100;
                    break;

                case LevelGoalType.DestroyObstacles:
                    foreach (TileData tile in removedTiles)
                    {
                        if (tile.obstacle != ObstacleType.None && tile.obstacleHealth > 0)
                            currentProgress++;
                    }
                    break;
            }

            if (currentProgress > goalValue)
                currentProgress = goalValue;

            if (currentProgress != previousProgress)
            {
                OnProgressChanged?.Invoke(currentProgress);
                Debug.Log($"[LevelGoal] Progress updated (TileData): {currentProgress}/{goalValue}");
            }
        }


"@

        # Anchor: insert before the existing Reset() method's xml summary
        $anchor = "        /// <summary>`r`n        /// 레벨 리셋"
        $altAnchor = "        /// <summary>`n        /// 레벨 리셋"

        if ($lgContent.Contains($anchor)) {
            $patched = $lgContent.Replace($anchor, $overload + $anchor)
            Set-Content -Path $levelGoalPath -Value $patched -Encoding UTF8
            Write-Host "   [OK] TileData overload inserted before Reset()" -ForegroundColor Green
        } elseif ($lgContent.Contains($altAnchor)) {
            $patched = $lgContent.Replace($altAnchor, ($overload -replace "`r`n", "`n") + $altAnchor)
            Set-Content -Path $levelGoalPath -Value $patched -Encoding UTF8
            Write-Host "   [OK] TileData overload inserted before Reset() (LF)" -ForegroundColor Green
        } else {
            Write-Host "   [!] Could not find Reset() anchor — inserting before final brace" -ForegroundColor Yellow
            $idx = $lgContent.LastIndexOf("    }")
            if ($idx -lt 0) {
                Write-Host "   [X] No closing class brace found in LevelGoal.cs" -ForegroundColor Red
            } else {
                $patched = $lgContent.Substring(0, $idx) + $overload + $lgContent.Substring($idx)
                Set-Content -Path $levelGoalPath -Value $patched -Encoding UTF8
                Write-Host "   [OK] TileData overload appended" -ForegroundColor Green
            }
        }
    }
}
Write-Host ""

# ----------------------------------------------------------------------
# 4. Rewrite Board.cs with LevelGoal integration
# ----------------------------------------------------------------------
Write-Host "4] Rewriting Board.cs with LevelGoal integration..." -ForegroundColor Yellow
$boardPath = Join-Path $puzzlePath "Board.cs"
$boardContent = @"
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
                    if (AudioManager.instance != null)
                        AudioManager.instance.PlaySFX("match_success");

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
                        if (CafeRestorationManager.instance != null)
                            CafeRestorationManager.instance.OnPuzzleClear(starsEarned);
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
                        if (CafeRestorationManager.instance != null)
                            CafeRestorationManager.instance.OnPuzzleClear(starsEarned);
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

        private void RemoveInitialMatches()
        {
            int attempts = 0;
            while (MatchLogic.FindAllMatches(board).Count > 0 && attempts < 10)
            {
                List<List<TileData>> matches = MatchLogic.FindAllMatches(board);
                RemoveMatches(matches);
                ApplyGravity();
                FillEmpty();
                attempts++;
            }
            if (attempts >= 10)
                Debug.LogWarning("[Board] Could not remove all initial matches after 10 attempts");
        }

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
"@

Set-Content -Path $boardPath -Value $boardContent -Encoding UTF8
Write-Host "   [OK] Board.cs rewritten with LevelGoal integration" -ForegroundColor Green
Write-Host ""

# ----------------------------------------------------------------------
# 5. Summary
# ----------------------------------------------------------------------
Write-Host "=== Stage 2 Setup Complete ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Created:" -ForegroundColor Green
Write-Host "   + Assets/Scripts/Puzzle/SpecialItem.cs"
Write-Host ""
Write-Host "Modified:" -ForegroundColor Green
Write-Host "   ~ Assets/Scripts/Puzzle/LevelGoal.cs   (added UpdateProgress(List<TileData>) overload)"
Write-Host "   ~ Assets/Scripts/Puzzle/Board.cs       (LevelGoal integration; goal-based completion)"
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "   - Wire up Level data + LevelGoal component in your gameplay scene"
Write-Host "   - Call Board.Initialize(level, levelGoal) instead of the legacy overload"
Write-Host "   - Verify in Unity (compile + Play Mode)"
Write-Host ""
