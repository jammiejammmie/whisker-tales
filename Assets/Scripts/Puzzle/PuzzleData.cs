namespace WhiskerTales.Puzzle
{
    /// <summary>
    /// 레벨 목표 타입
    /// </summary>
    public enum LevelGoalType
    {
        RemoveBlocks,
        CollectItems,
        ReachScore,
        DestroyObstacles
    }

    /// <summary>
    /// 레벨 데이터 (Puzzle 내부 자족 정의)
    /// 외부 데이터 모듈이 도입되면 이 클래스를 그쪽으로 이전 가능
    /// </summary>
    [System.Serializable]
    public class Level
    {
        public int levelId;
        public int moveLimit = 25;
        public LevelGoalType goalType = LevelGoalType.RemoveBlocks;
        public int goalValue = 50;
    }
}
