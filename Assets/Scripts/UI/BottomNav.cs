using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WhiskerTales.Core;

namespace WhiskerTales.UI
{
    /// <summary>
    /// 하단 네비게이션 (Stage 4 §4-1).
    /// Shop / Cat Room / Home / Gallery / Friends 5탭.
    /// 각 탭은 패널 활성화/비활성화로 전환. 미구현 화면은 Coming Soon stub.
    /// </summary>
    public class BottomNav : MonoBehaviour
    {
        public enum Tab { Shop, CatRoom, Home, Gallery, Friends }

        [Serializable]
        public class TabBinding
        {
            public Tab tab;
            public Button button;
            public GameObject panel;
            public Image icon;
        }

        [SerializeField] private TabBinding[] tabs = new TabBinding[5];

        [Header("Tint")]
        [SerializeField] private Color activeColor = new Color(0.91f, 0.66f, 0.49f);
        [SerializeField] private Color inactiveColor = new Color(0.55f, 0.45f, 0.33f, 0.6f);

        [Header("Default Tab")]
        [SerializeField] private Tab defaultTab = Tab.Home;

        private void OnEnable()
        {
            foreach (var b in tabs)
            {
                if (b == null || b.button == null) continue;
                Tab captured = b.tab;
                b.button.onClick.AddListener(() => SwitchTo(captured));
            }
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnNavigationRequested += HandleNavigationRequested;
            }
        }

        private void OnDisable()
        {
            foreach (var b in tabs)
            {
                if (b == null || b.button == null) continue;
                b.button.onClick.RemoveAllListeners();
            }
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnNavigationRequested -= HandleNavigationRequested;
            }
        }

        private void HandleNavigationRequested(NavigationTarget target)
        {
            switch (target)
            {
                case NavigationTarget.Title:    SwitchTo(Tab.Home); break;
                case NavigationTarget.Shop:     SwitchTo(Tab.Shop); break;
                case NavigationTarget.CatRoom:  SwitchTo(Tab.CatRoom); break;
                case NavigationTarget.Gallery:  SwitchTo(Tab.Gallery); break;
                case NavigationTarget.Friends:  SwitchTo(Tab.Friends); break;
                // Cafe/Gameplay/Settings는 BottomNav 외부 패널이므로 처리 안 함
            }
        }

        private void Start()
        {
            SwitchTo(defaultTab);
        }

        public void SwitchTo(Tab target)
        {
            AudioManager.instance?.PlayButtonClick();

            foreach (var b in tabs)
            {
                if (b == null) continue;
                bool active = b.tab == target;
                if (b.panel != null) b.panel.SetActive(active);
                if (b.icon != null) b.icon.color = active ? activeColor : inactiveColor;
            }
        }
    }
}
