# 냥이의 집 - Unity 프로젝트 구조 설계

**작성일**: 2026년 5월 4일  
**목표**: Unity 2022.3.21f LTS 기반 프로젝트 구조 설계 및 데이터 모델 정의

---

## 📁 프로젝트 폴더 구조

```
WhiskerTales/
├── Assets/
│   ├── Scripts/
│   │   ├── Core/                    # 핵심 시스템
│   │   │   ├── GameManager.cs       # 게임 전체 관리
│   │   │   ├── DataManager.cs       # 데이터 저장/로드
│   │   │   └── AudioManager.cs      # 사운드 관리
│   │   │
│   │   ├── Puzzle/                  # 매치-3 퍼즐 시스템
│   │   │   ├── Board.cs             # 게임 보드 (8x8 그리드)
│   │   │   ├── Tile.cs              # 개별 타일
│   │   │   ├── MatchLogic.cs        # 매치 판정 알고리즘
│   │   │   ├── SpecialItem.cs       # 폭탄, 번개 등
│   │   │   └── LevelGoal.cs         # 레벨 목표 관리
│   │   │
│   │   ├── Cat/                     # 고양이 시스템
│   │   │   ├── CatManager.cs        # 고양이 관리
│   │   │   ├── Cat.cs               # 고양이 데이터 & 로직
│   │   │   ├── CatAffinity.cs       # 호감도 시스템
│   │   │   └── CatAnimation.cs      # 고양이 애니메이션
│   │   │
│   │   ├── Cafe/                    # 카페 복원 시스템
│   │   │   ├── CafeManager.cs       # 카페 관리
│   │   │   ├── RestorationTask.cs   # 복원 작업
│   │   │   ├── Furniture.cs         # 가구 배치
│   │   │   └── RegularCustomer.cs   # 단골 손님
│   │   │
│   │   ├── Economy/                 # 경제 시스템
│   │   │   ├── Currency.cs          # 화폐 (동전, 별, 보석)
│   │   │   ├── Shop.cs              # 상점
│   │   │   └── Reward.cs            # 보상 시스템
│   │   │
│   │   ├── UI/                      # UI 시스템
│   │   │   ├── MainMenuUI.cs        # 메인 메뉴
│   │   │   ├── GameplayUI.cs        # 게임 플레이 UI
│   │   │   ├── ResultUI.cs          # 결과 화면
│   │   │   ├── CafeUI.cs            # 카페 UI
│   │   │   └── SettingsUI.cs        # 설정 UI
│   │   │
│   │   ├── Services/                # 외부 서비스
│   │   │   ├── GooglePlayServices.cs # Google Play 로그인
│   │   │   ├── AdMobManager.cs      # 광고 관리
│   │   │   ├── BillingManager.cs    # 결제 관리
│   │   │   └── AnalyticsManager.cs  # 분석
│   │   │
│   │   └── Utilities/               # 유틸리티
│   │       ├── Constants.cs         # 상수 정의
│   │       ├── Extensions.cs        # 확장 메서드
│   │       └── Logger.cs            # 로깅
│   │
│   ├── Scenes/
│   │   ├── MainMenu.unity           # 메인 메뉴 씬
│   │   ├── Gameplay.unity           # 게임 플레이 씬
│   │   └── Cafe.unity               # 카페 씬
│   │
│   ├── Prefabs/
│   │   ├── Tiles/
│   │   │   └── Tile.prefab          # 타일 프리팹
│   │   ├── UI/
│   │   │   └── Button.prefab        # 버튼 프리팹
│   │   └── Effects/
│   │       ├── Explosion.prefab     # 폭탄 폭발 이펙트
│   │       └── Particle.prefab      # 파티클 이펙트
│   │
│   ├── Resources/
│   │   ├── Data/
│   │   │   ├── Levels/              # 레벨 데이터 (JSON/CSV)
│   │   │   ├── Cats/                # 고양이 데이터
│   │   │   └── Config/              # 게임 설정
│   │   ├── Audio/
│   │   │   ├── BGM/                 # 배경음
│   │   │   └── SFX/                 # 효과음
│   │   └── Sprites/
│   │       ├── Tiles/               # 타일 스프라이트
│   │       ├── Cats/                # 고양이 스프라이트
│   │       └── UI/                  # UI 스프라이트
│   │
│   └── Animations/
│       ├── Cat/                     # 고양이 애니메이션
│       ├── Tile/                    # 타일 애니메이션
│       └── UI/                      # UI 애니메이션
│
├── ProjectSettings/                 # Unity 프로젝트 설정
├── Packages/                        # 패키지 관리
└── README.md                        # 프로젝트 문서

```

---

## 📊 데이터 모델 정의

### 1. 사용자 진행도 (UserProgress)

```csharp
[System.Serializable]
public class UserProgress
{
    public string userId;                    // Google Play ID
    public int currentLevel;                 // 현재 레벨 (1~50)
    public int completedLevels;              // 완료한 레벨 수
    public int coins;                        // 동전
    public int stars;                        // 별
    public int gems;                         // 보석
    public int lives;                        // 생명 (최대 5)
    public DateTime lastPlayTime;            // 마지막 플레이 시간
    public List<int> unlockedCats;           // 획득한 고양이 ID
    public Dictionary<int, int> catAffinity; // 고양이별 호감도
    public List<bool> completedTasks;        // 완료한 복원 작업
}
```

### 2. 고양이 데이터 (Cat)

```csharp
[System.Serializable]
public class Cat
{
    public int catId;                  // 고양이 ID (1~5)
    public string name;                // 이름 (나비, 루나, 뭉치, 호두, 초코)
    public string personality;         // 성격 (밝음, 신비로움, 귀여움, 차분함, 활발함)
    public string story;               // 구조 사연
    public int affinityLevel;          // 호감도 레벨 (0~5)
    public int affinityPoints;         // 호감도 포인트 (0~100)
    public string favoriteSnack;       // 좋아하는 간식
    public string favoriteActivity;    // 좋아하는 활동
    public Sprite portrait;            // 고양이 일러스트
    public AnimationClip[] animations; // 애니메이션 (걷기, 놀기, 자기)
}
```

### 3. 레벨 데이터 (Level)

```csharp
[System.Serializable]
public class Level
{
    public int levelId;                // 레벨 ID (1~50)
    public int moveLimit;              // 이동 제한 (기본 20~40)
    public LevelGoalType goalType;     // 목표 타입 (블록 제거, 점수, 재료 수집 등)
    public int goalValue;              // 목표값 (예: 블록 50개 제거)
    public int starThresholds[3];      // 별 획득 기준 (1별, 2별, 3별)
    public int baseReward;             // 기본 보상 (동전)
    public List<Obstacle> obstacles;   // 장애물 배치
    public int difficulty;             // 난이도 (1~10)
}

public enum LevelGoalType
{
    RemoveBlocks,      // 블록 제거
    CollectItems,      // 아이템 수집
    ReachScore,        // 점수 달성
    DestroyObstacles   // 장애물 제거
}
```

### 4. 카페 복원 작업 (RestorationTask)

```csharp
[System.Serializable]
public class RestorationTask
{
    public int taskId;                 // 작업 ID (1~20)
    public string taskName;            // 작업명 (예: "마당 빗자루질하기")
    public string description;         // 설명
    public int requiredStars;          // 필요한 별
    public int coinReward;             // 동전 보상
    public int gemReward;              // 보석 보상
    public bool isCompleted;           // 완료 여부
    public Sprite beforeImage;         // 복원 전 이미지
    public Sprite afterImage;          // 복원 후 이미지
}
```

### 5. 경제 시스템 (Economy)

```csharp
[System.Serializable]
public class EconomyConfig
{
    // 기본 보상
    public int coinPerMatch3;          // 매치-3당 동전
    public int coinPerLevel;           // 레벨 클리어 보상
    public int starPerLevel;           // 레벨당 별
    
    // 광고 보상
    public int coinPerAdWatch;         // 광고 시청당 동전
    public int gemPerAdWatch;          // 광고 시청당 보석
    
    // 호감도 보상
    public int coinPerAffinity;        // 호감도 레벨당 동전
    public int gemPerAffinity;         // 호감도 레벨당 보석
    
    // 생명 회복
    public int lifeRecoveryMinutes;    // 생명 회복 시간 (분)
    public int maxLives;               // 최대 생명
    
    // 인앱결제 가격 (USD)
    public float coinPackageSmall;     // 소형 동전 패키지
    public float coinPackageMedium;    // 중형 동전 패키지
    public float coinPackageLarge;     // 대형 동전 패키지
    public float adRemovalPrice;       // 광고 제거 가격
}
```

---

## 🔧 핵심 시스템 인터페이스

### 1. 게임 매니저 (GameManager)

**책임**: 게임 전체 상태 관리, 씬 전환, 게임 루프

```csharp
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    public GameState CurrentState { get; private set; }
    
    public void StartLevel(int levelId) { }
    public void CompleteLevel(int stars) { }
    public void FailLevel() { }
    public void PauseGame() { }
    public void ResumeGame() { }
    public void ReturnToMenu() { }
}

public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    LevelComplete,
    LevelFailed,
    Cafe
}
```

### 2. 보드 시스템 (Board)

**책임**: 8x8 그리드 생성, 타일 관리, 매치 판정

```csharp
public class Board : MonoBehaviour
{
    private Tile[,] tiles = new Tile[8, 8];
    
    public void Initialize() { }
    public void SpawnTiles() { }
    public bool TryMatch(Tile tile1, Tile tile2) { }
    public List<Tile> FindMatches() { }
    public void RemoveMatches(List<Tile> matches) { }
    public void ApplyGravity() { }
    public void FillEmpty() { }
}
```

### 3. 고양이 관리자 (CatManager)

**책임**: 고양이 데이터 관리, 호감도 시스템

```csharp
public class CatManager : MonoBehaviour
{
    public static CatManager Instance { get; private set; }
    
    private Dictionary<int, Cat> cats = new Dictionary<int, Cat>();
    
    public void UnlockCat(int catId) { }
    public void IncreaseCatAffinity(int catId, int points) { }
    public Cat GetCat(int catId) { }
    public List<Cat> GetUnlockedCats() { }
}
```

### 4. 데이터 매니저 (DataManager)

**책임**: 게임 데이터 저장/로드 (PlayerPrefs 또는 JSON)

```csharp
public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }
    
    public void SaveUserProgress(UserProgress progress) { }
    public UserProgress LoadUserProgress() { }
    public void SaveLevelData(int levelId, LevelProgress progress) { }
    public LevelProgress LoadLevelData(int levelId) { }
}
```

---

## 📝 개발 컨벤션

### 네이밍 컨벤션
- **클래스**: PascalCase (예: `GameManager`, `TileSpawner`)
- **메서드**: PascalCase (예: `StartLevel()`, `GetCatAffinity()`)
- **변수**: camelCase (예: `currentLevel`, `coinReward`)
- **상수**: UPPER_SNAKE_CASE (예: `MAX_LIVES`, `BOARD_SIZE`)
- **프리팹**: 접두사 + PascalCase (예: `Tile_Red`, `UI_Button_Play`)

### 코드 구조
- 모든 매니저는 **싱글톤 패턴** 사용
- **의존성 주입** 활용하여 결합도 낮추기
- **이벤트 시스템** 사용하여 느슨한 결합 유지
- **상수는 Constants.cs에 중앙화**

### 주석 규칙
```csharp
/// <summary>
/// 메서드 설명
/// </summary>
/// <param name="paramName">파라미터 설명</param>
/// <returns>반환값 설명</returns>
public void MethodName(int paramName) { }
```

---

## 🎯 Phase 1 완료 기준

✅ **프로젝트 구조 완성**
- 폴더 구조 생성
- 기본 스크립트 틀 작성

✅ **데이터 모델 구현**
- UserProgress 클래스
- Cat 클래스
- Level 클래스
- RestorationTask 클래스
- EconomyConfig 클래스

✅ **핵심 매니저 초기화**
- GameManager 기본 구조
- DataManager 저장/로드 기능
- CatManager 고양이 관리

✅ **Git 저장소 설정**
- .gitignore 설정
- 초기 커밋

---

**다음 단계**: Phase 2 - Match-3 Core Logic Implementation
