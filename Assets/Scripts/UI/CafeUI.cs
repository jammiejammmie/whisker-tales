using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using WhiskerTales.Core;
using WhiskerTales.Cafe;
using WhiskerTales.Utilities;

namespace WhiskerTales.UI
{
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

        private void InitializeUI()
        {
            if (backButton != null) backButton.onClick.AddListener(OnBackClicked);
            RefreshTaskList();
            RefreshCatDisplay();
            UpdateProgressDisplay();
        }

        private void SubscribeToEvents()
        {
            if (cafeManager != null)
            {
                cafeManager.OnTaskCompleted += OnTaskCompleted;
                cafeManager.OnCafeUpdated += OnCafeUpdated;
            }
        }

        private void RefreshTaskList()
        {
            foreach (GameObject item in taskUIItems) Destroy(item);
            taskUIItems.Clear();

            List<RestorationTask> tasks = cafeManager.GetAllTasks();
            foreach (RestorationTask task in tasks)
            {
                GameObject taskItem = Instantiate(taskItemPrefab, taskListContainer);
                taskUIItems.Add(taskItem);

                TextMeshProUGUI taskNameText = taskItem.transform.Find("TaskName")?.GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI taskStarsText = taskItem.transform.Find("RequiredStars")?.GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI taskRewardText = taskItem.transform.Find("Reward")?.GetComponent<TextMeshProUGUI>();
                Button completeButton = taskItem.transform.Find("CompleteButton")?.GetComponent<Button>();
                Image completedImage = taskItem.transform.Find("Completed")?.GetComponent<Image>();

                if (taskNameText != null) taskNameText.text = task.taskName;
                if (taskStarsText != null) taskStarsText.text = "Stars: " + task.requiredStars;
                if (taskRewardText != null) taskRewardText.text = "Coins: " + task.coinReward;

                if (completeButton != null)
                {
                    int taskId = task.taskId;
                    completeButton.onClick.AddListener(() => OnCompleteTaskClicked(taskId));
                    completeButton.interactable = !task.isCompleted;
                }
                if (completedImage != null) completedImage.gameObject.SetActive(task.isCompleted);
            }
        }

        private void RefreshCatDisplay()
        {
            foreach (Transform child in catDisplayContainer) Destroy(child.gameObject);

            List<WhiskerTales.Core.Cat> unlockedCats = catManager.GetUnlockedCats();
            foreach (WhiskerTales.Core.Cat cat in unlockedCats)
            {
                GameObject catDisplay = Instantiate(catDisplayPrefab, catDisplayContainer);

                TextMeshProUGUI catNameText = catDisplay.transform.Find("CatName")?.GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI catAffinityText = catDisplay.transform.Find("Affinity")?.GetComponent<TextMeshProUGUI>();
                Image catPortrait = catDisplay.transform.Find("Portrait")?.GetComponent<Image>();

                if (catNameText != null) catNameText.text = cat.name;
                if (catAffinityText != null) catAffinityText.text = "Affinity: " + cat.affinityLevel + "/5";
                if (catPortrait != null)
                {
                    Sprite portrait = Resources.Load<Sprite>(cat.portraitPath);
                    if (portrait != null) catPortrait.sprite = portrait;
                }
            }
        }

        private void UpdateProgressDisplay()
        {
            float progress = cafeManager.GetCafeProgressPercentage();
            int completedTasks = cafeManager.GetCompletedTaskCount();
            int totalTasks = Constants.TOTAL_RESTORATION_TASKS;

            if (cafeProgressText != null)
                cafeProgressText.text = "Progress: " + completedTasks + "/" + totalTasks;

            if (cafeProgressSlider != null)
                cafeProgressSlider.value = progress / 100f;
        }

        private void OnCompleteTaskClicked(int taskId)
        {
            if (cafeManager.CompleteTask(taskId))
                Debug.Log("[CafeUI] Task " + taskId + " completed");
            else
                Debug.LogWarning("[CafeUI] Failed to complete task " + taskId);
        }

        private void OnTaskCompleted(int taskId)
        {
            RefreshTaskList();
            UpdateProgressDisplay();
        }

        private void OnCafeUpdated()
        {
            RefreshTaskList();
            RefreshCatDisplay();
            UpdateProgressDisplay();
        }

        private void OnBackClicked()
        {
            GameManager.Instance.ReturnToMenu();
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
