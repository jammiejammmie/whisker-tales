using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace WhiskerTales.UI.Home.EditorTools
{
    /// <summary>
    /// 한 번 클릭으로 Stage 2 홈 화면 셋업을 끝내는 자동화 스크립트.
    /// Idempotent — 여러 번 실행해도 중복 자식/entry 안 생김.
    ///
    /// 처리 항목:
    ///   1. HomeObjectSet.asset 생성 (Assets/Resources/Data/)
    ///   2. UIAssetRegistryRuntime의 sprites 리스트에 bg_home_main 등록 (활성 씬)
    ///   3. Screen_MainTitle.prefab에 자식 4종 추가 + 컴포넌트 부착 + 필드 와이어링
    ///       - HomeObjectLayer (+ DoorLightZone, SleepFlashOverlay)
    ///       - HomeNabi (+ Body, EarLeft, EyeMask, TouchZone)
    ///       - HomeAmbientLayer (+ LanternGlow, LeafSway)
    ///       - TXT_HomeCopy
    /// </summary>
    public static class SetupHomeScreen
    {
        private const string BgSpritePath = "Assets/Resources/Sprites/Backgrounds/bg_home_main.png";
        private const string CatNabiSpritePath = "Assets/Resources/Sprites/Characters/cat_nabi.png";
        private const string PrefabPath = "Assets/WhiskerTales/Prefabs/UI/Screens/Screen_MainTitle.prefab";
        private const string SetAssetPath = "Assets/Resources/Data/HomeObjectSet.asset";
        private const string SetAssetDir = "Assets/Resources/Data";
        private const string SpriteKey = "bg_home_main";

        [MenuItem("Whisker Tales/Setup/Setup Home Screen")]
        public static void Run()
        {
            EditorUtility.DisplayProgressBar("Setup Home Screen", "Preflight…", 0.05f);

            try
            {
                Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BgSpritePath);
                if (bgSprite == null)
                {
                    Fail("bg_home_main.png Sprite를 로드할 수 없습니다 (Texture Type=Sprite 확인).");
                    return;
                }

                Sprite catNabi = AssetDatabase.LoadAssetAtPath<Sprite>(CatNabiSpritePath);
                if (catNabi == null)
                {
                    Fail("cat_nabi.png Sprite를 로드할 수 없습니다.");
                    return;
                }

                GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
                if (prefabAsset == null)
                {
                    Fail($"Prefab을 찾을 수 없습니다: {PrefabPath}");
                    return;
                }

                EditorUtility.DisplayProgressBar("Setup Home Screen", "HomeObjectSet asset…", 0.20f);
                HomeObjectSet set = EnsureHomeObjectSet();

                EditorUtility.DisplayProgressBar("Setup Home Screen", "UIAssetRegistry entry…", 0.40f);
                bool registryOk = EnsureRegistryEntry(bgSprite);

                EditorUtility.DisplayProgressBar("Setup Home Screen", "Screen_MainTitle prefab…", 0.65f);
                SetupPrefab(catNabi, set);

                EditorUtility.DisplayProgressBar("Setup Home Screen", "Save…", 0.95f);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                EditorUtility.ClearProgressBar();

                string registryNote = registryOk
                    ? "✓ UIAssetRegistry entry"
                    : "⚠ UIAssetRegistryRuntime을 활성 씬에서 못 찾음 — 수동 등록 필요";

                EditorUtility.DisplayDialog(
                    "Setup Home Screen",
                    "완료\n\n" +
                    "✓ HomeObjectSet.asset\n" +
                    registryNote + "\n" +
                    "✓ Screen_MainTitle.prefab 자식 추가\n\n" +
                    "씬에 변경이 있을 수 있으니 Ctrl+S로 저장하세요.",
                    "확인");

                Debug.Log("[Setup Home Screen] Completed.");
            }
            catch (System.Exception e)
            {
                EditorUtility.ClearProgressBar();
                Fail($"예외 발생: {e.GetType().Name}: {e.Message}\n{e.StackTrace}");
            }
        }

        private static void Fail(string message)
        {
            EditorUtility.ClearProgressBar();
            Debug.LogError("[Setup Home Screen] " + message);
            EditorUtility.DisplayDialog("Setup Home Screen — 실패", message, "확인");
        }

        // -----------------------------------------------------------------
        // 1. HomeObjectSet asset
        // -----------------------------------------------------------------
        private static HomeObjectSet EnsureHomeObjectSet()
        {
            if (!Directory.Exists(SetAssetDir))
            {
                Directory.CreateDirectory(SetAssetDir);
                AssetDatabase.Refresh();
            }

            HomeObjectSet existing = AssetDatabase.LoadAssetAtPath<HomeObjectSet>(SetAssetPath);

            if (existing != null)
            {
                Debug.Log("[Setup Home Screen] HomeObjectSet already exists — kept as-is.");
                return existing;
            }

            HomeObjectSet set = ScriptableObject.CreateInstance<HomeObjectSet>();

            HomeObjectEntry[] entries = new HomeObjectEntry[]
            {
                new HomeObjectEntry
                {
                    id = "cushion",
                    sprite = null,
                    anchorMin = new Vector2(0.5f, 0f),
                    anchorMax = new Vector2(0.5f, 0f),
                    pivot = new Vector2(0.5f, 0.5f),
                    anchoredPosition = new Vector2(340f, 560f),
                    size = new Vector2(280f, 120f),
                    interaction = HomeInteractionTarget.None
                },
                new HomeObjectEntry
                {
                    id = "teacup",
                    sprite = null,
                    anchorMin = new Vector2(0.5f, 0f),
                    anchorMax = new Vector2(0.5f, 0f),
                    pivot = new Vector2(0.5f, 0.5f),
                    anchoredPosition = new Vector2(360f, 400f),
                    size = new Vector2(130f, 130f),
                    interaction = HomeInteractionTarget.None
                },
                new HomeObjectEntry
                {
                    id = "puzzle_book",
                    sprite = null,
                    anchorMin = new Vector2(0.5f, 0f),
                    anchorMax = new Vector2(0.5f, 0f),
                    pivot = new Vector2(0.5f, 0.5f),
                    anchoredPosition = new Vector2(50f, 520f),
                    size = new Vector2(360f, 220f),
                    interaction = HomeInteractionTarget.LevelSelect
                }
            };

            SerializedObject so = new SerializedObject(set);
            SerializedProperty entriesProp = so.FindProperty("entries");
            entriesProp.arraySize = entries.Length;

            for (int i = 0; i < entries.Length; i++)
            {
                SerializedProperty e = entriesProp.GetArrayElementAtIndex(i);
                HomeObjectEntry src = entries[i];

                e.FindPropertyRelative("id").stringValue = src.id;
                e.FindPropertyRelative("sprite").objectReferenceValue = src.sprite;
                e.FindPropertyRelative("anchorMin").vector2Value = src.anchorMin;
                e.FindPropertyRelative("anchorMax").vector2Value = src.anchorMax;
                e.FindPropertyRelative("pivot").vector2Value = src.pivot;
                e.FindPropertyRelative("anchoredPosition").vector2Value = src.anchoredPosition;
                e.FindPropertyRelative("size").vector2Value = src.size;
                e.FindPropertyRelative("interaction").enumValueIndex = (int)src.interaction;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(set, SetAssetPath);
            Debug.Log("[Setup Home Screen] Created HomeObjectSet.asset (3 entries).");
            return set;
        }

        // -----------------------------------------------------------------
        // 2. UIAssetRegistryRuntime entry
        // -----------------------------------------------------------------
        private static bool EnsureRegistryEntry(Sprite bgSprite)
        {
            UIAssetRegistryRuntime registry = FindRegistryInLoadedScenes();

            if (registry == null)
            {
                Debug.LogWarning("[Setup Home Screen] UIAssetRegistryRuntime not found in any loaded scene. Open the Main scene and re-run, or register the entry manually.");
                return false;
            }

            SerializedObject so = new SerializedObject(registry);
            SerializedProperty spritesProp = so.FindProperty("sprites");

            for (int i = 0; i < spritesProp.arraySize; i++)
            {
                SerializedProperty entry = spritesProp.GetArrayElementAtIndex(i);
                string key = entry.FindPropertyRelative("key").stringValue;

                if (key == SpriteKey)
                {
                    SerializedProperty spriteProp = entry.FindPropertyRelative("sprite");

                    if (spriteProp.objectReferenceValue != bgSprite)
                    {
                        spriteProp.objectReferenceValue = bgSprite;
                        so.ApplyModifiedProperties();
                        EditorSceneManager.MarkSceneDirty(registry.gameObject.scene);
                        Debug.Log("[Setup Home Screen] UIAssetRegistry entry already existed — sprite reference updated.");
                    }
                    else
                    {
                        Debug.Log("[Setup Home Screen] UIAssetRegistry entry already correct — kept.");
                    }

                    return true;
                }
            }

            spritesProp.arraySize++;
            SerializedProperty newEntry = spritesProp.GetArrayElementAtIndex(spritesProp.arraySize - 1);
            newEntry.FindPropertyRelative("key").stringValue = SpriteKey;
            newEntry.FindPropertyRelative("sprite").objectReferenceValue = bgSprite;
            so.ApplyModifiedProperties();

            EditorSceneManager.MarkSceneDirty(registry.gameObject.scene);
            Debug.Log("[Setup Home Screen] Added UIAssetRegistry entry: bg_home_main.");
            return true;
        }

        private static UIAssetRegistryRuntime FindRegistryInLoadedScenes()
        {
            for (int s = 0; s < SceneManager.sceneCount; s++)
            {
                Scene scene = SceneManager.GetSceneAt(s);

                if (scene.isLoaded == false)
                {
                    continue;
                }

                GameObject[] roots = scene.GetRootGameObjects();

                for (int r = 0; r < roots.Length; r++)
                {
                    UIAssetRegistryRuntime found = roots[r].GetComponentInChildren<UIAssetRegistryRuntime>(true);

                    if (found != null)
                    {
                        return found;
                    }
                }
            }

            return null;
        }

        // -----------------------------------------------------------------
        // 3. Screen_MainTitle prefab
        // -----------------------------------------------------------------
        private static void SetupPrefab(Sprite catNabi, HomeObjectSet set)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(PrefabPath);

            try
            {
                TMP_FontAsset font = ExtractTaglineFont(root);

                int sleepFlashSibling = GetSiblingIndexOfChild(root, "Overlay_Cream") + 1;
                int objectLayerSibling = sleepFlashSibling;
                int nabiSibling = objectLayerSibling + 1;
                int ambientSibling = nabiSibling + 1;

                GameObject objectLayerGo = EnsureChild(root, "HomeObjectLayer", out bool createdLayer);
                ConfigureRectStretch(objectLayerGo);
                HomeObjectLayer objectLayer = EnsureComponent<HomeObjectLayer>(objectLayerGo);
                if (createdLayer == true)
                {
                    objectLayerGo.transform.SetSiblingIndex(objectLayerSibling);
                }

                GameObject doorLightGo = EnsureChild(objectLayerGo, "DoorLightZone", out _);
                ConfigureRectAnchored(doorLightGo, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-180f, 260f), new Vector2(320f, 420f));
                EnsureInvisibleTouchImage(doorLightGo, raycast: true);

                GameObject sleepFlashGo = EnsureChild(objectLayerGo, "SleepFlashOverlay", out _);
                ConfigureRectStretch(sleepFlashGo);
                Image sleepFlashImage = EnsureComponent<Image>(sleepFlashGo);
                sleepFlashImage.color = new Color(1f, 1f, 1f, 0.01f);
                sleepFlashImage.raycastTarget = false;
                sleepFlashImage.sprite = null;

                ApplyHomeObjectLayerFields(objectLayer, set, doorLightGo.GetComponent<RectTransform>(), sleepFlashImage);

                GameObject nabiGo = EnsureChild(root, "HomeNabi", out bool createdNabi);
                ConfigureRectAnchored(nabiGo, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f), new Vector2(-260f, 680f), new Vector2(280f, 320f));
                HomeNabi nabi = EnsureComponent<HomeNabi>(nabiGo);
                if (createdNabi == true)
                {
                    nabiGo.transform.SetSiblingIndex(nabiSibling);
                }

                GameObject bodyGo = EnsureChild(nabiGo, "Body", out _);
                ConfigureRectStretch(bodyGo);
                Image bodyImage = EnsureComponent<Image>(bodyGo);
                bodyImage.sprite = catNabi;
                bodyImage.color = Color.white;
                bodyImage.preserveAspect = true;
                bodyImage.raycastTarget = false;

                GameObject earGo = EnsureChild(bodyGo, "EarLeft", out _);
                ConfigureRectAnchored(earGo, new Vector2(0.3f, 0.85f), new Vector2(0.3f, 0.85f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(80f, 80f));

                GameObject eyeMaskGo = EnsureChild(nabiGo, "EyeMask", out _);
                ConfigureRectStretch(eyeMaskGo);
                CanvasGroup eyeCg = EnsureComponent<CanvasGroup>(eyeMaskGo);
                eyeCg.alpha = 0f;
                eyeCg.blocksRaycasts = false;
                eyeCg.interactable = false;
                Image eyeImage = EnsureComponent<Image>(eyeMaskGo);
                eyeImage.color = new Color(0f, 0f, 0f, 0.6f);
                eyeImage.raycastTarget = false;
                eyeImage.sprite = null;

                GameObject touchGo = EnsureChild(nabiGo, "TouchZone", out _);
                ConfigureRectStretch(touchGo);
                Image touchImage = EnsureInvisibleTouchImage(touchGo, raycast: true);

                ApplyHomeNabiFields(nabi, bodyGo.GetComponent<RectTransform>(), earGo.GetComponent<RectTransform>(), eyeCg, touchImage);

                GameObject ambientGo = EnsureChild(root, "HomeAmbientLayer", out bool createdAmbient);
                ConfigureRectStretch(ambientGo);
                HomeAmbientLayer ambient = EnsureComponent<HomeAmbientLayer>(ambientGo);
                if (createdAmbient == true)
                {
                    ambientGo.transform.SetSiblingIndex(ambientSibling);
                }

                GameObject lanternGo = EnsureChild(ambientGo, "LanternGlow", out _);
                ConfigureRectAnchored(lanternGo, new Vector2(0.16f, 0.62f), new Vector2(0.16f, 0.62f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(220f, 220f));
                Image lanternImage = EnsureComponent<Image>(lanternGo);
                lanternImage.color = new Color(1f, 0.85f, 0.55f, 0.4f);
                lanternImage.raycastTarget = false;
                lanternImage.sprite = null;

                GameObject leafGo = EnsureChild(ambientGo, "LeafSway", out _);
                ConfigureRectAnchored(leafGo, new Vector2(0.1f, 0.85f), new Vector2(0.1f, 0.85f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(400f, 400f));

                ApplyHomeAmbientFields(ambient, lanternImage, leafGo.GetComponent<RectTransform>());

                GameObject copyGo = EnsureChild(root, "TXT_HomeCopy", out bool createdCopy);
                ConfigureRectAnchored(copyGo, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f), new Vector2(0f, 220f), new Vector2(900f, 70f));
                TextMeshProUGUI copyTmp = EnsureComponent<TextMeshProUGUI>(copyGo);
                copyTmp.text = "오늘도 여기 있어요.";
                copyTmp.fontSize = 38f;
                copyTmp.alignment = TextAlignmentOptions.Center;
                copyTmp.color = UILayoutConstants.Cream;
                copyTmp.raycastTarget = false;
                copyTmp.alpha = 0f;
                if (font != null)
                {
                    copyTmp.font = font;
                }
                HomeCopyFader fader = EnsureComponent<HomeCopyFader>(copyGo);
                ApplyHomeCopyFaderField(fader, copyTmp);
                if (createdCopy == true)
                {
                    int taglineIdx = GetSiblingIndexOfChild(root, "TXT_Tagline");
                    if (taglineIdx >= 0)
                    {
                        copyGo.transform.SetSiblingIndex(taglineIdx + 1);
                    }
                }

                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                Debug.Log("[Setup Home Screen] Screen_MainTitle.prefab updated.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        // -----------------------------------------------------------------
        // GameObject / Component helpers
        // -----------------------------------------------------------------
        private static GameObject EnsureChild(GameObject parent, string childName, out bool created)
        {
            Transform existing = parent.transform.Find(childName);

            if (existing != null)
            {
                created = false;
                return existing.gameObject;
            }

            GameObject go = new GameObject(childName, typeof(RectTransform));
            go.transform.SetParent(parent.transform, false);
            created = true;
            return go;
        }

        private static T EnsureComponent<T>(GameObject go) where T : Component
        {
            T component = go.GetComponent<T>();

            if (component == null)
            {
                component = go.AddComponent<T>();
            }

            return component;
        }

        private static void ConfigureRectStretch(GameObject go)
        {
            RectTransform rect = go.GetComponent<RectTransform>();

            if (rect == null)
            {
                rect = go.AddComponent<RectTransform>();
            }

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
        }

        private static void ConfigureRectAnchored(GameObject go, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 size)
        {
            RectTransform rect = go.GetComponent<RectTransform>();

            if (rect == null)
            {
                rect = go.AddComponent<RectTransform>();
            }

            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = size;
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
        }

        private static Image EnsureInvisibleTouchImage(GameObject go, bool raycast)
        {
            Image image = EnsureComponent<Image>(go);
            image.color = new Color(1f, 1f, 1f, 0.01f);
            image.sprite = null;
            image.raycastTarget = raycast;
            return image;
        }

        private static int GetSiblingIndexOfChild(GameObject parent, string childName)
        {
            Transform child = parent.transform.Find(childName);
            return child != null ? child.GetSiblingIndex() : -1;
        }

        private static TMP_FontAsset ExtractTaglineFont(GameObject root)
        {
            Transform tagline = root.transform.Find("TXT_Tagline");

            if (tagline == null)
            {
                return null;
            }

            TextMeshProUGUI tmp = tagline.GetComponent<TextMeshProUGUI>();
            return tmp != null ? tmp.font : null;
        }

        // -----------------------------------------------------------------
        // SerializedObject field writers (private [SerializeField] 안전 접근)
        // -----------------------------------------------------------------
        private static void ApplyHomeObjectLayerFields(HomeObjectLayer layer, HomeObjectSet set, RectTransform doorLight, Image sleepFlash)
        {
            SerializedObject so = new SerializedObject(layer);
            so.FindProperty("set").objectReferenceValue = set;
            so.FindProperty("doorLightZone").objectReferenceValue = doorLight;
            so.FindProperty("sleepFlashOverlay").objectReferenceValue = sleepFlash;
            // installer는 런타임에 FindObjectOfType으로 자동 검색되므로 비워둠
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ApplyHomeNabiFields(HomeNabi nabi, RectTransform body, RectTransform earLeft, CanvasGroup eyeMask, Image touchZone)
        {
            SerializedObject so = new SerializedObject(nabi);
            so.FindProperty("body").objectReferenceValue = body;
            so.FindProperty("earLeft").objectReferenceValue = earLeft;
            so.FindProperty("eyeMask").objectReferenceValue = eyeMask;
            so.FindProperty("touchZoneImage").objectReferenceValue = touchZone;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ApplyHomeAmbientFields(HomeAmbientLayer ambient, Image lanternGlow, RectTransform leafSway)
        {
            SerializedObject so = new SerializedObject(ambient);
            so.FindProperty("lanternGlow").objectReferenceValue = lanternGlow;
            so.FindProperty("leafSway").objectReferenceValue = leafSway;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ApplyHomeCopyFaderField(HomeCopyFader fader, TextMeshProUGUI tmp)
        {
            SerializedObject so = new SerializedObject(fader);
            so.FindProperty("text").objectReferenceValue = tmp;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
