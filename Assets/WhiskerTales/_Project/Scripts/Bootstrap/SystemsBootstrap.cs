using UnityEngine;
using UnityEngine.SceneManagement;
using WhiskerTales.Core;

namespace WhiskerTales.Bootstrap
{
    /// <summary>
    /// V2 격리 가드 — 부팅 진입점이 Boot_Persistent (build index 0)인지 진단.
    /// V2 모드: index 0 + name "Boot_Persistent" → V2 부팅 흐름 확정.
    /// Legacy 모드: 그 외 → 기존 v1 흐름 (AppBootstrap 등) 그대로 동작, V2 자동 init 스킵.
    ///
    /// AppBootstrap.cs는 freeze 상태이므로 직접 차단할 수 없음 — 대신 씬 분리로 격리:
    /// Boot_Persistent에는 AppBootstrap 컴포넌트가 부착되지 않으므로 V2 부팅 시엔 v1이 자동 차단됨.
    /// </summary>
    public static class SystemsBootstrap
    {
        public static bool IsV2Mode { get; private set; }
        public static bool BootDetected { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void DetectMode()
        {
            if (BootDetected == true)
            {
                return;
            }

            BootDetected = true;

            Scene active = SceneManager.GetActiveScene();
            int idx = active.buildIndex;
            string name = active.name;

            IsV2Mode = (idx == 0 && name == "Boot_Persistent");

            if (IsV2Mode == true)
            {
                DebugLogger.Info(LogCategory.UI, "SystemsBootstrap: V2 mode (Boot_Persistent, idx=0).");
            }
            else
            {
                DebugLogger.Info(LogCategory.UI, "SystemsBootstrap: legacy mode (scene='" + name + "', idx=" + idx + "). V2 auto-init skipped.");
            }
        }
    }
}
