using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WhiskerTales.Core;

namespace WhiskerTales.UI
{
    public sealed class UIFactory
    {
        private readonly UIAssetRegistry assetRegistry;
        private readonly TMP_FontAsset tmpFont;

        public UIFactory(UIAssetRegistry assetRegistry, TMP_FontAsset tmpFont = null)
        {
            this.assetRegistry = assetRegistry;
            this.tmpFont = tmpFont;
        }

        public Image CreateImage(string name, Transform parent, string spriteKey, Vector2 anchoredPosition, Vector2 size, Vector2 anchor, Vector2 pivot)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            ApplyRect(rt, anchoredPosition, size, anchor, pivot);

            Image image = go.GetComponent<Image>();

            if (assetRegistry != null)
            {
                image.sprite = assetRegistry.GetSprite(spriteKey);
            }

            image.preserveAspect = false;
            return image;
        }

        public TextMeshProUGUI CreateText(string name, Transform parent, string text, int fontSize, Vector2 anchoredPosition, Vector2 size, Vector2 anchor, TextAlignmentOptions alignment)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            ApplyRect(rt, anchoredPosition, size, anchor, new Vector2(0.5f, 0.5f));

            TextMeshProUGUI label = go.GetComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = fontSize;
            label.color = UILayoutConstants.Text;
            label.alignment = alignment;
            label.enableWordWrapping = true;

            if (tmpFont != null)
            {
                label.font = tmpFont;
            }

            return label;
        }

        public Button CreateButton(string name, Transform parent, string spriteKey, Vector2 anchoredPosition, Vector2 size, Vector2 anchor)
        {
            Image image = CreateImage(name, parent, spriteKey, anchoredPosition, size, anchor, new Vector2(0.5f, 0.5f));
            Button button = image.gameObject.AddComponent<Button>();
            ButtonFeedback feedback = image.gameObject.AddComponent<ButtonFeedback>();
            feedback.Bind(button);
            return button;
        }

        public static void ApplyRect(RectTransform rt, Vector2 anchoredPosition, Vector2 size, Vector2 anchor, Vector2 pivot)
        {
            if (rt == null)
            {
                DebugLogger.Warning(LogCategory.UI, "ApplyRect target RectTransform is null.");
                return;
            }

            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = size;
        }
    }
}
