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
