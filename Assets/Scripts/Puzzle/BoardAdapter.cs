using System;
using WhiskerTales.Core;

namespace WhiskerTales.Puzzle
{
    public sealed class BoardAdapter
    {
        private Match3Core core;

        public Match3Core Core => core;

        public int Width => core != null ? core.Width : GameConstants.Board.Size;
        public int Height => core != null ? core.Height : GameConstants.Board.Size;

        public void Initialize(int width, int height, int tileTypeCount, int seed = 0)
        {
            core = new Match3Core(width, height, tileTypeCount, seed);

            DebugLogger.Info(
                LogCategory.Puzzle,
                $"BoardAdapter initialized. Size={width}x{height}, TileTypes={tileTypeCount}"
            );
        }

        public bool TrySwapTiles(int x1, int y1, int x2, int y2, out Match3ResolveResult result)
        {
            result = null;

            if (core == null)
            {
                DebugLogger.Warning(LogCategory.Puzzle, "BoardAdapter.TrySwapTiles called before Initialize.");
                return false;
            }

            if (!IsInside(x1, y1) || !IsInside(x2, y2))
            {
                DebugLogger.Warning(
                    LogCategory.Puzzle,
                    $"Swap rejected. Out of bounds: ({x1},{y1}) -> ({x2},{y2})"
                );
                return false;
            }

            bool success = core.TrySwapTiles(x1, y1, x2, y2, out result);

            if (!success)
            {
                DebugLogger.Info(LogCategory.Puzzle, $"Swap failed: {result?.Reason}");
                return false;
            }

            GameEvents.RaiseTileSwapped(x1, y1, x2, y2);

            if (result != null)
            {
                DispatchResolveEvents(result);
            }

            return true;
        }

        public int GetTileType(int x, int y)
        {
            if (core == null)
            {
                return -1;
            }

            if (!IsInside(x, y))
            {
                return -1;
            }

            return core.GetCell(x, y).TileType;
        }

        public SpecialItemType GetSpecialItemType(int x, int y)
        {
            if (core == null)
            {
                return SpecialItemType.None;
            }

            if (!IsInside(x, y))
            {
                return SpecialItemType.None;
            }

            return core.GetCell(x, y).Special;
        }

        public void SetCell(int x, int y, int tileType, SpecialItemType special = SpecialItemType.None)
        {
            if (core == null)
            {
                DebugLogger.Warning(LogCategory.Puzzle, "SetCell called before Initialize.");
                return;
            }

            if (!IsInside(x, y))
            {
                DebugLogger.Warning(LogCategory.Puzzle, $"SetCell out of bounds: ({x},{y})");
                return;
            }

            core.SetCell(x, y, tileType, special);
        }

        public Cell GetCell(int x, int y)
        {
            if (core == null)
            {
                return Cell.Empty;
            }

            if (!IsInside(x, y))
            {
                return Cell.Empty;
            }

            return core.GetCell(x, y);
        }

        private void DispatchResolveEvents(Match3ResolveResult result)
        {
            if (result == null)
            {
                return;
            }

            foreach (CascadeStep step in result.CascadeSteps)
            {
                GameEvents.RaiseCascadeStarted(step.Depth);

                int matchedCount = 0;

                foreach (MatchGroup group in step.Matches)
                {
                    if (group == null || group.Positions == null)
                    {
                        continue;
                    }

                    matchedCount += group.Positions.Count;
                }

                if (matchedCount > 0)
                {
                    GameEvents.RaiseMatchFound(matchedCount);
                }

                foreach (CreatedSpecialTile created in step.CreatedSpecials)
                {
                    if (created.Type != SpecialItemType.None)
                    {
                        GameEvents.RaiseSpecialTileCreated(created.Type);
                    }
                }
            }

            GameEvents.RaiseCascadeEnded(result.TotalCascadeDepth);
        }

        private bool IsInside(int x, int y)
        {
            if (core == null)
            {
                return false;
            }

            return x >= 0 && x < core.Width && y >= 0 && y < core.Height;
        }
    }
}
