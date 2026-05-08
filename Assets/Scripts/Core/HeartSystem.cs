using System;
using UnityEngine;
using WhiskerTales.Utilities;

namespace WhiskerTales.Core
{
    /// <summary>
    /// 하트(생명) 자동 충전 시스템.
    /// Constants.LIFE_RECOVERY_MINUTES 마다 1개씩 충전, Constants.MAX_LIVES 까지.
    /// 오프라인 누적 시간 처리, GameManager.UserProgress 와 연동.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class HeartSystem : MonoBehaviour
    {
        public static HeartSystem Instance { get; private set; }

        public event Action<int> OnLivesChanged;
        public event Action<TimeSpan> OnTickRefreshTime;

        private float tickAccumulator;
        private const float TICK_INTERVAL_SECONDS = 1f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            ProcessRecoveryFromElapsed();
            EmitLivesChanged();
        }

        private void Update()
        {
            tickAccumulator += Time.unscaledDeltaTime;
            if (tickAccumulator < TICK_INTERVAL_SECONDS) return;
            tickAccumulator = 0f;

            ProcessRecoveryFromElapsed();
            EmitTickRefreshTime();
        }

        public int CurrentLives
        {
            get
            {
                var p = GetProgress();
                return p?.lives ?? 0;
            }
        }

        public bool IsFull => CurrentLives >= Constants.MAX_LIVES;

        public TimeSpan TimeUntilNextLife
        {
            get
            {
                var p = GetProgress();
                if (p == null || p.lives >= Constants.MAX_LIVES) return TimeSpan.Zero;

                DateTime nextAt = p.lastLifeRecoveryTime.AddMinutes(Constants.LIFE_RECOVERY_MINUTES);
                TimeSpan remain = nextAt - DateTime.Now;
                return remain < TimeSpan.Zero ? TimeSpan.Zero : remain;
            }
        }

        public TimeSpan TimeUntilFull
        {
            get
            {
                var p = GetProgress();
                if (p == null || p.lives >= Constants.MAX_LIVES) return TimeSpan.Zero;

                int missing = Constants.MAX_LIVES - p.lives;
                return TimeUntilNextLife + TimeSpan.FromMinutes((missing - 1) * Constants.LIFE_RECOVERY_MINUTES);
            }
        }

        /// <summary>
        /// 레벨 시작 등 외부에서 하트 1개 차감. 부족하면 false.
        /// </summary>
        public bool TrySpendLife()
        {
            var p = GetProgress();
            if (p == null) return false;
            if (p.lives <= 0) return false;

            bool wasFull = p.lives >= Constants.MAX_LIVES;
            p.lives--;
            if (wasFull)
            {
                p.lastLifeRecoveryTime = DateTime.Now;
            }

            GameManager.Instance?.SaveGame();
            EmitLivesChanged();
            return true;
        }

        /// <summary>
        /// 광고/IAP 등으로 하트 추가. MAX 초과 안 함.
        /// </summary>
        public void AddLives(int amount)
        {
            if (amount <= 0) return;
            var p = GetProgress();
            if (p == null) return;

            p.lives = Mathf.Min(p.lives + amount, Constants.MAX_LIVES);
            GameManager.Instance?.SaveGame();
            EmitLivesChanged();
        }

        private void ProcessRecoveryFromElapsed()
        {
            var p = GetProgress();
            if (p == null) return;
            if (p.lives >= Constants.MAX_LIVES) return;

            DateTime now = DateTime.Now;
            double elapsedMinutes = (now - p.lastLifeRecoveryTime).TotalMinutes;
            if (elapsedMinutes < Constants.LIFE_RECOVERY_MINUTES) return;

            int recovered = Mathf.FloorToInt((float)(elapsedMinutes / Constants.LIFE_RECOVERY_MINUTES));
            int missing = Constants.MAX_LIVES - p.lives;
            int actuallyApply = Mathf.Min(recovered, missing);
            if (actuallyApply <= 0) return;

            p.lives += actuallyApply;
            p.lastLifeRecoveryTime = p.lastLifeRecoveryTime.AddMinutes(actuallyApply * Constants.LIFE_RECOVERY_MINUTES);

            if (p.lives >= Constants.MAX_LIVES)
            {
                p.lastLifeRecoveryTime = now;
            }

            GameManager.Instance?.SaveGame();
            EmitLivesChanged();
        }

        private UserProgress GetProgress()
        {
            return GameManager.Instance?.UserProgress;
        }

        private void EmitLivesChanged()
        {
            OnLivesChanged?.Invoke(CurrentLives);
        }

        private void EmitTickRefreshTime()
        {
            OnTickRefreshTime?.Invoke(TimeUntilNextLife);
        }
    }
}
