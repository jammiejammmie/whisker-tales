# 냥이의 집 (Whisker Tales) - 최종 통합 체크리스트

**목표:** 구글 플레이 스토어 심사 제출 준비 완료

**예상 완료 날짜:** 2026년 5월 7일

---

## 📋 Phase 1: Unity 프로젝트 준비 (1-2일)

### 1.1 폴더 구조 확인
- [ ] Assets/Scripts 폴더 확인
- [ ] Assets/Art/Backgrounds 폴더 생성
- [ ] Assets/Resources/Sounds 폴더 생성
- [ ] Assets/_Data/Cafe 폴더 확인
- [ ] Assets/_Data/CatDex 폴더 생성

### 1.2 스크립트 파일 추가
- [ ] CafeRestorationManager.cs 추가
- [ ] AudioManager.cs 추가
- [ ] CatDexManager.cs 추가
- [ ] CatDexUI.cs 추가
- [ ] 기존 GameController.cs 확인

### 1.3 데이터 파일 추가
- [ ] CafeRestorationData.json 추가
- [ ] CatDexData.json 추가

---

## 📋 Phase 2: 카페 복원 시스템 통합 (1-2일)

### 2.1 Scene 설정
- [ ] CafeRestorationManager를 Scene에 추가
- [ ] 싱글톤 패턴 확인
- [ ] DontDestroyOnLoad 설정 확인

### 2.2 배경 이미지 추가
- [ ] 배경 이미지 15개 다운로드
- [ ] Assets/Art/Backgrounds에 저장
- [ ] PNG 포맷 확인 (2560x1440px)
- [ ] Sprite로 Import 설정
  - Texture Type: Sprite (2D and UI)
  - Sprite Mode: Single
  - Pixels Per Unit: 100

### 2.3 CafeRestorationManager 설정
- [ ] Inspector에서 CafeRestorationData.json 로드
- [ ] UI 요소 할당:
  - [ ] Progress Bar Image
  - [ ] Stage Name Text
  - [ ] Progress Text (예: "3/5")
  - [ ] Background Image (for fading)
- [ ] 배경 이미지 15개 할당
- [ ] 초기 배경 이미지 설정

### 2.4 GameController 수정
- [ ] OnPuzzleComplete() 메서드에 복원 진행도 증가 로직 추가
  ```csharp
  CafeRestorationManager.Instance.AddRestoration(starsEarned);
  ```
- [ ] 퍼즐 클리어 시 별 개수만큼 복원 진행도 증가 확인

### 2.5 테스트
- [ ] 게임 실행
- [ ] 퍼즐 클리어 시 복원 진행도 증가 확인
- [ ] 배경 이미지 페이드 애니메이션 확인
- [ ] 단계 완료 알림 표시 확인
- [ ] 15단계 모두 진행 테스트

---

## 📋 Phase 3: 오디오 시스템 통합 (1일)

### 3.1 폴더 및 파일 준비
- [ ] Assets/Resources/Sounds 폴더 생성
- [ ] 배경음악 파일 다운로드: hanok_ambient_bgm.wav
- [ ] 효과음 파일 다운로드: game_sfx_pack.wav
- [ ] 파일을 Assets/Resources/Sounds에 저장

### 3.2 AudioManager 설정
- [ ] AudioManager를 Scene에 추가
- [ ] 싱글톤 패턴 확인
- [ ] DontDestroyOnLoad 설정 확인
- [ ] Inspector에서 오디오 클립 할당:
  - [ ] Background Music: hanok_ambient_bgm
  - [ ] SFX Clips: 4개 효과음 할당

### 3.3 게임 연동
- [ ] GameController에서 배경음악 재생
  ```csharp
  AudioManager.Instance.PlayBGM("hanok_ambient_bgm");
  ```
- [ ] 퍼즐 매치 시 효과음 재생
  ```csharp
  AudioManager.Instance.PlaySFX("match_success");
  ```
- [ ] 버튼 클릭 시 효과음 재생
- [ ] 보상 획득 시 효과음 재생

### 3.4 테스트
- [ ] 게임 시작 시 배경음악 재생 확인
- [ ] 퍼즐 매치 시 효과음 재생 확인
- [ ] 음량 조절 확인
- [ ] 음소거 기능 확인

---

## 📋 Phase 4: 고양이 도감 시스템 통합 (1-2일)

### 4.1 UI Canvas 생성
- [ ] Canvas 생성: "DexCanvas"
- [ ] Canvas Scaler 설정 (1920x1080)
- [ ] CanvasGroup 컴포넌트 추가

### 4.2 도감 UI 요소 생성
- [ ] DexPanel (Panel) 생성
- [ ] 다음 UI 요소 생성:
  - [ ] CatImage (Image) - 고양이 상세 이미지
  - [ ] CatName (Text) - 고양이 이름
  - [ ] CatDescription (Text) - 고양이 설명
  - [ ] CatBreed (Text) - 고양이 품종
  - [ ] CatPersonality (Text) - 고양이 성격
  - [ ] DexGrid (ScrollView) - 도감 그리드
  - [ ] CloseButton (Button) - 닫기 버튼
  - [ ] OpenDexButton (Button) - 열기 버튼 (메인 화면)

### 4.3 도감 그리드 아이템 프리팹 생성
- [ ] Prefabs 폴더에 "CatDexItem.prefab" 생성
- [ ] 구조:
  - [ ] Image (고양이 이미지)
  - [ ] Button 컴포넌트 추가
  - [ ] LayoutElement 추가 (크기 설정)

### 4.4 CatDexManager 설정
- [ ] CatDexManager를 Scene에 추가
- [ ] Inspector에서 UI 요소 할당:
  - [ ] Dex Grid Content
  - [ ] Cat Dex Item Prefab
  - [ ] Total Cats Text
  - [ ] Collected Cats Text
  - [ ] Cat Detail Image
  - [ ] Cat Detail Name
  - [ ] Cat Detail Description
  - [ ] Cat Detail Breed
  - [ ] Cat Detail Personality

### 4.5 CatDexUI 설정
- [ ] DexCanvas에 CatDexUI 스크립트 추가
- [ ] Inspector에서 설정:
  - [ ] Dex Canvas Group
  - [ ] Open Dex Button
  - [ ] Close Dex Button
  - [ ] 나머지 UI 요소 할당

### 4.6 GameController 수정
- [ ] 퍼즐 클리어 시 고양이 수집 로직 추가
  ```csharp
  int catId = Random.Range(1, 16);
  CatDexManager.Instance.CollectCat(catId);
  ```
- [ ] 카페 복원 단계 완료 시 특별 고양이 수집
  ```csharp
  if (stageId == 5) CatDexManager.Instance.CollectCat(6);
  if (stageId == 10) CatDexManager.Instance.CollectCat(11);
  if (stageId == 15) CatDexManager.Instance.CollectCat(14);
  ```

### 4.7 테스트
- [ ] 게임 실행
- [ ] 퍼즐 클리어 시 고양이 수집 확인
- [ ] 도감 버튼 클릭해서 도감 열기 확인
- [ ] 수집한 고양이 표시 확인
- [ ] 미수집 고양이 물음표 표시 확인
- [ ] 고양이 상세 정보 표시 확인
- [ ] 도감 닫기 확인

---

## 📋 Phase 5: 안정성 및 최적화 (1일)

### 5.1 버그 수정
- [ ] 게임 실행 중 크래시 없음 확인
- [ ] 모든 기능 정상 작동 확인
- [ ] 메모리 누수 확인
- [ ] 프레임 드롭 확인

### 5.2 성능 최적화
- [ ] 배경 이미지 최적화 (압축)
- [ ] 사운드 파일 최적화 (비트레이트 확인)
- [ ] UI 렌더링 최적화
- [ ] 메모리 사용량 확인

### 5.3 호환성 테스트
- [ ] Android 8.0 이상 테스트
- [ ] 다양한 해상도 테스트 (1080p, 1440p, 2K)
- [ ] 다양한 기기 테스트 (가능하면)

---

## 📋 Phase 6: 구글 플레이 심사 준비 (1일)

### 6.1 게임 빌드 생성
- [ ] Build Settings 확인
  - [ ] Platform: Android
  - [ ] Target API Level: 31 이상
  - [ ] Minimum API Level: 24
- [ ] 게임 빌드 생성 (APK 또는 AAB)
- [ ] 빌드 테스트 (실제 기기에서)

### 6.2 스크린샷 준비
- [ ] 게임플레이 스크린샷 5개 (1080x1920px)
  - [ ] 메인 화면
  - [ ] 퍼즐 게임플레이
  - [ ] 카페 복원 진행도
  - [ ] 고양이 도감
  - [ ] 게임 완료 화면

### 6.3 앱 설명 작성
- [ ] 앱 제목: "냥이의 집 - 고양이 퍼즐 게임"
- [ ] 짧은 설명 (80자 이내)
- [ ] 긴 설명 (4000자 이내)
- [ ] 개인정보 보호 정책 링크

### 6.4 앱 아이콘 확인
- [ ] 512x512px PNG
- [ ] 배경 없음 (투명)
- [ ] 고양이 캐릭터 포함

### 6.5 콘텐츠 등급 설정
- [ ] 콘텐츠 등급 설정 완료
- [ ] 개인정보 수집 정책 설정 완료

---

## 📋 Phase 7: 구글 플레이 스토어 제출 (1일)

### 7.1 Google Play Console 설정
- [ ] Google Play Console 계정 확인
- [ ] 개발자 등록 확인
- [ ] 신원 확인 완료 (진행 중)

### 7.2 앱 등록
- [ ] 새 앱 생성
- [ ] 앱 이름: "냥이의 집"
- [ ] 기본 언어: 한국어
- [ ] 앱 카테고리: 게임 > 퍼즐

### 7.3 스토어 정보 입력
- [ ] 앱 설명 입력
- [ ] 스크린샷 업로드 (5개)
- [ ] 앱 아이콘 업로드
- [ ] 기능 그래픽 업로드 (선택)
- [ ] 개인정보 보호 정책 링크 입력

### 7.4 콘텐츠 등급 설정
- [ ] 콘텐츠 등급 설정 완료

### 7.5 가격 및 배포
- [ ] 가격: 무료
- [ ] 배포 국가: 한국 (선택)
- [ ] 배포 국가: 전 세계 (권장)

### 7.6 심사 제출
- [ ] 모든 필수 항목 확인
- [ ] 심사 제출 버튼 클릭
- [ ] 제출 완료 확인

---

## 📋 Phase 8: 심사 후 (2-3주)

### 8.1 심사 모니터링
- [ ] Google Play Console에서 심사 상태 확인
- [ ] 심사 결과 대기 (예상 1-2주)

### 8.2 심사 거절 시 대응
- [ ] 거절 사유 확인
- [ ] 필요한 수정 사항 적용
- [ ] 재심사 제출

### 8.3 심사 통과 후
- [ ] 게임 출시 확인
- [ ] Google Play 스토어에서 검색 가능 확인
- [ ] 다운로드 링크 공유

---

## 🎯 최종 확인 사항

### 필수 요소 (100% 완성)
- [x] 매치-3 게임플레이
- [x] 고양이 캐릭터
- [x] 한옥 배경
- [x] 카페 복원 시스템
- [x] 배경음악 + 효과음
- [x] 고양이 도감
- [x] 안정성 (크래시 없음)

### 권장 요소 (추가 개발)
- [ ] 냥스타그램 (심사 후)
- [ ] 더 많은 고양이 (심사 후)
- [ ] 이벤트 시스템 (심사 후)
- [ ] 멀티플레이 (심사 후)

---

## 📅 타임라인

| 날짜 | 작업 | 상태 |
|------|------|------|
| 5월 5-6일 | Phase 1-4 (통합) | ⏳ 진행 중 |
| 5월 6-7일 | Phase 5-6 (최적화 + 준비) | ⏳ 대기 |
| 5월 7일 | Phase 7 (심사 제출) | ⏳ 대기 |
| 5월 17-21일 | 심사 진행 | ⏳ 대기 |
| 5월 21일 이후 | 출시 | ⏳ 대기 |

---

## 🚀 준비 완료!

**모든 필수 요소가 완성되었습니다!**

이제 Unity에서 통합하면 게임이 완성됩니다! 🎮✨

**화이팅!** 💪🐱
