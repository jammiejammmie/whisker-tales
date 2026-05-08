using UnityEngine;
using System.Collections.Generic;
using WhiskerTales.Core;
using WhiskerTales.Utilities;

namespace WhiskerTales.Cat
{
    /// <summary>
    /// 고양이 시스템 관리
    /// 고양이 데이터, 호감도, 언락 관리
    /// </summary>
    public class CatManager : MonoBehaviour
    {
        public static CatManager Instance { get; private set; }

        private Dictionary<int, Core.Cat> cats = new Dictionary<int, Core.Cat>();
        private DataManager dataManager;
        private GameManager gameManager;

        // 이벤트
        public delegate void CatUnlockedHandler(int catId);
        public event CatUnlockedHandler OnCatUnlocked;

        public delegate void CatAffinityChangedHandler(int catId, int newAffinity);
        public event CatAffinityChangedHandler OnCatAffinityChanged;

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

            InitializeCats();
        }

        /// <summary>
        /// 고양이 초기화
        /// </summary>
        private void InitializeCats()
        {
            // 초기 5마리 고양이 데이터 설정
            CreateCat(Constants.CAT_NABI, "나비", "밝고 활발함", 
                "길을 헤매던 나비를 발견했어요. 밝은 성격으로 카페에 활기를 가져다줍니다.",
                "생선", "낚싯대로 놀기");

            CreateCat(Constants.CAT_BELLA, "벨라", "우아하고 도도함",
                "흰털에 핑크 리본을 단 공주님. 카페에서 가장 도도한 자리를 지킵니다.",
                "우유", "거울 앞에서 단장하기");

            CreateCat(Constants.CAT_SAMI, "사미", "신비롭고 영리함",
                "파란 눈의 시암 출신 사미. 조용히 창밖을 응시하는 모습이 일품입니다.",
                "참치", "창밖 구경하기");

            CreateCat(Constants.CAT_HODU, "호두", "차분하고 지혜로움",
                "머스타드 스카프를 두른 태비. 카페의 든든한 지주입니다.",
                "닭고기", "무릎에 앉기");

            CreateCat(Constants.CAT_GUREUMI, "구름이", "온순하고 포근함",
                "비 오는 날 처마 밑에서 발견된 회색 고양이. 파란 스카프가 트레이드마크입니다.",
                "삶은 계란", "포근하게 잠자기");

            Debug.Log("[CatManager] Cats initialized");
        }

        /// <summary>
        /// 고양이 생성 헬퍼
        /// </summary>
        private void CreateCat(int catId, string name, string personality, string story, 
                               string favoriteSnack, string favoriteActivity)
        {
            Core.Cat cat = new Core.Cat
            {
                catId = catId,
                name = name,
                personality = personality,
                story = story,
                favoriteSnack = favoriteSnack,
                favoriteActivity = favoriteActivity,
                affinityLevel = 0,
                affinityPoints = 0,
                portraitPath = $"Sprites/Cats/cat_{catId}",
                animationPaths = new List<string>
                {
                    $"Animations/Cat/cat_{catId}_walk",
                    $"Animations/Cat/cat_{catId}_play",
                    $"Animations/Cat/cat_{catId}_sleep"
                }
            };

            cats[catId] = cat;
        }

        /// <summary>
        /// 고양이 언락
        /// </summary>
        public void UnlockCat(int catId)
        {
            if (!cats.ContainsKey(catId))
            {
                Debug.LogError($"[CatManager] Cat {catId} not found");
                return;
            }

            if (gameManager.UserProgress.unlockedCats.Contains(catId))
            {
                Debug.LogWarning($"[CatManager] Cat {catId} already unlocked");
                return;
            }

            gameManager.UserProgress.unlockedCats.Add(catId);
            gameManager.UserProgress.catAffinity[catId] = 0;

            OnCatUnlocked?.Invoke(catId);
            Debug.Log($"[CatManager] Cat {catId} ({cats[catId].name}) unlocked!");
        }

        /// <summary>
        /// 고양이 호감도 증가
        /// </summary>
        public void IncreaseCatAffinity(int catId, int points)
        {
            if (!cats.ContainsKey(catId))
            {
                Debug.LogError($"[CatManager] Cat {catId} not found");
                return;
            }

            if (!gameManager.UserProgress.unlockedCats.Contains(catId))
            {
                Debug.LogWarning($"[CatManager] Cat {catId} not unlocked");
                return;
            }

            Core.Cat cat = cats[catId];
            cat.AddAffinityPoints(points);

            // 호감도 레벨 업 시 보상
            if (cat.affinityLevel > 0 && cat.affinityPoints < Constants.AFFINITY_POINTS_PER_LEVEL / 2)
            {
                int coinReward = Constants.COIN_PER_AFFINITY_LEVEL;
                int gemReward = Constants.GEM_PER_AFFINITY_LEVEL;

                gameManager.AddCoins(coinReward);
                gameManager.AddGems(gemReward);

                Debug.Log($"[CatManager] Cat {catId} affinity level up! Rewards: {coinReward} coins, {gemReward} gems");
            }

            OnCatAffinityChanged?.Invoke(catId, cat.affinityPoints);
            Debug.Log($"[CatManager] Cat {catId} affinity increased by {points} (Total: {cat.affinityPoints})");
        }

        /// <summary>
        /// 쓰다듬기 (매일 1회)
        /// </summary>
        public void PetCat(int catId)
        {
            IncreaseCatAffinity(catId, Constants.AFFINITY_PER_PET);
            gameManager.AddCoins(10);
            Debug.Log($"[CatManager] Petted cat {catId}");
        }

        /// <summary>
        /// 간식 주기
        /// </summary>
        public void GiveSnack(int catId)
        {
            if (!gameManager.SpendCoins(50))
            {
                Debug.LogWarning("[CatManager] Not enough coins to give snack");
                return;
            }

            IncreaseCatAffinity(catId, Constants.AFFINITY_PER_SNACK);
            Debug.Log($"[CatManager] Gave snack to cat {catId}");
        }

        /// <summary>
        /// 놀아주기
        /// </summary>
        public void PlayWithCat(int catId)
        {
            IncreaseCatAffinity(catId, Constants.AFFINITY_PER_PLAY);
            Debug.Log($"[CatManager] Played with cat {catId}");
        }

        /// <summary>
        /// 고양이 데이터 반환
        /// </summary>
        public Core.Cat GetCat(int catId)
        {
            if (cats.ContainsKey(catId))
            {
                return cats[catId];
            }
            else
            {
                Debug.LogError($"[CatManager] Cat {catId} not found");
                return null;
            }
        }

        /// <summary>
        /// 언락된 모든 고양이 반환
        /// </summary>
        public List<Core.Cat> GetUnlockedCats()
        {
            List<Core.Cat> unlockedCats = new List<Core.Cat>();

            foreach (int catId in gameManager.UserProgress.unlockedCats)
            {
                if (cats.ContainsKey(catId))
                {
                    unlockedCats.Add(cats[catId]);
                }
            }

            return unlockedCats;
        }

        /// <summary>
        /// 모든 고양이 반환
        /// </summary>
        public List<Core.Cat> GetAllCats()
        {
            return new List<Core.Cat>(cats.Values);
        }

        /// <summary>
        /// 고양이가 언락되었는지 확인
        /// </summary>
        public bool IsCatUnlocked(int catId)
        {
            return gameManager.UserProgress.unlockedCats.Contains(catId);
        }

        /// <summary>
        /// 고양이 호감도 레벨 반환
        /// </summary>
        public int GetCatAffinityLevel(int catId)
        {
            if (cats.ContainsKey(catId))
            {
                return cats[catId].affinityLevel;
            }
            return 0;
        }

        /// <summary>
        /// 고양이 호감도 포인트 반환
        /// </summary>
        public int GetCatAffinityPoints(int catId)
        {
            if (cats.ContainsKey(catId))
            {
                return cats[catId].affinityPoints;
            }
            return 0;
        }
    }
}
