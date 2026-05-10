using System;
using UnityEngine;
using UnityEngine.UI;
using WhiskerTales.Core;

namespace WhiskerTales.UI
{
    public sealed class BottomNavController : MonoBehaviour
    {
        [Serializable]
        public sealed class TabBinding
        {
            public string screenId;
            public Button button;
            public RectTransform icon;
            public GameObject selectedGlow;
        }

        [SerializeField] private ScreenNavigator navigator;
        [SerializeField] private TabBinding[] tabs;

        private int selectedIndex = -1;

        private void Awake()
        {
            WireButtons();
        }

        public void Select(int index)
        {
            if (tabs == null)
            {
                return;
            }

            if (index < 0 || index >= tabs.Length)
            {
                DebugLogger.Warning(LogCategory.UI, "BottomNav invalid tab index: " + index);
                return;
            }

            selectedIndex = index;

            for (int i = 0; i < tabs.Length; i++)
            {
                bool selected = i == selectedIndex;
                TabBinding tab = tabs[i];

                if (tab == null)
                {
                    continue;
                }

                if (tab.icon != null)
                {
                    tab.icon.localScale = selected == true ? Vector3.one * 1.05f : Vector3.one;
                }

                if (tab.selectedGlow != null)
                {
                    tab.selectedGlow.SetActive(selected);
                }
            }

            if (navigator != null && string.IsNullOrEmpty(tabs[index].screenId) == false)
            {
                navigator.Show(tabs[index].screenId);
            }
        }

        private void WireButtons()
        {
            if (tabs == null)
            {
                return;
            }

            for (int i = 0; i < tabs.Length; i++)
            {
                int capturedIndex = i;
                TabBinding tab = tabs[i];

                if (tab == null || tab.button == null)
                {
                    continue;
                }

                tab.button.onClick.AddListener(() => Select(capturedIndex));
            }
        }
    }
}
