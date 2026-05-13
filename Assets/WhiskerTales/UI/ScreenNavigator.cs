using System;
using System.Collections.Generic;
using UnityEngine;
using WhiskerTales.Core;

namespace WhiskerTales.UI
{
    public sealed class ScreenNavigator : MonoBehaviour
    {
        [SerializeField] private List<UIScreenBase> screens = new List<UIScreenBase>();
        [SerializeField] private string initialScreenId = "home";

        private readonly Dictionary<string, UIScreenBase> screenMap = new Dictionary<string, UIScreenBase>();
        private UIScreenBase currentScreen;

        // V2-16: TabBarController subscribes to flip active-tint and toggle visibility per screen.
        public event Action<string> ScreenShown;

        public string CurrentScreenId
        {
            get
            {
                if (currentScreen == null)
                {
                    return null;
                }
                return currentScreen.ScreenId;
            }
        }

        private void Awake()
        {
            BuildMap();
        }

        private void Start()
        {
            if (string.IsNullOrEmpty(initialScreenId) == false)
            {
                Show(initialScreenId, true);
            }
        }

        public void Show(string screenId, bool instant = false)
        {
            if (screenMap.Count == 0)
            {
                BuildMap();
            }

            if (screenMap.TryGetValue(screenId, out UIScreenBase next) == false)
            {
                DebugLogger.Warning(LogCategory.UI, "Screen not found: " + screenId);
                return;
            }

            if (currentScreen == next)
            {
                return;
            }

            if (currentScreen != null)
            {
                currentScreen.Hide(instant);
            }

            currentScreen = next;
            currentScreen.Show(instant);

            Action<string> handler = ScreenShown;

            if (handler != null)
            {
                handler.Invoke(screenId);
            }
        }

        private void BuildMap()
        {
            screenMap.Clear();

            for (int i = 0; i < screens.Count; i++)
            {
                UIScreenBase screen = screens[i];

                if (screen == null)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(screen.ScreenId) == true)
                {
                    DebugLogger.Warning(LogCategory.UI, "Screen has empty id: " + screen.name);
                    continue;
                }

                if (screenMap.ContainsKey(screen.ScreenId) == true)
                {
                    DebugLogger.Warning(LogCategory.UI, "Duplicate screen id: " + screen.ScreenId);
                    continue;
                }

                screenMap.Add(screen.ScreenId, screen);
                screen.Hide(true);
            }
        }
    }
}
