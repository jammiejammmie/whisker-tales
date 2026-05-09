# Unity Inspector 와이어링 가이드 — Phase A-1 타이틀/메인 화면

> 대상 씬: `Assets/Scenes/MainScenes.unity`
> 적용 시점: Phase A-1 코드 머지 후, Unity Editor 작업
> 단일 씬 + 패널 전환 구조

---

## 1. 매니저 GameObject 계층

씬 루트에 `[Managers]` 빈 GameObject 하나를 만들고 그 아래에 자식들을 둡니다. (DontDestroyOnLoad가 적용되므로 어디에 두든 동작하지만, 가독성 위해 그룹화)

```
[Managers]
├── GameManager         (Component: GameManager — Core/GameManager.cs)
├── DataManager         (GameManager가 자동 추가하므로 별도 작업 불필요)
├── HeartSystem         (Component: HeartSystem — Core/HeartSystem.cs)
├── AudioManager        (Component: AudioManager + Inspector에서 BGM/SFX 클립 할당)
├── CafeRestorationManager  (Component + 배경 15장 할당, 아래 §3 참조)
├── CatManager          (Component: CatManager — Cat/CatManager.cs)
├── I18nManager         (자동 생성)
└── IdleRewardSystem    (자동 생성)
```

각 매니저는 싱글톤이므로 씬당 1개. `GameInitializer`는 별도로 두지 않거나, 필요한 자동 생성 매니저(I18nManager 등)만 남깁니다.

---

## 2. UI Canvas 계층

씬 루트에 `Canvas` (Render Mode: Screen Space - Overlay, CanvasScaler: 1080x1920 reference)를 만들고 아래 구조를 구성합니다.

```
Canvas
├── TitlePanel              (GameObject 활성화/비활성화로 화면 전환)
│   ├── Background          (Image — 동적 배경)
│   ├── TopHUD              (RectTransform, 상단 anchor)
│   │   ├── LivesGroup
│   │   │   ├── HeartIcon (Image)
│   │   │   ├── LivesText (TMP_Text — "5" 또는 "FULL")
│   │   │   ├── LivesTimerText (TMP_Text — "29:30")
│   │   │   └── PlusButton (Button)
│   │   ├── AnchoviesGroup
│   │   │   ├── AnchovyIcon (Image)
│   │   │   ├── AnchoviesText (TMP_Text — "12,450")
│   │   │   └── PlusButton (Button)
│   │   ├── StarsGroup
│   │   │   ├── StarIcon (Image)
│   │   │   └── StarsText (TMP_Text — "215")
│   │   └── SettingsButton (Button)
│   ├── EventBanner          (우상단 — 최초 비활성, TitleUI가 활성화)
│   │   └── CountdownText (TMP_Text — "3d 12h")
│   ├── LogoText             (TMP_Text — "위스커 테일즈")
│   ├── DailyCopyText        (TMP_Text — 오늘의 빛깔 카피)
│   └── PlayArea             (매치-3 그리드 영역, Phase A-3에서 채움)
│
├── ShopPanel                (비활성, "Coming Soon" stub)
├── CatRoomPanel             (비활성, Phase A-2에서 CatBondScreen 배치)
├── GalleryPanel             (비활성, stub)
├── FriendsPanel             (비활성, stub)
├── SettingsPanel            (비활성)
│
└── BottomNav                (하단 anchor)
    ├── ShopButton (Button + Icon Image)
    ├── CatRoomButton
    ├── HomeButton           (default tab — TitlePanel 표시)
    ├── GalleryButton
    └── FriendsButton
```

> **주의:** `BottomNav`는 모든 탭 패널 위에 떠 있어야 함 (Hierarchy에서 패널보다 아래쪽 = 그려질 때 위쪽).

---

## 3. 컴포넌트별 Inspector 연결

### 3.1 CafeRestorationManager

`zoneBackgrounds` 필드에 zone × stage = 15장 할당:

| Zone | Stage 1 | Stage 2 | Stage 3 | Stage 4 | Stage 5 |
|---|---|---|---|---|---|
| Element 0 (Zone 1) | bg_zone1_stage1 | bg_zone1_stage2 | bg_zone1_stage3 | bg_zone1_stage4 | bg_zone1_stage5 |
| Element 1 (Zone 2) | bg_zone2_stage1 | bg_zone2_stage2 | bg_zone2_stage3 | bg_zone2_stage4 | bg_zone2_stage5 |
| Element 2 (Zone 3) | bg_zone3_stage1 | bg_zone3_stage2 | bg_zone3_stage3 | bg_zone3_stage4 | bg_zone3_stage5 |

PNG 파일은 `Assets/Sprites/Backgrounds/`에서 드래그 (Texture Type이 `Sprite (2D and UI)`인지 확인).

`progressBar`, `stageDescriptionText`, `starsRequiredText`, `cafeBackgroundImage`는 카페 복원 화면(Phase A-4)에서 연결. Phase A-1 단계에서는 비워둬도 OK.

### 3.2 TitleUI (TitlePanel에 부착)

| 필드 | 연결 대상 |
|---|---|
| **Top HUD** | |
| livesText | TitlePanel/TopHUD/LivesGroup/LivesText |
| livesTimerText | TitlePanel/TopHUD/LivesGroup/LivesTimerText |
| livesPlusButton | TitlePanel/TopHUD/LivesGroup/PlusButton |
| anchoviesText | TitlePanel/TopHUD/AnchoviesGroup/AnchoviesText |
| anchoviesPlusButton | TitlePanel/TopHUD/AnchoviesGroup/PlusButton |
| starsText | TitlePanel/TopHUD/StarsGroup/StarsText |
| settingsButton | TitlePanel/TopHUD/SettingsButton |
| **Center** | |
| backgroundImage | TitlePanel/Background |
| logoText | TitlePanel/LogoText |
| dailyCopyText | TitlePanel/DailyCopyText |
| **Event Banner** | |
| eventBannerRoot | TitlePanel/EventBanner |
| eventCountdownText | TitlePanel/EventBanner/CountdownText |
| eventDurationDays | 3 (기본값) |
| eventDurationHours | 12 (기본값) |
| **Panels** | |
| titlePanel | TitlePanel (자기 자신) |
| settingsPanel | Canvas/SettingsPanel |

### 3.3 BottomNav (BottomNav GameObject에 부착)

`tabs` 배열 5개 element:

| Element | tab | button | panel | icon |
|---|---|---|---|---|
| 0 | Shop | ShopButton | Canvas/ShopPanel | ShopButton/Icon (Image) |
| 1 | CatRoom | CatRoomButton | Canvas/CatRoomPanel | CatRoomButton/Icon |
| 2 | Home | HomeButton | Canvas/TitlePanel | HomeButton/Icon |
| 3 | Gallery | GalleryButton | Canvas/GalleryPanel | GalleryButton/Icon |
| 4 | Friends | FriendsButton | Canvas/FriendsPanel | FriendsButton/Icon |

`defaultTab` = `Home`
`activeColor` / `inactiveColor` 는 디폴트값 사용 (#E8A87C 코랄 / #8B7355 60% 알파).

---

## 4. 폰트 / 컬러 설정 (인계 패킷 §5)

### TextMeshPro 폰트 에셋

`Assets/Fonts/` 폴더에 폰트 파일을 배치 후 `Window > TextMeshPro > Font Asset Creator`로 SDF 에셋 생성:

- **헤딩** (LogoText): Noto Serif KR Bold
- **본문** (HUD, 카피): Noto Sans KR Regular

폰트 라이선스: Google Fonts (OFL).

### 컬러 팔레트 (Image / Text color)

| 용도 | Hex | 비고 |
|---|---|---|
| Primary | #8B7355 | 나무톤 |
| Background tint | #F5F1E8 | 한지 크림 |
| Accent | #E8A87C | 따뜻한 코랄 (BottomNav active) |
| Dark | #2C2C2C | 차콜 |
| 벨라 핑크 | #F4A7B9 | 교감 화면 Pet 버튼 |
| 나비 초록 | #7CB87C | Play 버튼 |
| 구름이 파랑 | #7BA7BC | 보조 액센트 |
| 방울 금색 | #D4A847 | 별/보상 |

DailyCopyText의 색은 `DailyCopy.GetToday()` 가 자동으로 설정 (날짜 시드 기반).

---

## 5. 검증 체크리스트

- [ ] Play 버튼 누르면 콘솔에 `[GameManager] Game initialized`, `[HeartSystem] ...` 로그 출력
- [ ] TopHUD에 멸치 100, 별 0, 하트 5 (FULL) 표시
- [ ] Background 이미지가 zone1_stage1로 표시
- [ ] DailyCopyText에 한국어/영어 카피 표시 (시스템 언어에 따라)
- [ ] EventBanner에 카운트다운 표시 (3d 12h부터)
- [ ] BottomNav에서 Home 탭이 활성 색상 (코랄)
- [ ] Shop 탭 누르면 TitlePanel 사라지고 ShopPanel 활성화
- [ ] 다시 Home 누르면 TitlePanel 복귀

---

## 6. 알려진 미구현 / 후속 작업

| 항목 | Phase |
|---|---|
| ShopPanel / GalleryPanel / FriendsPanel 내용 | Phase B+ |
| LivesPlusButton (광고/IAP) 연결 | IAP/AdMob 통합 시 |
| AnchoviesPlusButton (상점) 연결 | Phase B Shop |
| SettingsPanel 내용 | Phase A-9 |
| PlayArea 매치-3 그리드 | Phase A-3 |
| CatRoomPanel 안에 CatBondScreen 배치 | Phase A-2 |
| CafeUI.cs의 Resources.Load(portraitPath) → Inspector 할당 | Phase A-2 또는 A-7 |

---

## 7. 자주 발생하는 문제

**Q. TitleUI가 NullReference로 에러를 뱉음**
→ Inspector 필드 일부가 비어 있음. §3.2 표대로 모두 연결했는지 확인. 비워둬도 되는 필드(eventBannerRoot 등)는 null 체크가 들어있지만, livesText/anchoviesText 같이 표시 필수인 필드는 채워야 함.

**Q. 배경이 안 바뀜**
→ CafeRestorationManager.zoneBackgrounds에 PNG 할당 여부, 그리고 PNG의 Texture Type이 `Sprite (2D and UI)` 인지 확인.

**Q. BottomNav 버튼 누르면 모든 패널이 사라짐**
→ `tabs` 배열의 panel 필드가 잘못 연결됨. Home 탭의 panel이 TitlePanel을 가리키는지 확인.

**Q. 하트 카운트다운이 30:00에서 시작 안 함**
→ 처음 실행 시 lives가 5(FULL)이라 타이머는 빈 문자열. 디버그용으로 `HeartSystem.Instance.TrySpendLife()`를 호출해 1개 차감 후 확인.
