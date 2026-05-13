using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WhiskerTales.Core;

namespace WhiskerTales.UI
{
    // Scene-level persistent tab bar. Lives once under Canvas_Screens/SafeAreaRoot as a sibling
    // of all screen instances, so it overlays on top regardless of which screen is showing.
    //
    // Visibility: a configurable list of screen IDs where the bar appears (home / catroom / cafe /
    // sleepmode / settings). Hidden on focused-session screens (gameplay / levelselect / levelclear
    // / gamefail) so play doesn't compete with navigation.
    //
    // Active tint: full-alpha for the current screen's tab, soft desaturated alpha for the others.
    // Intentionally NOT a glow ring / scale pulse — the request was "breathing 수준의 미세 강조",
    // i.e. quiet emphasis, not flashy mobile-game UI.
    public sealed class TabBarController : MonoBehaviour
    {
        [Serializable]
        public sealed class TabBinding
        {
            public string screenId;
            public Button button;
            public Image image;
        }

        [SerializeField] private ScreenNavigator navigator;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private List<TabBinding> tabs = new List<TabBinding>();
        [SerializeField] private List<string> visibleOnScreens = new List<string>();

        [Header("Active / Inactive Tint")]
        [SerializeField] private Color activeTint = new Color(1f, 1f, 1f, 1f);
        [SerializeField] private Color inactiveTint = new Color(1f, 1f, 1f, 0.62f);

        private void Awake()
        {
            for (int i = 0; i < tabs.Count; i++)
            {
                TabBinding tab = tabs[i];

                if (tab == null || tab.button == null)
                {
                    continue;
                }

                string targetId = tab.screenId;
                tab.button.onClick.RemoveAllListeners();
                tab.button.onClick.AddListener(() => OnTabPressed(targetId));
            }
        }

        private void OnEnable()
        {
            if (navigator != null)
            {
                navigator.ScreenShown += OnScreenShown;
            }
        }

        private void Start()
        {
            // Catch the initial Show("home") fired by ScreenNavigator.Start, in case our OnEnable
            // ran after that event (execution-order race).
            if (navigator != null && string.IsNullOrEmpty(navigator.CurrentScreenId) == false)
            {
                OnScreenShown(navigator.CurrentScreenId);
            }
            else
            {
                // No screen yet — hide the bar so it doesn't flash visible on Boot_Persistent.
                ApplyVisibility(false);
                RefreshActiveTint(null);
            }
        }

        private void OnDisable()
        {
            if (navigator != null)
            {
                navigator.ScreenShown -= OnScreenShown;
            }
        }

        private void OnTabPressed(string targetId)
        {
            if (navigator == null || string.IsNullOrEmpty(targetId) == true)
            {
                return;
            }

            DebugLogger.Info(LogCategory.UI, "[TabBar] -> " + targetId);
            navigator.Show(targetId);
        }

        private void OnScreenShown(string screenId)
        {
            bool visible = visibleOnScreens != null && visibleOnScreens.Contains(screenId);
            ApplyVisibility(visible);
            RefreshActiveTint(screenId);
        }

        private void ApplyVisibility(bool visible)
        {
            if (canvasGroup == null)
            {
                return;
            }

            canvasGroup.alpha = visible == true ? 1f : 0f;
            canvasGroup.blocksRaycasts = visible;
            canvasGroup.interactable = visible;
        }

        private void RefreshActiveTint(string activeScreenId)
        {
            for (int i = 0; i < tabs.Count; i++)
            {
                TabBinding tab = tabs[i];

                if (tab == null || tab.image == null)
                {
                    continue;
                }

                bool isActive = tab.screenId == activeScreenId;
                tab.image.color = isActive == true ? activeTint : inactiveTint;
            }
        }
    }
}
