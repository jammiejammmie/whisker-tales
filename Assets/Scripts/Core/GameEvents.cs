using System;
using WhiskerTales.Puzzle;
using WhiskerTales.Sleep;

namespace WhiskerTales.Core
{
    /// <summary>
    /// Central event bus for Whisker Tales.
    /// Phase 1 stabilization: existing direct calls stay; systems can additionally raise/listen here.
    /// </summary>
    public static class GameEvents
    {
        public static event Action<int> OnLevelStarted;
        public static event Action<int, int> OnLevelCompleted;
        public static event Action<int> OnLevelFailed;
        public static event Action<int, int, int, int> OnTileSwapped;
        public static event Action<int> OnMatchFound;
        public static event Action<SpecialItemType> OnSpecialTileCreated;
        public static event Action<int> OnCascadeStarted;
        public static event Action<int> OnCascadeEnded;
        public static event Action<int, int> OnGoalUpdated;
        public static event Action<int> OnCoinEarned;
        public static event Action<int> OnHeartChanged;
        public static event Action<int, int> OnCatAffinityChanged;
        public static event Action OnSleepModeEntered;
        public static event Action<SleepModeManager.SleepReward> OnSleepModeExited;
        public static event Action OnDetoxModalShown;
        public static event Action<string> OnBoosterUsed;

        public static void RaiseLevelStarted(int level) => SafeInvoke(() => OnLevelStarted?.Invoke(level), nameof(OnLevelStarted));
        public static void RaiseLevelCompleted(int level, int stars) => SafeInvoke(() => OnLevelCompleted?.Invoke(level, stars), nameof(OnLevelCompleted));
        public static void RaiseLevelFailed(int level) => SafeInvoke(() => OnLevelFailed?.Invoke(level), nameof(OnLevelFailed));
        public static void RaiseTileSwapped(int x1, int y1, int x2, int y2) => SafeInvoke(() => OnTileSwapped?.Invoke(x1, y1, x2, y2), nameof(OnTileSwapped));
        public static void RaiseMatchFound(int count) => SafeInvoke(() => OnMatchFound?.Invoke(count), nameof(OnMatchFound));
        public static void RaiseSpecialTileCreated(SpecialItemType type) => SafeInvoke(() => OnSpecialTileCreated?.Invoke(type), nameof(OnSpecialTileCreated));
        public static void RaiseCascadeStarted(int depth) => SafeInvoke(() => OnCascadeStarted?.Invoke(depth), nameof(OnCascadeStarted));
        public static void RaiseCascadeEnded(int totalDepth) => SafeInvoke(() => OnCascadeEnded?.Invoke(totalDepth), nameof(OnCascadeEnded));
        public static void RaiseGoalUpdated(int current, int target) => SafeInvoke(() => OnGoalUpdated?.Invoke(current, target), nameof(OnGoalUpdated));
        public static void RaiseCoinEarned(int amount) => SafeInvoke(() => OnCoinEarned?.Invoke(amount), nameof(OnCoinEarned));
        public static void RaiseHeartChanged(int current) => SafeInvoke(() => OnHeartChanged?.Invoke(current), nameof(OnHeartChanged));
        public static void RaiseCatAffinityChanged(int catId, int amount) => SafeInvoke(() => OnCatAffinityChanged?.Invoke(catId, amount), nameof(OnCatAffinityChanged));
        public static void RaiseSleepModeEntered() => SafeInvoke(() => OnSleepModeEntered?.Invoke(), nameof(OnSleepModeEntered));
        public static void RaiseSleepModeExited(SleepModeManager.SleepReward reward) => SafeInvoke(() => OnSleepModeExited?.Invoke(reward), nameof(OnSleepModeExited));
        public static void RaiseDetoxModalShown() => SafeInvoke(() => OnDetoxModalShown?.Invoke(), nameof(OnDetoxModalShown));
        public static void RaiseBoosterUsed(string boosterType) => SafeInvoke(() => OnBoosterUsed?.Invoke(boosterType), nameof(OnBoosterUsed));

        private static void SafeInvoke(Action invoke, string eventName)
        {
            try
            {
                invoke?.Invoke();
            }
            catch (Exception ex)
            {
                DebugLogger.Exception(LogCategory.Analytics, new Exception($"GameEvents listener failed: {eventName}", ex));
            }
        }
    }
}
