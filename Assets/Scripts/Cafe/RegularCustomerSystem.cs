using UnityEngine;
using System.Collections.Generic;
using WhiskerTales.Core;
using WhiskerTales.Utilities;

namespace WhiskerTales.Cafe
{
    /// <summary>
    /// 단골 손님 시스템
    /// 카페에 방문하는 손님들이 보상을 제공
    /// </summary>
    public class RegularCustomerSystem : MonoBehaviour
    {
        public static RegularCustomerSystem Instance { get; private set; }

        [SerializeField] private int maxCustomersPerDay = 5;
        [SerializeField] private float customerVisitInterval = 300f; // 5분

        private List<RegularCustomer> customers = new List<RegularCustomer>();
        private List<RegularCustomer> todayVisitors = new List<RegularCustomer>();
        private float timeSinceLastVisit = 0f;
        private GameManager gameManager;
        private Cat.CatManager catManager;

        // 이벤트
        public delegate void CustomerVisitedHandler(RegularCustomer customer);
        public event CustomerVisitedHandler OnCustomerVisited;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            gameManager = GameManager.Instance;
            catManager = Cat.CatManager.Instance;

            InitializeCustomers();
        }

        private void Update()
        {
            timeSinceLastVisit += Time.deltaTime;

            if (timeSinceLastVisit >= customerVisitInterval)
            {
                timeSinceLastVisit = 0f;
                TryCustomerVisit();
            }
        }

        /// <summary>
        /// 단골 손님 초기화
        /// </summary>
        private void InitializeCustomers()
        {
            // 5마리 고양이에 대응하는 5명의 단골 손님 생성
            CreateCustomer(1, "미라", Constants.CAT_NABI, 100, 2);
            CreateCustomer(2, "준호", Constants.CAT_LUNA, 120, 3);
            CreateCustomer(3, "지은", Constants.CAT_MUNGCHI, 80, 1);
            CreateCustomer(4, "철수", Constants.CAT_HODU, 150, 4);
            CreateCustomer(5, "영희", Constants.CAT_CHOCO, 110, 2);

            Debug.Log($"[RegularCustomerSystem] {customers.Count} customers initialized");
        }

        /// <summary>
        /// 단골 손님 생성 헬퍼
        /// </summary>
        private void CreateCustomer(int customerId, string name, int preferredCatId, int coinReward, int gemReward)
        {
            RegularCustomer customer = new RegularCustomer
            {
                customerId = customerId,
                name = name,
                preferredCatId = preferredCatId,
                coinReward = coinReward,
                gemReward = gemReward,
                portraitPath = $"Sprites/Customers/customer_{customerId}"
            };

            customers.Add(customer);
        }

        /// <summary>
        /// 손님 방문 시도
        /// </summary>
        private void TryCustomerVisit()
        {
            // 오늘 방문한 손님 수 확인
            if (todayVisitors.Count >= maxCustomersPerDay)
            {
                Debug.Log("[RegularCustomerSystem] Max customers visited today");
                return;
            }

            // 선호하는 고양이가 카페에 있는지 확인
            RegularCustomer customer = GetRandomCustomer();
            if (customer == null)
            {
                return;
            }

            // 선호하는 고양이가 언락되었는지 확인
            if (!catManager.IsCatUnlocked(customer.preferredCatId))
            {
                Debug.Log($"[RegularCustomerSystem] Customer {customer.name}'s preferred cat not unlocked");
                return;
            }

            // 손님 방문 처리
            VisitCustomer(customer);
        }

        /// <summary>
        /// 랜덤 손님 선택 (아직 방문하지 않은 손님)
        /// </summary>
        private RegularCustomer GetRandomCustomer()
        {
            List<RegularCustomer> availableCustomers = new List<RegularCustomer>();

            foreach (RegularCustomer customer in customers)
            {
                if (!todayVisitors.Contains(customer))
                {
                    availableCustomers.Add(customer);
                }
            }

            if (availableCustomers.Count == 0)
            {
                return null;
            }

            return availableCustomers[Random.Range(0, availableCustomers.Count)];
        }

        /// <summary>
        /// 손님 방문 처리
        /// </summary>
        private void VisitCustomer(RegularCustomer customer)
        {
            // 오늘 방문 목록에 추가
            todayVisitors.Add(customer);

            // 보상 지급
            gameManager.AddCoins(customer.coinReward);
            gameManager.AddGems(customer.gemReward);

            // 선호하는 고양이 호감도 증가
            catManager.IncreaseCatAffinity(customer.preferredCatId, 5);

            OnCustomerVisited?.Invoke(customer);

            Debug.Log($"[RegularCustomerSystem] Customer {customer.name} visited! Rewards: {customer.coinReward} coins, {customer.gemReward} gems");
        }

        /// <summary>
        /// 손님 반환
        /// </summary>
        public RegularCustomer GetCustomer(int customerId)
        {
            foreach (RegularCustomer customer in customers)
            {
                if (customer.customerId == customerId)
                    return customer;
            }
            return null;
        }

        /// <summary>
        /// 모든 손님 반환
        /// </summary>
        public List<RegularCustomer> GetAllCustomers()
        {
            return new List<RegularCustomer>(customers);
        }

        /// <summary>
        /// 오늘 방문한 손님 반환
        /// </summary>
        public List<RegularCustomer> GetTodayVisitors()
        {
            return new List<RegularCustomer>(todayVisitors);
        }

        /// <summary>
        /// 오늘 방문한 손님 수
        /// </summary>
        public int GetTodayVisitorCount()
        {
            return todayVisitors.Count;
        }

        /// <summary>
        /// 남은 방문 가능 손님 수
        /// </summary>
        public int GetRemainingCustomersToday()
        {
            return Mathf.Max(0, maxCustomersPerDay - todayVisitors.Count);
        }

        /// <summary>
        /// 일일 리셋 (자정 시)
        /// </summary>
        public void ResetDaily()
        {
            todayVisitors.Clear();
            timeSinceLastVisit = 0f;
            Debug.Log("[RegularCustomerSystem] Daily reset");
        }

        /// <summary>
        /// 특정 고양이를 선호하는 손님 반환
        /// </summary>
        public List<RegularCustomer> GetCustomersForCat(int catId)
        {
            List<RegularCustomer> catCustomers = new List<RegularCustomer>();

            foreach (RegularCustomer customer in customers)
            {
                if (customer.preferredCatId == catId)
                {
                    catCustomers.Add(customer);
                }
            }

            return catCustomers;
        }

        /// <summary>
        /// 손님 방문 확률 (%)
        /// </summary>
        public float GetVisitProbability()
        {
            if (GetRemainingCustomersToday() <= 0)
                return 0f;

            // 남은 손님이 많을수록 확률 증가
            return (GetRemainingCustomersToday() / (float)maxCustomersPerDay) * 100f;
        }
    }
}
