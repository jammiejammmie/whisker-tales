using UnityEngine;
using WhiskerTales.Core;

namespace WhiskerTales.UI
{
    public static class UILayoutConstants
    {
        public const int CanvasWidth = 1080;
        public const int CanvasHeight = 1920;
        public const int SafeTop = 88;
        public const int SafeBottom = 34;

        public static readonly Color Cream = FromHex("#F5F1E8");
        public static readonly Color Brown = FromHex("#8B7355");
        public static readonly Color Coral = FromHex("#E8A87C");
        public static readonly Color Text = FromHex("#2C1810");
        public static readonly Color SoftPink = FromHex("#F4A0B5");

        public const float PrimaryButtonHeight = 130f;
        public const float SecondaryButtonHeight = 105f;
        public const float BottomNavHeight = 170f;
        public const float TopButtonSize = 96f;
        public const float ScreenFadeSeconds = 0.28f;
        public const float ButtonPressScale = 0.94f;

        // Stage 2 — Home "살아있는 공간" timings.
        public const float HomeNabiBreathScale = 0.98f;
        public const float HomeNabiBreathSeconds = 3.5f;
        public const float HomeNabiBlinkMinSeconds = 4f;
        public const float HomeNabiBlinkMaxSeconds = 7f;
        public const float HomeNabiBlinkDurationSeconds = 0.18f;
        public const float HomeNabiNoticeEarTime = 1.5f;
        public const float HomeNabiNoticeBlinkTime = 3.0f;
        public const float HomeNabiEarTwitchAngle = 6f;
        public const float HomeNabiEarTwitchSeconds = 0.12f;

        public const float HomePuzzleBookPressScale = 1.03f;
        public const float HomePuzzleBookPressSeconds = 0.18f;
        public const float HomeSleepFlashSeconds = 0.45f;
        public const float HomeSleepFlashPeakAlpha = 0.55f;

        public const float HomeLanternGlowMinAlpha = 0.4f;
        public const float HomeLanternGlowMaxAlpha = 0.7f;
        public const float HomeLanternGlowSeconds = 4f;
        public const float HomeLeafSwayAngle = 2f;
        public const float HomeLeafSwaySeconds = 5.5f;

        public const float HomeCopyFadeStartSeconds = 2.4f;
        public const float HomeCopyFadeDurationSeconds = 1.6f;

        public const float HomeBackgroundCrossfadeSeconds = 4f;

        // Stage 3 — Home Nabi quiet presence (HomeNabiPositionSystem).
        // 의도: "직접 보이는 연출"이 아니라 "어, 뭔가 살아있었던 것 같은데?" 수준.
        public const float HomeNabiQuietBreathScale = 0.99f;
        public const float HomeNabiQuietBreathSecondsMin = 4f;
        public const float HomeNabiQuietBreathSecondsMax = 5f;
        public const float HomeNabiQuietBlinkIntervalMin = 8f;
        public const float HomeNabiQuietBlinkIntervalMax = 15f;
        public const float HomeNabiQuietBlinkCloseSeconds = 0.15f;
        public const float HomeNabiQuietBlinkOpenSeconds = 0.2f;
        public const float HomeNabiQuietStretchIntervalMin = 300f;
        public const float HomeNabiQuietStretchIntervalMax = 600f;
        public const float HomeNabiQuietStretchChance = 0.25f;
        public const float HomeNabiQuietStretchHoldSeconds = 1.4f;
        public const float HomeNabiPositionFadeSeconds = 1.5f;
        public const float HomeNabiPositionStayMinSeconds = 180f;
        public const float HomeNabiPositionStayMaxSeconds = 480f;
        public const float HomeNabiHiddenProbability = 0.25f;
        public const float HomeNabiFrontalPoseChance = 0.12f;

        // Spatial integration — shadow + perspective + time-of-day tint.
        public const float HomeNabiShadowBaseAlpha = 0.25f;
        public const float HomeNabiTintFadeSeconds = 4f;
        public const float HomeNabiPerspectiveDefault = 1f;
        public static readonly Color HomeNabiTintDawn = FromHex("#E8EDF5");
        public static readonly Color HomeNabiTintDay = FromHex("#FFFFFF");
        public static readonly Color HomeNabiTintEvening = FromHex("#F8EDD8");
        public static readonly Color HomeNabiTintNight = FromHex("#C4A882");

        public static Color FromHex(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out Color color) == true)
            {
                return color;
            }

            DebugLogger.Warning(LogCategory.UI, "Invalid color hex: " + hex);
            return Color.white;
        }
    }
}
