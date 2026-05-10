using System;
using System.Collections.Generic;
using UnityEngine;
using WhiskerTales.Core;

namespace WhiskerTales.UI
{
    public sealed class UIAssetRegistryRuntime : MonoBehaviour
    {
        [Serializable]
        public sealed class SpriteEntry
        {
            public string key;
            public Sprite sprite;
        }

        public static UIAssetRegistryRuntime Instance { get; private set; }

        [SerializeField] private List<SpriteEntry> sprites = new List<SpriteEntry>();

        private readonly Dictionary<string, Sprite> spriteMap = new Dictionary<string, Sprite>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            BuildMap();
        }

        private void BuildMap()
        {
            spriteMap.Clear();

            for (int i = 0; i < sprites.Count; i++)
            {
                SpriteEntry entry = sprites[i];

                if (entry == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(entry.key) == true)
                {
                    continue;
                }

                if (entry.sprite == null)
                {
                    DebugLogger.Warning(LogCategory.UI, $"UIAssetRegistry entry has null sprite: {entry.key}");
                    continue;
                }

                spriteMap[entry.key] = entry.sprite;
            }

            DebugLogger.Info(LogCategory.UI, $"UIAssetRegistry loaded sprites: {spriteMap.Count}");
        }

        public bool TryGetSprite(string key, out Sprite sprite)
        {
            sprite = null;

            if (string.IsNullOrWhiteSpace(key) == true)
            {
                return false;
            }

            return spriteMap.TryGetValue(key, out sprite);
        }

        public Sprite GetSprite(string key)
        {
            if (TryGetSprite(key, out Sprite sprite) == true)
            {
                return sprite;
            }

            DebugLogger.Warning(LogCategory.UI, $"Missing sprite key: {key}");
            return null;
        }
    }
}
