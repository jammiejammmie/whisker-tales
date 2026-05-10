#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WhiskerTales.UI;

namespace WhiskerTales.EditorTools
{
    public static class WhiskerTalesSceneSetupEditor
    {
        private const string ScenePath = "Assets/WhiskerTales/Scenes/Main.unity";
        private const string ScreenPrefabDir = "Assets/WhiskerTales/Prefabs/UI/Screens";
        private const string CommonPrefabDir = "Assets/WhiskerTales/Prefabs/UI/Common";
        private const string ResourcesDir = "Assets/WhiskerTales/Resources";
        private const string ArtRoot = "Assets/WhiskerTales/Art";

        private static readonly Vector2 ReferenceResolution = new Vector2(1080f, 1920f);

        [MenuItem("WhiskerTales/Setup Scene")]
        public static void SetupScene()
        {
            EnsureFolders();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Main";

            CreateEventSystem();
            CreateMainCamera();

            GameObject runtimeRoot = new GameObject("WhiskerRuntime");
            runtimeRoot.AddComponent<UIAssetRegistryRuntime>();
            PhoneVisibleSceneInstaller installer = runtimeRoot.AddComponent<PhoneVisibleSceneInstaller>();

            Canvas canvas = CreateCanvas();
            RectTransform safeArea = CreateFullRect("SafeArea", canvas.transform);
            SafeAreaHandler safeAreaHandler = safeArea.gameObject.AddComponent<SafeAreaHandler>();

            RectTransform screensRoot = CreateFullRect("Screens", safeArea);
            RectTransform persistentRoot = CreateFullRect("PersistentUI", safeArea);
            RectTransform popupsRoot = CreateFullRect("Popups", safeArea);
            RectTransform overlayRoot = CreateFullRect("FullscreenOverlay", canvas.transform);
            overlayRoot.SetAsLastSibling();

            GameObject audioRoot = new GameObject("AudioRoot");

            ScreenRefs refs = CreateAllScreens(screensRoot, popupsRoot);
            ConnectInstaller(installer, canvas, safeArea, screensRoot, popupsRoot, persistentRoot, refs);
            AutoMapSprites(runtimeRoot.GetComponent<UIAssetRegistryRuntime>());
            AutoBindScreenSprites(refs);
            AutoBindBottomNavs(refs, installer);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            Debug.Log("WhiskerTales scene setup complete: " + ScenePath);
        }

        private static void EnsureFolders()
        {
            CreateFolder("Assets", "WhiskerTales");
            CreateFolder("Assets/WhiskerTales", "Scenes");
            CreateFolder("Assets/WhiskerTales", "Prefabs");
            CreateFolder("Assets/WhiskerTales/Prefabs", "UI");
            CreateFolder("Assets/WhiskerTales/Prefabs/UI", "Screens");
            CreateFolder("Assets/WhiskerTales/Prefabs/UI", "Common");
            CreateFolder("Assets/WhiskerTales/Prefabs/UI", "Popups");
            CreateFolder("Assets/WhiskerTales/Prefabs/UI", "Items");
            CreateFolder("Assets/WhiskerTales", "Resources");
        }

        private static void CreateFolder(string parent, string child)
        {
            string path = parent + "/" + child;

            if (AssetDatabase.IsValidFolder(path) == false)
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }

        private static void CreateEventSystem()
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        private static void CreateMainCamera()
        {
            GameObject go = new GameObject("Main Camera");
            go.tag = "MainCamera";

            Camera camera = go.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            // #1a1a1a (26/255 = 0.10196...)
            camera.backgroundColor = new Color(26f / 255f, 26f / 255f, 26f / 255f, 1f);
            camera.cullingMask = ~0; // Everything
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 1000f;
            camera.depth = -1f;

            go.AddComponent<AudioListener>();
        }

        private static Canvas CreateCanvas()
        {
            GameObject go = new GameObject("Canvas_Main", typeof(RectTransform));
            Canvas canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = false;

            CanvasScaler scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.matchWidthOrHeight = 0.5f;

            go.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private static RectTransform CreateFullRect(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0.5f);
            return rt;
        }

        private static ScreenRefs CreateAllScreens(RectTransform screensRoot, RectTransform popupsRoot)
        {
            ScreenRefs refs = new ScreenRefs();

            refs.mainTitle = CreateScreen<MainTitleScreenController>("Screen_MainTitle", screensRoot);
            refs.gameplay = CreateScreen<GameplayUIScreenController>("Screen_Gameplay", screensRoot);
            refs.catBonding = CreateScreen<CatBondingScreenController>("Screen_CatBonding", screensRoot);
            refs.cafeRestoration = CreateScreen<CafeRestorationScreenController>("Screen_CafeRestoration", screensRoot);
            refs.arcade = CreateScreen<ArcadeScreenController>("Screen_Arcade", screensRoot);
            refs.meditation = CreateScreen<MeditationGardenScreenController>("Screen_Meditation", screensRoot);
            refs.settings = CreateScreen<SettingsScreenController>("Screen_Settings", screensRoot);
            refs.levelClear = CreateScreen<LevelClearScreenController>("Screen_LevelClear", screensRoot);
            refs.gameOver = CreateScreen<GameOverScreenController>("Screen_GameOver", screensRoot);
            refs.tutorial = CreateScreen<TutorialOverlayController>("Overlay_Tutorial", popupsRoot);
            refs.loading = CreateScreen<LoadingScreenController>("Screen_Loading", screensRoot);
            refs.detox = CreateScreen<DetoxModalController>("Modal_Detox", popupsRoot);
            refs.sleep = CreateScreen<SleepModeScreenController>("Screen_SleepMode", screensRoot);
            refs.idleReward = CreateScreen<IdleRewardModalController>("Modal_IdleReward", popupsRoot);
            refs.referral = CreateScreen<ReferralShareScreenController>("Screen_ReferralShare", screensRoot);
            refs.photoStudio = CreateScreen<PhotoStudioScreenController>("Screen_PhotoStudio", screensRoot);

            BuildMainTitle(refs.mainTitle.gameObject);
            BuildGameplay(refs.gameplay.gameObject);
            BuildCatBonding(refs.catBonding.gameObject);
            BuildCafeRestoration(refs.cafeRestoration.gameObject);
            BuildArcade(refs.arcade.gameObject);
            BuildMeditation(refs.meditation.gameObject);
            BuildSettings(refs.settings.gameObject);
            BuildLevelClear(refs.levelClear.gameObject);
            BuildGameOver(refs.gameOver.gameObject);
            BuildTutorial(refs.tutorial.gameObject);
            BuildLoading(refs.loading.gameObject);
            BuildDetox(refs.detox.gameObject);
            BuildSleep(refs.sleep.gameObject);
            BuildIdleReward(refs.idleReward.gameObject);
            BuildReferral(refs.referral.gameObject);
            BuildPhotoStudio(refs.photoStudio.gameObject);

            SavePrefab(refs.mainTitle.gameObject, "Screen_MainTitle.prefab");
            SavePrefab(refs.gameplay.gameObject, "Screen_Gameplay.prefab");
            SavePrefab(refs.catBonding.gameObject, "Screen_CatBonding.prefab");
            SavePrefab(refs.cafeRestoration.gameObject, "Screen_CafeRestoration.prefab");
            SavePrefab(refs.arcade.gameObject, "Screen_Arcade.prefab");
            SavePrefab(refs.meditation.gameObject, "Screen_Meditation.prefab");
            SavePrefab(refs.settings.gameObject, "Screen_Settings.prefab");
            SavePrefab(refs.levelClear.gameObject, "Screen_LevelClear.prefab");
            SavePrefab(refs.gameOver.gameObject, "Screen_GameOver.prefab");
            SavePrefab(refs.tutorial.gameObject, "Overlay_Tutorial.prefab");
            SavePrefab(refs.loading.gameObject, "Screen_Loading.prefab");
            SavePrefab(refs.detox.gameObject, "Modal_Detox.prefab");
            SavePrefab(refs.sleep.gameObject, "Screen_SleepMode.prefab");
            SavePrefab(refs.idleReward.gameObject, "Modal_IdleReward.prefab");
            SavePrefab(refs.referral.gameObject, "Screen_ReferralShare.prefab");
            SavePrefab(refs.photoStudio.gameObject, "Screen_PhotoStudio.prefab");

            return refs;
        }

        private static T CreateScreen<T>(string name, Transform parent) where T : UIScreenBase
        {
            RectTransform rt = CreateFullRect(name, parent);
            CanvasGroup group = rt.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 1f;
            return rt.gameObject.AddComponent<T>();
        }

        private static void SavePrefab(GameObject instance, string fileName)
        {
            string path = ScreenPrefabDir + "/" + fileName;
            PrefabUtility.SaveAsPrefabAsset(instance, path);
        }

        private static void BuildMainTitle(GameObject root)
        {
            CreateImage("BG_Background", root.transform, StretchFull(), "bg_zone1_stage5");
            Image overlay = CreateImage("Overlay_Cream", root.transform, StretchFull(), null);
            overlay.color = new Color(0.96f, 0.94f, 0.9f, 0.12f);
            CreateImage("Logo", root.transform, TopCenter(0, -160, 520, 250), "logo_whisker_tales");
            CreateImage("Cats_Group", root.transform, Center(0, -130, 900, 980), "cat_group_main");
            CreateText("TXT_Tagline", root.transform, BottomCenter(0, 280, 900, 90), "고요한 한옥에서 고양이와 함께", 54, TextAlignmentOptions.Center);
            RectTransform nav = CreateBottomNav(root.transform);
            nav.name = "BottomNav";
        }

        private static void BuildGameplay(GameObject root)
        {
            CreateImage("BG_Background", root.transform, StretchFull(), "bg_zone1_stage1");
            RectTransform topHud = CreateRect("TopHUD", root.transform, TopCenter(0, -88, 1000, 150));
            CreateImage("Btn_Back", topHud, TopCenter(-460, -24, 96, 96), "btn_back").gameObject.AddComponent<Button>();
            CreateText("TXT_Level", topHud, TopCenter(-330, -34, 220, 80), "Level 1", 38, TextAlignmentOptions.Center);
            CreateText("TXT_Moves", topHud, TopCenter(0, -34, 260, 80), "Moves: 18", 38, TextAlignmentOptions.Center);
            CreateText("TXT_Score", topHud, TopCenter(320, -34, 300, 80), "Score: 0", 38, TextAlignmentOptions.Center);
            RectTransform detox = CreateRect("DetoxMessagePanel", root.transform, TopCenter(0, -285, 760, 150));
            Image detoxBg = detox.gameObject.AddComponent<Image>();
            detoxBg.sprite = FindSpriteByKey("tutorial_bubble");
            detoxBg.type = Image.Type.Sliced;
            CreateText("TXT_DetoxMessage", detox, StretchFull(), "오늘 당신의 시간은 어떤 빛깔이었나요?", 34, TextAlignmentOptions.Center);
            RectTransform board = CreateRect("PuzzleBoard", root.transform, TopCenter(0, -575, 768, 768));
            CreateImage("BoardFrame", board, StretchFull(), null).color = new Color(0.55f, 0.45f, 0.33f, 0.35f);
            CreateRect("TileContainer", board, StretchFull());
            CreateRect("MatchEffectLayer", board, StretchFull());
            CreateRect("HintLayer", board, StretchFull());
            RectTransform boosters = CreateRect("BoosterRow", root.transform, BottomCenter(0, 400, 640, 130));
            CreateImage("Btn_Hammer", boosters, Center(-220, 0, 120, 120), "btn_restart").gameObject.AddComponent<Button>();
            CreateImage("Btn_ColorBomb", boosters, Center(0, 0, 120, 120), "tile_bomb").gameObject.AddComponent<Button>();
            CreateImage("Btn_Shuffle", boosters, Center(220, 0, 120, 120), "btn_restart").gameObject.AddComponent<Button>();
            RectTransform goal = CreateRect("GoalPanel", root.transform, BottomCenter(0, 170, 820, 180));
            Image goalBg = goal.gameObject.AddComponent<Image>();
            goalBg.color = new Color(0.96f, 0.94f, 0.9f, 0.72f);
            CreateText("TXT_Goal", goal, StretchFull(), "블록 20개 제거  0/20", 40, TextAlignmentOptions.Center);
        }

        private static void BuildCatBonding(GameObject root)
        {
            CreateImage("BG_Background", root.transform, StretchFull(), "bg_zone1_stage4");
            CreateText("TXT_CatName", root.transform, TopCenter(0, -125, 620, 90), "나비", 64, TextAlignmentOptions.Center);
            CreateImage("Btn_Camera", root.transform, TopCenter(410, -96, 96, 96), "btn_camera").gameObject.AddComponent<Button>();
            CreateImage("Btn_Help", root.transform, TopCenter(300, -96, 80, 80), "icon_paw").gameObject.AddComponent<Button>();
            CreateImage("CatPortrait", root.transform, Center(0, -210, 720, 780), "cat_nabi");
            CreateText("TXT_Level", root.transform, BottomCenter(0, 640, 700, 80), "Lv.1", 42, TextAlignmentOptions.Center);
            RectTransform bar = CreateRect("AffinityBar", root.transform, BottomCenter(0, 555, 720, 70));
            CreateImage("AffinityFill", bar, StretchFull(), null).color = new Color(0.91f, 0.66f, 0.49f, 0.9f);
            CreateText("TXT_Affinity", bar, StretchFull(), "35%", 30, TextAlignmentOptions.Center);
            CreateText("TXT_RewardAffinity", root.transform, BottomCenter(-210, 470, 260, 60), "+5 Affinity", 30, TextAlignmentOptions.Center);
            CreateText("TXT_RewardCoins", root.transform, BottomCenter(210, 470, 260, 60), "+10 Coins", 30, TextAlignmentOptions.Center);
            RectTransform buttons = CreateRect("InteractionButtons", root.transform, BottomCenter(0, 245, 920, 160));
            CreateLabeledButton("Btn_Pet", buttons, Center(-310, 0, 250, 130), "쓰다듬기");
            CreateLabeledButton("Btn_Treat", buttons, Center(0, 0, 250, 130), "간식");
            CreateLabeledButton("Btn_Play", buttons, Center(310, 0, 250, 130), "놀기");
            RectTransform bubble = CreateRect("TutorialBubble", root.transform, BottomCenter(0, 165, 780, 150));
            Image bubbleImg = bubble.gameObject.AddComponent<Image>();
            bubbleImg.sprite = FindSpriteByKey("tutorial_bubble");
            bubbleImg.type = Image.Type.Sliced;
            CreateText("TXT_Tip", bubble, StretchFull(), "나비가 조용히 당신을 바라보고 있어요.", 32, TextAlignmentOptions.Center);
            CreateBottomNav(root.transform);
        }

        private static void BuildCafeRestoration(GameObject root)
        {
            CreateImage("BG_Background", root.transform, StretchFull(), "bg_zone2_stage2");
            CreateText("TXT_Title", root.transform, TopCenter(0, -116, 600, 90), "카페 복원", 62, TextAlignmentOptions.Center);
            CreateText("TXT_StarTotal", root.transform, TopCenter(360, -104, 250, 86), "12", 42, TextAlignmentOptions.Center);
            RectTransform content = CreateRect("ScrollContent", root.transform, TopCenter(0, -285, 920, 1120));
            for (int i = 0; i < 4; i++)
            {
                RectTransform card = CreateRect("RestoreCard_" + (i + 1), content, TopCenter(0, -i * 258, 880, 230));
                Image cardBg = card.gameObject.AddComponent<Image>();
                cardBg.color = new Color(0.96f, 0.94f, 0.9f, 0.82f);
                CreateText("TXT_Title", card, Center(-210, 45, 420, 60), i == 0 ? "낡은 간판 복원" : "복원 항목", 36, TextAlignmentOptions.Left);
                CreateText("TXT_Progress", card, Center(-210, -35, 420, 60), "필요 별 3개", 30, TextAlignmentOptions.Left);
                CreateLabeledButton("Btn_Restore", card, Center(260, -35, 260, 90), "복원하기");
                CreateImage("Icon_Lock", card, Center(300, 42, 64, 64), "icon_lock");
            }
            RectTransform zones = CreateRect("ZoneProgressRow", root.transform, BottomCenter(0, 205, 900, 90));
            CreateText("TXT_Zone1", zones, Center(-300, 0, 260, 70), "1구역 2/5", 30, TextAlignmentOptions.Center);
            CreateText("TXT_Zone2", zones, Center(0, 0, 260, 70), "2구역 0/5", 30, TextAlignmentOptions.Center);
            CreateText("TXT_Zone3", zones, Center(300, 0, 260, 70), "3구역 0/5", 30, TextAlignmentOptions.Center);
            CreateBottomNav(root.transform);
        }

        private static void BuildArcade(GameObject root)
        {
            CreateImage("BG_Background", root.transform, StretchFull(), "bg_zone2_stage3");
            CreateText("TXT_Title", root.transform, TopCenter(0, -120, 700, 90), "오늘의 미니게임", 60, TextAlignmentOptions.Center);
            string[] names = { "고양이 숨은그림찾기", "고양이 두더지잡기", "Coming Soon" };
            string[] cats = { "cat_nabi", "cat_hodu", "cat_gureumi" };
            for (int i = 0; i < 3; i++)
            {
                RectTransform card = CreateRect("GameCard_" + (i + 1), root.transform, TopCenter(0, -300 - i * 340, 860, 310));
                Image bg = card.gameObject.AddComponent<Image>();
                bg.color = new Color(0.96f, 0.94f, 0.9f, 0.82f);
                CreateImage("CatImage", card, Center(-270, 0, 220, 220), cats[i]);
                CreateText("TXT_Title", card, Center(110, 35, 460, 80), names[i], 38, TextAlignmentOptions.Center);
                CreateText("TXT_State", card, Center(110, -50, 460, 70), i == 2 ? "잠김" : "플레이", 30, TextAlignmentOptions.Center);
                card.gameObject.AddComponent<Button>();
            }
            CreateText("TXT_Bottom", root.transform, BottomCenter(0, 215, 700, 70), "내일 또 만나요", 34, TextAlignmentOptions.Center);
            CreateBottomNav(root.transform);
        }

        private static void BuildMeditation(GameObject root)
        {
            CreateImage("BG_Background", root.transform, StretchFull(), "bg_zone3_stage5");
            CreateText("TXT_Title", root.transform, TopCenter(0, -120, 600, 90), "명상 정원", 62, TextAlignmentOptions.Center);
            CreateText("TXT_PeacePoint", root.transform, TopCenter(345, -108, 290, 84), "24", 42, TextAlignmentOptions.Center);
            RectTransform frame = CreateRect("SandCanvasFrame", root.transform, TopCenter(0, -330, 900, 980));
            Image bg = frame.gameObject.AddComponent<Image>();
            bg.color = new Color(0.96f, 0.94f, 0.9f, 0.82f);
            CreateRect("DrawingLayer", frame, Center(0, 0, 820, 880));
            CreateLabeledButton("Btn_ResetGarden", root.transform, BottomCenter(-245, 210, 360, 100), "정원 초기화");
            CreateLabeledButton("Btn_EndMeditation", root.transform, BottomCenter(245, 210, 360, 100), "명상 종료");
            CreateBottomNav(root.transform);
        }

        private static void BuildSettings(GameObject root)
        {
            Image bg = CreateImage("BG_SettingsPaper", root.transform, StretchFull(), null);
            bg.color = new Color(0.96f, 0.94f, 0.9f, 1f);
            CreateText("TXT_Title", root.transform, TopCenter(0, -110, 500, 90), "설정", 64, TextAlignmentOptions.Center);
            RectTransform scroll = CreateRect("ScrollView_Settings", root.transform, TopCenter(0, -235, 940, 1360));
            string[] rows = { "일일 알림", "BGM 볼륨", "효과음 볼륨", "디톡스 모드", "언어", "개인정보처리방침", "문의하기", "앱 평가하기", "버전 v1.0.0", "친구 초대", "피드백", "품질 진단" };
            for (int i = 0; i < rows.Length; i++)
            {
                RectTransform row = CreateRect("Row_" + i, scroll, TopCenter(0, -i * 114, 900, 96));
                Image rowBg = row.gameObject.AddComponent<Image>();
                rowBg.color = new Color(1f, 1f, 1f, 0.28f);
                CreateText("TXT_Row", row, StretchFull(), rows[i], 32, TextAlignmentOptions.Left);
            }
            CreateBottomNav(root.transform);
        }

        private static void BuildLevelClear(GameObject root)
        {
            CreateImage("BG_Background", root.transform, StretchFull(), "level_clear_bg");
            CreateText("TXT_Title", root.transform, TopCenter(0, -180, 800, 110), "레벨 클리어!", 78, TextAlignmentOptions.Center);
            CreateText("TXT_Level", root.transform, TopCenter(0, -290, 600, 70), "Level 1", 42, TextAlignmentOptions.Center);
            RectTransform stars = CreateRect("StarGroup", root.transform, TopCenter(0, -420, 700, 220));
            CreateImage("Star_01", stars, Center(-220, 0, 180, 180), "icon_star_filled");
            CreateImage("Star_02", stars, Center(0, 35, 210, 210), "icon_star_filled");
            CreateImage("Star_03", stars, Center(220, 0, 180, 180), "icon_star_filled");
            CreateText("TXT_Score", root.transform, TopCenter(0, -690, 720, 140), "12,400", 52, TextAlignmentOptions.Center);
            CreateLabeledButton("Btn_Next", root.transform, BottomCenter(0, 360, 720, 130), "다음 레벨");
            CreateLabeledButton("Btn_Home", root.transform, BottomCenter(0, 205, 560, 105), "홈으로");
        }

        private static void BuildGameOver(GameObject root)
        {
            CreateImage("BG_Background", root.transform, StretchFull(), "game_over_bg");
            CreateImage("EmotionSad", root.transform, TopCenter(0, -370, 180, 180), "emotion_sad");
            CreateText("TXT_Main", root.transform, TopCenter(0, -580, 700, 100), "괜찮아요", 72, TextAlignmentOptions.Center);
            CreateText("TXT_Sub", root.transform, TopCenter(0, -690, 800, 80), "고양이가 기다리고 있어요", 42, TextAlignmentOptions.Center);
            CreateLabeledButton("Btn_Retry", root.transform, BottomCenter(0, 360, 720, 130), "다시 도전");
            CreateLabeledButton("Btn_Home", root.transform, BottomCenter(0, 205, 560, 105), "홈으로");
        }

        private static void BuildTutorial(GameObject root)
        {
            Image dim = CreateImage("Overlay_Dim", root.transform, StretchFull(), null);
            dim.color = new Color(0f, 0f, 0f, 0.55f);
            CreateImage("HighlightRing", root.transform, Center(0, 0, 300, 300), "highlight_ring");
            CreateImage("HintTap", root.transform, Center(110, -120, 200, 300), "hint_tap");
            CreateImage("HintSwipe", root.transform, Center(0, -120, 300, 200), "hint_swipe");
            RectTransform bubble = CreateRect("TutorialBubble", root.transform, BottomCenter(0, 280, 820, 270));
            Image b = bubble.gameObject.AddComponent<Image>();
            b.sprite = FindSpriteByKey("tutorial_bubble");
            b.type = Image.Type.Sliced;
            CreateText("TXT_Tutorial", bubble, StretchFull(), "같은 타일 3개를 맞춰보세요!", 38, TextAlignmentOptions.Center);
            CreateLabeledButton("Btn_Confirm", root.transform, BottomCenter(0, 215, 360, 90), "확인");
        }

        private static void BuildLoading(GameObject root)
        {
            CreateImage("BG_Background", root.transform, StretchFull(), "bg_zone1_stage5");
            CreateImage("Logo", root.transform, TopCenter(0, -240, 620, 260), "logo_whisker_tales");
            CreateImage("LoadingCatSpinner", root.transform, TopCenter(0, -910, 180, 180), "loading_cat");
            CreateText("TXT_Loading", root.transform, TopCenter(0, -1110, 420, 60), "불러오는 중...", 36, TextAlignmentOptions.Center);
            RectTransform bar = CreateRect("ProgressBarFrame", root.transform, BottomCenter(0, 260, 620, 44));
            CreateImage("ProgressBarFill", bar, LeftCenter(-290, 0, 0, 28), null).color = new Color(0.91f, 0.66f, 0.49f, 1f);
        }

        private static void BuildDetox(GameObject root)
        {
            Image dim = CreateImage("Overlay_Dim", root.transform, StretchFull(), null);
            dim.color = new Color(0f, 0f, 0f, 0.65f);
            RectTransform card = CreateRect("DetoxCard", root.transform, Center(0, 0, 860, 920));
            Image img = card.gameObject.AddComponent<Image>();
            img.sprite = FindSpriteByKey("detox_card_bg");
            img.type = Image.Type.Sliced;
            CreateText("TXT_Title", card, TopCenter(0, -90, 500, 80), "오늘의 한마디", 56, TextAlignmentOptions.Center);
            CreateText("TXT_Message", card, Center(0, -40, 660, 340), "잠깐 쉬어가도 괜찮아요.", 42, TextAlignmentOptions.Center);
            CreateLabeledButton("Btn_Confirm", card, BottomCenter(0, 250, 620, 120), "잠깐 쉬어갈게요");
            CreateLabeledButton("Btn_Skip", card, BottomCenter(0, 395, 420, 88), "계속 할게요");
        }

        private static void BuildSleep(GameObject root)
        {
            CreateImage("BG_Background", root.transform, StretchFull(), "bg_zone3_stage5");
            CreateImage("EmotionSleepy", root.transform, TopCenter(0, -360, 260, 260), "emotion_sleepy");
            CreateText("TXT_Title", root.transform, TopCenter(0, -630, 600, 100), "수면 모드", 76, TextAlignmentOptions.Center);
            CreateText("TXT_Sub", root.transform, TopCenter(0, -735, 720, 80), "폰을 내려놓고 쉬어요", 40, TextAlignmentOptions.Center);
            CreateText("TXT_Timer", root.transform, TopCenter(0, -940, 520, 140), "00:00:00", 56, TextAlignmentOptions.Center);
            CreateLabeledButton("Btn_Enter", root.transform, BottomCenter(0, 360, 720, 130), "수면 모드 시작");
            CreateLabeledButton("Btn_Exit", root.transform, BottomCenter(0, 205, 520, 100), "종료");
        }

        private static void BuildIdleReward(GameObject root)
        {
            Image dim = CreateImage("Overlay_Dim", root.transform, StretchFull(), null);
            dim.color = new Color(0f, 0f, 0f, 0.58f);
            RectTransform card = CreateRect("RewardCard", root.transform, Center(0, 0, 820, 880));
            Image c = card.gameObject.AddComponent<Image>();
            c.color = new Color(0.96f, 0.94f, 0.9f, 0.94f);
            CreateImage("EmotionHappy", card, TopCenter(0, -80, 200, 200), "emotion_happy");
            CreateText("TXT_Title", card, TopCenter(0, -300, 600, 90), "돌아왔어요!", 64, TextAlignmentOptions.Center);
            CreateText("TXT_OfflineTime", card, TopCenter(0, -400, 680, 70), "그동안 2시간 자리를 비웠어요", 36, TextAlignmentOptions.Center);
            CreateText("TXT_CoinReward", card, TopCenter(0, -540, 520, 80), "멸치 +120", 38, TextAlignmentOptions.Center);
            CreateText("TXT_HeartReward", card, TopCenter(0, -640, 520, 80), "하트 +3", 38, TextAlignmentOptions.Center);
            CreateLabeledButton("Btn_Confirm", card, BottomCenter(0, 80, 560, 120), "확인");
        }

        private static void BuildReferral(GameObject root)
        {
            CreateImage("BG_Background", root.transform, StretchFull(), "bg_zone1_stage5");
            CreateImage("Btn_Back", root.transform, TopCenter(-430, -100, 96, 96), "btn_back").gameObject.AddComponent<Button>();
            CreateText("TXT_Title", root.transform, TopCenter(0, -120, 500, 90), "친구 초대", 68, TextAlignmentOptions.Center);
            CreateImage("CatPortrait", root.transform, TopCenter(0, -420, 520, 520), "cat_nabi");
            CreateText("TXT_Code", root.transform, TopCenter(0, -900, 760, 150), "NABI-1234", 68, TextAlignmentOptions.Center);
            CreateText("TXT_Desc", root.transform, TopCenter(0, -1090, 760, 120), "친구에게 초대 코드를 보내고 함께 냥이의 집을 복원해요.", 34, TextAlignmentOptions.Center);
            CreateLabeledButton("Btn_Share", root.transform, BottomCenter(0, 320, 680, 130), "공유하기");
        }

        private static void BuildPhotoStudio(GameObject root)
        {
            CreateImage("BG_PhotoStudio", root.transform, StretchFull(), "bg_zone1_stage5");
            CreateImage("Btn_Back", root.transform, TopCenter(-430, -100, 96, 96), "btn_back").gameObject.AddComponent<Button>();
            RectTransform preview = CreateRect("PreviewFrame", root.transform, TopCenter(0, -290, 860, 980));
            Image p = preview.gameObject.AddComponent<Image>();
            p.color = new Color(0.96f, 0.94f, 0.9f, 0.64f);
            CreateImage("CatPortrait", preview, Center(0, 0, 580, 680), "cat_nabi");
            RectTransform thumbs = CreateRect("BackgroundSelectorRow", root.transform, BottomCenter(0, 515, 900, 150));
            for (int i = 0; i < 5; i++)
            {
                CreateImage("BGThumb_" + (i + 1), thumbs, Center(-336 + i * 168, 0, 150, 120), "bg_zone1_stage5").gameObject.AddComponent<Button>();
            }
            CreateLabeledButton("Btn_Front", root.transform, BottomCenter(-180, 330, 320, 100), "정면");
            CreateLabeledButton("Btn_Play", root.transform, BottomCenter(180, 330, 320, 100), "놀아주기");
            CreateLabeledButton("Btn_Capture", root.transform, BottomCenter(-180, 170, 320, 120), "찍기");
            CreateLabeledButton("Btn_Share", root.transform, BottomCenter(180, 170, 320, 120), "공유");
            Image flash = CreateImage("FlashOverlay", root.transform, StretchFull(), null);
            flash.color = new Color(1f, 1f, 1f, 0f);
            flash.raycastTarget = false;
        }

        private static RectTransform CreateBottomNav(Transform parent)
        {
            RectTransform nav = CreateRect("BottomNav", parent, BottomStretch(0, 0, 1080, 170));
            Image bg = nav.gameObject.AddComponent<Image>();
            bg.sprite = FindSpriteByKey("nav_bar_bg");
            bg.type = Image.Type.Sliced;
            BottomNavRuntimeBinder binder = nav.gameObject.AddComponent<BottomNavRuntimeBinder>();
            string[] names = { "Home", "CatRoom", "Cafe", "Arcade", "Meditation" };
            string[] keys = { "tab_home", "tab_catroom", "tab_cafe", "tab_arcade", "tab_meditation" };
            float[] x = { -360f, -180f, 0f, 180f, 360f };
            for (int i = 0; i < names.Length; i++)
            {
                GameObject tab = CreateImage("Tab_" + names[i], nav, BottomCenter(x[i], 12, 120, 120), keys[i]).gameObject;
                tab.AddComponent<Button>();
            }
            return nav;
        }

        private static RectTransform CreateLabeledButton(string name, Transform parent, RectSpec spec, string label)
        {
            RectTransform rt = CreateRect(name, parent, spec);
            Image image = rt.gameObject.AddComponent<Image>();
            image.color = new Color(0.96f, 0.94f, 0.9f, 0.92f);
            image.type = Image.Type.Sliced;
            rt.gameObject.AddComponent<Button>();
            CreateText("TXT_Label", rt, StretchFull(), label, 32, TextAlignmentOptions.Center);
            return rt;
        }

        private static Image CreateImage(string name, Transform parent, RectSpec spec, string spriteKey)
        {
            RectTransform rt = CreateRect(name, parent, spec);
            Image image = rt.gameObject.AddComponent<Image>();
            image.sprite = string.IsNullOrEmpty(spriteKey) ? null : FindSpriteByKey(spriteKey);
            image.color = Color.white;
            image.raycastTarget = false;
            return image;
        }

        private static TMP_Text CreateText(string name, Transform parent, RectSpec spec, string text, int fontSize, TextAlignmentOptions alignment)
        {
            RectTransform rt = CreateRect(name, parent, spec);
            TextMeshProUGUI tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.color = new Color(0.17f, 0.09f, 0.06f, 1f);
            tmp.enableWordWrapping = true;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static RectTransform CreateRect(string name, Transform parent, RectSpec spec)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = spec.anchorMin;
            rt.anchorMax = spec.anchorMax;
            rt.pivot = spec.pivot;
            rt.anchoredPosition = spec.anchoredPosition;
            rt.sizeDelta = spec.sizeDelta;
            rt.offsetMin = spec.offsetMin;
            rt.offsetMax = spec.offsetMax;
            return rt;
        }

        private static RectSpec StretchFull()
        {
            return new RectSpec
            {
                anchorMin = Vector2.zero,
                anchorMax = Vector2.one,
                pivot = new Vector2(0.5f, 0.5f),
                offsetMin = Vector2.zero,
                offsetMax = Vector2.zero,
                anchoredPosition = Vector2.zero,
                sizeDelta = Vector2.zero
            };
        }

        private static RectSpec Center(float x, float y, float w, float h)
        {
            return Anchored(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), x, y, w, h);
        }

        private static RectSpec TopCenter(float x, float y, float w, float h)
        {
            return Anchored(new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), x, y, w, h);
        }

        private static RectSpec BottomCenter(float x, float y, float w, float h)
        {
            return Anchored(new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), x, y, w, h);
        }

        private static RectSpec LeftCenter(float x, float y, float w, float h)
        {
            return Anchored(new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), x, y, w, h);
        }

        private static RectSpec BottomStretch(float x, float y, float w, float h)
        {
            return BottomCenter(x, y, w, h);
        }

        private static RectSpec Anchored(Vector2 min, Vector2 max, Vector2 pivot, float x, float y, float w, float h)
        {
            return new RectSpec
            {
                anchorMin = min,
                anchorMax = max,
                pivot = pivot,
                anchoredPosition = new Vector2(x, y),
                sizeDelta = new Vector2(w, h),
                offsetMin = Vector2.zero,
                offsetMax = Vector2.zero
            };
        }

        private static Sprite FindSpriteByKey(string key)
        {
            if (string.IsNullOrEmpty(key) == true)
            {
                return null;
            }

            string[] guids = AssetDatabase.FindAssets(key + " t:Sprite", new[] { ArtRoot, "Assets" });

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

                if (sprite != null && Path.GetFileNameWithoutExtension(path).IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return sprite;
                }
            }

            return null;
        }

        private static void AutoMapSprites(UIAssetRegistryRuntime registry)
        {
            if (registry == null)
            {
                return;
            }

            SerializedObject so = new SerializedObject(registry);
            SerializedProperty spritesProp = so.FindProperty("sprites");

            if (spritesProp == null)
            {
                return;
            }

            List<string> keys = GetRequiredSpriteKeys();
            spritesProp.ClearArray();

            for (int i = 0; i < keys.Count; i++)
            {
                spritesProp.InsertArrayElementAtIndex(i);
                SerializedProperty item = spritesProp.GetArrayElementAtIndex(i);
                item.FindPropertyRelative("key").stringValue = keys[i];
                item.FindPropertyRelative("sprite").objectReferenceValue = FindSpriteByKey(keys[i]);
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(registry);
        }

        private static List<string> GetRequiredSpriteKeys()
        {
            return new List<string>
            {
                "bg_zone1_stage1", "bg_zone1_stage2", "bg_zone1_stage3", "bg_zone1_stage4", "bg_zone1_stage5",
                "bg_zone2_stage1", "bg_zone2_stage2", "bg_zone2_stage3", "bg_zone2_stage4", "bg_zone2_stage5",
                "bg_zone3_stage1", "bg_zone3_stage2", "bg_zone3_stage3", "bg_zone3_stage4", "bg_zone3_stage5",
                "level_clear_bg", "game_over_bg", "cat_sami", "cat_bella", "cat_nabi", "cat_gureumi", "cat_hodu", "cat_group_main", "loading_cat",
                "logo_whisker_tales", "tile_yarn", "tile_fish", "tile_milk", "tile_catnip", "tile_pawprint", "tile_fishbone", "tile_rocket", "tile_bomb", "tile_rainbow",
                "nav_bar_bg", "tab_home", "tab_catroom", "tab_cafe", "tab_arcade", "tab_meditation", "btn_back", "btn_settings", "btn_home", "btn_restart", "btn_share", "btn_camera", "btn_pause", "btn_next", "btn_retry", "btn_restore", "btn_confirm", "btn_skip",
                "nyangi_heart", "nyangi_heart_plus1", "icon_coin", "icon_lock", "icon_star_filled", "icon_star_empty", "icon_paw", "icon_heart", "highlight_ring", "hint_tap", "hint_swipe", "arrow_right", "arrow_down", "tutorial_bubble", "detox_card_bg", "emotion_happy", "emotion_love", "emotion_sleepy", "emotion_excited", "emotion_sad", "emotion_surprised"
            };
        }

        private static void AutoBindScreenSprites(ScreenRefs refs)
        {
            // Prefab creation already assigns sprites by object names and keys.
            // This method exists as an extension point for future stricter binding.
        }

        private static void AutoBindBottomNavs(ScreenRefs refs, PhoneVisibleSceneInstaller installer)
        {
            BottomNavRuntimeBinder[] binders = UnityEngine.Object.FindObjectsOfType<BottomNavRuntimeBinder>(true);

            for (int i = 0; i < binders.Length; i++)
            {
                SerializedObject so = new SerializedObject(binders[i]);
                so.FindProperty("installer").objectReferenceValue = installer;
                AssignButton(so, "homeButton", binders[i].transform, "Tab_Home");
                AssignButton(so, "catRoomButton", binders[i].transform, "Tab_CatRoom");
                AssignButton(so, "cafeButton", binders[i].transform, "Tab_Cafe");
                AssignButton(so, "arcadeButton", binders[i].transform, "Tab_Arcade");
                AssignButton(so, "meditationButton", binders[i].transform, "Tab_Meditation");
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(binders[i]);
            }
        }

        private static void AssignButton(SerializedObject so, string propertyName, Transform root, string childName)
        {
            Transform child = FindChildRecursive(root, childName);

            if (child == null)
            {
                return;
            }

            Button button = child.GetComponent<Button>();
            so.FindProperty(propertyName).objectReferenceValue = button;
        }

        private static Transform FindChildRecursive(Transform root, string name)
        {
            if (root == null)
            {
                return null;
            }

            if (root.name == name)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindChildRecursive(root.GetChild(i), name);

                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static void ConnectInstaller(PhoneVisibleSceneInstaller installer, Canvas canvas, RectTransform safeArea, RectTransform screensRoot, RectTransform popupsRoot, RectTransform persistentRoot, ScreenRefs refs)
        {
            SerializedObject so = new SerializedObject(installer);
            SetObject(so, "canvas", canvas);
            SetObject(so, "safeAreaRoot", safeArea);
            SetObject(so, "screensRoot", screensRoot);
            SetObject(so, "popupsRoot", popupsRoot);
            SetObject(so, "persistentRoot", persistentRoot);
            SetObject(so, "mainTitle", refs.mainTitle);
            SetObject(so, "gameplay", refs.gameplay);
            SetObject(so, "catBonding", refs.catBonding);
            SetObject(so, "cafeRestoration", refs.cafeRestoration);
            SetObject(so, "arcade", refs.arcade);
            SetObject(so, "meditation", refs.meditation);
            SetObject(so, "settings", refs.settings);
            SetObject(so, "levelClear", refs.levelClear);
            SetObject(so, "gameOver", refs.gameOver);
            SetObject(so, "tutorial", refs.tutorial);
            SetObject(so, "loading", refs.loading);
            SetObject(so, "detoxModal", refs.detox);
            SetObject(so, "sleepMode", refs.sleep);
            SetObject(so, "idleReward", refs.idleReward);
            SetObject(so, "referral", refs.referral);
            SetObject(so, "photoStudio", refs.photoStudio);
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(installer);
        }

        private static void SetObject(SerializedObject so, string property, UnityEngine.Object value)
        {
            SerializedProperty prop = so.FindProperty(property);

            if (prop != null)
            {
                prop.objectReferenceValue = value;
            }
        }

        private sealed class ScreenRefs
        {
            public MainTitleScreenController mainTitle;
            public GameplayUIScreenController gameplay;
            public CatBondingScreenController catBonding;
            public CafeRestorationScreenController cafeRestoration;
            public ArcadeScreenController arcade;
            public MeditationGardenScreenController meditation;
            public SettingsScreenController settings;
            public LevelClearScreenController levelClear;
            public GameOverScreenController gameOver;
            public TutorialOverlayController tutorial;
            public LoadingScreenController loading;
            public DetoxModalController detox;
            public SleepModeScreenController sleep;
            public IdleRewardModalController idleReward;
            public ReferralShareScreenController referral;
            public PhotoStudioScreenController photoStudio;
        }

        private struct RectSpec
        {
            public Vector2 anchorMin;
            public Vector2 anchorMax;
            public Vector2 pivot;
            public Vector2 anchoredPosition;
            public Vector2 sizeDelta;
            public Vector2 offsetMin;
            public Vector2 offsetMax;
        }
    }
}
#endif
