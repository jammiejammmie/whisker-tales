using UnityEngine;

namespace WhiskerTales.Settings
{
    /// <summary>
    /// Phase B 설정 관리. 현재는 DetoxModeEnabled만 노출 (B-2 needed).
    /// 나머지 항목(BGM/SFX 볼륨, 알림, 언어 등)은 §3-5 화면 작업 시 확장.
    /// PlayerPrefs 백킹.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class SettingsManager : MonoBehaviour
    {
        public const string PREF_DETOX_ENABLED = "Settings.DetoxModeEnabled";

        public static SettingsManager Instance { get; private set; }

        public bool DetoxModeEnabled
        {
            get => PlayerPrefs.GetInt(PREF_DETOX_ENABLED, 1) == 1;
            set
            {
                PlayerPrefs.SetInt(PREF_DETOX_ENABLED, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
    }
}
