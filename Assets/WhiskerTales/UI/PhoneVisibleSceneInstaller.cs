using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WhiskerTales.Core;

namespace WhiskerTales.UI
{
    public sealed class PhoneVisibleSceneInstaller : MonoBehaviour
    {
        [Header("Roots")]
        [SerializeField] private Canvas canvas;
        [SerializeField] private RectTransform safeAreaRoot;
        [SerializeField] private RectTransform screensRoot;
        [SerializeField] private RectTransform popupsRoot;
        [SerializeField] private RectTransform persistentRoot;

        [Header("Screens")]
        [SerializeField] private MainTitleScreenController mainTitle;
        [SerializeField] private GameplayUIScreenController gameplay;
        [SerializeField] private CatBondingScreenController catBonding;
        [SerializeField] private CafeRestorationScreenController cafeRestoration;
        [SerializeField] private ArcadeScreenController arcade;
        [SerializeField] private MeditationGardenScreenController meditation;
        [SerializeField] private SettingsScreenController settings;
        [SerializeField] private LoadingScreenController loading;
        [SerializeField] private DetoxModalController detoxModal;
        [SerializeField] private SleepModeScreenController sleepMode;
        [SerializeField] private LevelClearScreenController levelClear;
        [SerializeField] private GameOverScreenController gameOver;
        [SerializeField] private TutorialOverlayController tutorial;
        [SerializeField] private IdleRewardModalController idleReward;
        [SerializeField] private ReferralShareScreenController referral;
        [SerializeField] private PhotoStudioScreenController photoStudio;

        private UIAssetRegistryRuntime assets;
        private readonly List<UIScreenBase> screens = new List<UIScreenBase>();

        private void Awake()
        {
            assets = UIAssetRegistryRuntime.Instance;

            if (assets == null)
            {
                DebugLogger.Warning(LogCategory.UI, "PhoneVisibleSceneInstaller could not find UIAssetRegistryRuntime.");
            }

            ConfigureCanvas();
            RegisterScreens();
            ApplyInitialSprites();
            HideAllScreens();
        }

        private void Start()
        {
            ShowLoadingThenHome();
        }

        private void ConfigureCanvas()
        {
            if (canvas == null)
            {
                canvas = FindObjectOfType<Canvas>();
            }

            if (canvas == null)
            {
                DebugLogger.Warning(LogCategory.UI, "No Canvas found in scene.");
                return;
            }

            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();

            if (scaler == null)
            {
                scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            }

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        private void RegisterScreens()
        {
            screens.Clear();

            AddScreen(mainTitle);
            AddScreen(gameplay);
            AddScreen(catBonding);
            AddScreen(cafeRestoration);
            AddScreen(arcade);
            AddScreen(meditation);
            AddScreen(settings);
            AddScreen(loading);
            AddScreen(detoxModal);
            AddScreen(sleepMode);
            AddScreen(levelClear);
            AddScreen(gameOver);
            AddScreen(tutorial);
            AddScreen(idleReward);
            AddScreen(referral);
            AddScreen(photoStudio);
        }

        private void AddScreen(UIScreenBase screen)
        {
            if (screen == null)
            {
                return;
            }

            screens.Add(screen);
        }

        private void HideAllScreens()
        {
            for (int i = 0; i < screens.Count; i++)
            {
                if (screens[i] != null)
                {
                    screens[i].Hide();
                }
            }
        }

        private void ShowLoadingThenHome()
        {
            HideAllScreens();

            if (loading != null)
            {
                loading.Show();
                loading.SetProgress(0.1f);
                Invoke(nameof(ShowHome), 0.8f);
                return;
            }

            ShowHome();
        }

        public void ShowHome()
        {
            HideAllScreens();

            if (mainTitle != null)
            {
                mainTitle.Show();
            }
        }

        public void ShowGameplay()
        {
            HideAllScreens();

            if (gameplay != null)
            {
                gameplay.Show();
            }
        }

        public void ShowCatBonding()
        {
            HideAllScreens();

            if (catBonding != null)
            {
                catBonding.Show();
                catBonding.SetCat(GetSprite("cat_nabi"), "나비", 1, 0.35f, "나비가 조용히 당신을 바라보고 있어요.");
            }
        }

        public void ShowCafeRestoration()
        {
            HideAllScreens();

            if (cafeRestoration != null)
            {
                cafeRestoration.Show();
                cafeRestoration.SetTotalStars(12);
                cafeRestoration.SetZoneProgress(2, 0, 0);
                cafeRestoration.ConfigureCard(0, "낡은 간판 복원", "필요 별 3개", false, () => DebugLogger.Info(LogCategory.UI, "Restore card 0"));
                cafeRestoration.ConfigureCard(1, "마당 잡초 정리", "필요 별 4개", false, () => DebugLogger.Info(LogCategory.UI, "Restore card 1"));
                cafeRestoration.ConfigureCard(2, "등불 다시 걸기", "잠김", true, null);
            }
        }

        public void ShowMeditation()
        {
            HideAllScreens();

            if (meditation != null)
            {
                meditation.Show();
                meditation.SetPeacePoints(24);
            }
        }

        private void ApplyInitialSprites()
        {
            ApplyBackground(mainTitle, "bg_zone1_stage5");
            ApplyBackground(gameplay, "bg_zone1_stage1");
            ApplyBackground(catBonding, "bg_zone1_stage4");
            ApplyBackground(cafeRestoration, "bg_zone2_stage2");
            ApplyBackground(meditation, "bg_zone3_stage5");
            ApplyBackground(loading, "bg_zone1_stage5");
            ApplyBackground(sleepMode, "bg_zone3_stage5");
            ApplyBackground(levelClear, "level_clear_bg");
            ApplyBackground(gameOver, "game_over_bg");
        }

        private void ApplyBackground(Component root, string key)
        {
            if (root == null)
            {
                return;
            }

            Image[] images = root.GetComponentsInChildren<Image>(true);
            Sprite sprite = GetSprite(key);

            if (sprite == null)
            {
                return;
            }

            for (int i = 0; i < images.Length; i++)
            {
                Image image = images[i];

                if (image == null)
                {
                    continue;
                }

                if (image.gameObject.name.Contains("BG") == true || image.gameObject.name.Contains("Background") == true)
                {
                    image.sprite = sprite;
                    image.type = Image.Type.Simple;
                    image.preserveAspect = false;
                    return;
                }
            }
        }

        private Sprite GetSprite(string key)
        {
            if (assets == null)
            {
                return null;
            }

            return assets.GetSprite(key);
        }
    }
}
