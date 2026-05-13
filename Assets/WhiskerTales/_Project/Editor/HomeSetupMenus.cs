#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using WhiskerTales.Runtime;

namespace WhiskerTales.EditorTools
{
    public static class HomeSetupMenus
    {
        private const string HomePrefabPath = "Assets/WhiskerTales/_Project/Prefabs/UI/Screens/HomeScreen.prefab";
        private const string HomeArtDir = "Assets/WhiskerTales/Art/01_MAIN_HOME";
        private const string AmbienceDir = "Assets/WhiskerTales/Audio/Ambience";
        private const string NabiDir = "Assets/WhiskerTales/Audio/Nabi";

        [MenuItem("Whisker Tales/Setup/Setup Time Of Day")]
        public static void SetupTimeOfDay()
        {
            if (EditorApplication.isPlaying == true)
            {
                Debug.LogWarning("[HomeSetupMenus] Cannot run during Play Mode.");
                return;
            }

            GameObject prefabRoot = LoadPrefabContents();

            if (prefabRoot == null)
            {
                Debug.LogError("[HomeSetupMenus] HomeScreen prefab not found at " + HomePrefabPath);
                return;
            }

            try
            {
                HomeTimeOfDayController controller = EnsureTimeOfDayController(prefabRoot);
                BindSpritePools(controller);
                SaveAndUnload(prefabRoot);
                Debug.Log("[HomeSetupMenus] Setup Time Of Day complete.");
            }
            catch (System.Exception e)
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
                Debug.LogError("[HomeSetupMenus] Setup Time Of Day failed: " + e.Message);
            }
        }

        [MenuItem("Whisker Tales/Setup/Setup Ambience Controller")]
        public static void SetupAmbienceController()
        {
            if (EditorApplication.isPlaying == true)
            {
                Debug.LogWarning("[HomeSetupMenus] Cannot run during Play Mode.");
                return;
            }

            GameObject prefabRoot = LoadPrefabContents();

            if (prefabRoot == null)
            {
                Debug.LogError("[HomeSetupMenus] HomeScreen prefab not found at " + HomePrefabPath);
                return;
            }

            try
            {
                HomeTimeOfDayController tod = EnsureTimeOfDayController(prefabRoot);
                HomeAmbienceController controller = EnsureAmbienceController(prefabRoot, tod);
                BindAmbienceClips(controller);
                SaveAndUnload(prefabRoot);
                Debug.Log("[HomeSetupMenus] Setup Ambience Controller complete.");
            }
            catch (System.Exception e)
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
                Debug.LogError("[HomeSetupMenus] Setup Ambience Controller failed: " + e.Message);
            }
        }

        private static GameObject LoadPrefabContents()
        {
            if (File.Exists(HomePrefabPath) == false)
            {
                return null;
            }

            return PrefabUtility.LoadPrefabContents(HomePrefabPath);
        }

        private static void SaveAndUnload(GameObject root)
        {
            PrefabUtility.SaveAsPrefabAsset(root, HomePrefabPath);
            PrefabUtility.UnloadPrefabContents(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static HomeTimeOfDayController EnsureTimeOfDayController(GameObject root)
        {
            HomeTimeOfDayController existing = root.GetComponentInChildren<HomeTimeOfDayController>(true);

            if (existing != null)
            {
                EnsureBackgroundImages(existing);
                return existing;
            }

            GameObject host = new GameObject("TimeOfDay", typeof(RectTransform));
            host.transform.SetParent(root.transform, false);
            RectTransform rect = host.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            host.transform.SetAsFirstSibling();

            HomeTimeOfDayController controller = host.AddComponent<HomeTimeOfDayController>();
            EnsureBackgroundImages(controller);
            return controller;
        }

        private static void EnsureBackgroundImages(HomeTimeOfDayController controller)
        {
            SerializedObject so = new SerializedObject(controller);
            SerializedProperty propA = so.FindProperty("backgroundA");
            SerializedProperty propB = so.FindProperty("backgroundB");

            Image imgA = propA.objectReferenceValue as Image;
            Image imgB = propB.objectReferenceValue as Image;

            if (imgA == null)
            {
                imgA = CreateBackgroundImage(controller.transform, "Background_A");
                propA.objectReferenceValue = imgA;
            }

            if (imgB == null)
            {
                imgB = CreateBackgroundImage(controller.transform, "Background_B");
                propB.objectReferenceValue = imgB;
            }

            imgA.transform.SetSiblingIndex(0);
            imgB.transform.SetSiblingIndex(1);

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Image CreateBackgroundImage(Transform parent, string name)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            Image image = go.GetComponent<Image>();
            image.preserveAspect = false;
            image.raycastTarget = false;
            image.color = Color.white;
            return image;
        }

        private static HomeAmbienceController EnsureAmbienceController(GameObject root, HomeTimeOfDayController tod)
        {
            HomeAmbienceController existing = root.GetComponentInChildren<HomeAmbienceController>(true);

            if (existing == null)
            {
                GameObject host = new GameObject("Ambience", typeof(RectTransform));
                host.transform.SetParent(root.transform, false);
                existing = host.AddComponent<HomeAmbienceController>();
            }

            SerializedObject so = new SerializedObject(existing);
            so.FindProperty("timeOfDayController").objectReferenceValue = tod;

            EnsureAudioSource(existing.transform, "AmbienceSource_A", so, "ambienceSource");
            EnsureAudioSource(existing.transform, "AmbienceSource_B", so, "ambienceSourceB");
            EnsureAudioSource(existing.transform, "NabiSource", so, "nabiSource");

            so.ApplyModifiedPropertiesWithoutUndo();
            return existing;
        }

        private static void EnsureAudioSource(Transform parent, string name, SerializedObject so, string propertyName)
        {
            SerializedProperty prop = so.FindProperty(propertyName);
            AudioSource source = prop.objectReferenceValue as AudioSource;

            if (source != null)
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
