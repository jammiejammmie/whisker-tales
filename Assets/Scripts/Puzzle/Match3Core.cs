using System;
using System.Collections.Generic;

namespace WhiskerTales.Puzzle
{
    public sealed class Match3Core
    {
        public const int DefaultBoardSize = 8;

        private readonly int width;
        private readonly int height;
        private readonly int tileTypeCount;
        private readonly Random random;

        private readonly Cell[,] cells;

        public int Width => width;
        public int Height => height;

        public Match3Core(int width = DefaultBoardSize, int height = DefaultBoardSize, int tileTypeCount = 6, int seed = 0)
        {
            this.width = Math.Max(1, width);
            this.height = Math.Max(1, height);
            this.tileTypeCount = Math.Max(3, tileTypeCount);
            random = seed == 0 ? new Random() : new Random(seed);

            cells = new Cell[this.width, this.height];
            GenerateInitialBoard();
        }

        public Cell GetCell(int x, int y)
        {
            if (!IsInside(x, y))
            {
                return Cell.Empty;
            }

            return cells[x, y];
        }

        public void SetCell(int x, int y, int tileType, SpecialItemType special = SpecialItemType.None)
        {
            if (!IsInside(x, y))
            {
                return;
            }

            cells[x, y] = new Cell(tileType, special);
        }

        public bool TrySwapTiles(int x1, int y1, int x2, int y2, out Match3ResolveResult result)
        {
            result = new Match3ResolveResult();

            if (!IsInside(x1, y1) || !IsInside(x2, y2))
            {
                result.WasValidMove = false;
                result.Reason = "Swap position out of bounds.";
                return false;
            }

            if (!AreAdjacent(x1, y1, x2, y2))
            {
                result.WasValidMove = false;
                result.Reason = "Tiles are not adjacent.";
                return false;
            }

            Swap(x1, y1, x2, y2);

            List<MatchGroup> initialMatches = FindAllMatches();

            bool specialActivated =
                cells[x1, y1].Special != SpecialItemType.None ||
                cells[x2, y2].Special != SpecialItemType.None;

            if (initialMatches.Count == 0 && !specialActivated)
            {
                Swap(x1, y1, x2, y2);
                result.WasValidMove = false;
                result.Reason = "Swap produced no match.";
                return false;
            }

            result.WasValidMove = true;
            result.SwappedFrom = new BoardPos(x1, y1);
            result.SwappedTo = new BoardPos(x2, y2);

            ResolveBoard(initialMatches, result);

            return true;
        }

        public List<MatchGroup> FindAllMatches()
        {
            List<MatchGroup> groups = new List<MatchGroup>();

            // Horizontal
            for (int y = 0; y < height; y++)
            {
                int runStart = 0;

                for (int x = 1; x <= width; x++)
                {
                    bool ended =
                        x == width ||
                        cells[x, y].TileType != cells[runStart, y].TileType ||
                        cells[x, y].IsEmpty ||
                        cells[runStart, y].IsEmpty;

                    if (ended)
                    {
                        int length = x - runStart;

                        if (length >= 3 && !cells[runStart, y].IsEmpty)
                        {
                            MatchGroup group = new MatchGroup();
                            group.Direction = MatchDirection.Horizontal;

                            for (int i = runStart; i < x; i++)
                            {
                                group.Positions.Add(new BoardPos(i, y));
                            }

                            groups.Add(group);
                        }

                        runStart = x;
                    }
                }
            }

            // Vertical
            for (int x = 0; x < width; x++)
            {
                int runStart = 0;

                for (int y = 1; y <= height; y++)
                {
                    bool ended =
                        y == height ||
                        cells[x, y].TileType != cells[x, runStart].TileType ||
                        cells[x, y].IsEmpty ||
                        cells[x, runStart].IsEmpty;

                    if (ended)
                    {
                        int length = y - runStart;

                        if (length >= 3 && !cells[x, runStart].IsEmpty)
                        {
                            MatchGroup group = new MatchGroup();
                            group.Direction = MatchDirection.Vertical;

                            for (int i = runStart; i < y; i++)
                            {
                                group.Positions.Add(new BoardPos(x, i));
                            }

                            groups.Add(group);
                        }

                        runStart = y;
                    }
                }
            }

            return groups;
        }

        private void ResolveBoard(List<MatchGroup> firstMatches, Match3ResolveResult result)
        {
            int cascadeDepth = 0;
            List<MatchGroup> matches = firstMatches;

            while (matches.Count > 0)
            {
                cascadeDepth++;

                CascadeStep step = new CascadeStep();
                step.Depth = cascadeDepth;
                step.Matches.AddRange(matches);

                HashSet<BoardPos> removeSet = new HashSet<BoardPos>();

                foreach (MatchGroup match in matches)
                {
                    foreach (BoardPos pos in match.Positions)
                    {
                        removeSet.Add(pos);
                    }

                    SpecialItemType produced = GetSpecialFromMatch(match);

                    if (produced != SpecialItemType.None)
                    {
                        BoardPos survivor = match.Positions[match.Positions.Count / 2];
                        Cell survivorCell = cells[survivor.X, survivor.Y];

                        cells[survivor.X, survivor.Y] = new Cell(survivorCell.TileType, produced);
                        removeSet.Remove(survivor);

                        step.CreatedSpecials.Add(new CreatedSpecialTile(survivor, produced));
                    }
                }

                foreach (BoardPos pos in removeSet)
                {
                    if (!IsInside(pos.X, pos.Y))
                    {
                        continue;
                    }

                    step.RemovedTiles.Add(pos);
                    cells[pos.X, pos.Y] = Cell.Empty;
                }

                ApplyGravity(step);
                FillEmptyCells(step);

                result.CascadeSteps.Add(step);

                matches = FindAllMatches();
            }

            result.TotalCascadeDepth = cascadeDepth;
        }

        private void ApplyGravity(CascadeStep step)
        {
            for (int x = 0; x < width; x++)
            {
                int writeY = height - 1;

                for (int readY = height - 1; readY >= 0; readY--)
                {
                    if (cells[x, readY].IsEmpty)
                    {
                        continue;
                    }

                    if (writeY != readY)
                    {
                        cells[x, writeY] = cells[x, readY];
                        cells[x, readY] = Cell.Empty;

                        step.MovedTiles.Add(new TileMove(
                            new BoardPos(x, readY),
                            new BoardPos(x, writeY)
                        ));
                    }

                    writeY--;
                }
            }
        }

        private void FillEmptyCells(CascadeStep step)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (!cells[x, y].IsEmpty)
                    {
                        continue;
                    }

                    int tileType = random.Next(0, tileTypeCount);
                    cells[x, y] = new Cell(tileType, SpecialItemType.None);

                    step.SpawnedTiles.Add(new SpawnedTile(
                        new BoardPos(x, y),
                        tileType
                    ));
                }
            }
        }

        private SpecialItemType GetSpecialFromMatch(MatchGroup match)
        {
            int count = match.Positions.Count;

            if (count >= 5)
            {
                return SpecialItemType.Rainbow;
            }

            if (count == 4)
            {
                return match.Direction == MatchDirection.Horizontal
                    ? SpecialItemType.RocketHorizontal
                    : SpecialItemType.RocketVertical;
            }

            return SpecialItemType.None;
        }

        private void GenerateInitialBoard()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int tileType;
                    int safety = 0;

                    do
                    {
                        tileType = random.Next(0, tileTypeCount);
                        safety++;
                    }
                    while (WouldCreateInitialMatch(x, y, tileType) && safety < 100);

                    cells[x, y] = new Cell(tileType, SpecialItemType.None);
                }
            }
        }

        private bool WouldCreateInitialMatch(int x, int y, int tileType)
        {
            if (x >= 2)
            {
                if (cells[x - 1, y].TileType == tileType &&
                    cells[x - 2, y].TileType == tileType)
                {
                    return true;
                }
            }

            if (y >= 2)
            {
                if (cells[x, y - 1].TileType == tileType &&
                    cells[x, y - 2].TileType == tileType)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsInside(int x, int y)
        {
            return x >= 0 && x < width && y >= 0 && y < height;
        }

        private bool AreAdjacent(int x1, int y1, int x2, int y2)
        {
            int dx = Math.Abs(x1 - x2);
            int dy = Math.Abs(y1 - y2);

            return dx + dy == 1;
        }

        private void Swap(int x1, int y1, int x2, int y2)
        {
            Cell temp = cells[x1, y1];
            cells[x1, y1] = cells[x2, y2];
            cells[x2, y2] = temp;
        }
    }

    public readonly struct Cell
    {
        public static readonly Cell Empty = new Cell(-1, SpecialItemType.None);

        public readonly int TileType;
        public readonly SpecialItemType Special;

        public bool IsEmpty => TileType < 0;

        public Cell(int tileType, SpecialItemType special)
        {
            TileType = tileType;
            Special = special;
        }
    }

    public readonly struct BoardPos
    {
        public readonly int X;
        public readonly int Y;

        public BoardPos(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    public enum MatchDirection
    {
        Horizontal,
        Vertical
    }

    public sealed class MatchGroup
    {
        public MatchDirection Direction;
        public readonly List<BoardPos> Positions = new List<BoardPos>();
    }

    public sealed class Match3ResolveResult
    {
        public bool WasValidMove;
        public string Reason;
        public BoardPos SwappedFrom;
        public BoardPos SwappedTo;
        public int TotalCascadeDepth;

        public readonly List<CascadeStep> CascadeSteps = new List<CascadeStep>();
    }

    public sealed class CascadeStep
    {
        public int Depth;

        public readonly List<MatchGroup> Matches = new List<MatchGroup>();
        public readonly List<BoardPos> RemovedTiles = new List<BoardPos>();
        public readonly List<TileMove> MovedTiles = new List<TileMove>();
        public readonly List<SpawnedTile> SpawnedTiles = new List<SpawnedTile>();
        public readonly List<CreatedSpecialTile> CreatedSpecials = new List<CreatedSpecialTile>();
    }

    public readonly struct TileMove
    {
        public readonly BoardPos From;
        public readonly BoardPos To;

        public TileMove(BoardPos from, BoardPos to)
        {
            From = from;
            To = to;
        }
    }

    public readonly struct SpawnedTile
    {
        public readonly BoardPos Position;
        public readonly int TileType;

        public SpawnedTile(BoardPos position, int tileType)
        {
            Position = position;
            TileType = tileType;
        }
    }

    public readonly struct CreatedSpecialTile
    {
        public readonly BoardPos Position;
        public readonly SpecialItemType Type;

        public CreatedSpecialTile(BoardPos position, SpecialItemType type)
        {
            Position = position;
            Type = type;
        }
    }
}
