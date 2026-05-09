using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WhiskerTales.Cat;
using WhiskerTales.Core;
using WhiskerTales.Utilities;

namespace WhiskerTales.UI
{
    /// <summary>
    /// 고양이 교감 화면 (Stage 4 §4-2).
    /// 5마리 공통 화면, SetCat(catId)으로 대상 전환.
    /// Pet / Treat / Play 3버튼, 일일 로테이션 보너스(3개 다 쓰면) 처리.
    /// CatManager의 호감도 메서드를 호출하고 UI를 갱신.
    /// </summary>
    public class CatBondScreen : MonoBehaviour
    {
        [Serializable]
        public class CatPortraitBinding
        {
            public int catId;
            public Sprite fullshot;
        }

        [Header("Top Bar")]
        [SerializeField] private Button backButton;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private Button cameraButton;
        [SerializeField] private Button helpButton;

        [Header("Name Tag (top-left)")]
        [SerializeField] private TMP_Text catNameText;
        [SerializeField] private TMP_Text catLevelText;

        [Header("Reward Hint (top-right)")]
        [SerializeField] private TMP_Text rewardAffinityText;
        [SerializeField] private TMP_Text rewardCoinsText;

        [Header("Center")]
        [SerializeField] private Image catFullshotImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private CatPortraitBinding[] catPortraits;
        [SerializeField] private int interiorBackgroundZone = 2;
        [SerializeField] private int interiorBackgroundStage = 5;

        [Header("Affinity Bar (bottom)")]
        [SerializeField] private Image affinityProgressBar;
        [SerializeField] private TMP_Text affinityProgressText;
        [SerializeField] private TMP_Text affinityNextLevelText;

        [Header("Action Buttons")]
        [SerializeField] private Button petButton;
        [SerializeField] private Button treatButton;
        [SerializeField] private Button playButton;
        [SerializeField] private int treatCost = 50;

        [Header("FX")]
        [SerializeField] private ParticleSystem headHeartParticle;
        [SerializeField] private GameObject dailyBonusToast;
        [SerializeField] private TMP_Text dailyBonusToastText;

        [Header("Hint")]
        [SerializeField] private TMP_Text hintText;

        [Header("Daily Bonus")]
        [Tooltip("3가지 행동 모두 시 보너스 코인")]
        [SerializeField] private int dailyBonusCoins = 50;
        [Tooltip("3가지 행동 모두 시 보너스 호감도")]
        [SerializeField] private int dailyBonusAffinity = 10;

        private const int FLAG_PET = 1;
        private const int FLAG_TREAT = 2;
        private const int FLAG_PLAY = 4;
        private const int FLAG_ALL = FLAG_PET | FLAG_TREAT | FLAG_PLAY;

        private int currentCatId = Constants.CAT_NABI;

        private void OnEnable()
        {
            if (backButton != null) backButton.onClick.AddListener(HandleBackClicked);
            if (cameraButton != null) cameraButton.onClick.AddListener(HandleCameraClicked);
            if (helpButton != null) helpButton.onClick.AddListener(HandleHelpClicked);
            if (petButton != null) petButton.onClick.AddListener(HandlePetClicked);
            if (treatButton != null) treatButton.onClick.AddListener(HandleTreatClicked);
            if (playButton != null) playButton.onClick.AddListener(HandlePlayClicked);

            if (CatManager.Instance != null)
            {
                CatManager.Instance.OnCatAffinityChanged += HandleAffinityChanged;
            }

            ApplyStaticTexts();
            RefreshAll();
        }

        private void OnDisable()
        {
            if (backButton != null) backButton.onClick.RemoveListener(HandleBackClicked);
            if (cameraButton != null) cameraButton.onClick.RemoveListener(HandleCameraClicked);
            if (helpButton != null) helpButton.onClick.RemoveListener(HandleHelpClicked);
            if (petButton != null) petButton.onClick.RemoveListener(HandlePetClicked);
            if (treatButton != null) treatButton.onClick.RemoveListener(HandleTreatClicked);
            if (playButton != null) playButton.onClick.RemoveListener(HandlePlayClicked);

            if (CatManager.Instance != null)
            {
                CatManager.Instance.OnCatAffinityChanged -= HandleAffinityChanged;
            }
        }

        /// <summary>
        /// Cat Room 등에서 다른 화면이 호출하여 어떤 고양이를 표시할지 지정.
        /// </summary>
        public void SetCat(int catId)
        {
            currentCatId = catId;
            if (isActiveAndEnabled) RefreshAll();
        }

        private void ApplyStaticTexts()
        {
            if (titleText != null && I18nManager.Instance != null)
            {
                titleText.text = I18nManager.Instance.GetLocalizedText("game_title");
            }
            if (rewardAffinityText != null) rewardAffinityText.text = $"♥ +{Constants.AFFINITY_PER_PET} Affinity";
            if (rewardCoinsText != null) rewardCoinsText.text = "🐾 +10 Coins";
            if (hintText != null)
            {
                bool ko = I18nManager.Instance != null && I18nManager.Instance.currentLanguage == SystemLanguage.Korean;
                hintText.text = ko
                    ? "매일 다른 방법으로 교감하면 더 많은 보상을 받을 수 있어요!"
                    : "Bond in different ways each day to earn more rewards!";
            }
        }

        private void RefreshAll()
        {
            RefreshCatVisuals();
            RefreshNameTag();
            RefreshAffinityBar();
            RefreshBackground();
            HideToast();
        }

        private void RefreshCatVisuals()
        {
            if (catFullshotImage == null || catPortraits == null) return;
            foreach (var b in catPortraits)
            {
                if (b == null) continue;
                if (b.catId == currentCatId && b.fullshot != null)
                {
                    catFullshotImage.sprite = b.fullshot;
                    return;
                }
            }
        }

        private void RefreshNameTag()
        {
            if (CatManager.Instance == null) return;
            Core.Cat cat = CatManager.Instance.GetCat(currentCatId);
            if (cat == null) return;

            if (catNameText != null) catNameText.text = cat.name;
            if (catLevelText != null) catLevelText.text = $"⭐ Lv.{cat.affinityLevel + 1}";
        }

        private void RefreshAffinityBar()
        {
            if (CatManager.Instance == null) return;
            Core.Cat cat = CatManager.Instance.GetCat(currentCatId);
            if (cat == null) return;

            float ratio = (float)cat.affinityPoints / Constants.AFFINITY_POINTS_PER_LEVEL;
            if (affinityProgressBar != null) affinityProgressBar.fillAmount = Mathf.Clamp01(ratio);
            if (affinityProgressText != null) affinityProgressText.text = $"{cat.affinityPoints} / {Constants.AFFINITY_POINTS_PER_LEVEL}";
            if (affinityNextLevelText != null)
            {
                int nextLv = Mathf.Min(cat.affinityLevel + 2, Constants.AFFINITY_LEVEL_MAX);
                affinityNextLevelText.text = $"→ Lv.{nextLv}";
            }
        }

        private void RefreshBackground()
        {
            if (backgroundImage == null || CafeRestorationManager.instance == null) return;
            Sprite sp = CafeRestorationManager.instance.GetBackground(interiorBackgroundZone, interiorBackgroundStage);
            if (sp != null) backgroundImage.sprite = sp;
        }

        private void HandlePetClicked()
        {
            AudioManager.instance?.PlayButtonClick();
            CatManager.Instance?.PetCat(currentCatId);
            PlayHeartFx();
            MarkDailyFlag(FLAG_PET);
        }

        private void HandleTreatClicked()
        {
            AudioManager.instance?.PlayButtonClick();
            int before = GameManager.Instance?.UserProgress?.coins ?? 0;
            CatManager.Instance?.GiveSnack(currentCatId);
            int after = GameManager.Instance?.UserProgress?.coins ?? before;
            if (after < before)
            {
                PlayHeartFx();
                MarkDailyFlag(FLAG_TREAT);
            }
        }

        private void HandlePlayClicked()
        {
            AudioManager.instance?.PlayButtonClick();
            CatManager.Instance?.PlayWithCat(currentCatId);
            PlayHeartFx();
            MarkDailyFlag(FLAG_PLAY);
        }

        private void HandleCameraClicked()
        {
            AudioManager.instance?.PlayButtonClick();
            // TODO §4-8 포토 스튜디오 / 공유 시트 통합
            Debug.Log($"[CatBondScreen] Camera button (TODO: photo studio for cat {currentCatId})");
        }

        private void HandleHelpClicked()
        {
            AudioManager.instance?.PlayButtonClick();
            Debug.Log("[CatBondScreen] Help button (TODO: tutorial overlay)");
        }

        private void HandleBackClicked()
        {
            AudioManager.instance?.PlayButtonClick();
            GameManager.Instance?.RequestNavigation(NavigationTarget.CatRoom);
        }

        private void HandleAffinityChanged(int catId, int _)
        {
            if (catId != currentCatId) return;
            RefreshNameTag();
            RefreshAffinityBar();
        }

        private void PlayHeartFx()
        {
            if (headHeartParticle == null) return;
            headHeartParticle.Stop();
            headHeartParticle.Play();
        }

        // ===== 일일 로테이션 보너스 =====

        private string DailyKey => $"catbond.{currentCatId}.{DateTime.Now:yyyyMMdd}.flags";

        private void MarkDailyFlag(int flag)
        {
            int prev = PlayerPrefs.GetInt(DailyKey, 0);
            int next = prev | flag;
            if (next == prev) return;
            PlayerPrefs.SetInt(DailyKey, next);

            if ((prev & FLAG_ALL) != FLAG_ALL && (next & FLAG_ALL) == FLAG_ALL)
            {
                GrantDailyBonus();
            }
        }

        private void GrantDailyBonus()
        {
            GameManager.Instance?.AddCoins(dailyBonusCoins);
            CatManager.Instance?.IncreaseCatAffinity(currentCatId, dailyBonusAffinity);
            ShowToast(BuildBonusMessage());
        }

        private string BuildBonusMessage()
        {
            bool ko = I18nManager.Instance != null && I18nManager.Instance.currentLanguage == SystemLanguage.Korean;
            return ko
                ? $"오늘의 교감 완료! 🐾 +{dailyBonusCoins} / ♥ +{dailyBonusAffinity}"
                : $"Today's bond complete! 🐾 +{dailyBonusCoins} / ♥ +{dailyBonusAffinity}";
        }

        private void ShowToast(string message)
        {
            if (dailyBonusToast == null) return;
            if (dailyBonusToastText != null) dailyBonusToastText.text = message;
            dailyBonusToast.SetActive(true);
            StopAllCoroutines();
            StartCoroutine(HideToastAfter(3f));
        }

        private void HideToast()
        {
            if (dailyBonusToast != null) dailyBonusToast.SetActive(false);
        }

        private IEnumerator HideToastAfter(float seconds)
        {
            yield return new WaitForSecondsRealtime(seconds);
            HideToast();
        }
    }
}
