# 카페 복원 시스템 & 오디오 매니저 통합 가이드

**작성일:** 2026년 5월 4일
**목적:** 새로운 시스템을 기존 게임에 통합하는 방법 설명

---

## 📋 통합할 파일들

### 1. 새로 생성된 스크립트:
- `Assets/Scripts/CafeRestorationManager.cs` - 카페 복원 시스템
- `Assets/Scripts/AudioManager.cs` - 오디오 관리 시스템

### 2. 새로 생성된 데이터:
- `Assets/_Data/Cafe/CafeRestorationData.json` - 복원 단계 데이터

### 3. 기존 스크립트 수정 필요:
- `Assets/Scripts/GameController.cs` - 퍼즐 클리어 이벤트 연동
- `Assets/Scripts/Match3Engine.cs` - 별 획득 로직 연동

---

## 🔧 통합 단계

### Step 1: CafeRestorationManager를 Scene에 추가

1. Unity Editor 열기
2. `Assets/Scenes/MainScene.unity` 열기
3. Hierarchy에서 마우스 우클릭 → Create Empty
4. 이름을 `CafeRestorationManager`로 변경
5. Inspector에서 `Add Component` → `CafeRestorationManager` 추가

**필요한 UI 요소 할당:**
- Progress Bar (Image)
- Stage Description Text (Text)
- Stars Required Text (Text)
- Cafe Background Image (Image)

---

### Step 2: AudioManager를 Scene에 추가

1. Hierarchy에서 마우스 우클릭 → Create Empty
2. 이름을 `AudioManager`로 변경
3. Inspector에서 `Add Component` → `AudioManager` 추가

**필요한 오디오 클립 할당:**
- BGM Clip (배경음악)
- Match Success Clip (매치 성공음)
- Button Click Clip (버튼 클릭음)
- Reward Get Clip (보상 획득음)
- Cat Meow Clip (고양이 울음소리)

---

### Step 3: GameController에서 퍼즐 클리어 이벤트 연동

**기존 GameController.cs에 다음 코드 추가:**

```csharp
// GameController.cs의 OnPuzzleClear() 메서드에 추가

public void OnPuzzleClear(int starsEarned)
{
    // 기존 코드...
    
    // 카페 복원 시스템에 별 전달
    if (CafeRestorationManager.instance != null)
    {
        CafeRestorationManager.instance.OnPuzzleClear(starsEarned);
    }
    
    // 오디오 재생
    if (AudioManager.instance != null)
    {
        AudioManager.instance.PlayMatchSuccess();
        AudioManager.instance.PlayRewardGet();
    }
}
```

---

### Step 4: 버튼 클릭에 오디오 연동

**모든 버튼 클릭 이벤트에 다음 코드 추가:**

```csharp
// 버튼 클릭 이벤트 핸들러
public void OnButtonClick()
{
    // 오디오 재생
    if (AudioManager.instance != null)
    {
        AudioManager.instance.PlayButtonClick();
    }
    
    // 기존 버튼 기능...
}
```

---

### Step 5: 고양이 상호작용에 오디오 연동

**CatManager.cs의 고양이 상호작용 메서드에 추가:**

```csharp
// 고양이 쓰다듬기
public void PetCat()
{
    // 오디오 재생
    if (AudioManager.instance != null)
    {
        AudioManager.instance.PlayCatMeow();
    }
    
    // 기존 애니메이션...
}
```

---

## 📁 폴더 구조 확인

```
WhiskerTales/
├── Assets/
│   ├── Scripts/
│   │   ├── CafeRestorationManager.cs ✅ (신규)
│   │   ├── AudioManager.cs ✅ (신규)
│   │   ├── GameController.cs (수정 필요)
│   │   ├── Match3Engine.cs (수정 필요)
│   │   ├── CatManager.cs (수정 필요)
│   │   └── ... (기존 파일들)
│   ├── _Data/
│   │   ├── Cafe/
│   │   │   └── CafeRestorationData.json ✅ (신규)
│   │   ├── Levels/
│   │   └── ... (기존 데이터)
│   ├── _Art/
│   │   ├── Backgrounds/
│   │   │   ├── cafe_stage_1_sign_restored.png (필요)
│   │   │   ├── cafe_stage_2_wall_restored.png (필요)
│   │   │   ├── ... (더 많은 배경 이미지)
│   │   └── ... (기존 아트)
│   └── Scenes/
│       └── MainScene.unity (수정 필요)
```

---

## 🎵 사운드 파일 준비

### 필요한 사운드 파일:

1. **배경음악 (BGM)**
   - 파일명: `hanok_ambient_bgm.mp3` 또는 `.wav`
   - 길이: 1-2분 (반복 재생)
   - 위치: `Assets/Resources/Sounds/`

2. **효과음 (SFX)**
   - `match_success.wav` - 매치 성공음 (0.3-0.5초)
   - `button_click.wav` - 버튼 클릭음 (0.2-0.3초)
   - `reward_get.wav` - 보상 획득음 (0.5-0.8초)
   - 위치: `Assets/Resources/Sounds/`

3. **고양이 울음소리**
   - `cat_meow.wav` - 이미 있음 (`Cat_Meow_Sound.wav`)

---

## 🖼️ 배경 이미지 준비

### 필요한 배경 이미지:

```
Assets/Resources/Backgrounds/
├── cafe_stage_1_sign_restored.png
├── cafe_stage_2_wall_restored.png
├── cafe_stage_3_ground_clean.png
├── cafe_stage_4_garden_flowers.png
├── cafe_stage_5_entrance_complete.png
├── cafe_stage_6_roof_restored.png
├── cafe_stage_7_windows_fixed.png
├── cafe_stage_8_walls_painted.png
├── cafe_stage_9_door_replaced.png
├── cafe_stage_10_main_complete.png
├── cafe_stage_11_pond_clean.png
├── cafe_stage_12_trees_trimmed.png
├── cafe_stage_13_bench_installed.png
├── cafe_stage_14_lanterns_hung.png
└── cafe_stage_15_backyard_complete.png
```

각 이미지는 게임 화면 해상도에 맞춰 준비해야 합니다.

---

## ✅ 통합 체크리스트

### 스크립트 통합:
- [ ] CafeRestorationManager.cs를 Scene에 추가
- [ ] AudioManager.cs를 Scene에 추가
- [ ] GameController.cs 수정 (OnPuzzleClear 메서드)
- [ ] 모든 버튼에 오디오 연동
- [ ] CatManager.cs 수정 (고양이 상호작용)

### 데이터 준비:
- [ ] CafeRestorationData.json이 Resources/Cafe 폴더에 있는지 확인
- [ ] 모든 배경 이미지가 Resources/Backgrounds 폴더에 있는지 확인
- [ ] 모든 사운드 파일이 Resources/Sounds 폴더에 있는지 확인

### UI 설정:
- [ ] Progress Bar UI 할당
- [ ] Stage Description Text UI 할당
- [ ] Stars Required Text UI 할당
- [ ] Cafe Background Image 할당

### 오디오 설정:
- [ ] BGM Clip 할당
- [ ] Match Success Clip 할당
- [ ] Button Click Clip 할당
- [ ] Reward Get Clip 할당
- [ ] Cat Meow Clip 할당

### 테스트:
- [ ] 게임 실행 시 배경음악 재생 확인
- [ ] 퍼즐 클리어 시 효과음 재생 확인
- [ ] 버튼 클릭 시 클릭음 재생 확인
- [ ] 카페 복원 진행도 업데이트 확인
- [ ] 단계 완료 시 배경 이미지 변경 확인

---

## 🐛 문제 해결

### 배경음악이 재생되지 않음:
1. AudioManager가 Scene에 있는지 확인
2. BGM Clip이 할당되었는지 확인
3. Console에서 에러 메시지 확인

### 효과음이 재생되지 않음:
1. 해당 SFX Clip이 할당되었는지 확인
2. sfxSource의 음량이 0이 아닌지 확인
3. 음소거 상태가 아닌지 확인

### 카페 복원 진행도가 업데이트되지 않음:
1. CafeRestorationManager가 Scene에 있는지 확인
2. UI 요소들이 할당되었는지 확인
3. GameController에서 OnPuzzleClear 메서드가 호출되는지 확인

### 배경 이미지가 변경되지 않음:
1. 배경 이미지 파일이 Resources/Backgrounds 폴더에 있는지 확인
2. 파일명이 CafeRestorationData.json의 visualChangeKey와 일치하는지 확인
3. Cafe Background Image가 할당되었는지 확인

---

## 📞 추가 지원

문제가 발생하면:
1. Console 로그 확인
2. Inspector에서 모든 할당이 제대로 되었는지 확인
3. 위의 체크리스트 다시 확인

---

**작성자:** Manus AI
**최종 수정:** 2026년 5월 4일
