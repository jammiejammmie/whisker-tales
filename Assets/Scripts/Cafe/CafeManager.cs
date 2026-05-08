using UnityEngine;
using System.Collections.Generic;
using WhiskerTales.Core;
using WhiskerTales.Utilities;

namespace WhiskerTales.Cafe
{
    /// <summary>
    /// 카페 복원 시스템 관리
    /// 복원 작업, 가구 배치, 단골 손님 등 관리
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class CafeManager : MonoBehaviour
    {
        public static CafeManager Instance { get; private set; }

        [SerializeField] private int totalAreas = Constants.CAFE_AREAS;
        [SerializeField] private int tasksPerArea = Constants.TASKS_PER_AREA;

        private List<RestorationTask> restorationTasks = new List<RestorationTask>();
        private Dictionary<int, bool> taskCompletionStatus = new Dictionary<int, bool>();
        private DataManager dataManager;
        private GameManager gameManager;

        // 이벤트
        public delegate void TaskCompletedHandler(int taskId);
        public event TaskCompletedHandler OnTaskCompleted;

        public delegate void AreaCompletedHandler(int areaId);
        public event AreaCompletedHandler OnAreaCompleted;

        public delegate void CafeUpdatedHandler();
        public event CafeUpdatedHandler OnCafeUpdated;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            dataManager = DataManager.Instance;
            gameManager = GameManager.Instance;

            InitializeRestorationTasks();
        }

        /// <summary>
        /// 복원 작업 초기화
        /// </summary>
        private void InitializeRestorationTasks()
        {
            // 3개 구역 × 7개 작업 = 21개 작업
            int taskId = 1;

            // 구역 1: 입구 마당
            CreateTasksForArea(1, "입구 마당", new string[]
            {
                "마당 빗자루질하기",
                "낡은 간판 고치기",
                "잡초 제거하기",
                "돌 깔기",
                "등불 설치하기",
                "화분 배치하기",
                "입구 문 칠하기"
            }, ref taskId);

            // 구역 2: 카페 본채
            CreateTasksForArea(2, "카페 본채", new string[]
            {
                "기와 새로 올리기",
                "창호지 교체하기",
                "바닥 닦기",
                "벽 칠하기",
                "조명 설치하기",
                "테이블 배치하기",
                "의자 정렬하기"
            }, ref taskId);

            // 구역 3: 뒷마당 정원
            CreateTasksForArea(3, "뒷마당 정원", new string[]
            {
                "정원 정리하기",
                "연못 청소하기",
                "나무 가지치기",
                "돌다리 놓기",
                "벤치 설치하기",
                "등불 달기",
                "꽃 심기"
            }, ref taskId);

            Debug.Log($"[CafeManager] {restorationTasks.Count} restoration tasks initialized");
        }

        /// <summary>
        /// 구역별 작업 생성 헬퍼
        /// </summary>
        private void CreateTasksForArea(int areaId, string areaName, string[] taskNames, ref int taskId)
        {
            for (int i = 0; i < taskNames.Length; i++)
            {
                RestorationTask task = new RestorationTask
                {
                    taskId = taskId,
                    taskName = taskNames[i],
                    description = $"{areaName} - {taskNames[i]}",
                    requiredStars = (i + 1) * 2,  // 2, 4, 6, 8, 10, 12, 14
                    coinReward = 100 + (i * 50),  // 100, 150, 200, ...
                    gemReward = (i + 1),          // 1, 2, 3, ...
                    isCompleted = false,
                    beforeImagePath = $"Sprites/Cafe/area_{areaId}_task_{i}_before",
                    afterImagePath = $"Sprites/Cafe/area_{areaId}_task_{i}_after"
                };

                restorationTasks.Add(task);
                taskCompletionStatus[taskId] = false;
                taskId++;
            }
        }

        /// <summary>
        /// 복원 작업 완료
        /// </summary>
        public bool CompleteTask(int taskId)
        {
            RestorationTask task = GetTask(taskId);
            if (task == null)
            {
                Debug.LogError($"[CafeManager] Task {taskId} not found");
                return false;
            }

            if (task.isCompleted)
            {
                Debug.LogWarning($"[CafeManager] Task {taskId} already completed");
                return false;
            }

            // 별 확인
            if (!gameManager.SpendStars(task.requiredStars))
            {
                Debug.LogWarning($"[CafeManager] Not enough stars for task {taskId}");
                return false;
            }

            // 작업 완료
            task.isCompleted = true;
            taskCompletionStatus[taskId] = true;

            // 보상 지급
            gameManager.AddCoins(task.coinReward);
            gameManager.AddGems(task.gemReward);

            OnTaskCompleted?.Invoke(taskId);
            OnCafeUpdated?.Invoke();

            Debug.Log($"[CafeManager] Task {taskId} completed! Rewards: {task.coinReward} coins, {task.gemReward} gems");

            // 구역 완료 확인
            CheckAreaCompletion(GetAreaIdFromTaskId(taskId));

            return true;
        }

        /// <summary>
        /// 작업 ID에서 구역 ID 추출
        /// </summary>
        private int GetAreaIdFromTaskId(int taskId)
        {
            // 1-7: 구역 1, 8-14: 구역 2, 15-21: 구역 3
            return (taskId - 1) / Constants.TASKS_PER_AREA + 1;
        }

        /// <summary>
        /// 구역 완료 확인
        /// </summary>
        private void CheckAreaCompletion(int areaId)
        {
            int startTaskId = (areaId - 1) * Constants.TASKS_PER_AREA + 1;
            int endTaskId = startTaskId + Constants.TASKS_PER_AREA - 1;

            bool allCompleted = true;
            for (int i = startTaskId; i <= endTaskId; i++)
            {
                if (!taskCompletionStatus.ContainsKey(i) || !taskCompletionStatus[i])
                {
                    allCompleted = false;
                    break;
                }
            }

            if (allCompleted)
            {
                OnAreaCompleted?.Invoke(areaId);
                Debug.Log($"[CafeManager] Area {areaId} completed!");

                // 새로운 고양이 언락 (구역 완료 시)
                UnlockCatForArea(areaId);
            }
        }

        /// <summary>
        /// 구역 완료 시 고양이 언락
        /// </summary>
        private void UnlockCatForArea(int areaId)
        {
            // 구역별 고양이 언락 (예시)
            int catIdToUnlock = areaId + 1; // 구역 1 완료 → 벨라(2) 언락, 구역 2 완료 → 사미(3) 언락 등

            if (catIdToUnlock <= Constants.TOTAL_CATS)
            {
                Cat.CatManager catManager = Cat.CatManager.Instance;
                if (catManager != null && !catManager.IsCatUnlocked(catIdToUnlock))
                {
                    catManager.UnlockCat(catIdToUnlock);
                    Debug.Log($"[CafeManager] Cat {catIdToUnlock} unlocked for completing area {areaId}!");
                }
            }
        }

        /// <summary>
        /// 복원 작업 반환
        /// </summary>
        public RestorationTask GetTask(int taskId)
        {
            foreach (RestorationTask task in restorationTasks)
            {
                if (task.taskId == taskId)
                    return task;
            }
            return null;
        }

        /// <summary>
        /// 구역별 작업 반환
        /// </summary>
        public List<RestorationTask> GetTasksForArea(int areaId)
        {
            List<RestorationTask> areaTasks = new List<RestorationTask>();
            int startTaskId = (areaId - 1) * Constants.TASKS_PER_AREA + 1;
            int endTaskId = startTaskId + Constants.TASKS_PER_AREA - 1;

            foreach (RestorationTask task in restorationTasks)
            {
                if (task.taskId >= startTaskId && task.taskId <= endTaskId)
                {
                    areaTasks.Add(task);
                }
            }

            return areaTasks;
        }

        /// <summary>
        /// 모든 복원 작업 반환
        /// </summary>
        public List<RestorationTask> GetAllTasks()
        {
            return new List<RestorationTask>(restorationTasks);
        }

        /// <summary>
        /// 완료된 작업 수 반환
        /// </summary>
        public int GetCompletedTaskCount()
        {
            int count = 0;
            foreach (RestorationTask task in restorationTasks)
            {
                if (task.isCompleted)
                    count++;
            }
            return count;
        }

        /// <summary>
        /// 구역별 완료 진행도 반환 (0~100)
        /// </summary>
        public float GetAreaProgressPercentage(int areaId)
        {
            List<RestorationTask> areaTasks = GetTasksForArea(areaId);
            if (areaTasks.Count == 0)
                return 0f;

            int completedCount = 0;
            foreach (RestorationTask task in areaTasks)
            {
                if (task.isCompleted)
                    completedCount++;
            }

            return (completedCount / (float)areaTasks.Count) * 100f;
        }

        /// <summary>
        /// 전체 카페 완료 진행도 반환 (0~100)
        /// </summary>
        public float GetCafeProgressPercentage()
        {
            if (restorationTasks.Count == 0)
                return 0f;

            int completedCount = GetCompletedTaskCount();
            return (completedCount / (float)restorationTasks.Count) * 100f;
        }

        /// <summary>
        /// 구역 완료 여부 확인
        /// </summary>
        public bool IsAreaCompleted(int areaId)
        {
            List<RestorationTask> areaTasks = GetTasksForArea(areaId);
            foreach (RestorationTask task in areaTasks)
            {
                if (!task.isCompleted)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 전체 카페 완료 여부 확인
        /// </summary>
        public bool IsCafeCompleted()
        {
            foreach (RestorationTask task in restorationTasks)
            {
                if (!task.isCompleted)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 다음 미완료 작업 반환
        /// </summary>
        public RestorationTask GetNextIncompleteTask()
        {
            foreach (RestorationTask task in restorationTasks)
            {
                if (!task.isCompleted)
                    return task;
            }
            return null;
        }

        /// <summary>
        /// 카페 리셋 (디버그용)
        /// </summary>
        public void ResetCafe()
        {
            foreach (RestorationTask task in restorationTasks)
            {
                task.isCompleted = false;
                taskCompletionStatus[task.taskId] = false;
            }
            Debug.Log("[CafeManager] Cafe reset");
        }
    }
}
