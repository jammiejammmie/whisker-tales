using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WhiskerTales.Sleep;

namespace WhiskerTales.UI
{
    /// <summary>
    /// Phase C-3 방치형 보상 수령 모달.
    /// 앱 재접속 시 SleepModeManager.ProcessPendingOfflineSleep 결과 (그리고 향후 카페 운영 모드 누적)
    /// 가 있으면 AppBootstrap이 Show(reward) 호출해서 보여줌.
    /// "그동안 X시간 자리를 비웠어요" + 멸치/호감도/하트/💝 breakdown.
    /// </summary>
    public class IdleRewardModal : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text rewardText;
        [SerializeField] private Button confirmButton;

        public bool IsShown => root != null && root.activeSelf;

        private void OnEnable()
        {
            if (confirmButton != null) confirmButton.onClick.AddListener(HandleConfirm);
        }

        private void OnDisable()
        {
            if (confirmButton != null) confirmButton.onClick.RemoveListener(HandleConfirm);
        }

        public void Show(SleepModeManager.SleepReward reward)
        {
            if (titleText != null) titleText.text = $"그동안 {reward.hours:F1}시간 자리를 비웠어요";
            if (rewardText != null) rewardText.text = BuildRewardSummary(reward);
            if (root != null) root.SetActive(true);
        }

        public void Hide()
        {
            if (root != null) root.SetActive(false);
        }

        public static string BuildRewardSummary(SleepModeManager.SleepReward reward)
        {
            StringBuilder sb = new StringBuilder();
            if (reward.anchovies > 0) sb.AppendLine($"멸치 +{reward.anchovies}");
            if (reward.affinity > 0)  sb.AppendLine($"호감도 +{reward.affinity}");
            if (reward.hearts > 0)    sb.AppendLine($"❤ 하트 +{reward.hearts}");
            if (reward.nyangiHeart > 0) sb.AppendLine($"💝 냥이 마음 +{reward.nyangiHeart}");
            if (sb.Length == 0) sb.AppendLine("(보상 없음)");
            return sb.ToString().TrimEnd();
        }

        private void HandleConfirm()
        {
            AudioManager.instance?.PlayButtonClick();
            Hide();
        }
    }
}
