using System;
using UnityEngine;

namespace WhiskerTales.UI
{
    /// <summary>
    /// 타이틀 화면 일일 카피 ("오늘의 빛깔") 로테이션.
    /// 날짜 기반 시드로 결정되어 같은 날엔 같은 카피가 표시됨.
    /// </summary>
    public static class DailyCopy
    {
        private static readonly string[] CopiesKo =
        {
            "오늘 당신의 시간은 어떤 빛깔이었나요?",
            "고양이가 잠들었어요. 당신도 쉬어가세요.",
            "잠시 폰을 내려놓고, 진짜 당신의 삶을 돌보세요.",
            "막혀도 괜찮아요. 고양이가 당신 곁에 있으니까요.",
            "오늘 하루도 수고하셨어요.",
            "따뜻한 차 한 잔, 어떠세요?",
            "한옥의 처마 끝에 햇살이 머무는 시간.",
        };

        private static readonly string[] CopiesEn =
        {
            "What color was your day today?",
            "The cat fell asleep. Take a rest, too.",
            "Put down your phone for a moment, and tend to your real life.",
            "It's okay to get stuck. The cats are by your side.",
            "Thank you for getting through today.",
            "How about a warm cup of tea?",
            "Sunlight lingers on the eaves of the hanok.",
        };

        // 인계 패킷 §5 컬러 팔레트의 따뜻한 색조들
        private static readonly Color[] AccentColors =
        {
            new Color(0.91f, 0.66f, 0.49f), // #E8A87C 코랄
            new Color(0.96f, 0.65f, 0.73f), // #F4A7B9 핑크
            new Color(0.49f, 0.72f, 0.49f), // #7CB87C 초록
            new Color(0.48f, 0.66f, 0.74f), // #7BA7BC 파랑
            new Color(0.83f, 0.66f, 0.28f), // #D4A847 금색
            new Color(0.55f, 0.45f, 0.33f), // #8B7355 나무톤
            new Color(0.60f, 0.50f, 0.70f), // 보라
        };

        public struct DailyEntry
        {
            public string Text;
            public Color Accent;
        }

        public static DailyEntry GetToday()
        {
            int seed = DateTime.Now.Year * 10000 + DateTime.Now.Month * 100 + DateTime.Now.Day;
            string text = PickByLanguage(seed);
            Color color = AccentColors[seed % AccentColors.Length];
            return new DailyEntry { Text = text, Accent = color };
        }

        private static string PickByLanguage(int seed)
        {
            bool ko = I18nManager.Instance != null && I18nManager.Instance.currentLanguage == SystemLanguage.Korean;
            string[] pool = ko ? CopiesKo : CopiesEn;
            return pool[seed % pool.Length];
        }
    }
}
