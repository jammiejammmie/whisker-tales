using UnityEngine;
using UnityEngine.EventSystems;
using WhiskerTales.Core;

namespace WhiskerTales.Platform
{
    /// <summary>
    /// EventSystem count == 1 강제 — V2 절대 원칙.
    /// 중복 EventSystem이 있으면 첫 번째만 남기고 나머지 Destroy.
    /// 하나도 없으면 자동 생성 + StandaloneInputModule 부착.
    /// </summary>
    public sealed class TouchInputGuard : MonoBehaviour
    {
        private void Awake()
        {
            EnforceSingleEventSystem("Awake");
        }

        private void Start()
        {
            EnforceSingleEventSystem("Start");
        }

        private void EnforceSingleEventSystem(string phase)
        {
            EventSystem[] all = FindObjectsOfType<EventSystem>(true);

            if (all == null || all.Length == 0)
            {
                GameObject go = new GameObject("EventSystem");
                go.AddComponent<EventSystem>();
                go.AddComponent<StandaloneInputModule>();
                DontDestroyOnLoad(go);
                DebugLogger.Info(LogCategory.UI, "TouchInputGuard[" + phase + "]: created EventSystem.");
                return;
            }

            if (all.Length == 1)
            {
                return;
            }

            EventSystem keep = all[0];

            for (int i = 1; i < all.Length; i++)
            {
                if (all[i] == null || all[i] == keep)
                {
                    continue;
                }

                DebugLogger.Warning(LogCategory.UI, "TouchInputGuard[" + phase + "]: destroying duplicate EventSystem '" + all[i].name + "'");
                Destroy(all[i].gameObject);
            }
        }
    }
}
