using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using WhiskerTales.Utilities;
using WhiskerTales.Core;

namespace WhiskerTales.Puzzle
{
    /// <summary>
    /// 매치-3 게임 보드 (8x8 그리드)
    /// 타일 생성, 제거, 매치 판정, 중력 적용 등 관리
    /// </summary>
    public class Board : MonoBehaviour
    {
        [SerializeField] private Tile tilePrefab;
        [SerializeField] private Transform boardContainer;
        [SerializeField] private float tileSize = 1f;
        [SerializeField] private float tileSpacing = 0.1f;

        private Tile[,] tiles = new Tile[Constants.BOARD_SIZE, Constants.BOARD_SIZE];
        private bool isProcessing = false;

        // 이벤트
        public delegate void TilesMatchedHandler(List<Tile> matchedTiles);
        public event TilesMatchedHandler OnTilesMatched;

        public delegate void BoardChangedHandler();
        public event BoardChangedHandler OnBoardChanged;

        private void Start()
        {
            Initialize();
        }

        /// <summary>
        /// 보드 초기화
        /// </summary>
        public void Initialize()
        {
            if (boardContainer == null)
            {
                boardContainer = transform;
            }

            SpawnTiles();
            Debug.Log("[Board] Board initialized");
        }

        /// <summary>
        /// 타일 생성
        /// </summary>
        public void SpawnTiles()
        {
            // 기존 타일 제거
            foreach (Transform child in boardContainer)
            {
                Destroy(child.gameObject);
            }

            // 새 타일 생성
            for (int x = 0; x < Constants.BOARD_SIZE; x++)
            {
                for (int y = 0; y < Constants.BOARD_SIZE; y++)
                {
                    CreateTile(x, y);
                }
            }

            Debug.Log("[Board] Tiles spawned");
        }

        /// <summary>
        /// 특정 위치에 타일 생성
        /// </summary>
        private Tile CreateTile(int x, int y)
        {
            Tile tile = Instantiate(tilePrefab, boardContainer);
            
            // 위치 설정
            Vector3 position = new Vector3(
                x * (tileSize + tileSpacing),
                -y * (tileSize + tileSpacing),
                0
            );
            tile.transform.localPosition = position;

            // 타일 초기화
            tile.Initialize(x, y);
            
            // 랜덤 타입 설정 (장애물 없는 기본 타일)
            TileType randomType = GetRandomTileType();
            tile.SetType(randomType);

            tiles[x, y] = tile;
            return tile;
        }

        /// <summary>
        /// 랜덤 타일 타입 반환
        /// </summary>
        private TileType GetRandomTileType()
        {
            int randomIndex = Random.Range(0, 6);
            return (TileType)randomIndex;
        }

        /// <summary>
        /// 두 타일 스왑 시도
        /// </summary>
        public bool TrySwapTiles(Tile tile1, Tile tile2)
        {
            if (isProcessing)
            {
                Debug.LogWarning("[Board] Board is processing, cannot swap");
                return false;
            }

            if (!IsValidSwap(tile1, tile2))
            {
                Debug.LogWarning("[Board] Invalid swap");
                return false;
            }

            StartCoroutine(SwapTilesCoroutine(tile1, tile2));
            return true;
        }

        /// <summary>
        /// 스왑 가능 여부 확인
        /// </summary>
        private bool IsValidSwap(Tile tile1, Tile tile2)
        {
            if (tile1 == null || tile2 == null)
                return false;

            // 인접한 타일인지 확인
            int dx = Mathf.Abs(tile1.x - tile2.x);
            int dy = Mathf.Abs(tile1.y - tile2.y);

            return (dx == 1 && dy == 0) || (dx == 0 && dy == 1);
        }

        /// <summary>
        /// 타일 스왑 코루틴
        /// </summary>
        private IEnumerator SwapTilesCoroutine(Tile tile1, Tile tile2)
        {
            isProcessing = true;

            // 타일 위치 교환
            int tempX = tile1.x;
            int tempY = tile1.y;

            tile1.x = tile2.x;
            tile1.y = tile2.y;
            tile2.x = tempX;
            tile2.y = tempY;

            tiles[tile1.x, tile1.y] = tile1;
            tiles[tile2.x, tile2.y] = tile2;

            // 애니메이션
            yield return StartCoroutine(AnimateSwap(tile1, tile2));

            // 매치 확인
            List<Tile> matches = FindMatches();

            if (matches.Count == 0)
            {
                // 매치 없음 - 스왑 취소
                Debug.Log("[Board] No match found, reversing swap");
                yield return StartCoroutine(AnimateSwap(tile1, tile2));

                // 위치 되돌리기
                tile1.x = tempX;
                tile1.y = tempY;
                tile2.x = tile1.x;
                tile2.y = tile1.y;

                tiles[tile1.x, tile1.y] = tile1;
                tiles[tile2.x, tile2.y] = tile2;
            }
            else
            {
                // 매치 있음 - 처리
                yield return StartCoroutine(ProcessMatches(matches));
            }

            isProcessing = false;
            OnBoardChanged?.Invoke();
        }

        /// <summary>
        /// 스왑 애니메이션
        /// </summary>
        private IEnumerator AnimateSwap(Tile tile1, Tile tile2)
        {
            float duration = 0.2f;
            float elapsed = 0f;

            Vector3 tile1StartPos = tile1.transform.localPosition;
            Vector3 tile2StartPos = tile2.transform.localPosition;

            Vector3 tile1TargetPos = new Vector3(
                tile2.x * (tileSize + tileSpacing),
                -tile2.y * (tileSize + tileSpacing),
                0
            );

            Vector3 tile2TargetPos = new Vector3(
                tile1.x * (tileSize + tileSpacing),
                -tile1.y * (tileSize + tileSpacing),
                0
            );

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                tile1.transform.localPosition = Vector3.Lerp(tile1StartPos, tile1TargetPos, t);
                tile2.transform.localPosition = Vector3.Lerp(tile2StartPos, tile2TargetPos, t);

                yield return null;
            }

            tile1.transform.localPosition = tile1TargetPos;
            tile2.transform.localPosition = tile2TargetPos;
        }

        /// <summary>
        /// 모든 매치 찾기
        /// </summary>
        public List<Tile> FindMatches()
        {
            List<Tile> matches = new List<Tile>();
            bool[,] matched = new bool[Constants.BOARD_SIZE, Constants.BOARD_SIZE];

            // 가로 매치 찾기
            for (int y = 0; y < Constants.BOARD_SIZE; y++)
            {
                for (int x = 0; x < Constants.BOARD_SIZE; x++)
                {
                    if (matched[x, y] || tiles[x, y] == null)
                        continue;

                    List<Tile> horizontalMatches = FindHorizontalMatches(x, y);
                    if (horizontalMatches.Count >= Constants.MATCH_MINIMUM)
                    {
                        foreach (Tile tile in horizontalMatches)
                        {
                            matches.Add(tile);
                            matched[tile.x, tile.y] = true;
                        }
                    }
                }
            }

            // 세로 매치 찾기
            for (int x = 0; x < Constants.BOARD_SIZE; x++)
            {
                for (int y = 0; y < Constants.BOARD_SIZE; y++)
                {
                    if (matched[x, y] || tiles[x, y] == null)
                        continue;

                    List<Tile> verticalMatches = FindVerticalMatches(x, y);
                    if (verticalMatches.Count >= Constants.MATCH_MINIMUM)
                    {
                        foreach (Tile tile in verticalMatches)
                        {
                            matches.Add(tile);
                            matched[tile.x, tile.y] = true;
                        }
                    }
                }
            }

            return matches;
        }

        /// <summary>
        /// 가로 매치 찾기
        /// </summary>
        private List<Tile> FindHorizontalMatches(int startX, int startY)
        {
            List<Tile> matches = new List<Tile>();
            Tile startTile = tiles[startX, startY];

            if (startTile == null)
                return matches;

            TileType type = startTile.type;
            int count = 1;

            // 오른쪽으로 확인
            for (int x = startX + 1; x < Constants.BOARD_SIZE; x++)
            {
                if (tiles[x, startY] != null && tiles[x, startY].type == type)
                {
                    count++;
                }
                else
                {
                    break;
                }
            }

            // 왼쪽으로 확인
            for (int x = startX - 1; x >= 0; x--)
            {
                if (tiles[x, startY] != null && tiles[x, startY].type == type)
                {
                    count++;
                }
                else
                {
                    break;
                }
            }

            // 매치 개수가 3 이상이면 모두 추가
            if (count >= Constants.MATCH_MINIMUM)
            {
                matches.Add(startTile);

                for (int x = startX + 1; x < Constants.BOARD_SIZE && tiles[x, startY] != null && 
                     tiles[x, startY].type == type; x++)
                {
                    matches.Add(tiles[x, startY]);
                }

                for (int x = startX - 1; x >= 0 && tiles[x, startY] != null && 
                     tiles[x, startY].type == type; x--)
                {
                    matches.Add(tiles[x, startY]);
                }
            }

            return matches;
        }

        /// <summary>
        /// 세로 매치 찾기
        /// </summary>
        private List<Tile> FindVerticalMatches(int startX, int startY)
        {
            List<Tile> matches = new List<Tile>();
            Tile startTile = tiles[startX, startY];

            if (startTile == null)
                return matches;

            TileType type = startTile.type;
            int count = 1;

            // 아래로 확인
            for (int y = startY + 1; y < Constants.BOARD_SIZE; y++)
            {
                if (tiles[startX, y] != null && tiles[startX, y].type == type)
                {
                    count++;
                }
                else
                {
                    break;
                }
            }

            // 위로 확인
            for (int y = startY - 1; y >= 0; y--)
            {
                if (tiles[startX, y] != null && tiles[startX, y].type == type)
                {
                    count++;
                }
                else
                {
                    break;
                }
            }

            // 매치 개수가 3 이상이면 모두 추가
            if (count >= Constants.MATCH_MINIMUM)
            {
                matches.Add(startTile);

                for (int y = startY + 1; y < Constants.BOARD_SIZE && tiles[startX, y] != null && 
                     tiles[startX, y].type == type; y++)
                {
                    matches.Add(tiles[startX, y]);
                }

                for (int y = startY - 1; y >= 0 && tiles[startX, y] != null && 
                     tiles[startX, y].type == type; y--)
                {
                    matches.Add(tiles[startX, y]);
                }
            }

            return matches;
        }

        /// <summary>
        /// 매치 처리
        /// </summary>
        private IEnumerator ProcessMatches(List<Tile> matches)
        {
            OnTilesMatched?.Invoke(matches);

            // 타일 제거 애니메이션
            foreach (Tile tile in matches)
            {
                tile.AnimateRemoval();
            }

            yield return new WaitForSeconds(0.3f);

            // 타일 제거
            foreach (Tile tile in matches)
            {
                tiles[tile.x, tile.y] = null;
            }

            // 중력 적용
            yield return StartCoroutine(ApplyGravity());

            // 새 타일 생성
            yield return StartCoroutine(FillEmpty());

            // 연쇄 매치 확인
            List<Tile> newMatches = FindMatches();
            if (newMatches.Count > 0)
            {
                yield return StartCoroutine(ProcessMatches(newMatches));
            }
        }

        /// <summary>
        /// 중력 적용
        /// </summary>
        private IEnumerator ApplyGravity()
        {
            bool hasMoved = true;

            while (hasMoved)
            {
                hasMoved = false;

                for (int x = 0; x < Constants.BOARD_SIZE; x++)
                {
                    for (int y = Constants.BOARD_SIZE - 2; y >= 0; y--)
                    {
                        if (tiles[x, y] != null && tiles[x, y + 1] == null)
                        {
                            // 타일을 아래로 이동
                            tiles[x, y + 1] = tiles[x, y];
                            tiles[x, y] = null;
                            tiles[x, y + 1].y = y + 1;

                            // 애니메이션
                            StartCoroutine(AnimateTileFall(tiles[x, y + 1]));

                            hasMoved = true;
                        }
                    }
                }

                if (hasMoved)
                    yield return new WaitForSeconds(0.05f);
            }
        }

        /// <summary>
        /// 타일 낙하 애니메이션
        /// </summary>
        private IEnumerator AnimateTileFall(Tile tile)
        {
            float duration = 0.1f;
            float elapsed = 0f;

            Vector3 startPos = tile.transform.localPosition;
            Vector3 targetPos = new Vector3(
                tile.x * (tileSize + tileSpacing),
                -tile.y * (tileSize + tileSpacing),
                0
            );

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                tile.transform.localPosition = Vector3.Lerp(startPos, targetPos, t);
                yield return null;
            }

            tile.transform.localPosition = targetPos;
        }

        /// <summary>
        /// 빈 공간 채우기
        /// </summary>
        private IEnumerator FillEmpty()
        {
            for (int x = 0; x < Constants.BOARD_SIZE; x++)
            {
                for (int y = 0; y < Constants.BOARD_SIZE; y++)
                {
                    if (tiles[x, y] == null)
                    {
                        CreateTile(x, y);
                        yield return new WaitForSeconds(0.05f);
                    }
                }
            }
        }

        /// <summary>
        /// 타일 반환
        /// </summary>
        public Tile GetTile(int x, int y)
        {
            if (IsValidPosition(x, y))
            {
                return tiles[x, y];
            }
            return null;
        }

        /// <summary>
        /// 유효한 위치인지 확인
        /// </summary>
        public bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < Constants.BOARD_SIZE && y >= 0 && y < Constants.BOARD_SIZE;
        }

        /// <summary>
        /// 보드 리셋
        /// </summary>
        public void Reset()
        {
            isProcessing = false;
            SpawnTiles();
        }
    }
}
