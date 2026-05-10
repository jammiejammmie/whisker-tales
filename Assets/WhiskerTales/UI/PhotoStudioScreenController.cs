using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WhiskerTales.Core;

namespace WhiskerTales.UI
{
    public sealed class PhotoStudioScreenController : UIScreenBase
    {
        [SerializeField] private RawImage previewTarget;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image catPortrait;
        [SerializeField] private Image flashOverlay;

        [SerializeField] private Button backButton;
        [SerializeField] private Button frontPoseButton;
        [SerializeField] private Button playPoseButton;
        [SerializeField] private Button captureButton;
        [SerializeField] private Button shareButton;

        [SerializeField] private TMP_Text titleText;

        private Texture2D lastCapture;

        protected override void Awake()
        {
            base.Awake();

            if (backButton != null)
            {
                backButton.onClick.AddListener(Hide);
            }

            if (frontPoseButton != null)
            {
                frontPoseButton.onClick.AddListener(() => DebugLogger.Info(LogCategory.UI, "Photo pose selected: Front"));
            }

            if (playPoseButton != null)
            {
                playPoseButton.onClick.AddListener(() => DebugLogger.Info(LogCategory.UI, "Photo pose selected: Play"));
            }

            if (captureButton != null)
            {
                captureButton.onClick.AddListener(() => StartCoroutine(CaptureRoutine()));
            }

            if (shareButton != null)
            {
                shareButton.onClick.AddListener(ShareLastCapture);
            }
        }

        private IEnumerator CaptureRoutine()
        {
            if (flashOverlay != null)
            {
                flashOverlay.color = new Color(1f, 1f, 1f, 0f);
                flashOverlay.gameObject.SetActive(true);
                flashOverlay.DOFade(0.8f, 0.05f);
                flashOverlay.DOFade(0f, 0.18f).SetDelay(0.05f);
            }

            yield return new WaitForEndOfFrame();

            Texture2D tex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0f, 0f, Screen.width, Screen.height), 0, 0);
            tex.Apply();
            lastCapture = tex;

            if (previewTarget != null)
            {
                previewTarget.texture = lastCapture;
            }

            DebugLogger.Info(LogCategory.UI, "Photo captured.");
        }

        private void ShareLastCapture()
        {
            if (lastCapture == null)
            {
                DebugLogger.Warning(LogCategory.UI, "Share pressed but no capture exists.");
                return;
            }

            byte[] pngBytes = lastCapture.EncodeToPNG();

            if (pngBytes == null || pngBytes.Length == 0)
            {
                DebugLogger.Warning(LogCategory.UI, "Photo capture PNG encode failed.");
                return;
            }

            DebugLogger.Info(LogCategory.UI, $"Photo ready to share. Bytes={pngBytes.Length}");
        }

        private void OnDestroy()
        {
            if (lastCapture != null)
            {
                Destroy(lastCapture);
                lastCapture = null;
            }
        }
    }
}
