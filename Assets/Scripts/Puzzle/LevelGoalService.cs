using System.Collections.Generic;
using UnityEngine;
using WhiskerTales.Core;

namespace WhiskerTales.Puzzle
{
    [System.Serializable]
    public sealed class LevelGoalEntry
    {
        public LevelGoalType type;
        public int target;
        public int current;
        public int tileType = -1;

        public bool IsComplete
        {
            get { return current >= target; }
        }
    }

    public sealed class LevelGoalService
    {
        private readonly List<LevelGoalEntry> goals = new List<LevelGoalEntry>();
        private int score;

        public IReadOnlyList<LevelGoalEntry> Goals
        {
            get { return goals; }
        }

        public int Score
        {
            get { return score; }
        }

        public void SetGoals(IEnumerable<LevelGoalEntry> newGoals)
        {
            goals.Clear();

            if (newGoals == null)
            {
                return;
            }

            foreach (LevelGoalEntry goal in newGoals)
            {
                if (goal != null)
                {
                    goals.Add(goal);
                }
            }
        }

        public void ApplyResolveResult(Match3ResolveResult result, IReadOnlyList<RemovedTileMeta> removedMeta)
        {
            if (result == null)
            {
                return;
            }

            int removedCount = 0;

            for (int i = 0; i < result.CascadeSteps.Count; i++)
            {
                if (result.CascadeSteps[i] != null)
                {
                    removedCount += result.CascadeSteps[i].RemovedTiles.Count;
                }
            }

            AddScore(removedCount * GameConstants.Currency.ScorePerTile);
            AddGoalProgress(LevelGoalType.RemoveBlocks, -1, removedCount);

            if (removedMeta != null)
            {
                for (int i = 0; i < removedMeta.Count; i++)
                {
                    RemovedTileMeta meta = removedMeta[i];
                    AddGoalProgress(LevelGoalType.CollectItems, meta.TileType, 1);

                    if (meta.WasObstacle == true)
                    {
                        AddGoalProgress(LevelGoalType.DestroyObstacles, -1, 1);
                    }

                    if (meta.HadJelly == true)
                    {
                        AddGoalProgress(LevelGoalType.ClearJelly, -1, 1);
                    }
                }
            }
        }

        public void AddScore(int amount)
        {
            score += Mathf.Max(0, amount);
            AddGoalProgress(LevelGoalType.ReachScore, -1, score, true);
        }

        public bool AreAllGoalsComplete()
        {
            if (goals.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < goals.Count; i++)
            {
                if (goals[i] != null && goals[i].IsComplete == false)
                {
                    return false;
                }
            }

            return true;
        }

        private void AddGoalProgress(LevelGoalType type, int tileType, int amount, bool setAbsolute = false)
        {
            for (int i = 0; i < goals.Count; i++)
            {
                LevelGoalEntry goal = goals[i];

                if (goal == null)
                {
                    continue;
                }

                if (goal.type != type)
                {
                    continue;
                }

                if (goal.tileType >= 0 && tileType >= 0 && goal.tileType != tileType)
                {
                    continue;
                }

                if (setAbsolute == true)
                {
                    goal.current = amount;
                }
                else
                {
                    goal.current += Mathf.Max(0, amount);
                }

                GameEvents.RaiseGoalUpdated(goal.current, goal.target);
            }
        }
    }

    public readonly struct RemovedTileMeta
    {
        public readonly int TileType;
        public readonly bool WasObstacle;
        public readonly bool HadJelly;

        public RemovedTileMeta(int tileType, bool wasObstacle, bool hadJelly)
        {
            TileType = tileType;
            WasObstacle = wasObstacle;
            HadJelly = hadJelly;
        }
    }
}
