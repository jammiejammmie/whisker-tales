using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using WhiskerTales.Cat;
using WhiskerTales.Core;
using WhiskerTales.Puzzle;
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

        private SpriteLibrary spriteLib;
        private Canvas rootCanvas;
        private RectTransform titlePanel;
        private RectTransform gameplayPanel;
        private RectTransform catRoomPanel;
        private RectTransform cafePanel;
        private Dictionary<NavigationTarget, RectTransform> panels;

        private void Awake()
        {
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

            panels = new Dictionary<NavigationTarget, RectTransform>
            {
                { NavigationTarget.Title,    titlePanel },
                { NavigationTarget.Gameplay, gameplayPanel },
                { NavigationTarget.CatRoom,  catRoomPanel },
                { NavigationTarget.Cafe,     cafePanel },
            };

            if (GameManager.Instance != null)
                GameManager.Instance.OnNavigationRequested += ShowPanel;

            ShowPanel(initialPanel);
            Debug.Log("[AppBootstrap] Scene constructed");
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnNavigationRequested -= ShowPanel;
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

            if (I18nManager.Instance != null)
                I18nManager.Instance.SetLanguage(Application.systemLanguage);

            InstallKoreanFontFallback();
        }

        /// <summary>
        /// LiberationSans SDF (TMP Essentials 기본 폰트)에는 한글 글리프가 없어서
        /// 한국어 텍스트가 □로 렌더링됨. TMP 3.0.6은 OS 동적 폰트(Font.CreateDynamicFromOSFont)
        /// 기반으로는 TMP_FontAsset을 못 만들어서, Windows/Fonts의 malgun.ttf를 Assets/Fonts/로
        /// 복사해 AssetDatabase로 임포트한 뒤 그 Font 에셋으로 CreateFontAsset(Font) 호출.
        /// 에디터 전용. 런타임 빌드는 별도 처리 필요 (TODO: Addressables 또는 StreamingAssets).
        /// </summary>
        private static void InstallKoreanFontFallback()
        {
#if UNITY_EDITOR
            try
            {
                if (TMPro.TMP_Settings.fallbackFontAssets == null) return;
                if (HasKoreanFallback()) return;

                string projectFontPath = "Assets/Fonts/MalgunGothic.ttf";

                if (!System.IO.File.Exists(projectFontPath))
                {
                    string[] sysCandidates =
                    {
                        @"C:\Windows\Fonts\malgun.ttf",
                        @"C:\Windows\Fonts\malgunbd.ttf",
                        @"C:\Windows\Fonts\NanumGothic.ttf",
                        @"C:\Windows\Fonts\gulim.ttc",
                    };
                    string sysPath = null;
                    foreach (string p in sysCandidates) { if (System.IO.File.Exists(p)) { sysPath = p; break; } }
                    if (sysPath == null)
                    {
                        Debug.LogWarning("[AppBootstrap] No Korean OS font available — Korean text may render as □.");
                        return;
                    }

                    System.IO.Directory.CreateDirectory("Assets/Fonts");
                    System.IO.File.Copy(sysPath, projectFontPath, false);
                    UnityEditor.AssetDatabase.ImportAsset(projectFontPath, UnityEditor.ImportAssetOptions.ForceUpdate);
                    Debug.Log($"[AppBootstrap] Imported Korean font: {sysPath} -> {projectFontPath}");
                }

                Font koFont = UnityEditor.AssetDatabase.LoadAssetAtPath<Font>(projectFontPath);
                if (koFont == null)
                {
                    Debug.LogWarning($"[AppBootstrap] LoadAssetAtPath<Font> returned null for {projectFontPath}");
                    return;
                }

                TMPro.TMP_FontAsset koAsset = TMPro.TMP_FontAsset.CreateFontAsset(koFont);
                if (koAsset == null)
                {
                    Debug.LogWarning($"[AppBootstrap] CreateFontAsset still returned null for {koFont.name}");
                    return;
                }

                koAsset.name = "Korean Fallback (MalgunGothic)";
                koAsset.atlasPopulationMode = TMPro.AtlasPopulationMode.Dynamic;
                TMPro.TMP_Settings.fallbackFontAssets.Add(koAsset);

                Debug.Log($"[AppBootstrap] Korean fallback registered: {koAsset.name}");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[AppBootstrap] Korean font setup failed: {e.GetType().Name}: {e.Message}");
            }
#else
            Debug.LogWarning("[AppBootstrap] Korean fallback not available in runtime build (TODO: bundle font via StreamingAssets).");
#endif
        }

        private static bool HasKoreanFallback()
        {
            var list = TMPro.TMP_Settings.fallbackFontAssets;
            if (list == null) return false;
            foreach (var f in list)
            {
                if (f != null && f.name != null && f.name.IndexOf("Korean", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
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
                catPortraits[kv.id] = LoadSpriteAt($"Assets/Sprites/Characters/cat_{kv.name}.png");
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
