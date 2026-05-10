using System;
using System.Reflection;
using UnityEngine;
using WhiskerTales.Core;

namespace WhiskerTales.Puzzle
{
    public static class BoardExtensions
    {
        public static int GetTileType(this Board board, int x, int y)
        {
            if (board == null)
            {
                return -1;
            }

            TileData tileData = board.GetTile(x, y);
            return tileData != null ? (int)tileData.type : -1;
        }

        public static void ShuffleBoard(this Board board)
        {
            if (board == null)
            {
                DebugLogger.Warning(LogCategory.Puzzle, "ShuffleBoard ignored null Board.");
                return;
            }

            FieldInfo field = typeof(Board).GetField("board", BindingFlags.Instance | BindingFlags.NonPublic);

            if (field == null)
            {
                DebugLogger.Warning(LogCategory.Puzzle, "ShuffleBoard could not find private board field.");
                return;
            }

            TileData[,] tiles = field.GetValue(board) as TileData[,];

            if (tiles == null)
            {
                DebugLogger.Warning(LogCategory.Puzzle, "ShuffleBoard found null tile array.");
                return;
            }

            int height = tiles.GetLength(0);
            int width = tiles.GetLength(1);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int rx = UnityEngine.Random.Range(0, width);
                    int ry = UnityEngine.Random.Range(0, height);
                    TileData a = tiles[y, x];
                    TileData b = tiles[ry, rx];
                    tiles[y, x] = b;
                    tiles[ry, rx] = a;

                    if (tiles[y, x] != null)
                    {
                        tiles[y, x].x = x;
                        tiles[y, x].y = y;
                    }

                    if (tiles[ry, rx] != null)
                    {
                        tiles[ry, rx].x = rx;
                        tiles[ry, rx].y = ry;
                    }
                }
            }

            PushToAdapterIfPresent(board, tiles, width, height);
        }

        private static void PushToAdapterIfPresent(Board board, TileData[,] tiles, int width, int height)
        {
            FieldInfo adapterField = typeof(Board).GetField("adapter", BindingFlags.Instance | BindingFlags.NonPublic);

            if (adapterField == null)
            {
                return;
            }

            object adapter = adapterField.GetValue(board);

            if (adapter == null)
            {
                return;
            }

            MethodInfo setCell = adapter.GetType().GetMethod("SetCell", new[] { typeof(int), typeof(int), typeof(int), typeof(SpecialItemType) });

            if (setCell == null)
            {
                return;
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    TileData tile = tiles[y, x];
                    int type = tile != null ? (int)tile.type : -1;
                    SpecialItemType special = tile != null ? tile.specialItem : SpecialItemType.None;
                    setCell.Invoke(adapter, new object[] { x, y, type, special });
                }
            }
        }
    }
}
