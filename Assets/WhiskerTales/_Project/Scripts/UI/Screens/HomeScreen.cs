using UnityEngine;
using UnityEngine.UI;
using WhiskerTales.Core;
using WhiskerTales.Navigation;

namespace WhiskerTales.UI.Screens
{
    /// <summary>
    /// V2 HomeScreen — BaseScreen 상속, ScreenId.Home.
    /// 기존 Stage 2 컴포넌트(HomeObjectLayer/HomeNabi/HomeAmbientLayer/HomeCopyFader)를
    /// 자식으로 그대로 재사용하면서 새 V2 ScreenNavigator 체계 안에서 동작.
    /// 한옥 배경(bg_home_main) 자동 적용 — 빈 화면 → 살아있는 공간으로.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class HomeScreen : BaseScreen
    {
        [Header("Background")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private string backgroundResourcePath = "Sprites/Backgrounds/bg_home_main";

        protected override void Awake()
        {
            base.Awake();
            ApplyBackground();
        }

        private void ApplyBackground()
        {
            if (backgroundImage == null)
            {
                DebugLogger.Warning(LogCategory.UI, "HomeScreen: backgroundImage not assigned.");
                return;
            }

            if (backgroundImage.sprite != null)
            {
                return;
            }

            Sprite bg = Resources.Load<Sprite>(backgroundResourcePath);

            if (bg == null)
            {
                DebugLogger.Warning(LogCategory.UI, "HomeScreen: background sprite not found at Resources/" + backgroundResourcePath);
                return;
            }

            backgroundImage.sprite = bg;
            backgroundImage.preserveAspect = false;
            backgroundImage.color = Color.white;
            backgroundImage.raycastTarget = false;
            DebugLogger.Info(LogCategory.UI, "HomeScreen: background applied (" + backgroundResourcePath + ").");
        }
    }
}
