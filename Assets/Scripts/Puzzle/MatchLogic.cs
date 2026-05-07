using System.Collections.Generic;
using UnityEngine;

namespace WhiskerTales.Puzzle
{
    /// <summary>
    /// 留ㅼ튂-3 寃뚯엫??留ㅼ튂 ?먯젙 諛?遺꾩꽍 濡쒖쭅???대떦?섎뒗 static ?대옒??
    /// 蹂대뱶 ?곸쓽 留ㅼ튂瑜?李얘퀬, ?뱀닔 ?꾩씠???앹꽦 ?щ?瑜??먯젙??
    /// MonoBehaviour瑜??곸냽?섏? ?딆쓬 (?쒖닔 濡쒖쭅)
    /// </summary>
    public static class MatchLogic
    {
        /// <summary>
        /// 理쒖냼 留ㅼ튂 媛쒖닔 (3媛??댁긽?댁뼱??留ㅼ튂濡??몄젙)
        /// </summary>
        private const int MIN_MATCH_COUNT = 3;

        /// <summary>
        /// 蹂대뱶 ?꾩껜?먯꽌 紐⑤뱺 留ㅼ튂瑜?李얠쓬
        /// </summary>
        /// <param name="board">8횞8 ????곗씠??諛곗뿴</param>
        /// <returns>留ㅼ튂????쇰뱾??由ъ뒪??(媛?洹몃９蹂꾨줈 遺꾨━)</returns>
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

            // ?대? 留ㅼ튂????쇱? 以묐났 泥섎━?섏? ?딄린 ?꾪빐 異붿쟻
            bool[,] matched = new bool[rows, cols];

            // 媛濡?留ㅼ튂 李얘린
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

            // ?몃줈 留ㅼ튂 李얘린
            for (int x = 0; x < cols; x++)
            {
                List<TileData> colMatches = FindMatchesInColumn(board, x);
                // ?대? 媛濡쒖뿉??留ㅼ튂????쇱? ?쒖쇅
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
        /// ?뱀젙 ??row)?먯꽌 媛濡?留ㅼ튂瑜?李얠쓬
        /// 媛숈? ??낆쓽 ??쇱씠 3媛??댁긽 ?곗냽?쇰줈 ?덉쑝硫?留ㅼ튂
        /// </summary>
        /// <param name="board">8횞8 ????곗씠??諛곗뿴</param>
        /// <param name="row">寃?ы븷 ??踰덊샇 (0~7)</param>
        /// <returns>留ㅼ튂????쇰뱾??由ъ뒪??/returns>
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

                // 鍮?移몄씠嫄곕굹 ?좉릿 ??쇱? 嫄대꼫?곌린
                if (currentTile == null || currentTile.isLocked)
                {
                    i++;
                    continue;
                }

                // 媛숈? ??낆쓽 ?곗냽?????李얘린
                List<TileData> group = new List<TileData> { currentTile };
                int j = i + 1;

                while (j < cols && board[row, j] != null && board[row, j].type == currentTile.type)
                {
                    group.Add(board[row, j]);
                    j++;
                }

                // 3媛??댁긽?대㈃ 留ㅼ튂
                if (group.Count >= MIN_MATCH_COUNT)
                {
                    matches.AddRange(group);
                }

                i = j;
            }

            return matches;
        }

        /// <summary>
        /// ?뱀젙 ??column)?먯꽌 ?몃줈 留ㅼ튂瑜?李얠쓬
        /// 媛숈? ??낆쓽 ??쇱씠 3媛??댁긽 ?곗냽?쇰줈 ?덉쑝硫?留ㅼ튂
        /// </summary>
        /// <param name="board">8횞8 ????곗씠??諛곗뿴</param>
        /// <param name="col">寃?ы븷 ??踰덊샇 (0~7)</param>
        /// <returns>留ㅼ튂????쇰뱾??由ъ뒪??/returns>
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

                // 鍮?移몄씠嫄곕굹 ?좉릿 ??쇱? 嫄대꼫?곌린
                if (currentTile == null || currentTile.isLocked)
                {
                    i++;
                    continue;
                }

                // 媛숈? ??낆쓽 ?곗냽?????李얘린
                List<TileData> group = new List<TileData> { currentTile };
                int j = i + 1;

                while (j < rows && board[j, col] != null && board[j, col].type == currentTile.type)
                {
                    group.Add(board[j, col]);
                    j++;
                }

                // 3媛??댁긽?대㈃ 留ㅼ튂
                if (group.Count >= MIN_MATCH_COUNT)
                {
                    matches.AddRange(group);
                }

                i = j;
            }

            return matches;
        }

        /// <summary>
        /// 留ㅼ튂????쇰뱾??援ъ꽦??遺꾩꽍?섏뿬 ?앹꽦???뱀닔 ?꾩씠?쒖쓣 ?먯젙
        /// </summary>
        /// <param name="matches">留ㅼ튂????쇰뱾??由ъ뒪??/param>
        /// <returns>?앹꽦???뱀닔 ?꾩씠?????/returns>
        public static SpecialItemType GetSpecialItemType(List<TileData> matches)
        {
            if (matches == null || matches.Count < MIN_MATCH_COUNT)
            {
                return SpecialItemType.None;
            }

            int matchCount = matches.Count;

            // 4媛?吏곸꽑 留ㅼ튂 ??濡쒖폆 ?앹꽦
            if (matchCount == 4 && IsLinearMatch(matches))
            {
                return SpecialItemType.Rocket;
            }

            // 5媛?L/T ?뺥깭 留ㅼ튂 ????깂 ?앹꽦
            if (matchCount == 5 && IsLOrTShape(matches))
            {
                return SpecialItemType.Bomb;
            }

            // 5媛?吏곸꽑 留ㅼ튂 ??臾댁?媛??몄떎 ?앹꽦
            if (matchCount >= 5 && IsLinearMatch(matches))
            {
                return SpecialItemType.Rainbow;
            }

            // 湲곕낯 3媛?留ㅼ튂 ???뱀닔 ?꾩씠???놁쓬
            return SpecialItemType.None;
        }

        /// <summary>
        /// ????쇱쓣 ?ㅼ솑?????덈뒗吏 ?먯젙
        /// ?몄젒????쇰쭔 ?ㅼ솑 媛?ν븯怨? ?좉릿 ??쇱? ?ㅼ솑 遺덇???
        /// </summary>
        /// <param name="tileA">泥?踰덉㎏ ???/param>
        /// <param name="tileB">??踰덉㎏ ???/param>
        /// <returns>?ㅼ솑 媛?ν븯硫?true, 遺덇??ν븯硫?false</returns>
        public static bool IsValidSwap(TileData tileA, TileData tileB)
        {
            if (tileA == null || tileB == null)
            {
                return false;
            }

            // ?좉릿 ??쇱? ?ㅼ솑 遺덇???
            if (tileA.isLocked || tileB.isLocked)
            {
                return false;
            }

            // ?대룞 以묒씤 ??쇱? ?ㅼ솑 遺덇???
            if (tileA.isMoving || tileB.isMoving)
            {
                return false;
            }

            // ?몄젒?섏? ?딆쑝硫??ㅼ솑 遺덇???
            return tileA.IsAdjacentTo(tileB);
        }

        /// <summary>
        /// 二쇱뼱吏???쇰뱾???쇱쭅??媛濡??먮뒗 ?몃줈)???대（?붿? ?먯젙
        /// </summary>
        /// <param name="tiles">??쇰뱾??由ъ뒪??/param>
        /// <returns>?쇱쭅?좎씠硫?true, ?꾨땲硫?false</returns>
        private static bool IsLinearMatch(List<TileData> tiles)
        {
            if (tiles == null || tiles.Count < MIN_MATCH_COUNT)
                return false;

            // 紐⑤몢 媛숈? X 醫뚰몴 (?몃줈 ?쇱쭅??
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

            // 紐⑤몢 媛숈? Y 醫뚰몴 (媛濡??쇱쭅??
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
        /// 二쇱뼱吏???쇰뱾??L ?먮뒗 T ?뺥깭瑜??대（?붿? ?먯젙
        /// </summary>
        /// <param name="tiles">??쇰뱾??由ъ뒪??/param>
        /// <returns>L ?먮뒗 T ?뺥깭硫?true, ?꾨땲硫?false</returns>
        private static bool IsLOrTShape(List<TileData> tiles)
        {
            if (tiles == null || tiles.Count != 5)
                return false;

            // L ?먮뒗 T ?뺥깭: ??異뺤뿉 3媛? ?ㅻⅨ 異뺤뿉 2媛??댁긽 (援먯쭛??1媛?
            // ?? (1,1), (1,2), (1,3), (2,3), (3,3) ??L???뺥깭

            // X 醫뚰몴蹂?洹몃９??
            Dictionary<int, int> xGroups = new Dictionary<int, int>();
            foreach (TileData tile in tiles)
            {
                if (!xGroups.ContainsKey(tile.x))
                    xGroups[tile.x] = 0;
                xGroups[tile.x]++;
            }

            // Y 醫뚰몴蹂?洹몃９??
            Dictionary<int, int> yGroups = new Dictionary<int, int>();
            foreach (TileData tile in tiles)
            {
                if (!yGroups.ContainsKey(tile.y))
                    yGroups[tile.y] = 0;
                yGroups[tile.y]++;
            }

            // L/T ?뺥깭: ??異뺤뿉 3媛? ?ㅻⅨ 異뺤뿉 3媛??댁긽 (援먯쭛???덉쓬)
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
