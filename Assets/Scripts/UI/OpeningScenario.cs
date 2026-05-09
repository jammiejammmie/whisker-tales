using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace WhiskerTales.UI
{
    /// <summary>
    /// 오프닝 시나리오 (Stage 4 §4-10). 8개 scene tap-to-advance 시네마틱.
    /// 최초 1회만 재생 — PlayerPrefs.OpeningScenarioSeen=1로 마킹.
    /// 완료 또는 Skip 시 OnComplete 발행, AppBootstrap이 Level 1로 전환.
    /// </summary>
    public class OpeningScenario : MonoBehaviour
    {
        public const string PREF_SEEN = "OpeningScenarioSeen";

        [Header("Visuals")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private RectTransform letterPanel;
        [SerializeField] private TMP_Text letterText;
        [SerializeField] private Image catImage;
        [SerializeField] private TMP_Text narrationText;

        [Header("Buttons")]
        [SerializeField] private Button tapAreaButton;
        [SerializeField] private Button skipButton;
        [SerializeField] private Button startButton;

        [Header("Assets (injected by AppBootstrap)")]
        [SerializeField] private Sprite cafeBgSprite;
        [SerializeField] private Sprite catSprite;
        [SerializeField] private AudioClip catMeowClip;
        [SerializeField] private AudioSource sfxSource;

        [Header("Tuning")]
        [SerializeField] private float tapCooldown = 0.3f;

        public event Action OnComplete;

        private struct Scene
        {
            public string narration;
            public bool showCafeBg;
            public bool showLetter;
            public bool showCat;
            public bool showStartButton;
            public bool playCatMeow;
        }

        private Scene[] scenes;
        private int currentScene;
        private float lastTap;
        private bool letterShown;
        private bool finished;

        private void Awake()
        {
            scenes = new[]
            {
                new Scene { narration = "비가 내리는 어느 날 밤..." },
                new Scene { narration = "할머니 유품을 정리하다\n한 통의 편지를 발견했다." },
                new Scene { narration = "내가 평생을 바친 냥이의 집이\n마음에 걸리는구나.\n\n그곳 아이들은 이제\n갈 곳이 없단다.", showLetter = true },
                new Scene { narration = "", showCafeBg = true },
                new Scene { narration = "세상에... 정말 엉망이네.\n하지만 할머니의 온기가\n아직 남아있는 것 같아.", showCafeBg = true },
                new Scene { narration = "마당 한구석에서\n바스락 소리...", showCafeBg = true },
                new Scene { narration = "...앗, 고양이?!", showCafeBg = true, showCat = true, playCatMeow = true },
                new Scene { narration = "나비가 편히 쉴 수 있도록\n마당의 잡초를 제거해주세요!", showCafeBg = true, showCat = true, showStartButton = true },
            };
        }

        private void OnEnable()
        {
            if (tapAreaButton != null) tapAreaButton.onClick.AddListener(HandleTap);
            if (skipButton != null) skipButton.onClick.AddListener(HandleSkip);
            if (startButton != null) startButton.onClick.AddListener(HandleStart);

            currentScene = 0;
            letterShown = false;
            finished = false;
            ShowCurrentScene();
        }

        private void OnDisable()
        {
            if (tapAreaButton != null) tapAreaButton.onClick.RemoveListener(HandleTap);
            if (skipButton != null) skipButton.onClick.RemoveListener(HandleSkip);
            if (startButton != null) startButton.onClick.RemoveListener(HandleStart);
            StopAllCoroutines();
        }

        private void HandleTap()
        {
            if (finished) return;
            if (Time.unscaledTime - lastTap < tapCooldown) return;
            lastTap = Time.unscaledTime;

            // Don't advance off the last scene via tap — user must press Start.
            if (currentScene >= scenes.Length - 1 && scenes[scenes.Length - 1].showStartButton) return;

            currentScene++;
            if (currentScene >= scenes.Length) { Complete(); return; }
            ShowCurrentScene();
        }

        private void HandleSkip()
        {
            if (finished) return;
            Complete();
        }

        private void HandleStart()
        {
            if (finished) return;
            Complete();
        }

        private void ShowCurrentScene()
        {
            if (currentScene < 0 || currentScene >= scenes.Length) return;
            Scene s = scenes[currentScene];

            // Background
            if (backgroundImage != null)
            {
                if (s.showCafeBg && cafeBgSprite != null)
                {
                    backgroundImage.sprite = cafeBgSprite;
                    backgroundImage.color = Color.white;
                }
                else
                {
                    backgroundImage.sprite = null;
                    backgroundImage.color = new Color(0.10f, 0.12f, 0.18f);
                }
            }

            // Letter (with one-shot zoom-in animation when first appearing)
            if (s.showLetter)
            {
                if (letterPanel != null)
                {
                    letterPanel.gameObject.SetActive(true);
                    if (!letterShown)
                    {
                        StopCoroutine(nameof(AnimateLetterIn));
                        StartCoroutine(nameof(AnimateLetterIn));
                    }
                }
                if (letterText != null) letterText.text = s.narration;
                if (narrationText != null) narrationText.text = "";
                letterShown = true;
            }
            else
            {
                if (letterPanel != null) letterPanel.gameObject.SetActive(false);
                if (narrationText != null) narrationText.text = s.narration;
                letterShown = false;
            }

            // Cat
            if (catImage != null)
            {
                catImage.gameObject.SetActive(s.showCat);
                if (s.showCat && catSprite != null) catImage.sprite = catSprite;
            }

            // Start button
            if (startButton != null) startButton.gameObject.SetActive(s.showStartButton);

            // SFX
            if (s.playCatMeow && sfxSource != null && catMeowClip != null)
            {
                sfxSource.PlayOneShot(catMeowClip);
            }
        }

        private IEnumerator AnimateLetterIn()
        {
            if (letterPanel == null) yield break;
            const float duration = 0.6f;
            Vector3 from = Vector3.one * 0.15f;
            Vector3 to = Vector3.one;
            float t = 0f;
            letterPanel.localScale = from;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float p = Mathf.Clamp01(t / duration);
                // Ease-out cubic
                float e = 1f - Mathf.Pow(1f - p, 3f);
                letterPanel.localScale = Vector3.Lerp(from, to, e);
                yield return null;
            }
            letterPanel.localScale = to;
        }

        private void Complete()
        {
            if (finished) return;
            finished = true;
            PlayerPrefs.SetInt(PREF_SEEN, 1);
            PlayerPrefs.Save();
            OnComplete?.Invoke();
        }
    }
}
