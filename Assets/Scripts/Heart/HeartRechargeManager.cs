using System;
using UnityEngine;
using WhiskerTales.Core;

namespace WhiskerTales.Heart
{
    /// <summary>
    /// Phase C-4 하트 충전 시스템.
    /// 1. 시간당 1개 자동 충전 (Awake/EnsureInitialized 시 LastRechargeUtc → 경과 시간 → AddLives)
    /// 2. SNS 공유 → +1 (1일 1회)
    /// 3. 수면 모드 완료 → +2 (SleepModeManager.HEARTS_PER_SESSION에서 처리)
    /// 4. 친구 코드 입력 → +3 (ReferralManager에서 처리)
    /// 모든 ‘+N’은 GameManager.AddLives 호출 — MAX_LIVES=5 cap 자동 적용.
    /// </summary>
    [DefaultExecutionOrder(-90)] // GameManager(-150)/CurrencyManager(-100)보다 뒤
    public class HeartRechargeManager : MonoBehaviour
    {
        public const string PREF_LAST_RECHARGE       = "Heart.LastRechargeUtc";
        public const string PREF_LAST_SHARE_REWARD   = "Heart.LastShareRewardDate";
        public const int   HEARTS_PER_HOUR           = 1;
        public const int   HEARTS_PER_SHARE          = 1;

        public static HeartRechargeManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            EnsureInitialized();
            ProcessOfflineRecharge();
        }

        public void EnsureInitialized()
        {
            Instance = this;
        }

        /// <summary>
        /// 앱 시작/Awake 시 호출. PREF_LAST_RECHARGE 이후 경과 시간만큼 하트 추가.
        /// 첫 실행 시 (PREF 없음) 현재 시각만 저장하고 nothing 추가.
        /// </summary>
        public int ProcessOfflineRecharge()
        {
            DateTime now = DateTime.UtcNow;
            string stored = PlayerPrefs.GetString(PREF_LAST_RECHARGE, "");

            if (string.IsNullOrEmpty(stored)
                || !DateTime.TryParse(stored, null,
                    System.Globalization.DateTimeStyles.RoundtripKind, out DateTime last))
            {
                // First run / corrupt — just stamp now, no recharge.
                PlayerPrefs.SetString(PREF_LAST_RECHARGE, now.ToString("o"));
                PlayerPrefs.Save();
                return 0;
            }

            TimeSpan elapsed = now - last;
            int hours = (int)elapsed.TotalHours;
            if (hours <= 0) return 0;

            int hearts = hours * HEARTS_PER_HOUR;
            GameManager.Instance?.AddLives(hearts);

            // LastRechargeUtc는 정수 시간 단위로만 진행 (분/초 잔여분은 다음 회수에 사용).
            DateTime newLast = last.AddHours(hours);
            PlayerPrefs.SetString(PREF_LAST_RECHARGE, newLast.ToString("o"));
            PlayerPrefs.Save();

            Debug.Log($"[HeartRecharge] Offline +{hearts} hearts ({elapsed.TotalHours:F2}h elapsed, {hours}h consumed)");
            return hearts;
        }

        /// <summary>
        /// SNS 공유 후 1일 1회 +1 하트. 이미 오늘 받았으면 false.
        /// </summary>
        public bool TryAwardShareHeart()
        {
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            string lastDate = PlayerPrefs.GetString(PREF_LAST_SHARE_REWARD, "");
            if (lastDate == today) return false;

            PlayerPrefs.SetString(PREF_LAST_SHARE_REWARD, today);
            PlayerPrefs.Save();
            GameManager.Instance?.AddLives(HEARTS_PER_SHARE);
            Debug.Log($"[HeartRecharge] +{HEARTS_PER_SHARE} heart from SNS share (date={today})");
            return true;
        }

#if UNITY_EDITOR
        public void DebugReset()
        {
            PlayerPrefs.DeleteKey(PREF_LAST_RECHARGE);
            PlayerPrefs.DeleteKey(PREF_LAST_SHARE_REWARD);
            PlayerPrefs.Save();
        }

        public void DebugSetLastRechargeHoursAgo(int hours)
        {
            DateTime t = DateTime.UtcNow.AddHours(-hours);
            PlayerPrefs.SetString(PREF_LAST_RECHARGE, t.ToString("o"));
            PlayerPrefs.Save();
        }
#endif
    }
}
