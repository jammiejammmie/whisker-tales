using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// CatDexUI - 고양이 도감 UI 관리
/// 도감 화면의 모든 UI 요소를 제어합니다.
/// </summary>
public class CatDexUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup dexCanvasGroup;
    [SerializeField] private Button openDexButton;
    [SerializeField] private Button closeDexButton;
    [SerializeField] private Transform dexGridContent;
    [SerializeField] private GameObject catDexItemPrefab;
    [SerializeField] private ScrollRect dexScrollRect;
    [SerializeField] private Text collectionStatsText;
    [SerializeField] private Image rarityFilterImage;
    [SerializeField] private Button[] rarityFilterButtons;

    private int selectedRarityFilter = 0; // 0 = All, 1-4 = Specific rarity
    private bool isDexOpen = false;

    private void Start()
    {
        InitializeUI();
        SetupEventListeners();
    }

    /// <summary>
    /// UI 초기화
    /// </summary>
    private void InitializeUI()
    {
        // 도감 초기 상태: 닫혀있음
        dexCanvasGroup.alpha = 0;
        dexCanvasGroup.interactable = false;
        dexCanvasGroup.blocksRaycasts = false;
    }

    /// <summary>
    /// 이벤트 리스너 설정
    /// </summary>
    private void SetupEventListeners()
    {
        openDexButton.onClick.AddListener(OpenDex);
        closeDexButton.onClick.AddListener(CloseDex);

        for (int i = 0; i < rarityFilterButtons.Length; i++)
        {
            int index = i;
            rarityFilterButtons[i].onClick.AddListener(() => FilterByRarity(index));
        }
    }

    /// <summary>
    /// 도감 열기
    /// </summary>
    public void OpenDex()
    {
        if (isDexOpen) return;

        isDexOpen = true;
        StartCoroutine(AnimateDexOpen());
        UpdateCollectionStats();
    }

    /// <summary>
    /// 도감 닫기
    /// </summary>
    public void CloseDex()
    {
        if (!isDexOpen) return;

        isDexOpen = false;
        StartCoroutine(AnimateDexClose());
    }

    /// <summary>
    /// 도감 열기 애니메이션
    /// </summary>
    private System.Collections.IEnumerator AnimateDexOpen()
    {
        dexCanvasGroup.interactable = true;
        dexCanvasGroup.blocksRaycasts = true;

        float duration = 0.3f;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            dexCanvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        dexCanvasGroup.alpha = 1;
    }

    /// <summary>
    /// 도감 닫기 애니메이션
    /// </summary>
    private System.Collections.IEnumerator AnimateDexClose()
    {
        float duration = 0.3f;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            dexCanvasGroup.alpha = 1 - Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        dexCanvasGroup.alpha = 0;
        dexCanvasGroup.interactable = false;
        dexCanvasGroup.blocksRaycasts = false;
    }

    /// <summary>
    /// 레어도별 필터링
    /// </summary>
    private void FilterByRarity(int rarity)
    {
        selectedRarityFilter = rarity;
        // TODO: 필터링 로직 구현
        Debug.Log($"필터: 레어도 {rarity}");
    }

    /// <summary>
    /// 수집 통계 업데이트
    /// </summary>
    private void UpdateCollectionStats()
    {
        int collected = CatDexManager.Instance.GetCollectionPercentage();
        collectionStatsText.text = $"수집률: {collected}%";
    }

    /// <summary>
    /// 도감 그리드 갱신
    /// </summary>
    public void RefreshDexGrid()
    {
        CatDexManager.Instance.UpdateDexUI();
    }

    /// <summary>
    /// 도감 열려있는지 확인
    /// </summary>
    public bool IsDexOpen()
    {
        return isDexOpen;
    }
}
