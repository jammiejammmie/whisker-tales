using UnityEngine;

namespace WhiskerTales.Utilities
{
    /// <summary>
    /// 게임 전체에서 사용되는 상수 정의
    /// </summary>
    public static class Constants
    {
        // ===== 게임 설정 =====
        public const int BOARD_SIZE = 8;                    // 보드 크기 (8x8)
        public const int TOTAL_LEVELS = 50;                 // 총 레벨 수
        public const int TOTAL_CATS = 5;                    // 출시 시 고양이 수
        public const int TOTAL_RESTORATION_TASKS = 20;      // 복원 작업 수
        public const int MAX_LIVES = 5;                     // 최대 생명
        public const int LIFE_RECOVERY_MINUTES = 30;        // 생명 회복 시간 (분)

        // ===== 매치-3 시스템 =====
        public const int MATCH_MINIMUM = 3;                 // 최소 매치 개수
        public const int ROCKET_MATCH = 4;                  // 로켓 생성 매치 (직선 4개)
        public const int BOMB_MATCH = 5;                    // 폭탄 생성 매치 (L/T 5개)
        public const int RAINBOW_MATCH = 5;                 // 무지개 생성 매치 (직선 5개)

        // ===== 보상 시스템 =====
        public const int COIN_PER_MATCH3 = 10;              // 매치-3당 동전
        public const int COIN_PER_LEVEL_CLEAR = 100;        // 레벨 클리어 보상
        public const int STAR_PER_LEVEL = 1;                // 레벨당 별
        public const int COIN_PER_AD_WATCH = 50;            // 광고 시청당 동전
        public const int GEM_PER_AD_WATCH = 1;              // 광고 시청당 보석
        public const int COIN_PER_AFFINITY_LEVEL = 50;      // 호감도 레벨당 동전
        public const int GEM_PER_AFFINITY_LEVEL = 5;        // 호감도 레벨당 보석

        // ===== 고양이 호감도 =====
        public const int AFFINITY_LEVEL_MAX = 5;            // 최대 호감도 레벨
        public const int AFFINITY_POINTS_PER_LEVEL = 100;   // 레벨당 호감도 포인트
        public const int AFFINITY_PER_PET = 5;              // 쓰다듬기당 호감도
        public const int AFFINITY_PER_SNACK = 15;           // 간식 주기당 호감도
        public const int AFFINITY_PER_PLAY = 10;            // 놀아주기당 호감도

        // ===== 카페 복원 =====
        public const int CAFE_AREAS = 3;                    // 카페 구역 수
        public const int TASKS_PER_AREA = 7;                // 구역당 복원 작업 수

        // ===== 인앱결제 가격 (USD) =====
        public const float COIN_PACKAGE_SMALL = 1.99f;      // 소형 동전 패키지
        public const float COIN_PACKAGE_MEDIUM = 4.99f;     // 중형 동전 패키지
        public const float COIN_PACKAGE_LARGE = 9.99f;      // 대형 동전 패키지
        public const float AD_REMOVAL_PRICE = 4.99f;        // 광고 제거 가격
        public const float SEASON_PASS_PRICE = 9.99f;       // 시즌 패스 가격

        // ===== 동전 패키지 보상 =====
        public const int COIN_SMALL = 500;                  // 소형 패키지 동전
        public const int COIN_MEDIUM = 1500;                // 중형 패키지 동전
        public const int COIN_LARGE = 4000;                 // 대형 패키지 동전

        // ===== 고양이 ID =====
        public const int CAT_NABI = 1;                      // 나비
        public const int CAT_LUNA = 2;                      // 루나
        public const int CAT_MUNGCHI = 3;                   // 뭉치
        public const int CAT_HODU = 4;                      // 호두
        public const int CAT_CHOCO = 5;                     // 초코

        // ===== 난이도 =====
        public const int DIFFICULTY_EASY = 1;              // 쉬움
        public const int DIFFICULTY_NORMAL = 5;            // 보통
        public const int DIFFICULTY_HARD = 10;             // 어려움

        // ===== 저장 데이터 키 =====
        public const string SAVE_KEY_USER_PROGRESS = "UserProgress";
        public const string SAVE_KEY_LEVEL_DATA = "LevelData_";
        public const string SAVE_KEY_CATS = "Cats";
        public const string SAVE_KEY_CAFE = "CafeData";

        // ===== UI 설정 =====
        public const float UI_BUTTON_SCALE_NORMAL = 1f;
        public const float UI_BUTTON_SCALE_PRESSED = 0.95f;
        public const float UI_ANIMATION_DURATION = 0.3f;

        // ===== 방치형 보상 =====
        public const int OFFLINE_REWARD_MAX_HOURS = 8;      // 최대 오프라인 보상 시간
        public const int COIN_PER_OFFLINE_HOUR = 50;        // 시간당 동전
        public const int OFFLINE_REWARD_INTERVAL_MINUTES = 30; // 보상 간격 (분)

        // ===== 레벨 기본값 =====
        public const int LEVEL_MOVE_LIMIT_MIN = 20;         // 최소 이동 제한
        public const int LEVEL_MOVE_LIMIT_MAX = 40;         // 최대 이동 제한
    }

    /// <summary>
    /// 게임 상태 열거형
    /// </summary>
    public enum GameState
    {
        MainMenu,       // 메인 메뉴
        Playing,        // 게임 플레이 중
        Paused,         // 일시 정지
        LevelComplete,  // 레벨 완료
        LevelFailed,    // 레벨 실패
        Cafe,           // 카페 화면
        Settings        // 설정 화면
    }

    /// <summary>
    /// 레벨 목표 타입
    /// </summary>
    public enum LevelGoalType
    {
        RemoveBlocks,      // 블록 제거
        CollectItems,      // 아이템 수집
        ReachScore,        // 점수 달성
        DestroyObstacles   // 장애물 제거
    }

    /// <summary>
    /// 타일 타입
    /// </summary>
    public enum TileType
    {
        Red,           // 빨강
        Blue,          // 파랑
        Green,         // 초록
        Yellow,        // 노랑
        Purple,        // 보라
        Orange,        // 주황
        Empty,         // 비어있음
        Obstacle       // 장애물
    }

    /// <summary>
    /// 특수 아이템 타입
    /// </summary>
    public enum SpecialItemType
    {
        None,          // 없음
        Rocket,        // 로켓
        Bomb,          // 폭탄
        Rainbow,       // 무지개
        Hammer         // 망치
    }

    /// <summary>
    /// 호감도 레벨
    /// </summary>
    public enum AffinityLevel
    {
        Stranger,      // 낯선 (0)
        Acquainted,    // 친해지는 중 (1)
        Friend,        // 친구 (2)
        BestFriend,    // 단짝 (3)
        Family         // 가족 (4)
    }

    /// <summary>
    /// 장애물 타입
    /// </summary>
    public enum ObstacleType
    {
        Box,           // 상자
        Ice,           // 얼음
        Lock,          // 자물쇠
        Chain          // 사슬
    }
}
