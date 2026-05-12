using UnityEngine;
using WhiskerTales.Core;

namespace WhiskerTales.Platform
{
    /// <summary>
    /// 안드로이드 환경 가드 — 마젠타 회피의 핵심.
    /// 모든 카메라의 clear color를 크림색 #F5F1E8로 강제 설정.
    /// shader strip / texture missing 시 Unity 디폴트 마젠타가 화면 가득 차는 것을 차단.
    /// </summary>
    public sealed class AndroidRuntimeGuard : MonoBehaviour
    {
        private static readonly Color CreamBackground = HexToColor("#F5F1E8");

        [SerializeField] private Camera targetCamera;
        [SerializeField] private bool overrideAllCameras = true;
        [SerializeField] private bool reapplyOnSceneLoad = true;

        private void Awake()
        {
            ApplyCreamBackground("Awake");
        }

        private void Start()
        {
            ApplyCreamBackground("Start");
        }

        private void OnEnable()
        {
            if (reapplyOnSceneLoad == true)
            {
                UnityEngine.SceneManagement.SceneManager.sceneLoaded += HandleSceneLoaded;
            }
        }

        private void OnDisable()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        private void HandleSceneLoaded(UnityEngine.SceneManagement.Scene s, UnityEngine.SceneManagement.LoadSceneMode m)
        {
            ApplyCreamBackground("sceneLoaded:" + s.name);
        }

        private void ApplyCreamBackground(string phase)
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }

            if (targetCamera != null)
            {
                targetCamera.clearFlags = CameraClearFlags.SolidColor;
                targetCamera.backgroundColor = CreamBackground;
            }

            if (overrideAllCameras == true)
            {
                Camera[] all = Camera.allCameras;
                int patched = 0;

                for (int i = 0; i < all.Length; i++)
                {
                    Camera c = all[i];

                    if (c == null)
                    {
                        continue;
                    }

                    c.clearFlags = CameraClearFlags.SolidColor;
                    c.backgroundColor = CreamBackground;
                    patched++;
                }

                DebugLogger.Info(LogCategory.UI, "AndroidRuntimeGuard[" + phase + "]: cream applied to " + patched + " camera(s).");
            }
        }

        private static Color HexToColor(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out Color c) == true)
            {
                return c;
            }

            return new Color(0.961f, 0.945f, 0.910f);
        }
    }
}
