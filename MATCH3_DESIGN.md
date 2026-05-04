# 냥이의 집 - Match-3 핵심 로직 설계

**작성일**: 2026년 5월 4일  
**목표**: 매치-3 퍼즐 시스템의 핵심 로직 설계 및 알고리즘 정의

---

## 📋 개요

매치-3 퍼즐은 게임의 가장 핵심적인 부분입니다. 이 문서는 구현 전에 모든 로직을 명확히 정의합니다.

### 핵심 특징
- **8x8 그리드** (Royal Match 기준)
- **6가지 기본 타일** (색상)
- **4가지 특수 아이템** (로켓, 폭탄, 무지개, 망치)
- **4가지 장애물** (상자, 얼음, 자물쇠, 사슬)
- **다양한 레벨 목표** (블록 제거, 점수, 재료 수집, 장애물 제거)

---

## 🎮 게임 플로우

```
[레벨 시작]
    ↓
[보드 초기화 (8x8 그리드)]
    ↓
[플레이어 입력 (타일 스왑)]
    ↓
[매치 판정]
    ├─ 매치 없음 → 스왑 취소
    └─ 매치 있음 → [타일 제거] → [중력 적용] → [새 타일 생성]
    ↓
[특수 아이템 생성 체크]
    ├─ 4개 직선 → 로켓 생성
    ├─ L/T 형태 5개 → 폭탄 생성
    └─ 직선 5개 → 무지개 생성
    ↓
[레벨 목표 업데이트]
    ├─ 블록 제거 카운트
    ├─ 점수 계산
    └─ 재료 수집 카운트
    ↓
[레벨 완료 체크]
    ├─ 목표 달성 → [레벨 완료]
    ├─ 이동 제한 초과 → [레벨 실패]
    └─ 계속 진행 → [플레이어 입력 대기]
```

---

## 🔧 핵심 클래스 설계

### 1. Board 클래스

**책임**: 8x8 그리드 관리, 타일 생성/제거, 매치 판정

```csharp
public class Board : MonoBehaviour
{
    private Tile[,] tiles = new Tile[8, 8];
    private const int BOARD_SIZE = 8;
    
    // 초기화
    public void Initialize() { }
    public void SpawnTiles() { }
    
    // 매치 판정
    public bool TrySwapTiles(Tile tile1, Tile tile2) { }
    public List<Tile> FindMatches() { }
    public List<Tile> FindMatchesAt(int x, int y) { }
    
    // 타일 제거 및 낙하
    public void RemoveMatches(List<Tile> matches) { }
    public void ApplyGravity() { }
    public void FillEmpty() { }
    
    // 특수 아이템
    public void CreateSpecialItem(Tile tile, SpecialItemType type) { }
    public void ActivateSpecialItem(Tile tile) { }
    
    // 유틸리티
    public Tile GetTile(int x, int y) { }
    public bool IsValidPosition(int x, int y) { }
    public List<Tile> GetAdjacentTiles(int x, int y) { }
}
```

### 2. Tile 클래스

**책임**: 개별 타일 데이터 및 상태 관리

```csharp
public class Tile : MonoBehaviour
{
    public int x, y;                          // 그리드 위치
    public TileType type;                     // 타일 타입 (색상)
    public SpecialItemType specialItem;       // 특수 아이템
    public ObstacleType obstacle;             // 장애물
    public int obstacleHealth;                // 장애물 내구도
    
    // 상태
    public bool isMatched;                    // 매치됨
    public bool isMoving;                     // 이동 중
    public bool isLocked;                     // 잠김
    
    // 메서드
    public void SetType(TileType newType) { }
    public void SetSpecialItem(SpecialItemType item) { }
    public void AddObstacle(ObstacleType obs, int health) { }
    public void RemoveObstacle() { }
    public void DamageObstacle() { }
    public void MoveTo(int newX, int newY) { }
    public void AnimateRemoval() { }
}
```

### 3. MatchLogic 클래스

**책임**: 매치 판정 알고리즘

```csharp
public static class MatchLogic
{
    // 매치 찾기
    public static List<Tile> FindAllMatches(Tile[,] board) { }
    public static List<Tile> FindMatchesInRow(Tile[,] board, int row) { }
    public static List<Tile> FindMatchesInColumn(Tile[,] board, int col) { }
    
    // 특수 아이템 생성 조건 확인
    public static SpecialItemType GetSpecialItemType(List<Tile> matches) { }
    
    // 스왑 가능 여부 확인
    public static bool IsValidSwap(Tile tile1, Tile tile2) { }
    public static bool WillCreateMatch(Tile tile1, Tile tile2) { }
}
```

### 4. SpecialItem 클래스

**책임**: 특수 아이템 로직 (로켓, 폭탄, 무지개, 망치)

```csharp
public class SpecialItem
{
    // 로켓: 한 줄 제거 (가로 또는 세로)
    public static void ActivateRocket(Tile tile, Board board) { }
    
    // 폭탄: 3x3 영역 제거
    public static void ActivateBomb(Tile tile, Board board) { }
    
    // 무지개: 같은 색 모두 제거
    public static void ActivateRainbow(Tile tile, TileType targetColor, Board board) { }
    
    // 망치: 임의의 타일 1개 제거 (플레이어 선택)
    public static void ActivateHammer(Tile tile, Board board) { }
}
```

### 5. LevelGoal 클래스

**책임**: 레벨 목표 관리 및 진행도 추적

```csharp
public class LevelGoal : MonoBehaviour
{
    public LevelGoalType goalType;
    public int goalValue;                     // 목표값
    public int currentProgress;               // 현재 진행도
    public int moveLimit;                     // 이동 제한
    public int movesUsed;                     // 사용한 이동
    
    public void Initialize(Level levelData) { }
    public void UpdateProgress(List<Tile> removedTiles) { }
    public bool IsGoalAchieved() { }
    public bool IsMovesExceeded() { }
    public float GetProgressPercentage() { }
}
```

---

## 🎯 매치 판정 알고리즘

### 알고리즘 1: 기본 매치 찾기

```
FOR EACH 타일 (x, y):
    IF 타일이 이미 확인됨:
        CONTINUE
    
    // 가로 매치 확인
    count = 1
    FOR i = x+1 TO 7:
        IF board[i][y].type == board[x][y].type:
            count++
        ELSE:
            BREAK
    IF count >= 3:
        ADD 모든 매치된 타일 TO matchList
        MARK 타일들을 확인됨으로 표시
    
    // 세로 매치 확인 (동일한 로직)
```

### 알고리즘 2: 특수 아이템 생성

```
matchCount = 매치된 타일 개수
matchShape = 매치의 형태 (직선, L, T 등)

IF matchCount == 4 AND matchShape == 직선:
    CREATE 로켓
ELSE IF matchCount == 5 AND matchShape == (L 또는 T):
    CREATE 폭탄
ELSE IF matchCount == 5 AND matchShape == 직선:
    CREATE 무지개
```

### 알고리즘 3: 중력 적용

```
FOR EACH 열 (x):
    emptyCount = 0
    FOR y = 7 DOWN TO 0:
        IF board[x][y] == NULL:
            emptyCount++
        ELSE IF emptyCount > 0:
            MOVE board[x][y] DOWN BY emptyCount
            board[x][y + emptyCount] = board[x][y]
            board[x][y] = NULL
```

### 알고리즘 4: 새 타일 생성

```
FOR EACH 열 (x):
    FOR y = 0 TO 7:
        IF board[x][y] == NULL:
            CREATE 새 타일
            SET 타입 = RANDOM 색상
            PLACE AT board[x][y]
            ANIMATE 타일 낙하
```

---

## 📊 특수 아이템 상호작용

### 로켓 (Rocket)
- **생성 조건**: 4개 직선 매치
- **효과**: 한 줄 제거 (생성된 방향)
- **다른 특수 아이템과 상호작용**:
  - 로켓 + 로켓 = 두 줄 제거
  - 로켓 + 폭탄 = 한 줄 + 3x3 제거
  - 로켓 + 무지개 = 한 줄 + 같은 색 모두 제거

### 폭탄 (Bomb)
- **생성 조건**: 5개 L/T 형태 매치
- **효과**: 3x3 영역 제거
- **다른 특수 아이템과 상호작용**:
  - 폭탄 + 폭탄 = 두 개의 3x3 제거
  - 폭탄 + 무지개 = 3x3 + 같은 색 모두 제거

### 무지개 (Rainbow)
- **생성 조건**: 5개 직선 매치
- **효과**: 같은 색 모두 제거 (플레이어가 색 선택)
- **다른 특수 아이템과 상호작용**:
  - 무지개 + 무지개 = 모든 타일 제거

### 망치 (Hammer)
- **획득 방법**: 광고 시청 또는 구매
- **효과**: 임의의 타일 1개 제거
- **사용 제한**: 레벨당 최대 3개

---

## 🎨 애니메이션 및 이펙트

### 타일 제거 애니메이션
- **지속 시간**: 0.3초
- **이펙트**: 스케일 축소 + 페이드 아웃 + 파티클

### 타일 낙하 애니메이션
- **지속 시간**: 0.1초 × 낙하 칸 수
- **이펙트**: 부드러운 이동 + 회전

### 특수 아이템 활성화
- **로켓**: 회전 + 이동 + 폭발 이펙트
- **폭탄**: 팽창 + 폭발 이펙트
- **무지개**: 반짝임 + 색상 변화

---

## 🧪 테스트 케이스

### 기본 매치
- [ ] 3개 가로 매치
- [ ] 3개 세로 매치
- [ ] 4개 직선 매치 (로켓 생성)
- [ ] 5개 L 형태 매치 (폭탄 생성)
- [ ] 5개 직선 매치 (무지개 생성)

### 특수 아이템 상호작용
- [ ] 로켓 + 로켓
- [ ] 폭탄 + 폭탄
- [ ] 로켓 + 폭탄
- [ ] 무지개 + 모든 타일

### 장애물
- [ ] 상자 제거
- [ ] 얼음 녹이기
- [ ] 자물쇠 풀기
- [ ] 사슬 끊기

### 엣지 케이스
- [ ] 연쇄 매치 (cascade)
- [ ] 여러 매치 동시 발생
- [ ] 보드 전체 매치 불가능 상황
- [ ] 이동 제한 초과

---

## 📈 성능 최적화

### 알고리즘 최적화
- **매치 찾기**: O(n²) → 필요시 O(n) 최적화
- **중력 적용**: 한 번에 모든 열 처리
- **타일 풀링**: 타일 생성/제거 대신 재사용

### 메모리 최적화
- **타일 캐싱**: 자주 사용되는 타일 미리 생성
- **이벤트 시스템**: 콜백 대신 이벤트 사용
- **가비지 컬렉션**: 불필요한 객체 생성 최소화

---

## 🔄 다음 단계

1. **Board.cs** - 보드 관리 및 타일 생성
2. **Tile.cs** - 개별 타일 데이터 및 애니메이션
3. **MatchLogic.cs** - 매치 판정 알고리즘
4. **SpecialItem.cs** - 특수 아이템 로직
5. **LevelGoal.cs** - 레벨 목표 관리
6. **기본 UI** - 게임 화면 UI

---

**이 설계 문서는 구현 중 필요시 업데이트됩니다.**
