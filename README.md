# 냥이의 집 (Whisker Tales) - Unity Project

**게임명**: 냥이의 집 (Whisker Tales)  
**개발사**: Nyang Studio  
**장르**: 캐주얼 게임 (매치-3 퍼즐 + 고양이 수집 + 카페 경영)  
**플랫폼**: Android (Google Play Store)  
**출시 목표**: 2026년 5월 26일  
**엔진**: Unity 2022.3.21f LTS

---

## 📋 프로젝트 개요

'냥이의 집'은 매치-3 퍼즐과 고양이 수집, 한옥 카페 복원 요소를 결합한 감성 모바일 게임입니다.

플레이어는 할머니의 폐업한 한옥 카페를 복원하며, 버려진 고양이들을 구조하고 각 고양이의 특별한 사연을 알아갑니다.

### 핵심 요소
- **매치-3 퍼즐**: 50개 레벨, 다양한 목표 및 난이도
- **고양이 수집**: 5마리 고양이, 각각의 사연과 호감도 시스템
- **카페 복원**: 15~20개 복원 작업으로 한옥 카페 복구
- **경제 시스템**: 동전, 별, 보석을 통한 진행도 관리
- **광고 & 결제**: AdMob 광고 + Google Play Billing

---

## 🎮 게임 플레이 루프

```
매치-3 레벨 클리어
    ↓
동전 & 별 획득
    ↓
카페 복원 작업 완료
    ↓
새로운 고양이 언락
    ↓
고양이와 교감 (호감도 증가)
    ↓
단골 손님 방문 → 추가 보상
    ↓
다음 레벨 도전
```

---

## 📁 프로젝트 구조

```
WhiskerTales/
├── Assets/
│   ├── Scripts/
│   │   ├── Core/              # 핵심 시스템
│   │   │   ├── GameManager.cs
│   │   │   ├── DataManager.cs
│   │   │   └── DataModels.cs
│   │   ├── Puzzle/            # 매치-3 시스템
│   │   ├── Cat/               # 고양이 시스템
│   │   ├── Cafe/              # 카페 복원 시스템
│   │   ├── Economy/           # 경제 시스템
│   │   ├── UI/                # UI 시스템
│   │   ├── Services/          # 외부 서비스
│   │   └── Utilities/         # 유틸리티
│   ├── Scenes/                # 게임 씬
│   ├── Prefabs/               # 프리팹
│   ├── Resources/             # 리소스 (데이터, 오디오, 스프라이트)
│   └── Animations/            # 애니메이션
├── ProjectSettings/           # Unity 프로젝트 설정
└── README.md                  # 이 파일
```

---

## 🛠️ 개발 환경 설정

### 요구사항
- **Unity**: 2022.3.21f LTS 이상
- **C#**: 9.0 이상
- **Android SDK**: API Level 30 이상

### 초기 설정

1. **Unity 프로젝트 열기**
   ```bash
   # Unity Hub에서 프로젝트 열기
   # 또는 커맨드라인에서
   unity -projectPath /path/to/WhiskerTales
   ```

2. **필수 패키지 설치**
   - Unity UI (기본 포함)
   - TextMesh Pro (기본 포함)
   - Google Play Services
   - Google Mobile Ads SDK

3. **Git 저장소 초기화**
   ```bash
   cd WhiskerTales
   git init
   git add .
   git commit -m "Initial commit: Project structure and core systems"
   ```

---

## 📝 코딩 컨벤션

### 네이밍
- **클래스**: `PascalCase` (예: `GameManager`, `TileSpawner`)
- **메서드**: `PascalCase` (예: `StartLevel()`)
- **변수**: `camelCase` (예: `currentLevel`, `coinReward`)
- **상수**: `UPPER_SNAKE_CASE` (예: `MAX_LIVES`)

### 구조
- 모든 매니저는 **싱글톤 패턴** 사용
- **네임스페이스** 활용: `WhiskerTales.Core`, `WhiskerTales.Puzzle` 등
- **이벤트 시스템** 사용하여 느슨한 결합 유지

### 주석
```csharp
/// <summary>
/// 메서드 설명
/// </summary>
/// <param name="paramName">파라미터 설명</param>
/// <returns>반환값 설명</returns>
public void MethodName(int paramName) { }
```

---

## 🎯 개발 일정

| 주차 | 기간 | 주요 작업 | 상태 |
|------|------|---------|------|
| **1주** | 5/4~5/10 | 프로젝트 구조, 데이터 모델, 매치-3 기본 로직 | 🔄 진행 중 |
| **2주** | 5/11~5/17 | 호감도, 카페 복원, 단골 손님 시스템 | ⏳ 예정 |
| **3주** | 5/18~5/24 | 애니메이션, 사운드, Google 로그인, 50개 레벨 | ⏳ 예정 |
| **4주** | 5/25~5/26 | 마케팅 자료, 최종 테스트, Google Play 제출 | ⏳ 예정 |

---

## 🔧 핵심 시스템

### GameManager
- 게임 전체 상태 관리
- 씬 전환 및 게임 루프 제어
- 싱글톤 패턴

### DataManager
- 게임 데이터 저장/로드 (JSON)
- PlayerPrefs 대신 파일 시스템 사용
- 자동 저장 기능

### CatManager
- 고양이 데이터 관리
- 호감도 시스템
- 고양이 언락 및 상호작용

### Board (매치-3)
- 8x8 그리드 관리
- 매치 판정 알고리즘
- 타일 생성 및 제거

---

## 📊 데이터 모델

### UserProgress
```csharp
public class UserProgress
{
    public string userId;           // Google Play ID
    public int currentLevel;        // 현재 레벨
    public int coins;               // 동전
    public int stars;               // 별
    public int gems;                // 보석
    public List<int> unlockedCats;  // 언락된 고양이
    // ... 기타 필드
}
```

### Cat
```csharp
public class Cat
{
    public int catId;               // 고양이 ID
    public string name;             // 이름
    public int affinityLevel;       // 호감도 레벨
    public string story;            // 구조 사연
    // ... 기타 필드
}
```

---

## 🎨 에셋 관리

### 스프라이트
- **경로**: `Assets/Resources/Sprites/`
- **타일**: `Tiles/` 폴더
- **고양이**: `Cats/` 폴더
- **UI**: `UI/` 폴더

### 애니메이션
- **경로**: `Assets/Animations/`
- **고양이 애니메이션**: `Cat/` 폴더
- **타일 애니메이션**: `Tile/` 폴더

### 오디오
- **경로**: `Assets/Resources/Audio/`
- **배경음**: `BGM/` 폴더
- **효과음**: `SFX/` 폴더

---

## 🧪 테스트

### 로컬 테스트
1. Unity Editor에서 Play 버튼 클릭
2. 메인 메뉴 씬 (`MainMenu.unity`) 시작

### 디바이스 테스트
1. Android 디바이스 연결
2. `File > Build Settings` → Android 선택
3. `Build and Run` 클릭

### 디버그
- `Debug.Log()` 사용
- Unity Console 확인
- Logcat (Android) 확인

---

## 📱 Google Play 연동

### 필수 설정
1. **Google Play Console** 계정 생성
2. **Google Play Services** 설정
3. **Google Mobile Ads** 설정
4. **Google Play Billing** 설정

### 빌드 설정
- **Player Settings** → Android
- **Package Name**: `com.nyang.whiskertaless` (예시)
- **Version Code**: 1
- **Version Name**: 1.0.0

---

## 🚀 빌드 및 배포

### Android APK 빌드
```
File > Build Settings > Android > Build
```

### Android AAB 빌드 (Google Play 제출용)
```
File > Build Settings > Android > Build App Bundle
```

### GameCI를 사용한 자동 빌드
```bash
# GitHub Actions를 통한 자동 빌드 설정
# .github/workflows/build.yml 참조
```

---

## 📞 개발팀

- **Manus AI**: 기술 개발, 코드 작성, 에셋 생성
- **사용자**: 의사결정, 피드백, 테스트

---

## 📝 변경 이력

| 날짜 | 버전 | 변경사항 |
|------|------|---------|
| 2026-05-04 | 1.0.0 | 초기 프로젝트 구조 및 핵심 시스템 구현 |

---

## 📌 중요 노트

- **스코프 관리**: MVP 스코프 엄격히 준수
- **데이터 안정성**: 정기적인 저장 및 백업
- **성능**: 모바일 최적화 우선
- **사용자 경험**: 직관적인 UI/UX 설계

---

**이 프로젝트는 Manus와 사용자가 함께 만드는 '냥이의 집'입니다.**

**마지막 업데이트**: 2026년 5월 4일
