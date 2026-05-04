# 배경 이미지 통합 가이드 (Background Images Integration Guide)

**작성일:** 2026년 5월 4일
**목적:** 15개의 카페 복원 단계 배경 이미지를 Unity 게임에 통합

---

## ✅ 완성된 배경 이미지 (15개)

### 📁 파일 위치:
```
WhiskerTales/Assets/Art/Backgrounds/
├── cafe_stage_1_sign_restored.png (입구 마당 1단계)
├── cafe_stage_2_wall_restored.png (입구 마당 2단계)
├── cafe_stage_3_ground_clean.png (입구 마당 3단계)
├── cafe_stage_4_garden_flowers.png (입구 마당 4단계)
├── cafe_stage_5_entrance_complete.png (입구 마당 완성)
├── cafe_stage_6_roof_restored.png (카페 본채 1단계)
├── cafe_stage_7_windows_fixed.png (카페 본채 2단계)
├── cafe_stage_8_walls_painted.png (카페 본채 3단계)
├── cafe_stage_9_door_replaced.png (카페 본채 4단계)
├── cafe_stage_10_main_complete.png (카페 본채 완성)
├── cafe_stage_11_pond_clean.png (뒷마당 1단계)
├── cafe_stage_12_trees_trimmed.png (뒷마당 2단계)
├── cafe_stage_13_bench_installed.png (뒷마당 3단계)
├── cafe_stage_14_lanterns_hung.png (뒷마당 4단계)
└── cafe_stage_15_backyard_complete.png (뒷마당 완성)
```

---

## 🎮 Unity 통합 방법

### 1단계: 폴더 구조 확인

```
Assets/
├── Art/
│   └── Backgrounds/
│       ├── cafe_stage_1_sign_restored.png
│       ├── cafe_stage_2_wall_restored.png
│       ├── ... (총 15개)
│       └── cafe_stage_15_backyard_complete.png
├── Scripts/
│   ├── CafeRestorationManager.cs
│   └── ... (기존 스크립트)
└── Resources/
    └── Sounds/
        └── ... (사운드 파일)
```

### 2단계: 이미지 임포트 설정

1. **Unity Editor에서 각 이미지 선택**
2. **Inspector에서 다음 설정 확인:**
   - **Texture Type:** Sprite (2D and UI)
   - **Sprite Mode:** Single
   - **Pixels Per Unit:** 100
   - **Filter Mode:** Bilinear
   - **Compression:** Default

3. **Apply 클릭**

### 3단계: CafeRestorationManager에 이미지 할당

1. **Scene에서 CafeRestorationManager 오브젝트 선택**
2. **Inspector에서 CafeRestorationManager 컴포넌트 찾기**
3. **다음 필드에 이미지 할당:**

```csharp
// CafeRestorationManager.cs에서 수정 필요:

[SerializeField] private Sprite[] entranceStageSprites; // 5개 (stage 1-5)
[SerializeField] private Sprite[] mainBuildingStageSprites; // 5개 (stage 6-10)
[SerializeField] private Sprite[] backyardStageSprites; // 5개 (stage 11-15)

// 또는 통합 배열:
[SerializeField] private Sprite[] allStageSprites = new Sprite[15]; // 15개 모두
```

### 4단계: 배경 이미지 표시 스크립트

```csharp
// 예제: CafeRestorationManager.cs 수정

using UnityEngine;
using UnityEngine.UI;

public class CafeRestorationManager : MonoBehaviour
{
    [SerializeField] private Image backgroundImage; // UI Image 컴포넌트
    [SerializeField] private Sprite[] allStageSprites = new Sprite[15];
    
    private int currentStage = 0;
    
    public void UpdateBackgroundImage(int stage)
    {
        if (stage >= 0 && stage < allStageSprites.Length)
        {
            currentStage = stage;
            backgroundImage.sprite = allStageSprites[stage];
            
            // 페이드 효과 (선택사항)
            StartCoroutine(FadeBackgroundImage());
        }
    }
    
    private System.Collections.IEnumerator FadeBackgroundImage()
    {
        // 페이드 인 효과 (0.5초)
        float duration = 0.5f;
        float elapsed = 0f;
        
        Color startColor = backgroundImage.color;
        startColor.a = 0f;
        backgroundImage.color = startColor;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            Color color = backgroundImage.color;
            color.a = Mathf.Lerp(0f, 1f, elapsed / duration);
            backgroundImage.color = color;
            yield return null;
        }
        
        Color finalColor = backgroundImage.color;
        finalColor.a = 1f;
        backgroundImage.color = finalColor;
    }
}
```

---

## 📊 이미지 단계별 설명

### 입구 마당 (Entrance Area) - Stage 1-5

| 단계 | 파일명 | 설명 |
|------|--------|------|
| 1 | cafe_stage_1_sign_restored.png | 낡은 목재 간판, 덩굴, 진흙 |
| 2 | cafe_stage_2_wall_restored.png | 간판 복원, 벽 부분 정리 |
| 3 | cafe_stage_3_ground_clean.png | 지면 청소, 돌길 시작 |
| 4 | cafe_stage_4_garden_flowers.png | 꽃 정원 추가, 울타리 복원 |
| 5 | cafe_stage_5_entrance_complete.png | 완전 복원, 등불 설치 |

### 카페 본채 (Main Building) - Stage 6-10

| 단계 | 파일명 | 설명 |
|------|--------|------|
| 6 | cafe_stage_6_roof_restored.png | 손상된 지붕, 어두운 내부 |
| 7 | cafe_stage_7_windows_fixed.png | 창문 수리, 밝아진 내부 |
| 8 | cafe_stage_8_walls_painted.png | 벽 칠하기, 밝은 색상 |
| 9 | cafe_stage_9_door_replaced.png | 새 문 설치, 전통 문양 |
| 10 | cafe_stage_10_main_complete.png | 완전 복원, 테이블 배치 |

### 뒷마당 (Backyard) - Stage 11-15

| 단계 | 파일명 | 설명 |
|------|--------|------|
| 11 | cafe_stage_11_pond_clean.png | 흐린 연못, 이끼 덮인 돌 |
| 12 | cafe_stage_12_trees_trimmed.png | 나무 정리, 깨끗한 연못 |
| 13 | cafe_stage_13_bench_installed.png | 벤치 설치, 평온한 분위기 |
| 14 | cafe_stage_14_lanterns_hung.png | 등불 설치, 마법 같은 분위기 |
| 15 | cafe_stage_15_backyard_complete.png | 완전 복원, 달빛 반사 |

---

## 🎨 이미지 사양

**모든 이미지 공통:**
- **해상도:** 2560 x 1440px
- **포맷:** PNG
- **색상 공간:** sRGB
- **알파 채널:** 없음 (완전 불투명)

---

## 🔧 CafeRestorationData.json 업데이트

```json
{
  "areas": [
    {
      "areaId": 1,
      "areaName": "Entrance Area",
      "stages": [
        {
          "stageId": 1,
          "stageName": "Sign Restored",
          "requiredStars": 0,
          "backgroundImageIndex": 0,
          "description": "낡은 간판 복원"
        },
        {
          "stageId": 2,
          "stageName": "Wall Cleaned",
          "requiredStars": 5,
          "backgroundImageIndex": 1,
          "description": "벽 정리"
        },
        // ... (계속)
      ]
    }
  ]
}
```

---

## 🎯 테스트 체크리스트

- [ ] 모든 15개 이미지 임포트 확인
- [ ] CafeRestorationManager에 이미지 할당
- [ ] 배경 이미지 표시 스크립트 작동 확인
- [ ] 페이드 효과 작동 확인
- [ ] 게임 진행 시 이미지 순서 확인
- [ ] 각 단계별 이미지 전환 확인

---

## 💡 추가 최적화

### 1. 메모리 최적화
```csharp
// 필요할 때만 이미지 로드
[SerializeField] private string[] stageImagePaths = new string[15];

public void LoadStageImageOnDemand(int stage)
{
    if (stage >= 0 && stage < stageImagePaths.Length)
    {
        Sprite sprite = Resources.Load<Sprite>(stageImagePaths[stage]);
        backgroundImage.sprite = sprite;
    }
}
```

### 2. 애니메이션 효과
```csharp
// 슬라이드 전환
private System.Collections.IEnumerator SlideTransition()
{
    float duration = 0.3f;
    float elapsed = 0f;
    
    Vector3 startPos = backgroundImage.transform.localPosition;
    Vector3 endPos = startPos + Vector3.right * 100;
    
    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        backgroundImage.transform.localPosition = 
            Vector3.Lerp(startPos, endPos, elapsed / duration);
        yield return null;
    }
}
```

---

## 📝 주의사항

1. **이미지 순서:** 반드시 stage 1-15 순서대로 배열에 할당
2. **해상도:** 2560x1440 유지 (게임 품질 보장)
3. **메모리:** 15개 이미지 = 약 50-60MB (로드 시 고려)
4. **성능:** 필요시 이미지 압축 또는 스트리밍 로드 고려

---

**작성자:** Manus AI
**최종 수정:** 2026년 5월 4일
