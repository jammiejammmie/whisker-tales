#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using WhiskerTales.UI;
using WhiskerTales.UI.Screens;

namespace WhiskerTales.EditorTools
{
    // Builders for the V2-6 screens (Gameplay/CatRoom/Cafe/LevelClear/GameFail). Each saves a prefab
    // under _Project/Prefabs/UI/Screens/ and is invoked by BuildScript before Main_App scene build.
    public static class V2ScreenPrefabBuilders
    {
        public const string PrefabDirectory = "Assets/WhiskerTales/_Project/Prefabs/UI/Screens";

        public const string GameplayPrefabPath = PrefabDirectory + "/GameplayScreen.prefab";
        public const string CatRoomPrefabPath = PrefabDirectory + "/CatRoomScreen.prefab";
        public const string CafePrefabPath = PrefabDirectory + "/CafeScreen.prefab";
        public const string LevelClearPrefabPath = PrefabDirectory + "/LevelClearScreen.prefab";
        public const string GameFailPrefabPath = PrefabDirectory + "/GameFailScreen.prefab";
        public const string LevelSelectPrefabPath = PrefabDirectory + "/LevelSelectScreen.prefab";
        public const string SettingsPrefabPath = PrefabDirectory + "/SettingsScreen.prefab";
        public const string SleepModePrefabPath = PrefabDirectory + "/SleepModeScreen.prefab";

        private const string CatSpritePath = "Assets/WhiskerTales/Art/Cats/cat_nabi.png";
        private const string CatAlphaSpritePath = "Assets/WhiskerTales/_Project/Art/Generated/cat_nabi_alpha.png";
        private const string CafeBgSpritePath = "Assets/WhiskerTales/Art/Backgrounds/bg_zone2_stage2.png";

        [MenuItem("Whisker Tales/V2/Build All V2-6 Screen Prefabs")]
        public static void BuildAll()
        {
            EnsureDirectory();
            BuildGameplay();
            BuildCatRoom();
            BuildCafe();
            BuildLevelClear();
            BuildGameFail();
            BuildLevelSelect();
            BuildSettings();
            BuildSleepMode();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("Whisker Tales/V2/Build SleepModeScreen Prefab")]
        public static void BuildSleepMode()
        {
            EnsureDirectory();
            GameObject root = ScreenPrefabBuildHelpers.CreateRoot("SleepModeScreen", typeof(WhiskerTales.UI.Screens.SleepModeScreenController), "sleepmode");
            RectTransform rect = (RectTransform)root.transform;

            ScreenPrefabBuildHelpers.CreateTitle(rect, "수면 모드");
            Button backButton = ScreenPrefabBuildHelpers.CreateBackButton(rect);
            ScreenPrefabBuildHelpers.CreateBodyLabel(rect, "고양이도 곧 잠들겠어요.", new Vector2(0f, 0f), new Vector2(900f, 120f), ScreenPrefabBuildHelpers.BodyFontSize);

            WhiskerTales.UI.Screens.SleepModeScreenController controller = root.GetComponent<WhiskerTales.UI.Screens.SleepModeScreenController>();
            SerializedObject so = new SerializedObject(controller);
            ScreenPrefabBuildHelpers.SetReference(so, "backButton", backButton);
            so.ApplyModifiedPropertiesWithoutUndo();

            ScreenPrefabBuildHelpers.SavePrefab(root, SleepModePrefabPath);
            Debug.Log("[V2ScreenPrefabBuilders] Saved " + SleepModePrefabPath);
        }

        [MenuItem("Whisker Tales/V2/Build SettingsScreen Prefab")]
        public static void BuildSettings()
        {
            EnsureDirectory();
            GameObject root = ScreenPrefabBuildHelpers.CreateRoot("SettingsScreen", typeof(WhiskerTales.UI.Screens.SettingsScreenController), "settings");
            RectTransform rect = (RectTransform)root.transform;

            ScreenPrefabBuildHelpers.CreateTitle(rect, "설정");
            Button backButton = ScreenPrefabBuildHelpers.CreateBackButton(rect);

            // BGM row at y=+200, SFX at y=0, Haptics at y=-220.
            (Slider bgmSlider, TextMeshProUGUI bgmValue) = BuildSliderRow(rect, "BGM", new Vector2(0f, 200f));
            (Slider sfxSlider, TextMeshProUGUI sfxValue) = BuildSliderRow(rect, "효과음", new Vector2(0f, 0f));
            Toggle hapticsToggle = BuildToggleRow(rect, "진동", new Vector2(0f, -220f));

            WhiskerTales.UI.Screens.SettingsScreenController controller = root.GetComponent<WhiskerTales.UI.Screens.SettingsScreenController>();
            SerializedObject so = new SerializedObject(controller);
            ScreenPrefabBuildHelpers.SetReference(so, "backButton", backButton);
            ScreenPrefabBuildHelpers.SetReference(so, "bgmSlider", bgmSlider);
            ScreenPrefabBuildHelpers.SetReference(so, "sfxSlider", sfxSlider);
            ScreenPrefabBuildHelpers.SetReference(so, "hapticsToggle", hapticsToggle);
            ScreenPrefabBuildHelpers.SetReference(so, "bgmValueLabel", bgmValue);
            ScreenPrefabBuildHelpers.SetReference(so, "sfxValueLabel", sfxValue);
            so.ApplyModifiedPropertiesWithoutUndo();

            ScreenPrefabBuildHelpers.SavePrefab(root, SettingsPrefabPath);
            Debug.Log("[V2ScreenPrefabBuilders] Saved " + SettingsPrefabPath);
        }

        private static (Slider slider, TextMeshProUGUI valueLabel) BuildSliderRow(RectTransform parent, string label, Vector2 anchoredPosition)
        {
            // Row container
            GameObject rowGo = new GameObject("Row_" + label, typeof(RectTransform));
            RectTransform row = rowGo.GetComponent<RectTransform>();
            row.SetParent(parent, false);
            row.anchorMin = new Vector2(0.5f, 0.5f);
            row.anchorMax = new Vector2(0.5f, 0.5f);
            row.pivot = new Vector2(0.5f, 0.5f);
            row.sizeDelta = new Vector2(920f, 160f);
            row.anchoredPosition = anchoredPosition;

            // Left label
            CreateRowLabel(row, label, new Vector2(0f, 0f), new Vector2(0f, 0.5f), new Vector2(0.3f, 0.5f), TextAlignmentOptions.MidlineLeft);
            // Right value
            TextMeshProUGUI valueLabel = CreateRowLabel(row, "100%", new Vector2(0f, 0f), new Vector2(0.85f, 0.5f), new Vector2(1f, 0.5f), TextAlignmentOptions.MidlineRight);

            // Slider container in middle
            GameObject sliderGo = new GameObject("Slider", typeof(RectTransform), typeof(Slider));
            RectTransform sliderRect = sliderGo.GetComponent<RectTransform>();
            sliderRect.SetParent(row, false);
            sliderRect.anchorMin = new Vector2(0.3f, 0.5f);
            sliderRect.anchorMax = new Vector2(0.82f, 0.5f);
            sliderRect.pivot = new Vector2(0.5f, 0.5f);
            sliderRect.sizeDelta = new Vector2(0f, 40f);
            sliderRect.anchoredPosition = Vector2.zero;

            // Background track
            GameObject bgGo = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform bgRect = bgGo.GetComponent<RectTransform>();
            bgRect.SetParent(sliderRect, false);
            bgRect.anchorMin = new Vector2(0f, 0.5f);
            bgRect.anchorMax = new Vector2(1f, 0.5f);
            bgRect.pivot = new Vector2(0.5f, 0.5f);
            bgRect.sizeDelta = new Vector2(0f, 14f);
            bgGo.GetComponent<Image>().color = new Color(0.78f, 0.72f, 0.64f, 1f);

            // Fill area + fill
            GameObject fillArea = new GameObject("FillArea", typeof(RectTransform));
            RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
            fillAreaRect.SetParent(sliderRect, false);
            fillAreaRect.anchorMin = new Vector2(0f, 0.5f);
            fillAreaRect.anchorMax = new Vector2(1f, 0.5f);
            fillAreaRect.pivot = new Vector2(0.5f, 0.5f);
            fillAreaRect.sizeDelta = new Vector2(-30f, 14f);

            GameObject fillGo = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform fillRect = fillGo.GetComponent<RectTransform>();
            fillRect.SetParent(fillAreaRect, false);
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;
            fillGo.GetComponent<Image>().color = new Color(0.95f, 0.62f, 0.49f, 1f);

            // Handle area + handle
            GameObject handleArea = new GameObject("HandleSlideArea", typeof(RectTransform));
            RectTransform handleAreaRect = handleArea.GetComponent<RectTransform>();
            handleAreaRect.SetParent(sliderRect, false);
            handleAreaRect.anchorMin = new Vector2(0f, 0f);
            handleAreaRect.anchorMax = new Vector2(1f, 1f);
            handleAreaRect.pivot = new Vector2(0.5f, 0.5f);
            handleAreaRect.sizeDelta = new Vector2(-30f, 0f);

            GameObject handleGo = new GameObject("Handle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform handleRect = handleGo.GetComponent<RectTransform>();
            handleRect.SetParent(handleAreaRect, false);
            handleRect.sizeDelta = new Vector2(44f, 44f);
            Image handleImg = handleGo.GetComponent<Image>();
            handleImg.color = ScreenPrefabBuildHelpers.WarmBrown;

            Slider slider = sliderGo.GetComponent<Slider>();
            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.targetGraphic = handleImg;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0.7f;

            return (slider, valueLabel);
        }

        private static Toggle BuildToggleRow(RectTransform parent, string label, Vector2 anchoredPosition)
        {
            GameObject rowGo = new GameObject("Row_" + label, typeof(RectTransform));
            RectTransform row = rowGo.GetComponent<RectTransform>();
            row.SetParent(parent, false);
            row.anchorMin = new Vector2(0.5f, 0.5f);
            row.anchorMax = new Vector2(0.5f, 0.5f);
            row.pivot = new Vector2(0.5f, 0.5f);
            row.sizeDelta = new Vector2(920f, 140f);
            row.anchoredPosition = anchoredPosition;

            CreateRowLabel(row, label, new Vector2(0f, 0f), new Vector2(0f, 0.5f), new Vector2(0.7f, 0.5f), TextAlignmentOptions.MidlineLeft);

            GameObject toggleGo = new GameObject("Toggle", typeof(RectTransform), typeof(Toggle));
            RectTransform toggleRect = toggleGo.GetComponent<RectTransform>();
            toggleRect.SetParent(row, false);
            toggleRect.anchorMin = new Vector2(1f, 0.5f);
            toggleRect.anchorMax = new Vector2(1f, 0.5f);
            toggleRect.pivot = new Vector2(1f, 0.5f);
            toggleRect.sizeDelta = new Vector2(120f, 80f);
            toggleRect.anchoredPosition = Vector2.zero;

            GameObject bg = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform bgRect = bg.GetComponent<RectTransform>();
            bgRect.SetParent(toggleRect, false);
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            Image bgImage = bg.GetComponent<Image>();
            bgImage.color = new Color(0.78f, 0.72f, 0.64f, 1f);

            GameObject check = new GameObject("Checkmark", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform checkRect = check.GetComponent<RectTransform>();
            checkRect.SetParent(bgRect, false);
            checkRect.anchorMin = new Vector2(0.15f, 0.15f);
            checkRect.anchorMax = new Vector2(0.85f, 0.85f);
            checkRect.sizeDelta = Vector2.zero;
            Image checkImage = check.GetComponent<Image>();
            checkImage.color = new Color(0.95f, 0.62f, 0.49f, 1f);

            Toggle toggle = toggleGo.GetComponent<Toggle>();
            toggle.targetGraphic = bgImage;
            toggle.graphic = checkImage;
            toggle.isOn = true;
            return toggle;
        }

        private static TextMeshProUGUI CreateRowLabel(RectTransform parent, string text, Vector2 anchoredPos, Vector2 anchorMin, Vector2 anchorMax, TextAlignmentOptions alignment)
        {
            GameObject go = new GameObject("Label_" + text, typeof(RectTransform), typeof(CanvasRenderer));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = anchoredPos;

            TextMeshProUGUI label = go.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = 48;
            label.color = ScreenPrefabBuildHelpers.WarmBrown;
            label.alignment = alignment;
            label.raycastTarget = false;
            return label;
        }

        [MenuItem("Whisker Tales/V2/Build LevelSelectScreen Prefab")]
        public static void BuildLevelSelect()
        {
            EnsureDirectory();
            GameObject root = ScreenPrefabBuildHelpers.CreateRoot("LevelSelectScreen", typeof(LevelSelectScreenController), "levelselect");
            RectTransform rect = (RectTransform)root.transform;

            ScreenPrefabBuildHelpers.CreateTitle(rect, "레벨 선택");
            Button backButton = ScreenPrefabBuildHelpers.CreateBackButton(rect);

            // 10 levels in a 5×2 grid centered. Cell 160×160 with 36 spacing.
            const int cols = 5;
            const int rows = 2;
            const float cell = 160f;
            const float spacing = 36f;
            const float gridWidth = cols * cell + (cols - 1) * spacing;
            const float gridHeight = rows * cell + (rows - 1) * spacing;
            float originX = -(gridWidth - cell) * 0.5f;
            float originY = (gridHeight - cell) * 0.5f;

            Button[] levelButtons = new Button[cols * rows];

            for (int i = 0; i < levelButtons.Length; i++)
            {
                int col = i % cols;
                int row = i / cols;
                float x = originX + col * (cell + spacing);
                float y = originY - row * (cell + spacing);
                int levelId = i + 1;

                levelButtons[i] = ScreenPrefabBuildHelpers.CreatePrimaryButton(
                    rect,
                    "Btn_Level_" + levelId,
                    levelId.ToString(),
                    new Vector2(x, y),
                    new Vector2(cell, cell));
            }

            LevelSelectScreenController controller = root.GetComponent<LevelSelectScreenController>();
            SerializedObject so = new SerializedObject(controller);
            ScreenPrefabBuildHelpers.SetReference(so, "backButton", backButton);

            SerializedProperty arrayProp = so.FindProperty("levelButtons");

            if (arrayProp != null)
            {
                arrayProp.arraySize = levelButtons.Length;

                for (int i = 0; i < levelButtons.Length; i++)
                {
                    arrayProp.GetArrayElementAtIndex(i).objectReferenceValue = levelButtons[i];
                }
            }

            so.ApplyModifiedPropertiesWithoutUndo();

            ScreenPrefabBuildHelpers.SavePrefab(root, LevelSelectPrefabPath);
            Debug.Log("[V2ScreenPrefabBuilders] Saved " + LevelSelectPrefabPath);
        }

        public static void EnsureDirectory()
        {
            if (Directory.Exists(PrefabDirectory) == false)
            {
                Directory.CreateDirectory(PrefabDirectory);
            }
        }

        [MenuItem("Whisker Tales/V2/Build GameplayScreen Prefab")]
        public static void BuildGameplay()
        {
            EnsureDirectory();
            GameObject root = ScreenPrefabBuildHelpers.CreateRoot("GameplayScreen", typeof(GameplayScreenController), "gameplay");
            RectTransform rect = (RectTransform)root.transform;

            TextMeshProUGUI levelTitle = ScreenPrefabBuildHelpers.CreateTitle(rect, "Level 1");
            Button backButton = ScreenPrefabBuildHelpers.CreateBackButton(rect);
            RectTransform boardArea = ScreenPrefabBuildHelpers.CreateBoardArea(rect);

            // Edge-anchored HUD so wide Korean strings ("블록 30개 제거 (0/30)") don't clip past canvas.
            TextMeshProUGUI goal = CreateHudEdgeText(rect, "GoalText", isLeft: true, "목표");
            TextMeshProUGUI moves = CreateHudEdgeText(rect, "MovesText", isLeft: false, "이동");
            TextMeshProUGUI status = CreateHudText(rect, "StatusText", new Vector2(0f, -440f), TextAlignmentOptions.Center, string.Empty);

            GameplayScreenController controller = root.GetComponent<GameplayScreenController>();
            SerializedObject so = new SerializedObject(controller);
            ScreenPrefabBuildHelpers.SetReference(so, "backButton", backButton);
            ScreenPrefabBuildHelpers.SetReference(so, "boardArea", boardArea);
            ScreenPrefabBuildHelpers.SetReference(so, "match3Host", root.transform);
            ScreenPrefabBuildHelpers.SetReference(so, "goalText", goal);
            ScreenPrefabBuildHelpers.SetReference(so, "movesText", moves);
            ScreenPrefabBuildHelpers.SetReference(so, "statusText", status);
            ScreenPrefabBuildHelpers.SetReference(so, "levelTitle", levelTitle);
            so.ApplyModifiedPropertiesWithoutUndo();

            ScreenPrefabBuildHelpers.SavePrefab(root, GameplayPrefabPath);
            Debug.Log("[V2ScreenPrefabBuilders] Saved " + GameplayPrefabPath);
        }

        [MenuItem("Whisker Tales/V2/Build CatRoomScreen Prefab")]
        public static void BuildCatRoom()
        {
            EnsureDirectory();
            GameObject root = ScreenPrefabBuildHelpers.CreateRoot("CatRoomScreen", typeof(CatRoomScreenController), "catroom");
            RectTransform rect = (RectTransform)root.transform;

            // Layer order (back-to-front): bg → sunlight drift → dust motes → title/back → cat → body label.
            CreateSunlightDrift(rect);
            CreateDustMotes(rect);

            ScreenPrefabBuildHelpers.CreateTitle(rect, "고양이 방");
            Button backButton = ScreenPrefabBuildHelpers.CreateBackButton(rect);

            RectTransform catRect = CreateCat(rect);
            WhiskerTales.Polish.CatIdleLifeController idleAnimator = catRect.gameObject.AddComponent<WhiskerTales.Polish.CatIdleLifeController>();

            // Cat tap reaction: needs raycast on the cat Image to receive clicks.
            Image catImage = catRect.GetComponent<Image>();
            if (catImage != null)
            {
                catImage.raycastTarget = true;
            }
            catRect.gameObject.AddComponent<WhiskerTales.Polish.CatGazeReaction>();

            ScreenPrefabBuildHelpers.CreateBodyLabel(rect, "나비가 당신을 바라봅니다.", new Vector2(0f, -640f), new Vector2(900f, 100f), ScreenPrefabBuildHelpers.BodyFontSize);

            CatRoomScreenController controller = root.GetComponent<CatRoomScreenController>();
            SerializedObject so = new SerializedObject(controller);
            ScreenPrefabBuildHelpers.SetReference(so, "backButton", backButton);
            ScreenPrefabBuildHelpers.SetReference(so, "catRect", catRect);
            ScreenPrefabBuildHelpers.SetReference(so, "idleAnimator", idleAnimator);
            so.ApplyModifiedPropertiesWithoutUndo();

            ScreenPrefabBuildHelpers.SavePrefab(root, CatRoomPrefabPath);
            Debug.Log("[V2ScreenPrefabBuilders] Saved " + CatRoomPrefabPath);
        }

        private static void CreateSunlightDrift(RectTransform parent)
        {
            GameObject go = new GameObject("Sunlight", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(900f, 1600f);
            rect.anchoredPosition = new Vector2(-180f, 80f);
            rect.localRotation = Quaternion.Euler(0f, 0f, 22f);

            Image image = go.GetComponent<Image>();
            Sprite glow = AssetDatabase.LoadAssetAtPath<Sprite>(LanternGlowTextureGenerator.SpritePath);

            if (glow != null)
            {
                image.sprite = glow;
                image.type = Image.Type.Simple;
                image.preserveAspect = false;
            }

            image.color = new Color(1f, 0.92f, 0.62f, 0.18f);
            image.raycastTarget = false;

            go.AddComponent<WhiskerTales.Polish.SunlightDriftLayer>();
        }

        private static void CreateDustMotes(RectTransform parent)
        {
            GameObject go = new GameObject("DustMotes", typeof(RectTransform));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            WhiskerTales.Polish.DustMoteEmitter emitter = go.AddComponent<WhiskerTales.Polish.DustMoteEmitter>();

            Sprite[] sparkles = new Sprite[]
            {
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/WhiskerTales/Art/Effects/sparkle_01.png"),
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/WhiskerTales/Art/Effects/sparkle_02.png"),
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/WhiskerTales/Art/Effects/sparkle_03.png"),
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/WhiskerTales/Art/Effects/sparkle_04.png"),
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/WhiskerTales/Art/Effects/sparkle_05.png"),
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/WhiskerTales/Art/Effects/sparkle_06.png")
            };

            SerializedObject so = new SerializedObject(emitter);
            SerializedProperty list = so.FindProperty("sprites");

            if (list != null)
            {
                list.arraySize = sparkles.Length;

                for (int i = 0; i < sparkles.Length; i++)
                {
                    list.GetArrayElementAtIndex(i).objectReferenceValue = sparkles[i];
                }
            }

            SerializedProperty spawn = so.FindProperty("spawnArea");

            if (spawn != null)
            {
                spawn.objectReferenceValue = rect;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        [MenuItem("Whisker Tales/V2/Build CafeScreen Prefab")]
        public static void BuildCafe()
        {
            EnsureDirectory();
            GameObject root = ScreenPrefabBuildHelpers.CreateRoot("CafeScreen", typeof(CafeScreenController), "cafe");
            RectTransform rect = (RectTransform)root.transform;

            ApplyCafeBackground(rect);

            // Translucent dark strips behind text for legibility over the busy hanok bg.
            CreateBackdropTop(rect, new Vector2(0f, -180f), new Vector2(900f, 180f));
            TextMeshProUGUI title = ScreenPrefabBuildHelpers.CreateTitle(rect, "카페");
            title.color = ScreenPrefabBuildHelpers.BackgroundCream;

            Button backButton = ScreenPrefabBuildHelpers.CreateBackButton(rect);

            CreateBackdropCenter(rect, new Vector2(0f, 0f), new Vector2(900f, 140f));
            TextMeshProUGUI body = ScreenPrefabBuildHelpers.CreateBodyLabel(rect, "조용한 오후, 따뜻한 차 한 잔.", new Vector2(0f, 0f), new Vector2(900f, 120f), ScreenPrefabBuildHelpers.BodyFontSize);
            body.color = ScreenPrefabBuildHelpers.BackgroundCream;

            CafeScreenController controller = root.GetComponent<CafeScreenController>();
            SerializedObject so = new SerializedObject(controller);
            ScreenPrefabBuildHelpers.SetReference(so, "backButton", backButton);
            so.ApplyModifiedPropertiesWithoutUndo();

            ScreenPrefabBuildHelpers.SavePrefab(root, CafePrefabPath);
            Debug.Log("[V2ScreenPrefabBuilders] Saved " + CafePrefabPath);
        }

        private static void CreateBackdropTop(RectTransform parent, Vector2 anchoredPosition, Vector2 size)
        {
            CreateBackdrop(parent, "Backdrop", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), anchoredPosition, size);
        }

        private static void CreateBackdropCenter(RectTransform parent, Vector2 anchoredPosition, Vector2 size)
        {
            CreateBackdrop(parent, "Backdrop", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), anchoredPosition, size);
        }

        private static void CreateBackdrop(RectTransform parent, string name, Vector2 anchor, Vector2 pivot, Vector2 anchoredPosition, Vector2 size)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;

            Image image = go.GetComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.42f);
            image.raycastTarget = false;
        }

        [MenuItem("Whisker Tales/V2/Build LevelClearScreen Prefab")]
        public static void BuildLevelClear()
        {
            EnsureDirectory();
            // Fully qualified — WhiskerTales.UI.LevelClearScreenController (V1 Phase A+B) collides
            // with WhiskerTales.UI.Screens.LevelClearScreenController via the two using directives.
            GameObject root = ScreenPrefabBuildHelpers.CreateRoot("LevelClearScreen", typeof(WhiskerTales.UI.Screens.LevelClearScreenController), "levelclear");
            RectTransform rect = (RectTransform)root.transform;

            ScreenPrefabBuildHelpers.CreateTitle(rect, "레벨 클리어!");
            ScreenPrefabBuildHelpers.CreateBodyLabel(rect, "잘했어요. 다음으로 가볼까요?", new Vector2(0f, 200f), new Vector2(900f, 100f), ScreenPrefabBuildHelpers.BodyFontSize);

            Button homeButton = ScreenPrefabBuildHelpers.CreatePrimaryButton(rect, "Btn_Home", "홈으로", new Vector2(0f, -50f), new Vector2(640f, 180f));
            Button nextButton = ScreenPrefabBuildHelpers.CreatePrimaryButton(rect, "Btn_Next", "다시 도전", new Vector2(0f, -260f), new Vector2(640f, 180f));

            WhiskerTales.UI.Screens.LevelClearScreenController controller = root.GetComponent<WhiskerTales.UI.Screens.LevelClearScreenController>();
            SerializedObject so = new SerializedObject(controller);
            ScreenPrefabBuildHelpers.SetReference(so, "homeButton", homeButton);
            ScreenPrefabBuildHelpers.SetReference(so, "nextButton", nextButton);
            so.ApplyModifiedPropertiesWithoutUndo();

            ScreenPrefabBuildHelpers.SavePrefab(root, LevelClearPrefabPath);
            Debug.Log("[V2ScreenPrefabBuilders] Saved " + LevelClearPrefabPath);
        }

        [MenuItem("Whisker Tales/V2/Build GameFailScreen Prefab")]
        public static void BuildGameFail()
        {
            EnsureDirectory();
            GameObject root = ScreenPrefabBuildHelpers.CreateRoot("GameFailScreen", typeof(GameFailScreenController), "gamefail");
            RectTransform rect = (RectTransform)root.transform;

            ScreenPrefabBuildHelpers.CreateTitle(rect, "다시 도전!");
            ScreenPrefabBuildHelpers.CreateBodyLabel(rect, "괜찮아요. 한 번 더 해봐요.", new Vector2(0f, 200f), new Vector2(900f, 100f), ScreenPrefabBuildHelpers.BodyFontSize);

            Button homeButton = ScreenPrefabBuildHelpers.CreatePrimaryButton(rect, "Btn_Home", "홈으로", new Vector2(0f, -50f), new Vector2(640f, 180f));
            Button retryButton = ScreenPrefabBuildHelpers.CreatePrimaryButton(rect, "Btn_Retry", "다시 시작", new Vector2(0f, -260f), new Vector2(640f, 180f));

            GameFailScreenController controller = root.GetComponent<GameFailScreenController>();
            SerializedObject so = new SerializedObject(controller);
            ScreenPrefabBuildHelpers.SetReference(so, "homeButton", homeButton);
            ScreenPrefabBuildHelpers.SetReference(so, "retryButton", retryButton);
            so.ApplyModifiedPropertiesWithoutUndo();

            ScreenPrefabBuildHelpers.SavePrefab(root, GameFailPrefabPath);
            Debug.Log("[V2ScreenPrefabBuilders] Saved " + GameFailPrefabPath);
        }

        private static TextMeshProUGUI CreateHudEdgeText(RectTransform parent, string name, bool isLeft, string text)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);

            if (isLeft == true)
            {
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.sizeDelta = new Vector2(700f, 80f);
                rect.anchoredPosition = new Vector2(60f, -340f);
            }
            else
            {
                rect.anchorMin = new Vector2(1f, 1f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(1f, 1f);
                rect.sizeDelta = new Vector2(420f, 80f);
                rect.anchoredPosition = new Vector2(-60f, -340f);
            }

            TextMeshProUGUI label = go.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = 52;
            label.color = ScreenPrefabBuildHelpers.WarmBrown;
            label.alignment = isLeft == true ? TextAlignmentOptions.Left : TextAlignmentOptions.Right;
            label.raycastTarget = false;
            return label;
        }

        private static TextMeshProUGUI CreateHudText(RectTransform parent, string name, Vector2 anchoredPos, TextAlignmentOptions align, string text)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(500f, 80f);
            rect.anchoredPosition = anchoredPos;

            TextMeshProUGUI label = go.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = 52;
            label.color = ScreenPrefabBuildHelpers.WarmBrown;
            label.alignment = align;
            label.raycastTarget = false;
            return label;
        }

        private static RectTransform CreateCat(RectTransform parent)
        {
            GameObject go = new GameObject("Cat", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(720f, 720f);
            rect.anchoredPosition = new Vector2(0f, 0f);

            Image image = go.GetComponent<Image>();
            // Prefer the alpha-processed cat (white bg removed); fall back to raw if generator
            // hasn't run yet.
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(CatAlphaSpritePath);

            if (sprite == null)
            {
                sprite = AssetDatabase.LoadAssetAtPath<Sprite>(CatSpritePath);
            }

            if (sprite != null)
            {
                image.sprite = sprite;
                image.preserveAspect = true;
            }
            else
            {
                image.color = ScreenPrefabBuildHelpers.WarmBrown;
            }

            image.raycastTarget = false;
            return rect;
        }

        private static void ApplyCafeBackground(RectTransform parent)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(CafeBgSpritePath);

            if (sprite == null)
            {
                return;
            }

            Transform bg = parent.Find("Background");

            if (bg == null)
            {
                return;
            }

            Image image = bg.GetComponent<Image>();

            if (image != null)
            {
                image.sprite = sprite;
                image.color = Color.white;
                image.preserveAspect = false;
            }
        }
    }
}
#endif
