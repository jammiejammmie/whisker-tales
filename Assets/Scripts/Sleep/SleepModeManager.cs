using System;
using UnityEngine;
using WhiskerTales.Cat;
using WhiskerTales.Core;
using WhiskerTales.Currency;
using WhiskerTales.Utilities;

namespace WhiskerTales.Sleep
{
    /// <summary>
    /// Phase B §3-1-2 + Phase C-3 idle 보상 통합.
    /// EnterSleepMode: 진입 시간 기록 + 화면 밝기 0.2f.
    /// ExitSleepMode: 활성 세션의 보상 계산/적용.
    /// ProcessPendingOfflineSleep: 앱 재시작 시 PREF_ENTRY_TIME가 남아있으면 회수.
    /// 보상: 시간당 멸치 10, 30분당 호감도 +1, 8시간 풀 시 💝 +10, 매 세션마다 ❤ +2.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class SleepModeManager : MonoBehaviour
    {
        public const string PREF_ENTRY_TIME = "Sleep.EntryTimeUtc";

        public const float MAX_SLEEP_HOURS    = 8f;
        public const int   ANCHOVY_PER_HOUR   = 10;
        public const float MINUTES_PER_AFFINITY = 30f;
        public const int   NYANGI_HEART_FULL  = 10;
        public const int   HEARTS_PER_SESSION = 2;       // §C-4: 수면 모드 완료 → 하트 +2
        public const float TARGET_BRIGHTNESS  = 0.2f;

        public static SleepModeManager Instance { get; private set; }

        public struct SleepReward
        {
            public float hours;        // capped at 8
            public int   anchovies;
            public int   affinity;
            public int   nyangiHeart;
            public int   hearts;       // §C-4
        }

        private DateTime entryTimeUtc;
        private bool sleeping;
        private float originalBrightness = 1f;

        public bool IsSleeping => sleeping;
        public DateTime EntryTimeUtc => entryTimeUtc;
        public TimeSpan ElapsedSinceEntry => sleeping ? (DateTime.UtcNow - entryTimeUtc) : TimeSpan.Zero;

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

        public void EnterSleepMode()
        {
            if (sleeping) return;

            entryTimeUtc = DateTime.UtcNow;
            sleeping = true;
            originalBrightness = Screen.brightness;
            Screen.brightness = TARGET_BRIGHTNESS;

            PlayerPrefs.SetString(PREF_ENTRY_TIME, entryTimeUtc.ToString("o"));
            PlayerPrefs.Save();

            Debug.Log($"[SleepModeManager] Entered sleep mode at {entryTimeUtc:HH:mm:ss} UTC");
        }

        /// <summary>활성 sleep 세션 종료. 보상 계산 + 즉시 적용.</summary>
        public SleepReward ExitSleepMode()
        {
            if (!sleeping) return default;

            SleepReward reward = CalculateAndApplyRewards(entryTimeUtc, "sleep_mode_full_8h");

            Screen.brightness = originalBrightness;
            sleeping = false;
            PlayerPrefs.DeleteKey(PREF_ENTRY_TIME);
            PlayerPrefs.Save();

            return reward;
        }

        /// <summary>
        /// §C-3 앱 재시작 시 호출. PREF_ENTRY_TIME이 남아있으면 (이전 세션이 sleep 중 종료됐다는 뜻)
        /// 경과 시간 기준 보상 계산 + 적용 + 키 정리. 보상 없으면 default 반환.
        /// </summary>
        public SleepReward ProcessPendingOfflineSleep()
        {
            if (sleeping) return default;
            string stored = PlayerPrefs.GetString(PREF_ENTRY_TIME, "");
            if (string.IsNullOrEmpty(stored)) return default;

            if (!DateTime.TryParse(stored, null,
                System.Globalization.DateTimeStyles.RoundtripKind, out DateTime entry))
            {
                PlayerPrefs.DeleteKey(PREF_ENTRY_TIME);
                PlayerPrefs.Save();
                return default;
            }

            SleepReward reward = CalculateAndApplyRewards(entry, "sleep_mode_offline_recovery");
            PlayerPrefs.DeleteKey(PREF_ENTRY_TIME);
            PlayerPrefs.Save();
            Debug.Log($"[SleepModeManager] Offline sleep recovered — {reward.hours:F2}h, +{reward.anchovies} 멸치, +{reward.affinity} 호감도, +{reward.hearts} ❤, +{reward.nyangiHeart} 💝");
            return reward;
        }

        /// <summary>
        /// entryTime ~ DateTime.UtcNow 사이의 경과 시간으로 보상을 계산하고 즉시 적용.
        /// 활성 세션 종료 / 오프라인 복구 양쪽에서 공유.
        /// </summary>
        private SleepReward CalculateAndApplyRewards(DateTime entryTime, string nyangiReason)
        {
            TimeSpan elapsed = DateTime.UtcNow - entryTime;
            float totalHours = (float)elapsed.TotalHours;
            if (totalHours < 0) totalHours = 0;
            float cappedHours = Mathf.Min(totalHours, MAX_SLEEP_HOURS);
            float cappedMinutes = cappedHours * 60f;

            int anchovies = Mathf.FloorToInt(cappedHours * ANCHOVY_PER_HOUR);
            int affinity  = Mathf.FloorToInt(cappedMinutes / MINUTES_PER_AFFINITY);
            int nyangiHeart = totalHours >= MAX_SLEEP_HOURS ? NYANGI_HEART_FULL : 0;
            int hearts = (cappedHours > 0f) ? HEARTS_PER_SESSION : 0;

            if (anchovies > 0) GameManager.Instance?.AddCoins(anchovies);
            if (affinity > 0)
            {
                int catId = ResolveBondCatId();
                CatManager.Instance?.IncreaseCatAffinity(catId, affinity);
            }
            if (nyangiHeart > 0)
            {
                CurrencyManager.Instance?.TryAwardNyangiHeart(nyangiHeart, nyangiReason);
            }
            if (hearts > 0) GameManager.Instance?.AddLives(hearts);

            return new SleepReward
            {
                hours = cappedHours,
                anchovies = anchovies,
                affinity = affinity,
                nyangiHeart = nyangiHeart,
                hearts = hearts,
            };
        }

        private int ResolveBondCatId()
        {
            UserProgress up = GameManager.Instance?.UserProgress;
            if (up != null && up.unlockedCats != null && up.unlockedCats.Count > 0)
                return up.unlockedCats[0];
            return Constants.CAT_NABI;
        }

        private void OnApplicationPause(bool pause)
        {
            if (!sleeping) return;
            if (pause)
            {
                PlayerPrefs.SetString(PREF_ENTRY_TIME, entryTimeUtc.ToString("o"));
                PlayerPrefs.Save();
            }
        }
    }
}
