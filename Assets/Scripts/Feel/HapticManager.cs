using UnityEngine;

using WhiskerTales.Core;
namespace WhiskerTales.Feel
{
    public enum HapticStrength
    {
        Light,
        Medium,
        Heavy
    }

    public sealed class HapticManager : MonoBehaviour
    {
        public static HapticManager Instance { get; private set; }

        [SerializeField] private bool enabledHaptics = true;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            WhiskerTales.Core.GameEvents.OnTileSwapped += HandleTileSwapped;
            WhiskerTales.Core.GameEvents.OnMatchFound += HandleMatchFound;
            WhiskerTales.Core.GameEvents.OnSpecialTileCreated += HandleSpecialTileCreated;
            WhiskerTales.Core.GameEvents.OnLevelCompleted += HandleLevelCompleted;
        }

        private void OnDisable()
        {
            WhiskerTales.Core.GameEvents.OnTileSwapped -= HandleTileSwapped;
            WhiskerTales.Core.GameEvents.OnMatchFound -= HandleMatchFound;
            WhiskerTales.Core.GameEvents.OnSpecialTileCreated -= HandleSpecialTileCreated;
            WhiskerTales.Core.GameEvents.OnLevelCompleted -= HandleLevelCompleted;
        }

        public void Play(HapticStrength strength)
        {
            if (enabledHaptics == false)
            {
                return;
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            DebugLogger.Info(LogCategory.UI, $"Haptic {strength}");
#else
            Handheld.Vibrate();
#endif
        }

        public void Light()
        {
            Play(HapticStrength.Light);
        }

        public void Medium()
        {
            Play(HapticStrength.Medium);
        }

        public void Heavy()
        {
            Play(HapticStrength.Heavy);
        }

        private void HandleTileSwapped(int x1, int y1, int x2, int y2)
        {
            Light();
        }

        private void HandleMatchFound(int count)
        {
            Medium();
        }

        private void HandleSpecialTileCreated(WhiskerTales.Puzzle.SpecialItemType type)
        {
            Medium();
        }

        private void HandleLevelCompleted(int level, int stars)
        {
            Heavy();
        }
    }
}
