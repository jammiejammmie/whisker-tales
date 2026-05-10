using WhiskerTales.Core;
namespace WhiskerTales.Puzzle
{
    public enum LevelGoalType
    {
        RemoveBlocks,
        CollectItems,
        ReachScore,
        DestroyObstacles,
        ClearJelly
    }

    [System.Serializable]
    public class Level
    {
        public int levelId;
        public int moveLimit = 25;
        public LevelGoalType goalType = LevelGoalType.RemoveBlocks;
        public int goalValue = 50;
    }
}
