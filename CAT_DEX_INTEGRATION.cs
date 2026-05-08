using UnityEngine;

/// <summary>
/// CAT_DEX_INTEGRATION - 고양이 도감 시스템 통합 가이드
/// 
/// 이 파일은 CatDexManager와 CatDexUI를 게임에 통합하는 방법을 설명합니다.
/// </summary>

/*
 * ===== 고양이 도감 시스템 통합 단계 =====
 * 
 * 1. Scene 설정
 * ─────────────────────────────────────
 * 
 * Step 1: CatDexManager를 Scene에 추가
 * - Hierarchy에서 우클릭 → Create Empty
 * - 이름을 "CatDexManager"로 변경
 * - Inspector에서 "Add Component" → CatDexManager 추가
 * 
 * Step 2: CatDexUI Canvas 생성
 * - Hierarchy에서 우클릭 → UI → Canvas
 * - 이름을 "DexCanvas"로 변경
 * - Canvas 크기: 1920x1080 (또는 화면 크기에 맞게)
 * 
 * Step 3: 도감 UI 요소 생성
 * - DexCanvas 아래에 Panel 생성 (이름: "DexPanel")
 * - DexPanel 아래에 다음 요소들 생성:
 *   - Image (이름: "CatImage") - 고양이 상세 이미지
 *   - Text (이름: "CatName") - 고양이 이름
 *   - Text (이름: "CatDescription") - 고양이 설명
 *   - Text (이름: "CatBreed") - 고양이 품종
 *   - Text (이름: "CatPersonality") - 고양이 성격
 *   - ScrollView (이름: "DexGrid") - 도감 그리드
 *   - Button (이름: "CloseButton") - 닫기 버튼
 * 
 * Step 4: 도감 그리드 아이템 프리팹 생성
 * - Assets/Prefabs 폴더에 "CatDexItem.prefab" 생성
 * - 구조:
 *   - Image (고양이 이미지)
 *   - Button 컴포넌트 추가
 * 
 * ─────────────────────────────────────
 * 
 * 2. 스크립트 설정
 * ─────────────────────────────────────
 * 
 * Step 1: CatDexManager 설정
 * - CatDexManager Inspector에서:
 *   - Dex Grid Content: DexCanvas/DexPanel/DexGrid/Viewport/Content
 *   - Cat Dex Item Prefab: Assets/Prefabs/CatDexItem.prefab
 *   - Total Cats Text: DexPanel/Stats/TotalCatsText
 *   - Collected Cats Text: DexPanel/Stats/CollectedCatsText
 *   - Cat Detail Image: DexPanel/CatImage
 *   - Cat Detail Name: DexPanel/CatName
 *   - Cat Detail Description: DexPanel/CatDescription
 *   - Cat Detail Breed: DexPanel/CatBreed
 *   - Cat Detail Personality: DexPanel/CatPersonality
 * 
 * Step 2: CatDexUI 설정
 * - DexCanvas에 CatDexUI 스크립트 추가
 * - CatDexUI Inspector에서:
 *   - Dex Canvas Group: DexCanvas (CanvasGroup 컴포넌트 추가 필요)
 *   - Open Dex Button: 메인 화면의 도감 버튼
 *   - Close Dex Button: DexPanel/CloseButton
 *   - Dex Grid Content: DexCanvas/DexPanel/DexGrid/Viewport/Content
 *   - Cat Dex Item Prefab: Assets/Prefabs/CatDexItem.prefab
 *   - Dex Scroll Rect: DexCanvas/DexPanel/DexGrid
 *   - Collection Stats Text: DexPanel/Stats/CollectionStatsText
 * 
 * ─────────────────────────────────────
 * 
 * 3. GameController 수정
 * ─────────────────────────────────────
 * 
 * 퍼즐 클리어 시 고양이 수집 로직 추가:
 * 
 * public void OnPuzzleComplete(int levelId)
 * {
 *     // 기존 코드...
 *     
 *     // 고양이 수집 로직
 *     int catIdToCollect = Random.Range(1, 16); // 1-15 중 랜덤
 *     CatDexManager.Instance.CollectCat(catIdToCollect);
 *     
 *     // 또는 레벨별로 특정 고양이 수집
 *     if (levelId == 1) CatDexManager.Instance.CollectCat(1);
 *     if (levelId == 2) CatDexManager.Instance.CollectCat(2);
 *     // ...
 * }
 * 
 * ─────────────────────────────────────
 * 
 * 4. 카페 복원과 연동
 * ─────────────────────────────────────
 * 
 * CafeRestorationManager에서 고양이 수집 보상 추가:
 * 
 * public void OnRestorationStageComplete(int stageId)
 * {
 *     // 기존 코드...
 *     
 *     // 특정 단계 완료 시 고양이 수집
 *     if (stageId == 5) CatDexManager.Instance.CollectCat(6); // 입구 마당 완료 → 금눈이
 *     if (stageId == 10) CatDexManager.Instance.CollectCat(11); // 카페 본채 완료 → 황금냥
 *     if (stageId == 15) CatDexManager.Instance.CollectCat(14); // 뒷마당 완료 → 무지개냥
 * }
 * 
 * ─────────────────────────────────────
 * 
 * 5. 테스트
 * ─────────────────────────────────────
 * 
 * 1. 게임 실행
 * 2. 퍼즐 클리어 또는 카페 복원 진행
 * 3. 고양이 수집 알림 확인
 * 4. 도감 버튼 클릭해서 수집한 고양이 확인
 * 5. 고양이 상세 정보 표시 확인
 * 
 * ─────────────────────────────────────
 * 
 * 6. 고급 기능 (선택사항)
 * ─────────────────────────────────────
 * 
 * - 레어도별 필터링
 * - 고양이 정렬 (이름, 레어도, 수집 순서)
 * - 도감 완성도 보상
 * - 특정 고양이 조합 수집 시 특별 보상
 * - 고양이 상호작용 (터치해서 울음소리 재생 등)
 * 
 * ─────────────────────────────────────
 */

public class CAT_DEX_INTEGRATION
{
    // 이 클래스는 통합 가이드 용도이며, 실제 코드는 포함하지 않습니다.
    // 위의 주석을 참고하여 통합을 진행하세요.
}

/*
 * ===== 추가 팁 =====
 * 
 * 1. 고양이 이미지 준비
 *    - 각 고양이마다 512x512 PNG 이미지 준비
 *    - Sprite로 import 설정
 * 
 * 2. 고양이 울음소리
 *    - 각 고양이마다 울음소리 오디오 클립 준비
 *    - Assets/Resources/Sounds/Cats 폴더에 저장
 *    - AudioManager에서 재생
 * 
 * 3. 도감 저장
 *    - PlayerPrefs를 사용하여 수집한 고양이 정보 저장
 *    - 게임 종료 후에도 수집 정보 유지
 * 
 * 4. 성능 최적화
 *    - 도감 그리드는 ScrollView로 구현
 *    - 보이는 아이템만 렌더링 (Object Pool 패턴 사용 권장)
 * 
 * 5. UI/UX
 *    - 새로운 고양이 수집 시 파티클 이펙트 추가
 *    - 도감 열기/닫기 애니메이션 추가
 *    - 고양이 상세 정보 슬라이드 애니메이션 추가
 */
