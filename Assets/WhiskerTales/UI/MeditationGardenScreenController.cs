using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WhiskerTales.Core;

namespace WhiskerTales.UI
{
    public sealed class MeditationGardenScreenController : UIScreenBase, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text peacePointText;
        [SerializeField] private RectTransform sandCanvasFrame;
        [SerializeField] private RectTransform drawingLayer;
        [SerializeField] private Image brushDotPrefab;
        [SerializeField] private Button resetButton;
        [SerializeField] private Button exitButton;

        private bool isDrawing;
        private int peacePoints;
        private float lastSpawnTime;

        protected override void Awake()
        {
            base.Awake();

            if (resetButton != null)
            {
                resetButton.onClick.AddListener(ClearDrawing);
            }

            if (exitButton != null)
            {
                exitButton.onClick.AddListener(Hide);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData == null)
            {
                return;
            }

            isDrawing = true;
            TrySpawnBrush(eventData.position);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (isDrawing == false || eventData == null)
            {
                return;
            }

            if (Time.unscaledTime - lastSpawnTime < 0.03f)
            {
                return;
            }

            TrySpawnBrush(eventData.position);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isDrawing = false;
        }

        public void SetPeacePoints(int value)
        {
            peacePoints = Mathf.Max(0, value);

            if (peacePointText != null)
            {
                peacePointText.text = peacePoints.ToString();
            }
        }

        private void TrySpawnBrush(Vector2 screenPosition)
        {
            if (drawingLayer == null || brushDotPrefab == null)
            {
                return;
            }

            Vector2 localPosition;
            bool inside = RectTransformUtility.ScreenPointToLocalPointInRectangle(
                drawingLayer,
                screenPosition,
                null,
                out localPosition
            );

            if (inside == false)
            {
                return;
            }

            if (drawingLayer.rect.Contains(localPosition) == false)
            {
                return;
            }

            Image dot = Instantiate(brushDotPrefab, drawingLayer);
            RectTransform rt = dot.rectTransform;
            rt.anchoredPosition = localPosition;
            rt.sizeDelta = new Vector2(24f, 24f);
            dot.color = new Color(0.55f, 0.44f, 0.32f, 0.32f);
            dot.transform.localScale = Vector3.zero;
            dot.transform.DOScale(1f, 0.12f).SetEase(Ease.OutCubic);

            lastSpawnTime = Time.unscaledTime;
        }

        private void ClearDrawing()
        {
            if (drawingLayer == null)
            {
                return;
            }

            for (int i = drawingLayer.childCount - 1; i >= 0; i--)
            {
                Transform child = drawingLayer.GetChild(i);

                if (child != null)
                {
                    Destroy(child.gameObject);
                }
            }

            DebugLogger.Info(LogCategory.UI, "Meditation garden drawing cleared.");
        }
    }
}
