using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using WhiskerTales.Core;
using WhiskerTales.Cafe;
using WhiskerTales.Utilities;

namespace WhiskerTales.UI
{
    /// <summary>
    /// 카페 화면 UI 관리
    /// 복원 작업, 진행도, 고양이 표시 등
    /// </summary>
    public class CafeUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI cafeProgressText;
        [SerializeField] private Slider cafeProgressSlider;
        [SerializeField] private Transform taskListContainer;
        [SerializeField] private GameObject taskItemPrefab;
        [SerializeField] private Button backButton;
        [SerializeField] private Transform catDisplayContainer;
        [SerializeField] private GameObject catDisplayPrefab;

        private CafeManager cafeManager;
        private Cat.CatManager catManager;
        private GameManager gameManager;
        private List<GameObject> taskUIItems = new List<GameObject>();

        private void Start()
        {
            cafeManager = CafeManager.Instance;
            catManager = Cat.CatManager.Instance;
            gameManager = GameManager.Instance;

            InitializeUI();
            SubscribeToEvents();
        }

        /// <summary>
        /// UI 초기화
        /// </summary>
        private void InitializeUI()
        {
            if (backButton != null)
            {
                backButton.onClick.AddListener(OnBackClicked);
            }

            RefreshTaskList();
            RefreshCatDisplay();
            UpdateProgressDisplay();
        }

        /// <summary>
        /// 이벤트 구독
        /// </summary>
        private void SubscribeToEvents()
        {
            if (cafeManager != null)
            {
                cafeManager.OnTaskCompleted += OnTaskCompleted;
                cafeManager.OnCafeUpdated += OnCafeUpdated;
            }
        }

        /// <summary>
        /// 작업 목록 새로고침
        /// </summary>
        private void RefreshTaskList()
        {
            // 기존 UI 제거
            foreach (GameObject item in taskUIItems)
            {
                Destroy(item);
            }
            taskUIItems.Clear();

            // 새 작업 UI 생성
            List<RestorationTask> tasks = cafeManager.GetAllTasks();
            foreach (RestorationTask task in tasks)
            {
                GameObject taskItem = Instantiate(taskItemPrefab, taskListContainer);
                taskUIItems.Add(taskItem);

                // 작업 정보 설정
                TextMeshProUGUI taskNameText = taskItem.transform.Find("TaskName")?.GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI taskStarsText = taskItem.transform.Find("RequiredStars")?.GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI taskRewardText = taskItem.transform.Find("Reward")?.GetComponent<TextMeshProUGUI>();
                Button completeButton = taskItem.transform.Find("CompleteButton")?.GetComponent<Button>();
                Image completedImage = taskItem.transform.Find("Completed")?.GetComponent<Image>();

                if (taskNameText != null)
                    taskNameText.text = task.taskName;

                if (taskStarsText != null)
                    taskStarsText.text = $"⭐ {task.requiredStars}";

                if (taskRewardText != null)
                    taskRewardText.text = $"💰 {task.coinReward} + 💎 {task.gemReward}";

                if (completeButton != null)
                {
                    int taskId = task.taskId; // 클로저 문제 방지
                    completeButton.onClick.AddListener(() => OnCompleteTaskClicked(taskId));
                    completeButton.interactable = !task.isCompleted;
                }

                if (completedImage != null)
                    completedImage.gameObject.SetActive(task.isCompleted);
            }

            Debug.Log("[CafeUI] Task list refreshed");
        }

        /// <summary>
        /// 고양이 표시 새로고침
        /// </summary>
        private void RefreshCatDisplay()
        {
            // 기존 고양이 UI 제거
            foreach (Transform child in catDisplayContainer)
            {
                Destroy(child.gameObject);
            }

            // 언락된 고양이 표시
            List<Cat> unlockedCats = catManager.GetUnlockedCats();
            foreach (Cat cat in unlockedCats)
            {
                GameObject catDisplay = Instantiate(catDisplayPrefab, catDisplayContainer);

                // 고양이 정보 설정
                TextMeshProUGUI catNameText = catDisplay.transform.Find("CatName")?.GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI catAffinityText = catDisplay.transform.Find("Affinity")?.GetComponent<TextMeshProUGUI>();
                Image catPortrait = catDisplay.transform.Find("Portrait")?.GetComponent<Image>();

                if (catNameText != null)
                    catNameText.text = cat.name;

                if (catAffinityText != null)
                    catAffinityText.text = $"호감도: {cat.affinityLevel}/5";

                if (catPortrait != null)
                {
                    // 스프라이트 로드 (Resources 폴더에서)
                    Sprite portrait = Resources.Load<Sprite>(cat.portraitPath);
                    if (portrait != null)
                        catPortrait.sprite = portrait;
                }
            }

            Debug.Log("[CafeUI] Cat display refreshed");
        }

        /// <summary>
        /// 진행도 표시 업데이트
        /// </summary>
        private void UpdateProgressDisplay()
        {
            float progress = cafeManager.GetCafeProgressPercentage();
            int completedTasks = cafeManager.GetCompletedTaskCount();
            int totalTasks = Constants.TOTAL_RESTORATION_TASKS;

            if (cafeProgressText != null)
            {
                cafeProgressText.text = $"카페 복원 진행도: {completedTasks}/{totalTasks} ({progress:F1}%)";
            }

            if (cafeProgressSlider != null)
            {
                cafeProgressSlider.value = progress / 100f;
            }

            Debug.Log($"[CafeUI] Progress: {completedTasks}/{totalTasks} ({progress:F1}%)");
        }

        /// <summary>
        /// 작업 완료 클릭
        /// </summary>
        private void OnCompleteTaskClicked(int taskId)
        {
            if (cafeManager.CompleteTask(taskId))
            {
                Debug.Log($"[CafeUI] Task {taskId} completed");
            }
            else
            {
                Debug.LogWarning($"[CafeUI] Failed to complete task {taskId}");
            }
        }

        /// <summary>
        /// 작업 완료 이벤트 처리
        /// </summary>
        private void OnTaskCompleted(int taskId)
        {
            RefreshTaskList();
            UpdateProgressDisplay();
        }

        /// <summary>
        /// 카페 업데이트 이벤트 처리
        /// </summary>
        private void OnCafeUpdated()
        {
            RefreshTaskList();
            RefreshCatDisplay();
            UpdateProgressDisplay();
        }

        /// <summary>
        /// 뒤로가기 버튼 클릭
        /// </summary>
        private void OnBackClicked()
        {
            GameManager gameManager = GameManager.Instance;
            gameManager.ReturnToMenu();
            Debug.Log("[CafeUI] Back button clicked");
        }

        private void OnDestroy()
        {
            if (cafeManager != null)
            {
                cafeManager.OnTaskCompleted -= OnTaskCompleted;
                cafeManager.OnCafeUpdated -= OnCafeUpdated;
            }
        }
    }
}
