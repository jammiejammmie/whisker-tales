using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WhiskerTales.Audio;
using WhiskerTales.Core;
using WhiskerTales.Sleep;

namespace WhiskerTales.UI
{
    /// <summary>
    /// Phase B §3-1-2. 수면 모드 화면. 진입 시 SleepModeManager.EnterSleepMode 호출,
    /// purring 루프 30%, 화면 탭 시 보상 계산 → 보상 모달 → 확인 누르면 메인으로.
    /// TV 영역은 Phase B에선 더미 패널 ("Nyang TV"). AdMob 통합은 후속.
    /// </summary>
    public class SleepModeScreen : MonoBehaviour
    {
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image catImage;
        [SerializeField] private RectTransform tvArea;
        [SerializeField] private TMP_Text tvLabel;
        [SerializeField] private TMP_Text purringIndicator;
        [SerializeField] private Button tapToWakeButton;
        [SerializeField] private AudioSource purringSource;

        [SerializeField] private GameObject rewardModal;
        [SerializeField] private TMP_Text rewardText;
        [SerializeField] private Button rewardConfirmButton;

        [SerializeField, Range(0f, 1f)] private float purringVolume = 0.3f;

        public event Action OnSleepEnded;

        private bool awakening;

        private void OnEnable()
        {
            if (tapToWakeButton != null) tapToWakeButton.onClick.AddListener(HandleTap);
            if (rewardConfirmButton != null) rewardConfirmButton.onClick.AddListener(HandleConfirm);
            if (rewardModal != null) rewardModal.SetActive(false);
            awakening = false;

            SleepModeManager.Instance?.EnterSleepMode();
            StartPurring();
        }

        private void OnDisable()
        {
            if (tapToWakeButton != null) tapToWakeButton.onClick.RemoveListener(HandleTap);
            if (rewardConfirmButton != null) rewardConfirmButton.onClick.RemoveListener(HandleConfirm);
            StopPurring();
        }

        private void StartPurring()
        {
            if (purringSource == null) return;
            if (SoundManager.Instance != null && SoundManager.Instance.CurrentMode == SoundMode.Mute) return;
            purringSource.loop = true;
            purringSource.volume = purringVolume;
            if (!purringSource.isPlaying) purringSource.Play();
            if (purringIndicator != null) purringIndicator.text = "💤 골골송 재생 중";
        }

        private void StopPurring()
        {
            if (purringSource != null && purringSource.isPlaying) purringSource.Stop();
        }

        private void HandleTap()
        {
            if (awakening) return;
            // Don't react to taps on/inside reward modal — modal has its own confirm.
            if (rewardModal != null && rewardModal.activeSelf) return;
            awakening = true;

            StopPurring();
            var reward = SleepModeManager.Instance != null ? SleepModeManager.Instance.ExitSleepMode() : default;
            ShowRewardModal(reward);
        }

        private void ShowRewardModal(SleepModeManager.SleepReward reward)
        {
            if (rewardModal != null) rewardModal.SetActive(true);
            if (rewardText != null)
            {
                string text = $"잘 쉬셨어요!\n\n" +
                              $"⏱ {reward.hours:F1} 시간 휴식\n" +
                              $"🐟 멸치 +{reward.anchovies}\n" +
                              $"♥ 호감도 +{reward.affinity}";
                if (reward.hearts > 0) text += $"\n❤ 하트 +{reward.hearts}";
                if (reward.nyangiHeart > 0) text += $"\n💝 냥이 마음 +{reward.nyangiHeart}";
                rewardText.text = text;
            }
        }

        private void HandleConfirm()
        {
            AudioManager.instance?.PlayButtonClick();
            if (rewardModal != null) rewardModal.SetActive(false);
            gameObject.SetActive(false);
            OnSleepEnded?.Invoke();
            GameManager.Instance?.ReturnToMenu();
        }
    }
}
