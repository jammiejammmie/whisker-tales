using DG.Tweening;
using UnityEngine;
using WhiskerTales.Core;
using WhiskerTales.UI;

namespace WhiskerTales.Runtime
{
    [DisallowMultipleComponent]
    public sealed class HomeAmbienceController : MonoBehaviour
    {
        [Header("Linked Controller")]
        [SerializeField] private HomeTimeOfDayController timeOfDayController;

        [Header("Ambience Sources (two-layer crossfade)")]
        [SerializeField] private AudioSource ambienceSource;
        [SerializeField] private AudioSource ambienceSourceB;
        [SerializeField] private AudioSource nabiSource;

        [Header("Ambience Clips")]
        [SerializeField] private AudioClip[] dawnClips;
        [SerializeField] private AudioClip[] dayClips;
        [SerializeField] private AudioClip[] eveningClips;
        [SerializeField] private AudioClip[] nightClips;

        [Header("Nabi Sounds")]
        [SerializeField] private AudioClip[] nabiSoundClips;

        private HomeTimeOfDay currentTod;
        private bool hasCurrentTod;
        private float nextNabiTime;
        private AudioClip lastNabiClip;
        private Tween fadeATween;
        private Tween fadeBTween;

        private void OnEnable()
        {
            if (timeOfDayController != null)
            {
                timeOfDayController.TimeOfDayChanged += HandleTimeOfDayChanged;
                HandleTimeOfDayChanged(timeOfDayController.CurrentTimeOfDay);
            }
            else
            {
                DebugLogger.Warning(LogCategory.Audio, "[HomeAmbienceController] timeOfDayController not assigned");
            }

            ScheduleNextNabi();
        }

        private void OnDisable()
        {
            if (timeOfDayController != null)
            {
                timeOfDayController.TimeOfDayChanged -= HandleTimeOfDayChanged;
            }

            KillTweens();
        }

        private void Update()
        {
            if (nabiSource == null || nabiSoundClips == null || nabiSoundClips.Length == 0)
            {
                return;
            }

            if (Time.unscaledTime < nextNabiTime)
            {
                return;
            }

            if (nabiSource.isPlaying == true)
            {
                return;
            }

            PlayRandomNabi();
            ScheduleNextNabi();
        }

        private void HandleTimeOfDayChanged(HomeTimeOfDay tod)
        {
            AudioClip[] clips = GetAmbienceClips(tod);

            if (clips == null || clips.Length == 0)
            {
                DebugLogger.Warning(LogCategory.Audio, "[HomeAmbienceController] empty clip set for " + tod);
                return;
            }

            AudioClip layerA = clips.Length > 0 ? clips[0] : null;
            AudioClip layerB = clips.Length > 1 ? clips[1] : null;

            bool instant = hasCurrentTod == false;
            hasCurrentTod = true;
            currentTod = tod;

            ApplyLayer(ambienceSource, layerA, instant, ref fadeATween);
            ApplyLayer(ambienceSourceB, layerB, instant, ref fadeBTween);
        }

        private AudioClip[] GetAmbienceClips(HomeTimeOfDay tod)
        {
            switch (tod)
            {
                // Dawn and Morning share the dawn ambience layer per design.
                case HomeTimeOfDay.Dawn:
                case HomeTimeOfDay.Morning:
                    return dawnClips;
                case HomeTimeOfDay.Day:
                    return dayClips;
                case HomeTimeOfDay.Evening:
                    return eveningClips;
                case HomeTimeOfDay.Night:
                    return nightClips;
                default:
                    return dayClips;
            }
        }

        private void ApplyLayer(AudioSource source, AudioClip clip, bool instant, ref Tween activeTween)
        {
            if (source == null)
            {
                return;
            }

            if (clip == null)
            {
                if (activeTween != null)
                {
                    activeTween.Kill();
                    activeTween = null;
                }

                source.Stop();
                source.clip = null;
                source.volume = 0f;
                return;
            }

            if (activeTween != null)
            {
                activeTween.Kill();
                activeTween = null;
            }

            float target = UILayoutConstants.HomeAmbienceLayerVolume;

            if (instant == true)
            {
                source.clip = clip;
                source.loop = true;
                source.volume = target;
                source.Play();
                return;
            }

            float fade = UILayoutConstants.HomeAmbienceCrossfadeSeconds;
            float half = fade * 0.5f;
            AudioSource captured = source;
            AudioClip incoming = clip;

            if (captured.isPlaying == false || captured.clip == null)
            {
                captured.clip = incoming;
                captured.loop = true;
                captured.volume = 0f;
                captured.Play();
                activeTween = captured.DOFade(target, fade).SetEase(Ease.InOutSine).SetUpdate(true);
                return;
            }

            Sequence seq = DOTween.Sequence();
            seq.SetUpdate(true);
            seq.Append(captured.DOFade(0f, half).SetEase(Ease.InOutSine));
            seq.AppendCallback(delegate
            {
                captured.clip = incoming;
                captured.loop = true;
                captured.Play();
            });
            seq.Append(captured.DOFade(target, half).SetEase(Ease.InOutSine));
            activeTween = seq;
        }

        private void PlayRandomNabi()
        {
            AudioClip next = PickRandomDistinct(nabiSoundClips, lastNabiClip);

            if (next == null)
            {
                return;
            }

            lastNabiClip = next;
            float volume = Random.Range(
                UILayoutConstants.HomeNabiSoundMinVolume,
                UILayoutConstants.HomeNabiSoundMaxVolume);
            nabiSource.volume = volume;
            nabiSource.loop = false;
            nabiSource.clip = next;
            nabiSource.Play();
        }

        private static AudioClip PickRandomDistinct(AudioClip[] pool, AudioClip avoid)
        {
            if (pool == null || pool.Length == 0)
            {
                return null;
            }

            if (pool.Length == 1)
            {
                return pool[0];
            }

            int attempts = 0;

            while (attempts < 8)
            {
                AudioClip candidate = pool[Random.Range(0, pool.Length)];

                if (candidate != null && candidate != avoid)
                {
                    return candidate;
                }

                attempts++;
            }

            return pool[0];
        }

        private void ScheduleNextNabi()
        {
            float wait = Random.Range(
                UILayoutConstants.HomeNabiSoundMinIntervalSeconds,
                UILayoutConstants.HomeNabiSoundMaxIntervalSeconds);
            nextNabiTime = Time.unscaledTime + wait;
        }

        private void KillTweens()
        {
            if (fadeATween != null)
            {
                fadeATween.Kill();
                fadeATween = null;
            }

            if (fadeBTween != null)
            {
                fadeBTween.Kill();
                fadeBTween = null;
            }
        }
    }
}
