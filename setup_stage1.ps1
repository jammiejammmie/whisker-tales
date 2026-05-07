Write-Host "=== Whisker Tales Stage 1 Setup ===" -ForegroundColor Cyan
Write-Host ""

$projectRoot = Get-Location
$puzzlePath = Join-Path $projectRoot "Assets\Scripts\Puzzle"

Write-Host "1️⃣  Creating Assets/Scripts/Puzzle/ folder..." -ForegroundColor Yellow
if (-not (Test-Path $puzzlePath)) {
    New-Item -ItemType Directory -Path $puzzlePath -Force | Out-Null
    Write-Host "   ✅ Folder created" -ForegroundColor Green
} else {
    Write-Host "   ✅ Folder already exists" -ForegroundColor Green
}

Write-Host ""

Write-Host "2️⃣  Creating TileData.cs..." -ForegroundColor Yellow
$tileDataPath = Join-Path $puzzlePath "TileData.cs"
$tileDataContent = @"
namespace WhiskerTales.Puzzle
{
    /// <summary>
    /// 매치-3 보드의 타일 타입을 정의하는 열거형
    /// 6가지 색상: 생선, 우유, 털실, 캣닢, 발도장, 생선뼈
    /// </summary>
    public enum TileType
    {
        /// <summary>생선 타일</summary>
        Fish = 0,

        /// <summary>우유 타일</summary>
        Milk = 1,

        /// <summary>털실 타일</summary>
        Yarn = 2,

        /// <summary>캣닢 타일</summary>
        Catnip = 3,

        /// <summary>발도장 타일</summary>
        Pawprint = 4,

        /// <summary>생선뼈 타일</summary>
        Fishbone = 5
    }

    /// <summary>
    /// 특수 아이템 타입을 정의하는 열거형
    /// 매치-3 결과로 생성되는 특수 아이템들
    /// </summary>
    public enum SpecialItemType
    {
        /// <summary>특수 아이템 없음</summary>
        None = 0,

        /// <summary>로켓 (한 줄 제거)</summary>
        Rocket = 1,

        /// <summary>폭탄 (3×3 영역 제거)</summary>
        Bomb = 2,

        /// <summary>무지개 털실 (같은 색 모두 제거)</summary>
        Rainbow = 3,

        /// <summary>냥냥 망치 (임의 칸 제거)</summary>
        Hammer = 4
    }

    /// <summary>
    /// 장애물 타입을 정의하는 열거형
    /// 레벨에 배치되는 장애물들
    /// </summary>
    public enum ObstacleType
    {
        /// <summary>장애물 없음</summary>
        None = 0,

        /// <summary>상자 (2회 터치로 제거)</summary>
        Box = 1,

        /// <summary>얼음 (1회 터치로 제거)</summary>
        Ice = 2,

        /// <summary>자물쇠 (제거 불가, 매치 불가)</summary>
        Lock = 3,

        /// <summary>체인 (인접 타일과 함께 제거)</summary>
        Chain = 4
    }

    /// <summary>
    /// 매치-3 보드의 개별 타일 데이터
    /// MonoBehaviour를 상속하지 않는 순수 데이터 클래스
    /// </summary>
    public class TileData
    {
        /// <summary>타일의 X 좌표 (0~7)</summary>
        public int x;

        /// <summary>타일의 Y 좌표 (0~7)</summary>
        public int y;

        /// <summary>타일의 타입 (색상)</summary>
        public TileType type;

        /// <summary>타일이 가진 특수 아이템</summary>
        public SpecialItemType specialItem;

        /// <summary>타일이 가진 장애물</summary>
        public ObstacleType obstacle;

        /// <summary>장애물의 내구도 (Box, Ice, Chain 등)</summary>
        public int obstacleHealth;

        /// <summary>이 타일이 매치되었는지 여부</summary>
        public bool isMatched;

        /// <summary>이 타일이 현재 이동 중인지 여부</summary>
        public bool isMoving;

        /// <summary>이 타일이 잠겨있는지 여부 (Lock 장애물)</summary>
        public bool isLocked;

        /// <summary>
        /// TileData 생성자
        /// </summary>
        /// <param name="x">X 좌표</param>
        /// <param name="y">Y 좌표</param>
        /// <param name="type">타일 타입</param>
        public TileData(int x, int y, TileType type)
        {
            this.x = x;
            this.y = y;
            this.type = type;
            this.specialItem = SpecialItemType.None;
            this.obstacle = ObstacleType.None;
            this.obstacleHealth = 0;
            this.isMatched = false;
            this.isMoving = false;
            this.isLocked = false;
        }

        /// <summary>
        /// 이 타일이 다른 타일과 인접한지 확인
        /// </summary>
        /// <param name="other">비교할 다른 타일</param>
        /// <returns>인접하면 true, 아니면 false</returns>
        public bool IsAdjacentTo(TileData other)
        {
            if (other == null)
                return false;

            int dx = System.Math.Abs(this.x - other.x);
            int dy = System.Math.Abs(this.y - other.y);

            return (dx == 1 && dy == 0) || (dx == 0 && dy == 1);
        }
    }
}
"@

Set-Content -Path $tileDataPath -Value $tileDataContent -Encoding UTF8
Write-Host "   ✅ TileData.cs created" -ForegroundColor Green

Write-Host ""

Write-Host "3️⃣  Creating MatchLogic.cs..." -ForegroundColor Yellow
$matchLogicPath = Join-Path $puzzlePath "MatchLogic.cs"
$matchLogicContent = @"
using System.Collections.Generic;
using UnityEngine;

namespace WhiskerTales.Puzzle
{
    /// <summary>
    /// 매치-3 게임의 매치 판정 및 분석 로직을 담당하는 static 클래스
    /// 보드 상의 매치를 찾고, 특수 아이템 생성 여부를 판정함
    /// MonoBehaviour를 상속하지 않음 (순수 로직)
    /// </summary>
    public static class MatchLogic
    {
        /// <summary>
        /// 최소 매치 개수 (3개 이상이어야 매치로 인정)
        /// </summary>
        private const int MIN_MATCH_COUNT = 3;

        /// <summary>
        /// 보드 전체에서 모든 매치를 찾음
        /// </summary>
        /// <param name="board">8×8 타일 데이터 배열</param>
        /// <returns>매치된 타일들의 리스트 (각 그룹별로 분리)</returns>
        public static List<List<TileData>> FindAllMatches(TileData[,] board)
        {
            List<List<TileData>> allMatches = new List<List<TileData>>();

            if (board == null || board.GetLength(0) == 0 || board.GetLength(1) == 0)
            {
                Debug.LogWarning("[MatchLogic] Board is null or empty");
                return allMatches;
            }

            int rows = board.GetLength(0);
            int cols = board.GetLength(1);

            // 이미 매치된 타일은 중복 처리하지 않기 위해 추적
            bool[,] matched = new bool[rows, cols];

            // 가로 매치 찾기
            for (int y = 0; y < rows; y++)
            {
                List<TileData> rowMatches = FindMatchesInRow(board, y);
                foreach (TileData tile in rowMatches)
                {
                    matched[tile.y, tile.x] = true;
                }
                if (rowMatches.Count >= MIN_MATCH_COUNT)
                {
                    allMatches.Add(rowMatches);
                }
            }

            // 세로 매치 찾기
            for (int x = 0; x < cols; x++)
            {
                List<TileData> colMatches = FindMatchesInColumn(board, x);
                // 이미 가로에서 매치된 타일은 제외
                List<TileData> newMatches = new List<TileData>();
                foreach (TileData tile in colMatches)
                {
                    if (!matched[tile.y, tile.x])
                    {
                        newMatches.Add(tile);
                        matched[tile.y, tile.x] = true;
                    }
                }
                if (newMatches.Count >= MIN_MATCH_COUNT)
                {
                    allMatches.Add(newMatches);
                }
            }

            return allMatches;
        }

        /// <summary>
        /// 특정 행(row)에서 가로 매치를 찾음
        /// 같은 타입의 타일이 3개 이상 연속으로 있으면 매치
        /// </summary>
        /// <param name="board">8×8 타일 데이터 배열</param>
        /// <param name="row">검사할 행 번호 (0~7)</param>
        /// <returns>매치된 타일들의 리스트</returns>
        public static List<TileData> FindMatchesInRow(TileData[,] board, int row)
        {
            List<TileData> matches = new List<TileData>();

            if (board == null || row < 0 || row >= board.GetLength(0))
            {
                Debug.LogWarning($"[MatchLogic] Invalid row: {row}");
                return matches;
            }

            int cols = board.GetLength(1);
            int i = 0;

            while (i < cols)
            {
                TileData currentTile = board[row, i];

                // 빈 칸이거나 잠긴 타일은 건너뛰기
                if (currentTile == null || currentTile.isLocked)
                {
                    i++;
                    continue;
                }

                // 같은 타입의 연속된 타일 찾기
                List<TileData> group = new List<TileData> { currentTile };
                int j = i + 1;

                while (j < cols && board[row, j] != null && board[row, j].type == currentTile.type)
                {
                    group.Add(board[row, j]);
                    j++;
                }

                // 3개 이상이면 매치
                if (group.Count >= MIN_MATCH_COUNT)
                {
                    matches.AddRange(group);
                }

                i = j;
            }

            return matches;
        }

        /// <summary>
        /// 특정 열(column)에서 세로 매치를 찾음
        /// 같은 타입의 타일이 3개 이상 연속으로 있으면 매치
        /// </summary>
        /// <param name="board">8×8 타일 데이터 배열</param>
        /// <param name="col">검사할 열 번호 (0~7)</param>
        /// <returns>매치된 타일들의 리스트</returns>
        public static List<TileData> FindMatchesInColumn(TileData[,] board, int col)
        {
            List<TileData> matches = new List<TileData>();

            if (board == null || col < 0 || col >= board.GetLength(1))
            {
                Debug.LogWarning($"[MatchLogic] Invalid column: {col}");
                return matches;
            }

            int rows = board.GetLength(0);
            int i = 0;

            while (i < rows)
            {
                TileData currentTile = board[i, col];

                // 빈 칸이거나 잠긴 타일은 건너뛰기
                if (currentTile == null || currentTile.isLocked)
                {
                    i++;
                    continue;
                }

                // 같은 타입의 연속된 타일 찾기
                List<TileData> group = new List<TileData> { currentTile };
                int j = i + 1;

                while (j < rows && board[j, col] != null && board[j, col].type == currentTile.type)
                {
                    group.Add(board[j, col]);
                    j++;
                }

                // 3개 이상이면 매치
                if (group.Count >= MIN_MATCH_COUNT)
                {
                    matches.AddRange(group);
                }

                i = j;
            }

            return matches;
        }

        /// <summary>
        /// 매치된 타일들의 구성을 분석하여 생성될 특수 아이템을 판정
        /// </summary>
        /// <param name="matches">매치된 타일들의 리스트</param>
        /// <returns>생성될 특수 아이템 타입</returns>
        public static SpecialItemType GetSpecialItemType(List<TileData> matches)
        {
            if (matches == null || matches.Count < MIN_MATCH_COUNT)
            {
                return SpecialItemType.None;
            }

            int matchCount = matches.Count;

            // 4개 직선 매치 → 로켓 생성
            if (matchCount == 4 && IsLinearMatch(matches))
            {
                return SpecialItemType.Rocket;
            }

            // 5개 L/T 형태 매치 → 폭탄 생성
            if (matchCount == 5 && IsLOrTShape(matches))
            {
                return SpecialItemType.Bomb;
            }

            // 5개 직선 매치 → 무지개 털실 생성
            if (matchCount >= 5 && IsLinearMatch(matches))
            {
                return SpecialItemType.Rainbow;
            }

            // 기본 3개 매치 → 특수 아이템 없음
            return SpecialItemType.None;
        }

        /// <summary>
        /// 두 타일을 스왑할 수 있는지 판정
        /// 인접한 타일만 스왑 가능하고, 잠긴 타일은 스왑 불가능
        /// </summary>
        /// <param name="tileA">첫 번째 타일</param>
        /// <param name="tileB">두 번째 타일</param>
        /// <returns>스왑 가능하면 true, 불가능하면 false</returns>
        public static bool IsValidSwap(TileData tileA, TileData tileB)
        {
            if (tileA == null || tileB == null)
            {
                return false;
            }

            // 잠긴 타일은 스왑 불가능
            if (tileA.isLocked || tileB.isLocked)
            {
                return false;
            }

            // 이동 중인 타일은 스왑 불가능
            if (tileA.isMoving || tileB.isMoving)
            {
                return false;
            }

            // 인접하지 않으면 스왑 불가능
            return tileA.IsAdjacentTo(tileB);
        }

        /// <summary>
        /// 주어진 타일들이 일직선(가로 또는 세로)을 이루는지 판정
        /// </summary>
        /// <param name="tiles">타일들의 리스트</param>
        /// <returns>일직선이면 true, 아니면 false</returns>
        private static bool IsLinearMatch(List<TileData> tiles)
        {
            if (tiles == null || tiles.Count < MIN_MATCH_COUNT)
                return false;

            // 모두 같은 X 좌표 (세로 일직선)
            bool sameColumn = true;
            int firstX = tiles[0].x;
            foreach (TileData tile in tiles)
            {
                if (tile.x != firstX)
                {
                    sameColumn = false;
                    break;
                }
            }
            if (sameColumn)
                return true;

            // 모두 같은 Y 좌표 (가로 일직선)
            bool sameRow = true;
            int firstY = tiles[0].y;
            foreach (TileData tile in tiles)
            {
                if (tile.y != firstY)
                {
                    sameRow = false;
                    break;
                }
            }
            return sameRow;
        }

        /// <summary>
        /// 주어진 타일들이 L 또는 T 형태를 이루는지 판정
        /// </summary>
        /// <param name="tiles">타일들의 리스트</param>
        /// <returns>L 또는 T 형태면 true, 아니면 false</returns>
        private static bool IsLOrTShape(List<TileData> tiles)
        {
            if (tiles == null || tiles.Count != 5)
                return false;

            // L 또는 T 형태: 한 축에 3개, 다른 축에 2개 이상 (교집합 1개)
            // 예: (1,1), (1,2), (1,3), (2,3), (3,3) → L자 형태

            // X 좌표별 그룹화
            Dictionary<int, int> xGroups = new Dictionary<int, int>();
            foreach (TileData tile in tiles)
            {
                if (!xGroups.ContainsKey(tile.x))
                    xGroups[tile.x] = 0;
                xGroups[tile.x]++;
            }

            // Y 좌표별 그룹화
            Dictionary<int, int> yGroups = new Dictionary<int, int>();
            foreach (TileData tile in tiles)
            {
                if (!yGroups.ContainsKey(tile.y))
                    yGroups[tile.y] = 0;
                yGroups[tile.y]++;
            }

            // L/T 형태: 한 축에 3개, 다른 축에 3개 이상 (교집합 있음)
            bool hasThreeInX = false;
            bool hasThreeInY = false;

            foreach (int count in xGroups.Values)
            {
                if (count >= 3)
                    hasThreeInX = true;
            }

            foreach (int count in yGroups.Values)
            {
                if (count >= 3)
                    hasThreeInY = true;
            }

            return hasThreeInX && hasThreeInY;
        }
    }
}
"@

Set-Content -Path $matchLogicPath -Value $matchLogicContent -Encoding UTF8
Write-Host "   ✅ MatchLogic.cs created" -ForegroundColor Green

Write-Host ""

Write-Host "4️⃣  Creating Board.cs..." -ForegroundColor Yellow
$boardPath = Join-Path $puzzlePath "Board.cs"
$boardContent = @"
using System.Collections.Generic;
using UnityEngine;
using WhiskerTales.Core;

namespace WhiskerTales.Puzzle
{
    /// <summary>
    /// 매치-3 게임의 8×8 보드를 관리하는 MonoBehaviour
    /// 타일 생성, 스왑, 매치 판정, 중력 적용, 보드 채우기 등을 담당
    /// 싱글톤 패턴 사용
    /// </summary>
    public class Board : MonoBehaviour
    {
        /// <summary>
        /// Board 싱글톤 인스턴스
        /// </summary>
        public static Board instance { get; private set; }

        /// <summary>
        /// 보드 크기 (8×8)
        /// </summary>
        private const int BOARD_SIZE = 8;

        /// <summary>
        /// 보드의 타일 데이터 배열
        /// board[y, x] 형식으로 접근 (y: 행, x: 열)
        /// </summary>
        private TileData[,] board;

        /// <summary>
        /// 현재 레벨의 이동 횟수 제한
        /// </summary>
        private int moveLimit;

        /// <summary>
        /// 현재 사용한 이동 횟수
        /// </summary>
        private int movesUsed;

        /// <summary>
        /// 현재 획득한 별의 개수
        /// </summary>
        private int starsEarned;

        /// <summary>
        /// 현재 레벨이 클리어되었는지 여부
        /// </summary>
        private bool isLevelComplete;

        /// <summary>
        /// 보드가 현재 애니메이션 중인지 여부
        /// (타일 낙하, 제거 등)
        /// </summary>
        private bool isAnimating;

        /// <summary>
        /// 매치가 발견되었을 때 발생하는 이벤트
        /// </summary>
        public delegate void OnMatchFoundDelegate(List<TileData> matches);
        public event OnMatchFoundDelegate OnMatchFound;

        /// <summary>
        /// 레벨이 완료되었을 때 발생하는 이벤트
        /// </summary>
        public delegate void OnLevelCompleteDelegate(int stars);
        public event OnLevelCompleteDelegate OnLevelComplete;

        private void Awake()
        {
            // 싱글톤 패턴 구현
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
        }

        /// <summary>
        /// 보드 초기화
        /// 새 게임 시작 시 호출
        /// </summary>
        /// <param name="moveLimitValue">이 레벨의 이동 횟수 제한</param>
        public void Initialize(int moveLimitValue = 25)
        {
            board = new TileData[BOARD_SIZE, BOARD_SIZE];
            moveLimit = moveLimitValue;
            movesUsed = 0;
            starsEarned = 0;
            isLevelComplete = false;
            isAnimating = false;

            SpawnTiles();

            Debug.Log($"[Board] Initialized with move limit: {moveLimit}");
        }

        /// <summary>
        /// 보드에 타일을 생성하고 배치
        /// 각 칸에 랜덤한 타일 타입을 할당
        /// </summary>
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

            // 초기 매치 제거 (게임 시작 시 매치가 있으면 안 됨)
            RemoveInitialMatches();

            Debug.Log("[Board] Tiles spawned");
        }

        /// <summary>
        /// 두 타일을 스왑하려고 시도
        /// 스왑 후 매치가 있으면 true, 없으면 false 반환
        /// </summary>
        /// <param name="x1">첫 번째 타일의 X 좌표</param>
        /// <param name="y1">첫 번째 타일의 Y 좌표</param>
        /// <param name="x2">두 번째 타일의 X 좌표</param>
        /// <param name="y2">두 번째 타일의 Y 좌표</param>
        /// <returns>스왑 후 매치가 있으면 true, 없으면 false</returns>
        public bool TrySwapTiles(int x1, int y1, int x2, int y2)
        {
            // 유효성 검사
            if (!IsValidPosition(x1, y1) || !IsValidPosition(x2, y2))
            {
                Debug.LogWarning($"[Board] Invalid position: ({x1},{y1}) or ({x2},{y2})");
                return false;
            }

            TileData tileA = board[y1, x1];
            TileData tileB = board[y2, x2];

            // 스왑 가능 여부 확인
            if (!MatchLogic.IsValidSwap(tileA, tileB))
            {
                Debug.LogWarning($"[Board] Invalid swap: ({x1},{y1}) and ({x2},{y2})");
                return false;
            }

            // 이동 횟수 증가
            movesUsed++;

            // 타일 스왑
            SwapTiles(tileA, tileB);

            // 매치 찾기
            List<List<TileData>> allMatches = MatchLogic.FindAllMatches(board);

            if (allMatches.Count > 0)
            {
                // 매치 발견
                foreach (List<TileData> matches in allMatches)
                {
                    OnMatchFound?.Invoke(matches);
                    AudioManager.instance.PlaySFX("match_success");

                    // 특수 아이템 판정
                    SpecialItemType specialItem = MatchLogic.GetSpecialItemType(matches);
                    if (specialItem != SpecialItemType.None)
                    {
                        Debug.Log($"[Board] Special item created: {specialItem}");
                    }
                }

                // 매치 제거 및 보드 정리
                RemoveMatches(allMatches);
                ApplyGravity();
                FillEmpty();

                // 이동 횟수 확인
                // TODO Stage 2: 이동 소진이 아닌 목표 달성 조건으로 교체
                if (movesUsed >= moveLimit)
                {
                    isLevelComplete = true;
                    starsEarned = CalculateStars();
                    OnLevelComplete?.Invoke(starsEarned);
                    CafeRestorationManager.instance.OnPuzzleClear(starsEarned);
                    Debug.Log($"[Board] Level complete! Stars: {starsEarned}");
                }

                return true;
            }
            else
            {
                // 매치 없음 - 스왑 취소
                SwapTiles(tileA, tileB);
                movesUsed--;
                Debug.Log("[Board] No match found, swap cancelled");
                return false;
            }
        }

        /// <summary>
        /// 보드에서 매치된 타일들을 모두 찾음
        /// </summary>
        /// <returns>매치된 타일들의 리스트</returns>
        public List<TileData> FindMatches()
        {
            List<List<TileData>> allMatches = MatchLogic.FindAllMatches(board);
            List<TileData> result = new List<TileData>();

            foreach (List<TileData> matches in allMatches)
            {
                result.AddRange(matches);
            }

            return result;
        }

        /// <summary>
        /// 주어진 매치들을 보드에서 제거
        /// </summary>
        /// <param name="allMatches">제거할 매치들의 리스트 (각 그룹별로 분리)</param>
        public void RemoveMatches(List<List<TileData>> allMatches)
        {
            if (allMatches == null || allMatches.Count == 0)
                return;

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

        /// <summary>
        /// 중력을 적용하여 타일들을 아래로 낙하시킴
        /// 빈 칸 위의 타일들이 아래로 내려옴
        /// </summary>
        public void ApplyGravity()
        {
            for (int x = 0; x < BOARD_SIZE; x++)
            {
                // 각 열에서 아래부터 위로 스캔
                int writePos = BOARD_SIZE - 1;

                for (int y = BOARD_SIZE - 1; y >= 0; y--)
                {
                    if (board[y, x] != null)
                    {
                        if (y != writePos)
                        {
                            // 타일을 아래로 이동
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

        /// <summary>
        /// 빈 칸을 새로운 타일로 채움
        /// 보드 상단에서 새 타일을 생성
        /// </summary>
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

        /// <summary>
        /// 보드의 현재 상태를 문자열로 출력 (디버깅용)
        /// </summary>
        public void PrintBoardState()
        {
            string boardString = "[Board State]\n";
            for (int y = 0; y < BOARD_SIZE; y++)
            {
                for (int x = 0; x < BOARD_SIZE; x++)
                {
                    if (board[y, x] != null)
                    {
                        boardString += $"{(int)board[y, x].type} ";
                    }
                    else
                    {
                        boardString += "X ";
                    }
                }
                boardString += "\n";
            }
            Debug.Log(boardString);
        }

        /// <summary>
        /// 현재 이동 횟수를 반환
        /// </summary>
        /// <returns>사용한 이동 횟수</returns>
        public int GetMovesUsed()
        {
            return movesUsed;
        }

        /// <summary>
        /// 남은 이동 횟수를 반환
        /// </summary>
        /// <returns>남은 이동 횟수</returns>
        public int GetMovesRemaining()
        {
            return Mathf.Max(0, moveLimit - movesUsed);
        }

        /// <summary>
        /// 현재 획득한 별의 개수를 반환
        /// </summary>
        /// <returns>별의 개수</returns>
        public int GetStarsEarned()
        {
            return starsEarned;
        }

        /// <summary>
        /// 보드가 현재 애니메이션 중인지 반환
        /// </summary>
        /// <returns>애니메이션 중이면 true</returns>
        public bool IsAnimating()
        {
            return isAnimating;
        }

        /// <summary>
        /// 레벨이 완료되었는지 반환
        /// </summary>
        /// <returns>완료되었으면 true</returns>
        public bool IsLevelComplete()
        {
            return isLevelComplete;
        }

        /// <summary>
        /// 주어진 위치의 타일을 반환
        /// </summary>
        /// <param name="x">X 좌표</param>
        /// <param name="y">Y 좌표</param>
        /// <returns>타일 데이터, 없으면 null</returns>
        public TileData GetTile(int x, int y)
        {
            if (IsValidPosition(x, y))
            {
                return board[y, x];
            }
            return null;
        }

        // ===== Private Methods =====

        /// <summary>
        /// 두 타일의 위치를 교환
        /// </summary>
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

        /// <summary>
        /// 주어진 위치가 보드 범위 내인지 확인
        /// </summary>
        private bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < BOARD_SIZE && y >= 0 && y < BOARD_SIZE;
        }

        /// <summary>
        /// 랜덤한 타일 타입을 반환
        /// 6가지 타입 중 하나를 랜덤하게 선택
        /// </summary>
        private TileType GetRandomTileType()
        {
            int randomIndex = Random.Range(0, 6);
            return (TileType)randomIndex;
        }

        /// <summary>
        /// 게임 시작 시 초기 매치를 제거
        /// 보드 생성 후 이미 매치가 있으면 제거하고 다시 채움
        /// </summary>
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
            {
                Debug.LogWarning("[Board] Could not remove all initial matches after 10 attempts");
            }
        }

        /// <summary>
        /// 현재 보드 상태를 기반으로 별의 개수를 계산
        /// 이동 횟수가 적을수록 더 많은 별을 획득
        /// </summary>
        private int CalculateStars()
        {
            // 기본: 1별
            int stars = 1;

            // 이동 횟수에 따라 추가 별 획득
            if (movesUsed <= moveLimit * 0.5f)
            {
                stars = 3; // 매우 효율적
            }
            else if (movesUsed <= moveLimit * 0.75f)
            {
                stars = 2; // 효율적
            }

            return stars;
        }
    }
}
"@

Set-Content -Path $boardPath -Value $boardContent -Encoding UTF8
Write-Host "   ✅ Board.cs created" -ForegroundColor Green

Write-Host ""

Write-Host "5️⃣  Updating .gitignore..." -ForegroundColor Yellow
$gitignorePath = Join-Path $projectRoot ".gitignore"

if (Test-Path $gitignorePath) {
    $gitignoreContent = Get-Content -Path $gitignorePath -Raw
    $gitignoreContent = $gitignoreContent -replace "^\*\.meta\s*`r?`n", ""
    Set-Content -Path $gitignorePath -Value $gitignoreContent -Encoding UTF8
    Write-Host "   ✅ .gitignore updated (*.meta removed)" -ForegroundColor Green
} else {
    Write-Host "   ⚠️  .gitignore not found" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "=== Setup Complete! ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "✅ Created files:" -ForegroundColor Green
Write-Host "   - Assets/Scripts/Puzzle/TileData.cs"
Write-Host "   - Assets/Scripts/Puzzle/MatchLogic.cs"
Write-Host "   - Assets/Scripts/Puzzle/Board.cs"
Write-Host ""
Write-Host "Press any key to close..." -ForegroundColor Cyan
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
