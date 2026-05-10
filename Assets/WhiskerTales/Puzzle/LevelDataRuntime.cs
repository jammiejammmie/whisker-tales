using System;
using System.Collections.Generic;
using UnityEngine;
using WhiskerTales.Core;

namespace WhiskerTales.Puzzle
{
    [Serializable]
    public sealed class LevelDataRuntime
    {
        public int id;
        public int moves;
        public string goalType;
        public int target;
        public int targetTileType;
        public int tileTypes;
        public string[] obstacles;
    }

    [Serializable]
    public sealed class LevelDataRuntimeCollection
    {
        public List<LevelDataRuntime> levels = new List<LevelDataRuntime>();
    }

    public sealed class LevelDataRuntimeLoader : MonoBehaviour
    {
        [SerializeField] private string resourcePath = "Levels/levels_001_010";

        public LevelDataRuntimeCollection Data { get; private set; }

        private void Awake()
        {
            Load();
        }

        public void Load()
        {
            TextAsset json = Resources.Load<TextAsset>(resourcePath);

            if (json == null)
            {
                DebugLogger.Warning(LogCategory.Puzzle, $"Level json not found: {resourcePath}");
                Data = new LevelDataRuntimeCollection();
                return;
            }

            Data = JsonUtility.FromJson<LevelDataRuntimeCollection>(json.text);

            if (Data == null)
            {
                DebugLogger.Warning(LogCategory.Puzzle, "Failed to parse level data json.");
                Data = new LevelDataRuntimeCollection();
                return;
            }

            DebugLogger.Info(LogCategory.Puzzle, $"Loaded levels: {Data.levels.Count}");
        }

        public LevelDataRuntime GetLevel(int id)
        {
            if (Data == null || Data.levels == null)
            {
                return null;
            }

            for (int i = 0; i < Data.levels.Count; i++)
            {
                LevelDataRuntime level = Data.levels[i];

                if (level != null && level.id == id)
                {
                    return level;
                }
            }

            return null;
        }
    }
}
