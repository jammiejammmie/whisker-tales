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
