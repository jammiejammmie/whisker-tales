#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using WhiskerTales.Polish;
using WhiskerTales.UI;
using WhiskerTales.UI.Screens;
// ButtonClickSfx lives in WhiskerTales.UI (same namespace as above).

namespace WhiskerTales.EditorTools
{
    public static class HomeScreenPrefabBuilder
    {
        public const string PrefabPath = "Assets/WhiskerTales/_Project/Prefabs/UI/Screens/HomeScreen.prefab";
        private const string PrefabDirectory = "Assets/WhiskerTales/_Project/Prefabs/UI/Screens";
        private const string ButtonSpritePath = "Assets/WhiskerTales/Art/UI/btn_large.png";
        private const string LogoSpritePath = "Assets/WhiskerTales/Art/UI/logo_whisker_tales.png";
        private const string LogoAlphaPath = "Assets/WhiskerTales/_Project/Art/Generated/logo_whisker_tales_alpha.png";

        private const string ScreenId = "home";
        private const float CanvasWidth = 1080f;
        private const float CanvasHeight = 2400f;
        private const float ButtonWidth = 720f;
        private const float ButtonHeight = 200f;
        private const float ButtonGap = 40f;
        private const int ButtonFontSize = 64;
        private const int TitleFontSize = 80;

        private static readonly Color BackgroundColor = new Color(0.961f, 0.945f, 0.910f, 1f);
        private static readonly Color ButtonLabelColor = new Color(0.173f, 0.094f, 0.063f, 1f);
        private static readonly Color TitleColor = new Color(0.545f, 0.451f, 0.333f, 1f);

        [InitializeOnLoadMethod]
        private static void AutoBuildOnEditorLoad()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode == true)
            {
                return;
            }
            if (File.Exists(PrefabPath) == true)
            {
                return;
            }
            EditorApplication.delayCall += TryAutoBuild;
        }

        private static void TryAutoBuild()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode == true)
            {
                return;
            }
            if (EditorApplication.isCompiling == true || EditorApplication.isUpdating == true)
            {
                EditorApplication.delayCall += TryAutoBuild;
                return;
            }
            if (File.Exists(PrefabPath) == true)
            {
                return;
            }
            Build();
        }

        [MenuItem("Whisker Tales/V2/Build HomeScreen Prefab")]
        public static void Build()
        {
            if (EditorApplication.isPlaying == true)
            {
                Debug.LogWarning("[HomeScreenPrefabBuilder] Cannot build during Play Mode.");
                return;
            }

            if (Directory.Exists(PrefabDirectory) == false)
            {
                Directory.CreateDirectory(PrefabDirectory);
            }

            GameObject root = BuildRoot();

            try
            {
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("[HomeScreenPrefabBuilder] Saved " + PrefabPath);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static GameObject BuildRoot()
        {
            GameObject root = new GameObject("HomeScreen", typeof(RectTransform), typeof(CanvasGroup));
            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            HomeScreenController controller = root.AddComponent<HomeScreenController>();
            SetScreenId(controller, ScreenId);

            CreateBackground(rootRect);
            CreateLanternGlow(rootRect);
            // 3 petal layers in back-to-front order so far petals render behind mid render behind near.
            CreatePetalLayer(rootRect, "PetalsFar",  size: 60f,  fall: 65f,  interval: 1.2f, maxActive: 12, sway: 50f, alpha: 0.55f);
            CreatePetalLayer(rootRect, "PetalsMid",  size: 100f, fall: 115f, interval: 0.7f, maxActive: 18, sway: 80f, alpha: 0.85f);
            CreatePetalLayer(rootRect, "PetalsNear", size: 150f, fall: 180f, interval: 0.5f, maxActive: 10, sway: 110f, alpha: 1.0f);
            CreateLogo(rootRect);

            // V2-16: Play button gone, tab bar lifted out to scene-level TabBarController.
            // Home is now intentionally empty in the center (asset发주 pending in next phase).
            // Only on-screen interactive element is the gear (top-right).
            Button settingsButton = CreateSettingsGear(rootRect);

            SerializedObject so = new SerializedObject(controller);
            SetReference(so, "settingsButton", settingsButton);
            so.ApplyModifiedPropertiesWithoutUndo();

            return root;
        }

        private static void SetScreenId(UIScreenBase screen, string id)
        {
            SerializedObject so = new SerializedObject(screen);
            SerializedProperty prop = so.FindProperty("screenId");

            if (prop != null)
            {
                prop.stringValue = id;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void CreateLanternGlow(RectTransform parent)
        {
            GameObject go = new GameObject("LanternGlow", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(960f, 720f);
            rect.anchoredPosition = new Vector2(0f, -80f);

            Image image = go.GetComponent<Image>();

            // V2-10: prefer the procedurally generated round glow sprite; fall back to the
            // tutorial bubble if the generator hasn't run yet (first editor open after a checkout).
            Sprite glowSprite = AssetDatabase.LoadAssetAtPath<Sprite>(LanternGlowTextureGenerator.SpritePath);

            if (glowSprite == null)
            {
                glowSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/WhiskerTales/Art/UI/tutorial_bubble.png");
            }

            if (glowSprite != null)
            {
                image.sprite = glowSprite;
                image.type = Image.Type.Simple;
                image.preserveAspect = true;
            }

            image.color = new Color(1f, 0.78f, 0.42f, 0.55f);
            image.raycastTarget = false;

            go.AddComponent<LanternGlowPulse>();
        }

        private static void CreatePetalLayer(RectTransform parent, string name, float size, float fall, float interval, int maxActive, float sway, float alpha)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            AmbientPetalEmitter emitter = go.AddComponent<AmbientPetalEmitter>();

            Sprite[] petals = new Sprite[]
            {
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/WhiskerTales/Art/Effects/petal_01.png"),
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/WhiskerTales/Art/Effects/petal_02.png"),
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/WhiskerTales/Art/Effects/petal_03.png"),
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/WhiskerTales/Art/Effects/petal_04.png"),
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/WhiskerTales/Art/Effects/petal_05.png"),
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/WhiskerTales/Art/Effects/petal_06.png"),
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/WhiskerTales/Art/Effects/petal_07.png"),
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/WhiskerTales/Art/Effects/petal_08.png")
            };

            SerializedObject so = new SerializedObject(emitter);
            SerializedProperty list = so.FindProperty("petalSprites");

            if (list != null)
            {
                list.arraySize = petals.Length;

                for (int i = 0; i < petals.Length; i++)
                {
                    list.GetArrayElementAtIndex(i).objectReferenceValue = petals[i];
                }
            }

            SetFloat(so, "spawnIntervalSeconds", interval);
            SetFloat(so, "fallSpeed", fall);
            SetFloat(so, "swayAmplitude", sway);
            SetFloat(so, "petalImageSize", size);
            SetFloat(so, "petalAlpha", alpha);
            SetInt(so, "maxActive", maxActive);

            SerializedProperty spawnArea = so.FindProperty("spawnArea");

            if (spawnArea != null)
            {
                spawnArea.objectReferenceValue = rect;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetFloat(SerializedObject so, string name, float value)
        {
            SerializedProperty prop = so.FindProperty(name);

            if (prop != null)
            {
                prop.floatValue = value;
            }
        }

        private static void SetInt(SerializedObject so, string name, int value)
        {
            SerializedProperty prop = so.FindProperty(name);

            if (prop != null)
            {
                prop.intValue = value;
            }
        }

        private static (Button home, Button catRoom, Button cafe, Button meditation) CreateTabBar(RectTransform parent)
        {
            // Tab strip anchored to bottom edge, 1080×200 with nav_bar_bg behind 4 tab icons.
            GameObject bar = new GameObject("TabBar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform barRect = bar.GetComponent<RectTransform>();
            barRect.SetParent(parent, false);
            barRect.anchorMin = new Vector2(0f, 0f);
            barRect.anchorMax = new Vector2(1f, 0f);
            barRect.pivot = new Vector2(0.5f, 0f);
            barRect.sizeDelta = new Vector2(0f, 200f);
            barRect.anchoredPosition = Vector2.zero;

            Image barImage = bar.GetComponent<Image>();
            Sprite barSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/WhiskerTales/Art/UI/nav_bar_bg.png");

            if (barSprite != null)
            {
                barImage.sprite = barSprite;
                barImage.type = Image.Type.Sliced;
            }
            else
            {
                barImage.color = new Color(0.945f, 0.918f, 0.847f, 0.96f);
            }

            barImage.raycastTarget = true;

            Button home = CreateTab(barRect, "Tab_Home", "Assets/WhiskerTales/Art/UI/tab_home.png", new Vector2(-360f, 20f));
            Button catRoom = CreateTab(barRect, "Tab_CatRoom", "Assets/WhiskerTales/Art/UI/tab_catroom.png", new Vector2(-120f, 20f));
            Button cafe = CreateTab(barRect, "Tab_Cafe", "Assets/WhiskerTales/Art/UI/tab_cafe.png", new Vector2(120f, 20f));
            Button meditation = CreateTab(barRect, "Tab_Meditation", "Assets/WhiskerTales/Art/UI/tab_meditation.png", new Vector2(360f, 20f));

            return (home, catRoom, cafe, meditation);
        }

        private static Button CreateTab(RectTransform parent, string name, string spritePath, Vector2 anchoredPosition)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = new Vector2(160f, 160f);
            rect.anchoredPosition = anchoredPosition;

            Image image = go.GetComponent<Image>();
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);

            if (sprite != null)
            {
                image.sprite = sprite;
                image.preserveAspect = true;
            }
            else
            {
                image.color = new Color(0.545f, 0.451f, 0.333f, 1f);
            }

            Button button = go.GetComponent<Button>();
            button.targetGraphic = image;
            go.AddComponent<ButtonClickSfx>();
            return button;
        }

        private static Button CreateSettingsGear(RectTransform parent)
        {
            GameObject go = new GameObject("Btn_Settings", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.sizeDelta = new Vector2(140f, 140f);
            rect.anchoredPosition = new Vector2(-60f, -120f);

            Image image = go.GetComponent<Image>();
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/WhiskerTales/Art/UI/btn_settings.png");

            if (sprite != null)
            {
                image.sprite = sprite;
                image.preserveAspect = true;
            }
            else
            {
                image.color = new Color(0.545f, 0.451f, 0.333f, 1f);
            }

            Button button = go.GetComponent<Button>();
            button.targetGraphic = image;
            go.AddComponent<ButtonClickSfx>();
            return button;
        }

        private static void CreateBackground(RectTransform parent)
        {
            GameObject bg = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform rect = bg.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image image = bg.GetComponent<Image>();
            image.color = BackgroundColor;
            image.raycastTarget = false;
        }

        private static void CreateLogo(RectTransform parent)
        {
            GameObject logo = new GameObject("Title", typeof(RectTransform), typeof(CanvasRenderer));
            RectTransform rect = logo.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(900f, 280f);
            rect.anchoredPosition = new Vector2(0f, -180f);

            // Prefer the alpha-processed logo (white bg removed); fall back to raw if generator
            // hasn't run yet.
            Sprite logoSprite = AssetDatabase.LoadAssetAtPath<Sprite>(LogoAlphaPath);

            if (logoSprite == null)
            {
                logoSprite = AssetDatabase.LoadAssetAtPath<Sprite>(LogoSpritePath);
            }

            if (logoSprite != null)
            {
                Image image = logo.AddComponent<Image>();
                image.sprite = logoSprite;
                image.preserveAspect = true;
                image.raycastTarget = false;
            }
            else
            {
                TextMeshProUGUI label = logo.AddComponent<TextMeshProUGUI>();
                label.text = "Whisker Tales";
                label.fontSize = TitleFontSize;
                label.color = TitleColor;
                label.alignment = TextAlignmentOptions.Center;
                label.fontStyle = FontStyles.Bold;
                label.raycastTarget = false;
            }
        }

        private static Button CreateButton(RectTransform parent, string name, string label, Vector2 anchoredPosition)
        {
            GameObject buttonGo = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            RectTransform rect = buttonGo.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(ButtonWidth, ButtonHeight);
            rect.anchoredPosition = anchoredPosition;

            Image image = buttonGo.GetComponent<Image>();
            Sprite buttonSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ButtonSpritePath);

            if (buttonSprite != null)
            {
                image.sprite = buttonSprite;
                image.type = Image.Type.Sliced;
            }
            else
            {
                image.color = new Color(0.910f, 0.659f, 0.486f, 1f);
            }

            image.preserveAspect = false;

            Button button = buttonGo.GetComponent<Button>();
            button.targetGraphic = image;
            buttonGo.AddComponent<ButtonClickSfx>();

            CreateButtonLabel(rect, label);

            return button;
        }

        private static void CreateButtonLabel(RectTransform parent, string text)
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
            label.color = ButtonLabelColor;
            label.alignment = TextAlignmentOptions.Center;
            label.fontStyle = FontStyles.Bold;
            label.raycastTarget = false;
        }

        private static void SetReference(SerializedObject so, string propertyName, Object value)
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
