using System.Collections.Generic;
using UnityEngine;
using WhiskerTales.Core;

namespace WhiskerTales.Navigation
{
    /// <summary>
    /// V2 ScreenNavigator — enum 기반.
    /// 기존 WhiskerTales.UI.ScreenNavigator (string 기반)와 별개로 공존.
    /// </summary>
    public sealed class ScreenNavigator : MonoBehaviour
    {
        [SerializeField] private ScreenId initialScreen = ScreenId.Home;
        [SerializeField] private List<BaseScreen> screens = new List<BaseScreen>();

        private readonly Dictionary<ScreenId, BaseScreen> map = new Dictionary<ScreenId, BaseScreen>();
        private BaseScreen current;

        public ScreenId Current
        {
            get { return current != null ? current.Id : ScreenId.None; }
        }

        private void Awake()
        {
            map.Clear();

            for (int i = 0; i < screens.Count; i++)
            {
                BaseScreen s = screens[i];

                if (s == null)
                {
                    continue;
                }

                if (s.Id == ScreenId.None)
                {
                    DebugLogger.Warning(LogCategory.UI, "ScreenNavigator: '" + s.name + "' has ScreenId.None — skipped.");
                    continue;
                }

                if (map.ContainsKey(s.Id) == true)
                {
                    DebugLogger.Warning(LogCategory.UI, "ScreenNavigator: duplicate ScreenId " + s.Id + " on '" + s.name + "' — skipped.");
                    continue;
                }

                map[s.Id] = s;
                s.Hide(true);
            }

            DebugLogger.Info(LogCategory.UI, "ScreenNavigator: registered " + map.Count + " screens.");
        }

        private void Start()
        {
            if (initialScreen != ScreenId.None)
            {
                Show(initialScreen, true);
            }
        }

        public void Show(ScreenId id, bool instant = false)
        {
            if (map.TryGetValue(id, out BaseScreen target) == false)
            {
                DebugLogger.Warning(LogCategory.UI, "ScreenNavigator.Show: " + id + " not registered.");
                return;
            }

            if (current != null && current != target)
            {
                current.Hide(instant);
            }

            target.Show(instant);
            current = target;

            DebugLogger.Info(LogCategory.UI, "ScreenNavigator.Show: " + id);
        }
    }
}
