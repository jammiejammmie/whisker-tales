using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

using WhiskerTales.Core;
namespace WhiskerTales.Feel
{
    public enum SfxId
    {
        ButtonClick,
        TileSelect,
        TileSwap,
        MatchPop,
        SpecialCreate,
        SpecialActivate,
        LevelClear,
        LevelFail,
        CoinCollect,
        HeartGain,
        CatPet,
        CafeRestore,
        MeditationDraw,
        SleepEnter
    }

    public enum BgmId
    {
        HomeAmbience,
        GameplayLoop,
        MeditationAmbience,
        SleepAmbience,
        LevelClearFanfare
    }

    public enum SoundMode
    {
        Normal,
        CatOnly,
        Muted
    }

    public enum AudioBus
    {
        Master,
        Bgm,
        Sfx
    }

    public sealed class AudioService : MonoBehaviour
    {
        public static AudioService Instance { get; private set; }

        [SerializeField] private AudioMixer mixer;
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource sfxSourcePrefab;
        [SerializeField] private int sfxSourceCount = 8;
        [SerializeField] private List<SfxBinding> sfxBindings = new List<SfxBinding>();
        [SerializeField] private List<BgmBinding> bgmBindings = new List<BgmBinding>();

        private readonly Queue<AudioSource> sfxPool = new Queue<AudioSource>();
        private readonly Dictionary<SfxId, AudioClip> sfxClips = new Dictionary<SfxId, AudioClip>();
        private readonly Dictionary<BgmId, AudioClip> bgmClips = new Dictionary<BgmId, AudioClip>();
        private SoundMode mode = SoundMode.Normal;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            BuildLookups();
            WarmPool();
        }

        public void PlaySfx(SfxId id)
        {
            if (mode == SoundMode.Muted)
            {
                return;
            }

            if (sfxClips.TryGetValue(id, out AudioClip clip) == false || clip == null)
            {
                DebugLogger.Warning(LogCategory.Audio, $"Missing SFX clip: {id}");
                return;
            }

            AudioSource source = GetSfxSource();

            if (source == null)
            {
                return;
            }

            source.clip = clip;
            source.volume = GetDefaultSfxVolume(id);
            source.Play();
            StartCoroutine(ReturnWhenDone(source));
        }

        public void PlayBgm(BgmId id)
        {
            if (mode == SoundMode.Muted)
            {
                return;
            }

            if (bgmSource == null)
            {
                DebugLogger.Warning(LogCategory.Audio, "BGM source is null.");
                return;
            }

            if (bgmClips.TryGetValue(id, out AudioClip clip) == false || clip == null)
            {
                DebugLogger.Warning(LogCategory.Audio, $"Missing BGM clip: {id}");
                return;
            }

            if (bgmSource.clip == clip && bgmSource.isPlaying == true)
            {
                return;
            }

            bgmSource.clip = clip;
            bgmSource.loop = id != BgmId.LevelClearFanfare;
            bgmSource.volume = id == BgmId.LevelClearFanfare ? 0.88f : 0.55f;
            bgmSource.Play();
        }

        public void SetMode(SoundMode newMode)
        {
            mode = newMode;

            if (mode == SoundMode.Muted && bgmSource != null)
            {
                bgmSource.Stop();
            }
        }

        public void SetVolume(AudioBus bus, float value)
        {
            float clamped = Mathf.Clamp01(value);
            string parameter = bus.ToString();
            float db = clamped <= 0.0001f ? -80f : Mathf.Log10(clamped) * 20f;

            if (mixer != null)
            {
                mixer.SetFloat(parameter, db);
            }
        }

        private void BuildLookups()
        {
            sfxClips.Clear();
            bgmClips.Clear();

            for (int i = 0; i < sfxBindings.Count; i++)
            {
                if (sfxBindings[i] != null && sfxBindings[i].clip != null)
                {
                    sfxClips[sfxBindings[i].id] = sfxBindings[i].clip;
                }
            }

            for (int i = 0; i < bgmBindings.Count; i++)
            {
                if (bgmBindings[i] != null && bgmBindings[i].clip != null)
                {
                    bgmClips[bgmBindings[i].id] = bgmBindings[i].clip;
                }
            }
        }

        private void WarmPool()
        {
            if (sfxSourcePrefab == null)
            {
                GameObject go = new GameObject("SfxSourcePrefabRuntime");
                go.transform.SetParent(transform);
                sfxSourcePrefab = go.AddComponent<AudioSource>();
                sfxSourcePrefab.playOnAwake = false;
            }

            for (int i = 0; i < sfxSourceCount; i++)
            {
                AudioSource source = Instantiate(sfxSourcePrefab, transform);
                source.playOnAwake = false;
                source.gameObject.SetActive(false);
                sfxPool.Enqueue(source);
            }
        }

        private AudioSource GetSfxSource()
        {
            if (sfxPool.Count > 0)
            {
                AudioSource source = sfxPool.Dequeue();
                source.gameObject.SetActive(true);
                return source;
            }

            AudioSource expanded = Instantiate(sfxSourcePrefab, transform);
            expanded.playOnAwake = false;
            return expanded;
        }

        private System.Collections.IEnumerator ReturnWhenDone(AudioSource source)
        {
            if (source == null)
            {
                yield break;
            }

            while (source.isPlaying == true)
            {
                yield return null;
            }

            source.Stop();
            source.clip = null;
            source.gameObject.SetActive(false);
            sfxPool.Enqueue(source);
        }

        private float GetDefaultSfxVolume(SfxId id)
        {
            if (id == SfxId.LevelClear)
            {
                return 0.88f;
            }

            if (id == SfxId.CoinCollect || id == SfxId.HeartGain)
            {
                return 0.72f;
            }

            if (id == SfxId.MatchPop || id == SfxId.SpecialActivate)
            {
                return 0.78f;
            }

            return 0.55f;
        }
    }

    [System.Serializable]
    public sealed class SfxBinding
    {
        public SfxId id;
        public AudioClip clip;
    }

    [System.Serializable]
    public sealed class BgmBinding
    {
        public BgmId id;
        public AudioClip clip;
    }
}
