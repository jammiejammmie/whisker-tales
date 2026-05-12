using UnityEngine;
using WhiskerTales.Core;

namespace WhiskerTales.Platform
{
    /// <summary>
    /// 안드로이드 status bar / navigation bar 색상 강제.
    /// Unity 디폴트(투명)를 쓰면 일부 OEM에서 마젠타로 보이는 경우 회피.
    /// Whisker Tales 따뜻한 다크 톤(#2C1810) 사용.
    /// </summary>
    public sealed class AndroidSystemBars : MonoBehaviour
    {
        private static readonly Color StatusBarColor = HexToColor("#2C1810");
        private static readonly Color NavBarColor = HexToColor("#2C1810");

        private void Awake()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            ApplySystemBars();
#else
            DebugLogger.Info(LogCategory.UI, "AndroidSystemBars: editor/non-android — skipped.");
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private void ApplySystemBars()
        {
            try
            {
                using (AndroidJavaClass player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    AndroidJavaObject activity = player.GetStatic<AndroidJavaObject>("currentActivity");
                    AndroidJavaObject window = activity.Call<AndroidJavaObject>("getWindow");

                    int statusInt = ColorToAndroidInt(StatusBarColor);
                    int navInt = ColorToAndroidInt(NavBarColor);

                    activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                    {
                        try
                        {
                            window.Call("setStatusBarColor", statusInt);
                            window.Call("setNavigationBarColor", navInt);
                        }
                        catch (System.Exception inner)
                        {
                            DebugLogger.Warning(LogCategory.UI, "AndroidSystemBars (UI thread) failed: " + inner.Message);
                        }
                    }));
                }

                DebugLogger.Info(LogCategory.UI, "AndroidSystemBars applied.");
            }
            catch (System.Exception e)
            {
                DebugLogger.Warning(LogCategory.UI, "AndroidSystemBars failed: " + e.Message);
            }
        }

        private static int ColorToAndroidInt(Color c)
        {
            int a = (int)(c.a * 255f);
            int r = (int)(c.r * 255f);
            int g = (int)(c.g * 255f);
            int b = (int)(c.b * 255f);
            return (a << 24) | (r << 16) | (g << 8) | b;
        }
#endif

        private static Color HexToColor(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out Color c) == true)
            {
                return c;
            }

            return Color.black;
        }
    }
}
