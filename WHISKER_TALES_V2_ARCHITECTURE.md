# Whisker Tales V2 Runtime Architecture
**작성: 채과장 (ChatGPT) + 노부장 (Claude)**
**날짜: 2026-05-11**

---

## 핵심 결정

> "망가진 런타임 껍데기를 버리고, 살아있는 게임 코어를 새 집으로 이사시키는 단계"

- **Architecture:** Persistent Boot + Single Main_App scene
- **UI:** Prefab-based screen panels (코드 생성 X)
- **Navigation:** ScreenNavigator + BaseScreen + PopupManager
- **Android:** AndroidRuntimeGuard + SafeAreaController + single EventSystem
- **Migration:** AppBootstrap freeze, 기존 Match-3/Save/Audio는 Adapter로 연결

---

## 버리는 것 vs 살리는 것

### 버리는 것 (AppBootstrap의 나쁜 부분)
- 절차형 UI 생성 코드 (런타임 Button/Image 생성)
- reflection InjectField
- 임시 색상/sprite 생성
- 화면 구조 코드 생성

### 살리는 것 (기존 코어)
- 매치3 로직 (BoardView, TileView, GameService)
- 레벨 JSON 데이터 (schemaVersion:1)
- 고양이 설정 및 수집 시스템
- SaveService
- AudioService
- GameConstants, DebugLogger, LogCategory
- TileType enum (tile_fish/milk/yarn/catnip/pawprint/fishbone)

---

## Scene 구조

```
Build Settings:
0. Boot_Persistent
1. Main_App
```

### Boot_Persistent
```
Boot_Persistent
├─ RuntimeRoot
│   ├─ GameRuntime
│   ├─ ServiceRegistry
│   ├─ SaveService
│   ├─ AudioService
│   ├─ AssetProvider
│   ├─ AndroidRuntimeGuard
│   └─ SceneLoader
├─ EventSystem
├─ MainCamera
└─ GlobalOverlayCanvas
```

### Main_App
```
Main_App
├─ UI_Root
│   ├─ Canvas_Main (Sorting: 0)
│   │   ├─ SafeAreaRoot
│   │   │   ├─ SplashScreen
│   │   │   ├─ LoadingScreen
│   │   │   ├─ HomeScreen
│   │   │   ├─ LevelSelectScreen
│   │   │   ├─ GameplayScreen
│   │   │   ├─ LevelClearScreen
│   │   │   ├─ GameFailScreen
│   │   │   ├─ CatRoomScreen
│   │   │   └─ CafeScreen
│   │   └─ ToastLayer
│   ├─ Canvas_Popup (Sorting: 100)
│   ├─ Canvas_Transition (Sorting: 500)
│   └─ Canvas_SystemOverlay (Sorting: 1000)
├─ GameplayRoot
│   ├─ BoardRoot
│   ├─ TileRoot
│   └─ EffectRoot
└─ WorldRoot
```

---

## Canvas 설정

| Canvas | Render Mode | Sorting Order | GraphicRaycaster |
|--------|-------------|---------------|-----------------|
| Canvas_Main | Screen Space Overlay | 0 | Yes |
| Canvas_Popup | Screen Space Overlay | 100 | Yes |
| Canvas_Transition | Screen Space Overlay | 500 | Yes |
| Canvas_SystemOverlay | Screen Space Overlay | 1000 | No |

**Canvas Scaler (Main):**
- UI Scale Mode: Scale With Screen Size
- Reference Resolution: 1080 x 2400
- Match: 0.5

---

## 화면 전환 흐름

```
App Launch
→ Boot_Persistent (services init)
→ Main_App load
→ SplashScreen (로고 + 고양이 애니)
→ LoadingScreen (asset preload)
→ HomeScreen ←→ LevelSelect ←→ Gameplay ←→ LevelClear/Fail
             ←→ CatRoom
             ←→ Cafe
```

### ScreenId
```csharp
namespace WhiskerTales.UI
{
    public enum ScreenId
    {
        Splash,
        Loading,
        Home,
        LevelSelect,
        Gameplay,
        LevelClear,
        GameFail,
        CatRoom,
        Cafe,
        Settings,
        DetoxModal,
        SleepMode
    }
}
```

### Android Back Button 규칙
- Gameplay 중 Back → PausePopup
- Popup 열림 → Popup 닫기
- Home에서 Back → ExitConfirm
- 그 외 → 이전 화면

---

## Boot Sequence (정확한 순서)

```
1. Boot_Persistent 로드
2. GameRuntime.Awake()
3. 중복 RuntimeRoot 제거
4. Application.targetFrameRate 설정
5. AndroidRuntimeGuard 적용
6. EventSystem 검증 (count == 1 확인)
7. ServiceRegistry 생성
8. SaveService 초기화
9. AudioService 초기화
10. AssetProvider 초기화
11. TMP Settings 검증
12. Main_App 씬 로드
13. LoadingScreen 표시
14. 필수 prefab/sprite/audio preload
15. 첫 세이브 데이터 로드
16. SplashScreen 재생
17. HomeScreen 진입
18. 첫 interactive frame unlock
```

---

## 파일/폴더 구조

```
Assets/WhiskerTales/
├─ _Project/
│   ├─ Scenes/
│   │   ├─ Boot_Persistent.unity
│   │   └─ Main_App.unity
│   │
│   ├─ Prefabs/
│   │   ├─ Runtime/
│   │   │   ├─ RuntimeRoot.prefab
│   │   │   ├─ EventSystem.prefab
│   │   │   └─ MainCamera.prefab
│   │   │
│   │   ├─ UI/
│   │   │   ├─ Screens/
│   │   │   │   ├─ SplashScreen.prefab
│   │   │   │   ├─ LoadingScreen.prefab
│   │   │   │   ├─ HomeScreen.prefab
│   │   │   │   ├─ LevelSelectScreen.prefab
│   │   │   │   ├─ GameplayScreen.prefab
│   │   │   │   ├─ LevelClearScreen.prefab
│   │   │   │   ├─ GameFailScreen.prefab
│   │   │   │   ├─ CatRoomScreen.prefab
│   │   │   │   └─ CafeScreen.prefab
│   │   │   │
│   │   │   ├─ Popups/
│   │   │   │   ├─ SettingsPopup.prefab
│   │   │   │   ├─ ExitConfirmPopup.prefab
│   │   │   │   └─ PausePopup.prefab
│   │   │   │
│   │   │   └─ Common/
│   │   │       ├─ WTButton.prefab
│   │   │       ├─ CatMessageBubble.prefab
│   │   │       └─ SoftPanel.prefab
│   │   │
│   │   └─ Gameplay/
│   │       ├─ BoardRoot.prefab
│   │       ├─ TileView.prefab
│   │       └─ EffectsRoot.prefab
│   │
│   └─ Scripts/
│       ├─ Runtime/
│       │   ├─ GameRuntime.cs
│       │   ├─ ServiceRegistry.cs
│       │   ├─ RuntimeBootstrapper.cs
│       │   └─ AppLifecycleController.cs
│       │
│       ├─ Platform/
│       │   ├─ AndroidRuntimeGuard.cs
│       │   ├─ AndroidSystemBars.cs
│       │   ├─ SafeAreaController.cs
│       │   └─ TouchInputGuard.cs
│       │
│       ├─ UI/
│       │   ├─ ScreenId.cs
│       │   ├─ BaseScreen.cs
│       │   ├─ ScreenNavigator.cs
│       │   ├─ ScreenTransitionController.cs
│       │   ├─ BackButtonController.cs
│       │   ├─ PopupManager.cs
│       │   └─ Screens/
│       │       ├─ SplashScreenController.cs
│       │       ├─ LoadingScreenController.cs
│       │       ├─ HomeScreenController.cs
│       │       ├─ LevelSelectScreenController.cs
│       │       ├─ GameplayScreenController.cs
│       │       ├─ LevelClearScreenController.cs
│       │       ├─ GameFailScreenController.cs
│       │       ├─ CatRoomScreenController.cs
│       │       └─ CafeScreenController.cs
│       │
│       ├─ Flow/
│       │   ├─ GameFlowController.cs
│       │   ├─ LevelFlowController.cs
│       │   └─ SessionController.cs
│       │
│       ├─ Integration/
│       │   ├─ Match3RuntimeAdapter.cs
│       │   ├─ BoardViewInstaller.cs
│       │   ├─ LevelDataProvider.cs
│       │   └─ LegacyServiceBridge.cs
│       │
│       └─ Editor/
│           ├─ WhiskerBuildValidator.cs
│           ├─ UIReferenceValidator.cs
│           └─ AndroidBuildValidator.cs
```

---

## 노실장 구현 순서

### Phase V2-0: Safety Branch
```
git checkout -b runtime-v2
```
- AppBootstrap.cs freeze 확인
- 기존 빌드 백업
- 기존 씬 절대 삭제 금지

### Phase V2-1: Boot_Persistent 생성
**만들 파일:**
- GameRuntime.cs
- ServiceRegistry.cs
- RuntimeBootstrapper.cs
- AndroidRuntimeGuard.cs
- SafeAreaController.cs
- TouchInputGuard.cs

**성공 기준:**
- Galaxy S24 Ultra에서 첫 화면 깨짐 없음
- 터치 입력 로그 정상
- EventSystem 하나만 존재

### Phase V2-2: Main_App 씬 생성
**만들 것:**
- Main_App.unity
- Canvas_Main, Canvas_Popup, Canvas_Transition, Canvas_SystemOverlay
- SafeAreaRoot

**성공 기준:**
- Splash → Loading → Home 전환
- Android 탭 정상
- Back 버튼 정상

### Phase V2-3: ScreenNavigator 구현
**만들 파일:**
- ScreenId.cs
- BaseScreen.cs
- ScreenNavigator.cs
- ScreenTransitionController.cs
- BackButtonController.cs
- PopupManager.cs

### Phase V2-4: 기존 서비스 연결
**살릴 것:** SaveService, AudioService, GameConstants, Level JSON

### Phase V2-5: Gameplay Integration
**살릴 것:** BoardView, TileView, GameService, Match3Core
**새로 만들 것:** Match3RuntimeAdapter.cs, GameplayScreenController.cs

### Phase V2-6: CatRoom / Cafe 연결

### Phase V2-7: 감성 폴리싱
- Soft fade transition
- Cat idle breathing
- Lantern flicker
- Gentle haptic
- Soft click sounds

---

## 절대 원칙 (V1에서 이어받기)

- 타일 키: `tile_fish / tile_milk / tile_yarn / tile_catnip / tile_pawprint / tile_fishbone`
- 모든 if/else brace {} 필수
- namespace `WhiskerTales.[Layer]`
- 하드코딩 금지 (GameConstants 사용)
- `DebugLogger.Info(LogCategory.X, ...)`

---

## 주의사항 (Pitfalls)

1. **Additive Scene 너무 빨리 도입 금지** — v2.0은 단일 Main_App으로
2. **EventSystem 중복 금지** — 부팅 시 count == 1 검사
3. **투명 Image alpha 0 금지** — 터치 영역은 alpha 0.01
4. **SafeArea 무시 금지** — S24 Ultra 상단 UI 주의
5. **AppBootstrap 일부 재활용 금지** — 참고만, 코드 복붙 X
6. **구조 정리하다 감성 잃기 금지** — V2 목적은 "폰에서 따뜻하게 작동하는 게임"

---

## Android 특이사항

- Status Bar: 초기엔 투명 시도 X (마젠타 원인)
- Navigation Bar: solid dark/warm color
- Touch: StandaloneInputModule 고정
- Texture: ASTC 우선 (UI 4x4, BG 8x8, Cat 6x6)
- TMP: Settings에 NotoSansKR SDF 고정 등록 (런타임 주입 X)
