using System;
using UnityEngine;

namespace WhiskerTales.Currency
{
    /// <summary>
    /// Phase B §6. 신규 화폐 "냥이 마음 💝" 관리.
    /// 디톡스 행동(수면/명상/오락실)으로만 획득. IAP 차단(Master Briefing §60).
    /// 일일 캡 30, 자정(KST) 리셋. PlayerPrefs로 영구 저장.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class CurrencyManager : MonoBehaviour
    {
        public const string PREF_NYANGI_HEART  = "Currency.NyangiHeart";
        public const string PREF_DAILY_GAINED  = "Currency.NyangiHeart.DailyGained";
        public const string PREF_DAILY_DATE    = "Currency.NyangiHeart.DailyDate";
        public const int    DAILY_CAP          = 30;

        public static CurrencyManager Instance { get; private set; }

        public event Action<int> OnNyangiHeartChanged;
        public event Action OnDailyCapReached;

        private int nyangiHeart;
        private int dailyGained;
        private string dailyDate;

        public int NyangiHeart   => nyangiHeart;
        public int DailyGained   => dailyGained;
        public int DailyRemaining => Mathf.Max(0, DAILY_CAP - dailyGained);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            nyangiHeart = PlayerPrefs.GetInt(PREF_NYANGI_HEART, 0);
            dailyGained = PlayerPrefs.GetInt(PREF_DAILY_GAINED, 0);
            dailyDate   = PlayerPrefs.GetString(PREF_DAILY_DATE, Today());
            CheckMidnightReset();
        }

        private static string Today() => DateTime.Now.ToString("yyyy-MM-dd");

        private void CheckMidnightReset()
        {
            string today = Today();
            if (today != dailyDate)
            {
                dailyDate = today;
                dailyGained = 0;
                Save();
                Debug.Log("[CurrencyManager] Midnight reset of daily nyangi-heart counter.");
            }
        }

        private void Save()
        {
            PlayerPrefs.SetInt(PREF_NYANGI_HEART, nyangiHeart);
            PlayerPrefs.SetInt(PREF_DAILY_GAINED, dailyGained);
            PlayerPrefs.SetString(PREF_DAILY_DATE, dailyDate);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 디톡스 행동으로 냥이 마음 지급. 일일 캡 초과분은 자동 절단.
        /// 부분 지급된 경우에도 true. 캡으로 0 지급된 경우 false.
        /// </summary>
        public bool TryAwardNyangiHeart(int amount, string reason = "")
        {
            if (amount <= 0) return false;
            CheckMidnightReset();

            int allowable = Mathf.Min(amount, DailyRemaining);
            if (allowable <= 0)
            {
                OnDailyCapReached?.Invoke();
                Debug.Log($"[CurrencyManager] Daily cap reached, no nyangi-heart awarded (reason: {reason}).");
                return false;
            }

            nyangiHeart += allowable;
            dailyGained += allowable;
            Save();
            OnNyangiHeartChanged?.Invoke(nyangiHeart);
            Debug.Log($"[CurrencyManager] +{allowable} 💝 (reason: {reason}). Total: {nyangiHeart}, today: {dailyGained}/{DAILY_CAP}");

            if (dailyGained >= DAILY_CAP) OnDailyCapReached?.Invoke();
            return true;
        }

        public bool TrySpendNyangiHeart(int amount)
        {
            if (amount <= 0) return false;
            if (nyangiHeart < amount) return false;
            nyangiHeart -= amount;
            Save();
            OnNyangiHeartChanged?.Invoke(nyangiHeart);
            return true;
        }

        /// <summary>
        /// IAP 경로 차단. Master Briefing §60: 냥이 마음 💝는 절대 IAP로 살 수 없음.
        /// 호출 시 컴파일 에러를 발생시켜 IAPManager가 실수로 잡지 못하게 함.
        /// </summary>
        [Obsolete("IAP path BLOCKED at code level — nyangi-heart cannot be purchased per Master Briefing §60.", true)]
        public bool AwardNyangiHeartFromIAP(int amount, string productId = "")
        {
            // 컴파일 시 [Obsolete(true)] 때문에 호출 자체가 에러. 그래도 만에 하나 reflection 등으로
            // 우회 호출되더라도 즉시 거부 + 로그.
            Debug.LogError($"[CurrencyManager] IAP BLOCKED: nyangi-heart cannot be purchased ({productId}, amount={amount}).");
            return false;
        }
    }
}
