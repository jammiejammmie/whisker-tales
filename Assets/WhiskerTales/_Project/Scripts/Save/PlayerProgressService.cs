using WhiskerTales.Core;
using WhiskerTales.Save;

namespace WhiskerTales.Save
{
    // Thin facade over SaveService for V2 level-unlock state. Stores nothing of its own — every
    // accessor reads/writes the live GameSaveData and persists immediately on mutation.
    // `progress.level` is treated as the highest *unlocked* level (1-indexed). Clearing level N
    // unlocks N+1.
    public static class PlayerProgressService
    {
        public const int MinLevelId = 1;

        public static int MaxUnlockedLevel
        {
            get
            {
                GameSaveData data = LoadOrDefault();

                if (data == null || data.progress == null)
                {
                    return MinLevelId;
                }

                if (data.progress.level < MinLevelId)
                {
                    return MinLevelId;
                }

                return data.progress.level;
            }
        }

        public static bool IsLevelUnlocked(int levelId)
        {
            return levelId >= MinLevelId && levelId <= MaxUnlockedLevel;
        }

        // Mark a level cleared. If clearing the current highest unlocked level, advance by one and
        // persist. Clearing replays of earlier levels is a no-op (no rollback).
        public static void MarkLevelCleared(int levelId)
        {
            SaveService save = SystemsBootstrap.SaveService;

            if (save == null)
            {
                DebugLogger.Warning(LogCategory.Save, "[PlayerProgressService] SaveService unavailable; cleared state not persisted.");
                return;
            }

            GameSaveData data = save.Load();

            if (data == null || data.progress == null)
            {
                return;
            }

            int previous = data.progress.level;

            if (levelId >= data.progress.level)
            {
                data.progress.level = levelId + 1;
                save.Save(data);
                DebugLogger.Info(LogCategory.Save, "[PlayerProgressService] level " + levelId + " cleared. unlocked: " + previous + " -> " + data.progress.level);
            }
            else
            {
                DebugLogger.Info(LogCategory.Save, "[PlayerProgressService] level " + levelId + " replayed (already cleared).");
            }
        }

        private static GameSaveData LoadOrDefault()
        {
            SaveService save = SystemsBootstrap.SaveService;

            if (save == null)
            {
                return GameSaveData.CreateDefault();
            }

            return save.Load();
        }
    }
}
