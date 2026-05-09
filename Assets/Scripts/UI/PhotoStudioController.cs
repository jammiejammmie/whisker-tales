using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WhiskerTales.Cat;
using WhiskerTales.Core;
using WhiskerTales.Utilities;

namespace WhiskerTales.UI
{
    /// <summary>
    /// Phase B §3-4. 고양이 포토 스튜디오. 배경/포즈 선택 후 ScreenCapture로 스크린샷,
    /// 선택적으로 NativeShare 연결. NativeShare 미설치 시 path만 로그 (후속 연결).
    /// AppBootstrap이 5종 배경/2종 포즈/액션 버튼을 빌드하고 SerializeField 주입.
    /// </summary>
    public class PhotoStudioController : MonoBehaviour
    {
        public enum PoseKind { Front, Play }

        [Serializable]
        public class BackgroundOption
        {
            public string label;
            public Sprite sprite;
            public Button thumbnailButton;
        }

        [Header("Top Bar")]
        [SerializeField] private Button backButton;
        [SerializeField] private TMP_Text titleText;

        [Header("Preview")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image catImage;

        [Header("Selectors")]
        [SerializeField] private BackgroundOption[] backgroundOptions; // 5개
        [SerializeField] private Button poseFrontButton;
        [SerializeField] private Button posePlayButton;

        [Header("Actions")]
        [SerializeField] private Button captureButton;
        [SerializeField] private Button shareButton;
        [SerializeField] private RectTransform actionRow; // 캡처 시 잠시 숨김

        [Header("Cat Sprite Set (injected from spriteLib)")]
        [SerializeField] private Sprite[] frontSpritesByCatId; // index 1..5
        [SerializeField] private Sprite[] playSpritesByCatId;  // index 1..5

        public const string SHARE_URL = "https://whiskertales-mwjyt48n.manus.space";

        private int currentCatId = Constants.CAT_NABI;
        private int currentBackgroundIdx = 0;
        private PoseKind currentPose = PoseKind.Front;
        private string lastCapturedPath;

        public int CurrentCatId => currentCatId;
        public int CurrentBackgroundIdx => currentBackgroundIdx;
        public PoseKind CurrentPose => currentPose;
        public string LastCapturedPath => lastCapturedPath;

        private void OnEnable()
        {
            if (backButton != null) backButton.onClick.AddListener(HandleBack);
            if (poseFrontButton != null) poseFrontButton.onClick.AddListener(() => SetPose(PoseKind.Front));
            if (posePlayButton != null) posePlayButton.onClick.AddListener(() => SetPose(PoseKind.Play));
            if (captureButton != null) captureButton.onClick.AddListener(HandleCapture);
            if (shareButton != null) shareButton.onClick.AddListener(HandleShare);

            if (backgroundOptions != null)
            {
                for (int i = 0; i < backgroundOptions.Length; i++)
                {
                    if (backgroundOptions[i] == null || backgroundOptions[i].thumbnailButton == null) continue;
                    int captured = i;
                    backgroundOptions[i].thumbnailButton.onClick.AddListener(() => SetBackground(captured));
                }
            }

            ApplyAll();
        }

        private void OnDisable()
        {
            if (backButton != null) backButton.onClick.RemoveListener(HandleBack);
            if (poseFrontButton != null) poseFrontButton.onClick.RemoveAllListeners();
            if (posePlayButton != null) posePlayButton.onClick.RemoveAllListeners();
            if (captureButton != null) captureButton.onClick.RemoveListener(HandleCapture);
            if (shareButton != null) shareButton.onClick.RemoveListener(HandleShare);
            if (backgroundOptions != null)
                foreach (var o in backgroundOptions)
                    if (o != null && o.thumbnailButton != null) o.thumbnailButton.onClick.RemoveAllListeners();
        }

        // ===== Public API =====

        public void SetCat(int catId)
        {
            currentCatId = catId;
            ApplyCatSprite();
        }

        public void SetBackground(int idx)
        {
            if (backgroundOptions == null || idx < 0 || idx >= backgroundOptions.Length) return;
            currentBackgroundIdx = idx;
            ApplyBackground();
            AudioManager.instance?.PlayButtonClick();
        }

        public void SetPose(PoseKind pose)
        {
            currentPose = pose;
            ApplyCatSprite();
            AudioManager.instance?.PlayButtonClick();
        }

        public void TogglePose()
        {
            SetPose(currentPose == PoseKind.Front ? PoseKind.Play : PoseKind.Front);
        }

        // ===== Apply state to UI =====

        private void ApplyAll()
        {
            ApplyBackground();
            ApplyCatSprite();
        }

        private void ApplyBackground()
        {
            if (backgroundImage == null || backgroundOptions == null) return;
            if (currentBackgroundIdx < 0 || currentBackgroundIdx >= backgroundOptions.Length) return;
            Sprite sp = backgroundOptions[currentBackgroundIdx]?.sprite;
            if (sp != null) { backgroundImage.sprite = sp; backgroundImage.color = Color.white; }
        }

        private void ApplyCatSprite()
        {
            if (catImage == null) return;
            Sprite sp = ResolveCatSprite();
            if (sp != null) catImage.sprite = sp;
        }

        private Sprite ResolveCatSprite()
        {
            Sprite[] arr = (currentPose == PoseKind.Play) ? playSpritesByCatId : frontSpritesByCatId;
            if (arr == null) return null;
            if (currentCatId >= 0 && currentCatId < arr.Length) return arr[currentCatId];
            return null;
        }

        // ===== Capture / Share =====

        private void HandleCapture()
        {
            AudioManager.instance?.PlayButtonClick();
            StartCoroutine(CaptureRoutine());
        }

        private IEnumerator CaptureRoutine()
        {
            // 캡처 중 액션 행 잠시 숨김 (사진에 안 들어가게)
            bool actionWasVisible = actionRow != null && actionRow.gameObject.activeSelf;
            if (actionRow != null) actionRow.gameObject.SetActive(false);

            yield return new WaitForEndOfFrame();

            Texture2D shot = ScreenCapture.CaptureScreenshotAsTexture();
            byte[] png = shot.EncodeToPNG();
            DestroyImmediate(shot);

            string filename = $"whisker_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            string fullPath = Path.Combine(Application.persistentDataPath, filename);
            File.WriteAllBytes(fullPath, png);
            lastCapturedPath = fullPath;
            Debug.Log($"[PhotoStudio] Captured: {fullPath}");

            if (actionRow != null && actionWasVisible) actionRow.gameObject.SetActive(true);
        }

        private void HandleShare()
        {
            AudioManager.instance?.PlayButtonClick();
            // Phase C-2: 공유 카드 화면으로 이동 (레퍼럴 코드 포함). 캡처된 PNG는 lastCapturedPath에 보존됨.
            GameManager.Instance?.RequestNavigation(NavigationTarget.ShareCard);
        }

        private string BuildShareText()
        {
            string catName = ResolveCatName();
            return $"우리 {catName} 너무 귀여워요 🐾\n#냥이의집 #WhiskerTales #NyangStudio\n{SHARE_URL}";
        }

        private string ResolveCatName()
        {
            Core.Cat cat = CatManager.Instance?.GetCat(currentCatId);
            if (cat != null && !string.IsNullOrEmpty(cat.name)) return cat.name;
            return "고양이";
        }

        private void HandleBack()
        {
            AudioManager.instance?.PlayButtonClick();
            GameManager.Instance?.RequestNavigation(NavigationTarget.CatRoom);
        }

#if UNITY_EDITOR
        // ===== Editor-only debug API for tests =====

        public Sprite DebugGetCurrentBackgroundSprite()
            => backgroundImage != null ? backgroundImage.sprite : null;

        public Sprite DebugGetCurrentCatSprite()
            => catImage != null ? catImage.sprite : null;

        public int DebugBackgroundOptionsCount()
            => backgroundOptions == null ? 0 : backgroundOptions.Length;

        public void DebugInjectFrontSprite(int catId, Sprite sp)
        {
            if (frontSpritesByCatId == null) frontSpritesByCatId = new Sprite[6];
            if (catId >= 0 && catId < frontSpritesByCatId.Length) frontSpritesByCatId[catId] = sp;
        }

        public void DebugInjectPlaySprite(int catId, Sprite sp)
        {
            if (playSpritesByCatId == null) playSpritesByCatId = new Sprite[6];
            if (catId >= 0 && catId < playSpritesByCatId.Length) playSpritesByCatId[catId] = sp;
        }

        public void DebugInjectBackgroundOption(int idx, Sprite sp)
        {
            if (backgroundOptions == null || idx < 0 || idx >= backgroundOptions.Length) return;
            if (backgroundOptions[idx] == null) backgroundOptions[idx] = new BackgroundOption();
            backgroundOptions[idx].sprite = sp;
        }

        public void DebugAllocBackgroundOptions(int count)
        {
            backgroundOptions = new BackgroundOption[count];
            for (int i = 0; i < count; i++) backgroundOptions[i] = new BackgroundOption();
        }

        public void DebugAttachImages(Image bg, Image cat)
        {
            backgroundImage = bg;
            catImage = cat;
        }
#endif
    }
}
