#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using WhiskerTales.Runtime;

namespace WhiskerTales.EditorTools
{
    public static class HomeSetupMenus
    {
        private const string ScenePath = "Assets/WhiskerTales/_Project/Scenes/Main_App.unity";
        private const string HomeArtDir = "Assets/WhiskerTales/Art/01_MAIN_HOME";
        private const string AmbienceDir = "Assets/WhiskerTales/Audio/Ambience";
        private const string NabiDir = "Assets/WhiskerTales/Audio/Nabi";

        private const string CanvasBackgroundName = "Canvas_Background";
        private const string HomeBackgroundName = "HomeBackground";
        private const string HomeAmbienceName = "HomeAmbience";
        private const string LayerAName = "LayerA";
        private const string LayerBName = "LayerB";

        [MenuItem("Whisker Tales/Setup/Setup Time Of Day")]
        public static void SetupTimeOfDay()
        {
            if (EditorApplication.isPlaying == true)
            {
                Debug.LogWarning("[HomeSetupMenus] Cannot run during Play Mode.");
                return;
            }

            UnityEngine.SceneManagement.Scene scene;

            if (OpenMainAppScene(out scene) == false)
            {
                Debug.LogError("[HomeSetupMenus] Setup Time Of Day FAIL — could not open " + ScenePath);
                return;
            }

            Transform canvasBg = FindCanvasBackground(scene);

            if (canvasBg == null)
            {
                Debug.LogError("[HomeSetupMenus] Setup Time Of Day FAIL — " + CanvasBackgroundName + " not found in scene.");
                return;
            }

            HomeTimeOfDayController controller = EnsureTimeOfDayController(canvasBg);
            BindSpritePools(controller);
            SaveScene(scene);
            Debug.Log("[HomeSetupMenus] Setup Time Of Day PASS — HomeTimeOfDayController attached under " + CanvasBackgroundName + "/" + HomeBackgroundName + ".");
        }

        [MenuItem("Whisker Tales/Setup/Setup Ambience Controller")]
        public static void SetupAmbienceController()
        {
            if (EditorApplication.isPlaying == true)
            {
                Debug.LogWarning("[HomeSetupMenus] Cannot run during Play Mode.");
                return;
            }

            UnityEngine.SceneManagement.Scene scene;

            if (OpenMainAppScene(out scene) == false)
            {
                Debug.LogError("[HomeSetupMenus] Setup Ambience Controller FAIL — could not open " + ScenePath);
                return;
            }

            Transform canvasBg = FindCanvasBackground(scene);

            if (canvasBg == null)
            {
                Debug.LogError("[HomeSetupMenus] Setup Ambience Controller FAIL — " + CanvasBackgroundName + " not found in scene.");
                return;
            }

            HomeTimeOfDayController tod = EnsureTimeOfDayController(canvasBg);
            HomeAmbienceController controller = EnsureAmbienceController(canvasBg, tod);
            BindAmbienceClips(controller);
            SaveScene(scene);
            Debug.Log("[HomeSetupMenus] Setup Ambience Controller PASS — HomeAmbienceController attached under " + CanvasBackgroundName + "/" + HomeAmbienceName + ".");
        }

        [MenuItem("Whisker Tales/Test/Verify Home Background Controllers")]
        public static void VerifyHomeBackground()
        {
            UnityEngine.SceneManagement.Scene scene;

            if (OpenMainAppScene(out scene) == false)
            {
                Debug.LogError("[HomeSetupMenus] Verify FAIL — could not open scene at " + ScenePath);
                return;
            }

            Transform canvasBg = FindCanvasBackground(scene);

            if (canvasBg == null)
            {
                Debug.LogError("[HomeSetupMenus] Verify FAIL — " + CanvasBackgroundName + " not found in scene.");
                return;
            }

            Transform homeBg = canvasBg.Find(HomeBackgroundName);

            if (homeBg == null)
            {
                Debug.LogError("[HomeSetupMenus] Verify FAIL — " + HomeBackgroundName + " GameObject missing.");
                return;
            }

            HomeTimeOfDayController tod = homeBg.GetComponent<HomeTimeOfDayController>();

            if (tod == null)
            {
                Debug.LogError("[HomeSetupMenus] Verify FAIL — HomeTimeOfDayController component missing on " + HomeBackgroundName + ".");
                return;
            }

            Transform layerA = homeBg.Find(LayerAName);
            Transform layerB = homeBg.Find(LayerBName);

            if (layerA == null || layerB == null)
            {
                Debug.LogError("[HomeSetupMenus] Verify FAIL — LayerA/LayerB GameObject missing.");
                return;
            }

            if (layerA.GetComponent<Image>() == null || layerB.GetComponent<Image>() == null)
            {
                Debug.LogError("[HomeSetupMenus] Verify FAIL — LayerA/LayerB Image component missing.");
                return;
            }

            SerializedObject todSo = new SerializedObject(tod);

            if (todSo.FindProperty("backgroundA").objectReferenceValue == null ||
                todSo.FindProperty("backgroundB").objectReferenceValue == null)
            {
                Debug.LogError("[HomeSetupMenus] Verify FAIL — HomeTimeOfDayController backgroundA/B references unassigned.");
                return;
            }

            Transform ambHost = canvasBg.Find(HomeAmbienceName);

            if (ambHost == null)
            {
                Debug.LogError("[HomeSetupMenus] Verify FAIL — " + HomeAmbienceName + " GameObject missing.");
                return;
            }

            HomeAmbienceController amb = ambHost.GetComponent<HomeAmbienceController>();

            if (amb == null)
            {
                Debug.LogError("[HomeSetupMenus] Verify FAIL — HomeAmbienceController component missing.");
                return;
            }

            SerializedObject ambSo = new SerializedObject(amb);

            if (ambSo.FindProperty("timeOfDayController").objectReferenceValue == null ||
                ambSo.FindProperty("ambienceSource").objectReferenceValue == null ||
                ambSo.FindProperty("ambienceSourceB").objectReferenceValue == null ||
                ambSo.FindProperty("nabiSource").objectReferenceValue == null)
            {
                Debug.LogError("[HomeSetupMenus] Verify FAIL — HomeAmbienceController references unassigned.");
                return;
            }

            Debug.Log("[HomeSetupMenus] Verify Home Background Controllers PASS.");
        }

        // Called from MainAppSceneBuilder during V2 builds so HomeBackground/HomeAmbience
        // survive scene re-creation. The scene is already open in memory; we do not save it.
        public static void EnsureHomeBackgroundOnScene(UnityEngine.SceneManagement.Scene scene)
        {
            Transform canvasBg = FindCanvasBackground(scene);

            if (canvasBg == null)
            {
                Debug.LogWarning("[HomeSetupMenus] EnsureHomeBackgroundOnScene: " + CanvasBackgroundName + " not found in scene " + scene.path);
                return;
            }

            HomeTimeOfDayController tod = EnsureTimeOfDayController(canvasBg);
            BindSpritePools(tod);
            HomeAmbienceController amb = EnsureAmbienceController(canvasBg, tod);
            BindAmbienceClips(amb);
        }

        private static bool OpenMainAppScene(out UnityEngine.SceneManagement.Scene scene)
        {
            scene = default(UnityEngine.SceneManagement.Scene);

            if (File.Exists(ScenePath) == false)
            {
                return false;
            }

            UnityEngine.SceneManagement.Scene active = EditorSceneManager.GetActiveScene();

            if (active.path == ScenePath && active.isLoaded == true)
            {
                scene = active;
                return true;
            }

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo() == false)
            {
                return false;
            }

            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            return scene.IsValid();
        }

        private static void SaveScene(UnityEngine.SceneManagement.Scene scene)
        {
            EditorSceneManager.MarkSceneDirty(scene);

            if (EditorSceneManager.SaveScene(scene) == false)
            {
                Debug.LogError("[HomeSetupMenus] Failed to save scene at " + ScenePath);
                return;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static Transform FindCanvasBackground(UnityEngine.SceneManagement.Scene scene)
        {
            GameObject[] roots = scene.GetRootGameObjects();

            for (int i = 0; i < roots.Length; i++)
            {
                Transform found = FindRecursive(roots[i].transform, CanvasBackgroundName);

                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static Transform FindRecursive(Transform parent, string name)
        {
            if (parent.name == name)
            {
                return parent;
            }

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform result = FindRecursive(parent.GetChild(i), name);

                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private static HomeTimeOfDayController EnsureTimeOfDayController(Transform canvasBg)
        {
            Transform existing = canvasBg.Find(HomeBackgroundName);
            GameObject host;

            if (existing != null)
            {
                host = existing.gameObject;
            }
            else
            {
                host = new GameObject(HomeBackgroundName, typeof(RectTransform));
                host.transform.SetParent(canvasBg, false);
                RectTransform rect = (RectTransform)host.transform;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                host.transform.SetAsFirstSibling();
            }

            HomeTimeOfDayController controller = host.GetComponent<HomeTimeOfDayController>();

            if (controller == null)
            {
                controller = host.AddComponent<HomeTimeOfDayController>();
            }

            EnsureBackgroundImages(controller);
            return controller;
        }

        private static void EnsureBackgroundImages(HomeTimeOfDayController controller)
        {
            SerializedObject so = new SerializedObject(controller);
            SerializedProperty propA = so.FindProperty("backgroundA");
            SerializedProperty propB = so.FindProperty("backgroundB");

            Image imgA = EnsureLayerImage(controller.transform, LayerAName, 0);
            Image imgB = EnsureLayerImage(controller.transform, LayerBName, 1);

            propA.objectReferenceValue = imgA;
            propB.objectReferenceValue = imgB;

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Image EnsureLayerImage(Transform parent, string name, int siblingIndex)
        {
            Transform existing = parent.Find(name);
            GameObject go;

            if (existing != null)
            {
                go = existing.gameObject;
            }
            else
            {
                go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                go.transform.SetParent(parent, false);
            }

            RectTransform rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.SetSiblingIndex(siblingIndex);

            Image image = go.GetComponent<Image>();

            if (image == null)
            {
                image = go.AddComponent<Image>();
            }

            image.preserveAspect = false;
            image.raycastTarget = false;
            image.color = Color.white;
            return image;
        }

        private static HomeAmbienceController EnsureAmbienceController(Transform canvasBg, HomeTimeOfDayController tod)
        {
            Transform existing = canvasBg.Find(HomeAmbienceName);
            GameObject host;

            if (existing != null)
            {
                host = existing.gameObject;
            }
            else
            {
                host = new GameObject(HomeAmbienceName);
                host.transform.SetParent(canvasBg, false);
            }

            HomeAmbienceController controller = host.GetComponent<HomeAmbienceController>();

            if (controller == null)
            {
                controller = host.AddComponent<HomeAmbienceController>();
            }

            SerializedObject so = new SerializedObject(controller);
            so.FindProperty("timeOfDayController").objectReferenceValue = tod;

            EnsureAudioSource(host.transform, "AmbienceSource_A", so, "ambienceSource");
            EnsureAudioSource(host.transform, "AmbienceSource_B", so, "ambienceSourceB");
            EnsureAudioSource(host.transform, "NabiSource", so, "nabiSource");

            so.ApplyModifiedPropertiesWithoutUndo();
            return controller;
        }

        private static void EnsureAudioSource(Transform parent, string name, SerializedObject so, string propertyName)
        {
            SerializedProperty prop = so.FindProperty(propertyName);
            AudioSource source = prop.objectReferenceValue as AudioSource;

            if (source != null && source.transform.parent == parent)
            {
                return;
            }

            Transform existingChild = parent.Find(name);

            if (existingChild != null)
            {
                source = existingChild.GetComponent<AudioSource>();
            }

            if (source == null)
            {
                GameObject go = new GameObject(name);
                go.transform.SetParent(parent, false);
                source = go.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.loop = true;
                source.spatialBlend = 0f;
                source.volume = 0f;
            }

            prop.objectReferenceValue = source;
        }

        private static void BindSpritePools(HomeTimeOfDayController controller)
        {
            SerializedObject so = new SerializedObject(controller);
            BindSpriteArray(so, "dawnSprites", HomeArtDir, "bg_dawn_");
            BindSpriteArray(so, "morningSprites", HomeArtDir, "bg_morning_");
            BindSpriteArray(so, "daySprites", HomeArtDir, "bg_day_");
            BindSpriteArray(so, "eveningSprites", HomeArtDir, "bg_evening_");
            BindSpriteArray(so, "nightSprites", HomeArtDir, "bg_night_");
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void BindSpriteArray(SerializedObject so, string propertyName, string folder, string prefix)
        {
            SerializedProperty prop = so.FindProperty(propertyName);

            if (prop == null)
            {
                return;
            }

            List<Sprite> sprites = LoadAssetsByPrefix<Sprite>(folder, prefix);
            prop.arraySize = sprites.Count;

            for (int i = 0; i < sprites.Count; i++)
            {
                prop.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
            }
        }

        private static void BindAmbienceClips(HomeAmbienceController controller)
        {
            SerializedObject so = new SerializedObject(controller);
            BindClipArray(so, "dawnClips", AmbienceDir, new[] { "amb_dawn_wind", "amb_dawn_breeze" });
            BindClipArray(so, "dayClips", AmbienceDir, new[] { "amb_day_wind", "amb_day_breeze" });
            BindClipArray(so, "eveningClips", AmbienceDir, new[] { "amb_evening_wind", "amb_evening_breeze" });
            BindClipArray(so, "nightClips", AmbienceDir, new[] { "amb_night_cricket_v01", "amb_night_cricket_v02" });
            BindClipArrayByPrefix(so, "nabiSoundClips", NabiDir, "nabi_");
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void BindClipArray(SerializedObject so, string propertyName, string folder, string[] baseNames)
        {
            SerializedProperty prop = so.FindProperty(propertyName);

            if (prop == null)
            {
                return;
            }

            List<AudioClip> clips = new List<AudioClip>();

            for (int i = 0; i < baseNames.Length; i++)
            {
                AudioClip clip = LoadFirstAssetByPrefix<AudioClip>(folder, baseNames[i]);

                if (clip != null)
                {
                    clips.Add(clip);
                }
            }

            prop.arraySize = clips.Count;

            for (int i = 0; i < clips.Count; i++)
            {
                prop.GetArrayElementAtIndex(i).objectReferenceValue = clips[i];
            }
        }

        private static void BindClipArrayByPrefix(SerializedObject so, string propertyName, string folder, string prefix)
        {
            SerializedProperty prop = so.FindProperty(propertyName);

            if (prop == null)
            {
                return;
            }

            List<AudioClip> clips = LoadAssetsByPrefix<AudioClip>(folder, prefix);
            prop.arraySize = clips.Count;

            for (int i = 0; i < clips.Count; i++)
            {
                prop.GetArrayElementAtIndex(i).objectReferenceValue = clips[i];
            }
        }

        private static List<T> LoadAssetsByPrefix<T>(string folder, string prefix) where T : Object
        {
            List<T> result = new List<T>();

            if (AssetDatabase.IsValidFolder(folder) == false)
            {
                return result;
            }

            string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name, new[] { folder });

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                string fileName = Path.GetFileNameWithoutExtension(path);

                if (fileName.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase) == false)
                {
                    continue;
                }

                T asset = AssetDatabase.LoadAssetAtPath<T>(path);

                if (asset != null)
                {
                    result.Add(asset);
                }
            }

            result.Sort(delegate (T a, T b) { return string.Compare(a.name, b.name, System.StringComparison.OrdinalIgnoreCase); });
            return result;
        }

        private static T LoadFirstAssetByPrefix<T>(string folder, string prefix) where T : Object
        {
            List<T> all = LoadAssetsByPrefix<T>(folder, prefix);
            return all.Count > 0 ? all[0] : null;
        }
    }
}
#endif
