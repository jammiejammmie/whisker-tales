using System.Collections.Generic;
using UnityEngine;

namespace WhiskerTales.Puzzle
{
    /// <summary>
    /// 매치-3 게임의 매치 판정 + 특수 아이템 패턴 분류 static 유틸.
    /// 보드 상의 매치를 찾고, 매치 그룹의 형태(직선/L/T/2×2)로 어떤 특수 아이템이 생성될지 결정.
    /// MonoBehaviour 아님 — 순수 로직.
    /// </summary>
    public static class MatchLogic
    {
        /// <summary>최소 매치 개수 (3개 이상이어야 매치로 인정).</summary>
        private const int MIN_MATCH_COUNT = 3;

        /// <summary>
        /// 보드 전체에서 모든 매치를 찾음. 가로/세로 매치 그룹별로 분리해서 반환.
        /// 가로 매치에 포함된 타일은 세로 매치에서 중복 제거됨.
        /// </summary>
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
            bool[,] matched = new bool[rows, cols];

            // 가로 매치
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

            // 세로 매치 (가로에서 잡힌 타일 제외)
            for (int x = 0; x < cols; x++)
            {
                List<TileData> colMatches = FindMatchesInColumn(board, x);
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

            // 2×2 정사각 매치 (가로/세로에서 안 잡힌 4타일 동일 타입 정사각형)
            for (int y = 0; y < rows - 1; y++)
            {
                for (int x = 0; x < cols - 1; x++)
                {
                    TileData tl = board[y, x];
                    TileData tr = board[y, x + 1];
                    TileData bl = board[y + 1, x];
                    TileData br = board[y + 1, x + 1];
                    if (tl == null || tr == null || bl == null || br == null) continue;
                    if (tl.isLocked || tr.isLocked || bl.isLocked || br.isLocked) continue;
                    if (tl.type != tr.type || tl.type != bl.type || tl.type != br.type) continue;
                    // 4타일이 이미 모두 가로/세로 매치에 잡혔으면 패스 (중복 회피)
                    if (matched[y, x] && matched[y, x + 1] && matched[y + 1, x] && matched[y + 1, x + 1]) continue;

                    List<TileData> square = new List<TileData> { tl, tr, bl, br };
                    matched[y, x] = true;
                    matched[y, x + 1] = true;
                    matched[y + 1, x] = true;
                    matched[y + 1, x + 1] = true;
                    allMatches.Add(square);
                }
            }

            return allMatches;
        }

        /// <summary>
        /// 특정 행에서 가로 매치를 찾음. 같은 타입의 타일이 3개 이상 연속하면 매치.
        /// 잠긴 타일은 매치 끊김 지점.
        /// </summary>
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
                if (currentTile == null || currentTile.isLocked)
                {
                    i++;
                    continue;
                }

                List<TileData> group = new List<TileData> { currentTile };
                int j = i + 1;
                while (j < cols && board[row, j] != null && !board[row, j].isLocked
                       && board[row, j].type == currentTile.type)
                {
                    group.Add(board[row, j]);
                    j++;
                }

                if (group.Count >= MIN_MATCH_COUNT)
                {
                    matches.AddRange(group);
                }
                i = j;
            }
            return matches;
        }

        /// <summary>
        /// 특정 열에서 세로 매치를 찾음. 같은 타입의 타일이 3개 이상 연속하면 매치.
        /// </summary>
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
                if (currentTile == null || currentTile.isLocked)
                {
                    i++;
                    continue;
                }

                List<TileData> group = new List<TileData> { currentTile };
                int j = i + 1;
                while (j < rows && board[j, col] != null && !board[j, col].isLocked
                       && board[j, col].type == currentTile.type)
                {
                    group.Add(board[j, col]);
                    j++;
                }

                if (group.Count >= MIN_MATCH_COUNT)
                {
                    matches.AddRange(group);
                }
                i = j;
            }
            return matches;
        }

        /// <summary>
        /// 매치 그룹의 형태로 어떤 특수 아이템이 생성될지 결정.
        /// 우선순위: 5 L/T → Bomb, 5+ 직선 → Rainbow, 4 가로 → RocketHorizontal, 4 세로 → RocketVertical, 그 외 None.
        /// </summary>
        public static SpecialItemType GetSpecialItemType(List<TileData> matches)
        {
            if (matches == null || matches.Count < MIN_MATCH_COUNT)
            {
                return SpecialItemType.None;
            }

            int matchCount = matches.Count;

            if (matchCount == 5 && IsLOrTShape(matches))
            {
                return SpecialItemType.Bomb;
            }

            if (matchCount >= 5 && IsLinearMatch(matches))
            {
                return SpecialItemType.Rainbow;
            }

            if (matchCount == 4)
            {
                if (IsHorizontalMatch(matches)) return SpecialItemType.RocketHorizontal;
                if (IsVerticalMatch(matches))   return SpecialItemType.RocketVertical;
                if (Is2x2Match(matches))        return SpecialItemType.RocketHorizontal; // 정사각 → 가로 로켓 기본값
            }

            return SpecialItemType.None;
        }

        /// <summary>4타일이 2×2 정사각형 배치인지.</summary>
        public static bool Is2x2Match(List<TileData> tiles)
        {
            if (tiles == null || tiles.Count != 4) return false;
            int minX = int.MaxValue, maxX = int.MinValue;
            int minY = int.MaxValue, maxY = int.MinValue;
            foreach (TileData t in tiles)
            {
                if (t == null) return false;
                if (t.x < minX) minX = t.x;
                if (t.x > maxX) maxX = t.x;
                if (t.y < minY) minY = t.y;
                if (t.y > maxY) maxY = t.y;
            }
            return (maxX - minX == 1) && (maxY - minY == 1);
        }

        /// <summary>주어진 타일들이 같은 행에 정렬되어 있는지 (가로 매치).</summary>
        public static bool IsHorizontalMatch(List<TileData> tiles)
        {
            if (tiles == null || tiles.Count < MIN_MATCH_COUNT) return false;
            int firstY = tiles[0].y;
            foreach (TileData tile in tiles)
            {
                if (tile.y != firstY) return false;
            }
            return true;
        }

        /// <summary>주어진 타일들이 같은 열에 정렬되어 있는지 (세로 매치).</summary>
        public static bool IsVerticalMatch(List<TileData> tiles)
        {
            if (tiles == null || tiles.Count < MIN_MATCH_COUNT) return false;
            int firstX = tiles[0].x;
            foreach (TileData tile in tiles)
            {
                if (tile.x != firstX) return false;
            }
            return true;
        }

        /// <summary>두 타일을 스왑할 수 있는지 (인접 + 잠금/이동 중 아님).</summary>
        public static bool IsValidSwap(TileData tileA, TileData tileB)
        {
            if (tileA == null || tileB == null) return false;
            if (tileA.isLocked || tileB.isLocked) return false;
            if (tileA.isMoving || tileB.isMoving) return false;
            return tileA.IsAdjacentTo(tileB);
        }

        /// <summary>주어진 타일들이 일직선(가로 또는 세로)에 있는지.</summary>
        private static bool IsLinearMatch(List<TileData> tiles)
        {
            return IsHorizontalMatch(tiles) || IsVerticalMatch(tiles);
        }

        /// <summary>L 또는 T 모양인지 — 어떤 행에 3개 이상 + 어떤 열에도 3개 이상이면 교차점이 있음.</summary>
        private static bool IsLOrTShape(List<TileData> tiles)
        {
            if (tiles == null || tiles.Count != 5) return false;

            Dictionary<int, int> xGroups = new Dictionary<int, int>();
            Dictionary<int, int> yGroups = new Dictionary<int, int>();

            foreach (TileData tile in tiles)
            {
                if (!xGroups.ContainsKey(tile.x)) xGroups[tile.x] = 0;
                xGroups[tile.x]++;
                if (!yGroups.ContainsKey(tile.y)) yGroups[tile.y] = 0;
                yGroups[tile.y]++;
            }

            bool hasThreeInX = false;
            bool hasThreeInY = false;
            foreach (int count in xGroups.Values) if (count >= 3) hasThreeInX = true;
            foreach (int count in yGroups.Values) if (count >= 3) hasThreeInY = true;

            return hasThreeInX && hasThreeInY;
        }
    }
}
