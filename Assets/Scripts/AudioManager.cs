using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 오디오 관리 시스템
/// 배경음악, 효과음, 고양이 울음소리 등을 관리합니다.
/// </summary>
public class AudioManager : MonoBehaviour
{
    // 싱글톤
    public static AudioManager instance;

    // 오디오 소스
    private AudioSource bgmSource;
    private AudioSource sfxSource;

    // 사운드 클립 (Inspector에서 할당)
    [SerializeField] private AudioClip bgmClip;
    [SerializeField] private AudioClip matchSuccessClip;
    [SerializeField] private AudioClip buttonClickClip;
    [SerializeField] private AudioClip rewardGetClip;
    [SerializeField] private AudioClip catMeowClip;

    // 사운드 클립 맵 (동적 로드용)
    private Dictionary<string, AudioClip> soundClips = new Dictionary<string, AudioClip>();

    // 음량 설정
    [Range(0f, 1f)] public float bgmVolume = 0.5f;
    [Range(0f, 1f)] public float sfxVolume = 0.7f;

    // 음소거 상태
    private bool isMuted = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializeAudioSources();
        LoadSoundClips();
        PlayBGM();
    }

    /// <summary>
    /// 오디오 소스 초기화
    /// </summary>
    private void InitializeAudioSources()
    {
        // BGM 오디오 소스 생성
        GameObject bgmObject = new GameObject("BGMSource");
        bgmObject.transform.parent = transform;
        bgmSource = bgmObject.AddComponent<AudioSource>();
        bgmSource.volume = bgmVolume;
        bgmSource.loop = true;

        // SFX 오디오 소스 생성
        GameObject sfxObject = new GameObject("SFXSource");
        sfxObject.transform.parent = transform;
        sfxSource = sfxObject.AddComponent<AudioSource>();
        sfxSource.volume = sfxVolume;
        sfxSource.loop = false;

        Debug.Log("오디오 소스 초기화 완료");
    }

    /// <summary>
    /// 사운드 클립 로드
    /// Resources/Sounds 폴더에서 모든 클립 로드
    /// </summary>
    private void LoadSoundClips()
    {
        // Inspector에서 할당된 클립들 추가
        if (bgmClip != null) soundClips["bgm"] = bgmClip;
        if (matchSuccessClip != null) soundClips["match_success"] = matchSuccessClip;
        if (buttonClickClip != null) soundClips["button_click"] = buttonClickClip;
        if (rewardGetClip != null) soundClips["reward_get"] = rewardGetClip;
        if (catMeowClip != null) soundClips["cat_meow"] = catMeowClip;

        // Resources/Sounds 폴더에서 추가 클립 로드
        AudioClip[] clips = Resources.LoadAll<AudioClip>("Sounds");
        foreach (AudioClip clip in clips)
        {
            if (!soundClips.ContainsKey(clip.name))
            {
                soundClips[clip.name] = clip;
            }
        }

        Debug.Log($"사운드 클립 {soundClips.Count}개 로드 완료");
    }

    /// <summary>
    /// 배경음악 재생
    /// </summary>
    public void PlayBGM()
    {
        if (bgmSource == null || bgmClip == null)
        {
            Debug.LogWarning("BGM을 재생할 수 없습니다. 오디오 소스 또는 클립이 없습니다.");
            return;
        }

        bgmSource.clip = bgmClip;
        bgmSource.volume = isMuted ? 0 : bgmVolume;
        bgmSource.Play();
        Debug.Log("배경음악 재생 시작");
    }

    /// <summary>
    /// 배경음악 일시정지
    /// </summary>
    public void PauseBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.Pause();
            Debug.Log("배경음악 일시정지");
        }
    }

    /// <summary>
    /// 배경음악 중지
    /// </summary>
    public void StopBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.Stop();
            Debug.Log("배경음악 중지");
        }
    }

    /// <summary>
    /// 효과음 재생
    /// </summary>
    public void PlaySFX(string sfxName)
    {
        if (isMuted)
            return;

        if (!soundClips.ContainsKey(sfxName))
        {
            Debug.LogWarning($"사운드를 찾을 수 없습니다: {sfxName}");
            return;
        }

        if (sfxSource == null)
        {
            Debug.LogWarning("SFX 오디오 소스가 없습니다.");
            return;
        }

        sfxSource.volume = sfxVolume;
        sfxSource.PlayOneShot(soundClips[sfxName]);
        Debug.Log($"효과음 재생: {sfxName}");
    }

    /// <summary>
    /// 매치 성공음 재생
    /// </summary>
    public void PlayMatchSuccess()
    {
        PlaySFX("match_success");
    }

    /// <summary>
    /// 버튼 클릭음 재생
    /// </summary>
    public void PlayButtonClick()
    {
        PlaySFX("button_click");
    }

    /// <summary>
    /// 보상 획득음 재생
    /// </summary>
    public void PlayRewardGet()
    {
        PlaySFX("reward_get");
    }

    /// <summary>
    /// 고양이 울음소리 재생
    /// </summary>
    public void PlayCatMeow()
    {
        PlaySFX("cat_meow");
    }

    /// <summary>
    /// 음소거 토글
    /// </summary>
    public void ToggleMute()
    {
        isMuted = !isMuted;
        bgmSource.volume = isMuted ? 0 : bgmVolume;
        sfxSource.volume = isMuted ? 0 : sfxVolume;
        Debug.Log($"음소거: {isMuted}");
    }

    /// <summary>
    /// 배경음악 음량 설정
    /// </summary>
    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        if (bgmSource != null)
        {
            bgmSource.volume = isMuted ? 0 : bgmVolume;
        }
    }

    /// <summary>
    /// 효과음 음량 설정
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null)
        {
            sfxSource.volume = isMuted ? 0 : sfxVolume;
        }
    }

    /// <summary>
    /// 음소거 상태 반환
    /// </summary>
    public bool IsMuted()
    {
        return isMuted;
    }

    /// <summary>
    /// 현재 배경음악 음량 반환
    /// </summary>
    public float GetBGMVolume()
    {
        return bgmVolume;
    }

    /// <summary>
    /// 현재 효과음 음량 반환
    /// </summary>
    public float GetSFXVolume()
    {
        return sfxVolume;
    }
}
