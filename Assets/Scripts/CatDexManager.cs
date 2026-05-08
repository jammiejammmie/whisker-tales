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
            new CatData { catId = 1, catName = "나비",    breed = "삼색",       personality = "밝고 활발함",       rarity = 1, catDescription = "네잎클로버 초록 칼라가 트레이드마크인 삼색이." },
            new CatData { catId = 2, catName = "벨라",    breed = "흰털 공주",  personality = "우아하고 도도함",   rarity = 1, catDescription = "흰털에 핑크 리본을 단 공주님." },
            new CatData { catId = 3, catName = "사미",    breed = "시암",       personality = "신비롭고 영리함",   rarity = 1, catDescription = "파란 눈에 파란 칼라를 두른 시암 출신." },
            new CatData { catId = 4, catName = "호두",    breed = "태비",       personality = "차분하고 지혜로움", rarity = 1, catDescription = "머스타드 스카프를 두른 태비. 카페의 지주." },
            new CatData { catId = 5, catName = "구름이",  breed = "회색 단모종", personality = "온순하고 포근함",   rarity = 1, catDescription = "비 오는 날 처마 밑에서 발견된 회색 고양이. 파란 스카프." }
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
