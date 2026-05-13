#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WhiskerTales.Assets;
using WhiskerTales.Core;
using WhiskerTales.Feel;
using WhiskerTales.Pooling;
using WhiskerTales.Puzzle;
using WhiskerTales.Runtime;
using WhiskerTales.Polish;
using WhiskerTales.Save;
using WhiskerTales.UI;
using WhiskerTales.UI.Screens;

namespace WhiskerTales.EditorTools
{
    public static class V2RuntimeTests
    {
        private const string MainAppScenePath = "Assets/WhiskerTales/_Project/Scenes/Main_App.unity";
        private const string BootPersistentScenePath = "Assets/WhiskerTales/_Project/Scenes/Boot_Persistent.unity";

        [MenuItem("Tools/Whisker Tales/Test/V2 - SystemsBootstrap guard exists")]
        public static void TestSystemsBootstrapGuard()
        {
            string path = "Assets/Scripts/Core/SystemsBootstrap.cs";

            if (File.Exists(path) == false)
            {
                Report("V2 SystemsBootstrap guard", false, "File missing: " + path);
                return;
            }

            string body = File.ReadAllText(path);
            bool hasGuardArray = body.Contains("V2EntryScenes");
            bool hasEnsureInit = body.Contains("EnsureInitialized");
            bool hasSceneCheck = body.Contains("IsV2BootEntryScene");
            bool ok = hasGuardArray == true && hasEnsureInit == true && hasSceneCheck == true;

            string detail = "V2EntryScenes=" + hasGuardArray
                + " EnsureInitialized=" + hasEnsureInit
                + " IsV2BootEntryScene=" + hasSceneCheck;
            Report("V2 SystemsBootstrap guard", ok, detail);
        }

        [MenuItem("Tools/Whisker Tales/Test/V2 - Main_App scene exists")]
        public static void TestMainAppSceneExists()
        {
            bool ok = File.Exists(MainAppScenePath);
            Report("V2 Main_App scene", ok, ok == true ? "found at " + MainAppScenePath : "missing: " + MainAppScenePath);
        }

        [MenuItem("Tools/Whisker Tales/Test/V2 - 4-layer Canvas + Navigator")]
        public static void TestCanvasStructure()
        {
            if (File.Exists(MainAppScenePath) == false)
            {
                Report("V2 4-layer Canvas", false, "Main_App.unity missing — run Whisker Tales/V2/Build Main_App Scene first");
                return;
            }

            Scene previous = SceneManager.GetActiveScene();
            Scene loaded = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(MainAppScenePath, UnityEditor.SceneManagement.OpenSceneMode.Additive);

            try
            {
                GameObject uiRoot = FindRootByName(loaded, "UIRoot");

                if (uiRoot == null)
                {
                    Report("V2 4-layer Canvas", false, "UIRoot not found in Main_App scene");
                    return;
                }

                Canvas background = FindChildCanvas(uiRoot, "Canvas_Background");
                Canvas screens = FindChildCanvas(uiRoot, "Canvas_Screens");
                Canvas modals = FindChildCanvas(uiRoot, "Canvas_Modals");
                Canvas toasts = FindChildCanvas(uiRoot, "Canvas_Toasts");
                ScreenNavigator navigator = screens != null ? screens.GetComponent<ScreenNavigator>() : null;

                bool allCanvases = background != null && screens != null && modals != null && toasts != null;
                bool sortingOk = allCanvases == true
                    && background.sortingOrder == 0
                    && screens.sortingOrder == 100
                    && modals.sortingOrder == 200
                    && toasts.sortingOrder == 300;
                bool navigatorOk = navigator != null;
                bool ok = allCanvases == true && sortingOk == true && navigatorOk == true;

                string detail = "BG=" + (background != null) + " Screens=" + (screens != null)
                    + " Modals=" + (modals != null) + " Toasts=" + (toasts != null)
                    + " sortingOk=" + sortingOk + " navigator=" + navigatorOk;
                Report("V2 4-layer Canvas", ok, detail);
            }
            finally
            {
                UnityEditor.SceneManagement.EditorSceneManager.CloseScene(loaded, true);

                if (previous.IsValid() == true)
                {
                    SceneManager.SetActiveScene(previous);
                }
            }
        }

        [MenuItem("Tools/Whisker Tales/Test/V2 - Services connect (Play Mode)")]
        public static void TestServicesConnect()
        {
            if (EditorApplication.isPlaying == false)
            {
                Report("V2 Services connect", false, "Enter Play Mode in Boot_Persistent or Main_App first");
                return;
            }

            SystemsBootstrap.EnsureInitialized();

            bool saveOk = SystemsBootstrap.SaveService != null;
            bool assetsOk = SystemsBootstrap.AssetProvider != null;
            bool audioOk = Object.FindObjectOfType<AudioService>() != null;
            bool hapticOk = Object.FindObjectOfType<HapticManager>() != null;
            bool particleOk = Object.FindObjectOfType<ParticlePoolManager>() != null;
            bool tilePoolOk = Object.FindObjectOfType<TilePool>() != null;
            bool hintOk = Object.FindObjectOfType<HintSystem>() != null;
            bool ok = saveOk && assetsOk && audioOk && hapticOk && particleOk && tilePoolOk && hintOk;

            string detail = "save=" + saveOk + " assets=" + assetsOk
                + " audio=" + audioOk + " haptic=" + hapticOk + " particle=" + particleOk
                + " tilePool=" + tilePoolOk + " hint=" + hintOk;
            Report("V2 Services connect", ok, detail);
        }

        [MenuItem("Tools/Whisker Tales/Test/V2 - Boot_Persistent scene exists")]
        public static void TestBootPersistentExists()
        {
            bool ok = File.Exists(BootPersistentScenePath);
            Report("V2 Boot_Persistent scene", ok, ok == true ? "found" : "missing: " + BootPersistentScenePath);
        }

        [MenuItem("Tools/Whisker Tales/Test/V2-10 - Lantern texture + Settings prefab")]
        public static void TestV210()
        {
            bool lanternOk = File.Exists("Assets/WhiskerTales/_Project/Art/Generated/lantern_glow.png");
            bool settingsOk = File.Exists(V2ScreenPrefabBuilders.SettingsPrefabPath);
            bool ok = lanternOk && settingsOk;
            Report("V2-10 lantern+settings", ok, "lanternTex=" + lanternOk + " settingsPrefab=" + settingsOk);
        }

        [MenuItem("Tools/Whisker Tales/Test/V2-16 - Home cleanup + scene tabbar")]
        public static void TestV216()
        {
            string homePath = "Assets/WhiskerTales/_Project/Prefabs/UI/Screens/HomeScreen.prefab";
            GameObject home = AssetDatabase.LoadAssetAtPath<GameObject>(homePath);

            bool noPlay = true;

            if (home != null)
            {
                Button[] buttons = home.GetComponentsInChildren<Button>(true);

                for (int i = 0; i < buttons.Length; i++)
                {
                    if (buttons[i] != null && buttons[i].gameObject.name.Contains("Btn_Play") == true)
                    {
                        noPlay = false;
                        break;
                    }
                }
            }

            // Scene-level tab bar check requires opening Main_App.
            bool tabBarOk = false;
            int tabCount = 0;

            if (File.Exists(MainAppScenePath) == true)
            {
                Scene previous = SceneManager.GetActiveScene();
                Scene loaded = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(MainAppScenePath, UnityEditor.SceneManagement.OpenSceneMode.Additive);

                try
                {
                    TabBarController controller = Object.FindObjectOfType<TabBarController>();
                    tabBarOk = controller != null;

                    if (tabBarOk == true)
                    {
                        Button[] tabs = controller.GetComponentsInChildren<Button>(true);
                        tabCount = tabs.Length;
                    }
                }
                finally
                {
                    UnityEditor.SceneManagement.EditorSceneManager.CloseScene(loaded, true);
                    if (previous.IsValid() == true) { SceneManager.SetActiveScene(previous); }
                }
            }

            bool ok = noPlay && tabBarOk && tabCount == 4;
            Report("V2-16 home cleanup", ok, "noPlayBtn=" + noPlay + " sceneTabBar=" + tabBarOk + " tabs=" + tabCount);
        }

        [MenuItem("Tools/Whisker Tales/Test/V2-13 - Tab bar + CatRoom life layers")]
        public static void TestV213()
        {
            bool sleepOk = File.Exists(V2ScreenPrefabBuilders.SleepModePrefabPath);

            string homePath = "Assets/WhiskerTales/_Project/Prefabs/UI/Screens/HomeScreen.prefab";
            string catPath = "Assets/WhiskerTales/_Project/Prefabs/UI/Screens/CatRoomScreen.prefab";
            GameObject home = AssetDatabase.LoadAssetAtPath<GameObject>(homePath);
            GameObject cat = AssetDatabase.LoadAssetAtPath<GameObject>(catPath);

            bool tabBarOk = false;
            int tabCount = 0;

            if (home != null)
            {
                Transform tabBar = home.transform.Find("TabBar");

                if (tabBar != null)
                {
                    tabBarOk = true;
                    tabCount = tabBar.GetComponentsInChildren<Button>(true).Length;
                }
            }

            bool dustOk = false;
            bool sunlightOk = false;
            bool gazeOk = false;

            if (cat != null)
            {
                dustOk = cat.GetComponentInChildren<WhiskerTales.Polish.DustMoteEmitter>(true) != null;
                sunlightOk = cat.GetComponentInChildren<WhiskerTales.Polish.SunlightDriftLayer>(true) != null;
                gazeOk = cat.GetComponentInChildren<WhiskerTales.Polish.CatGazeReaction>(true) != null;
            }

            bool ok = sleepOk && tabBarOk && tabCount >= 4 && dustOk && sunlightOk && gazeOk;
            Report("V2-13 tabbar+catlife", ok,
                "sleep=" + sleepOk + " tabBar=" + tabBarOk + " tabs=" + tabCount
                + " dust=" + dustOk + " sun=" + sunlightOk + " gaze=" + gazeOk);
        }

        [MenuItem("Tools/Whisker Tales/Test/V2-12 - Cat alpha + tile sprites")]
        public static void TestV212()
        {
            bool catAlphaOk = File.Exists(CatAlphaProcessor.OutputPath);

            string[] tilePaths = new string[]
            {
                "Assets/WhiskerTales/Art/Tiles/tile_fish.png",
                "Assets/WhiskerTales/Art/Tiles/tile_milk.png",
                "Assets/WhiskerTales/Art/Tiles/tile_yarn.png",
                "Assets/WhiskerTales/Art/Tiles/tile_catnip.png",
                "Assets/WhiskerTales/Art/Tiles/tile_pawprint.png",
                "Assets/WhiskerTales/Art/Tiles/tile_fishbone.png"
            };

            int tilesFound = 0;

            for (int i = 0; i < tilePaths.Length; i++)
            {
                if (File.Exists(tilePaths[i]) == true)
                {
                    tilesFound++;
                }
            }

            bool ok = catAlphaOk && tilesFound == 6;
            Report("V2-12 cat alpha+tiles", ok, "catAlpha=" + catAlphaOk + " tilePngs=" + tilesFound + "/6");
        }

        [MenuItem("Tools/Whisker Tales/Test/V2-11 - Cat anim + petal layers + logo alpha")]
        public static void TestV211()
        {
            bool logoOk = File.Exists("Assets/WhiskerTales/_Project/Art/Generated/logo_whisker_tales_alpha.png");

            string homePath = "Assets/WhiskerTales/_Project/Prefabs/UI/Screens/HomeScreen.prefab";
            string catPath = "Assets/WhiskerTales/_Project/Prefabs/UI/Screens/CatRoomScreen.prefab";

            GameObject home = AssetDatabase.LoadAssetAtPath<GameObject>(homePath);
            GameObject cat = AssetDatabase.LoadAssetAtPath<GameObject>(catPath);

            int petalLayers = 0;
            bool catAnimOk = false;

            if (home != null)
            {
                AmbientPetalEmitter[] emitters = home.GetComponentsInChildren<AmbientPetalEmitter>(true);
                petalLayers = emitters.Length;
            }

            if (cat != null)
            {
                CatIdleLifeController anim = cat.GetComponentInChildren<CatIdleLifeController>(true);
                catAnimOk = anim != null;
            }

            bool ok = logoOk && petalLayers >= 3 && catAnimOk;
            Report("V2-11 cat+petals+logo", ok, "logoAlpha=" + logoOk + " petalLayers=" + petalLayers + " catAnim=" + catAnimOk);
        }

        [MenuItem("Tools/Whisker Tales/Test/V2-9 - HomeScreen polish FX present")]
        public static void TestV29Polish()
        {
            string prefabPath = "Assets/WhiskerTales/_Project/Prefabs/UI/Screens/HomeScreen.prefab";

            if (File.Exists(prefabPath) == false)
            {
                Report("V2-9 HomeScreen polish", false, "HomeScreen prefab missing");
                return;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefab == null)
            {
                Report("V2-9 HomeScreen polish", false, "AssetDatabase load failed");
                return;
            }

            LanternGlowPulse lantern = prefab.GetComponentInChildren<LanternGlowPulse>(true);
            AmbientPetalEmitter emitter = prefab.GetComponentInChildren<AmbientPetalEmitter>(true);

            bool lanternOk = lantern != null;
            bool emitterOk = emitter != null;
            int petalCount = 0;

            if (emitterOk == true)
            {
                SerializedObject so = new SerializedObject(emitter);
                SerializedProperty list = so.FindProperty("petalSprites");

                if (list != null)
                {
                    petalCount = list.arraySize;
                }
            }

            bool petalsOk = petalCount >= 4;
            bool ok = lanternOk && emitterOk && petalsOk;
            Report("V2-9 HomeScreen polish", ok, "lantern=" + lanternOk + " emitter=" + emitterOk + " petalSprites=" + petalCount);
        }

        [MenuItem("Tools/Whisker Tales/Test/V2-9 - Player progress save")]
        public static void TestV29Progress()
        {
            int unlocked = PlayerProgressService.MaxUnlockedLevel;
            bool baseUnlocked = PlayerProgressService.IsLevelUnlocked(1);
            bool ok = unlocked >= 1 && baseUnlocked == true;
            Report("V2-9 Player progress", ok, "maxUnlocked=" + unlocked + " level1Unlocked=" + baseUnlocked);
        }

        [MenuItem("Tools/Whisker Tales/Test/V2-6 - All 5 screen prefabs exist")]
        public static void TestV26Prefabs()
        {
            string[] paths = new string[]
            {
                V2ScreenPrefabBuilders.GameplayPrefabPath,
                V2ScreenPrefabBuilders.CatRoomPrefabPath,
                V2ScreenPrefabBuilders.CafePrefabPath,
                V2ScreenPrefabBuilders.LevelClearPrefabPath,
                V2ScreenPrefabBuilders.GameFailPrefabPath,
                V2ScreenPrefabBuilders.LevelSelectPrefabPath
            };

            int present = 0;
            System.Text.StringBuilder detail = new System.Text.StringBuilder();

            for (int i = 0; i < paths.Length; i++)
            {
                bool exists = File.Exists(paths[i]);

                if (exists == true)
                {
                    present++;
                }

                detail.Append(Path.GetFileNameWithoutExtension(paths[i]));
                detail.Append("=");
                detail.Append(exists);
                detail.Append(" ");
            }

            bool ok = present == paths.Length;
            Report("V2-6 5 screen prefabs", ok, present + "/" + paths.Length + " " + detail.ToString());
        }

        [MenuItem("Tools/Whisker Tales/Test/V2-5 - HomeScreen prefab + wiring")]
        public static void TestHomeScreenWiring()
        {
            string prefabPath = "Assets/WhiskerTales/_Project/Prefabs/UI/Screens/HomeScreen.prefab";

            if (File.Exists(prefabPath) == false)
            {
                Report("V2-5 HomeScreen prefab", false, "missing: " + prefabPath);
                return;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefab == null)
            {
                Report("V2-5 HomeScreen prefab", false, "AssetDatabase load failed");
                return;
            }

            HomeScreenController controller = prefab.GetComponent<HomeScreenController>();
            bool prefabOk = controller != null;
            bool buttonsOk = false;

            if (prefabOk == true)
            {
                Button[] buttons = prefab.GetComponentsInChildren<Button>(true);
                buttonsOk = buttons.Length >= 4;
            }

            if (File.Exists(MainAppScenePath) == false)
            {
                Report("V2-5 HomeScreen prefab", prefabOk && buttonsOk,
                    "prefab=" + prefabOk + " buttons>=4=" + buttonsOk + " (Main_App scene missing — wiring check skipped)");
                return;
            }

            Scene previous = SceneManager.GetActiveScene();
            Scene loaded = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(MainAppScenePath, UnityEditor.SceneManagement.OpenSceneMode.Additive);

            try
            {
                ScreenNavigator nav = Object.FindObjectOfType<ScreenNavigator>();
                HomeScreenController sceneHome = Object.FindObjectOfType<HomeScreenController>();
                bool navOk = nav != null;
                bool sceneHomeOk = sceneHome != null;
                bool ok = prefabOk && buttonsOk && navOk && sceneHomeOk;

                string detail = "prefab=" + prefabOk + " buttons>=4=" + buttonsOk
                    + " navigator=" + navOk + " homeInScene=" + sceneHomeOk;
                Report("V2-5 HomeScreen prefab", ok, detail);
            }
            finally
            {
                UnityEditor.SceneManagement.EditorSceneManager.CloseScene(loaded, true);

                if (previous.IsValid() == true)
                {
                    SceneManager.SetActiveScene(previous);
                }
            }
        }

        [MenuItem("Tools/Whisker Tales/Test/V2 - Run all")]
        public static void RunAll()
        {
            Debug.Log("[V2RuntimeTests] === Running all V2 tests ===");
            TestSystemsBootstrapGuard();
            TestBootPersistentExists();
            TestMainAppSceneExists();
            TestCanvasStructure();
            TestHomeScreenWiring();
            TestV26Prefabs();
            TestV29Polish();
            TestV29Progress();
            TestV210();
            TestV211();
            TestV212();
            TestV213();
            TestV216();

            if (EditorApplication.isPlaying == true)
            {
                TestServicesConnect();
            }
            else
            {
                Debug.Log("[V2RuntimeTests] SKIP: V2 Services connect (requires Play Mode)");
            }

            Debug.Log("[V2RuntimeTests] === Done ===");
        }

        private static GameObject FindRootByName(Scene scene, string name)
        {
            GameObject[] roots = scene.GetRootGameObjects();

            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i] != null && roots[i].name == name)
                {
                    return roots[i];
                }
            }
            return null;
        }

        private static Canvas FindChildCanvas(GameObject root, string childName)
        {
            Transform child = root.transform.Find(childName);

            if (child == null)
            {
                return null;
            }

            return child.GetComponent<Canvas>();
        }

        private static void Report(string label, bool pass, string detail)
        {
            string tag = pass == true ? "PASS" : "FAIL";
            string line = "[V2RuntimeTests] " + tag + " " + label + " — " + detail;

            if (pass == true)
            {
                Debug.Log(line);
            }
            else
            {
                Debug.LogError(line);
            }
        }
    }
}
#endif
