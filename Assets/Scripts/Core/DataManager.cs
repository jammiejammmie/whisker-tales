using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using WhiskerTales.Utilities;

namespace WhiskerTales.Core
{
    /// <summary>
    /// 게임 데이터 저장 및 로드 관리
    /// JSON 직렬화 사용
    /// </summary>
    public class DataManager : MonoBehaviour
    {
        public static DataManager Instance { get; private set; }

        private string savePath;
        private const string SAVE_FILENAME = "game_save.json";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            // 저장 경로 설정
            savePath = Path.Combine(Application.persistentDataPath, SAVE_FILENAME);
            Debug.Log($"[DataManager] Save path: {savePath}");
        }

        /// <summary>
        /// 사용자 진행도 저장
        /// </summary>
        public void SaveUserProgress(UserProgress progress)
        {
            try
            {
                string json = JsonUtility.ToJson(progress, true);
                File.WriteAllText(savePath, json);
                Debug.Log("[DataManager] User progress saved successfully");
            }
            catch (Exception e)
            {
                Debug.LogError($"[DataManager] Failed to save user progress: {e.Message}");
            }
        }

        /// <summary>
        /// 사용자 진행도 로드
        /// </summary>
        public UserProgress LoadUserProgress()
        {
            try
            {
                if (File.Exists(savePath))
                {
                    string json = File.ReadAllText(savePath);
                    UserProgress progress = JsonUtility.FromJson<UserProgress>(json);
                    Debug.Log("[DataManager] User progress loaded successfully");
                    return progress;
                }
                else
                {
                    Debug.Log("[DataManager] Save file not found. Creating new game...");
                    return null;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[DataManager] Failed to load user progress: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 레벨 데이터 저장
        /// </summary>
        public void SaveLevelData(int levelId, LevelProgress progress)
        {
            try
            {
                string filename = $"level_{levelId}.json";
                string path = Path.Combine(Application.persistentDataPath, filename);
                string json = JsonUtility.ToJson(progress, true);
                File.WriteAllText(path, json);
                Debug.Log($"[DataManager] Level {levelId} data saved");
            }
            catch (Exception e)
            {
                Debug.LogError($"[DataManager] Failed to save level data: {e.Message}");
            }
        }

        /// <summary>
        /// 레벨 데이터 로드
        /// </summary>
        public LevelProgress LoadLevelData(int levelId)
        {
            try
            {
                string filename = $"level_{levelId}.json";
                string path = Path.Combine(Application.persistentDataPath, filename);

                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    LevelProgress progress = JsonUtility.FromJson<LevelProgress>(json);
                    Debug.Log($"[DataManager] Level {levelId} data loaded");
                    return progress;
                }
                else
                {
                    Debug.Log($"[DataManager] Level {levelId} data not found. Creating new...");
                    return new LevelProgress { levelId = levelId };
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[DataManager] Failed to load level data: {e.Message}");
                return new LevelProgress { levelId = levelId };
            }
        }

        /// <summary>
        /// 모든 레벨 데이터 로드
        /// </summary>
        public List<LevelProgress> LoadAllLevelData()
        {
            List<LevelProgress> allLevels = new List<LevelProgress>();

            for (int i = 1; i <= Constants.TOTAL_LEVELS; i++)
            {
                LevelProgress progress = LoadLevelData(i);
                allLevels.Add(progress);
            }

            return allLevels;
        }

        /// <summary>
        /// 고양이 데이터 저장
        /// </summary>
        public void SaveCatData(List<Cat> cats)
        {
            try
            {
                // List를 저장하기 위해 래퍼 클래스 사용
                CatListWrapper wrapper = new CatListWrapper { cats = cats };
                string json = JsonUtility.ToJson(wrapper, true);
                string path = Path.Combine(Application.persistentDataPath, "cats.json");
                File.WriteAllText(path, json);
                Debug.Log("[DataManager] Cat data saved");
            }
            catch (Exception e)
            {
                Debug.LogError($"[DataManager] Failed to save cat data: {e.Message}");
            }
        }

        /// <summary>
        /// 고양이 데이터 로드
        /// </summary>
        public List<Cat> LoadCatData()
        {
            try
            {
                string path = Path.Combine(Application.persistentDataPath, "cats.json");

                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    CatListWrapper wrapper = JsonUtility.FromJson<CatListWrapper>(json);
                    Debug.Log("[DataManager] Cat data loaded");
                    return wrapper.cats;
                }
                else
                {
                    Debug.Log("[DataManager] Cat data not found");
                    return new List<Cat>();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[DataManager] Failed to load cat data: {e.Message}");
                return new List<Cat>();
            }
        }

        /// <summary>
        /// 복원 작업 데이터 저장
        /// </summary>
        public void SaveRestorationTasks(List<RestorationTask> tasks)
        {
            try
            {
                RestorationTaskListWrapper wrapper = new RestorationTaskListWrapper { tasks = tasks };
                string json = JsonUtility.ToJson(wrapper, true);
                string path = Path.Combine(Application.persistentDataPath, "restoration_tasks.json");
                File.WriteAllText(path, json);
                Debug.Log("[DataManager] Restoration tasks saved");
            }
            catch (Exception e)
            {
                Debug.LogError($"[DataManager] Failed to save restoration tasks: {e.Message}");
            }
        }

        /// <summary>
        /// 복원 작업 데이터 로드
        /// </summary>
        public List<RestorationTask> LoadRestorationTasks()
        {
            try
            {
                string path = Path.Combine(Application.persistentDataPath, "restoration_tasks.json");

                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    RestorationTaskListWrapper wrapper = JsonUtility.FromJson<RestorationTaskListWrapper>(json);
                    Debug.Log("[DataManager] Restoration tasks loaded");
                    return wrapper.tasks;
                }
                else
                {
                    Debug.Log("[DataManager] Restoration tasks not found");
                    return new List<RestorationTask>();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[DataManager] Failed to load restoration tasks: {e.Message}");
                return new List<RestorationTask>();
            }
        }

        /// <summary>
        /// 모든 데이터 삭제 (디버그용)
        /// </summary>
        public void DeleteAllData()
        {
            try
            {
                string[] files = Directory.GetFiles(Application.persistentDataPath, "*.json");
                foreach (string file in files)
                {
                    File.Delete(file);
                }
                Debug.Log("[DataManager] All data deleted");
            }
            catch (Exception e)
            {
                Debug.LogError($"[DataManager] Failed to delete data: {e.Message}");
            }
        }

        /// <summary>
        /// 저장 파일 존재 여부 확인
        /// </summary>
        public bool SaveFileExists()
        {
            return File.Exists(savePath);
        }

        // ===== 래퍼 클래스 (JSON 직렬화용) =====

        [System.Serializable]
        private class CatListWrapper
        {
            public List<Cat> cats = new List<Cat>();
        }

        [System.Serializable]
        private class RestorationTaskListWrapper
        {
            public List<RestorationTask> tasks = new List<RestorationTask>();
        }
    }
}
