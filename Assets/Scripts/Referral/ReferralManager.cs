using System.Text.RegularExpressions;
using UnityEngine;
using WhiskerTales.Cat;
using WhiskerTales.Core;
using WhiskerTales.Utilities;

namespace WhiskerTales.Referral
{
    /// <summary>
    /// Phase C-2 레퍼럴 시스템 (서버 없이 v1.0).
    /// MyCode: 형식 [고양이이름]-[4자리숫자] (예: NABI-1234). 첫 호출 시 생성 후 PlayerPrefs로 영구 고정.
    /// 친구 코드 입력 시 +3 하트 (1회만, AddLives 통해 MAX_LIVES cap 적용).
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class ReferralManager : MonoBehaviour
    {
        public const string PREF_MY_CODE = "Referral.MyCode";
        public const string PREF_FRIEND_REDEEMED = "Referral.FriendRedeemed";
        public const string PREF_FRIEND_CODE_USED = "Referral.FriendCodeUsed";
        public const int FRIEND_REDEEM_LIVES = 3;

        // 형식: 5종 고양이 이름 중 하나 + 하이픈 + 4자리 숫자
        private static readonly Regex CODE_REGEX = new Regex(
            @"^(NABI|BELLA|SAMI|HODU|GUREUMI)-[0-9]{4}$",
            RegexOptions.Compiled);

        public static ReferralManager Instance { get; private set; }

        public string MyCode
        {
            get
            {
                string code = PlayerPrefs.GetString(PREF_MY_CODE, "");
                if (string.IsNullOrEmpty(code))
                {
                    code = GenerateCode();
                    PlayerPrefs.SetString(PREF_MY_CODE, code);
                    PlayerPrefs.Save();
                    Debug.Log($"[Referral] Generated MyCode: {code}");
                }
                return code;
            }
        }

        public bool IsFriendCodeRedeemed => PlayerPrefs.GetInt(PREF_FRIEND_REDEEMED, 0) == 1;
        public string FriendCodeUsed => PlayerPrefs.GetString(PREF_FRIEND_CODE_USED, "");

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

        private string GenerateCode()
        {
            string catName = ResolveCatName();
            int n = Random.Range(1000, 10000); // 4-digit
            return $"{catName}-{n}";
        }

        /// <summary>
        /// 첫 언락 고양이 이름(대문자)으로. 미설정 시 NABI 기본.
        /// </summary>
        private string ResolveCatName()
        {
            UserProgress up = GameManager.Instance?.UserProgress;
            if (up != null && up.unlockedCats != null && up.unlockedCats.Count > 0)
            {
                int catId = up.unlockedCats[0];
                string id = CatManager.GetCatNameId(catId);
                if (!string.IsNullOrEmpty(id) && id != "unknown") return id.ToUpperInvariant();
            }
            return "NABI";
        }

        /// <summary>
        /// 친구 코드 사용 시도. 검증 통과 시 +3 하트 지급 (이미 사용했으면 false).
        /// </summary>
        public bool TryRedeemFriendCode(string code, out string failReason)
        {
            failReason = "";
            if (string.IsNullOrWhiteSpace(code))
            {
                failReason = "코드를 입력해주세요";
                return false;
            }
            string normalized = code.Trim().ToUpperInvariant();

            if (!IsValidFormat(normalized))
            {
                failReason = "코드 형식이 올바르지 않아요 (예: NABI-1234)";
                return false;
            }
            if (IsFriendCodeRedeemed)
            {
                failReason = "이미 친구 코드를 사용했어요";
                return false;
            }
            if (normalized == MyCode)
            {
                failReason = "자신의 코드는 사용할 수 없어요";
                return false;
            }

            GameManager.Instance?.AddLives(FRIEND_REDEEM_LIVES);

            PlayerPrefs.SetInt(PREF_FRIEND_REDEEMED, 1);
            PlayerPrefs.SetString(PREF_FRIEND_CODE_USED, normalized);
            PlayerPrefs.Save();

            Debug.Log($"[Referral] Redeemed friend code {normalized} → +{FRIEND_REDEEM_LIVES} hearts");
            return true;
        }

        public static bool IsValidFormat(string code)
        {
            if (string.IsNullOrEmpty(code)) return false;
            return CODE_REGEX.IsMatch(code);
        }

        public static int ParseCatIdFromCode(string code)
        {
            if (string.IsNullOrEmpty(code)) return Constants.CAT_NABI;
            int dash = code.IndexOf('-');
            if (dash <= 0) return Constants.CAT_NABI;
            string name = code.Substring(0, dash).ToUpperInvariant();
            switch (name)
            {
                case "NABI":    return Constants.CAT_NABI;
                case "BELLA":   return Constants.CAT_BELLA;
                case "SAMI":    return Constants.CAT_SAMI;
                case "HODU":    return Constants.CAT_HODU;
                case "GUREUMI": return Constants.CAT_GUREUMI;
                default:        return Constants.CAT_NABI;
            }
        }

#if UNITY_EDITOR
        public void DebugReset()
        {
            PlayerPrefs.DeleteKey(PREF_MY_CODE);
            PlayerPrefs.DeleteKey(PREF_FRIEND_REDEEMED);
            PlayerPrefs.DeleteKey(PREF_FRIEND_CODE_USED);
            PlayerPrefs.Save();
        }
#endif
    }
}
