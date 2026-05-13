#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WhiskerTales.Platform;
using WhiskerTales.Puzzle;
using WhiskerTales.Runtime;
using WhiskerTales.UI;
using WhiskerTales.UI.Screens;

namespace WhiskerTales.EditorTools
{
    public static class MainAppSceneBuilder
    {
        private const string ScenePath = "Assets/WhiskerTales/_Project/Scenes/Main_App.unity";
        private const string SceneDirectory = "Assets/WhiskerTales/_Project/Scenes";

        // Reference resolution shared with Boot_Persistent.GlobalOverlayCanvas so DPI math matches.
        private static readonly Vector2 ReferenceResolution = new Vector2(1080f, 2400f);

        // Sorting orders for the 4-layer canvas stack. Boot_Persistent.GlobalOverlayCanvas sits at 1000
        // above everything in Main_App; toasts inside Main_App are app-scoped and stay below that.
        private const int SortBackground = 0;
        private const int SortScreens = 100;
        private const int SortModals = 200;
        private const int SortToasts = 300;

        [InitializeOnLoadMethod]
        private static void AutoBuildOnEditorLoad()
        {
            if (IsPlayModeActive() == true)
            {
                return;
            }
            if (File.Exists(ScenePath) == true)
            {
                return;
            }
            EditorApplication.delayCall += TryAutoBuild;
        }

        private static void TryAutoBuild()
        {
            if (IsPlayModeActive() == true)
            {
                return;
            }
            if (EditorApplication.isCompiling == true || EditorApplication.isUpdating == true)
            {
                EditorApplication.delayCall += TryAutoBuild;
                return;
            }
            if (File.Exists(ScenePath) == true)
            {
                return;
            }
            BuildInternal("Auto-generated");
        }

        [MenuItem("Whisker Tales/V2/Build Main_App Scene")]
        public static void Build()
        {
            if (IsPlayModeActive() == true)
            {
                Debug.LogWarning("[MainAppSceneBuilder] Skipped: cannot build scene while in Play Mode. Stop playing first.");
                return;
            }
            BuildInternal("Rebuilt");
        }

        private static bool IsPlayModeActive()
        {
            if (EditorApplication.isPlaying == true)
            {
                return true;
            }
            if (EditorApplication.isPlayingOrWillChangePlaymode == true)
            {
                return true;
            }
            return false;
        }

        private static void BuildInternal(string verb)
        {
            if (Directory.Exists(SceneDirectory) == false)
            {
                Directory.CreateDirectory(SceneDirectory);
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);

            GameObject bootstrapGo = CreateBootstrap(scene);
            CreateEventSystem(scene);
            CreateTileSpriteRegistry(scene);

            GameObject uiRoot = new GameObject("UIRoot");
            SceneManager.MoveGameObjectToScene(uiRoot, scene);

            Canvas backgroundCanvas = CreateCanvas(uiRoot.transform, "Canvas_Background", SortBackground, false);
            Canvas screensCanvas = CreateCanvas(uiRoot.transform, "Canvas_Screens", SortScreens, true);
            Canvas modalsCanvas = CreateCanvas(uiRoot.transform, "Canvas_Modals", SortModals, true);
            Canvas toastsCanvas = CreateCanvas(uiRoot.transform, "Canvas_Toasts", SortToasts, true);

            ScreenNavigator navigator = AttachNavigator(screensCanvas.gameObject);

            InstallScreens(navigator, screensCanvas);

            WireBootstrap(bootstrapGo, navigator, backgroundCanvas, screensCanvas, modalsCanvas, toastsCanvas);

            bool saved = EditorSceneManager.SaveScene(scene, ScenePath);
            EditorSceneManager.CloseScene(scene, true);

            if (saved == false)
            {
                Debug.LogError("[MainAppSceneBuilder] Failed to save scene at " + ScenePath);
                return;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[MainAppSceneBuilder] " + verb + " Main_App.unity at " + ScenePath
                + ". Add this scene after Boot_Persistent in Build Settings (index 1).");
        }

        private static GameObject CreateBootstrap(Scene targetScene)
        {
            GameObject go = new GameObject("MainAppBootstrap");
            go.AddComponent<MainAppBootstrap>();
            SceneManager.MoveGameObjectToScene(go, targetScene);
            return go;
        }

        private static void CreateTileSpriteRegistry(Scene targetScene)
        {
            GameObject go = new GameObject("V2TileSpriteRegistry");
            V2TileSpriteRegistry registry = go.AddComponent<V2TileSpriteRegistry>();
            SceneManager.MoveGameObjectToScene(go, targetScene);

            SerializedObject so = new SerializedObject(registry);
            AssignTile(so, "tileFish",     "Assets/WhiskerTales/Art/Tiles/tile_fish.png");
            AssignTile(so, "tileMilk",     "Assets/WhiskerTales/Art/Tiles/tile_milk.png");
            AssignTile(so, "tileYarn",     "Assets/WhiskerTales/Art/Tiles/tile_yarn.png");
            AssignTile(so, "tileCatnip",   "Assets/WhiskerTales/Art/Tiles/tile_catnip.png");
            AssignTile(so, "tilePawprint", "Assets/WhiskerTales/Art/Tiles/tile_pawprint.png");
            AssignTile(so, "tileFishbone", "Assets/WhiskerTales/Art/Tiles/tile_fishbone.png");
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AssignTile(SerializedObject so, string field, string path)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

            if (sprite == null)
            {
                Debug.LogWarning("[MainAppSceneBuilder] Tile sprite missing: " + path);
                return;
            }

            SerializedProperty prop = so.FindProperty(field);

            if (prop != null)
            {
                prop.objectReferenceValue = sprite;
            }
        }

        private static void CreateEventSystem(Scene targetScene)
        {
            // Boot_Persistent already places an EventSystem and TouchInputGuard prunes duplicates,
            // but Main_App keeps its own so it remains playable when opened solo in the editor.
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
            SceneManager.MoveGameObjectToScene(es, targetScene);
        }

        private static Canvas CreateCanvas(Transform parent, string name, int sortingOrder, bool addSafeArea)
        {
            GameObject canvasGo = new GameObject(name);
            canvasGo.transform.SetParent(parent, false);

            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;

            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.matchWidthOrHeight = 0.5f;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

            canvasGo.AddComponent<GraphicRaycaster>();

            if (addSafeArea == true)
            {
                GameObject safeArea = new GameObject("SafeAreaRoot", typeof(RectTransform));
                RectTransform safeRect = safeArea.GetComponent<RectTransform>();
                safeRect.SetParent(canvasGo.transform, false);
                safeRect.anchorMin = Vector2.zero;
                safeRect.anchorMax = Vector2.one;
                safeRect.offsetMin = Vector2.zero;
                safeRect.offsetMax = Vector2.zero;
                safeArea.AddComponent<SafeAreaController>();
            }

            return canvas;
        }

        private static ScreenNavigator AttachNavigator(GameObject host)
        {
            ScreenNavigator navigator = host.AddComponent<ScreenNavigator>();
            SetNavigatorInitialScreen(navigator, string.Empty);
            return navigator;
        }

        private static void SetNavigatorInitialScreen(ScreenNavigator navigator, string id)
        {
            SerializedObject so = new SerializedObject(navigator);
            SerializedProperty initialId = so.FindProperty("initialScreenId");

            if (initialId != null)
            {
                initialId.stringValue = id;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void InstallScreens(ScreenNavigator navigator, Canvas screensCanvas)
        {
            Transform safeArea = screensCanvas.transform.Find("SafeAreaRoot");
            Transform screenParent = safeArea != null ? safeArea : screensCanvas.transform;

            UIScreenBase home = InstantiateScreen(HomeScreenPrefabBuilder.PrefabPath, screenParent, navigator);
            UIScreenBase levelSelect = InstantiateScreen(V2ScreenPrefabBuilders.LevelSelectPrefabPath, screenParent, navigator);
            UIScreenBase gameplay = InstantiateScreen(V2ScreenPrefabBuilders.GameplayPrefabPath, screenParent, navigator);
            UIScreenBase catRoom = InstantiateScreen(V2ScreenPrefabBuilders.CatRoomPrefabPath, screenParent, navigator);
            UIScreenBase cafe = InstantiateScreen(V2ScreenPrefabBuilders.CafePrefabPath, screenParent, navigator);
            UIScreenBase levelClear = InstantiateScreen(V2ScreenPrefabBuilders.LevelClearPrefabPath, screenParent, navigator);
            UIScreenBase gameFail = InstantiateScreen(V2ScreenPrefabBuilders.GameFailPrefabPath, screenParent, navigator);
            UIScreenBase settings = InstantiateScreen(V2ScreenPrefabBuilders.SettingsPrefabPath, screenParent, navigator);
            UIScreenBase sleepMode = InstantiateScreen(V2ScreenPrefabBuilders.SleepModePrefabPath, screenParent, navigator);

            RegisterIfNotNull(navigator, home);
            RegisterIfNotNull(navigator, levelSelect);
            RegisterIfNotNull(navigator, gameplay);
            RegisterIfNotNull(navigator, catRoom);
            RegisterIfNotNull(navigator, cafe);
            RegisterIfNotNull(navigator, levelClear);
            RegisterIfNotNull(navigator, gameFail);
            RegisterIfNotNull(navigator, settings);
            RegisterIfNotNull(navigator, sleepMode);

            // V2-16: scene-level tab bar (sibling of all screens, renders on top). Visible only on
            // home/catroom/cafe/sleepmode/settings via TabBarController.
            CreateSceneTabBar(screenParent, navigator);

            if (home != null)
            {
                SetNavigatorInitialScreen(navigator, "home");
            }
            else
            {
                Debug.LogWarning("[MainAppSceneBuilder] HomeScreen prefab missing — scene built without home screen. Run Whisker Tales/V2/Build HomeScreen Prefab first.");
            }
        }

        private static UIScreenBase InstantiateScreen(string prefabPath, Transform parent, ScreenNavigator navigator)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefab == null)
            {
                Debug.LogWarning("[MainAppSceneBuilder] Prefab missing — skipping: " + prefabPath);
                return null;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            UIScreenBase screen = instance.GetComponent<UIScreenBase>();

            if (screen == null)
            {
                Debug.LogWarning("[MainAppSceneBuilder] Prefab has no UIScreenBase component: " + prefabPath);
                return null;
            }

            AssignNavigator(screen, navigator);
            return screen;
        }

        private static void AssignNavigator(UIScreenBase screen, ScreenNavigator navigator)
        {
            if (screen == null || navigator == null)
            {
                return;
            }

            SerializedObject so = new SerializedObject(screen);
            SerializedProperty navProp = so.FindProperty("navigator");

            if (navProp != null)
            {
                navProp.objectReferenceValue = navigator;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void RegisterIfNotNull(ScreenNavigator navigator, UIScreenBase screen)
        {
            if (screen == null)
            {
                return;
            }

            RegisterScreen(navigator, screen);
        }

        private static void CreateSceneTabBar(Transform parent, ScreenNavigator navigator)
        {
            // Bar container at bottom of SafeAreaRoot, full width, 200 tall.
            GameObject bar = new GameObject("TabBar", typeof(RectTransform), typeof(CanvasGroup), typeof(CanvasRenderer), typeof(Image));
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

            CanvasGroup group = bar.GetComponent<CanvasGroup>();
            group.alpha = 1f;
            group.interactable = true;
            group.blocksRaycasts = true;

            (Button homeBtn, Image homeImg) = CreateTab(barRect, "Tab_Home", "Assets/WhiskerTales/Art/UI/tab_home.png", new Vector2(-360f, 20f));
            (Button catRoomBtn, Image catRoomImg) = CreateTab(barRect, "Tab_CatRoom", "Assets/WhiskerTales/Art/UI/tab_catroom.png", new Vector2(-120f, 20f));
            (Button cafeBtn, Image cafeImg) = CreateTab(barRect, "Tab_Cafe", "Assets/WhiskerTales/Art/UI/tab_cafe.png", new Vector2(120f, 20f));
            (Button medBtn, Image medImg) = CreateTab(barRect, "Tab_Meditation", "Assets/WhiskerTales/Art/UI/tab_meditation.png", new Vector2(360f, 20f));

            TabBarController controller = bar.AddComponent<TabBarController>();
            SerializedObject so = new SerializedObject(controller);

            SetReferenceGeneric(so, "navigator", navigator);
            SetReferenceGeneric(so, "canvasGroup", group);

            SerializedProperty tabs = so.FindProperty("tabs");

            if (tabs != null)
            {
                tabs.arraySize = 4;
                AssignTab(tabs.GetArrayElementAtIndex(0), "home", homeBtn, homeImg);
                AssignTab(tabs.GetArrayElementAtIndex(1), "catroom", catRoomBtn, catRoomImg);
                AssignTab(tabs.GetArrayElementAtIndex(2), "cafe", cafeBtn, cafeImg);
                AssignTab(tabs.GetArrayElementAtIndex(3), "sleepmode", medBtn, medImg);
            }

            SerializedProperty visible = so.FindProperty("visibleOnScreens");

            if (visible != null)
            {
                string[] ids = new string[] { "home", "catroom", "cafe", "sleepmode", "settings" };
                visible.arraySize = ids.Length;

                for (int i = 0; i < ids.Length; i++)
                {
                    visible.GetArrayElementAtIndex(i).stringValue = ids[i];
                }
            }

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static (Button button, Image image) CreateTab(RectTransform parent, string name, string spritePath, Vector2 anchoredPosition)
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
            return (button, image);
        }

        private static void AssignTab(SerializedProperty element, string screenId, Button button, Image image)
        {
            SerializedProperty idProp = element.FindPropertyRelative("screenId");
            SerializedProperty btnProp = element.FindPropertyRelative("button");
            SerializedProperty imgProp = element.FindPropertyRelative("image");

            if (idProp != null) { idProp.stringValue = screenId; }
            if (btnProp != null) { btnProp.objectReferenceValue = button; }
            if (imgProp != null) { imgProp.objectReferenceValue = image; }
        }

        private static void SetReferenceGeneric(SerializedObject so, string propertyName, Object value)
        {
            SerializedProperty prop = so.FindProperty(propertyName);

            if (prop != null)
            {
                prop.objectReferenceValue = value;
            }
        }

        private static void RegisterScreen(ScreenNavigator navigator, UIScreenBase screen)
        {
            SerializedObject so = new SerializedObject(navigator);
            SerializedProperty screensProp = so.FindProperty("screens");

            if (screensProp == null)
            {
                return;
            }

            int index = screensProp.arraySize;
            screensProp.InsertArrayElementAtIndex(index);
            SerializedProperty element = screensProp.GetArrayElementAtIndex(index);
            element.objectReferenceValue = screen;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WireBootstrap(GameObject bootstrapGo, ScreenNavigator navigator,
            Canvas background, Canvas screens, Canvas modals, Canvas toasts)
        {
            MainAppBootstrap bootstrap = bootstrapGo.GetComponent<MainAppBootstrap>();

            if (bootstrap == null)
            {
                return;
            }

            SerializedObject so = new SerializedObject(bootstrap);
            AssignReference(so, "screenNavigator", navigator);
            AssignReference(so, "backgroundCanvas", background);
            AssignReference(so, "screensCanvas", screens);
            AssignReference(so, "modalsCanvas", modals);
            AssignReference(so, "toastsCanvas", toasts);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AssignReference(SerializedObject so, string propertyName, Object value)
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
