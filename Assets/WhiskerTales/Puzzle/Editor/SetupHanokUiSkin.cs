#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace WhiskerTales.EditorTools
{
    public static class SetupHanokUiSkin
    {
        private const string ScenePath = "Assets/WhiskerTales/Puzzle/Skin/Scenes/WhiskerGameScene.unity";
        private const string FontPath  = "Assets/WhiskerTales/Art/Fonts/NanumMyeongjo-Regular.ttf";
        private const string HanjiDir  = "Assets/WhiskerTales/Art/UI/Hanok";
        private const string HanjiPath = "Assets/WhiskerTales/Art/UI/Hanok/hanji_panel.png";

        private static readonly Color InkColor = new Color32(0x2B, 0x2A, 0x26, 0xFF);

        private const int HanjiSize   = 512;
        private const int HanjiBorder = 48;

        private const string CardName = "LeftHanjiCard";
        private static readonly Vector2 CardSize       = new Vector2(280f, 540f);
        private static readonly Vector2 CardAnchorPos  = new Vector2(170f, -560f);

        private static readonly Color BoosterButtonBgColor = new Color32(0xC8, 0xA9, 0x6E, 0xFF);
        private static readonly Color BoosterBadgeColor    = new Color32(0xD9, 0x40, 0x40, 0xFF);
        private static readonly Vector2 BoosterBarSize        = new Vector2(140f, 600f);
        private static readonly Vector2 BoosterBarAnchoredPos = new Vector2(-90f, 0f);

        private const string BoardPanelName = "GameBoardPanel";
        private static readonly Color BoardPanelColor   = new Color32(0xED, 0xE0, 0xC4, 0xFF);
        private static readonly Color BoardOutlineColor = new Color32(0x8B, 0x69, 0x14, 0xFF);
        private static readonly Vector2 BoardPanelSize        = new Vector2(900f, 900f);
        private static readonly Vector2 BoardPanelAnchoredPos = new Vector2(0f, -80f);
        private static readonly Vector2 BoardOutlineDistance  = new Vector2(3f, -3f);
        private const string GameConfigPath = "Assets/Vendor/CandyMatch3Kit/Resources/game_configuration.json";

        // =========================================================================
        // Phase 0 — Hanji texture generator
        // =========================================================================

        [MenuItem("WhiskerTales/Puzzle/Setup Hanok UI Skin/Generate Hanji Panel Sprite")]
        public static void GenerateHanjiSprite()
        {
            EnsureFolder(HanjiDir);

            var tex = new Texture2D(HanjiSize, HanjiSize, TextureFormat.RGBA32, false);
            Color paperLight = new Color(0.96f, 0.91f, 0.79f, 1f);
            Color paperDark  = new Color(0.78f, 0.70f, 0.55f, 1f);

            var rng = new System.Random(7777);
            for (int y = 0; y < HanjiSize; y++)
            {
                for (int x = 0; x < HanjiSize; x++)
                {
                    float n1 = Mathf.PerlinNoise(x * 0.040f, y * 0.040f);
                    float n2 = Mathf.PerlinNoise(x * 0.180f + 100f, y * 0.180f + 100f);
                    float noise = n1 * 0.7f + n2 * 0.3f;

                    float speckle = (float)rng.NextDouble();
                    float specMul = 1f;
                    if (speckle < 0.004f)
                    {
                        specMul = 0.78f;
                    }

                    Color c = Color.Lerp(paperDark, paperLight, noise) * specMul;
                    c.a = 1f;
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();

            byte[] png = tex.EncodeToPNG();
            File.WriteAllBytes(HanjiPath, png);
            Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(HanjiPath, ImportAssetOptions.ForceUpdate);

            var importer = AssetImporter.GetAtPath(HanjiPath) as TextureImporter;
            if (importer == null)
            {
                Debug.LogError($"[HanokUI] Failed to get TextureImporter for {HanjiPath}");
                return;
            }
            importer.textureType      = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100f;
            importer.filterMode       = FilterMode.Bilinear;
            importer.mipmapEnabled    = false;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteBorder = new Vector4(HanjiBorder, HanjiBorder, HanjiBorder, HanjiBorder);
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();

            Debug.Log($"[HanokUI] Generated {HanjiPath} ({HanjiSize}x{HanjiSize}, 9-slice {HanjiBorder}px)");
        }

        // =========================================================================
        // Phase 1 — Apply NanumMyeongjo font + ink color (#2B2A26) to legacy Text
        // =========================================================================

        [MenuItem("WhiskerTales/Puzzle/Setup Hanok UI Skin/Phase 1 - Font and Colors")]
        public static void Phase1Apply()
        {
            var font = LoadFont();
            if (font == null)
            {
                return;
            }

            var scene = OpenScene();
            var canvas = FindGameUICanvas(scene);
            if (canvas == null)
            {
                Debug.LogError("[Phase1] GameUICanvas not found in scene");
                return;
            }

            var texts = canvas.GetComponentsInChildren<Text>(true);
            int changed = 0;
            foreach (var t in texts)
            {
                Undo.RecordObject(t, "Phase1 Font+Color");
                t.font = font;
                t.color = InkColor;
                EditorUtility.SetDirty(t);
                changed++;
            }

            SaveAndRefresh(scene);
            Debug.Log($"[Phase1] Applied NanumMyeongjo + #2B2A26 to {changed} Text components under GameUICanvas");
        }

        [MenuItem("WhiskerTales/Puzzle/Setup Hanok UI Skin/Test/Phase 1 - Font and Colors")]
        public static void Phase1Test()
        {
            var font = LoadFont();
            if (font == null)
            {
                Debug.Log("[Phase1 TEST] FAIL — font asset missing");
                return;
            }

            var scene = OpenScene();
            var canvas = FindGameUICanvas(scene);
            if (canvas == null)
            {
                Debug.Log("[Phase1 TEST] FAIL — GameUICanvas not found");
                return;
            }

            var texts = canvas.GetComponentsInChildren<Text>(true);
            int total = texts.Length;
            int fontOK = 0;
            int colorOK = 0;
            var mismatchNames = new List<string>();
            foreach (var t in texts)
            {
                bool fOk = (t.font == font);
                bool cOk = ColorApproxEquals(t.color, InkColor);
                if (fOk)
                {
                    fontOK++;
                }
                if (cOk)
                {
                    colorOK++;
                }
                if (!fOk || !cOk)
                {
                    mismatchNames.Add($"{GetPath(t.transform)} (font={fOk}, color={cOk})");
                }
            }

            bool pass = (total > 0 && fontOK == total && colorOK == total);
            string status = pass ? "PASS" : "FAIL";
            Debug.Log($"[Phase1 TEST] {status} — total={total}, fontOK={fontOK}, colorOK={colorOK}");
            if (!pass)
            {
                foreach (var m in mismatchNames.Take(10))
                {
                    Debug.Log($"  - mismatch: {m}");
                }
            }
        }

        // =========================================================================
        // Phase 2 — TopBar Background → hanji_panel (Sliced)
        // =========================================================================

        [MenuItem("WhiskerTales/Puzzle/Setup Hanok UI Skin/Phase 2 - Hanji TopBar")]
        public static void Phase2Apply()
        {
            var sprite = LoadHanji();
            if (sprite == null)
            {
                return;
            }

            var scene = OpenScene();
            var canvas = FindGameUICanvas(scene);
            if (canvas == null)
            {
                Debug.LogError("[Phase2] GameUICanvas not found");
                return;
            }

            var bg = FindTopBarBackground(canvas);
            if (bg == null)
            {
                Debug.LogError("[Phase2] GameUICanvas/TopBar/Background Image not found");
                return;
            }

            Undo.RecordObject(bg, "Phase2 Hanji TopBar");
            bg.sprite = sprite;
            bg.type = Image.Type.Sliced;
            EditorUtility.SetDirty(bg);

            SaveAndRefresh(scene);
            Debug.Log("[Phase2] TopBar/Background → hanji_panel, type → Sliced");
        }

        [MenuItem("WhiskerTales/Puzzle/Setup Hanok UI Skin/Test/Phase 2 - Hanji TopBar")]
        public static void Phase2Test()
        {
            var sprite = LoadHanji();
            if (sprite == null)
            {
                Debug.Log("[Phase2 TEST] FAIL — hanji sprite missing");
                return;
            }

            var scene = OpenScene();
            var canvas = FindGameUICanvas(scene);
            if (canvas == null)
            {
                Debug.Log("[Phase2 TEST] FAIL — GameUICanvas not found");
                return;
            }

            var bg = FindTopBarBackground(canvas);
            if (bg == null)
            {
                Debug.Log("[Phase2 TEST] FAIL — TopBar/Background Image not found");
                return;
            }

            bool spriteOK = (bg.sprite == sprite);
            bool typeOK = (bg.type == Image.Type.Sliced);
            bool pass = spriteOK && typeOK;
            string status = pass ? "PASS" : "FAIL";
            Debug.Log($"[Phase2 TEST] {status} — sprite={spriteOK}, type=Sliced({typeOK})");
        }

        // =========================================================================
        // Phase 3 — Left vertical hanji card with STAGE / MOVES / GOAL labels
        // (new GameObject only; existing Kit-bound Text components are NOT touched
        //  — visual integration of dynamic counters is a manual follow-up step.)
        // =========================================================================

        [MenuItem("WhiskerTales/Puzzle/Setup Hanok UI Skin/Phase 3 - Left Hanji Card")]
        public static void Phase3Build()
        {
            var sprite = LoadHanji();
            if (sprite == null)
            {
                return;
            }
            var font = LoadFont();
            if (font == null)
            {
                return;
            }

            var scene = OpenScene();
            var canvas = FindGameUICanvas(scene);
            if (canvas == null)
            {
                Debug.LogError("[Phase3] GameUICanvas not found");
                return;
            }

            var existing = FindRecursive(canvas.transform, CardName);
            if (existing != null)
            {
                Undo.DestroyObjectImmediate(existing.gameObject);
                Debug.Log($"[Phase3] Existing {CardName} removed for rebuild");
            }

            var card = new GameObject(CardName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            Undo.RegisterCreatedObjectUndo(card, "Phase3 Card");
            card.layer = canvas.layer;
            card.transform.SetParent(canvas.transform, false);

            var rt = (RectTransform)card.transform;
            rt.anchorMin        = new Vector2(0f, 1f);
            rt.anchorMax        = new Vector2(0f, 1f);
            rt.pivot            = new Vector2(0.5f, 0.5f);
            rt.sizeDelta        = CardSize;
            rt.anchoredPosition = CardAnchorPos;

            var img = card.GetComponent<Image>();
            img.sprite = sprite;
            img.type   = Image.Type.Sliced;
            img.color  = Color.white;

            string[] labels = { "STAGE", "MOVES", "GOAL" };
            float rowH = CardSize.y / labels.Length;
            for (int i = 0; i < labels.Length; i++)
            {
                CreateCardLabel(card.transform, labels[i], font, i, rowH);
            }

            SaveAndRefresh(scene);
            Debug.Log($"[Phase3] {CardName} built with {labels.Length} labels under GameUICanvas — Kit bindings untouched");
        }

        [MenuItem("WhiskerTales/Puzzle/Setup Hanok UI Skin/Test/Phase 3 - Left Hanji Card")]
        public static void Phase3Test()
        {
            var sprite = LoadHanji();
            var font = LoadFont();
            if (sprite == null || font == null)
            {
                Debug.Log("[Phase3 TEST] FAIL — asset(s) missing");
                return;
            }

            var scene = OpenScene();
            var canvas = FindGameUICanvas(scene);
            if (canvas == null)
            {
                Debug.Log("[Phase3 TEST] FAIL — GameUICanvas not found");
                return;
            }

            var cardT = FindRecursive(canvas.transform, CardName);
            if (cardT == null)
            {
                Debug.Log($"[Phase3 TEST] FAIL — {CardName} not found under GameUICanvas");
                return;
            }

            var img = cardT.GetComponent<Image>();
            bool spriteOK = (img != null && img.sprite == sprite && img.type == Image.Type.Sliced);

            string[] expected = { "STAGE", "MOVES", "GOAL" };
            int labelsOK = 0;
            foreach (var lbl in expected)
            {
                var lt = cardT.Find($"Label_{lbl}");
                if (lt == null)
                {
                    continue;
                }
                var txt = lt.GetComponent<Text>();
                if (txt == null)
                {
                    continue;
                }
                bool textOK = (txt.text == lbl);
                bool fontOK = (txt.font == font);
                bool colOK  = ColorApproxEquals(txt.color, InkColor);
                if (textOK && fontOK && colOK)
                {
                    labelsOK++;
                }
            }

            bool pass = spriteOK && labelsOK == expected.Length;
            string status = pass ? "PASS" : "FAIL";
            Debug.Log($"[Phase3 TEST] {status} — card.sprite={spriteOK}, labels={labelsOK}/{expected.Length}");
        }

        // =========================================================================
        // Phase 4 — BoosterBar to right vertical column + button/badge recolor
        // =========================================================================

        [MenuItem("WhiskerTales/Puzzle/Setup Hanok UI Skin/Phase 4 - Booster Bar")]
        public static void Phase4Apply()
        {
            var scene = OpenScene();
            var canvas = FindGameUICanvas(scene);
            if (canvas == null)
            {
                Debug.LogError("[Phase4] GameUICanvas not found");
                return;
            }

            var bbT = FindRecursive(canvas.transform, "BoosterBar");
            if (bbT == null)
            {
                Debug.LogError("[Phase4] BoosterBar not found under GameUICanvas");
                return;
            }

            var brt = bbT as RectTransform;
            if (brt == null)
            {
                Debug.LogError("[Phase4] BoosterBar has no RectTransform");
                return;
            }

            Undo.RecordObject(brt, "Phase4 BoosterBar Layout");
            brt.anchorMin        = new Vector2(1f, 0.5f);
            brt.anchorMax        = new Vector2(1f, 0.5f);
            brt.pivot            = new Vector2(0.5f, 0.5f);
            brt.sizeDelta        = BoosterBarSize;
            brt.anchoredPosition = BoosterBarAnchoredPos;
            EditorUtility.SetDirty(brt);

            var vlg = bbT.GetComponent<VerticalLayoutGroup>();
            if (vlg == null)
            {
                vlg = Undo.AddComponent<VerticalLayoutGroup>(bbT.gameObject);
            }
            vlg.spacing                = 16f;
            vlg.padding                = new RectOffset(16, 16, 16, 16);
            vlg.childAlignment         = TextAnchor.MiddleCenter;
            vlg.childControlWidth      = false;
            vlg.childControlHeight     = false;
            vlg.childForceExpandWidth  = false;
            vlg.childForceExpandHeight = false;
            EditorUtility.SetDirty(vlg);

            int bgCount = 0;
            int badgeCount = 0;
            var allImages = bbT.GetComponentsInChildren<Image>(true);
            foreach (var img in allImages)
            {
                if (img.gameObject.name == "Background")
                {
                    Undo.RecordObject(img, "Phase4 Background Color");
                    img.color = BoosterButtonBgColor;
                    EditorUtility.SetDirty(img);
                    bgCount++;
                }
                else if (img.gameObject.name == "BoosterAmountCircle")
                {
                    Undo.RecordObject(img, "Phase4 Badge Color");
                    img.color = BoosterBadgeColor;
                    EditorUtility.SetDirty(img);
                    badgeCount++;
                }
            }

            SaveAndRefresh(scene);
            Debug.Log($"[Phase4] BoosterBar → right vertical {BoosterBarSize.x}x{BoosterBarSize.y} @ ({BoosterBarAnchoredPos.x},{BoosterBarAnchoredPos.y}); backgrounds recolored={bgCount}, badges recolored={badgeCount}");
        }

        [MenuItem("WhiskerTales/Puzzle/Setup Hanok UI Skin/Test/Phase 4 - Booster Bar")]
        public static void Phase4Test()
        {
            var scene = OpenScene();
            var canvas = FindGameUICanvas(scene);
            if (canvas == null)
            {
                Debug.Log("[Phase4 TEST] FAIL — GameUICanvas not found");
                return;
            }

            var bbT = FindRecursive(canvas.transform, "BoosterBar");
            if (bbT == null)
            {
                Debug.Log("[Phase4 TEST] FAIL — BoosterBar not found");
                return;
            }

            var brt = bbT as RectTransform;
            bool anchorOK = brt != null
                && Mathf.Approximately(brt.anchorMin.x, 1f) && Mathf.Approximately(brt.anchorMin.y, 0.5f)
                && Mathf.Approximately(brt.anchorMax.x, 1f) && Mathf.Approximately(brt.anchorMax.y, 0.5f);
            bool sizeOK = brt != null
                && Mathf.Approximately(brt.sizeDelta.x, BoosterBarSize.x)
                && Mathf.Approximately(brt.sizeDelta.y, BoosterBarSize.y);

            bool layoutOK = bbT.GetComponent<VerticalLayoutGroup>() != null;

            int bgOK = 0;
            int bgTotal = 0;
            int badgeOK = 0;
            int badgeTotal = 0;
            foreach (var img in bbT.GetComponentsInChildren<Image>(true))
            {
                if (img.gameObject.name == "Background")
                {
                    bgTotal++;
                    if (ColorApproxEquals(img.color, BoosterButtonBgColor))
                    {
                        bgOK++;
                    }
                }
                else if (img.gameObject.name == "BoosterAmountCircle")
                {
                    badgeTotal++;
                    if (ColorApproxEquals(img.color, BoosterBadgeColor))
                    {
                        badgeOK++;
                    }
                }
            }

            bool pass = anchorOK && sizeOK && layoutOK
                && bgTotal > 0 && bgOK == bgTotal
                && badgeTotal > 0 && badgeOK == badgeTotal;
            string status = pass ? "PASS" : "FAIL";
            Debug.Log($"[Phase4 TEST] {status} — anchor={anchorOK}, size={sizeOK}, vlg={layoutOK}, bg={bgOK}/{bgTotal}, badge={badgeOK}/{badgeTotal}");
        }

        // =========================================================================
        // Phase 5 — GameBoardPanel under BackgroundCanvas (renders behind tiles)
        // (does NOT touch game_configuration.json — see absolute rule on Vendor/.
        //  Current defaultZoomLevel is logged for manual tuning.)
        // =========================================================================

        [MenuItem("WhiskerTales/Puzzle/Setup Hanok UI Skin/Phase 5 - Game Board Panel")]
        public static void Phase5Apply()
        {
            var scene = OpenScene();
            var bgCanvas = FindCanvasByName(scene, "BackgroundCanvas");
            if (bgCanvas == null)
            {
                Debug.LogError("[Phase5] BackgroundCanvas not found in scene");
                return;
            }

            var existing = FindRecursive(bgCanvas.transform, BoardPanelName);
            if (existing != null)
            {
                Undo.DestroyObjectImmediate(existing.gameObject);
                Debug.Log($"[Phase5] Existing {BoardPanelName} removed for rebuild");
            }

            var panel = new GameObject(BoardPanelName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Outline));
            Undo.RegisterCreatedObjectUndo(panel, "Phase5 GameBoardPanel");
            panel.layer = bgCanvas.layer;
            panel.transform.SetParent(bgCanvas.transform, false);

            var prt = (RectTransform)panel.transform;
            prt.anchorMin        = new Vector2(0.5f, 0.5f);
            prt.anchorMax        = new Vector2(0.5f, 0.5f);
            prt.pivot            = new Vector2(0.5f, 0.5f);
            prt.sizeDelta        = BoardPanelSize;
            prt.anchoredPosition = BoardPanelAnchoredPos;

            var img = panel.GetComponent<Image>();
            img.color = BoardPanelColor;
            img.raycastTarget = false;

            var outline = panel.GetComponent<Outline>();
            outline.effectColor    = BoardOutlineColor;
            outline.effectDistance = BoardOutlineDistance;

            SaveAndRefresh(scene);

            float currentZoom = ReadDefaultZoomLevel();
            Debug.Log($"[Phase5] {BoardPanelName} created under BackgroundCanvas — size {BoardPanelSize.x}x{BoardPanelSize.y}, color #EDE0C4, outline #8B6914. Current defaultZoomLevel={currentZoom:F2} (in Vendor/, not auto-tuned per absolute rule)");
        }

        [MenuItem("WhiskerTales/Puzzle/Setup Hanok UI Skin/Test/Phase 5 - Game Board Panel")]
        public static void Phase5Test()
        {
            var scene = OpenScene();
            var bgCanvas = FindCanvasByName(scene, "BackgroundCanvas");
            if (bgCanvas == null)
            {
                Debug.Log("[Phase5 TEST] FAIL — BackgroundCanvas not found");
                return;
            }

            var panelT = FindRecursive(bgCanvas.transform, BoardPanelName);
            if (panelT == null)
            {
                Debug.Log($"[Phase5 TEST] FAIL — {BoardPanelName} not found under BackgroundCanvas");
                return;
            }

            var img = panelT.GetComponent<Image>();
            bool fillOK = (img != null && ColorApproxEquals(img.color, BoardPanelColor));

            var outline = panelT.GetComponent<Outline>();
            bool outlineOK = (outline != null && ColorApproxEquals(outline.effectColor, BoardOutlineColor));

            bool pass = fillOK && outlineOK;
            string status = pass ? "PASS" : "FAIL";
            float currentZoom = ReadDefaultZoomLevel();
            Debug.Log($"[Phase5 TEST] {status} — fill={fillOK}, outline={outlineOK}, defaultZoomLevel={currentZoom:F2}");
        }

        private static GameObject FindCanvasByName(Scene scene, string canvasName)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name == canvasName)
                {
                    return root;
                }
                var nested = FindRecursive(root.transform, canvasName);
                if (nested != null)
                {
                    return nested.gameObject;
                }
            }
            return null;
        }

        private static float ReadDefaultZoomLevel()
        {
            if (!File.Exists(GameConfigPath))
            {
                return -1f;
            }
            string json = File.ReadAllText(GameConfigPath);
            var m = System.Text.RegularExpressions.Regex.Match(json, "\"defaultZoomLevel\"\\s*:\\s*([0-9.]+)");
            if (!m.Success)
            {
                return -1f;
            }
            if (float.TryParse(m.Groups[1].Value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float v))
            {
                return v;
            }
            return -1f;
        }

        // =========================================================================
        // Internals
        // =========================================================================

        private static Scene OpenScene()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != ScenePath)
            {
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }
            return scene;
        }

        private static GameObject FindGameUICanvas(Scene scene)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name == "GameUICanvas")
                {
                    return root;
                }
                var nested = FindRecursive(root.transform, "GameUICanvas");
                if (nested != null)
                {
                    return nested.gameObject;
                }
            }
            return null;
        }

        private static Image FindTopBarBackground(GameObject canvas)
        {
            var topBar = FindRecursive(canvas.transform, "TopBar");
            if (topBar == null)
            {
                return null;
            }
            var bgT = topBar.Find("Background");
            if (bgT == null)
            {
                return null;
            }
            return bgT.GetComponent<Image>();
        }

        private static Transform FindRecursive(Transform parent, string targetName)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                var c = parent.GetChild(i);
                if (c.name == targetName)
                {
                    return c;
                }
                var found = FindRecursive(c, targetName);
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }

        private static Font LoadFont()
        {
            var font = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
            if (font == null)
            {
                Debug.LogError($"[HanokUI] Font not found at {FontPath}");
            }
            return font;
        }

        private static Sprite LoadHanji()
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(HanjiPath);
            if (sprite == null)
            {
                Debug.LogError($"[HanokUI] Hanji sprite not found at {HanjiPath}. Run 'WhiskerTales/Puzzle/Setup Hanok UI Skin/Generate Hanji Panel Sprite' first.");
            }
            return sprite;
        }

        private static void SaveAndRefresh(Scene scene)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        private static void CreateCardLabel(Transform parent, string text, Font font, int row, float rowH)
        {
            var labelGO = new GameObject($"Label_{text}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            labelGO.layer = parent.gameObject.layer;
            labelGO.transform.SetParent(parent, false);

            var lrt = (RectTransform)labelGO.transform;
            lrt.anchorMin        = new Vector2(0f, 1f);
            lrt.anchorMax        = new Vector2(1f, 1f);
            lrt.pivot            = new Vector2(0.5f, 0.5f);
            lrt.sizeDelta        = new Vector2(0f, rowH);
            lrt.anchoredPosition = new Vector2(0f, -(rowH * row + rowH * 0.5f));

            var t = labelGO.GetComponent<Text>();
            t.text = text;
            t.font = font;
            t.color = InkColor;
            t.alignment = TextAnchor.MiddleCenter;
            t.fontSize = 56;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
        }

        private static bool ColorApproxEquals(Color a, Color b)
        {
            return Mathf.Abs(a.r - b.r) < 0.01f
                && Mathf.Abs(a.g - b.g) < 0.01f
                && Mathf.Abs(a.b - b.b) < 0.01f;
        }

        private static string GetPath(Transform t)
        {
            var stack = new Stack<string>();
            while (t != null)
            {
                stack.Push(t.name);
                t = t.parent;
            }
            return string.Join("/", stack);
        }

        private static void EnsureFolder(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath))
            {
                return;
            }
            string parent = Path.GetDirectoryName(assetPath).Replace('\\', '/');
            string leaf = Path.GetFileName(assetPath);
            if (!AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
#endif
