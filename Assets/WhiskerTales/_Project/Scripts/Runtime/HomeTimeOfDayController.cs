using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using WhiskerTales.Core;
using WhiskerTales.UI;

namespace WhiskerTales.Runtime
{
    public enum HomeTimeOfDay
    {
        Dawn,
        Morning,
        Day,
        Evening,
        Night
    }

    [DisallowMultipleComponent]
    public sealed class HomeTimeOfDayController : MonoBehaviour
    {
        [Header("Background Images (crossfade pair)")]
        [SerializeField] private Image backgroundA;
        [SerializeField] private Image backgroundB;

        [Header("Sprite Pools (Inspector-assigned)")]
        [SerializeField] private Sprite[] dawnSprites;
        [SerializeField] private Sprite[] morningSprites;
        [SerializeField] private Sprite[] daySprites;
        [SerializeField] private Sprite[] eveningSprites;
        [SerializeField] private Sprite[] nightSprites;

        public event Action<HomeTimeOfDay> TimeOfDayChanged;

        public HomeTimeOfDay CurrentTimeOfDay
        {
            get { return currentTod; }
        }

        public Sprite CurrentSprite
        {
            get { return currentSprite; }
        }

        private HomeTimeOfDay currentTod;
        private Sprite currentSprite;
        private Image activeImage;
        private Image inactiveImage;
        private float nextSwapTime;
        private Tween fadeTween;
        private bool initialized;

        private void Awake()
        {
            InitializeIfNeeded();
        }

        private void OnDisable()
        {
            if (fadeTween != null)
            {
                fadeTween.Kill();
                fadeTween = null;
            }
        }

        private void Update()
        {
            if (initialized == false)
            {
                return;
            }

            HomeTimeOfDay observed = ResolveTimeOfDay(DateTime.Now.Hour);

            if (observed != currentTod)
            {
                currentTod = observed;
                SwapToRandomSprite(observed);
                ScheduleNextSwap();
                RaiseTimeOfDayChanged();
                return;
            }

            if (Time.unscaledTime >= nextSwapTime)
            {
                SwapToRandomSprite(currentTod);
                ScheduleNextSwap();
            }
        }

        public static HomeTimeOfDay ResolveTimeOfDay(int hour)
        {
            if (hour >= GameConstants.Home.DawnStartHour && hour < GameConstants.Home.MorningStartHour)
            {
                return HomeTimeOfDay.Dawn;
            }

            if (hour >= GameConstants.Home.MorningStartHour && hour < GameConstants.Home.DayStartHour)
            {
                return HomeTimeOfDay.Morning;
            }

            if (hour >= GameConstants.Home.DayStartHour && hour < GameConstants.Home.EveningStartHour)
            {
                return HomeTimeOfDay.Day;
            }

            if (hour >= GameConstants.Home.EveningStartHour && hour < GameConstants.Home.NightStartHour)
            {
                return HomeTimeOfDay.Evening;
            }

            return HomeTimeOfDay.Night;
        }

        public Sprite[] GetPool(HomeTimeOfDay tod)
        {
            switch (tod)
            {
                case HomeTimeOfDay.Dawn: return dawnSprites;
                case HomeTimeOfDay.Morning: return morningSprites;
                case HomeTimeOfDay.Day: return daySprites;
                case HomeTimeOfDay.Evening: return eveningSprites;
                case HomeTimeOfDay.Night: return nightSprites;
                default: return daySprites;
            }
        }

        private void InitializeIfNeeded()
        {
            if (initialized == true)
            {
                return;
            }

            if (backgroundA == null || backgroundB == null)
            {
                DebugLogger.Warning(LogCategory.UI, "[HomeTimeOfDayController] backgroundA/backgroundB not assigned");
                return;
            }

            activeImage = backgroundA;
            inactiveImage = backgroundB;
            Color a = activeImage.color;
            a.a = 1f;
            activeImage.color = a;
            Color b = inactiveImage.color;
            b.a = 0f;
            inactiveImage.color = b;

            currentTod = ResolveTimeOfDay(DateTime.Now.Hour);
            SwapToRandomSprite(currentTod, instant: true);
            ScheduleNextSwap();
            initialized = true;
            RaiseTimeOfDayChanged();
        }

        private void SwapToRandomSprite(HomeTimeOfDay tod)
        {
            SwapToRandomSprite(tod, instant: false);
        }

        private void SwapToRandomSprite(HomeTimeOfDay tod, bool instant)
        {
            Sprite next = PickRandomDistinct(GetPool(tod), currentSprite);

            if (next == null)
            {
                DebugLogger.Warning(LogCategory.UI, "[HomeTimeOfDayController] empty pool for " + tod);
                return;
            }

            currentSprite = next;

            if (instant == true)
            {
                activeImage.sprite = next;
                Color ac = activeImage.color;
                ac.a = 1f;
                activeImage.color = ac;
                Color bc = inactiveImage.color;
                bc.a = 0f;
                inactiveImage.color = bc;
                return;
            }

            Image incoming = inactiveImage;
            Image outgoing = activeImage;
            incoming.sprite = next;
            Color start = incoming.color;
            start.a = 0f;
            incoming.color = start;

            if (fadeTween != null)
            {
                fadeTween.Kill();
            }

            Sequence seq = DOTween.Sequence();
            seq.SetUpdate(true);
            seq.Join(incoming.DOFade(1f, UILayoutConstants.HomeBackgroundCrossfadeSeconds).SetEase(Ease.InOutSine));
            seq.Join(outgoing.DOFade(0f, UILayoutConstants.HomeBackgroundCrossfadeSeconds).SetEase(Ease.InOutSine));
            seq.OnComplete(OnFadeComplete);
            fadeTween = seq;

            activeImage = incoming;
            inactiveImage = outgoing;
        }

        private void OnFadeComplete()
        {
            fadeTween = null;
        }

        private static Sprite PickRandomDistinct(Sprite[] pool, Sprite avoid)
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
                Sprite candidate = pool[UnityEngine.Random.Range(0, pool.Length)];

                if (candidate != null && candidate != avoid)
                {
                    return candidate;
                }

                attempts++;
            }

            return pool[0];
        }

        private void ScheduleNextSwap()
        {
            float hold = UnityEngine.Random.Range(
                UILayoutConstants.HomeBackgroundMinHoldSeconds,
                UILayoutConstants.HomeBackgroundMaxHoldSeconds);
            nextSwapTime = Time.unscaledTime + hold;
        }

        private void RaiseTimeOfDayChanged()
        {
            if (TimeOfDayChanged != null)
            {
                TimeOfDayChanged.Invoke(currentTod);
            }
        }
    }
}
