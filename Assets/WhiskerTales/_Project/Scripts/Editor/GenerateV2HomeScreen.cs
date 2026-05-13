using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WhiskerTales.Navigation;
using WhiskerTales.UI.Home;
using WhiskerTales.UI.Screens;

namespace WhiskerTales.EditorTools
{
    /// <summary>
    /// V2-5: Main_App 씬에 HomeScreen + 자식 컴포넌트(HomeObjectLayer/HomeNabi/HomeAmbientLayer/HomeCopyFader) 자동 셋업.
    /// 메뉴: Whisker Tales/Setup/Setup V2 Home Screen
    ///
    /// 처리:
    ///   1. Main_App.unity 활성화 (이미 활성이면 그대로)
    ///   2. ScreenNavigator 자식으로 HomeScreen GO 생성 (idempotent)
    ///   3. Background Image (bg_home_main) + 자식 4종 구성 + 컴포넌트 와이어링
    ///   4. ScreenNavigator.screens 리스트에 HomeScreen 추가 + initialScreen=Home
    ///   5. 씬 저장
    /// </summary>
    public static class GenerateV2HomeScreen
    {
        private const string MainScenePath = "Assets/WhiskerTales/_Project/Scenes/Main_App.unity";
        private const string SetAssetPath = "Assets/Resources/Data/HomeObjectSet.asset";
        private const string CatNabiSpritePath = "Assets/Resources/Sprites/Characters/cat_nabi.png";

        [MenuItem("Whisker Tales/Setup/Setup V2 Home Screen")]
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

                EditorUtility.DisplayProgressBar("Setup V2 Home Screen", "Locating ScreenNavigator…", 0.1f);
                ScreenNavigator nav = FindNavigator(scene);

                if (nav == null)
                {
                    Fail("Main_App 씬에 ScreenNavigator가 없습니다. 먼저 Generate V2 Scenes 메뉴를 실행하세요.");
                    return;
                }

                EditorUtility.DisplayProgressBar("Setup V2 Home Screen", "Loading assets…", 0.2f);
                Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprites/Backgrounds/bg_home_main.png");

                if (bgSprite == null)
                {
                    Fail("bg_home_main.png Sprite를 로드할 수 없습니다.");
                    return;
                }

                Sprite catNabi = AssetDatabase.LoadAssetAtPath<Sprite>(CatNabiSpritePath);
                HomeObjectSet set = AssetDatabase.LoadAssetAtPath<HomeObjectSet>(SetAssetPath);

                // NotoSansKR_SDF를 primary font로 강제. 기본 TMP 폰트(LiberationSans 등)의
                // material/sprite asset 영향으로 마침표가 다른 색(핑크 등)으로 렌더링되는 이슈 회피.
                TMP_FontAsset koreanFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansKR_SDF.asset");
                TMP_FontAsset font = koreanFont != null ? koreanFont : TMP_Settings.defaultFontAsset;

                EditorUtility.DisplayProgressBar("Setup V2 Home Screen", "Building HomeScreen…", 0.5f);
                HomeScreen home = BuildHomeScreen(nav, bgSprite, catNabi, set, font);

                EditorUtility.DisplayProgressBar("Setup V2 Home Screen", "Registering in ScreenNavigator…", 0.85f);
                RegisterInNavigator(nav, home);

                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                AssetDatabase.SaveAssets();
                EditorUtility.ClearProgressBar();

                EditorUtility.DisplayDialog(
                    "Setup V2 Home Screen",
                    "완료\n\n" +
                    "✓ HomeScreen GameObject\n" +
                    "✓ Background (bg_home_main)\n" +
                    "✓ HomeObjectLayer + DoorLightZone + SleepFlashOverlay\n" +
                    "✓ HomeNabi (Body/EarLeft/EyeMask/TouchZone)\n" +
                    "✓ HomeAmbientLayer (LanternGlow/LeafSway)\n" +
                    "✓ TXT_HomeCopy\n" +
                    "✓ ScreenNavigator: initialScreen=Home + screens 등록\n\n" +
                    "재빌드하면 한옥 배경이 화면에 표시됩니다.",
                    "확인");
                Debug.Log("[Setup V2 Home Screen] Done.");
            }
            catch (System.Exception e)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError("[Setup V2 Home Screen] " + e);
                EditorUtility.DisplayDialog("Setup V2 Home Screen — 실패", e.Message, "확인");
            }
        }

        private static void Fail(string msg)
        {
            EditorUtility.ClearProgressBar();
            Debug.LogError("[Setup V2 Home Screen] " + msg);
            EditorUtility.DisplayDialog("Setup V2 Home Screen — 실패", msg, "확인");
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

        private static ScreenNavigator FindNavigator(Scene scene)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                ScreenNavigator nav = roots[i].GetComponentInChildren<ScreenNavigator>(true);
                if (nav != null) { return nav; }
            }
            return null;
        }

        // -----------------------------------------------------------------
        // HomeScreen + 자식 빌드
        // -----------------------------------------------------------------
        private static HomeScreen BuildHomeScreen(ScreenNavigator nav, Sprite bgSprite, Sprite catNabi, HomeObjectSet set, TMP_FontAsset font)
        {
            // 기존 HomeScreen이 있으면 재사용
            HomeScreen existing = nav.GetComponentInChildren<HomeScreen>(true);
            GameObject homeGo;
            HomeScreen home;

            if (existing != null)
            {
                homeGo = existing.gameObject;
                home = existing;
            }
            else
            {
                homeGo = new GameObject("HomeScreen", typeof(RectTransform), typeof(CanvasGroup));
                homeGo.transform.SetParent(nav.transform, false);
                StretchFull(homeGo);
                home = homeGo.AddComponent<HomeScreen>();
            }

            // ScreenId 강제
            SetSerializedEnum(home, "screenId", (int)ScreenId.Home);

            // Background
            GameObject bgGo = EnsureChild(homeGo, "BG_Background");
            StretchFull(bgGo);
            Image bgImg = EnsureComponent<Image>(bgGo);
            bgImg.sprite = bgSprite;
            bgImg.color = Color.white;
            bgImg.preserveAspect = false;
            bgImg.raycastTarget = false;
            bgGo.transform.SetAsFirstSibling();

            SetSerializedReference(home, "backgroundImage", bgImg);

            // HomeObjectLayer + 자식
            GameObject layerGo = EnsureChild(homeGo, "HomeObjectLayer");
            StretchFull(layerGo);
            HomeObjectLayer layer = EnsureComponent<HomeObjectLayer>(layerGo);
            CanvasGroup layerCg = EnsureComponent<CanvasGroup>(layerGo); // raycast 통과용 (자식만)
            layerCg.blocksRaycasts = true;
            layerCg.interactable = true;

            GameObject doorGo = EnsureChild(layerGo, "DoorLightZone");
            ConfigureRect(doorGo, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-180f, 260f), new Vector2(320f, 420f));
            EnsureInvisibleImage(doorGo, true);

            GameObject flashGo = EnsureChild(layerGo, "SleepFlashOverlay");
            StretchFull(flashGo);
            Image flashImg = EnsureComponent<Image>(flashGo);
            flashImg.color = new Color(1f, 1f, 1f, 0.01f);
            flashImg.sprite = null;
            flashImg.raycastTarget = false;

            SetSerializedReference(layer, "set", set);
            SetSerializedReference(layer, "navigator", nav);
            SetSerializedReference(layer, "doorLightZone", doorGo.GetComponent<RectTransform>());
            SetSerializedReference(layer, "sleepFlashOverlay", flashImg);

            // HomeNabi + 자식
            GameObject nabiGo = EnsureChild(homeGo, "HomeNabi");
            ConfigureRect(nabiGo, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f), new Vector2(-260f, 680f), new Vector2(280f, 320f));
            HomeNabi nabi = EnsureComponent<HomeNabi>(nabiGo);

            GameObject bodyGo = EnsureChild(nabiGo, "Body");
            StretchFull(bodyGo);
            Image bodyImg = EnsureComponent<Image>(bodyGo);
            if (catNabi != null) { bodyImg.sprite = catNabi; }
            bodyImg.color = Color.white;
            bodyImg.preserveAspect = true;
            bodyImg.raycastTarget = false;

            GameObject earGo = EnsureChild(bodyGo, "EarLeft");
            ConfigureRect(earGo, new Vector2(0.3f, 0.85f), new Vector2(0.3f, 0.85f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(80f, 80f));

            GameObject eyeGo = EnsureChild(nabiGo, "EyeMask");
            StretchFull(eyeGo);
            CanvasGroup eyeCg = EnsureComponent<CanvasGroup>(eyeGo);
            eyeCg.alpha = 0f;
            eyeCg.blocksRaycasts = false;
            eyeCg.interactable = false;
            Image eyeImg = EnsureComponent<Image>(eyeGo);
            eyeImg.color = new Color(0f, 0f, 0f, 0.6f);
            eyeImg.sprite = null;
            eyeImg.raycastTarget = false;

            GameObject touchGo = EnsureChild(nabiGo, "TouchZone");
            StretchFull(touchGo);
            Image touchImg = EnsureInvisibleImage(touchGo, true);

            SetSerializedReference(nabi, "body", bodyGo.GetComponent<RectTransform>());
            SetSerializedReference(nabi, "earLeft", earGo.GetComponent<RectTransform>());
            SetSerializedReference(nabi, "eyeMask", eyeCg);
            SetSerializedReference(nabi, "touchZoneImage", touchImg);

            // HomeAmbientLayer + 자식
            GameObject ambientGo = EnsureChild(homeGo, "HomeAmbientLayer");
            StretchFull(ambientGo);
            HomeAmbientLayer ambient = EnsureComponent<HomeAmbientLayer>(ambientGo);

            GameObject lanternGo = EnsureChild(ambientGo, "LanternGlow");
            ConfigureRect(lanternGo, new Vector2(0.16f, 0.62f), new Vector2(0.16f, 0.62f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(220f, 220f));
            Image lanternImg = EnsureComponent<Image>(lanternGo);
            lanternImg.color = new Color(1f, 0.85f, 0.55f, 0.4f);
            lanternImg.sprite = null;
            lanternImg.raycastTarget = false;

            GameObject leafGo = EnsureChild(ambientGo, "LeafSway");
            ConfigureRect(leafGo, new Vector2(0.1f, 0.85f), new Vector2(0.1f, 0.85f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(400f, 400f));

            SetSerializedReference(ambient, "lanternGlow", lanternImg);
            SetSerializedReference(ambient, "leafSway", leafGo.GetComponent<RectTransform>());

            // TXT_HomeCopy — 탭바(BottomNav 170) 바로 위 여백 두고 배치. 나비 본체와 겹치지 않게.
            GameObject copyGo = EnsureChild(homeGo, "TXT_HomeCopy");
            ConfigureRect(copyGo, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f), new Vector2(0f, 210f), new Vector2(900f, 60f));
            TextMeshProUGUI copyTmp = EnsureComponent<TextMeshProUGUI>(copyGo);
            // 마침표 색 강제 (벨트+멜빵: 명시적 한글 폰트 + rich-text color tag).
            copyTmp.text = "오늘도 여기 있어요<color=#F5F1E8>.</color>";
            copyTmp.richText = true;
            copyTmp.fontSize = 38f;
            copyTmp.alignment = TextAlignmentOptions.Center;
            copyTmp.color = WhiskerTales.UI.UILayoutConstants.Cream;
            copyTmp.raycastTarget = false;
            copyTmp.alpha = 0f;
            if (font != null) { copyTmp.font = font; }

            HomeCopyFader fader = EnsureComponent<HomeCopyFader>(copyGo);
            SetSerializedReference(fader, "text", copyTmp);
            // HomeCopyFader.copy를 새 rich-text 문자열로 갱신 (OnEnable에서 text.text = copy로 덮어쓰기 때문).
            SetSerializedString(fader, "copy", "오늘도 여기 있어요<color=#F5F1E8>.</color>");

            return home;
        }

        private static void RegisterInNavigator(ScreenNavigator nav, HomeScreen home)
        {
            SerializedObject so = new SerializedObject(nav);
            SerializedProperty initial = so.FindProperty("initialScreen");
            initial.enumValueIndex = (int)ScreenId.Home;

            SerializedProperty list = so.FindProperty("screens");

            // 중복 체크
            for (int i = 0; i < list.arraySize; i++)
            {
                Object existing = list.GetArrayElementAtIndex(i).objectReferenceValue;
                if (existing == home)
                {
                    so.ApplyModifiedPropertiesWithoutUndo();
                    return;
                }
            }

            list.arraySize++;
            list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = home;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // -----------------------------------------------------------------
        // 헬퍼
        // -----------------------------------------------------------------
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

        private static Image EnsureInvisibleImage(GameObject go, bool raycast)
        {
            Image img = EnsureComponent<Image>(go);
            img.color = new Color(1f, 1f, 1f, 0.01f);
            img.sprite = null;
            img.raycastTarget = raycast;
            return img;
        }

        private static void SetSerializedReference(Object target, string field, Object value)
        {
            SerializedObject so = new SerializedObject(target);
            SerializedProperty p = so.FindProperty(field);
            if (p == null)
            {
                Debug.LogWarning("[Setup V2 Home Screen] field not found: " + field + " on " + target.GetType().Name);
                return;
            }
            p.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetSerializedEnum(Object target, string field, int value)
        {
            SerializedObject so = new SerializedObject(target);
            SerializedProperty p = so.FindProperty(field);
            if (p == null) { return; }
            p.enumValueIndex = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetSerializedString(Object target, string field, string value)
        {
            SerializedObject so = new SerializedObject(target);
            SerializedProperty p = so.FindProperty(field);
            if (p == null)
            {
                Debug.LogWarning("[Setup V2 Home Screen] string field not found: " + field + " on " + target.GetType().Name);
                return;
            }
            p.stringValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
