using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace WhiskerTales.UI
{
    /// <summary>
    /// 매치-3 게임플레이 우측 부스터 버튼 패널 (Stage 4 §4-3).
    /// 3종 (Hammer / ColorBomb / Shuffle). 사용 횟수 카운트 표시.
    /// 실제 효과 적용은 Board와 SpecialItem 통합 시 (후속 작업).
    /// </summary>
    public class BoosterPanel : MonoBehaviour
    {
        public enum Booster { Hammer, ColorBomb, Shuffle }

        [Serializable]
        public class BoosterEntry
        {
            public Booster type;
            public Button button;
            public TMP_Text countText;
            public int initialCount = 3;
        }

        [SerializeField] private BoosterEntry[] entries = new BoosterEntry[3];

        public event Action<Booster> OnBoosterUsed;

        private void OnEnable()
        {
            foreach (var e in entries)
            {
                if (e == null || e.button == null) continue;
                Booster captured = e.type;
                e.button.onClick.AddListener(() => HandleClicked(captured));
                RefreshCountText(e);
            }
        }

        private void OnDisable()
        {
            foreach (var e in entries)
            {
                if (e == null || e.button == null) continue;
                e.button.onClick.RemoveAllListeners();
            }
        }

        private BoosterEntry FindEntry(Booster type)
        {
            foreach (var e in entries)
            {
                if (e != null && e.type == type) return e;
            }
            return null;
        }

        private void HandleClicked(Booster type)
        {
            var entry = FindEntry(type);
            if (entry == null || entry.initialCount <= 0) return;

            AudioManager.instance?.PlayButtonClick();
            entry.initialCount--;
            RefreshCountText(entry);
            OnBoosterUsed?.Invoke(type);
            Debug.Log($"[BoosterPanel] Used {type}. Remaining: {entry.initialCount}");
        }

        private void RefreshCountText(BoosterEntry entry)
        {
            if (entry == null || entry.countText == null) return;
            entry.countText.text = entry.initialCount > 0 ? $"x{entry.initialCount}" : "0";
            if (entry.button != null) entry.button.interactable = entry.initialCount > 0;
        }
    }
}
