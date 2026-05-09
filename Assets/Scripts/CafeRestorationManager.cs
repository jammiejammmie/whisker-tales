using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 카페/한옥 복원 시스템 관리자
/// 매치-3 퍼즐 클리어 → 별 획득 → 카페 복원 진행도 증가 → 시각적 변화
/// </summary>
public class CafeRestorationManager : MonoBehaviour
{
    [System.Serializable]
    public class RestorationStage
    {
        public int stage;
        public string description;
        public int starsRequired;
        public string visualChangeKey;
    }

    [System.Serializable]
    public class CafeArea
    {
        public int areaId;
        public string areaName;
        public List<RestorationStage> stages;
    }

    [System.Serializable]
    public class CafeRestorationData
    {
        public List<CafeArea> cafeAreas;
    }

    // 싱글톤
    public static CafeRestorationManager instance;

    // 복원 데이터
    private CafeRestorationData cafeData;
    private int currentAreaId = 1;
    private int currentStage = 1;
    private int currentStars = 0;

    // UI 참조
    public Image progressBar;
    public Text stageDescriptionText;
    public Text starsRequiredText;
    public Image cafeBackgroundImage;

    // 배경 이미지 (Inspector 직접 할당, zone × stage 5장씩)
    [System.Serializable]
    public class ZoneBackgrounds
    {
        public Sprite[] stages = new Sprite[5];
    }

    [SerializeField] private ZoneBackgrounds[] zoneBackgrounds = new ZoneBackgrounds[3]
    {
        new ZoneBackgrounds(),
        new ZoneBackgrounds(),
        new ZoneBackgrounds(),
    };

    // 키→스프라이트 룩업 (Inspector 배열에서 빌드)
    private Dictionary<string, Sprite> backgroundSprites = new Dictionary<string, Sprite>();

    // 이벤트
    public delegate void OnStageCompleted(int areaId, int stage);
    public event OnStageCompleted StageCompletedEvent;

    public int CurrentAreaId => currentAreaId;
    public int CurrentStage => currentStage;
    public string CurrentBackgroundKey => $"bg_zone{currentAreaId}_stage{currentStage}";

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        LoadCafeData();
        LoadBackgroundSprites();
        UpdateUI();
    }

    /// <summary>
    /// JSON 파일에서 카페 복원 데이터 로드
    /// </summary>
    private void LoadCafeData()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("CafeRestorationData");
        if (jsonFile != null)
        {
            cafeData = JsonUtility.FromJson<CafeRestorationData>(jsonFile.text);
            Debug.Log("카페 복원 데이터 로드 완료");
        }
        else
        {
            Debug.LogError("CafeRestorationData.json을 찾을 수 없습니다!");
        }
    }

    /// <summary>
    /// Inspector에서 할당된 zoneBackgrounds 배열을 키→Sprite 사전으로 인덱싱.
    /// 키 형식: bg_zone{Z}_stage{S} (Z=1..3, S=1..5)
    /// </summary>
    private void LoadBackgroundSprites()
    {
        backgroundSprites.Clear();
        if (zoneBackgrounds == null) return;

        int loaded = 0;
        for (int z = 0; z < zoneBackgrounds.Length; z++)
        {
            var zone = zoneBackgrounds[z];
            if (zone == null || zone.stages == null) continue;
            for (int s = 0; s < zone.stages.Length; s++)
            {
                Sprite sp = zone.stages[s];
                if (sp == null) continue;
                string key = $"bg_zone{z + 1}_stage{s + 1}";
                backgroundSprites[key] = sp;
                loaded++;
            }
        }
        Debug.Log($"배경 이미지 {loaded}개 로드 완료 (Inspector 할당)");
    }

    /// <summary>
    /// 외부(TitleUI 등)에서 zone/stage 인덱스로 배경 Sprite 조회.
    /// </summary>
    public Sprite GetBackground(int zoneId1Based, int stage1Based)
    {
        string key = $"bg_zone{zoneId1Based}_stage{stage1Based}";
        return backgroundSprites.TryGetValue(key, out Sprite sp) ? sp : null;
    }

    public Sprite GetCurrentBackground() => GetBackground(currentAreaId, currentStage);

    /// <summary>
    /// 퍼즐 클리어 시 호출 (GameController에서 호출)
    /// </summary>
    public void OnPuzzleClear(int starsEarned)
    {
        currentStars += starsEarned;
        Debug.Log($"별 획득: +{starsEarned} (현재: {currentStars})");

        UpdateUI();
        CheckStageCompletion();
    }

    /// <summary>
    /// UI 업데이트 (진행도 바, 텍스트 등)
    /// </summary>
    private void UpdateUI()
    {
        if (cafeData == null || cafeData.cafeAreas.Count == 0)
            return;

        CafeArea currentArea = cafeData.cafeAreas[currentAreaId - 1];
        RestorationStage currentStageData = currentArea.stages[currentStage - 1];

        // 진행도 바 업데이트
        float progress = (float)currentStars / currentStageData.starsRequired;
        progress = Mathf.Clamp01(progress);
        if (progressBar != null)
        {
            progressBar.fillAmount = progress;
        }

        // 텍스트 업데이트
        if (stageDescriptionText != null)
        {
            stageDescriptionText.text = $"{currentArea.areaName} - {currentStageData.description}";
        }

        if (starsRequiredText != null)
        {
            starsRequiredText.text = $"⭐ {currentStars} / {currentStageData.starsRequired}";
        }
    }

    /// <summary>
    /// 단계 완료 확인
    /// </summary>
    private void CheckStageCompletion()
    {
        if (cafeData == null || cafeData.cafeAreas.Count == 0)
            return;

        CafeArea currentArea = cafeData.cafeAreas[currentAreaId - 1];
        RestorationStage currentStageData = currentArea.stages[currentStage - 1];

        if (currentStars >= currentStageData.starsRequired)
        {
            CompleteStage();
        }
    }

    /// <summary>
    /// 단계 완료 처리
    /// </summary>
    private void CompleteStage()
    {
        Debug.Log($"[완료] 구역 {currentAreaId} - 단계 {currentStage} 완료!");

        // 시각적 변화 (배경 이미지 변경)
        UpdateCafeVisuals();

        // 알림 표시
        ShowCompletionNotification();

        // 이벤트 발생
        StageCompletedEvent?.Invoke(currentAreaId, currentStage);

        // 다음 단계로 진행
        MoveToNextStage();

        // UI 업데이트
        UpdateUI();
    }

    /// <summary>
    /// 카페 시각적 변화 (배경 이미지 변경)
    /// </summary>
    private void UpdateCafeVisuals()
    {
        if (cafeData == null || cafeData.cafeAreas.Count == 0)
            return;

        CafeArea currentArea = cafeData.cafeAreas[currentAreaId - 1];
        RestorationStage currentStageData = currentArea.stages[currentStage - 1];

        // 배경 이미지 변경
        if (backgroundSprites.ContainsKey(currentStageData.visualChangeKey))
        {
            if (cafeBackgroundImage != null)
            {
                StartCoroutine(FadeBackgroundImage(backgroundSprites[currentStageData.visualChangeKey]));
            }
        }
        else
        {
            Debug.LogWarning($"배경 이미지를 찾을 수 없음: {currentStageData.visualChangeKey}");
        }
    }

    /// <summary>
    /// 배경 이미지 페이드 전환 (부드러운 효과)
    /// </summary>
    private System.Collections.IEnumerator FadeBackgroundImage(Sprite newSprite)
    {
        float fadeDuration = 0.5f;
        float elapsedTime = 0f;

        Color originalColor = cafeBackgroundImage.color;

        // 페이드 아웃
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            cafeBackgroundImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        // 이미지 변경
        cafeBackgroundImage.sprite = newSprite;

        // 페이드 인
        elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            cafeBackgroundImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        cafeBackgroundImage.color = originalColor;
    }

    /// <summary>
    /// 단계 완료 알림 표시
    /// </summary>
    private void ShowCompletionNotification()
    {
        if (cafeData == null || cafeData.cafeAreas.Count == 0)
            return;

        CafeArea currentArea = cafeData.cafeAreas[currentAreaId - 1];
        RestorationStage currentStageData = currentArea.stages[currentStage - 1];

        string message = $"축하합니다!\n{currentArea.areaName}의 '{currentStageData.description}'이 완료되었습니다!";
        Debug.Log(message);

        // TODO: UI 팝업 표시 (Canvas에 알림 추가)
    }

    /// <summary>
    /// 다음 단계로 이동
    /// </summary>
    private void MoveToNextStage()
    {
        if (cafeData == null || cafeData.cafeAreas.Count == 0)
            return;

        CafeArea currentArea = cafeData.cafeAreas[currentAreaId - 1];

        // 현재 구역의 모든 단계가 완료되었는지 확인
        if (currentStage >= currentArea.stages.Count)
        {
            // 다음 구역으로 이동
            if (currentAreaId < cafeData.cafeAreas.Count)
            {
                currentAreaId++;
                currentStage = 1;
                currentStars = 0;
                Debug.Log($"다음 구역으로 이동: 구역 {currentAreaId}");
            }
            else
            {
                // 모든 구역 완료
                Debug.Log("모든 카페 복원이 완료되었습니다!");
                // TODO: 게임 완료 화면 표시
            }
        }
        else
        {
            // 다음 단계로 이동
            currentStage++;
            currentStars = 0;
            Debug.Log($"다음 단계로 이동: 단계 {currentStage}");
        }
    }

    /// <summary>
    /// 현재 진행도 반환 (0-1)
    /// </summary>
    public float GetCurrentProgress()
    {
        if (cafeData == null || cafeData.cafeAreas.Count == 0)
            return 0f;

        CafeArea currentArea = cafeData.cafeAreas[currentAreaId - 1];
        RestorationStage currentStageData = currentArea.stages[currentStage - 1];

        return (float)currentStars / currentStageData.starsRequired;
    }

    /// <summary>
    /// 현재 상태 정보 반환
    /// </summary>
    public string GetCurrentStatus()
    {
        if (cafeData == null || cafeData.cafeAreas.Count == 0)
            return "데이터 로드 중...";

        CafeArea currentArea = cafeData.cafeAreas[currentAreaId - 1];
        RestorationStage currentStageData = currentArea.stages[currentStage - 1];

        return $"{currentArea.areaName} - {currentStageData.description} ({currentStars}/{currentStageData.starsRequired})";
    }
}
