using System.Collections.Generic;
using UnityEngine;

using WhiskerTales.Core;
namespace WhiskerTales.Puzzle
{
    public sealed class SpecialActivator
    {
        private readonly BoardAdapter adapter;

        public SpecialActivator(BoardAdapter boardAdapter)
        {
            adapter = boardAdapter;
        }

        public List<BoardPos> Activate(BoardPos origin, SpecialItemType type, int targetTileType = -1, SpecialItemType comboType = SpecialItemType.None)
        {
            List<BoardPos> affected = new List<BoardPos>();

            if (adapter == null)
            {
                DebugLogger.Warning(LogCategory.Puzzle, "SpecialActivator has null adapter.");
                return affected;
            }

            if (comboType != SpecialItemType.None)
            {
                return ActivateCombo(origin, type, comboType, targetTileType);
            }

            if (type == SpecialItemType.RocketHorizontal)
            {
                AddRow(origin.Y, affected);
            }
            else if (type == SpecialItemType.RocketVertical)
            {
                AddColumn(origin.X, affected);
            }
            else if (type == SpecialItemType.Bomb)
            {
                AddSquare(origin, 1, affected);
            }
            else if (type == SpecialItemType.Rainbow)
            {
                AddAllOfType(targetTileType, affected);
            }

            RemoveAffected(affected);
            return affected;
        }

        private List<BoardPos> ActivateCombo(BoardPos origin, SpecialItemType a, SpecialItemType b, int targetTileType)
        {
            List<BoardPos> affected = new List<BoardPos>();

            if (a == SpecialItemType.Rainbow || b == SpecialItemType.Rainbow)
            {
                AddAllOfType(targetTileType, affected);
            }
            else if (a == SpecialItemType.Bomb || b == SpecialItemType.Bomb)
            {
                AddSquare(origin, 2, affected);
            }
            else
            {
                AddRow(origin.Y, affected);
                AddColumn(origin.X, affected);
            }

            RemoveAffected(affected);
            return affected;
        }

        private void AddRow(int y, List<BoardPos> affected)
        {
            for (int x = 0; x < GameConstants.Board.Size; x++)
            {
                AddUnique(new BoardPos(x, y), affected);
            }
        }

        private void AddColumn(int x, List<BoardPos> affected)
        {
            for (int y = 0; y < GameConstants.Board.Size; y++)
            {
                AddUnique(new BoardPos(x, y), affected);
            }
        }

        private void AddSquare(BoardPos center, int radius, List<BoardPos> affected)
        {
            for (int x = center.X - radius; x <= center.X + radius; x++)
            {
                for (int y = center.Y - radius; y <= center.Y + radius; y++)
                {
                    if (IsInside(x, y) == true)
                    {
                        AddUnique(new BoardPos(x, y), affected);
                    }
                }
            }
        }

        private void AddAllOfType(int tileType, List<BoardPos> affected)
        {
            if (tileType < 0)
            {
                DebugLogger.Warning(LogCategory.Puzzle, "Rainbow activation received invalid target type.");
                return;
            }

            for (int x = 0; x < GameConstants.Board.Size; x++)
            {
                for (int y = 0; y < GameConstants.Board.Size; y++)
                {
                    if (adapter.GetTileType(x, y) == tileType)
                    {
                        AddUnique(new BoardPos(x, y), affected);
                    }
                }
            }
        }

        private void RemoveAffected(List<BoardPos> affected)
        {
            if (affected == null)
            {
                return;
            }

            for (int i = 0; i < affected.Count; i++)
            {
                BoardPos pos = affected[i];

                if (IsInside(pos.X, pos.Y) == true)
                {
                    adapter.SetCell(pos.X, pos.Y, -1, SpecialItemType.None);
                }
            }

            if (affected.Count > 0)
            {
                GameEvents.RaiseMatchFound(affected.Count);
            }
        }

        private void AddUnique(BoardPos pos, List<BoardPos> affected)
        {
            if (affected == null)
            {
                return;
            }

            for (int i = 0; i < affected.Count; i++)
            {
                if (affected[i].X == pos.X && affected[i].Y == pos.Y)
                {
                    return;
                }
            }

            affected.Add(pos);
        }

        private bool IsInside(int x, int y)
        {
            return x >= 0 && x < GameConstants.Board.Size && y >= 0 && y < GameConstants.Board.Size;
        }
    }
}
