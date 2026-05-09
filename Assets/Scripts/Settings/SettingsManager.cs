using System;
using UnityEngine;

namespace WhiskerTales.Settings
{
    /// <summary>
    /// Phase B 설정 관리. PlayerPrefs 백킹. 일부 setter는 다른 매니저에 즉시 전파
    /// (BGM/SFX 볼륨 → AudioManager / SoundManager, 언어 → I18nManager).
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class SettingsManager : MonoBehaviour
    {
        public const string PREF_DETOX_ENABLED        = "Settings.DetoxModeEnabled";
        public const string PREF_DAILY_NOTIFICATION   = "Settings.DailyNotificationEnabled";
        public const string PREF_BGM_VOLUME           = "Settings.BgmVolume";
        public const string PREF_SFX_VOLUME           = "Settings.SfxVolume";
        public const string PREF_LANGUAGE             = "Settings.Language"; // "ko" / "en"

        public const string LANG_KO = "ko";
        public const string LANG_EN = "en";

        public static SettingsManager Instance { get; private set; }

        public event Action OnSettingsChanged;

        public bool DetoxModeEnabled
        {
            get => PlayerPrefs.GetInt(PREF_DETOX_ENABLED, 1) == 1;
            set
            {
                PlayerPrefs.SetInt(PREF_DETOX_ENABLED, value ? 1 : 0);
                PlayerPrefs.Save();
                OnSettingsChanged?.Invoke();
            }
        }

        public bool DailyNotificationEnabled
        {
            get => PlayerPrefs.GetInt(PREF_DAILY_NOTIFICATION, 1) == 1;
            set
            {
                PlayerPrefs.SetInt(PREF_DAILY_NOTIFICATION, value ? 1 : 0);
                PlayerPrefs.Save();
                OnSettingsChanged?.Invoke();
            }
        }

        public float BgmVolume
        {
            get => PlayerPrefs.GetFloat(PREF_BGM_VOLUME, 0.5f);
            set
            {
                float v = Mathf.Clamp01(value);
                PlayerPrefs.SetFloat(PREF_BGM_VOLUME, v);
                PlayerPrefs.Save();
                if (AudioManager.instance != null) AudioManager.instance.SetBGMVolume(v);
                OnSettingsChanged?.Invoke();
            }
        }

        public float SfxVolume
        {
            get => PlayerPrefs.GetFloat(PREF_SFX_VOLUME, 0.7f);
            set
            {
                float v = Mathf.Clamp01(value);
                PlayerPrefs.SetFloat(PREF_SFX_VOLUME, v);
                PlayerPrefs.Save();
                if (AudioManager.instance != null) AudioManager.instance.SetSFXVolume(v);
                if (Audio.SoundManager.Instance != null) Audio.SoundManager.Instance.SetSfxVolume(v);
                OnSettingsChanged?.Invoke();
            }
        }

        public string Language
        {
            get => PlayerPrefs.GetString(PREF_LANGUAGE, LANG_KO);
            set
            {
                string lang = (value == LANG_EN) ? LANG_EN : LANG_KO;
                PlayerPrefs.SetString(PREF_LANGUAGE, lang);
                PlayerPrefs.Save();
                if (I18nManager.Instance != null)
                {
                    I18nManager.Instance.SetLanguage(lang == LANG_EN ? SystemLanguage.English : SystemLanguage.Korean);
                }
                OnSettingsChanged?.Invoke();
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            EnsureInitialized();
        }

        public void EnsureInitialized()
        {
            Instance = this;
        }
    }
}
