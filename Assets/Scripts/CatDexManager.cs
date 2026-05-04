using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// CatDexManager - 고양이 도감 시스템
/// 게임 진행 중 수집한 고양이들을 관리하고 도감 UI를 제어합니다.
/// </summary>
public class CatDexManager : MonoBehaviour
{
    public static CatDexManager Instance { get; private set; }

    [System.Serializable]
    public class CatData
    {
        public int catId;
        public string catName;
        public string catDescription;
        public Sprite catImage;
        public string breed;
        public string personality;
        public int rarity; // 1: Common, 2: Uncommon, 3: Rare, 4: Legendary
        public bool isCollected;
        public int collectionDate; // Unix timestamp
    }

    [SerializeField] private List<CatData> allCats = new List<CatData>();
    [SerializeField] private List<int> collectedCatIds = new List<int>();
    [SerializeField] private Transform dexGridContent;
    [SerializeField] private GameObject catDexItemPrefab;
    [SerializeField] private Text totalCatsText;
    [SerializeField] private Text collectedCatsText;
    [SerializeField] private Image catDetailImage;
    [SerializeField] private Text catDetailName;
    [SerializeField] private Text catDetailDescription;
    [SerializeField] private Text catDetailBreed;
    [SerializeField] private Text catDetailPersonality;
    [SerializeField] private Image rarityStars;

    private CatData selectedCat;
    private bool isDexOpen = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        InitializeCats();
        LoadCollectedCats();
        UpdateDexUI();
    }

    /// <summary>
    /// 모든 고양이 데이터 초기화
    /// </summary>
    private void InitializeCats()
    {
        // 게임에 존재하는 모든 고양이 데이터
        // 실제로는 JSON이나 ScriptableObject에서 로드
        allCats = new List<CatData>
        {
            // Tier 1: Common (5마리)
            new CatData { catId = 1, catName = "주황이", breed = "길냥이", personality = "활발함", rarity = 1, catDescription = "밝고 활발한 주황색 고양이" },
            new CatData { catId = 2, catName = "검은고양이", breed = "검은 고양이", personality = "신비로움", rarity = 1, catDescription = "신비로운 검은색 고양이" },
            new CatData { catId = 3, catName = "흰눈이", breed = "페르시안", personality = "우아함", rarity = 1, catDescription = "우아한 흰색 고양이" },
            new CatData { catId = 4, catName = "얼룩이", breed = "길냥이", personality = "장난꾸러기", rarity = 1, catDescription = "장난을 좋아하는 얼룩 고양이" },
            new CatData { catId = 5, catName = "회색이", breed = "러시안 블루", personality = "조용함", rarity = 1, catDescription = "조용하고 차분한 회색 고양이" },

            // Tier 2: Uncommon (5마리)
            new CatData { catId = 6, catName = "금눈이", breed = "벵갈", personality = "지능적", rarity = 2, catDescription = "지능적이고 호기심 많은 고양이" },
            new CatData { catId = 7, catName = "복슬이", breed = "메인쿤", personality = "친절함", rarity = 2, catDescription = "크고 친절한 메인쿤" },
            new CatData { catId = 8, catName = "미니미", breed = "싱가푸라", personality = "귀여움", rarity = 2, catDescription = "작고 귀여운 싱가푸라" },
            new CatData { catId = 9, catName = "점박이", breed = "이집션 마우", personality = "민첩함", rarity = 2, catDescription = "민첩하고 빠른 고양이" },
            new CatData { catId = 10, catName = "솜털이", breed = "랙돌", personality = "온순함", rarity = 2, catDescription = "온순하고 부드러운 랙돌" },

            // Tier 3: Rare (3마리)
            new CatData { catId = 11, catName = "황금냥", breed = "버마", personality = "신비로움", rarity = 3, catDescription = "황금빛으로 빛나는 신비한 고양이" },
            new CatData { catId = 12, catName = "보라냥", breed = "코랫", personality = "우아함", rarity = 3, catDescription = "보라빛 눈을 가진 우아한 고양이" },
            new CatData { catId = 13, catName = "은색냥", breed = "톤키니즈", personality = "영리함", rarity = 3, catDescription = "은색으로 빛나는 영리한 고양이" },

            // Tier 4: Legendary (2마리)
            new CatData { catId = 14, catName = "무지개냥", breed = "전설의 고양이", personality = "신성함", rarity = 4, catDescription = "무지개 빛으로 빛나는 전설의 고양이" },
            new CatData { catId = 15, catName = "달의 고양이", breed = "달의 정령", personality = "신비함", rarity = 4, catDescription = "달빛처럼 빛나는 신비로운 고양이" }
        };

        // 초기화: 모든 고양이를 미수집 상태로
        foreach (var cat in allCats)
        {
            cat.isCollected = false;
        }
    }

    /// <summary>
    /// 고양이 수집
    /// </summary>
    public void CollectCat(int catId)
    {
        if (collectedCatIds.Contains(catId)) return;

        CatData cat = allCats.FirstOrDefault(c => c.catId == catId);
        if (cat != null)
        {
            cat.isCollected = true;
            cat.collectionDate = (int)System.DateTime.Now.Subtract(new System.DateTime(1970, 1, 1)).TotalSeconds;
            collectedCatIds.Add(catId);
            SaveCollectedCats();
            UpdateDexUI();

            // 새로운 고양이 수집 알림
            ShowCatCollectionNotification(cat);
        }
    }

    /// <summary>
    /// 고양이 수집 알림 표시
    /// </summary>
    private void ShowCatCollectionNotification(CatData cat)
    {
        // TODO: Toast 또는 팝업으로 알림 표시
        Debug.Log($"새로운 고양이 수집: {cat.catName}!");
    }

    /// <summary>
    /// 도감 UI 업데이트
    /// </summary>
    public void UpdateDexUI()
    {
        // 총 고양이 수와 수집한 고양이 수 표시
        totalCatsText.text = $"/ {allCats.Count}";
        collectedCatsText.text = collectedCatIds.Count.ToString();

        // 도감 그리드 갱신
        RefreshDexGrid();
    }

    /// <summary>
    /// 도감 그리드 갱신
    /// </summary>
    private void RefreshDexGrid()
    {
        // 기존 아이템 제거
        foreach (Transform child in dexGridContent)
        {
            Destroy(child.gameObject);
        }

        // 모든 고양이에 대해 도감 아이템 생성
        foreach (var cat in allCats)
        {
            GameObject item = Instantiate(catDexItemPrefab, dexGridContent);
            Image catImage = item.GetComponent<Image>();
            Button catButton = item.GetComponent<Button>();

            if (cat.isCollected)
            {
                catImage.sprite = cat.catImage;
                catImage.color = Color.white;
                catButton.onClick.AddListener(() => ShowCatDetail(cat));
            }
            else
            {
                // 미수집 고양이는 물음표 표시
                catImage.color = new Color(0.3f, 0.3f, 0.3f);
                catButton.interactable = false;
            }
        }
    }

    /// <summary>
    /// 고양이 상세 정보 표시
    /// </summary>
    public void ShowCatDetail(CatData cat)
    {
        selectedCat = cat;
        catDetailImage.sprite = cat.catImage;
        catDetailName.text = cat.catName;
        catDetailDescription.text = cat.catDescription;
        catDetailBreed.text = $"품종: {cat.breed}";
        catDetailPersonality.text = $"성격: {cat.personality}";

        // 레어도 별 표시
        UpdateRarityStars(cat.rarity);
    }

    /// <summary>
    /// 레어도 별 표시
    /// </summary>
    private void UpdateRarityStars(int rarity)
    {
        // TODO: 별 이미지 업데이트
        // rarity: 1 = ★☆☆☆☆, 2 = ★★☆☆☆, 3 = ★★★☆☆, 4 = ★★★★★
    }

    /// <summary>
    /// 도감 열기/닫기
    /// </summary>
    public void ToggleDex()
    {
        isDexOpen = !isDexOpen;
        // TODO: 도감 UI 애니메이션
    }

    /// <summary>
    /// 수집한 고양이 저장
    /// </summary>
    private void SaveCollectedCats()
    {
        string json = JsonUtility.ToJson(new CatCollectionData { collectedIds = collectedCatIds });
        PlayerPrefs.SetString("CollectedCats", json);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 수집한 고양이 로드
    /// </summary>
    private void LoadCollectedCats()
    {
        if (PlayerPrefs.HasKey("CollectedCats"))
        {
            string json = PlayerPrefs.GetString("CollectedCats");
            CatCollectionData data = JsonUtility.FromJson<CatCollectionData>(json);
            collectedCatIds = data.collectedIds;

            // 로드한 ID로 고양이 상태 업데이트
            foreach (int id in collectedCatIds)
            {
                CatData cat = allCats.FirstOrDefault(c => c.catId == id);
                if (cat != null)
                {
                    cat.isCollected = true;
                }
            }
        }
    }

    /// <summary>
    /// 수집 통계
    /// </summary>
    public int GetCollectionPercentage()
    {
        return Mathf.RoundToInt((float)collectedCatIds.Count / allCats.Count * 100);
    }

    /// <summary>
    /// 특정 레어도의 고양이 수집 여부
    /// </summary>
    public int GetCollectedByRarity(int rarity)
    {
        return allCats.Count(c => c.rarity == rarity && c.isCollected);
    }

    /// <summary>
    /// 다음 수집할 고양이 추천
    /// </summary>
    public CatData GetNextCatToCollect()
    {
        return allCats.FirstOrDefault(c => !c.isCollected);
    }

    [System.Serializable]
    private class CatCollectionData
    {
        public List<int> collectedIds = new List<int>();
    }
}
