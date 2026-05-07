using UnityEngine;
using System.Collections.Generic;

namespace WhiskerTales.Puzzle
{
    /// <summary>
    /// 레벨 목표 관리 및 진행도 추적
    /// </summary>
    public class LevelGoal : MonoBehaviour
    {
        public LevelGoalType goalType { get; private set; }
        public int goalValue { get; private set; }
        public int currentProgress { get; private set; }
        public int moveLimit { get; private set; }
        public int movesUsed { get; private set; }

        private Level levelData;

        // 이벤트
        public delegate void ProgressChangedHandler(int newProgress);
        public event ProgressChangedHandler OnProgressChanged;

        public delegate void MovesChangedHandler(int movesUsed, int moveLimit);
        public event MovesChangedHandler OnMovesChanged;

        public delegate void GoalAchievedHandler();
        public event GoalAchievedHandler OnGoalAchieved;

        public delegate void MovesExceededHandler();
        public event MovesExceededHandler OnMovesExceeded;

        /// <summary>
        /// 레벨 목표 초기화
        /// </summary>
        public void Initialize(Level level)
        {
            levelData = level;
            goalType = level.goalType;
            goalValue = level.goalValue;
            moveLimit = level.moveLimit;
            currentProgress = 0;
            movesUsed = 0;

            Debug.Log($"[LevelGoal] Initialized - Type: {goalType}, Goal: {goalValue}, Moves: {moveLimit}");
        }

        /// <summary>
        /// 이동 사용 (매치 발생 시)
        /// </summary>
        public void UseMove()
        {
            if (movesUsed < moveLimit)
            {
                movesUsed++;
                OnMovesChanged?.Invoke(movesUsed, moveLimit);
                Debug.Log($"[LevelGoal] Move used: {movesUsed}/{moveLimit}");

                if (movesUsed >= moveLimit)
                {
                    OnMovesExceeded?.Invoke();
                    Debug.LogWarning("[LevelGoal] Moves exceeded!");
                }
            }
        }

        /// <summary>
        /// 목표 달성 여부 확인
        /// </summary>
        public bool IsGoalAchieved()
        {
            bool achieved = currentProgress >= goalValue;

            if (achieved)
            {
                OnGoalAchieved?.Invoke();
                Debug.Log("[LevelGoal] Goal achieved!");
            }

            return achieved;
        }

        /// <summary>
        /// 이동 제한 초과 여부 확인
        /// </summary>
        public bool IsMovesExceeded()
        {
            return movesUsed >= moveLimit;
        }

        /// <summary>
        /// 진행도 백분율 반환 (0~100)
        /// </summary>
        public float GetProgressPercentage()
        {
            if (goalValue == 0)
                return 0f;

            return (currentProgress / (float)goalValue) * 100f;
        }

        /// <summary>
        /// 남은 이동 수 반환
        /// </summary>
        public int GetRemainingMoves()
        {
            return Mathf.Max(0, moveLimit - movesUsed);
        }

        /// <summary>
        /// 목표 설명 반환
        /// </summary>
        public string GetGoalDescription()
        {
            return goalType switch
            {
                LevelGoalType.RemoveBlocks => $"블록 {goalValue}개 제거",
                LevelGoalType.CollectItems => $"아이템 {goalValue}개 수집",
                LevelGoalType.ReachScore => $"점수 {goalValue} 달성",
                LevelGoalType.DestroyObstacles => $"장애물 {goalValue}개 제거",
                _ => "알 수 없는 목표"
            };
        }

        /// <summary>
        /// 현재 진행도 설명 반환
        /// </summary>
        public string GetProgressDescription()
        {
            return $"{currentProgress}/{goalValue}";
        }

        /// <summary>
        /// 별 개수 계산 (점수 기반)
        /// </summary>
        public int CalculateStars()
        {
            float percentage = GetProgressPercentage();

            if (percentage >= 100f)
                return 3;
            else if (percentage >= 70f)
                return 2;
            else if (percentage >= 50f)
                return 1;
            else
                return 0;
        }

        /// <summary>
        /// 진행도 업데이트 (제거된 TileData 기반)
        /// </summary>
        public void UpdateProgress(List<TileData> removedTiles)
        {
            if (removedTiles == null || removedTiles.Count == 0)
                return;

            int previousProgress = currentProgress;

            switch (goalType)
            {
                case LevelGoalType.RemoveBlocks:
                    currentProgress += removedTiles.Count;
                    break;

                case LevelGoalType.CollectItems:
                    foreach (TileData tile in removedTiles)
                    {
                        if (tile.specialItem != SpecialItemType.None)
                            currentProgress++;
                    }
                    break;

                case LevelGoalType.ReachScore:
                    currentProgress += removedTiles.Count * 100;
                    break;

                case LevelGoalType.DestroyObstacles:
                    foreach (TileData tile in removedTiles)
                    {
                        if (tile.obstacle != ObstacleType.None && tile.obstacleHealth > 0)
                            currentProgress++;
                    }
                    break;
            }

            if (currentProgress > goalValue)
                currentProgress = goalValue;

            if (currentProgress != previousProgress)
            {
                OnProgressChanged?.Invoke(currentProgress);
                Debug.Log($"[LevelGoal] Progress updated (TileData): {currentProgress}/{goalValue}");
            }
        }

        /// <summary>
        /// 레벨 리셋
        /// </summary>
        public void Reset()
        {
            currentProgress = 0;
            movesUsed = 0;
            Debug.Log("[LevelGoal] Reset");
        }
    }
}

