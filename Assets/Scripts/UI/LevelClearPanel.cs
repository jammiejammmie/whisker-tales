using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WhiskerTales.Core;
using WhiskerTales.Utilities;

namespace WhiskerTales.UI
{
    /// <summary>
    /// 레벨 클리어 연출 패널 (Stage 4 §4-3).
    /// 별 3개 → 코인 획득 → 카페 복원 진행 → Continue.
    /// CafeRestorationManager.OnPuzzleClear(stars) 호출하여 복원 진행도 갱신.
    /// </summary>
    public class LevelClearPanel : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private GameObject root;
        [SerializeField] private Image[] starImages = new Image[3];
        [SerializeField] private Sprite starFilled;
        [SerializeField] private Sprite starEmpty;
        [SerializeField] private TMP_Text coinRewardText;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private Button continueButton;

        [Header("Animation")]
        [SerializeField] private float starRevealDelay = 0.4f;
        [SerializeField] private float coinRevealDelay = 1.5f;

        private void OnEnable()
        {
            if (continueButton != null) continueButton.onClick.AddListener(HandleContinue);
            Hide();
        }

        private void OnDisable()
        {
            if (continueButton != null) continueButton.onClick.RemoveListener(HandleContinue);
        }

        public void Show(int stars, int coinReward)
        {
            if (root != null) root.SetActive(true);
            if (titleText != null && I18nManager.Instance != null)
            {
                titleText.text = I18nManager.Instance.GetLocalizedText("level_complete");
            }
            ResetStars();
            if (coinRewardText != null) coinRewardText.text = string.Empty;

            StopAllCoroutines();
            StartCoroutine(PlaySequence(stars, coinReward));
        }

        public void Hide()
        {
            if (root != null) root.SetActive(false);
        }

        private void ResetStars()
        {
            if (starImages == null) return;
            foreach (var img in starImages)
            {
                if (img == null) continue;
                if (starEmpty != null) img.sprite = starEmpty;
                img.transform.localScale = Vector3.one;
            }
        }

        private IEnumerator PlaySequence(int stars, int coinReward)
        {
            int clamped = Mathf.Clamp(stars, 0, 3);
            for (int i = 0; i < clamped; i++)
            {
                yield return new WaitForSecondsRealtime(starRevealDelay);
                if (i < starImages.Length && starImages[i] != null && starFilled != null)
                {
                    starImages[i].sprite = starFilled;
                    StartCoroutine(PunchScale(starImages[i].transform));
                }
                AudioManager.instance?.PlayRewardGet();
            }

            yield return new WaitForSecondsRealtime(coinRevealDelay - starRevealDelay);
            if (coinRewardText != null) coinRewardText.text = $"🐾 +{coinReward}";
            GameManager.Instance?.AddCoins(coinReward);
            AudioManager.instance?.PlayRewardGet();

            // 카페 복원 진행도 갱신
            if (CafeRestorationManager.instance != null)
            {
                CafeRestorationManager.instance.OnPuzzleClear(clamped);
            }
        }

        private IEnumerator PunchScale(Transform t)
        {
            const float duration = 0.25f;
            const float peakScale = 1.4f;
            float elapsed = 0f;
            Vector3 baseScale = Vector3.one;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float p = elapsed / duration;
                float s = Mathf.Lerp(peakScale, 1f, p);
                t.localScale = baseScale * s;
                yield return null;
            }
            t.localScale = baseScale;
        }

        private void HandleContinue()
        {
            AudioManager.instance?.PlayButtonClick();
            Hide();
            GameManager.Instance?.ReturnToMenu();
        }
    }
}
