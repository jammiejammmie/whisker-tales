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
    /// Main_App 씬의 HomeScreen 자식으로 HomeNabiPositionSystem GO를 자동 셋업.
    /// 메뉴: Whisker Tales/Setup/Setup Nabi Position System
    /// Idempotent — 다시 실행해도 중복 생성 없이 wiring/위치만 갱신.
    /// </summary>
    public static class SetupNabiPositionSystem
    {
        private const string MainScenePath = "Assets/WhiskerTales/_Project/Scenes/Main_App.unity";
        private const string SpriteFolder = "Assets/WhiskerTales/Art/02_NABI_HOME";

        private const string ContainerName = "HomeNabiPositionSystem";
        private const string BodyName = "NabiBody";
        private const string EyeMaskName = "EyeMask";
        private const string AnchorsName = "Anchors";

        // 한옥 배경 가정 기본 좌표 (1080x1920 canvas, center=0,0). Artist가 Inspector에서 미세조정.
        private struct AnchorSpec
        {
            public string id;
            public Vector2 pos;
            public HomeNabiPositionSystem.PoseId pose;
        }

        // 하단 anchor 3개(cushion/maru_end/puzzle_book)를 y=-550으로 상향 — 카피(탭바 위 y≈210)와
        // 본체(280×320 pivot center) 하단이 겹치지 않게. body_bottom = (960 + y) - 160 ≥ 카피 top.
        private static readonly AnchorSpec[] DefaultAnchors = new AnchorSpec[]
        {
            new AnchorSpec { id = "pos_maru_end",    pos = new Vector2( 350f, -550f), pose = HomeNabiPositionSystem.PoseId.Random },
            new AnchorSpec { id = "pos_puzzle_book", pos = new Vector2(-250f, -550f), pose = HomeNabiPositionSystem.PoseId.Random },
            new AnchorSpec { id = "pos_cushion",     pos = new Vector2(   0f, -550f), pose = HomeNabiPositionSystem.PoseId.SleepyLoaf },
            new AnchorSpec { id = "pos_door_front",  pos = new Vector2( 400f, -300f), pose = HomeNabiPositionSystem.PoseId.IdleSit },
            new AnchorSpec { id = "pos_sunlight",    pos = new Vector2(-150f, -450f), pose = HomeNabiPositionSystem.PoseId.SleepyLoaf },
            new AnchorSpec { id = "pos_eave",        pos = new Vector2(-300f,  200f), pose = HomeNabiPositionSystem.PoseId.Random },
            new AnchorSpec { id = "pos_yard_view",   pos = new Vector2( 200f, -100f), pose = HomeNabiPositionSystem.PoseId.Random },
            new AnchorSpec { id = "pos_lantern",     pos = new Vector2(-350f,  100f), pose = HomeNabiPositionSystem.PoseId.Random }
        };

        private const float BodyWidth = 280f;
        private const float BodyHeight = 320f;

        [MenuItem("Whisker Tales/Setup/Setup Nabi Position System")]
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

                EditorUtility.DisplayProgressBar("Setup Nabi Position System", "Locating HomeScreen…", 0.1f);
                HomeScreen home = FindHomeScreen(scene);

                if (home == null)
                {
                    Fail("Main_App 씬에 HomeScreen이 없습니다. 먼저 'Setup V2 Home Screen' 메뉴를 실행하세요.");
                    return;
                }

                EditorUtility.DisplayProgressBar("Setup Nabi Position System", "Loading sprites…", 0.3f);
                Sprite idleSit = LoadSprite("nabi_home_idle_sit_v01");
                Sprite sleepyLoaf = LoadSprite("nabi_home_sleepy_loaf_v01");
                Sprite stretchSmall = LoadSprite("nabi_home_stretch_small_v01");

                if (idleSit == null || sleepyLoaf == null || stretchSmall == null)
                {
                    Fail("02_NABI_HOME 폴더에서 3개 sprite를 모두 로드하지 못했습니다.");
                    return;
                }

                EditorUtility.DisplayProgressBar("Setup Nabi Position System", "Building hierarchy…", 0.55f);
                GameObject containerGo = EnsureChild(home.gameObject, ContainerName);
                StretchFull(containerGo);

                HomeNabiPositionSystem system = EnsureComponent<HomeNabiPositionSystem>(containerGo);

                // Body (RectTransform + CanvasGroup + Image)
                GameObject bodyGo = EnsureChild(containerGo, BodyName);
                ConfigureRect(bodyGo,
                    anchorMin: new Vector2(0.5f, 0.5f),
                    anchorMax: new Vector2(0.5f, 0.5f),
                    pivot: new Vector2(0.5f, 0.5f),
                    pos: DefaultAnchors[2].pos, // cushion으로 초기 배치
                    size: new Vector2(BodyWidth, BodyHeight));
                CanvasGroup bodyCg = EnsureComponent<CanvasGroup>(bodyGo);
                bodyCg.alpha = 1f;
                bodyCg.blocksRaycasts = false;
                bodyCg.interactable = false;
                Image bodyImg = EnsureComponent<Image>(bodyGo);
                bodyImg.sprite = sleepyLoaf;
                bodyImg.color = Color.white;
                bodyImg.preserveAspect = true;
                bodyImg.raycastTarget = false;

                // EyeMask (RectTransform stretch inside body + CanvasGroup + black Image)
                GameObject eyeGo = EnsureChild(bodyGo, EyeMaskName);
                StretchFull(eyeGo);
                CanvasGroup eyeCg = EnsureComponent<CanvasGroup>(eyeGo);
                eyeCg.alpha = 0f;
                eyeCg.blocksRaycasts = false;
                eyeCg.interactable = false;
                Image eyeImg = EnsureComponent<Image>(eyeGo);
                eyeImg.color = new Color(0f, 0f, 0f, 0.55f);
                eyeImg.sprite = null;
                eyeImg.raycastTarget = false;

                EditorUtility.DisplayProgressBar("Setup Nabi Position System", "Creating anchors…", 0.75f);

                // Anchors container (stretches full)
                GameObject anchorsGo = EnsureChild(containerGo, AnchorsName);
                StretchFull(anchorsGo);

                RectTransform[] anchorRects = new RectTransform[DefaultAnchors.Length];
                for (int i = 0; i < DefaultAnchors.Length; i++)
                {
                    AnchorSpec spec = DefaultAnchors[i];
                    GameObject anchorGo = EnsureChild(anchorsGo, spec.id);
                    ConfigureRect(anchorGo,
                        anchorMin: new Vector2(0.5f, 0.5f),
                        anchorMax: new Vector2(0.5f, 0.5f),
                        pivot: new Vector2(0.5f, 0.5f),
                        pos: spec.pos,
                        size: new Vector2(40f, 40f));
                    anchorRects[i] = anchorGo.GetComponent<RectTransform>();
                }

                EditorUtility.DisplayProgressBar("Setup Nabi Position System", "Wiring serialized fields…", 0.9f);

                SerializedObject so = new SerializedObject(system);
                SetObjectRef(so, "body", bodyGo.GetComponent<RectTransform>());
                SetObjectRef(so, "bodyImage", bodyImg);
                SetObjectRef(so, "bodyCanvasGroup", bodyCg);
                SetObjectRef(so, "eyeMask", eyeCg);
                SetObjectRef(so, "poseIdleSit", idleSit);
                SetObjectRef(so, "poseSleepyLoaf", sleepyLoaf);
                SetObjectRef(so, "poseStretchSmall", stretchSmall);

                SerializedProperty positionsProp = so.FindProperty("positions");
                if (positionsProp == null)
                {
                    Fail("HomeNabiPositionSystem.positions 필드를 찾지 못했습니다.");
                    return;
                }

                positionsProp.arraySize = DefaultAnchors.Length;
                for (int i = 0; i < DefaultAnchors.Length; i++)
                {
                    SerializedProperty elem = positionsProp.GetArrayElementAtIndex(i);
                    elem.FindPropertyRelative("id").stringValue = DefaultAnchors[i].id;
                    elem.FindPropertyRelative("anchor").objectReferenceValue = anchorRects[i];
                    elem.FindPropertyRelative("recommendedPose").enumValueIndex = (int)DefaultAnchors[i].pose;
                }

                so.ApplyModifiedPropertiesWithoutUndo();

                // HomeNabiPositionSystem을 HomeScreen 자식 마지막에 두어 다른 ambient 요소 위에 그려지게.
                // (BG/시간대 배경 위, NabiBody의 sibling이 없으면 그대로 OK)
                containerGo.transform.SetAsLastSibling();

                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                AssetDatabase.SaveAssets();
                EditorUtility.ClearProgressBar();

                EditorUtility.DisplayDialog(
                    "Setup Nabi Position System",
                    "완료\n\n" +
                    "✓ HomeNabiPositionSystem GameObject (HomeScreen 자식)\n" +
                    "✓ NabiBody (CanvasGroup + Image, 280×320, sleepyLoaf 기본 sprite)\n" +
                    "✓ EyeMask (alpha=0, 블링크용)\n" +
                    "✓ Anchors × 8 (한옥 배경 기본 좌표)\n" +
                    "✓ 3 sprite wired (idle_sit / sleepy_loaf / stretch_small)\n\n" +
                    "Anchor 위치는 한옥 배경 가정 기본값입니다.\n" +
                    "Scene 뷰에서 각 pos_* 를 선택해 실제 배경 위치에 맞게 조정하세요.",
                    "확인");
                Debug.Log("[Setup Nabi Position System] Done.");
            }
            catch (System.Exception e)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError("[Setup Nabi Position System] " + e);
                EditorUtility.DisplayDialog("Setup Nabi Position System — 실패", e.Message, "확인");
            }
        }

        private static void Fail(string msg)
        {
            EditorUtility.ClearProgressBar();
            Debug.LogError("[Setup Nabi Position System] " + msg);
            EditorUtility.DisplayDialog("Setup Nabi Position System — 실패", msg, "확인");
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

        private static HomeScreen FindHomeScreen(Scene scene)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                HomeScreen home = roots[i].GetComponentInChildren<HomeScreen>(true);
                if (home != null) { return home; }
            }
            return null;
        }

        private static Sprite LoadSprite(string fileNameNoExt)
        {
            string path = SpriteFolder + "/" + fileNameNoExt + ".png";
            Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (s == null)
            {
                Debug.LogWarning("[Setup Nabi Position System] sprite not found: " + path);
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

        private static void ConfigureRect(GameObject go, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            RectTransform r = go.GetComponent<RectTransform>();
            if (r == null) { r = go.AddComponent<RectTransform>(); }
            r.anchorMin = anchorMin;
            r.anchorMax = anchorMax;
            r.pivot = pivot;
            r.anchoredPosition = pos;
            r.sizeDelta = size;
            r.localScale = Vector3.one;
            r.localRotation = Quaternion.identity;
        }

        private static void SetObjectRef(SerializedObject so, string field, Object value)
        {
            SerializedProperty p = so.FindProperty(field);
            if (p == null)
            {
                Debug.LogWarning("[Setup Nabi Position System] field not found: " + field);
                return;
            }
            p.objectReferenceValue = value;
        }
    }
}
