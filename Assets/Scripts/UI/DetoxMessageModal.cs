using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WhiskerTales.Settings;

namespace WhiskerTales.UI
{
    /// <summary>
    /// Phase B §3-1-1. 매치-3 레벨 클리어 직후 20% 확률로 띄우는 디톡스 메시지 모달.
    /// 메시지는 Resources/DetoxMessages.json에서 로드. 직전 노출 ID 제외 랜덤.
    /// SettingsManager.DetoxModeEnabled가 false면 노출 안 함.
    /// AppBootstrap이 패널 + 자식 위젯을 빌드하고 SerializeField 주입.
    /// </summary>
    public class DetoxMessageModal : MonoBehaviour
    {
        public const string PREF_LAST_SHOWN = "Detox.LastShownId";
        public const float TRIGGER_PROBABILITY = 0.33f;
        // 앱 세션당 최소 N판 클리어 후부터 모달 활성화 (튜토리얼 스팸 방지).
        // 카운터는 인스턴스 필드라 AppBootstrap이 매 앱 시작마다 새로 빌드하면 자동으로 0으로 리셋됨.
        public const int MIN_LEVELS_BEFORE_TRIGGER = 3;

        [Serializable]
        public class MessageEntry
        {
            public string id;
            public string ko;
            public string en;
        }

        [Serializable]
        private class MessageData
        {
            public MessageEntry[] messages;
        }

        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button sleepButton;

        private MessageEntry[] entries;
        private Action onConfirm;
        private Action onSleep;
        private bool entriesLoaded;
        private int sessionLevelCount;

        public bool IsShown => root != null && root.activeSelf;
        public int SessionLevelCount => sessionLevelCount;

        /// <summary>
        /// AppBootstrap.RequestPostLevelFlow가 매 레벨 클리어마다 호출. MIN_LEVELS_BEFORE_TRIGGER에
        /// 도달해야 TryShow가 실제로 확률 체크에 들어감.
        /// </summary>
        public void NotifyLevelCleared()
        {
            sessionLevelCount++;
        }

        private void EnsureLoaded()
        {
            if (entriesLoaded) return;
            entriesLoaded = true;

            TextAsset asset = Resources.Load<TextAsset>("DetoxMessages");
            if (asset == null)
            {
                Debug.LogWarning("[DetoxMessageModal] Resources/DetoxMessages.json not found.");
                entries = new MessageEntry[0];
                return;
            }
            MessageData data = JsonUtility.FromJson<MessageData>(asset.text);
            entries = data?.messages ?? new MessageEntry[0];
            Debug.Log($"[DetoxMessageModal] Loaded {entries.Length} detox messages.");
        }

        private void OnEnable()
        {
            if (confirmButton != null) confirmButton.onClick.AddListener(HandleConfirm);
            if (sleepButton != null) sleepButton.onClick.AddListener(HandleSleep);
        }

        private void OnDisable()
        {
            if (confirmButton != null) confirmButton.onClick.RemoveListener(HandleConfirm);
            if (sleepButton != null) sleepButton.onClick.RemoveListener(HandleSleep);
        }

        /// <summary>
        /// 20% 확률로 모달 표시. SettingsManager.DetoxModeEnabled=false거나 확률 미당첨이면 false 반환.
        /// 표시되면 onConfirm/onSleep 콜백을 보관해두고 버튼 탭 시 호출.
        /// </summary>
        public bool TryShow(Action onConfirmAction, Action onSleepAction)
        {
            if (sessionLevelCount < MIN_LEVELS_BEFORE_TRIGGER) return false;
            if (SettingsManager.Instance != null && !SettingsManager.Instance.DetoxModeEnabled) return false;
            if (UnityEngine.Random.value > TRIGGER_PROBABILITY) return false;

            EnsureLoaded();
            MessageEntry pick = PickRandomExcludingLast();
            if (pick == null) return false;

            onConfirm = onConfirmAction;
            onSleep = onSleepAction;

            string text = ResolveLocalized(pick);
            if (messageText != null) messageText.text = text;

            PlayerPrefs.SetString(PREF_LAST_SHOWN, pick.id);
            PlayerPrefs.Save();

            if (root != null) root.SetActive(true);
            return true;
        }

        private static string ResolveLocalized(MessageEntry e)
        {
            bool ko = I18nManager.Instance != null && I18nManager.Instance.currentLanguage == SystemLanguage.Korean;
            return ko ? e.ko : (string.IsNullOrEmpty(e.en) ? e.ko : e.en);
        }

        private MessageEntry PickRandomExcludingLast()
        {
            if (entries == null || entries.Length == 0) return null;
            string lastId = PlayerPrefs.GetString(PREF_LAST_SHOWN, "");
            if (entries.Length == 1) return entries[0];

            List<MessageEntry> available = new List<MessageEntry>(entries.Length);
            foreach (var e in entries)
            {
                if (e == null) continue;
                if (e.id != lastId) available.Add(e);
            }
            if (available.Count == 0) return entries[UnityEngine.Random.Range(0, entries.Length)];
            return available[UnityEngine.Random.Range(0, available.Count)];
        }

        private void HandleConfirm()
        {
            AudioManager.instance?.PlayButtonClick();
            if (root != null) root.SetActive(false);
            Action cb = onConfirm;
            onConfirm = null;
            onSleep = null;
            cb?.Invoke();
        }

        private void HandleSleep()
        {
            AudioManager.instance?.PlayButtonClick();
            if (root != null) root.SetActive(false);
            Action cb = onSleep;
            onConfirm = null;
            onSleep = null;
            cb?.Invoke();
        }
    }
}
