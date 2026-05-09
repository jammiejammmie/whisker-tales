using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using WhiskerTales.UI;
using WhiskerTales.Utilities;

namespace WhiskerTales.EditorTests
{
    /// <summary>
    /// Editor PASS/FAIL — Tools/Whisker Tales/Test/Photo Studio.
    /// 배경 전환 / 포즈 토글 시 displayed sprite가 실제로 바뀌는지 검증.
    /// </summary>
    public static class PhotoStudioTests
    {
        [MenuItem("Tools/Whisker Tales/Test/Photo Studio")]
        public static void TestAll()
        {
            int passed = 0, failed = 0;

            GameObject host = new GameObject("PhotoStudioTest_Host");
            GameObject bgGo = new GameObject("PhotoStudioTest_Bg");
            GameObject catGo = new GameObject("PhotoStudioTest_Cat");
            try
            {
                // 컨트롤러 + 임시 Image들
                PhotoStudioController ctrl = host.AddComponent<PhotoStudioController>();

                // RectTransform이 필요함
                bgGo.AddComponent<RectTransform>();
                catGo.AddComponent<RectTransform>();
                Image bg = bgGo.AddComponent<Image>();
                Image cat = catGo.AddComponent<Image>();
                ctrl.DebugAttachImages(bg, cat);

                // 더미 sprite 3종 (서로 구분 가능하도록 다른 색)
                Sprite spA = MakeColoredSprite(Color.red);
                Sprite spB = MakeColoredSprite(Color.green);
                Sprite spC = MakeColoredSprite(Color.blue);

                // 배경 옵션 5개 슬롯 + 처음 3개에 sprite 주입
                ctrl.DebugAllocBackgroundOptions(5);
                ctrl.DebugInjectBackgroundOption(0, spA);
                ctrl.DebugInjectBackgroundOption(1, spB);
                ctrl.DebugInjectBackgroundOption(2, spC);

                // 고양이 sprite (front/play 구분)
                Sprite frontSp = MakeColoredSprite(new Color(0.9f, 0.5f, 0.3f));
                Sprite playSp  = MakeColoredSprite(new Color(0.3f, 0.5f, 0.9f));
                ctrl.DebugInjectFrontSprite(Constants.CAT_NABI, frontSp);
                ctrl.DebugInjectPlaySprite(Constants.CAT_NABI, playSp);
                ctrl.SetCat(Constants.CAT_NABI);

                // ===== Test 1: 배경 전환 =====
                ctrl.SetBackground(0);
                Sprite shown0 = ctrl.DebugGetCurrentBackgroundSprite();
                ctrl.SetBackground(2);
                Sprite shown2 = ctrl.DebugGetCurrentBackgroundSprite();

                if (shown0 == spA && shown2 == spC && shown0 != shown2)
                {
                    Debug.Log("  [PASS] Background switch — slot 0 (spA) → slot 2 (spC) reflected on Image");
                    passed++;
                }
                else
                {
                    Debug.LogError($"  [FAIL] Background switch — shown0={(shown0 == spA ? "spA" : "?")}, shown2={(shown2 == spC ? "spC" : "?")}, equal={shown0 == shown2}");
                    failed++;
                }

                // ===== Test 2: 포즈 토글 =====
                ctrl.SetPose(PhotoStudioController.PoseKind.Front);
                Sprite catFront = ctrl.DebugGetCurrentCatSprite();
                ctrl.SetPose(PhotoStudioController.PoseKind.Play);
                Sprite catPlay = ctrl.DebugGetCurrentCatSprite();

                if (catFront == frontSp && catPlay == playSp && catFront != catPlay)
                {
                    Debug.Log("  [PASS] Pose toggle — Front → Play reflected on cat Image");
                    passed++;
                }
                else
                {
                    Debug.LogError($"  [FAIL] Pose toggle — catFront={(catFront == frontSp ? "front" : "?")}, catPlay={(catPlay == playSp ? "play" : "?")}, equal={catFront == catPlay}");
                    failed++;
                }

                // ===== Test 3: TogglePose 왕복 =====
                ctrl.SetPose(PhotoStudioController.PoseKind.Front);
                ctrl.TogglePose();
                bool wasPlayAfterToggle = ctrl.CurrentPose == PhotoStudioController.PoseKind.Play;
                ctrl.TogglePose();
                bool wasFrontAfterTwo = ctrl.CurrentPose == PhotoStudioController.PoseKind.Front;
                if (wasPlayAfterToggle && wasFrontAfterTwo)
                {
                    Debug.Log("  [PASS] TogglePose roundtrip — Front → Play → Front");
                    passed++;
                }
                else
                {
                    Debug.LogError($"  [FAIL] TogglePose roundtrip — afterFirst={ctrl.CurrentPose}, afterSecond={ctrl.CurrentPose}");
                    failed++;
                }

                int total = passed + failed;
                string verdict = (failed == 0) ? "PASS" : "FAIL";
                Debug.Log($"[TEST] Photo Studio: {verdict} ({passed}/{total})");
            }
            finally
            {
                Object.DestroyImmediate(host);
                Object.DestroyImmediate(bgGo);
                Object.DestroyImmediate(catGo);
            }
        }

        private static Sprite MakeColoredSprite(Color c)
        {
            Texture2D t = new Texture2D(8, 8, TextureFormat.RGBA32, false);
            Color[] px = new Color[64];
            for (int i = 0; i < px.Length; i++) px[i] = c;
            t.SetPixels(px);
            t.Apply();
            return Sprite.Create(t, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f));
        }
    }
}
