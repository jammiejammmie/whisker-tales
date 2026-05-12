using UnityEngine;
using WhiskerTales.Core;

namespace WhiskerTales.Runtime
{
    /// <summary>
    /// 앱 전역 lifecycle 신호 (pause/resume/focus). 향후 SaveService 트리거 등에 사용.
    /// </summary>
    public sealed class AppLifecycleController : MonoBehaviour
    {
        private void OnApplicationPause(bool paused)
        {
            DebugLogger.Info(LogCategory.UI, "AppLifecycle: pause=" + paused);
        }

        private void OnApplicationFocus(bool focused)
        {
            DebugLogger.Info(LogCategory.UI, "AppLifecycle: focus=" + focused);
        }

        private void OnApplicationQuit()
        {
            DebugLogger.Info(LogCategory.UI, "AppLifecycle: quit");
        }
    }
}
