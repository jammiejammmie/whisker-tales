using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using WhiskerTales.UI.Home;

namespace WhiskerTales.EditorTools
{
    /// <summary>
    /// Main_App 씬의 기존 HomeNabi GameObject(자식 포함)를 안전하게 제거.
    /// 새 HomeNabiPositionSystem으로 교체할 때 1회 실행 후 이 파일을 지워도 됨.
    /// 메뉴: Whisker Tales/Setup/Remove Legacy HomeNabi
    /// </summary>
    public static class RemoveLegacyHomeNabi
    {
        private const string MainScenePath = "Assets/WhiskerTales/_Project/Scenes/Main_App.unity";

        [MenuItem("Whisker Tales/Setup/Remove Legacy HomeNabi")]
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

                int removed = 0;
                GameObject[] roots = scene.GetRootGameObjects();

                for (int i = 0; i < roots.Length; i++)
                {
                    HomeNabi[] nabis = roots[i].GetComponentsInChildren<HomeNabi>(true);
                    for (int j = 0; j < nabis.Length; j++)
                    {
                        if (nabis[j] == null) { continue; }
                        Debug.Log("[Remove Legacy HomeNabi] destroying: " + GetPath(nabis[j].gameObject));
                        Object.DestroyImmediate(nabis[j].gameObject);
                        removed++;
                    }
                }

                if (removed == 0)
                {
                    EditorUtility.DisplayDialog("Remove Legacy HomeNabi",
                        "HomeNabi GameObject를 찾지 못했습니다. 이미 제거된 상태입니다.",
                        "확인");
                    return;
                }

                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);

                EditorUtility.DisplayDialog("Remove Legacy HomeNabi",
                    "완료 — " + removed + "개 HomeNabi GameObject 제거됨.\n\n" +
                    "이 메뉴 스크립트는 더 이상 필요 없습니다.\n" +
                    "Assets/WhiskerTales/_Project/Scripts/Editor/RemoveLegacyHomeNabi.cs 파일을 삭제해도 됩니다.",
                    "확인");
                Debug.Log("[Remove Legacy HomeNabi] Done. removed=" + removed);
            }
            catch (System.Exception e)
            {
                Debug.LogError("[Remove Legacy HomeNabi] " + e);
                EditorUtility.DisplayDialog("Remove Legacy HomeNabi — 실패", e.Message, "확인");
            }
        }

        private static void Fail(string msg)
        {
            Debug.LogError("[Remove Legacy HomeNabi] " + msg);
            EditorUtility.DisplayDialog("Remove Legacy HomeNabi — 실패", msg, "확인");
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

        private static string GetPath(GameObject go)
        {
            string path = go.name;
            Transform t = go.transform.parent;
            while (t != null)
            {
                path = t.name + "/" + path;
                t = t.parent;
            }
            return path;
        }
    }
}
