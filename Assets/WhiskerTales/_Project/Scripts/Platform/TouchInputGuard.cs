using UnityEngine;
using UnityEngine.EventSystems;
using WhiskerTales.Core;

namespace WhiskerTales.Platform
{
    [DisallowMultipleComponent]
    public sealed class TouchInputGuard : MonoBehaviour
    {
        [SerializeField] private bool logTouches = true;

        private const float LogCooldownSeconds = 1f;
        private float lastLogTime;

        private void Awake()
        {
            ValidateEventSystem();
        }

        public void ValidateEventSystem()
        {
            EventSystem[] systems = FindObjectsOfType<EventSystem>(true);
            int count = systems == null ? 0 : systems.Length;

            if (count == 0)
            {
                GameObject host = new GameObject("EventSystem");
                host.AddComponent<EventSystem>();
                host.AddComponent<StandaloneInputModule>();
                DontDestroyOnLoad(host);
                DebugLogger.Info(LogCategory.UI, "[TouchInputGuard] Created EventSystem (count was 0).");
                return;
            }

            if (count == 1)
            {
                EnsureInputModule(systems[0]);
                DebugLogger.Info(LogCategory.UI, "[TouchInputGuard] EventSystem count == 1 (OK).");
                return;
            }

            DebugLogger.Error(LogCategory.UI, "[TouchInputGuard] EventSystem count == " + count + " (duplicates). Pruning extras.");
            for (int i = 1; i < count; i++)
            {
                if (systems[i] != null)
                {
                    Destroy(systems[i].gameObject);
                }
            }
            EnsureInputModule(systems[0]);
        }

        private void EnsureInputModule(EventSystem system)
        {
            if (system == null)
            {
                return;
            }
            if (system.GetComponent<StandaloneInputModule>() == null)
            {
                system.gameObject.AddComponent<StandaloneInputModule>();
            }
        }

        private void Update()
        {
            if (!logTouches)
            {
                return;
            }

            int touchCount = Input.touchCount;
            bool mouseDown = Input.GetMouseButtonDown(0);
            if (touchCount == 0 && !mouseDown)
            {
                return;
            }

            if (Time.unscaledTime - lastLogTime < LogCooldownSeconds)
            {
                return;
            }
            lastLogTime = Time.unscaledTime;

            if (touchCount > 0)
            {
                Touch t = Input.GetTouch(0);
                DebugLogger.Info(LogCategory.UI, "[TouchInputGuard] touch phase=" + t.phase + " pos=" + t.position);
            }
            else
            {
                DebugLogger.Info(LogCategory.UI, "[TouchInputGuard] mouseDown pos=" + (Vector2)Input.mousePosition);
            }
        }
    }
}
