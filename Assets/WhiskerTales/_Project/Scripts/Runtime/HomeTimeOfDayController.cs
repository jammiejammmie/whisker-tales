using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using WhiskerTales.Core;
using WhiskerTales.UI;

namespace WhiskerTales.Runtime
{
    /// <summary>
    /// 유저 기기 시간 기준으로 홈 배경을 새벽/낮/저녁/밤 4단계로 자동 전환.
    /// 전환은 두 장의 Image를 겹쳐 alpha CrossFade로 처리 — 깜빡임/플래시 없음.
    /// </summary>
    public sealed class HomeTimeOfDayController : MonoBehaviour
    {
        public enum TimeOfDay
        {
            Dawn,
            Day,
            Evening,
            Night
        }

        [Header("Crossfade Layers")]
        [Tooltip("두 장의 Image를 자식으로 두고, 같은 RectTransform으로 stretch — 둘이 번갈아가며 fade in/out.")]
        [SerializeField] private Image layerA;
        [SerializeField] private Image layerB;

        [Header("Sprites")]
        [SerializeField] private Sprite dawnSprite;
        [SerializeField] private Sprite daySprite;
        [SerializeField] private Sprite eveningSprite;
        [SerializeField] private Sprite nightSprite;

        private Image currentLayer;
        private Image nextLayer;
        private TimeOfDay currentTimeOfDay;
        private float checkAccumulator;
        private Tween activeTween;
        private bool initialized;

        /// <summary>구독자가 OnEnable 시점에 현재 시간대를 동기적으로 조회하기 위한 프로퍼티.</summary>
        public TimeOfDay CurrentTimeOfDay
        {
            get { return currentTimeOfDay; }
        }

        /// <summary>시간대 경계 전환 시 발생. 페이드와 동시에 fire — 구독자는 동일 기간 동안 자체 transition 가능.</summary>
        public event System.Action<TimeOfDay> TimeOfDayChanged;

        private void OnEnable()
        {
            if (initialized == false)
            {
                InitializeImmediate();
                initialized = true;
            }
        }

        private void OnDisable()
        {
            activeTween?.Kill();
            activeTween = null;
        }

        private void Update()
        {
            checkAccumulator += Time.unscaledDeltaTime;

            if (checkAccumulator < GameConstants.Home.BackgroundCheckIntervalSeconds)
            {
                return;
            }

            checkAccumulator = 0f;

            TimeOfDay now = ResolveTimeOfDay(DateTime.Now.Hour);

            if (now == currentTimeOfDay)
            {
                return;
            }

            CrossFadeTo(now);
        }

        private void InitializeImmediate()
        {
            if (layerA == null || layerB == null)
            {
                DebugLogger.Warning(LogCategory.UI, "HomeTimeOfDayController: layerA/layerB not assigned.");
                return;
            }

            currentLayer = layerA;
            nextLayer = layerB;

            currentTimeOfDay = ResolveTimeOfDay(DateTime.Now.Hour);
            Sprite sprite = ResolveSprite(currentTimeOfDay);

            if (sprite == null)
            {
                DebugLogger.Warning(LogCategory.UI, "HomeTimeOfDayController: sprite missing for " + currentTimeOfDay);
            }

            ApplyLayer(currentLayer, sprite, 1f, true);
            ApplyLayer(nextLayer, null, 0f, false);

            DebugLogger.Info(LogCategory.UI, "HomeTimeOfDayController: initial timeOfDay=" + currentTimeOfDay);
        }

        private void CrossFadeTo(TimeOfDay target)
        {
            if (currentLayer == null || nextLayer == null)
            {
                return;
            }

            Sprite sprite = ResolveSprite(target);

            if (sprite == null)
            {
                DebugLogger.Warning(LogCategory.UI, "HomeTimeOfDayController: sprite missing for " + target);
                return;
            }

            activeTween?.Kill();

            ApplyLayer(nextLayer, sprite, 0f, true);
            nextLayer.rectTransform.SetAsLastSibling();

            Image fadingIn = nextLayer;
            Image fadingOut = currentLayer;

            currentLayer = fadingIn;
            nextLayer = fadingOut;
            currentTimeOfDay = target;

            DebugLogger.Info(LogCategory.UI, "HomeTimeOfDayController: crossfade -> " + target);

            // 구독자에게 fire — 같은 fade duration 동안 자체 transition 가능 (e.g., nabi tint).
            if (TimeOfDayChanged != null)
            {
                TimeOfDayChanged.Invoke(target);
            }

            activeTween = fadingIn
                .DOFade(1f, UILayoutConstants.HomeBackgroundCrossfadeSeconds)
                .SetEase(Ease.InOutSine)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    activeTween = null;
                    ApplyLayer(fadingOut, null, 0f, false);
                });
        }

        private static void ApplyLayer(Image image, Sprite sprite, float alpha, bool active)
        {
            if (image == null)
            {
                return;
            }

            if (sprite != null)
            {
                image.sprite = sprite;
            }

            Color c = image.color;
            c.a = alpha;
            image.color = c;
            image.raycastTarget = false;
            image.preserveAspect = false;

            if (image.gameObject.activeSelf != active)
            {
                image.gameObject.SetActive(active);
            }
        }

        private Sprite ResolveSprite(TimeOfDay timeOfDay)
        {
            switch (timeOfDay)
            {
                case TimeOfDay.Dawn:
                    return dawnSprite;
                case TimeOfDay.Day:
                    return daySprite;
                case TimeOfDay.Evening:
                    return eveningSprite;
                case TimeOfDay.Night:
                    return nightSprite;
                default:
                    return null;
            }
        }

        // Boundaries: Dawn [04,07) / Day [07,17) / Evening [17,20) / Night [20,04).
        private static TimeOfDay ResolveTimeOfDay(int hour)
        {
            if (hour >= GameConstants.Home.DawnStartHour && hour < GameConstants.Home.DayStartHour)
            {
                return TimeOfDay.Dawn;
            }

            if (hour >= GameConstants.Home.DayStartHour && hour < GameConstants.Home.EveningStartHour)
            {
                return TimeOfDay.Day;
            }

            if (hour >= GameConstants.Home.EveningStartHour && hour < GameConstants.Home.NightStartHour)
            {
                return TimeOfDay.Evening;
            }

            return TimeOfDay.Night;
        }
    }
}
