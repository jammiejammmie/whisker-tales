using UnityEngine;
using UnityEngine.SceneManagement;
using WhiskerTales.Core;

namespace WhiskerTales.Runtime
{
    /// <summary>
    /// 첫 씬이 Boot_Persistent가 아닐 때를 대비한 안전망.
    /// 정상 흐름은 Boot_Persistent.unity에 GameRuntime이 직접 부착된 상태.
    /// </summary>
    public static class RuntimeBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureRuntime()
        {
            if (GameRuntime.Instance != null)
            {
                return;
            }

            int activeIdx = SceneManager.GetActiveScene().buildIndex;
            string activeName = SceneManager.GetActiveScene().name;

            if (activeIdx != 0)
            {
                DebugLogger.Info(LogCategory.UI, "RuntimeBootstrapper: not Boot_Persistent (idx=" + activeIdx + ", name=" + activeName + ") — V2 init skipped (legacy path).");
                return;
            }

            GameObject go = new GameObject("__GameRuntime");
            go.AddComponent<GameRuntime>();
            DebugLogger.Info(LogCategory.UI, "RuntimeBootstrapper: auto-spawned GameRuntime.");
        }
    }
}
