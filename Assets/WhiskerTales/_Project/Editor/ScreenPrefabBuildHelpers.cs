#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using WhiskerTales.UI;
using WhiskerTales.UI.Screens;

namespace WhiskerTales.EditorTools
{
    internal static class ScreenPrefabBuildHelpers
    {
        public const string ButtonSpritePath = "Assets/WhiskerTales/Art/UI/btn_large.png";
        public const string BackButtonSpritePath = "Assets/WhiskerTales/Art/UI/btn_back.png";

        public static readonly Color BackgroundCream = new Color(0.961f, 0.945f, 0.910f, 1f);
        public static readonly Color WarmBrown = new Color(0.545f, 0.451f, 0.333f, 1f);
        public static readonly Color DeepText = new Color(0.173f, 0.094f, 0.063f, 1f);
        public static readonly Color SoftPink = new Color(0.957f, 0.627f, 0.710f, 1f);

        public const int TitleFontSize = 80;
        public const int BodyFontSize = 48;
        public const int ButtonFontSize = 60;

        public static GameObject CreateRoot(string name, System.Type controllerType, string screenId)
        {
            GameObject root = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup));
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            UIScreenBase controller = (UIScreenBase)root.AddComponent(controllerType);
            SetScreenId(controller, screenId);

            CreateBackground(rect);

            return root;
        }

        public static void SetScreenId(UIScreenBase screen, string id)
        {
            SerializedObject so = new SerializedObject(screen);
            SerializedProperty prop = so.FindProperty("screenId");

            if (prop != null)
            {
                prop.stringValue = id;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        public static void CreateBackground(RectTransform parent)
        {
            GameObject bg = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform rect = bg.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image image = bg.GetComponent<Image>();
            image.color = BackgroundCream;
            image.raycastTarget = true;
        }

        public static TextMeshProUGUI CreateTitle(RectTransform parent, string text)
        {
            GameObject titleGo = new GameObject("Title", typeof(RectTransform), typeof(CanvasRenderer));
            RectTransform rect = titleGo.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(900f, 140f);
            rect.anchoredPosition = new Vector2(0f, -180f);

            TextMeshProUGUI label = titleGo.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = TitleFontSize;
            label.color = WarmBrown;
            label.alignment = TextAlignmentOptions.Center;
            label.fontStyle = FontStyles.Bold;
            label.raycastTarget = false;
            return label;
        }

        public static Button CreateBackButton(RectTransform parent)
        {
            GameObject buttonGo = new GameObject("Btn_Back", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            RectTransform rect = buttonGo.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2(140f, 140f);
            rect.anchoredPosition = new Vector2(60f, -120f);

            Image image = buttonGo.GetComponent<Image>();
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(BackButtonSpritePath);

            if (sprite != null)
            {
                image.sprite = sprite;
                image.preserveAspect = true;
            }
            else
            {
                image.color = WarmBrown;
            }

            Button button = buttonGo.GetComponent<Button>();
            button.targetGraphic = image;
            buttonGo.AddComponent<ButtonClickSfx>();
            return button;
        }

        public static Button CreatePrimaryButton(RectTransform parent, string name, string label, Vector2 anchoredPosition, Vector2 size)
        {
            GameObject buttonGo = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            RectTransform rect = buttonGo.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;

            Image image = buttonGo.GetComponent<Image>();
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ButtonSpritePath);

            if (sprite != null)
            {
                image.sprite = sprite;
                image.type = Image.Type.Sliced;
            }
            else
            {
                image.color = SoftPink;
            }

            Button button = buttonGo.GetComponent<Button>();
            button.targetGraphic = image;
            buttonGo.AddComponent<ButtonClickSfx>();

            CreateButtonLabel(rect, label);
            return button;
        }

        public static TextMeshProUGUI CreateButtonLabel(RectTransform parent, string text)
        {
            GameObject labelGo = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer));
            RectTransform rect = labelGo.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            TextMeshProUGUI label = labelGo.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = ButtonFontSize;
            label.color = DeepText;
            label.alignment = TextAlignmentOptions.Center;
            label.fontStyle = FontStyles.Bold;
            label.raycastTarget = false;
            return label;
        }

        public static TextMeshProUGUI CreateBodyLabel(RectTransform parent, string text, Vector2 anchoredPosition, Vector2 size, int fontSize)
        {
            GameObject labelGo = new GameObject("Body", typeof(RectTransform), typeof(CanvasRenderer));
            RectTransform rect = labelGo.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;

            TextMeshProUGUI label = labelGo.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = fontSize;
            label.color = DeepText;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;
            return label;
        }

        public static RectTransform CreateBoardArea(RectTransform parent)
        {
            GameObject area = new GameObject("BoardArea", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(GridLayoutGroup));
            RectTransform rect = area.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            // V2-16: tiles bumped — board 960→1000 (more horizontal fill on 1080-wide canvas),
            // cell 100→112 (12% larger), spacing 20→12 (less air between but still not touching).
            rect.sizeDelta = new Vector2(1000f, 1000f);
            rect.anchoredPosition = new Vector2(0f, 0f);

            Image bg = area.GetComponent<Image>();
            bg.color = new Color(0.945f, 0.918f, 0.847f, 0.92f);
            bg.raycastTarget = false;

            GridLayoutGroup grid = area.GetComponent<GridLayoutGroup>();
            // V2-16: 8*112 + 7*12 + 2*10 = 1000 (= boardArea width). Tiles still don't touch.
            grid.cellSize = new Vector2(112f, 112f);
            grid.spacing = new Vector2(12f, 12f);
            grid.padding = new RectOffset(10, 10, 10, 10);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 8;
            grid.childAlignment = TextAnchor.MiddleCenter;
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;

            return rect;
        }

        public static GameObject SavePrefab(GameObject root, string prefabPath)
        {
            GameObject saved = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);
            return saved;
        }

        public static void SetReference(SerializedObject so, string propertyName, Object value)
        {
            SerializedProperty prop = so.FindProperty(propertyName);

            if (prop == null)
            {
                return;
            }

            prop.objectReferenceValue = value;
        }
    }
}
#endif
