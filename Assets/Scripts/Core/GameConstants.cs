using System;

namespace WhiskerTales.Core
{
    /// <summary>
    /// Centralized constants for Whisker Tales.
    /// Phase 1 stabilization: add-only file. Existing systems can migrate gradually.
    /// </summary>
    public static class GameConstants
    {
        public static class Board
        {
            public const int Size = 8;
            public const int TileTypeCount = 6;
            public const int MaxCascadeIterations = 10;
            public const int InitialMatchResolveMaxIterations = 100;
            public const int SpecialChainSafetyLimit = 50;
            public const int DefaultMoveLimit = 25;
        }

        public static class Economy
        {
            public const int MaxLives = 5;
            public const int LivesRechargeMinutes = 30;
            public const int DailyNyangiHeartCap = 30;
            public const int DefaultCoinReward = 10;
            public const int DefaultHeartReward = 1;
            public const int SleepModeHeartsPerSession = 2;
            public const int SleepModeNyangiHeartFullReward = 10;
            public const int SleepModeAnchovyPerHour = 10;
        }

        public static class Sleep
        {
            public const float MaxSleepHours = 8f;
            public const float MinutesPerAffinityReward = 30f;
            public const float TargetBrightness = 0.2f;
        }

        public static class Detox
        {
            public const float ModalProbability = 0.33f;
            public const int ShowAfterLevels = 3;
        }

        public static class UI
        {
            public const int CanvasWidth = 1080;
            public const int CanvasHeight = 1920;
            public const int SafeAreaTop = 88;
            public const int SafeAreaBottom = 34;
            public const float StandardOverlayAlpha = 0.55f;
            public const float StrongOverlayAlpha = 0.65f;
            public const float ButtonPressScale = 0.94f;
            public const float SelectedTabScale = 1.05f;
            public const float NotificationBadgeSize = 42f;
        }

        public static class Timing
        {
            public const float TileSwapSeconds = 0.16f;
            public const float InvalidSwapReturnSeconds = 0.12f;
            public const float TileDropMinSeconds = 0.22f;
            public const float TileDropMaxSeconds = 0.34f;
            public const float MatchPopStaggerSeconds = 0.04f;
            public const float MatchBurstFrameSeconds = 0.045f;
            public const float ModalOpenSeconds = 0.24f;
            public const float ModalCloseSeconds = 0.16f;
            public const float ScreenFadeSeconds = 0.42f;
            public const float TabSwitchSeconds = 0.28f;
            public const float ToastDurationSeconds = 2.2f;
            public const float ImportantToastDurationSeconds = 3.5f;
            public const float CurrencyCountUpMinSeconds = 0.55f;
            public const float CurrencyCountUpMaxSeconds = 0.85f;
            public const float FlyingCurrencySeconds = 0.65f;
            public const float HintIdleSeconds = 5f;
        }

        public static class Audio
        {
            public const float ButtonClickVolume = 0.55f;
            public const float CoinCollectVolume = 0.72f;
            public const float MatchSoundVolume = 0.78f;
            public const float LevelCompleteVolume = 0.88f;
            public const float DefaultBgmVolume = 0.55f;
            public const float DefaultSfxVolume = 0.75f;
        }

        public static class Save
        {
            public const int SaveVersion = 1;
            public const string SaveKey = "WhiskerTales.SaveData";
            public const string SettingsKey = "WhiskerTales.Settings";
        }
    }
}
