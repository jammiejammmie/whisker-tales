using System;
using UnityEngine;
using WhiskerTales.Cat;
using WhiskerTales.Core;
using WhiskerTales.Currency;
using WhiskerTales.Utilities;

namespace WhiskerTales.Sleep
{
    /// <summary>
    /// Phase B §3-1-2. 수면 모드 진입/이탈 관리.
    /// EnterSleepMode: 진입 시간 기록 + 화면 밝기 0.2f.
    /// ExitSleepMode: 경과 시간으로 보상 계산 후 적용 (+ struct 반환).
    /// 시간당 멸치 10, 30분당 호감도 +1, 8시간 풀 적립 시 💝 +10.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class SleepModeManager : MonoBehaviour
    {
        public const string PREF_ENTRY_TIME = "Sleep.EntryTimeUtc";

        public const float MAX_SLEEP_HOURS    = 8f;
        public const int   ANCHOVY_PER_HOUR   = 10;
        public const float MINUTES_PER_AFFINITY = 30f;
        public const int   NYANGI_HEART_FULL  = 10;
        public const float TARGET_BRIGHTNESS  = 0.2f;

        public static SleepModeManager Instance { get; private set; }

        public struct SleepReward
        {
            public float hours;        // capped at 8
            public int   anchovies;
            public int   affinity;
            public int   nyangiHeart;
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
            Instance = this;
        }

        public void EnterSleepMode()
        {
            if (sleeping) return;

            entryTimeUtc = DateTime.UtcNow;
            sleeping = true;
            originalBrightness = Screen.brightness;
            // Screen.brightness no-ops on unsupported platforms (Editor on Windows etc.) — safe to call.
            Screen.brightness = TARGET_BRIGHTNESS;

            PlayerPrefs.SetString(PREF_ENTRY_TIME, entryTimeUtc.ToString("o"));
            PlayerPrefs.Save();

            Debug.Log($"[SleepModeManager] Entered sleep mode at {entryTimeUtc:HH:mm:ss} UTC");
        }

        /// <summary>
        /// 수면 모드 종료. 보상 계산해서 즉시 적용 (코인/호감도/💝).
        /// </summary>
        public SleepReward ExitSleepMode()
        {
            if (!sleeping) return default;

            TimeSpan elapsed = DateTime.UtcNow - entryTimeUtc;
            float totalHours = (float)elapsed.TotalHours;
            float cappedHours = Mathf.Min(totalHours, MAX_SLEEP_HOURS);
            float cappedMinutes = cappedHours * 60f;

            int anchovies = Mathf.FloorToInt(cappedHours * ANCHOVY_PER_HOUR);
            int affinity  = Mathf.FloorToInt(cappedMinutes / MINUTES_PER_AFFINITY);
            int nyangiHeart = totalHours >= MAX_SLEEP_HOURS ? NYANGI_HEART_FULL : 0;

            // Apply rewards
            if (anchovies > 0) GameManager.Instance?.AddCoins(anchovies);

            if (affinity > 0)
            {
                int catId = ResolveBondCatId();
                CatManager.Instance?.IncreaseCatAffinity(catId, affinity);
            }

            if (nyangiHeart > 0)
            {
                CurrencyManager.Instance?.TryAwardNyangiHeart(nyangiHeart, "sleep_mode_full_8h");
            }

            // Restore brightness + clear state
            Screen.brightness = originalBrightness;
            sleeping = false;
            PlayerPrefs.DeleteKey(PREF_ENTRY_TIME);
            PlayerPrefs.Save();

            Debug.Log($"[SleepModeManager] Exited sleep mode. Elapsed: {totalHours:F2}h (capped {cappedHours:F2}h), +{anchovies} 멸치, +{affinity} 호감도, +{nyangiHeart} 💝");

            return new SleepReward
            {
                hours = cappedHours,
                anchovies = anchovies,
                affinity = affinity,
                nyangiHeart = nyangiHeart,
            };
        }

        /// <summary>
        /// 호감도를 어떤 고양이에게 줄지 결정. 일단 첫 번째 언락 고양이.
        /// 후속 작업: CatBondScreen의 마지막 선택을 PlayerPrefs로 저장 후 사용.
        /// </summary>
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
            // Save entry time on pause so cold-start recovery is possible (v1.1 처리).
            // Resume: 경과 시간은 DateTime.UtcNow가 알아서 반영.
            if (pause)
            {
                PlayerPrefs.SetString(PREF_ENTRY_TIME, entryTimeUtc.ToString("o"));
                PlayerPrefs.Save();
            }
        }
    }
}
