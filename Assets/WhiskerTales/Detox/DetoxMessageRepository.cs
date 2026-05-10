using UnityEngine;
using WhiskerTales.Core;

namespace WhiskerTales.Detox
{
    [CreateAssetMenu(fileName = "DetoxMessageRepository", menuName = "Whisker Tales/Detox Message Repository")]
    public sealed class DetoxMessageRepository : ScriptableObject
    {
        [TextArea(2, 5)]
        [SerializeField] private string[] messages =
        {
            "잠깐 숨을 고르고, 창밖의 빛을 바라봐요.",
            "고양이는 기다릴 줄 알아요. 우리도 천천히 해요.",
            "오늘의 작은 휴식도 충분히 잘한 일이에요.",
            "폰을 내려놓는 시간도 게임의 일부예요.",
            "냥이의 집은 언제나 조용히 당신을 기다려요."
        };

        public string GetRandomMessage()
        {
            if (messages == null || messages.Length == 0)
            {
                DebugLogger.Warning(LogCategory.UI, "DetoxMessageRepository has no messages.");
                return "잠깐 쉬어가도 괜찮아요.";
            }

            int index = Random.Range(0, messages.Length);
            return messages[index];
        }
    }
}
