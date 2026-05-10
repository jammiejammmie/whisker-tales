using System;
using System.Collections.Generic;
using UnityEngine;
using WhiskerTales.Core;

namespace WhiskerTales.UI
{
    [CreateAssetMenu(fileName = "UIAssetRegistry", menuName = "Whisker Tales/UI Asset Registry")]
    public sealed class UIAssetRegistry : ScriptableObject
    {
        [Serializable]
        public sealed class SpriteEntry
        {
            public string key;
            public Sprite sprite;
        }

        [SerializeField] private List<SpriteEntry> sprites = new List<SpriteEntry>();
        private Dictionary<string, Sprite> cache;

        public Sprite GetSprite(string key)
        {
            EnsureCache();

            if (string.IsNullOrEmpty(key) == true)
            {
                DebugLogger.Warning(LogCategory.UI, "UIAssetRegistry.GetSprite called with empty key.");
                return null;
            }

            if (cache.TryGetValue(key, out Sprite sprite) == true)
            {
                return sprite;
            }

            DebugLogger.Warning(LogCategory.UI, "Sprite key not found: " + key);
            return null;
        }

        private void EnsureCache()
        {
            if (cache != null)
            {
                return;
            }

            cache = new Dictionary<string, Sprite>(StringComparer.Ordinal);

            for (int i = 0; i < sprites.Count; i++)
            {
                SpriteEntry entry = sprites[i];

                if (entry == null)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(entry.key) == true)
                {
                    continue;
                }

                if (entry.sprite == null)
                {
                    DebugLogger.Warning(LogCategory.UI, "UIAssetRegistry entry has null sprite: " + entry.key);
                    continue;
                }

                if (cache.ContainsKey(entry.key) == true)
                {
                    DebugLogger.Warning(LogCategory.UI, "Duplicate UI sprite key: " + entry.key);
                    continue;
                }

                cache.Add(entry.key, entry.sprite);
            }
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public void DebugClearCache()
        {
            cache = null;
        }
#endif
    }
}
