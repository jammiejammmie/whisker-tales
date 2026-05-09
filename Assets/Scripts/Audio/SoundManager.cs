using System;
using System.Collections.Generic;
using UnityEngine;

namespace WhiskerTales.Audio
{
    /// <summary>
    /// Phase B §4. 고양이/일반/음소거 3-모드 효과음 시스템.
    /// 외부에서 RegisterCatClip / RegisterNormalClip 으로 클립 주입 후 Play(SfxKey) 호출.
    /// AppBootstrap이 Awake 시점에 Assets/Audio/Cats/* 를 §4-3 매핑대로 등록.
    /// </summary>
    public enum SoundMode { Normal, Cat, Mute }

    public enum SfxKey
    {
        Click,
        Match,
        Combo,
        LevelClear,
        Coin,
        Fail,
        Pet,
    }

    [DefaultExecutionOrder(-100)]
    public class SoundManager : MonoBehaviour
    {
        public const string PREF_MODE = "Sound.Mode";
        public const string PREF_VOLUME = "Sound.SfxVolume";
        public const float DEFAULT_VOLUME = 0.8f;

        public static SoundManager Instance { get; private set; }

        public event Action<SoundMode> OnModeChanged;

        [SerializeField, Range(0f, 1f)] private float sfxVolume = DEFAULT_VOLUME;

        private SoundMode currentMode = SoundMode.Cat;
        private AudioSource sfxSource;
        private readonly Dictionary<SfxKey, AudioClip> normalClips = new Dictionary<SfxKey, AudioClip>();
        private readonly Dictionary<SfxKey, AudioClip> catClips = new Dictionary<SfxKey, AudioClip>();

        public SoundMode CurrentMode => currentMode;
        public float SfxVolume => sfxVolume;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            currentMode = (SoundMode)PlayerPrefs.GetInt(PREF_MODE, (int)SoundMode.Cat);
            sfxVolume = PlayerPrefs.GetFloat(PREF_VOLUME, DEFAULT_VOLUME);

            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.spatialBlend = 0f;
        }

        public void RegisterCatClip(SfxKey key, AudioClip clip)
        {
            if (clip == null) return;
            catClips[key] = clip;
        }

        public void RegisterNormalClip(SfxKey key, AudioClip clip)
        {
            if (clip == null) return;
            normalClips[key] = clip;
        }

        public void Play(SfxKey key)
        {
            if (currentMode == SoundMode.Mute || sfxSource == null) return;
            var dict = currentMode == SoundMode.Cat ? catClips : normalClips;
            if (dict.TryGetValue(key, out AudioClip clip) && clip != null)
            {
                sfxSource.PlayOneShot(clip, sfxVolume);
            }
        }

        public void SetMode(SoundMode mode)
        {
            if (mode == currentMode) return;
            currentMode = mode;
            PlayerPrefs.SetInt(PREF_MODE, (int)mode);
            PlayerPrefs.Save();
            OnModeChanged?.Invoke(mode);
            Debug.Log($"[SoundManager] Mode → {mode}");
        }

        public void SetSfxVolume(float v)
        {
            sfxVolume = Mathf.Clamp01(v);
            PlayerPrefs.SetFloat(PREF_VOLUME, sfxVolume);
            PlayerPrefs.Save();
        }
    }
}
