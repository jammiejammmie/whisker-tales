using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WhiskerTales.Runtime;
using WhiskerTales.UI.Screens;

namespace WhiskerTales.EditorTools
{
    /// <summary>
    /// Main_App의 Canvas_Background(SafeArea 밖, sortingOrder=0) 아래에 HomeTimeOfDayController +
    /// LayerA/LayerB Image 2장을 자동 생성하고, 01_MAIN_HOME의 4개 시간대 sprite를 wire한다.
    /// 메뉴: Whisker Tales/Setup/Setup Time Of Day
    /// Idempotent — 씬 어디에 있든 기존 HomeTimeOfDay를 찾아 재부모화 (HomeScreen → Canvas_Background).
    /// </summary>
    public static class SetupTimeOfDay
    {
        private const string MainScenePath = "Assets/WhiskerTales/_Project/Scenes/Main_App.unity";
        private const string SpriteFolder = "Assets/WhiskerTales/Art/01_MAIN_HOME";
        private const string CanvasBackgroundName = "Canvas_Background";
        private const string ContainerName = "HomeTimeOfDay";
        private const string LayerAName = "LayerA";
        private const string LayerBName = "LayerB";

        [MenuItem("Whisker Tales/Setup/Setup Time Of Day")]
        public static void Run()
        {
            try
            {
                Scene scene = EnsureMainAppOpen();

                if (scene.IsValid() == false)
                {
                    Fail("Main_App scene을 열 수 없습니다: " + MainScenePath);
                    return;
                }

                EditorUtility.DisplayProgressBar("Setup Time Of Day", "Locating Canvas_Background…", 0.1f);
                GameObject canvasBg = FindRootByName(scene, CanvasBackgroundName);

                if (canvasBg == null)
                {
                    Fail("Main_App 씬에 " + CanvasBackgroundName + " 가 없습니다. 먼저 'Generate V2 Scenes'를 실행하세요.");
                    return;
                }

                EditorUtility.DisplayProgressBar("Setup Time Of Day", "Loading sprites…", 0.3f);
                Sprite dawn = LoadSprite("bg_home_dawn_v01");
                Sprite day = LoadSprite("bg_home_day_v01");
                Sprite evening = LoadSprite("bg_home_evening_v01");
                Sprite night = LoadSprite("bg_home_night_v01");

                if (dawn == null || day == null || evening == null || night == null)
                {
                    Fail("01_MAIN_HOME 폴더에서 4개 sprite를 모두 로드하지 못했습니다.");
                    return;
                }

                EditorUtility.DisplayProgressBar("Setup Time Of Day", "Building hierarchy…", 0.6f);

                // 기존 HomeTimeOfDayController가 씬 어디에 있든 찾아서 Canvas_Background로 이동.
                HomeTimeOfDayController existing = FindControllerInScene(scene);
                GameObject containerGo;

                if (existing != null)
                {
                    containerGo = existing.gameObject;
                    containerGo.transform.SetParent(canvasBg.transform, false);
                }
                else
                {
                    containerGo = EnsureChild(canvasBg, ContainerName);
                }

                StretchFull(containerGo);
                containerGo.transform.SetAsFirstSibling();

                HomeTimeOfDayController controller = EnsureComponent<HomeTimeOfDayController>(containerGo);

                GameObject layerAGo = EnsureChild(containerGo, LayerAName);
                StretchFull(layerAGo);
                Image layerA = EnsureComponent<Image>(layerAGo);
                ConfigureBackgroundImage(layerA);

                GameObject layerBGo = EnsureChild(containerGo, LayerBName);
                StretchFull(layerBGo);
                Image layerB = EnsureComponent<Image>(layerBGo);
                ConfigureBackgroundImage(layerB);

                EditorUtility.DisplayProgressBar("Setup Time Of Day", "Wiring serialized fields…", 0.85f);
                SerializedObject so = new SerializedObject(controller);
                SetObjectRef(so, "layerA", layerA);
                SetObjectRef(so, "layerB", layerB);
                SetObjectRef(so, "dawnSprite", dawn);
                SetObjectRef(so, "daySprite", day);
                SetObjectRef(so, "eveningSprite", evening);
                SetObjectRef(so, "nightSprite", night);
                so.ApplyModifiedPropertiesWithoutUndo();

                // HomeScreen의 legacy BG_Background 정리 — 시간대 배경을 담당하는 건 HomeTimeOfDay이므로
                // 위쪽 Canvas_Screens(sort 100)에서 깔리는 정적 배경이 시간대 배경을 가리지 않도록.
                bool bgCleared = ClearHomeScreenBackground(scene);

                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                AssetDatabase.SaveAssets();
                EditorUtility.ClearProgressBar();

                string bgNote = bgCleared
                    ? "✓ HomeScreen.BG_Background 정리 (sprite=null, alpha=0, path 비움)"
                    : "ℹ HomeScreen.BG_Background 없음 — skip";

                EditorUtility.DisplayDialog(
                    "Setup Time Of Day",
                    "완료\n\n" +
                    "✓ HomeTimeOfDay GameObject (" + CanvasBackgroundName + " 첫 자식, SafeArea 밖)\n" +
                    "✓ LayerA / LayerB Image (stretch full → 화면 끝까지)\n" +
                    "✓ 4 sprite wired (dawn/day/evening/night)\n" +
                    bgNote + "\n\n" +
                    "이제 시간대 배경(Canvas_Background)이 화면 전체를 담당합니다.",
                    "확인");
                Debug.Log("[Setup Time Of Day] Done.");
            }
            catch (System.Exception e)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError("[Setup Time Of Day] " + e);
                EditorUtility.DisplayDialog("Setup Time Of Day — 실패", e.Message, "확인");
            }
        }

        private static void Fail(string msg)
        {
            EditorUtility.ClearProgressBar();
            Debug.LogError("[Setup Time Of Day] " + msg);
            EditorUtility.DisplayDialog("Setup Time Of Day — 실패", msg, "확인");
        }

        private static Scene EnsureMainAppOpen()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene s = SceneManager.GetSceneAt(i);
                if (s.path == MainScenePath && s.isLoaded == true)
                {
                    return s;
                }
            }

            if (EditorSceneManager.GetActiveScene().isDirty == true)
            {
                bool save = EditorUtility.DisplayDialog("Save current scene?",
                    "현재 씬에 변경이 있습니다. 저장 후 Main_App을 열까요?",
                    "저장 후 진행", "취소");

                if (save == false) { return default; }

                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            }

            return EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);
        }

        private static GameObject FindRootByName(Scene scene, string name)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i].name == name) { return roots[i]; }
            }
            return null;
        }

        private static HomeTimeOfDayController FindControllerInScene(Scene scene)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                HomeTimeOfDayController c = roots[i].GetComponentInChildren<HomeTimeOfDayController>(true);
                if (c != null) { return c; }
            }
            return null;
        }

        private static bool ClearHomeScreenBackground(Scene scene)
        {
            HomeScreen home = null;
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                HomeScreen found = roots[i].GetComponentInChildren<HomeScreen>(true);
                if (found != null) { home = found; break; }
            }

            if (home == null) { return false; }

            // 1) HomeScreen.backgroundResourcePath = "" → ApplyBackground이 Resources 로드 스킵.
            SerializedObject so = new SerializedObject(home);
            SerializedProperty pathProp = so.FindProperty("backgroundResourcePath");
            if (pathProp != null)
            {
                pathProp.stringValue = string.Empty;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            // 2) BG_Background 자식 Image: sprite=null + color.a=0 (벨트+멜빵).
            Transform bgTransform = home.transform.Find("BG_Background");
            if (bgTransform == null) { return true; }

            Image bgImg = bgTransform.GetComponent<Image>();
            if (bgImg != null)
            {
                bgImg.sprite = null;
                Color c = bgImg.color;
                c.a = 0f;
                bgImg.color = c;
                EditorUtility.SetDirty(bgImg);
            }

            return true;
        }

        private static Sprite LoadSprite(string fileNameNoExt)
        {
            string path = SpriteFolder + "/" + fileNameNoExt + ".png";
            Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (s == null)
            {
                Debug.LogWarning("[Setup Time Of Day] sprite not found: " + path);
            }
            return s;
        }

        private static GameObject EnsureChild(GameObject parent, string name)
        {
            Transform t = parent.transform.Find(name);
            if (t != null) { return t.gameObject; }

            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent.transform, false);
            return go;
        }

        private static T EnsureComponent<T>(GameObject go) where T : Component
        {
            T c = go.GetComponent<T>();
            if (c == null) { c = go.AddComponent<T>(); }
            return c;
        }

        private static void StretchFull(GameObject go)
        {
            RectTransform r = go.GetComponent<RectTransform>();
            if (r == null) { r = go.AddComponent<RectTransform>(); }
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.pivot = new Vector2(0.5f, 0.5f);
            r.anchoredPosition = Vector2.zero;
            r.sizeDelta = Vector2.zero;
            r.localScale = Vector3.one;
            r.localRotation = Quaternion.identity;
        }

        private static void ConfigureBackgroundImage(Image img)
        {
            img.color = Color.white;
            img.preserveAspect = false;
            img.raycastTarget = false;
        }

        private static void SetObjectRef(SerializedObject so, string field, Object value)
        {
            SerializedProperty p = so.FindProperty(field);
            if (p == null)
            {
                Debug.LogWarning("[Setup Time Of Day] field not found: " + field);
                return;
            }
            p.objectReferenceValue = value;
        }
    }
}
