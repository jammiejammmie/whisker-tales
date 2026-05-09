using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WhiskerTales.Core;

namespace WhiskerTales.UI
{
    /// <summary>
    /// Phase C 튜토리얼 — 처음 3레벨에서 한 번씩 노출. 손가락(paw) 펄스 애니메이션으로 가이드.
    /// PlayerPrefs로 레벨별 완료 여부 기록 (Tutorial.Level{N}Seen).
    /// AppBootstrap이 패널 + 자식 위젯 빌드 후 SerializeField 주입.
    /// </summary>
    public class TutorialOverlay : MonoBehaviour
    {
        public const string PREF_PREFIX = "Tutorial.Level";
        public const int MAX_TUTORIAL_LEVEL = 3;

        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text hintText;
        [SerializeField] private RectTransform fingerHand;
        [SerializeField] private Button confirmButton;

        [Header("Tuning")]
        [SerializeField] private float pulsePeriodSeconds = 0.8f;
        [SerializeField] private float pulseScaleMin = 0.85f;
        [SerializeField] private float pulseScaleMax = 1.25f;

        private int currentLevel;
        private Coroutine fingerLoop;

        public bool IsShown => root != null && root.activeSelf;
        public int CurrentLevel => currentLevel;

        public static string KeyForLevel(int level) => $"{PREF_PREFIX}{level}Seen";

        public static bool IsLevelSeen(int level)
        {
            if (level < 1 || level > MAX_TUTORIAL_LEVEL) return true;
            return PlayerPrefs.GetInt(KeyForLevel(level), 0) == 1;
        }

        public static void MarkLevelSeen(int level)
        {
            if (level < 1 || level > MAX_TUTORIAL_LEVEL) return;
            PlayerPrefs.SetInt(KeyForLevel(level), 1);
            PlayerPrefs.Save();
        }

        public static void DebugClearAll()
        {
            for (int l = 1; l <= MAX_TUTORIAL_LEVEL; l++)
                PlayerPrefs.DeleteKey(KeyForLevel(l));
            PlayerPrefs.Save();
        }

        private void OnEnable()
        {
            if (confirmButton != null) confirmButton.onClick.AddListener(HandleConfirm);
        }

        private void OnDisable()
        {
            if (confirmButton != null) confirmButton.onClick.RemoveListener(HandleConfirm);
            StopFinger();
        }

        /// <summary>
        /// 해당 레벨 튜토리얼이 아직 안 봤다면 표시. 표시되면 true.
        /// 이미 봤거나 범위 밖이면 false.
        /// </summary>
        public bool TryShowForLevel(int level)
        {
            if (level < 1 || level > MAX_TUTORIAL_LEVEL) return false;
            if (IsLevelSeen(level)) return false;

            currentLevel = level;
            ApplyHint(level);
            ApplyFingerPosition(level);

            if (root != null) root.SetActive(true);
            StartFingerLoop();
            return true;
        }

        private void ApplyHint(int level)
        {
            if (hintText == null) return;
            switch (level)
            {
                case 1: hintText.text = "같은 타일 3개를\n가로 또는 세로로 맞춰보세요!"; break;
                case 2: hintText.text = "타일 4개를 일직선으로 맞추면\n로켓이 생겨요! 🚀"; break;
                case 3: hintText.text = "왼쪽 위의 목표를\n달성하면 레벨 클리어!"; break;
                default: hintText.text = ""; break;
            }
        }

        /// <summary>
        /// 레벨별 손가락 위치(canvas-center 기준). 보드/HUD/목표 위치를 대략 가리킴.
        /// 정밀 좌표 매핑은 후속 (실제 board cell hit-test 등).
        /// </summary>
        private void ApplyFingerPosition(int level)
        {
            if (fingerHand == null) return;
            Vector2 pos;
            switch (level)
            {
                case 1: pos = new Vector2(-50, -100); break;  // 보드 중앙 약간 좌측
                case 2: pos = new Vector2(150, 0); break;     // 보드 우측 (로켓 생성 위치 예시)
                case 3: pos = new Vector2(-360, 600); break;  // HUD 좌상단 (목표 텍스트)
                default: pos = Vector2.zero; break;
            }
            fingerHand.anchoredPosition = pos;
            fingerHand.localScale = Vector3.one;
        }

        private void StartFingerLoop()
        {
            StopFinger();
            if (fingerHand == null) return;
            fingerLoop = StartCoroutine(FingerPulseLoop());
        }

        private void StopFinger()
        {
            if (fingerLoop != null) { StopCoroutine(fingerLoop); fingerLoop = null; }
            if (fingerHand != null) fingerHand.localScale = Vector3.one;
        }

        private IEnumerator FingerPulseLoop()
        {
            while (true)
            {
                float t = 0f;
                while (t < pulsePeriodSeconds)
                {
                    t += Time.unscaledDeltaTime;
                    float p = t / pulsePeriodSeconds;
                    float s = Mathf.Lerp(pulseScaleMin, pulseScaleMax,
                        0.5f * (1f - Mathf.Cos(p * Mathf.PI * 2f)));
                    if (fingerHand != null) fingerHand.localScale = Vector3.one * s;
                    yield return null;
                }
            }
        }

        private void HandleConfirm()
        {
            AudioManager.instance?.PlayButtonClick();
            MarkLevelSeen(currentLevel);
            StopFinger();
            if (root != null) root.SetActive(false);
        }

#if UNITY_EDITOR
        public bool DebugTryShow(int level) => TryShowForLevel(level);
        public void DebugMarkSeen(int level) => MarkLevelSeen(level);
        public void DebugClearAllSeen() => DebugClearAll();
#endif
    }
}
