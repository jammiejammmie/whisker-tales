using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WhiskerTales.Audio;
using WhiskerTales.Core;
using WhiskerTales.Currency;

namespace WhiskerTales.UI
{
    /// <summary>
    /// Phase B §3-2. 명상 정원 — 모래 정원에 손가락 드래그로 선을 그리는 디톡스 콘텐츠.
    /// 1손가락: 선 그리기 (Texture2D 픽셀 페인팅).
    /// 2손가락: 정원 초기화 (모래 텍스처 복원).
    /// 30초마다 평화 포인트 +1, 평화 포인트 100 도달 시 냥이 마음 💝 +5 (CurrencyManager).
    /// 위치/사운드는 §3-2 명세대로 (배경: bg_zone3_stage5, 풍경 소리 BGM placeholder).
    /// </summary>
    public class MeditationGardenController : MonoBehaviour
    {
        public const string PREF_PEACE_POINTS = "Meditation.PeacePoints";

        [Header("Visuals")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private RawImage sandImage;
        [SerializeField] private TMP_Text peacePointText;
        [SerializeField] private TMP_Text titleText;

        [Header("Buttons")]
        [SerializeField] private Button backButton;
        [SerializeField] private Button resetButton;
        [SerializeField] private Button endButton;

        [Header("Audio")]
        [SerializeField] private AudioSource bgmSource;       // 풍경 소리 BGM (placeholder, clip 없으면 무음)
        [SerializeField, Range(0f, 1f)] private float bgmVolume = 0.3f;

        [Header("Tuning")]
        [SerializeField] private int textureSize = 512;
        [SerializeField] private Color sandBaseColor = new Color(0.85f, 0.78f, 0.62f);
        [SerializeField] private Color drawColor    = new Color(0.42f, 0.35f, 0.25f); // §3-2: #6B5840 비슷
        [SerializeField] private int brushRadius = 5;
        [SerializeField] private float minDragDeltaNorm = 0.005f;
        [SerializeField] private float idleSecondsForPoint = 30f;
        [SerializeField] private int pointsForReward = 100;
        [SerializeField] private int rewardNyangiHeart = 5;

        private Texture2D sandTexture;
        private int peacePoints;
        private float idleTimer;
        private bool isDrawing;
        private Vector2 lastDrawNorm;

        public int PeacePoints => peacePoints;
        public int PointsForReward => pointsForReward;

        private void Awake()
        {
            peacePoints = PlayerPrefs.GetInt(PREF_PEACE_POINTS, 0);
            InitSandTexture();
        }

        private void OnEnable()
        {
            if (backButton != null) backButton.onClick.AddListener(HandleBack);
            if (resetButton != null) resetButton.onClick.AddListener(HandleResetButton);
            if (endButton != null) endButton.onClick.AddListener(HandleEnd);

            idleTimer = 0f;
            isDrawing = false;
            UpdatePeaceUI();

            if (bgmSource != null && SoundManager.Instance != null && SoundManager.Instance.CurrentMode != SoundMode.Mute)
            {
                bgmSource.loop = true;
                bgmSource.volume = bgmVolume;
                if (!bgmSource.isPlaying && bgmSource.clip != null) bgmSource.Play();
            }
        }

        private void OnDisable()
        {
            if (backButton != null) backButton.onClick.RemoveListener(HandleBack);
            if (resetButton != null) resetButton.onClick.RemoveListener(HandleResetButton);
            if (endButton != null) endButton.onClick.RemoveListener(HandleEnd);

            if (bgmSource != null && bgmSource.isPlaying) bgmSource.Stop();

            SavePeacePoints();
        }

        private void Update()
        {
            // 2손가락 → 정원 초기화 (탭 시 한 번만)
            if (Input.touchCount >= 2)
            {
                ResetGarden();
                isDrawing = false;
                return;
            }

            // 1손가락 또는 마우스 → 선 그리기
            bool inputActive = false;
            Vector2 screenPos = Vector2.zero;

            if (Input.touchCount == 1)
            {
                Touch t = Input.GetTouch(0);
                screenPos = t.position;
                inputActive = (t.phase == TouchPhase.Began || t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary);
            }
            else if (Input.GetMouseButton(0))
            {
                screenPos = Input.mousePosition;
                inputActive = true;
            }

            if (inputActive) HandleDrawAt(screenPos);
            else isDrawing = false;

            // 30초마다 평화 포인트 +1
            idleTimer += Time.deltaTime;
            if (idleTimer >= idleSecondsForPoint)
            {
                idleTimer -= idleSecondsForPoint;
                AddPeacePoint();
            }
        }

        private void HandleDrawAt(Vector2 screenPos)
        {
            if (sandImage == null) return;
            RectTransform rt = (RectTransform)sandImage.transform;
            if (!ScreenToNormalized(rt, screenPos, out Vector2 normalized))
            {
                isDrawing = false;
                return;
            }

            if (!isDrawing)
            {
                isDrawing = true;
                lastDrawNorm = normalized;
                DrawDot(normalized);
                return;
            }

            if (Vector2.Distance(normalized, lastDrawNorm) >= minDragDeltaNorm)
            {
                DrawLineSegment(lastDrawNorm, normalized);
                lastDrawNorm = normalized;
            }
        }

        private bool ScreenToNormalized(RectTransform rt, Vector2 screenPos, out Vector2 normalized)
        {
            Camera cam = null;
            Canvas canvas = rt.GetComponentInParent<Canvas>();
            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay) cam = canvas.worldCamera;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, screenPos, cam, out Vector2 local))
            {
                normalized = Vector2.zero;
                return false;
            }
            Rect r = rt.rect;
            if (local.x < r.xMin || local.x > r.xMax || local.y < r.yMin || local.y > r.yMax)
            {
                normalized = Vector2.zero;
                return false;
            }
            normalized = new Vector2((local.x - r.xMin) / r.width, (local.y - r.yMin) / r.height);
            return true;
        }

        private void DrawLineSegment(Vector2 a, Vector2 b)
        {
            float distance = Vector2.Distance(a, b);
            int steps = Mathf.Max(1, Mathf.RoundToInt(distance * textureSize));
            for (int i = 0; i <= steps; i++)
            {
                float t = (float)i / steps;
                DrawDotInternal(Vector2.Lerp(a, b, t), apply: false);
            }
            sandTexture.Apply();
        }

        private void DrawDot(Vector2 normalized)
        {
            DrawDotInternal(normalized, apply: true);
        }

        private void DrawDotInternal(Vector2 normalized, bool apply)
        {
            if (sandTexture == null) return;
            int cx = Mathf.RoundToInt(normalized.x * (textureSize - 1));
            int cy = Mathf.RoundToInt(normalized.y * (textureSize - 1));
            int rSq = brushRadius * brushRadius;
            for (int dy = -brushRadius; dy <= brushRadius; dy++)
            {
                int py = cy + dy;
                if (py < 0 || py >= textureSize) continue;
                for (int dx = -brushRadius; dx <= brushRadius; dx++)
                {
                    int px = cx + dx;
                    if (px < 0 || px >= textureSize) continue;
                    if (dx * dx + dy * dy > rSq) continue;
                    sandTexture.SetPixel(px, py, drawColor);
                }
            }
            if (apply) sandTexture.Apply();
        }

        private void InitSandTexture()
        {
            sandTexture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
            };
            if (sandImage != null) sandImage.texture = sandTexture;
            ResetSandPixels();
        }

        private void ResetSandPixels()
        {
            if (sandTexture == null) return;
            Color[] pixels = new Color[textureSize * textureSize];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = sandBaseColor;
            sandTexture.SetPixels(pixels);
            sandTexture.Apply();
        }

        public void ResetGarden()
        {
            ResetSandPixels();
            AudioManager.instance?.PlayButtonClick();
        }

        public void AddPeacePoint()
        {
            peacePoints++;
            // 100점 도달 시 보상 + 차감
            while (peacePoints >= pointsForReward)
            {
                CurrencyManager.Instance?.TryAwardNyangiHeart(rewardNyangiHeart, "meditation_garden_100pp");
                peacePoints -= pointsForReward;
                Debug.Log($"[MeditationGarden] 100 평화 포인트 → 💝 +{rewardNyangiHeart}");
            }
            UpdatePeaceUI();
            SavePeacePoints();
        }

        private void UpdatePeaceUI()
        {
            if (peacePointText != null) peacePointText.text = $"⭐ {peacePoints}";
        }

        private void SavePeacePoints()
        {
            PlayerPrefs.SetInt(PREF_PEACE_POINTS, peacePoints);
            PlayerPrefs.Save();
        }

        private void HandleBack()
        {
            AudioManager.instance?.PlayButtonClick();
            GameManager.Instance?.ReturnToMenu();
        }

        private void HandleResetButton()
        {
            ResetGarden();
        }

        private void HandleEnd()
        {
            AudioManager.instance?.PlayButtonClick();
            GameManager.Instance?.ReturnToMenu();
        }

#if UNITY_EDITOR
        // ===== Editor-only debug API for tests =====

        public void DebugAddPeacePoint() => AddPeacePoint();

        public void DebugResetPeacePoints()
        {
            peacePoints = 0;
            SavePeacePoints();
            UpdatePeaceUI();
        }

        public void DebugInitTexture()
        {
            if (sandTexture == null) InitSandTexture();
        }

        public void DebugDrawAt(Vector2 normalized) => DrawDot(normalized);

        public Color DebugSampleSandPixel(int px, int py)
        {
            if (sandTexture == null) return default;
            if (px < 0 || px >= textureSize || py < 0 || py >= textureSize) return default;
            return sandTexture.GetPixel(px, py);
        }

        public int DebugTextureSize => textureSize;
        public Color DebugSandBaseColor => sandBaseColor;
        public Color DebugDrawColor => drawColor;
#endif
    }
}
