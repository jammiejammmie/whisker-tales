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

            CreateCat(Constants.CAT_LUNA, "루나", "신비롭고 우아함",
                "달빛 아래서 만난 루나는 고급스러운 분위기를 풍깁니다.",
                "우유", "창밖 구경하기");

            CreateCat(Constants.CAT_MUNGCHI, "뭉치", "귀엽고 순진함",
                "가장 어린 뭉치는 항상 졸려있지만 사람들의 마음을 녹입니다.",
                "참치", "뭉치볼 가지고 놀기");

            CreateCat(Constants.CAT_HODU, "호두", "차분하고 지혜로움",
                "나이 많은 호두는 카페의 든든한 지주입니다.",
                "닭고기", "무릎에 앉기");

            CreateCat(Constants.CAT_CHOCO, "초코", "활동적이고 장난스러움",
                "초코는 항상 뛰어다니며 모든 것을 탐험하고 싶어합니다.",
                "새우", "공 가지고 놀기");

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
