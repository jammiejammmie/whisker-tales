using System;
using System.Collections.Generic;
using UnityEngine;
using WhiskerTales.Utilities;

namespace WhiskerTales.Core
{
    /// <summary>
    /// 사용자 진행도 데이터
    /// </summary>
    [System.Serializable]
    public class UserProgress
    {
        public string userId;                           // Google Play ID
        public int currentLevel;                        // 현재 레벨 (1~50)
        public int completedLevels;                     // 완료한 레벨 수
        public int coins;                               // 동전
        public int stars;                               // 별
        public int gems;                                // 보석
        public int lives;                               // 생명 (최대 5)
        public DateTime lastPlayTime;                   // 마지막 플레이 시간
        public DateTime lastLifeRecoveryTime;           // 마지막 생명 회복 시간
        public List<int> unlockedCats = new List<int>(); // 획득한 고양이 ID
        public Dictionary<int, int> catAffinity = new Dictionary<int, int>(); // 고양이별 호감도 포인트
        public List<bool> completedTasks = new List<bool>(); // 완료한 복원 작업
        public bool adRemovalPurchased;                 // 광고 제거 구매 여부

        public UserProgress()
        {
            userId = "";
            currentLevel = 1;
            completedLevels = 0;
            coins = 100;                                // 초기 동전
            stars = 0;
            gems = 0;
            lives = Constants.MAX_LIVES;
            lastPlayTime = DateTime.Now;
            lastLifeRecoveryTime = DateTime.Now;
            adRemovalPurchased = false;

            // 초기 고양이 (나비)
            unlockedCats.Add(Constants.CAT_NABI);
            catAffinity[Constants.CAT_NABI] = 0;

            // 복원 작업 초기화
            for (int i = 0; i < Constants.TOTAL_RESTORATION_TASKS; i++)
            {
                completedTasks.Add(false);
            }
        }
    }

    /// <summary>
    /// 고양이 데이터
    /// </summary>
    [System.Serializable]
    public class Cat
    {
        public int catId;                              // 고양이 ID
        public string name;                            // 이름
        public string personality;                     // 성격
        public string story;                           // 구조 사연
        public int affinityLevel;                      // 호감도 레벨 (0~5)
        public int affinityPoints;                     // 호감도 포인트 (0~100)
        public string favoriteSnack;                   // 좋아하는 간식
        public string favoriteActivity;                // 좋아하는 활동
        public string portraitPath;                    // 일러스트 경로
        public List<string> animationPaths = new List<string>(); // 애니메이션 경로

        public Cat()
        {
            affinityLevel = 0;
            affinityPoints = 0;
        }

        /// <summary>
        /// 호감도 포인트 추가
        /// </summary>
        public void AddAffinityPoints(int points)
        {
            affinityPoints += points;
            
            // 호감도 레벨 업
            while (affinityPoints >= Constants.AFFINITY_POINTS_PER_LEVEL && 
                   affinityLevel < Constants.AFFINITY_LEVEL_MAX)
            {
                affinityPoints -= Constants.AFFINITY_POINTS_PER_LEVEL;
                affinityLevel++;
            }

            // 최대값 제한
            if (affinityLevel >= Constants.AFFINITY_LEVEL_MAX)
            {
                affinityLevel = Constants.AFFINITY_LEVEL_MAX;
                affinityPoints = Constants.AFFINITY_POINTS_PER_LEVEL - 1;
            }
        }

        /// <summary>
        /// 호감도 레벨 이름 반환
        /// </summary>
        public string GetAffinityLevelName()
        {
            return ((AffinityLevel)affinityLevel).ToString();
        }
    }

    /// <summary>
    /// 레벨 데이터
    /// </summary>
    [System.Serializable]
    public class Level
    {
        public int levelId;                            // 레벨 ID
        public int moveLimit;                          // 이동 제한
        public LevelGoalType goalType;                 // 목표 타입
        public int goalValue;                          // 목표값
        public int[] starThresholds = new int[3];      // 별 획득 기준 [1별, 2별, 3별]
        public int baseReward;                         // 기본 보상 (동전)
        public int difficulty;                        // 난이도 (1~10)
        public List<Obstacle> obstacles = new List<Obstacle>(); // 장애물

        public Level()
        {
            starThresholds[0] = 1000;                  // 1별: 1000점
            starThresholds[1] = 2000;                  // 2별: 2000점
            starThresholds[2] = 3000;                  // 3별: 3000점
        }

        /// <summary>
        /// 점수에 따른 별 개수 반환
        /// </summary>
        public int GetStarsByScore(int score)
        {
            if (score >= starThresholds[2]) return 3;
            if (score >= starThresholds[1]) return 2;
            if (score >= starThresholds[0]) return 1;
            return 0;
        }
    }

    /// <summary>
    /// 장애물 데이터
    /// </summary>
    [System.Serializable]
    public class Obstacle
    {
        public int x;                                  // X 좌표
        public int y;                                  // Y 좌표
        public ObstacleType type;                      // 장애물 타입
        public int health;                             // 내구도 (몇 번 맞아야 제거되는가)

        public Obstacle(int x, int y, ObstacleType type, int health = 1)
        {
            this.x = x;
            this.y = y;
            this.type = type;
            this.health = health;
        }
    }

    /// <summary>
    /// 복원 작업 데이터
    /// </summary>
    [System.Serializable]
    public class RestorationTask
    {
        public int taskId;                             // 작업 ID
        public string taskName;                        // 작업명
        public string description;                     // 설명
        public int requiredStars;                      // 필요한 별
        public int coinReward;                         // 동전 보상
        public int gemReward;                          // 보석 보상
        public bool isCompleted;                       // 완료 여부
        public string beforeImagePath;                 // 복원 전 이미지 경로
        public string afterImagePath;                  // 복원 후 이미지 경로

        public RestorationTask()
        {
            isCompleted = false;
        }
    }

    /// <summary>
    /// 레벨 진행도 데이터
    /// </summary>
    [System.Serializable]
    public class LevelProgress
    {
        public int levelId;                            // 레벨 ID
        public int attempts;                           // 시도 횟수
        public int bestScore;                          // 최고 점수
        public int starsEarned;                        // 획득한 별
        public bool isCompleted;                       // 완료 여부
        public DateTime completedTime;                 // 완료 시간

        public LevelProgress()
        {
            attempts = 0;
            bestScore = 0;
            starsEarned = 0;
            isCompleted = false;
        }
    }

    /// <summary>
    /// 경제 시스템 설정
    /// </summary>
    [System.Serializable]
    public class EconomyConfig
    {
        // 기본 보상
        public int coinPerMatch3 = Constants.COIN_PER_MATCH3;
        public int coinPerLevel = Constants.COIN_PER_LEVEL_CLEAR;
        public int starPerLevel = Constants.STAR_PER_LEVEL;

        // 광고 보상
        public int coinPerAdWatch = Constants.COIN_PER_AD_WATCH;
        public int gemPerAdWatch = Constants.GEM_PER_AD_WATCH;

        // 호감도 보상
        public int coinPerAffinity = Constants.COIN_PER_AFFINITY_LEVEL;
        public int gemPerAffinity = Constants.GEM_PER_AFFINITY_LEVEL;

        // 생명 회복
        public int lifeRecoveryMinutes = Constants.LIFE_RECOVERY_MINUTES;
        public int maxLives = Constants.MAX_LIVES;

        // 인앱결제 가격
        public float coinPackageSmall = Constants.COIN_PACKAGE_SMALL;
        public float coinPackageMedium = Constants.COIN_PACKAGE_MEDIUM;
        public float coinPackageLarge = Constants.COIN_PACKAGE_LARGE;
        public float adRemovalPrice = Constants.AD_REMOVAL_PRICE;

        // 동전 패키지 보상
        public int coinSmall = Constants.COIN_SMALL;
        public int coinMedium = Constants.COIN_MEDIUM;
        public int coinLarge = Constants.COIN_LARGE;
    }

    /// <summary>
    /// 단골 손님 데이터
    /// </summary>
    [System.Serializable]
    public class RegularCustomer
    {
        public int customerId;                         // 손님 ID
        public string name;                            // 이름
        public int preferredCatId;                     // 선호하는 고양이
        public int coinReward;                         // 동전 보상
        public int gemReward;                          // 보석 보상
        public string portraitPath;                    // 초상화 경로
    }

    /// <summary>
    /// 게임 저장 데이터 (전체)
    /// </summary>
    [System.Serializable]
    public class GameSaveData
    {
        public UserProgress userProgress;
        public List<Cat> cats = new List<Cat>();
        public List<LevelProgress> levelProgresses = new List<LevelProgress>();
        public List<RestorationTask> restorationTasks = new List<RestorationTask>();
        public EconomyConfig economyConfig;
        public DateTime lastSaveTime;

        public GameSaveData()
        {
            userProgress = new UserProgress();
            economyConfig = new EconomyConfig();
            lastSaveTime = DateTime.Now;
        }
    }
}
