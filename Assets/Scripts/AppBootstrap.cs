using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using WhiskerTales.Audio;
using WhiskerTales.Cat;
using WhiskerTales.Core;
using WhiskerTales.Currency;
using WhiskerTales.Puzzle;
using WhiskerTales.Settings;
using WhiskerTales.Sleep;
using WhiskerTales.UI;
using WhiskerTales.Utilities;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WhiskerTales.Bootstrap
{
    /// <summary>
    /// Single entry-point. Attach to ONE empty GameObject in MainScenes.unity, press Play.
    /// Auto-generates managers + UI without any Inspector wiring.
    /// PNG sprites are loaded directly from Assets/Sprites/ via AssetDatabase (Editor only) —
    /// no file moves required. Runtime builds need Addressables/Resources (TODO).
    /// </summary>
    [DefaultExecutionOrder(-500)]
    public class AppBootstrap : MonoBehaviour
    {
        [Header("Initial Screen")]
        [SerializeField] private NavigationTarget initialPanel = NavigationTarget.Title;

        [Header("Initial Level (gameplay test)")]
        [SerializeField] private int moveLimit = 25;
        [SerializeField] private int goalValue = 50;
        [SerializeField] private LevelGoalType goalType = LevelGoalType.RemoveBlocks;

        public static AppBootstrap Instance { get; private set; }

        private SpriteLibrary spriteLib;
        private Canvas rootCanvas;
        private RectTransform titlePanel;
        private RectTransform gameplayPanel;
        private RectTransform catRoomPanel;
        private RectTransform cafePanel;
        private RectTransform arcadePanel;
        private RectTransform openingPanel;
        private OpeningScenario openingScenario;
        private LoadingScreen loadingScreen;
        private DetoxMessageModal detoxModal;
        private SleepModeScreen sleepModeScreen;
        private RectTransform meditationPanel;
        private MeditationGardenController meditationController;
        private RectTransform photoStudioPanel;
        private PhotoStudioController photoStudioController;
        private RectTransform settingsPanel;
        private SettingsScreen settingsScreen;
        private TextMeshProUGUI titleNyangiHeartText;
        private Dictionary<NavigationTarget, RectTransform> panels;

        public PhotoStudioController PhotoStudio => photoStudioController;
        public SettingsScreen Settings => settingsScreen;

        public DetoxMessageModal DetoxModal => detoxModal;
        public SleepModeScreen SleepScreen => sleepModeScreen;

        private void Awake()
        {
            Instance = this;
            spriteLib = new SpriteLibrary();
            spriteLib.LoadAll();
            TileView.SetTileSprites(spriteLib.tiles);

            EnsureEventSystem();
            EnsureCoreManagers();

            rootCanvas = CreateRootCanvas();

            titlePanel    = BuildTitlePanel(rootCanvas.transform);
            gameplayPanel = BuildGameplayPanel(rootCanvas.transform);
            catRoomPanel  = BuildCatRoomPanel(rootCanvas.transform);
            cafePanel     = BuildCafeRestorationPanel(rootCanvas.transform);
            arcadePanel     = BuildArcadePanel(rootCanvas.transform);
            meditationPanel = BuildMeditationGardenPanel(rootCanvas.transform);
            photoStudioPanel = BuildPhotoStudioPanel(rootCanvas.transform);
            settingsPanel    = BuildSettingsPanel(rootCanvas.transform);
            openingPanel    = BuildOpeningPanel(rootCanvas.transform);
            loadingScreen   = BuildLoadingScreen(rootCanvas.transform);
            detoxModal      = BuildDetoxMessageModal(rootCanvas.transform);
            sleepModeScreen = BuildSleepModeScreen(rootCanvas.transform);

            panels = new Dictionary<NavigationTarget, RectTransform>
            {
                { NavigationTarget.Title,            titlePanel },
                { NavigationTarget.Gameplay,         gameplayPanel },
                { NavigationTarget.CatRoom,          catRoomPanel },
                { NavigationTarget.Cafe,             cafePanel },
                { NavigationTarget.Arcade,           arcadePanel },
                { NavigationTarget.MeditationGarden, meditationPanel },
                { NavigationTarget.PhotoStudio,      photoStudioPanel },
                { NavigationTarget.Settings,         settingsPanel },
            };

            if (GameManager.Instance != null)
                GameManager.Instance.OnNavigationRequested += ShowPanel;

            bool firstRun = PlayerPrefs.GetInt(OpeningScenario.PREF_SEEN, 0) == 0;
            if (firstRun)
                StartOpeningScenario();
            else
                ShowPanel(initialPanel);

            Debug.Log($"[AppBootstrap] Scene constructed (firstRun={firstRun})");
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnNavigationRequested -= ShowPanel;
            if (CurrencyManager.Instance != null)
                CurrencyManager.Instance.OnNyangiHeartChanged -= HandleNyangiHeartChanged;
        }

        // ===== managers / event system =====

        private void EnsureEventSystem()
        {
            if (EventSystem.current != null) return;
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        private void EnsureCoreManagers()
        {
            GameObject managers = GameObject.Find("__Managers__");
            if (managers == null) managers = new GameObject("__Managers__");

            if (FindObjectOfType<GameManager>() == null)
                managers.AddComponent<GameManager>();
            if (FindObjectOfType<CatManager>() == null)
                managers.AddComponent<CatManager>();

            CafeRestorationManager cafe = FindObjectOfType<CafeRestorationManager>();
            if (cafe == null)
            {
                cafe = managers.AddComponent<CafeRestorationManager>();
                InjectField(cafe, "zoneBackgrounds", spriteLib.BuildZoneBackgrounds(typeof(CafeRestorationManager)));

                // Inject CafeRestorationData.json (kept at Assets/_Data/Cafe/, no file moves)
                // before its Start() runs.
                CafeRestorationManager.CafeRestorationData data = LoadCafeRestorationData();
                if (data != null) cafe.InjectCafeData(data);
            }

            if (FindObjectOfType<AudioManager>() == null)
                managers.AddComponent<AudioManager>();
            if (FindObjectOfType<I18nManager>() == null)
                managers.AddComponent<I18nManager>();
            if (FindObjectOfType<IdleRewardSystem>() == null)
                managers.AddComponent<IdleRewardSystem>();

            if (SoundManager.Instance == null)
            {
                managers.AddComponent<SoundManager>();
                RegisterSoundManagerClips();
            }
            if (CurrencyManager.Instance == null)
            {
                managers.AddComponent<CurrencyManager>();
            }
            if (SettingsManager.Instance == null)
            {
                managers.AddComponent<SettingsManager>();
            }
            if (SleepModeManager.Instance == null)
            {
                managers.AddComponent<SleepModeManager>();
            }

            if (I18nManager.Instance != null)
                I18nManager.Instance.SetLanguage(Application.systemLanguage);

            InstallFontFallbacks();
        }

        /// <summary>
        /// LiberationSans SDF (TMP Essentials 기본 폰트)는 라틴 글리프만 가지고 있어서
        /// 한국어/CJK/Devanagari/Thai 등이 □로 렌더링됨. Assets/Fonts/Resources/에 번들된
        /// Noto Sans 시리즈(Google Fonts OFL)를 Resources.Load로 읽어 동적 TMP_FontAsset 생성,
        /// TMP_Settings.fallbackFontAssets에 체인 등록. 런타임/APK 빌드 모두 호환.
        /// 순서는 사용 빈도 우선 (KR → CJK → Latin Extended → Devanagari → Thai).
        /// RTL(아랍어/히브리어)은 의도적 제외.
        /// </summary>
        private static void InstallFontFallbacks()
        {
            try
            {
                if (TMPro.TMP_Settings.fallbackFontAssets == null) return;

                string[] fontResourceNames =
                {
                    "NotoSansKR-Regular",          // Korean
                    "NotoSansJP-Regular",          // Japanese
                    "NotoSansSC-Regular",          // Simplified Chinese
                    "NotoSansTC-Regular",          // Traditional Chinese
                    "NotoSans-Regular",            // Latin Extended (악센트, 유럽 추가 글리프)
                    "NotoSansDevanagari-Regular",  // Devanagari (Hindi 등)
                    "NotoSansThai-Regular",        // Thai
                };

                int registered = 0, skipped = 0, failed = 0;
                foreach (string resName in fontResourceNames)
                {
                    string assetName = $"NotoFallback_{resName}";
                    if (HasFallbackNamed(assetName)) { skipped++; continue; }

                    Font font = Resources.Load<Font>(resName);
                    if (font == null)
                    {
                        Debug.LogWarning($"[AppBootstrap] Font resource missing: Resources/{resName}");
                        failed++;
                        continue;
                    }

                    TMPro.TMP_FontAsset asset = TMPro.TMP_FontAsset.CreateFontAsset(font);
                    if (asset == null)
                    {
                        Debug.LogWarning($"[AppBootstrap] CreateFontAsset returned null for {resName}");
                        failed++;
                        continue;
                    }

                    asset.name = assetName;
                    asset.atlasPopulationMode = TMPro.AtlasPopulationMode.Dynamic;
                    TMPro.TMP_Settings.fallbackFontAssets.Add(asset);
                    registered++;
                }

                Debug.Log($"[AppBootstrap] Font fallbacks: registered={registered}, skipped={skipped}, failed={failed}");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[AppBootstrap] Font fallback setup failed: {e.GetType().Name}: {e.Message}");
            }
        }

        private static bool HasFallbackNamed(string assetName)
        {
            var list = TMPro.TMP_Settings.fallbackFontAssets;
            if (list == null) return false;
            foreach (var f in list)
            {
                if (f != null && f.name == assetName) return true;
            }
            return false;
        }

        // ===== canvas =====

        private Canvas CreateRootCanvas()
        {
            GameObject go = new GameObject("Canvas",
                typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas c = go.GetComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = 0;

            CanvasScaler s = go.GetComponent<CanvasScaler>();
            s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            s.referenceResolution = new Vector2(1080, 1920);
            s.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            s.matchWidthOrHeight = 0.5f;
            return c;
        }

        private RectTransform NewPanel(Transform parent, string name)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image bg = go.GetComponent<Image>();
            bg.sprite = TileView.GetWhiteSprite();
            bg.color = new Color(0.10f, 0.12f, 0.16f, 1f);
            bg.raycastTarget = true;
            return rt;
        }

        // ===== TITLE =====

        private RectTransform BuildTitlePanel(Transform parent)
        {
            RectTransform panel = NewPanel(parent, "TitlePanel");
            panel.gameObject.SetActive(false);  // ShowPanel activates exactly one
            Image bg = panel.GetComponent<Image>();
            Sprite bgSprite = spriteLib.GetBackground(1, 1);
            if (bgSprite != null) { bg.sprite = bgSprite; bg.color = Color.white; }

            // 💝 Nyangi-heart indicator (top-right). Subscribes to CurrencyManager events.
            titleNyangiHeartText = CreateText(panel, "NyangiHeartText",
                new Vector2(1, 1), new Vector2(1, 1), new Vector2(-200, -100), new Vector2(320, 80),
                TextAlignmentOptions.Right, 48, "💝 0");
            titleNyangiHeartText.color = new Color(0.95f, 0.40f, 0.55f);
            BindNyangiHeartIndicator();

            // ⚙ Settings entry (top-left)
            Button settingsBtn = CreateButton(panel, "SettingsButton",
                new Vector2(0, 1), new Vector2(0, 1), new Vector2(90, -100), new Vector2(120, 120),
                "⚙", new Color(0.20f, 0.20f, 0.25f, 0.85f));
            settingsBtn.onClick.AddListener(() =>
            {
                AudioManager.instance?.PlayButtonClick();
                GameManager.Instance?.RequestNavigation(NavigationTarget.Settings);
            });

            CreateText(panel, "Title", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0, -200), new Vector2(900, 220), TextAlignmentOptions.Center, 110, "Whisker Tales");

            Button play = CreateButton(panel, "PlayButton",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 100), new Vector2(560, 160),
                "Play", new Color(0.95f, 0.55f, 0.30f, 1f));
            play.onClick.AddListener(() =>
            {
                AudioManager.instance?.PlayButtonClick();
                GameManager.Instance?.StartLevel(GameManager.Instance.UserProgress.currentLevel);
                GameManager.Instance?.RequestNavigation(NavigationTarget.Gameplay);
            });

            Button cats = CreateButton(panel, "CatsButton",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -100), new Vector2(560, 140),
                "Cat Room", new Color(0.40f, 0.62f, 0.95f, 1f));
            cats.onClick.AddListener(() =>
            {
                AudioManager.instance?.PlayButtonClick();
                GameManager.Instance?.RequestNavigation(NavigationTarget.CatRoom);
            });

            Button cafe = CreateButton(panel, "CafeButton",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -270), new Vector2(560, 140),
                "Cafe", new Color(0.50f, 0.78f, 0.55f, 1f));
            cafe.onClick.AddListener(() => GameManager.Instance?.GoToCafe());

            Button arcade = CreateButton(panel, "ArcadeButton",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -440), new Vector2(560, 140),
                "오락실", new Color(0.831f, 0.659f, 0.278f, 1f));
            arcade.onClick.AddListener(() =>
            {
                AudioManager.instance?.PlayButtonClick();
                GameManager.Instance?.RequestNavigation(NavigationTarget.Arcade);
            });

            Button meditation = CreateButton(panel, "MeditationButton",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -610), new Vector2(560, 140),
                "명상 정원", new Color(0.483f, 0.722f, 0.553f, 1f));
            meditation.onClick.AddListener(() =>
            {
                AudioManager.instance?.PlayButtonClick();
                GameManager.Instance?.RequestNavigation(NavigationTarget.MeditationGarden);
            });

            return panel;
        }

        // ===== GAMEPLAY =====

        private RectTransform BuildGameplayPanel(Transform parent)
        {
            RectTransform panel = NewPanel(parent, "GameplayPanel");
            // Build inactive so child scripts' OnEnable doesn't fire before InjectField populates
            // their private SerializeFields. ShowPanel will activate when navigated to.
            panel.gameObject.SetActive(false);

            // top background image (decorative — also used by GameplayUI.backgroundImage)
            Image bgImage = CreateImageObject(panel, "BackgroundImage",
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -560), new Vector2(0, 1100));
            bgImage.color = Color.white;
            Sprite zone1 = spriteLib.GetBackground(1, 1);
            if (zone1 != null) bgImage.sprite = zone1; else bgImage.color = new Color(0.18f, 0.22f, 0.30f, 1f);

            // detox copy under top bar
            TextMeshProUGUI detox = CreateText(panel, "DetoxCopy",
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -100), new Vector2(-80, 80),
                TextAlignmentOptions.Center, 36, "");

            // top-bar buttons
            Button pauseBtn = CreateButton(panel, "PauseButton",
                new Vector2(1, 1), new Vector2(1, 1), new Vector2(-80, -80), new Vector2(120, 120),
                "II", new Color(0.20f, 0.20f, 0.25f, 0.85f));
            Button menuBtn = CreateButton(panel, "MenuButton",
                new Vector2(0, 1), new Vector2(0, 1), new Vector2(80, -80), new Vector2(120, 120),
                "<", new Color(0.20f, 0.20f, 0.25f, 0.85f));

            // HUD (left column)
            RectTransform hud = MakeRT(panel, "HUD",
                new Vector2(0, 0.5f), new Vector2(0, 0.5f),
                new Vector2(220, 200), new Vector2(360, 460));

            TextMeshProUGUI levelText = CreateText(hud, "LevelText",
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -30), new Vector2(0, 56),
                TextAlignmentOptions.Left, 50, "Level 1");
            TextMeshProUGUI scoreText = CreateText(hud, "ScoreText",
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -100), new Vector2(0, 50),
                TextAlignmentOptions.Left, 42, "Score: 0");
            TextMeshProUGUI movesText = CreateText(hud, "MovesText",
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -160), new Vector2(0, 50),
                TextAlignmentOptions.Left, 42, "Moves: 0");
            TextMeshProUGUI goalText = CreateText(hud, "GoalText",
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -240), new Vector2(0, 90),
                TextAlignmentOptions.Left, 36, "Goal");
            Slider progressSlider = CreateSlider(hud, "ProgressSlider",
                new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 60), new Vector2(0, 30));

            // Feedback overlays (center)
            TextMeshProUGUI feedbackText = CreateText(panel, "FeedbackText",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 480), new Vector2(900, 90),
                TextAlignmentOptions.Center, 64, "");
            feedbackText.color = new Color(1f, 0.92f, 0.45f);
            TextMeshProUGUI comboText = CreateText(panel, "ComboText",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 380), new Vector2(900, 70),
                TextAlignmentOptions.Center, 50, "");
            comboText.color = new Color(1f, 0.65f, 0.30f);

            // BoardArea (center)
            RectTransform gridArea = BuildBoardArea(panel);

            // Booster panel (right column) → 3 buttons
            RectTransform boosterRoot = MakeRT(panel, "BoosterPanel",
                new Vector2(1, 0.5f), new Vector2(1, 0.5f),
                new Vector2(-180, 0), new Vector2(280, 700));
            BoosterPanel boosterScript = boosterRoot.gameObject.AddComponent<BoosterPanel>();

            (Button hammerBtn,    TMP_Text hammerCount)    = MakeBoosterButton(boosterRoot, "Hammer",    new Vector2(0, 220),  new Color(0.95f, 0.65f, 0.30f));
            (Button colorBombBtn, TMP_Text colorBombCount) = MakeBoosterButton(boosterRoot, "ColorBomb", new Vector2(0, 0),    new Color(0.85f, 0.40f, 0.85f));
            (Button shuffleBtn,   TMP_Text shuffleCount)   = MakeBoosterButton(boosterRoot, "Shuffle",   new Vector2(0, -220), new Color(0.40f, 0.78f, 0.85f));

            // Build BoosterEntry array via the public nested type
            var entries = new BoosterPanel.BoosterEntry[]
            {
                new BoosterPanel.BoosterEntry { type = BoosterPanel.Booster.Hammer,    button = hammerBtn,    countText = hammerCount,    initialCount = 3 },
                new BoosterPanel.BoosterEntry { type = BoosterPanel.Booster.ColorBomb, button = colorBombBtn, countText = colorBombCount, initialCount = 3 },
                new BoosterPanel.BoosterEntry { type = BoosterPanel.Booster.Shuffle,   button = shuffleBtn,   countText = shuffleCount,   initialCount = 3 },
            };
            InjectField(boosterScript, "entries", entries);

            // LevelClearPanel (overlay, hidden)
            (RectTransform lcRoot, LevelClearPanel lcScript) = BuildLevelClearOverlay(panel);

            // LevelFailPanel (overlay, hidden)
            GameObject failRoot = BuildLevelFailOverlay(panel);

            // Puzzle data + view (procedural — same pattern as GameBootstrap)
            Board board = new GameObject("Board").AddComponent<Board>();
            LevelGoal levelGoal = new GameObject("LevelGoal").AddComponent<LevelGoal>();
            board.transform.SetParent(panel, false);
            levelGoal.transform.SetParent(panel, false);

            WhiskerTales.Puzzle.Level level = new WhiskerTales.Puzzle.Level
            {
                levelId = 1,
                moveLimit = this.moveLimit,
                goalType = this.goalType,
                goalValue = this.goalValue
            };
            board.Initialize(level, levelGoal);

            BoardView view = new GameObject("BoardView").AddComponent<BoardView>();
            view.transform.SetParent(panel, false);
            view.board = board;
            view.levelGoal = levelGoal;
            view.gridContainer = gridArea;
            view.goalText = null;     // GameplayUI handles HUD; BoardView only repaints board
            view.movesText = null;
            view.statusText = null;
            view.BuildGrid();

            // GameplayUI script — inject all private SerializeFields
            GameplayUI ui = panel.gameObject.AddComponent<GameplayUI>();
            InjectField(ui, "levelText", levelText);
            InjectField(ui, "scoreText", scoreText);
            InjectField(ui, "movesText", movesText);
            InjectField(ui, "goalText", goalText);
            InjectField(ui, "progressSlider", progressSlider);
            InjectField(ui, "backgroundImage", bgImage);
            InjectField(ui, "detoxCopyText", detox);
            InjectField(ui, "pauseButton", pauseBtn);
            InjectField(ui, "menuButton", menuBtn);
            InjectField(ui, "feedbackText", feedbackText);
            InjectField(ui, "comboText", comboText);
            InjectField(ui, "levelClearPanel", lcScript);
            InjectField(ui, "levelFailPanel", failRoot);

            return panel;
        }

        private RectTransform BuildBoardArea(Transform parent)
        {
            GameObject area = new GameObject("BoardArea", typeof(RectTransform), typeof(GridLayoutGroup));
            RectTransform rt = area.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(-50, -100);
            rt.sizeDelta = new Vector2(960, 960);

            GridLayoutGroup grid = area.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(112, 112);
            grid.spacing = new Vector2(8, 8);
            grid.padding = new RectOffset(8, 8, 8, 8);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 8;
            grid.childAlignment = TextAnchor.MiddleCenter;
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;

            Image bg = area.AddComponent<Image>();
            bg.sprite = TileView.GetWhiteSprite();
            bg.color = new Color(0.07f, 0.09f, 0.12f, 0.85f);
            bg.raycastTarget = false;
            return rt;
        }

        private (Button, TMP_Text) MakeBoosterButton(Transform parent, string label, Vector2 anchoredPos, Color tint)
        {
            Button btn = CreateButton(parent, $"Booster_{label}",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                anchoredPos, new Vector2(220, 180), label, tint);
            // count text below
            TMP_Text count = CreateText(parent, $"Booster_{label}_Count",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                anchoredPos + new Vector2(80, -60), new Vector2(80, 50),
                TextAlignmentOptions.Right, 36, "x3");
            count.color = new Color(1f, 0.85f, 0.40f);
            return (btn, count);
        }

        private (RectTransform, LevelClearPanel) BuildLevelClearOverlay(RectTransform parent)
        {
            // Host stays active so the script's coroutines/listeners stay alive while the
            // visible "root" child toggles visibility via Show()/Hide().
            RectTransform host = MakeRT(parent, "LevelClearHost",
                new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);

            RectTransform root = MakeRT(host, "LevelClearPanel",
                new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
            Image dim = root.gameObject.AddComponent<Image>();
            dim.sprite = TileView.GetWhiteSprite();
            dim.color = new Color(0, 0, 0, 0.55f);
            dim.raycastTarget = true;

            TextMeshProUGUI title = CreateText(root, "Title",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 360), new Vector2(900, 120),
                TextAlignmentOptions.Center, 80, "Level Complete!");

            Image[] starImages = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                starImages[i] = CreateImageObject(root, $"Star{i}",
                    new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(-220 + i * 220, 100), new Vector2(180, 180));
            }

            TextMeshProUGUI coinReward = CreateText(root, "CoinReward",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -120), new Vector2(900, 90),
                TextAlignmentOptions.Center, 64, "");
            coinReward.color = new Color(1f, 0.85f, 0.40f);

            Button continueBtn = CreateButton(root, "ContinueButton",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -340), new Vector2(560, 140),
                "Continue", new Color(0.95f, 0.55f, 0.30f, 1f));

            LevelClearPanel script = host.gameObject.AddComponent<LevelClearPanel>();
            InjectField(script, "root", root.gameObject);
            InjectField(script, "starImages", starImages);
            InjectField(script, "starFilled", spriteLib.starFilled);
            InjectField(script, "starEmpty", spriteLib.starEmpty);
            InjectField(script, "coinRewardText", coinReward);
            InjectField(script, "titleText", title);
            InjectField(script, "continueButton", continueBtn);

            foreach (var img in starImages) img.sprite = spriteLib.starEmpty;

            root.gameObject.SetActive(false);
            return (host, script);
        }

        private GameObject BuildLevelFailOverlay(RectTransform parent)
        {
            RectTransform root = MakeRT(parent, "LevelFailPanel",
                new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
            Image dim = root.gameObject.AddComponent<Image>();
            dim.sprite = TileView.GetWhiteSprite();
            dim.color = new Color(0, 0, 0, 0.55f);
            CreateText(root, "Title",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 200), new Vector2(900, 120),
                TextAlignmentOptions.Center, 80, "Out of Moves");
            Button retry = CreateButton(root, "RetryButton",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -100), new Vector2(560, 140),
                "Back to Menu", new Color(0.40f, 0.62f, 0.95f, 1f));
            retry.onClick.AddListener(() => GameManager.Instance?.ReturnToMenu());
            root.gameObject.SetActive(false);
            return root.gameObject;
        }

        // ===== CAT ROOM (CatBondScreen) =====

        private RectTransform BuildCatRoomPanel(Transform parent)
        {
            RectTransform panel = NewPanel(parent, "CatRoomPanel");
            panel.gameObject.SetActive(false);  // see note in BuildGameplayPanel
            Image bg = panel.GetComponent<Image>();
            Sprite bgSprite = spriteLib.GetBackground(2, 5);
            if (bgSprite != null) { bg.sprite = bgSprite; bg.color = Color.white; }

            // Top bar
            Button backBtn = CreateButton(panel, "BackButton",
                new Vector2(0, 1), new Vector2(0, 1), new Vector2(80, -80), new Vector2(120, 120),
                "<", new Color(0.20f, 0.20f, 0.25f, 0.85f));
            TextMeshProUGUI titleText = CreateText(panel, "TitleText",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -100), new Vector2(700, 100),
                TextAlignmentOptions.Center, 60, "Whisker Tales");
            Button cameraBtn = CreateButton(panel, "CameraButton",
                new Vector2(1, 1), new Vector2(1, 1), new Vector2(-220, -80), new Vector2(120, 120),
                "Cam", new Color(0.20f, 0.20f, 0.25f, 0.85f));
            Button helpBtn = CreateButton(panel, "HelpButton",
                new Vector2(1, 1), new Vector2(1, 1), new Vector2(-80, -80), new Vector2(120, 120),
                "?", new Color(0.20f, 0.20f, 0.25f, 0.85f));

            // Name tag (top-left under back btn)
            TextMeshProUGUI catName = CreateText(panel, "CatNameText",
                new Vector2(0, 1), new Vector2(0, 1), new Vector2(280, -200), new Vector2(420, 80),
                TextAlignmentOptions.Left, 56, "");
            TextMeshProUGUI catLevel = CreateText(panel, "CatLevelText",
                new Vector2(0, 1), new Vector2(0, 1), new Vector2(280, -270), new Vector2(420, 60),
                TextAlignmentOptions.Left, 40, "");
            catLevel.color = new Color(1f, 0.85f, 0.40f);

            // Reward hint (top-right)
            TextMeshProUGUI rewardAffinity = CreateText(panel, "RewardAffinityText",
                new Vector2(1, 1), new Vector2(1, 1), new Vector2(-280, -200), new Vector2(420, 60),
                TextAlignmentOptions.Right, 36, "");
            TextMeshProUGUI rewardCoins = CreateText(panel, "RewardCoinsText",
                new Vector2(1, 1), new Vector2(1, 1), new Vector2(-280, -260), new Vector2(420, 60),
                TextAlignmentOptions.Right, 36, "");

            // Cat fullshot center
            Image catFullshot = CreateImageObject(panel, "CatFullshot",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 100), new Vector2(720, 1000));
            catFullshot.preserveAspect = true;

            // Affinity bar (bottom area, above buttons)
            RectTransform barRoot = MakeRT(panel, "AffinityBar",
                new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 540), new Vector2(820, 90));
            Image barBg = barRoot.gameObject.AddComponent<Image>();
            barBg.sprite = TileView.GetWhiteSprite();
            barBg.color = new Color(0, 0, 0, 0.55f);

            Image barFill = CreateImageObject(barRoot, "Fill",
                new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
            // Fill anchor stretch — set offsets to zero
            RectTransform fillRT = (RectTransform)barFill.transform;
            fillRT.anchorMin = new Vector2(0, 0);
            fillRT.anchorMax = new Vector2(1, 1);
            fillRT.offsetMin = new Vector2(6, 6);
            fillRT.offsetMax = new Vector2(-6, -6);
            barFill.type = Image.Type.Filled;
            barFill.fillMethod = Image.FillMethod.Horizontal;
            barFill.fillAmount = 0f;
            barFill.color = new Color(1f, 0.55f, 0.75f);

            TextMeshProUGUI progressLabel = CreateText(barRoot, "ProgressLabel",
                new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero,
                TextAlignmentOptions.Center, 38, "0 / 100");
            TextMeshProUGUI nextLabel = CreateText(panel, "NextLevelText",
                new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 460), new Vector2(820, 50),
                TextAlignmentOptions.Right, 32, "→ Lv.2");

            // Action buttons (bottom row)
            Button petBtn = CreateButton(panel, "PetButton",
                new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-280, 280), new Vector2(220, 200),
                "Pet", new Color(0.95f, 0.55f, 0.75f));
            Button treatBtn = CreateButton(panel, "TreatButton",
                new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 280), new Vector2(220, 200),
                "Treat", new Color(0.95f, 0.75f, 0.30f));
            Button playBtn = CreateButton(panel, "PlayButton",
                new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(280, 280), new Vector2(220, 200),
                "Play", new Color(0.40f, 0.78f, 0.85f));

            // Hint
            TextMeshProUGUI hint = CreateText(panel, "HintText",
                new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 100), new Vector2(900, 80),
                TextAlignmentOptions.Center, 28, "");
            hint.color = new Color(1f, 1f, 1f, 0.85f);

            // Daily-bonus toast (hidden)
            RectTransform toastRoot = MakeRT(panel, "DailyBonusToast",
                new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 720), new Vector2(820, 100));
            Image toastBg = toastRoot.gameObject.AddComponent<Image>();
            toastBg.sprite = TileView.GetWhiteSprite();
            toastBg.color = new Color(0.05f, 0.05f, 0.10f, 0.85f);
            TextMeshProUGUI toastText = CreateText(toastRoot, "Text",
                new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero,
                TextAlignmentOptions.Center, 32, "");
            toastRoot.gameObject.SetActive(false);

            // Build CatPortraitBinding[] from sprite library
            var bindings = new CatBondScreen.CatPortraitBinding[]
            {
                new CatBondScreen.CatPortraitBinding { catId = Constants.CAT_NABI,    fullshot = spriteLib.GetCatPortrait(Constants.CAT_NABI) },
                new CatBondScreen.CatPortraitBinding { catId = Constants.CAT_BELLA,   fullshot = spriteLib.GetCatPortrait(Constants.CAT_BELLA) },
                new CatBondScreen.CatPortraitBinding { catId = Constants.CAT_SAMI,    fullshot = spriteLib.GetCatPortrait(Constants.CAT_SAMI) },
                new CatBondScreen.CatPortraitBinding { catId = Constants.CAT_HODU,    fullshot = spriteLib.GetCatPortrait(Constants.CAT_HODU) },
                new CatBondScreen.CatPortraitBinding { catId = Constants.CAT_GUREUMI, fullshot = spriteLib.GetCatPortrait(Constants.CAT_GUREUMI) },
            };

            // Attach script + inject
            CatBondScreen cb = panel.gameObject.AddComponent<CatBondScreen>();
            InjectField(cb, "backButton", backBtn);
            InjectField(cb, "titleText", titleText);
            InjectField(cb, "cameraButton", cameraBtn);
            InjectField(cb, "helpButton", helpBtn);
            InjectField(cb, "catNameText", catName);
            InjectField(cb, "catLevelText", catLevel);
            InjectField(cb, "rewardAffinityText", rewardAffinity);
            InjectField(cb, "rewardCoinsText", rewardCoins);
            InjectField(cb, "catFullshotImage", catFullshot);
            InjectField(cb, "backgroundImage", bg);
            InjectField(cb, "catPortraits", bindings);
            InjectField(cb, "affinityProgressBar", barFill);
            InjectField(cb, "affinityProgressText", progressLabel);
            InjectField(cb, "affinityNextLevelText", nextLabel);
            InjectField(cb, "petButton", petBtn);
            InjectField(cb, "treatButton", treatBtn);
            InjectField(cb, "playButton", playBtn);
            InjectField(cb, "dailyBonusToast", toastRoot.gameObject);
            InjectField(cb, "dailyBonusToastText", toastText);
            InjectField(cb, "hintText", hint);

            return panel;
        }

        // ===== CAFE RESTORATION (§4-4) =====

        private static CafeRestorationManager.CafeRestorationData LoadCafeRestorationData()
        {
#if UNITY_EDITOR
            const string path = "Assets/_Data/Cafe/CafeRestorationData.json";
            TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            if (asset != null)
            {
                return JsonUtility.FromJson<CafeRestorationManager.CafeRestorationData>(asset.text);
            }
            Debug.LogWarning($"[AppBootstrap] {path} not found. Cafe screen will be empty.");
            return null;
#else
            // Runtime build: would need StreamingAssets/Resources/Addressables.
            return null;
#endif
        }

        private RectTransform BuildCafeRestorationPanel(Transform parent)
        {
            RectTransform panel = NewPanel(parent, "CafePanel");
            panel.gameObject.SetActive(false);

            Image bg = panel.GetComponent<Image>();
            Sprite bgSprite = spriteLib.GetBackground(1, 1);
            if (bgSprite != null) { bg.sprite = bgSprite; bg.color = new Color(1f, 1f, 1f, 0.6f); }

            // Top bar
            Button backBtn = CreateButton(panel, "BackButton",
                new Vector2(0, 1), new Vector2(0, 1), new Vector2(80, -80), new Vector2(120, 120),
                "<", new Color(0.20f, 0.20f, 0.25f, 0.85f));

            TextMeshProUGUI title = CreateText(panel, "Title",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -100), new Vector2(700, 100),
                TextAlignmentOptions.Center, 60, "Cafe Restoration");

            TextMeshProUGUI totalStars = CreateText(panel, "TotalStars",
                new Vector2(1, 1), new Vector2(1, 1), new Vector2(-200, -100), new Vector2(280, 80),
                TextAlignmentOptions.Right, 50, "⭐ 0");
            totalStars.color = new Color(1f, 0.85f, 0.40f);

            // ScrollRect for the 15-card list
            ScrollRect scroll = BuildScrollRect(panel,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 60), new Vector2(900, 1080));
            RectTransform content = scroll.content;

            // Build 15 cards
            var data = LoadCafeRestorationData();
            int totalStages = 0;
            if (data != null && data.cafeAreas != null)
                foreach (var a in data.cafeAreas) if (a.stages != null) totalStages += a.stages.Count;
            if (totalStages == 0) totalStages = 15; // fallback

            var cards = new CafeRestorationScreen.CardWidget[totalStages];
            int cardIdx = 0;
            if (data != null && data.cafeAreas != null)
            {
                for (int z = 0; z < data.cafeAreas.Count; z++)
                {
                    var area = data.cafeAreas[z];
                    if (area.stages == null) continue;
                    for (int s = 0; s < area.stages.Count; s++)
                    {
                        cards[cardIdx] = BuildCafeCard(content, z + 1, s + 1, area.stages[s]);
                        cardIdx++;
                    }
                }
            }

            // Bottom: 3 zone progress rows
            RectTransform zoneRow = MakeRT(panel, "ZoneProgressRow",
                new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 120), new Vector2(900, 200));

            var zoneBars = new Image[3];
            var zoneTexts = new TextMeshProUGUI[3];
            for (int i = 0; i < 3; i++)
            {
                float y = 70 - i * 60;
                TextMeshProUGUI label = CreateText(zoneRow, $"ZoneLabel{i+1}",
                    new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(20, y), new Vector2(220, 50),
                    TextAlignmentOptions.Left, 32, $"{i+1}구역 0/5");
                zoneTexts[i] = label;

                RectTransform barContainer = MakeRT(zoneRow, $"ZoneBarBg{i+1}",
                    new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(0, y),  new Vector2(-280, 28));
                barContainer.anchoredPosition = new Vector2(140, y);
                Image barBg = barContainer.gameObject.AddComponent<Image>();
                barBg.sprite = TileView.GetWhiteSprite();
                barBg.color = new Color(0, 0, 0, 0.5f);

                Image barFill = CreateImageObject(barContainer, "Fill",
                    new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
                RectTransform fillRT = (RectTransform)barFill.transform;
                fillRT.offsetMin = new Vector2(4, 4); fillRT.offsetMax = new Vector2(-4, -4);
                barFill.type = Image.Type.Filled;
                barFill.fillMethod = Image.FillMethod.Horizontal;
                barFill.fillAmount = 0f;
                barFill.color = new Color(0.95f, 0.65f, 0.30f);
                zoneBars[i] = barFill;
            }

            // Completion burst overlay (re-parented to a card on success)
            RectTransform burstRoot = MakeRT(panel, "CompletionBurst",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(160, 160));
            TextMeshProUGUI burstText = CreateText(burstRoot, "BurstText",
                new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero,
                TextAlignmentOptions.Center, 110, "✓");
            burstText.color = new Color(0.95f, 0.85f, 0.30f);
            burstRoot.gameObject.SetActive(false);

            // Attach script + inject
            CafeRestorationScreen screen = panel.gameObject.AddComponent<CafeRestorationScreen>();
            InjectField(screen, "backButton", backBtn);
            InjectField(screen, "titleText", title);
            InjectField(screen, "totalStarsText", totalStars);
            InjectField(screen, "backgroundImage", bg);
            InjectField(screen, "cards", cards);
            InjectField(screen, "zoneProgressBars", zoneBars);
            InjectField(screen, "zoneProgressTexts", zoneTexts);
            InjectField(screen, "completionBurstRoot", burstRoot);
            InjectField(screen, "completionBurstText", burstText);

            return panel;
        }

        private CafeRestorationScreen.CardWidget BuildCafeCard(
            Transform parent, int areaId, int stageIdx,
            CafeRestorationManager.RestorationStage stage)
        {
            // Each card sits in a VerticalLayoutGroup container — width follows parent, height fixed.
            GameObject go = new GameObject($"Card_z{areaId}_s{stageIdx}",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);

            LayoutElement le = go.GetComponent<LayoutElement>();
            le.minHeight = 140;
            le.preferredHeight = 140;
            le.flexibleWidth = 1f;

            Image bg = go.GetComponent<Image>();
            bg.sprite = TileView.GetWhiteSprite();
            bg.color = new Color(0.85f, 0.85f, 0.88f, 0.95f);

            TextMeshProUGUI desc = CreateText(rt, "Desc",
                new Vector2(0, 1), new Vector2(0.6f, 1), new Vector2(0, -10), new Vector2(0, 60),
                TextAlignmentOptions.Left, 38, $"[{areaId}-{stageIdx}] {stage?.description ?? ""}");
            ((RectTransform)desc.transform).offsetMin = new Vector2(24, -70);
            ((RectTransform)desc.transform).offsetMax = new Vector2(-12, -10);
            desc.color = new Color(0.15f, 0.15f, 0.20f);

            TextMeshProUGUI starsText = CreateText(rt, "Stars",
                new Vector2(0, 0), new Vector2(0.5f, 0), new Vector2(0, 10), new Vector2(0, 60),
                TextAlignmentOptions.Left, 36, $"⭐ {stage?.starsRequired ?? 0}");
            ((RectTransform)starsText.transform).offsetMin = new Vector2(24, 12);
            ((RectTransform)starsText.transform).offsetMax = new Vector2(-12, 70);
            starsText.color = new Color(0.85f, 0.55f, 0.20f);

            TextMeshProUGUI stateLabel = CreateText(rt, "State",
                new Vector2(0.5f, 0), new Vector2(0.7f, 0), new Vector2(0, 10), new Vector2(0, 60),
                TextAlignmentOptions.Center, 32, "");
            ((RectTransform)stateLabel.transform).offsetMin = new Vector2(8, 12);
            ((RectTransform)stateLabel.transform).offsetMax = new Vector2(-8, 70);
            stateLabel.color = new Color(0.20f, 0.20f, 0.25f);

            Button restoreBtn = CreateButton(rt, "RestoreButton",
                new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-130, 0), new Vector2(220, 100),
                "복원하기", new Color(0.95f, 0.55f, 0.30f, 1f));

            return new CafeRestorationScreen.CardWidget
            {
                areaId = areaId,
                stageIdx = stageIdx,
                root = rt,
                background = bg,
                descText = desc,
                starsText = starsText,
                stateLabel = stateLabel,
                restoreButton = restoreBtn,
            };
        }

        private ScrollRect BuildScrollRect(Transform parent,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            RectTransform rt = MakeRT(parent, "Scroll", anchorMin, anchorMax, anchoredPos, sizeDelta);
            Image scrollBg = rt.gameObject.AddComponent<Image>();
            scrollBg.sprite = TileView.GetWhiteSprite();
            scrollBg.color = new Color(0, 0, 0, 0.35f);
            scrollBg.raycastTarget = true;

            ScrollRect scroll = rt.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;

            // Viewport (RectMask2D — no graphic required, clips by RectTransform bounds)
            RectTransform viewport = MakeRT(rt, "Viewport",
                new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
            viewport.offsetMin = new Vector2(8, 8); viewport.offsetMax = new Vector2(-8, -8);
            viewport.gameObject.AddComponent<RectMask2D>();

            // Content
            RectTransform content = MakeRT(viewport, "Content",
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 0), new Vector2(0, 0));
            content.pivot = new Vector2(0.5f, 1f);
            VerticalLayoutGroup vlg = content.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(12, 12, 12, 12);
            vlg.spacing = 12;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;   // honor each card's LayoutElement.preferredHeight (140)
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            ContentSizeFitter csf = content.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            scroll.viewport = viewport;
            scroll.content = content;
            return scroll;
        }

        // ===== ARCADE (Phase B mini-game hub) =====

        private RectTransform BuildArcadePanel(Transform parent)
        {
            // Color palette from §5 (한지 크림, 나무톤, 코랄)
            Color paperBg = new Color(0.96f, 0.945f, 0.91f);
            Color woodDark = new Color(0.40f, 0.30f, 0.18f);
            Color cardBg = new Color(0.965f, 0.92f, 0.82f);
            Color cardBorder = new Color(0.545f, 0.451f, 0.333f);
            Color titleBrown = new Color(0.30f, 0.20f, 0.12f);

            RectTransform panel = NewPanel(parent, "ArcadePanel");
            panel.gameObject.SetActive(false);
            Image bg = panel.GetComponent<Image>();
            bg.color = paperBg;

            // Back button
            Button backBtn = CreateButton(panel, "BackButton",
                new Vector2(0, 1), new Vector2(0, 1), new Vector2(80, -80), new Vector2(120, 120),
                "<", new Color(0.20f, 0.20f, 0.25f, 0.85f));

            // Whisker Tales title (top — anchored from top with small offset so it stays visible
            // regardless of game view aspect/height)
            TextMeshProUGUI title = CreateText(panel, "Title",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -120), new Vector2(760, 160),
                TextAlignmentOptions.Center, 96, "Whisker Tales");
            title.fontStyle = FontStyles.Bold;
            title.color = woodDark;

            // "오늘의 미니게임" subtitle banner
            RectTransform banner = MakeRT(panel, "SubtitleBanner",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -260), new Vector2(660, 90));
            Image bannerBg = banner.gameObject.AddComponent<Image>();
            bannerBg.sprite = TileView.GetWhiteSprite();
            bannerBg.color = cardBg;
            bannerBg.raycastTarget = false;
            TextMeshProUGUI subtitle = CreateText(banner, "SubtitleText",
                new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero,
                TextAlignmentOptions.Center, 56, "❀ 오늘의 미니게임 ❀");
            subtitle.color = titleBrown;
            subtitle.fontStyle = FontStyles.Bold;
            subtitle.raycastTarget = false;

            // ScrollRect — fills area below banner to bottom of panel.
            // Stretch anchors with offsetMin/Max so the rect grows/shrinks with canvas height.
            ScrollRect scroll = BuildScrollRect(panel, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            RectTransform scrollRT = (RectTransform)scroll.transform;
            scrollRT.offsetMin = new Vector2(30, 30);
            scrollRT.offsetMax = new Vector2(-30, -320);
            RectTransform content = scroll.content;
            VerticalLayoutGroup vlg = content.GetComponent<VerticalLayoutGroup>();
            if (vlg != null) vlg.spacing = 40;

            // Hide scrollbar — BuildScrollRect doesn't add one, and ScrollRect's inspector-side
            // Scrollbar Visibility property only matters when a Scrollbar reference is set.
            scroll.verticalScrollbar = null;
            scroll.horizontalScrollbar = null;

            // Cards as scroll content children (sized via LayoutElement, positioned by VerticalLayoutGroup)
            var cardData = new (ArcadeScreen.CardKind kind, string title, int catId, bool locked)[]
            {
                (ArcadeScreen.CardKind.HiddenPicture, "고양이\n숨은그림찾기", Constants.CAT_NABI,    false),
                (ArcadeScreen.CardKind.WhackAMole,    "고양이\n두더지잡기",   Constants.CAT_HODU,    false),
                (ArcadeScreen.CardKind.ComingSoon,    "Coming\nSoon",       Constants.CAT_GUREUMI, true),
            };

            var cards = new ArcadeScreen.ArcadeCard[cardData.Length];
            for (int i = 0; i < cardData.Length; i++)
            {
                var d = cardData[i];
                cards[i] = BuildArcadeCard(content, d.kind, d.title, d.catId, d.locked,
                    cardBg, cardBorder, titleBrown);
            }

            // Footer as last scroll-content child — scrolls with the cards
            GameObject footerGo = new GameObject("Footer",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(LayoutElement));
            RectTransform footer = footerGo.GetComponent<RectTransform>();
            footer.SetParent(content, false);
            LayoutElement footerLE = footerGo.GetComponent<LayoutElement>();
            footerLE.preferredHeight = 100;
            footerLE.flexibleWidth = 1f;
            TextMeshProUGUI footerText = CreateText(footer, "FooterText",
                new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero,
                TextAlignmentOptions.Center, 44, "❀ 내일 또 만나요 🐱 ❀");
            footerText.color = woodDark;
            footerText.raycastTarget = false;

            // Attach script + inject
            ArcadeScreen screen = panel.gameObject.AddComponent<ArcadeScreen>();
            InjectField(screen, "backButton", backBtn);
            InjectField(screen, "cards", cards);

            return panel;
        }

        private ArcadeScreen.ArcadeCard BuildArcadeCard(
            Transform parent, ArcadeScreen.CardKind kind, string titleText,
            int catId, bool locked,
            Color cardBg, Color cardBorder, Color titleColor)
        {
            // Card sized via LayoutElement (height 320, full-width via VerticalLayoutGroup childForceExpandWidth)
            GameObject cardGo = new GameObject($"Card_{kind}",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
            RectTransform rt = cardGo.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            // VerticalLayoutGroup will override anchoredPosition + sizeDelta. Just keep sane defaults.
            rt.sizeDelta = new Vector2(0, 320);

            LayoutElement le = cardGo.GetComponent<LayoutElement>();
            le.minHeight = 320;
            le.preferredHeight = 320;
            le.flexibleWidth = 1f;

            Image cardImg = cardGo.GetComponent<Image>();
            cardImg.sprite = TileView.GetWhiteSprite();
            cardImg.color = cardBg;

            Button cardBtn = cardGo.GetComponent<Button>();

            // Decorative border (slightly inset darker rect via Outline)
            Outline outline = cardGo.AddComponent<Outline>();
            outline.effectColor = cardBorder;
            outline.effectDistance = new Vector2(4, -4);

            // Cat sprite (left ~40% of card)
            Image catImg = CreateImageObject(rt, "Cat",
                new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(160, 0), new Vector2(260, 260));
            catImg.preserveAspect = true;
            catImg.raycastTarget = false;
            Sprite catSprite = spriteLib.GetCatPortrait(catId);
            if (catSprite != null) catImg.sprite = catSprite;

            // Subtle scenery backdrop behind cat (small inner rect)
            Image catBackdrop = CreateImageObject(rt, "CatBackdrop",
                new Vector2(0, 0), new Vector2(0, 1), new Vector2(160, 0), new Vector2(300, 0));
            ((RectTransform)catBackdrop.transform).offsetMin = new Vector2(20, 16);
            ((RectTransform)catBackdrop.transform).offsetMax = new Vector2(-360, -16);
            catBackdrop.color = new Color(0.85f, 0.78f, 0.62f, 0.6f);
            catBackdrop.raycastTarget = false;
            ((RectTransform)catBackdrop.transform).SetSiblingIndex(0); // behind catImg

            // Title text (right side)
            TextMeshProUGUI title = CreateText(rt, "CardTitle",
                new Vector2(0.4f, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero,
                TextAlignmentOptions.Center, 62, titleText);
            ((RectTransform)title.transform).offsetMin = new Vector2(20, 50);
            ((RectTransform)title.transform).offsetMax = new Vector2(-40, -50);
            title.color = titleColor;
            title.fontStyle = FontStyles.Bold;
            title.raycastTarget = false;

            // Paw decoration (top-center of card)
            TextMeshProUGUI paw = CreateText(rt, "PawTop",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(120, -30), new Vector2(60, 60),
                TextAlignmentOptions.Center, 44, "🐾");
            paw.color = new Color(0.545f, 0.451f, 0.333f, 0.7f);
            paw.raycastTarget = false;

            // Lock icon + label for Coming Soon
            Image lockIcon = null;
            TextMeshProUGUI lockLabel = null;
            if (locked)
            {
                // Dim card
                cardImg.color = new Color(cardBg.r * 0.85f, cardBg.g * 0.85f, cardBg.b * 0.85f, 1f);

                // Lock icon (rounded rect with 🔒 emoji as fallback)
                lockIcon = CreateImageObject(rt, "LockIcon",
                    new Vector2(0.7f, 0.5f), new Vector2(0.7f, 0.5f), new Vector2(-30, -100), new Vector2(60, 60));
                lockIcon.color = new Color(0.545f, 0.451f, 0.333f);

                // Use TMP text overlay for the lock glyph (Noto fallback should render 🔒)
                TextMeshProUGUI lockGlyph = CreateText(lockIcon.transform, "LockGlyph",
                    new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero,
                    TextAlignmentOptions.Center, 40, "🔒");
                lockGlyph.color = Color.white;
                lockGlyph.raycastTarget = false;

                lockLabel = CreateText(rt, "LockLabel",
                    new Vector2(0.7f, 0.5f), new Vector2(0.7f, 0.5f), new Vector2(80, -100), new Vector2(180, 60),
                    TextAlignmentOptions.Left, 36, "준비중");
                lockLabel.color = new Color(0.545f, 0.451f, 0.333f);
                lockLabel.raycastTarget = false;
            }

            return new ArcadeScreen.ArcadeCard
            {
                kind = kind,
                root = rt,
                button = cardBtn,
                titleText = title,
                lockIcon = lockIcon,
                lockLabel = lockLabel,
            };
        }

        // ===== OPENING SCENARIO (§4-10) =====

        private RectTransform BuildOpeningPanel(Transform parent)
        {
            RectTransform panel = NewPanel(parent, "OpeningPanel");
            panel.gameObject.SetActive(false);

            Image bg = panel.GetComponent<Image>();
            bg.color = new Color(0.10f, 0.12f, 0.18f);

            // Tap area (full-screen, near-invisible — captures background taps to advance)
            GameObject tapGo = new GameObject("TapArea",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            RectTransform tapRT = tapGo.GetComponent<RectTransform>();
            tapRT.SetParent(panel, false);
            tapRT.anchorMin = Vector2.zero; tapRT.anchorMax = Vector2.one;
            tapRT.offsetMin = Vector2.zero; tapRT.offsetMax = Vector2.zero;
            Image tapImg = tapGo.GetComponent<Image>();
            tapImg.sprite = TileView.GetWhiteSprite();
            tapImg.color = new Color(0, 0, 0, 0.001f);
            tapImg.raycastTarget = true;
            Button tapBtn = tapGo.GetComponent<Button>();

            // Letter panel (centered, beige paper, hidden default)
            RectTransform letter = MakeRT(panel, "Letter",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(820, 620));
            Image letterBg = letter.gameObject.AddComponent<Image>();
            letterBg.sprite = TileView.GetWhiteSprite();
            letterBg.color = new Color(0.95f, 0.92f, 0.85f);
            letterBg.raycastTarget = false; // let taps pass to TapArea

            TextMeshProUGUI letterTxt = CreateText(letter, "LetterText",
                new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero,
                TextAlignmentOptions.Center, 50, "");
            ((RectTransform)letterTxt.transform).offsetMin = new Vector2(48, 48);
            ((RectTransform)letterTxt.transform).offsetMax = new Vector2(-48, -48);
            letterTxt.color = new Color(0.20f, 0.18f, 0.15f);
            letterTxt.raycastTarget = false;
            letter.gameObject.SetActive(false);

            // Cat (centered, hidden default)
            Image cat = CreateImageObject(panel, "CatImage",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 100), new Vector2(700, 800));
            cat.preserveAspect = true;
            cat.raycastTarget = false;
            cat.gameObject.SetActive(false);

            // Narration backdrop + text (bottom)
            RectTransform narrBackdrop = MakeRT(panel, "NarrationBackdrop",
                new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 280), new Vector2(960, 360));
            Image narrBg = narrBackdrop.gameObject.AddComponent<Image>();
            narrBg.sprite = TileView.GetWhiteSprite();
            narrBg.color = new Color(0, 0, 0, 0.65f);
            narrBg.raycastTarget = false;

            TextMeshProUGUI narration = CreateText(narrBackdrop, "NarrationText",
                new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero,
                TextAlignmentOptions.Center, 50, "");
            ((RectTransform)narration.transform).offsetMin = new Vector2(40, 30);
            ((RectTransform)narration.transform).offsetMax = new Vector2(-40, -30);
            narration.raycastTarget = false;

            // Skip button (top-right)
            Button skipBtn = CreateButton(panel, "SkipButton",
                new Vector2(1, 1), new Vector2(1, 1), new Vector2(-160, -100), new Vector2(220, 90),
                "Skip", new Color(0.30f, 0.30f, 0.35f, 0.85f));

            // Start button (bottom-center, hidden default)
            Button startBtn = CreateButton(panel, "StartButton",
                new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 100), new Vector2(560, 140),
                "시작하기", new Color(0.95f, 0.55f, 0.30f, 1f));
            startBtn.gameObject.SetActive(false);

            // Audio source for cat meow
            AudioSource sfx = panel.gameObject.AddComponent<AudioSource>();
            sfx.playOnAwake = false;
            sfx.spatialBlend = 0f;

            AudioClip catMeow = LoadAudioClipAt("Assets/Audio/Cats/cat_meow_nabi.wav");
            Sprite catSpr   = spriteLib.GetCatPortrait(Constants.CAT_NABI);
            Sprite cafeBgSpr = spriteLib.GetBackground(1, 1);

            OpeningScenario opening = panel.gameObject.AddComponent<OpeningScenario>();
            InjectField(opening, "backgroundImage", bg);
            InjectField(opening, "letterPanel", letter);
            InjectField(opening, "letterText", letterTxt);
            InjectField(opening, "catImage", cat);
            InjectField(opening, "narrationText", narration);
            InjectField(opening, "tapAreaButton", tapBtn);
            InjectField(opening, "skipButton", skipBtn);
            InjectField(opening, "startButton", startBtn);
            InjectField(opening, "cafeBgSprite", cafeBgSpr);
            InjectField(opening, "catSprite", catSpr);
            InjectField(opening, "catMeowClip", catMeow);
            InjectField(opening, "sfxSource", sfx);

            openingScenario = opening;
            return panel;
        }

        private static AudioClip LoadAudioClipAt(string assetPath)
        {
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
#else
            return null;
#endif
        }

        private void StartOpeningScenario()
        {
            if (panels != null)
            {
                foreach (var kv in panels)
                {
                    if (kv.Value != null) kv.Value.gameObject.SetActive(false);
                }
            }
            if (openingPanel == null) return;

            if (openingScenario != null)
            {
                openingScenario.OnComplete -= HandleOpeningComplete;
                openingScenario.OnComplete += HandleOpeningComplete;
            }
            openingPanel.gameObject.SetActive(true);
        }

        private void HandleOpeningComplete()
        {
            if (openingPanel != null) openingPanel.gameObject.SetActive(false);
            GameManager.Instance?.StartLevel(1);
            ShowPanel(NavigationTarget.Gameplay);
        }

        // ===== Placeholder panel =====

        private RectTransform BuildPlaceholderPanel(Transform parent, string label)
        {
            RectTransform panel = NewPanel(parent, label);
            panel.gameObject.SetActive(false);
            CreateText(panel, "Label",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(900, 200),
                TextAlignmentOptions.Center, 64, label);
            Button back = CreateButton(panel, "BackToTitle",
                new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 200), new Vector2(560, 140),
                "Back", new Color(0.40f, 0.62f, 0.95f));
            back.onClick.AddListener(() => GameManager.Instance?.ReturnToMenu());
            return panel;
        }

        // ===== panel switching =====

        public void ShowPanel(NavigationTarget target)
        {
            if (panels == null) return;
            foreach (var kv in panels)
            {
                if (kv.Value != null) kv.Value.gameObject.SetActive(kv.Key == target);
            }
        }

        // ===== Phase B-3/B-4: Settings (§3-5) =====

        private RectTransform BuildSettingsPanel(Transform parent)
        {
            Color paperBg = new Color(0.96f, 0.945f, 0.91f);
            Color sectionHeaderColor = new Color(0.30f, 0.20f, 0.12f);
            Color rowBg = new Color(1f, 1f, 1f, 0.6f);

            RectTransform panel = NewPanel(parent, "SettingsPanel");
            panel.gameObject.SetActive(false);
            Image bg = panel.GetComponent<Image>();
            bg.color = paperBg;

            // Top bar
            Button backBtn = CreateButton(panel, "BackButton",
                new Vector2(0, 1), new Vector2(0, 1), new Vector2(80, -80), new Vector2(120, 120),
                "<", new Color(0.20f, 0.20f, 0.25f, 0.85f));

            CreateText(panel, "Title",
                new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -100), new Vector2(700, 100),
                TextAlignmentOptions.Center, 60, "설정")
                .color = sectionHeaderColor;

            // ScrollRect for sections
            ScrollRect scroll = BuildScrollRect(panel, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            RectTransform scrollRT = (RectTransform)scroll.transform;
            scrollRT.offsetMin = new Vector2(40, 30);
            scrollRT.offsetMax = new Vector2(-40, -200);
            scroll.verticalScrollbar = null;
            scroll.horizontalScrollbar = null;
            RectTransform content = scroll.content;
            VerticalLayoutGroup vlg = content.GetComponent<VerticalLayoutGroup>();
            if (vlg != null) { vlg.spacing = 16; vlg.padding = new RectOffset(16, 16, 16, 16); }

            // ===== 알림 =====
            BuildSectionHeader(content, "🔔 알림", sectionHeaderColor);
            (Button dailyBtn, TMP_Text dailyLabel) = BuildToggleRow(content, "일일 알림", rowBg);

            // ===== 사운드 =====
            BuildSectionHeader(content, "🔊 사운드", sectionHeaderColor);
            Slider bgmSlider = BuildSliderRow(content, "BGM 볼륨", rowBg);
            Slider sfxSlider = BuildSliderRow(content, "효과음 볼륨", rowBg);
            (Button smNorm, Button smCat, Button smMute) = BuildThreeWayRow(content, "사운드 모드", rowBg, "일반", "고양이", "음소거");

            // ===== 디톡스 =====
            BuildSectionHeader(content, "🌙 디톡스", sectionHeaderColor);
            (Button detoxBtn, TMP_Text detoxLabel) = BuildToggleRow(content, "디톡스 모드", rowBg);

            // ===== 언어 =====
            BuildSectionHeader(content, "🌐 언어", sectionHeaderColor);
            (Button langKo, Button langEn) = BuildTwoWayRow(content, "언어 선택", rowBg, "한국어", "English");

            // ===== 정보 / 외부 =====
            BuildSectionHeader(content, "📜 정보", sectionHeaderColor);
            Button privacyBtn = BuildLinkRow(content, "개인정보처리방침", rowBg);
            Button mailBtn    = BuildLinkRow(content, "📧 문의하기", rowBg);
            Button rateBtn    = BuildLinkRow(content, "⭐ 앱 평가하기", rowBg);
            TMP_Text versionLabel = BuildTextRow(content, "버전", "v1.0.0", rowBg);

            // ===== 피드백 =====
            BuildSectionHeader(content, "💌 피드백", sectionHeaderColor);
            Button[] stars = BuildStarRow(content, rowBg);
            TMP_InputField feedbackInput = BuildInputRow(content, "한 줄 피드백을 남겨주세요...");
            Button feedbackSubmit = BuildSubmitRow(content, "제출");

            // Attach script + inject
            SettingsScreen screen = panel.gameObject.AddComponent<SettingsScreen>();
            InjectField(screen, "backButton", backBtn);
            InjectField(screen, "dailyNotificationToggleButton", dailyBtn);
            InjectField(screen, "dailyNotificationLabel", dailyLabel);
            InjectField(screen, "bgmSlider", bgmSlider);
            InjectField(screen, "sfxSlider", sfxSlider);
            InjectField(screen, "soundModeNormalButton", smNorm);
            InjectField(screen, "soundModeCatButton", smCat);
            InjectField(screen, "soundModeMuteButton", smMute);
            InjectField(screen, "detoxToggleButton", detoxBtn);
            InjectField(screen, "detoxLabel", detoxLabel);
            InjectField(screen, "langKoButton", langKo);
            InjectField(screen, "langEnButton", langEn);
            InjectField(screen, "privacyButton", privacyBtn);
            InjectField(screen, "mailButton", mailBtn);
            InjectField(screen, "rateButton", rateBtn);
            InjectField(screen, "versionLabel", versionLabel);
            InjectField(screen, "starButtons", stars);
            InjectField(screen, "feedbackInput", feedbackInput);
            InjectField(screen, "feedbackSubmitButton", feedbackSubmit);

            settingsScreen = screen;
            return panel;
        }

        // ===== Settings row builders =====

        private void BuildSectionHeader(Transform parent, string label, Color color)
        {
            GameObject go = new GameObject($"Section_{label}", typeof(RectTransform), typeof(LayoutElement));
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            LayoutElement le = go.GetComponent<LayoutElement>();
            le.preferredHeight = 60; le.flexibleWidth = 1f;
            TextMeshProUGUI text = CreateText(rt, "Header",
                new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero,
                TextAlignmentOptions.Left, 44, label);
            text.fontStyle = FontStyles.Bold;
            text.color = color;
            text.raycastTarget = false;
        }

        private RectTransform BuildBaseRow(Transform parent, Color rowBg, float height = 100f)
        {
            GameObject go = new GameObject("Row", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            LayoutElement le = go.GetComponent<LayoutElement>();
            le.preferredHeight = height; le.flexibleWidth = 1f;
            Image img = go.GetComponent<Image>();
            img.sprite = TileView.GetWhiteSprite();
            img.color = rowBg;
            return rt;
        }

        private (Button, TMP_Text) BuildToggleRow(Transform parent, string label, Color rowBg)
        {
            RectTransform row = BuildBaseRow(parent, rowBg);
            CreateText(row, "Label",
                new Vector2(0, 0), new Vector2(0.6f, 1), new Vector2(0, 0), new Vector2(-30, 0),
                TextAlignmentOptions.Left, 36, label).color = new Color(0.20f, 0.20f, 0.25f);
            ((RectTransform)row.GetChild(0)).offsetMin = new Vector2(30, 0);
            ((RectTransform)row.GetChild(0)).offsetMax = new Vector2(-10, 0);

            Button toggle = CreateButton(row, "Toggle",
                new Vector2(0.7f, 0.5f), new Vector2(1, 0.5f), new Vector2(-20, 0), new Vector2(0, 70),
                "", new Color(0.483f, 0.722f, 0.553f));
            ((RectTransform)toggle.transform).offsetMin = new Vector2(0, -35);
            ((RectTransform)toggle.transform).offsetMax = new Vector2(-30, 35);

            // Replace auto Label with our own (so we can grab a ref)
            Transform autoLabel = toggle.transform.Find("Label");
            if (autoLabel != null) UnityEngine.Object.DestroyImmediate(autoLabel.gameObject);
            TMP_Text labelText = CreateText(toggle.transform, "ToggleLabel",
                new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero,
                TextAlignmentOptions.Center, 36, "ON");
            labelText.color = Color.white;
            labelText.raycastTarget = false;

            return (toggle, labelText);
        }

        private Slider BuildSliderRow(Transform parent, string label, Color rowBg)
        {
            RectTransform row = BuildBaseRow(parent, rowBg, 110f);
            CreateText(row, "Label",
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -8), new Vector2(0, 40),
                TextAlignmentOptions.Left, 32, label).color = new Color(0.20f, 0.20f, 0.25f);
            ((RectTransform)row.GetChild(0)).offsetMin = new Vector2(30, -45);
            ((RectTransform)row.GetChild(0)).offsetMax = new Vector2(-10, -5);

            Slider s = CreateSlider(row, "Slider",
                new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 30), new Vector2(0, 36));
            RectTransform srt = (RectTransform)s.transform;
            srt.anchorMin = new Vector2(0, 0); srt.anchorMax = new Vector2(1, 0);
            srt.offsetMin = new Vector2(30, 18); srt.offsetMax = new Vector2(-30, 54);
            s.minValue = 0; s.maxValue = 1; s.interactable = true;
            return s;
        }

        private (Button, Button, Button) BuildThreeWayRow(Transform parent, string label, Color rowBg, string a, string b, string c)
        {
            RectTransform row = BuildBaseRow(parent, rowBg, 130f);
            CreateText(row, "Label",
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -8), new Vector2(0, 40),
                TextAlignmentOptions.Left, 32, label).color = new Color(0.20f, 0.20f, 0.25f);
            ((RectTransform)row.GetChild(0)).offsetMin = new Vector2(30, -45);
            ((RectTransform)row.GetChild(0)).offsetMax = new Vector2(-10, -5);

            Button bA = CreateButton(row, "OptA",
                new Vector2(0, 0), new Vector2(0.333f, 0), new Vector2(0, 30), new Vector2(0, 60),
                a, new Color(0.55f, 0.55f, 0.60f));
            ((RectTransform)bA.transform).offsetMin = new Vector2(30, 15);
            ((RectTransform)bA.transform).offsetMax = new Vector2(-5, 75);
            Button bB = CreateButton(row, "OptB",
                new Vector2(0.333f, 0), new Vector2(0.667f, 0), new Vector2(0, 30), new Vector2(0, 60),
                b, new Color(0.55f, 0.55f, 0.60f));
            ((RectTransform)bB.transform).offsetMin = new Vector2(5, 15);
            ((RectTransform)bB.transform).offsetMax = new Vector2(-5, 75);
            Button bC = CreateButton(row, "OptC",
                new Vector2(0.667f, 0), new Vector2(1, 0), new Vector2(0, 30), new Vector2(0, 60),
                c, new Color(0.55f, 0.55f, 0.60f));
            ((RectTransform)bC.transform).offsetMin = new Vector2(5, 15);
            ((RectTransform)bC.transform).offsetMax = new Vector2(-30, 75);
            return (bA, bB, bC);
        }

        private (Button, Button) BuildTwoWayRow(Transform parent, string label, Color rowBg, string a, string b)
        {
            RectTransform row = BuildBaseRow(parent, rowBg, 130f);
            CreateText(row, "Label",
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -8), new Vector2(0, 40),
                TextAlignmentOptions.Left, 32, label).color = new Color(0.20f, 0.20f, 0.25f);
            ((RectTransform)row.GetChild(0)).offsetMin = new Vector2(30, -45);
            ((RectTransform)row.GetChild(0)).offsetMax = new Vector2(-10, -5);

            Button bA = CreateButton(row, "OptA",
                new Vector2(0, 0), new Vector2(0.5f, 0), new Vector2(0, 30), new Vector2(0, 60),
                a, new Color(0.55f, 0.55f, 0.60f));
            ((RectTransform)bA.transform).offsetMin = new Vector2(30, 15);
            ((RectTransform)bA.transform).offsetMax = new Vector2(-10, 75);
            Button bB = CreateButton(row, "OptB",
                new Vector2(0.5f, 0), new Vector2(1, 0), new Vector2(0, 30), new Vector2(0, 60),
                b, new Color(0.55f, 0.55f, 0.60f));
            ((RectTransform)bB.transform).offsetMin = new Vector2(10, 15);
            ((RectTransform)bB.transform).offsetMax = new Vector2(-30, 75);
            return (bA, bB);
        }

        private Button BuildLinkRow(Transform parent, string label, Color rowBg)
        {
            RectTransform row = BuildBaseRow(parent, rowBg);
            Button btn = CreateButton(row, "Link",
                new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero,
                label, new Color(0.40f, 0.62f, 0.95f, 0.85f));
            ((RectTransform)btn.transform).offsetMin = new Vector2(20, 15);
            ((RectTransform)btn.transform).offsetMax = new Vector2(-20, -15);
            return btn;
        }

        private TMP_Text BuildTextRow(Transform parent, string label, string value, Color rowBg)
        {
            RectTransform row = BuildBaseRow(parent, rowBg, 80f);
            CreateText(row, "Label",
                new Vector2(0, 0), new Vector2(0.5f, 1), Vector2.zero, Vector2.zero,
                TextAlignmentOptions.Left, 32, label).color = new Color(0.20f, 0.20f, 0.25f);
            ((RectTransform)row.GetChild(0)).offsetMin = new Vector2(30, 0);
            ((RectTransform)row.GetChild(0)).offsetMax = new Vector2(-10, 0);
            TMP_Text valueText = CreateText(row, "Value",
                new Vector2(0.5f, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero,
                TextAlignmentOptions.Right, 32, value);
            valueText.color = new Color(0.50f, 0.50f, 0.55f);
            ((RectTransform)valueText.transform).offsetMin = new Vector2(10, 0);
            ((RectTransform)valueText.transform).offsetMax = new Vector2(-30, 0);
            return valueText;
        }

        private Button[] BuildStarRow(Transform parent, Color rowBg)
        {
            RectTransform row = BuildBaseRow(parent, rowBg, 130f);
            Button[] stars = new Button[5];
            for (int i = 0; i < 5; i++)
            {
                float xMin = i / 5f, xMax = (i + 1) / 5f;
                Button b = CreateButton(row, $"Star{i+1}",
                    new Vector2(xMin, 0), new Vector2(xMax, 1), Vector2.zero, Vector2.zero,
                    "★", new Color(0.55f, 0.55f, 0.60f));
                ((RectTransform)b.transform).offsetMin = new Vector2(10, 15);
                ((RectTransform)b.transform).offsetMax = new Vector2(-10, -15);
                Image img = b.GetComponent<Image>();
                if (img != null) img.color = new Color(0.55f, 0.55f, 0.60f);
                stars[i] = b;
            }
            return stars;
        }

        private TMP_InputField BuildInputRow(Transform parent, string placeholder)
        {
            GameObject go = new GameObject("Input", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            LayoutElement le = go.GetComponent<LayoutElement>();
            le.preferredHeight = 100; le.flexibleWidth = 1f;
            Image bg = go.GetComponent<Image>();
            bg.sprite = TileView.GetWhiteSprite();
            bg.color = new Color(1f, 1f, 1f, 0.85f);

            TMP_InputField input = go.AddComponent<TMP_InputField>();

            // Text Area (viewport)
            RectTransform textArea = MakeRT(rt, "TextArea",
                new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
            textArea.offsetMin = new Vector2(20, 10); textArea.offsetMax = new Vector2(-20, -10);

            TextMeshProUGUI placeholderText = CreateText(textArea, "Placeholder",
                new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero,
                TextAlignmentOptions.Left, 32, placeholder);
            placeholderText.color = new Color(0.50f, 0.50f, 0.55f);
            placeholderText.raycastTarget = false;

            TextMeshProUGUI textComp = CreateText(textArea, "Text",
                new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero,
                TextAlignmentOptions.Left, 32, "");
            textComp.color = new Color(0.20f, 0.20f, 0.25f);
            textComp.raycastTarget = false;

            input.textViewport = textArea;
            input.textComponent = textComp;
            input.placeholder = placeholderText;
            input.characterLimit = 200;
            return input;
        }

        private Button BuildSubmitRow(Transform parent, string label)
        {
            RectTransform row = BuildBaseRow(parent, new Color(0, 0, 0, 0), 100f);
            Button b = CreateButton(row, "Submit",
                new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero,
                label, new Color(0.95f, 0.55f, 0.30f, 1f));
            ((RectTransform)b.transform).offsetMin = new Vector2(40, 15);
            ((RectTransform)b.transform).offsetMax = new Vector2(-40, -15);
            return b;
        }

        // ===== Phase B-3: Photo Studio (§3-4) =====

        private RectTransform BuildPhotoStudioPanel(Transform parent)
        {
            RectTransform panel = NewPanel(parent, "PhotoStudioPanel");
            panel.gameObject.SetActive(false);

            // Top bar
            Button backBtn = CreateButton(panel, "BackButton",
                new Vector2(0, 1), new Vector2(0, 1), new Vector2(80, -80), new Vector2(120, 120),
                "<", new Color(0.20f, 0.20f, 0.25f, 0.85f));

            TextMeshProUGUI title = CreateText(panel, "Title",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -100), new Vector2(700, 100),
                TextAlignmentOptions.Center, 60, "포토 스튜디오");
            title.color = new Color(0.30f, 0.20f, 0.12f);
            title.fontStyle = FontStyles.Bold;

            // Preview area: bg full coverage in upper region, cat sprite centered
            Image bgImg = CreateImageObject(panel, "PreviewBackground",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -700), new Vector2(960, 1100));
            bgImg.preserveAspect = false;
            bgImg.raycastTarget = false;

            Image catImg = CreateImageObject(panel, "PreviewCat",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -750), new Vector2(720, 900));
            catImg.preserveAspect = true;
            catImg.raycastTarget = false;

            // 5 background thumbnails row (above the pose row)
            // 5종 — 한옥 실내 / 마당 / 뒷정원 / 벚꽃(임시) / 눈(임시)
            (string label, int zone, int stage)[] bgChoices = new (string, int, int)[]
            {
                ("실내",  2, 5),
                ("마당",  1, 5),
                ("뒷정원", 3, 5),
                ("벚꽃",  1, 3), // 임시 — 정식 벚꽃 배경은 v1.1
                ("눈",    3, 1), // 임시 — 정식 눈 배경은 v1.1
            };

            var bgOptions = new PhotoStudioController.BackgroundOption[bgChoices.Length];
            int totalW = bgChoices.Length * 130 + (bgChoices.Length - 1) * 16;
            int rowStartX = -totalW / 2 + 65;
            for (int i = 0; i < bgChoices.Length; i++)
            {
                int posX = rowStartX + i * (130 + 16);
                Sprite sp = spriteLib.GetBackground(bgChoices[i].zone, bgChoices[i].stage);

                Button thumb = CreateButton(panel, $"BgThumb_{i}",
                    new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(posX, 480), new Vector2(130, 100),
                    "", new Color(1f, 1f, 1f, 1f));
                Image thumbImg = thumb.GetComponent<Image>();
                if (sp != null) { thumbImg.sprite = sp; thumbImg.color = Color.white; }
                else            { thumbImg.color = new Color(0.55f, 0.55f, 0.55f); }
                Outline ol = thumb.gameObject.AddComponent<Outline>();
                ol.effectColor = new Color(0.20f, 0.18f, 0.15f);
                ol.effectDistance = new Vector2(2, -2);
                // remove the auto-created button label
                Transform labelChild = thumb.transform.Find("Label");
                if (labelChild != null) UnityEngine.Object.DestroyImmediate(labelChild.gameObject);

                TextMeshProUGUI subLabel = CreateText(panel, $"BgLabel_{i}",
                    new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(posX, 410), new Vector2(140, 36),
                    TextAlignmentOptions.Center, 28, bgChoices[i].label);
                subLabel.color = new Color(0.20f, 0.18f, 0.15f);
                subLabel.raycastTarget = false;

                bgOptions[i] = new PhotoStudioController.BackgroundOption
                {
                    label = bgChoices[i].label,
                    sprite = sp,
                    thumbnailButton = thumb,
                };
            }

            // Pose toggle row
            Button frontBtn = CreateButton(panel, "PoseFront",
                new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-150, 320), new Vector2(280, 90),
                "정면", new Color(0.91f, 0.659f, 0.486f, 1f));
            Button playBtn = CreateButton(panel, "PosePlay",
                new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(150, 320), new Vector2(280, 90),
                "놀아주기", new Color(0.483f, 0.722f, 0.553f, 1f));

            // Action row (capture + share) — captured during photo so we can hide it
            RectTransform actionRow = MakeRT(panel, "ActionRow",
                new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 150), new Vector2(900, 130));
            Button captureBtn = CreateButton(actionRow, "CaptureButton",
                new Vector2(0, 0), new Vector2(0.5f, 1), Vector2.zero, Vector2.zero,
                "📸 찍기", new Color(0.95f, 0.55f, 0.30f, 1f));
            ((RectTransform)captureBtn.transform).offsetMin = new Vector2(0, 0);
            ((RectTransform)captureBtn.transform).offsetMax = new Vector2(-20, 0);

            Button shareBtn = CreateButton(actionRow, "ShareButton",
                new Vector2(0.5f, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero,
                "📤 공유", new Color(0.40f, 0.62f, 0.95f, 1f));
            ((RectTransform)shareBtn.transform).offsetMin = new Vector2(20, 0);
            ((RectTransform)shareBtn.transform).offsetMax = new Vector2(0, 0);

            // Build front/play sprite arrays indexed by catId (1..5)
            Sprite[] frontArr = new Sprite[6];
            Sprite[] playArr  = new Sprite[6];
            int[] catIds = { Constants.CAT_NABI, Constants.CAT_BELLA, Constants.CAT_SAMI, Constants.CAT_HODU, Constants.CAT_GUREUMI };
            foreach (int id in catIds)
            {
                if (id < frontArr.Length)
                {
                    frontArr[id] = spriteLib.GetCatPortrait(id);
                    playArr[id]  = spriteLib.GetCatPlayPortrait(id);
                }
            }

            // Attach script + inject
            PhotoStudioController ctrl = panel.gameObject.AddComponent<PhotoStudioController>();
            InjectField(ctrl, "backButton", backBtn);
            InjectField(ctrl, "titleText", title);
            InjectField(ctrl, "backgroundImage", bgImg);
            InjectField(ctrl, "catImage", catImg);
            InjectField(ctrl, "backgroundOptions", bgOptions);
            InjectField(ctrl, "poseFrontButton", frontBtn);
            InjectField(ctrl, "posePlayButton", playBtn);
            InjectField(ctrl, "captureButton", captureBtn);
            InjectField(ctrl, "shareButton", shareBtn);
            InjectField(ctrl, "actionRow", actionRow);
            InjectField(ctrl, "frontSpritesByCatId", frontArr);
            InjectField(ctrl, "playSpritesByCatId", playArr);

            photoStudioController = ctrl;
            return panel;
        }

        // ===== Phase B-3: Meditation Garden (§3-2) =====

        private RectTransform BuildMeditationGardenPanel(Transform parent)
        {
            RectTransform panel = NewPanel(parent, "MeditationGardenPanel");
            panel.gameObject.SetActive(false);

            Image bg = panel.GetComponent<Image>();
            Sprite zone3 = spriteLib.GetBackground(3, 5);
            if (zone3 != null) { bg.sprite = zone3; bg.color = new Color(1f, 1f, 1f, 0.85f); }
            else                bg.color = new Color(0.55f, 0.65f, 0.55f);

            // Top bar
            Button backBtn = CreateButton(panel, "BackButton",
                new Vector2(0, 1), new Vector2(0, 1), new Vector2(80, -80), new Vector2(120, 120),
                "<", new Color(0.20f, 0.20f, 0.25f, 0.85f));

            TextMeshProUGUI title = CreateText(panel, "Title",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -100), new Vector2(700, 100),
                TextAlignmentOptions.Center, 60, "명상 정원");
            title.color = new Color(0.30f, 0.20f, 0.12f);
            title.fontStyle = FontStyles.Bold;

            TextMeshProUGUI peacePoint = CreateText(panel, "PeacePointText",
                new Vector2(1, 1), new Vector2(1, 1), new Vector2(-200, -100), new Vector2(320, 80),
                TextAlignmentOptions.Right, 48, "⭐ 0");
            peacePoint.color = new Color(0.30f, 0.30f, 0.50f);
            peacePoint.fontStyle = FontStyles.Bold;

            // Sand area (RawImage centered, ~900x1100)
            RectTransform sandRoot = MakeRT(panel, "SandArea",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 30), new Vector2(900, 1100));
            // Frame border
            Image sandFrame = sandRoot.gameObject.AddComponent<Image>();
            sandFrame.sprite = TileView.GetWhiteSprite();
            sandFrame.color = new Color(0.42f, 0.32f, 0.20f);
            sandFrame.raycastTarget = false;

            GameObject sandGo = new GameObject("Sand", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
            RectTransform sandRT = sandGo.GetComponent<RectTransform>();
            sandRT.SetParent(sandRoot, false);
            sandRT.anchorMin = Vector2.zero; sandRT.anchorMax = Vector2.one;
            sandRT.offsetMin = new Vector2(20, 20); sandRT.offsetMax = new Vector2(-20, -20);
            RawImage sandImage = sandGo.GetComponent<RawImage>();
            sandImage.color = Color.white;
            sandImage.raycastTarget = true;

            // Bottom buttons
            Button resetBtn = CreateButton(panel, "ResetButton",
                new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-200, 160), new Vector2(360, 130),
                "🌬 정원 초기화", new Color(0.55f, 0.55f, 0.60f, 1f));

            Button endBtn = CreateButton(panel, "EndButton",
                new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(200, 160), new Vector2(360, 130),
                "⏸ 명상 종료", new Color(0.91f, 0.659f, 0.486f, 1f));

            // Wind chime BGM placeholder — clip not assigned (자산 도착 시 채울 자리)
            AudioSource bgm = panel.gameObject.AddComponent<AudioSource>();
            bgm.playOnAwake = false;
            bgm.loop = true;
            bgm.spatialBlend = 0f;

            MeditationGardenController ctrl = panel.gameObject.AddComponent<MeditationGardenController>();
            InjectField(ctrl, "backgroundImage", bg);
            InjectField(ctrl, "sandImage", sandImage);
            InjectField(ctrl, "peacePointText", peacePoint);
            InjectField(ctrl, "titleText", title);
            InjectField(ctrl, "backButton", backBtn);
            InjectField(ctrl, "resetButton", resetBtn);
            InjectField(ctrl, "endButton", endBtn);
            InjectField(ctrl, "bgmSource", bgm);

            meditationController = ctrl;
            return panel;
        }

        // ===== Phase B-2: Detox modal + Sleep mode =====

        /// <summary>
        /// LevelClearPanel.HandleContinue가 호출. 20% 확률로 디톡스 모달 → 확인=메뉴, 쉬어갈게요=수면 모드.
        /// 미당첨/디톡스 OFF면 즉시 메인으로.
        /// </summary>
        public void RequestPostLevelFlow()
        {
            if (detoxModal != null) detoxModal.NotifyLevelCleared();
            bool shown = detoxModal != null && detoxModal.TryShow(
                onConfirmAction: () => GameManager.Instance?.ReturnToMenu(),
                onSleepAction: ShowSleepMode);
            if (!shown) GameManager.Instance?.ReturnToMenu();
        }

        public void ShowSleepMode()
        {
            if (sleepModeScreen == null) return;
            sleepModeScreen.gameObject.SetActive(true);
        }

        private DetoxMessageModal BuildDetoxMessageModal(Transform parent)
        {
            // Full-screen overlay panel (hidden default). Center card with message + 2 buttons.
            RectTransform panel = NewPanel(parent, "DetoxMessageOverlay");
            panel.gameObject.SetActive(false);

            // Dim backdrop (한지 크림 60% alpha)
            Image bg = panel.GetComponent<Image>();
            bg.color = new Color(0.96f, 0.945f, 0.91f, 0.60f);
            bg.raycastTarget = true;

            // Center card
            RectTransform card = MakeRT(panel, "Card",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(880, 540));
            Image cardBg = card.gameObject.AddComponent<Image>();
            cardBg.sprite = TileView.GetWhiteSprite();
            cardBg.color = new Color(0.965f, 0.92f, 0.82f, 1f);
            cardBg.raycastTarget = true;

            // Top divider (small horizontal line)
            RectTransform divider = MakeRT(card, "Divider",
                new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -50), new Vector2(640, 6));
            Image divImg = divider.gameObject.AddComponent<Image>();
            divImg.sprite = TileView.GetWhiteSprite();
            divImg.color = new Color(0.545f, 0.451f, 0.333f, 0.7f);
            divImg.raycastTarget = false;

            TextMeshProUGUI msg = CreateText(card, "MessageText",
                new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero,
                TextAlignmentOptions.Center, 50, "");
            ((RectTransform)msg.transform).offsetMin = new Vector2(60, 180);
            ((RectTransform)msg.transform).offsetMax = new Vector2(-60, -100);
            msg.color = new Color(0.20f, 0.18f, 0.15f);
            msg.fontStyle = FontStyles.Bold;
            msg.raycastTarget = false;

            // Buttons row
            Button confirmBtn = CreateButton(card, "ConfirmButton",
                new Vector2(0, 0), new Vector2(0.5f, 0), new Vector2(0, 80), new Vector2(0, 130),
                "확인", new Color(0.55f, 0.55f, 0.60f, 1f));
            ((RectTransform)confirmBtn.transform).offsetMin = new Vector2(60, 30);
            ((RectTransform)confirmBtn.transform).offsetMax = new Vector2(-20, 160);

            Button sleepBtn = CreateButton(card, "SleepButton",
                new Vector2(0.5f, 0), new Vector2(1, 0), new Vector2(0, 80), new Vector2(0, 130),
                "쉬어갈게요", new Color(0.91f, 0.659f, 0.486f, 1f));
            ((RectTransform)sleepBtn.transform).offsetMin = new Vector2(20, 30);
            ((RectTransform)sleepBtn.transform).offsetMax = new Vector2(-60, 160);

            DetoxMessageModal modal = panel.gameObject.AddComponent<DetoxMessageModal>();
            InjectField(modal, "root", panel.gameObject);
            InjectField(modal, "messageText", msg);
            InjectField(modal, "confirmButton", confirmBtn);
            InjectField(modal, "sleepButton", sleepBtn);

            return modal;
        }

        private SleepModeScreen BuildSleepModeScreen(Transform parent)
        {
            RectTransform panel = NewPanel(parent, "SleepModeOverlay");
            panel.gameObject.SetActive(false);

            Image bg = panel.GetComponent<Image>();
            // Use zone2 stage5 with 50% black overlay for night feel (per packet §3-1-2 임시 처리).
            Sprite night = spriteLib.GetBackground(2, 5);
            if (night != null) { bg.sprite = night; bg.color = new Color(0.18f, 0.18f, 0.22f, 1f); }
            else { bg.color = new Color(0.10f, 0.12f, 0.18f, 1f); }
            bg.raycastTarget = true;

            // Tap-to-wake (full-screen, near-invisible) — added FIRST so it's behind cat/TV/labels.
            GameObject tapGo = new GameObject("TapToWake",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            RectTransform tapRT = tapGo.GetComponent<RectTransform>();
            tapRT.SetParent(panel, false);
            tapRT.anchorMin = Vector2.zero; tapRT.anchorMax = Vector2.one;
            tapRT.offsetMin = Vector2.zero; tapRT.offsetMax = Vector2.zero;
            Image tapImg = tapGo.GetComponent<Image>();
            tapImg.sprite = TileView.GetWhiteSprite();
            tapImg.color = new Color(0, 0, 0, 0.001f);
            tapImg.raycastTarget = true;
            Button tapBtn = tapGo.GetComponent<Button>();

            // TV area (dummy "Nyang TV" panel)
            RectTransform tv = MakeRT(panel, "TVArea",
                new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -360), new Vector2(720, 420));
            Image tvBg = tv.gameObject.AddComponent<Image>();
            tvBg.sprite = TileView.GetWhiteSprite();
            tvBg.color = new Color(0.04f, 0.04f, 0.06f, 1f);
            tvBg.raycastTarget = false;
            TextMeshProUGUI tvLabel = CreateText(tv, "TVLabel",
                new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero,
                TextAlignmentOptions.Center, 56, "📺 Nyang TV");
            tvLabel.color = new Color(0.85f, 0.85f, 0.95f, 0.85f);
            tvLabel.raycastTarget = false;

            // Sleeping cat image (use first unlocked cat — fallback nabi). Sleep PNG isn't in
            // SpriteLibrary yet, so for now use front portrait scaled down.
            Image cat = CreateImageObject(panel, "SleepingCat",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -100), new Vector2(560, 560));
            cat.preserveAspect = true;
            cat.raycastTarget = false;
            Sprite catSpr = spriteLib.GetCatPortrait(Constants.CAT_NABI);
            if (catSpr != null) cat.sprite = catSpr;

            // Purring indicator (bottom)
            TextMeshProUGUI purrInd = CreateText(panel, "PurringIndicator",
                new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 200), new Vector2(700, 80),
                TextAlignmentOptions.Center, 42, "💤 골골송 재생 중");
            purrInd.color = new Color(0.95f, 0.92f, 0.85f, 0.85f);
            purrInd.raycastTarget = false;

            // Audio source (loop) for purring
            AudioSource purring = panel.gameObject.AddComponent<AudioSource>();
            purring.playOnAwake = false;
            purring.loop = true;
            purring.spatialBlend = 0f;
#if UNITY_EDITOR
            purring.clip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Cats/cat_purring.wav");
#endif

            // Reward modal (child of panel, hidden default)
            RectTransform rewardCard = MakeRT(panel, "RewardModal",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(820, 600));
            Image rewardBg = rewardCard.gameObject.AddComponent<Image>();
            rewardBg.sprite = TileView.GetWhiteSprite();
            rewardBg.color = new Color(0.96f, 0.945f, 0.91f, 1f);
            rewardBg.raycastTarget = true;

            TextMeshProUGUI rewardTxt = CreateText(rewardCard, "RewardText",
                new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero,
                TextAlignmentOptions.Center, 44, "");
            ((RectTransform)rewardTxt.transform).offsetMin = new Vector2(40, 180);
            ((RectTransform)rewardTxt.transform).offsetMax = new Vector2(-40, -60);
            rewardTxt.color = new Color(0.20f, 0.18f, 0.15f);
            rewardTxt.raycastTarget = false;

            Button rewardConfirm = CreateButton(rewardCard, "RewardConfirm",
                new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 80), new Vector2(420, 130),
                "확인", new Color(0.91f, 0.659f, 0.486f, 1f));

            rewardCard.gameObject.SetActive(false);

            SleepModeScreen screen = panel.gameObject.AddComponent<SleepModeScreen>();
            InjectField(screen, "backgroundImage", bg);
            InjectField(screen, "catImage", cat);
            InjectField(screen, "tvArea", tv);
            InjectField(screen, "tvLabel", tvLabel);
            InjectField(screen, "purringIndicator", purrInd);
            InjectField(screen, "tapToWakeButton", tapBtn);
            InjectField(screen, "purringSource", purring);
            InjectField(screen, "rewardModal", rewardCard.gameObject);
            InjectField(screen, "rewardText", rewardTxt);
            InjectField(screen, "rewardConfirmButton", rewardConfirm);

            return screen;
        }

        // ===== Phase B-1: Sound / Loading / Currency =====

        /// <summary>
        /// §4-3 매핑대로 SoundManager에 cat 클립을 등록.
        /// 일반 모드 클립은 v1.0에서 자리만 (null 등록 안 함, Play시 dict 미스로 자연스럽게 무음).
        /// 에디터 전용 AssetDatabase 로드 — APK 빌드는 후속 (Resources/StreamingAssets 정리 필요).
        /// </summary>
        private static void RegisterSoundManagerClips()
        {
            SoundManager sm = SoundManager.Instance;
            if (sm == null) return;

#if UNITY_EDITOR
            AudioClip nabi    = LoadAudioClipAt("Assets/Audio/Cats/cat_meow_nabi.wav");
            AudioClip hodu    = LoadAudioClipAt("Assets/Audio/Cats/cat_meow_hodu.wav");
            AudioClip bella   = LoadAudioClipAt("Assets/Audio/Cats/cat_meow_bella.wav");
            AudioClip sami    = LoadAudioClipAt("Assets/Audio/Cats/cat_meow_sami.wav");
            AudioClip gureumi = LoadAudioClipAt("Assets/Audio/Cats/cat_meow_gureumi.wav");
            AudioClip purring = LoadAudioClipAt("Assets/Audio/Cats/cat_purring.wav");

            sm.RegisterCatClip(SfxKey.Click,      nabi);     // 짧은 "냥"
            sm.RegisterCatClip(SfxKey.Match,      hodu);     // "뿅" 톤
            sm.RegisterCatClip(SfxKey.Combo,      bella);    // 흥분한 "냐~!"
            sm.RegisterCatClip(SfxKey.LevelClear, sami);     // 길고 기쁜 "므야아~"
            sm.RegisterCatClip(SfxKey.Coin,       bella);    // 방울 톤 (벨라 목방울)
            sm.RegisterCatClip(SfxKey.Fail,       gureumi);  // 작고 실망한 "음냥..."
            sm.RegisterCatClip(SfxKey.Pet,        purring);  // 골골송

            int registered = 0;
            foreach (var c in new[] { nabi, hodu, bella, sami, gureumi, purring }) if (c != null) registered++;
            Debug.Log($"[AppBootstrap] SoundManager cat clips registered: {registered}/6");
#else
            Debug.LogWarning("[AppBootstrap] SoundManager cat clips: editor-only loader — APK build needs StreamingAssets/Resources.");
#endif
        }

        private LoadingScreen BuildLoadingScreen(Transform parent)
        {
            RectTransform panel = NewPanel(parent, "LoadingScreenOverlay");
            panel.gameObject.SetActive(false);
            Image bg = panel.GetComponent<Image>();
            bg.color = new Color(0.96f, 0.945f, 0.91f, 0.95f); // 한지 크림 95% — almost solid

            // Rotating cat face (Image) at center
            Image catFace = CreateImageObject(panel, "CatFace",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 80), new Vector2(280, 280));
            catFace.preserveAspect = true;
            catFace.raycastTarget = false;

            // Message text below
            TextMeshProUGUI msg = CreateText(panel, "Message",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -160), new Vector2(900, 100),
                TextAlignmentOptions.Center, 44, "냥이가 준비 중이에요...");
            msg.color = new Color(0.30f, 0.20f, 0.12f);
            msg.raycastTarget = false;

            // Purring AudioSource (loop)
            AudioSource purring = panel.gameObject.AddComponent<AudioSource>();
            purring.playOnAwake = false;
            purring.loop = true;
            purring.spatialBlend = 0f;
#if UNITY_EDITOR
            purring.clip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Cats/cat_purring.wav");
#endif

            // Loading entries — 5 cats, messages from §5-3
            var entries = new LoadingScreen.CatLoadingEntry[]
            {
                new LoadingScreen.CatLoadingEntry { catId = Constants.CAT_BELLA,   face = spriteLib.GetCatPortrait(Constants.CAT_BELLA),   messageKo = "우아하게 준비 중이에요~ 🎀", messageEn = "Preparing elegantly~" },
                new LoadingScreen.CatLoadingEntry { catId = Constants.CAT_NABI,    face = spriteLib.GetCatPortrait(Constants.CAT_NABI),    messageKo = "신나게 달려오고 있어요! 🍀", messageEn = "Running over excitedly!" },
                new LoadingScreen.CatLoadingEntry { catId = Constants.CAT_HODU,    face = spriteLib.GetCatPortrait(Constants.CAT_HODU),    messageKo = "든든하게 준비했습니다 🧣", messageEn = "Got it covered." },
                new LoadingScreen.CatLoadingEntry { catId = Constants.CAT_GUREUMI, face = spriteLib.GetCatPortrait(Constants.CAT_GUREUMI), messageKo = "살금살금 오고 있어요... 💙", messageEn = "Sneaking quietly..." },
                new LoadingScreen.CatLoadingEntry { catId = Constants.CAT_SAMI,    face = spriteLib.GetCatPortrait(Constants.CAT_SAMI),    messageKo = "조용히 기다려주세요 🔔", messageEn = "Please wait quietly." },
            };

            LoadingScreen ls = panel.gameObject.AddComponent<LoadingScreen>();
            InjectField(ls, "catFaceImage", catFace);
            InjectField(ls, "messageText", msg);
            InjectField(ls, "purringSource", purring);
            InjectField(ls, "entries", entries);

            return ls;
        }

        private void BindNyangiHeartIndicator()
        {
            if (titleNyangiHeartText == null) return;
            // CurrencyManager may already be created (EnsureCoreManagers ran first); subscribe + sync.
            CurrencyManager cm = CurrencyManager.Instance;
            if (cm != null)
            {
                titleNyangiHeartText.text = $"💝 {cm.NyangiHeart}";
                cm.OnNyangiHeartChanged -= HandleNyangiHeartChanged;
                cm.OnNyangiHeartChanged += HandleNyangiHeartChanged;
            }
        }

        private void HandleNyangiHeartChanged(int newValue)
        {
            if (titleNyangiHeartText != null)
                titleNyangiHeartText.text = $"💝 {newValue}";
        }

        // ===== widget helpers =====

        private static RectTransform MakeRT(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;
            return rt;
        }

        private static TextMeshProUGUI CreateText(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta,
            TextAlignmentOptions align, float fontSize, string defaultText)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;

            TextMeshProUGUI t = go.GetComponent<TextMeshProUGUI>();
            t.text = defaultText;
            t.fontSize = fontSize;
            t.color = Color.white;
            t.alignment = align;
            t.enableWordWrapping = false;
            return t;
        }

        private static Button CreateButton(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta,
            string label, Color color)
        {
            GameObject go = new GameObject(name,
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;

            Image img = go.GetComponent<Image>();
            img.sprite = TileView.GetWhiteSprite();
            img.color = color;

            Button btn = go.GetComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(1.05f, 1.05f, 1.05f, 1f);
            cb.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            btn.colors = cb;

            // child label
            CreateText(go.transform, "Label",
                new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero,
                TextAlignmentOptions.Center, Mathf.Max(28, sizeDelta.y * 0.35f), label);
            return btn;
        }

        private static Image CreateImageObject(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;

            Image img = go.GetComponent<Image>();
            img.sprite = TileView.GetWhiteSprite();
            img.color = Color.white;
            return img;
        }

        private static Slider CreateSlider(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Slider));
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;

            // background
            Image bg = CreateImageObject(go.transform, "Background",
                new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
            ((RectTransform)bg.transform).offsetMin = Vector2.zero;
            ((RectTransform)bg.transform).offsetMax = Vector2.zero;
            bg.color = new Color(0, 0, 0, 0.5f);

            // fill area + fill
            RectTransform fillArea = MakeRT(go.transform, "Fill Area",
                new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
            fillArea.offsetMin = new Vector2(4, 4);
            fillArea.offsetMax = new Vector2(-4, -4);

            Image fill = CreateImageObject(fillArea, "Fill",
                new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
            ((RectTransform)fill.transform).offsetMin = Vector2.zero;
            ((RectTransform)fill.transform).offsetMax = Vector2.zero;
            fill.color = new Color(0.55f, 0.95f, 0.55f);

            Slider slider = go.GetComponent<Slider>();
            slider.fillRect = (RectTransform)fill.transform;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0f;
            slider.interactable = false;
            return slider;
        }

        // ===== reflection helper =====

        private static void InjectField(object target, string fieldName, object value)
        {
            if (target == null) { Debug.LogWarning($"[AppBootstrap] InjectField: target null for {fieldName}"); return; }
            FieldInfo f = target.GetType().GetField(fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (f == null)
            {
                Debug.LogWarning($"[AppBootstrap] Field '{fieldName}' not found on {target.GetType().Name}");
                return;
            }
            f.SetValue(target, value);
        }
    }

    /// <summary>
    /// Loads sprites from Assets/Sprites/* via AssetDatabase. Editor-only — runtime builds
    /// would need Addressables or Resources/. Keeps PNGs in their existing folders (no moves).
    /// </summary>
    internal class SpriteLibrary
    {
        public Sprite[,] backgroundsByZoneStage; // [zoneIdx 0..2, stageIdx 0..4]
        public Dictionary<int, Sprite> catPortraits = new Dictionary<int, Sprite>();
        public Dictionary<int, Sprite> catPlayPortraits = new Dictionary<int, Sprite>();
        public Sprite[] tiles;                    // indexed by TileType enum
        public Sprite starFilled;
        public Sprite starEmpty;

        public void LoadAll()
        {
            backgroundsByZoneStage = new Sprite[3, 5];

#if UNITY_EDITOR
            for (int z = 1; z <= 3; z++)
                for (int s = 1; s <= 5; s++)
                    backgroundsByZoneStage[z - 1, s - 1] = LoadSpriteAt($"Assets/Sprites/Backgrounds/bg_zone{z}_stage{s}.png");

            foreach (var kv in new (int id, string name)[]
            {
                (Constants.CAT_NABI,    "nabi"),
                (Constants.CAT_BELLA,   "bella"),
                (Constants.CAT_SAMI,    "sami"),
                (Constants.CAT_HODU,    "hodu"),
                (Constants.CAT_GUREUMI, "gureumi"),
            })
            {
                catPortraits[kv.id]     = LoadSpriteAt($"Assets/Sprites/Characters/cat_{kv.name}.png");
                catPlayPortraits[kv.id] = LoadSpriteAt($"Assets/Sprites/Characters/cat_{kv.name}_play.png");
            }

            tiles = new Sprite[6];
            tiles[(int)TileType.Fish]     = LoadSpriteAt("Assets/Sprites/Tiles/tile_fish.png");
            tiles[(int)TileType.Milk]     = LoadSpriteAt("Assets/Sprites/Tiles/tile_milk.png");
            tiles[(int)TileType.Yarn]     = LoadSpriteAt("Assets/Sprites/Tiles/tile_yarn.png");
            tiles[(int)TileType.Catnip]   = LoadSpriteAt("Assets/Sprites/Tiles/tile_catnip.png");
            tiles[(int)TileType.Pawprint] = LoadSpriteAt("Assets/Sprites/Tiles/tile_pawprint.png");
            tiles[(int)TileType.Fishbone] = LoadSpriteAt("Assets/Sprites/Tiles/tile_fishbone.png");
#else
            Debug.LogWarning("[SpriteLibrary] Runtime build: Assets/Sprites/* not loadable without Addressables. Using fallback colors.");
            tiles = new Sprite[6];
#endif

            // Procedural star sprites (simple colored squares — good enough until art is ready)
            starFilled = MakeColoredSprite(new Color(1f, 0.85f, 0.30f));
            starEmpty  = MakeColoredSprite(new Color(0.30f, 0.30f, 0.35f));
        }

        public Sprite GetBackground(int zone1Based, int stage1Based)
        {
            int z = zone1Based - 1, s = stage1Based - 1;
            if (backgroundsByZoneStage == null) return null;
            if (z < 0 || z >= 3 || s < 0 || s >= 5) return null;
            return backgroundsByZoneStage[z, s];
        }

        public Sprite GetCatPortrait(int catId)
        {
            return catPortraits.TryGetValue(catId, out Sprite sp) ? sp : null;
        }

        public Sprite GetCatPlayPortrait(int catId)
        {
            return catPlayPortraits.TryGetValue(catId, out Sprite sp) ? sp : null;
        }

        /// <summary>
        /// Build the CafeRestorationManager.ZoneBackgrounds[] structure via reflection on its
        /// nested type. Returned object is the typed array CafeRestorationManager expects.
        /// </summary>
        public Array BuildZoneBackgrounds(Type cafeMgrType)
        {
            Type zoneType = cafeMgrType.GetNestedType("ZoneBackgrounds", BindingFlags.Public | BindingFlags.NonPublic);
            if (zoneType == null) return null;

            Array zoneArray = Array.CreateInstance(zoneType, 3);
            FieldInfo stagesField = zoneType.GetField("stages", BindingFlags.Public | BindingFlags.Instance);
            for (int z = 0; z < 3; z++)
            {
                object zoneInst = Activator.CreateInstance(zoneType);
                Sprite[] stages = new Sprite[5];
                for (int s = 0; s < 5; s++) stages[s] = backgroundsByZoneStage[z, s];
                stagesField.SetValue(zoneInst, stages);
                zoneArray.SetValue(zoneInst, z);
            }
            return zoneArray;
        }

#if UNITY_EDITOR
        private static Sprite LoadSpriteAt(string assetPath)
        {
            Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (sp != null) return sp;

            UnityEngine.Object[] all = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            foreach (var o in all) if (o is Sprite s) return s;

            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            if (tex != null) return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));

            Debug.LogWarning($"[SpriteLibrary] Missing sprite: {assetPath}");
            return null;
        }
#endif

        private static Sprite MakeColoredSprite(Color c)
        {
            Texture2D t = new Texture2D(8, 8, TextureFormat.RGBA32, false);
            Color[] px = new Color[64];
            for (int i = 0; i < px.Length; i++) px[i] = c;
            t.SetPixels(px);
            t.Apply();
            return Sprite.Create(t, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f));
        }
    }
}
