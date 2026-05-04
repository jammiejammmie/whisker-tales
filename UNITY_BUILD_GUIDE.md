# 냥이의 집 (Whisker Tales) - Unity 빌드 및 구글 플레이 제출 가이드

**목표:** Unity에서 게임을 빌드하고 구글 플레이 스토어에 제출하기

**예상 소요 시간:** 3-4시간

---

## 🎮 Step 1: Unity 프로젝트 최종 점검

### 1.1 프로젝트 열기
```
1. Unity Hub 열기
2. "WhiskerTales" 프로젝트 선택
3. 프로젝트 로드 (2-3분 소요)
```

### 1.2 Scene 확인
```
1. Project 탭에서 "Assets/Scenes" 폴더 열기
2. 다음 Scene 확인:
   - MainMenu.unity (메인 메뉴)
   - GamePlay.unity (게임 플레이)
   - GameOver.unity (게임 오버)
   - Victory.unity (승리 화면)
```

### 1.3 스크립트 통합 확인
```
✅ CafeRestorationManager.cs 추가됨
✅ AudioManager.cs 추가됨
✅ CatDexManager.cs 추가됨
✅ GameController.cs 수정됨
✅ 모든 스크립트 컴파일 오류 없음
```

### 1.4 리소스 확인
```
✅ 배경 이미지 15개 (Assets/Art/Backgrounds/)
✅ 배경음악 (Assets/Resources/Sounds/)
✅ 효과음 (Assets/Resources/Sounds/)
✅ 고양이 스프라이트 (Assets/Art/Characters/)
✅ UI 요소 (Assets/Art/UI/)
```

---

## 🔧 Step 2: Build Settings 설정

### 2.1 Build Settings 열기
```
메뉴: File → Build Settings
또는 단축키: Ctrl+Shift+B
```

### 2.2 플랫폼 선택
```
1. "Android" 선택
2. "Switch Platform" 클릭 (첫 빌드인 경우)
3. 잠시 기다림 (1-2분)
```

### 2.3 Scene 추가
```
1. "Scenes In Build" 섹션 확인
2. 다음 Scene 순서대로 추가:
   0. MainMenu.unity
   1. GamePlay.unity
   2. GameOver.unity
   3. Victory.unity
   
추가 방법:
- Scene을 Build Settings 창으로 드래그
- 또는 "Add Open Scenes" 클릭
```

### 2.4 Build Settings 상세 설정
```
1. "Player Settings..." 클릭
2. 다음 항목 확인:

[Android 탭]
- Minimum API Level: 24 (Android 7.0)
- Target API Level: 33 이상 (Android 13+)
- Scripting Backend: IL2CPP
- ARM64 아키텍처 선택

[Resolution and Presentation]
- Default Orientation: Portrait
- Resolution: 1080 x 1920 (또는 기기 해상도)
- Full Screen: On

[Splash Image]
- Show Splash Screen: On
- Splash Image: 게임 로고 (선택)

[Other Settings]
- Company Name: Whisker Tales
- Product Name: 냥이의 집
- Package Name: com.whiskertales.game
- Version: 1.0.0
- Bundle Version Code: 1
```

---

## 🔐 Step 3: 서명 설정 (Keystore)

### 3.1 Keystore 파일 생성
```
1. Player Settings에서 "Android" 탭 선택
2. "Publishing Settings" 섹션 찾기
3. "Keystore Manager" 클릭

또는 메뉴: Assets → Android → Keystore Manager
```

### 3.2 새 Keystore 생성
```
1. "Create New" 클릭
2. 저장 위치 선택: 프로젝트 폴더 내
3. 파일명: "whisker_tales.keystore"
4. 암호 설정 (기억해두기!)
   예: WhiskerTales2026!
5. "Create" 클릭
```

### 3.3 Key 생성
```
1. "Create New Key" 클릭
2. 다음 정보 입력:
   - Alias: whisker_tales_key
   - Password: (암호 설정)
   - Validity (years): 25
   - First and Last Name: Whisker Tales
   - Organizational Unit: Game Development
   - Organization: Whisker Tales
   - City or Locality: Seoul
   - State or Province: Seoul
   - Country Code: KR
3. "Create Key" 클릭
```

### 3.4 서명 설정 확인
```
Player Settings → Publishing Settings에서:
- Keystore: whisker_tales.keystore
- Keystore password: (설정한 암호)
- Key Alias: whisker_tales_key
- Key password: (설정한 암호)
```

---

## 🏗️ Step 4: 게임 빌드

### 4.1 빌드 설정 최종 확인
```
Build Settings에서:
✅ Platform: Android
✅ Scenes: 4개 모두 추가
✅ Player Settings 완료
✅ Keystore 설정 완료
```

### 4.2 APK 빌드 생성
```
1. Build Settings 창에서 "Build" 클릭
2. 저장 위치 선택: 프로젝트 폴더 내 "Builds" 폴더
3. 파일명: WhiskerTales_v1.0.apk
4. "Save" 클릭
5. 빌드 시작 (5-15분 소요)

빌드 진행 상황:
- "Building Player..." 메시지 표시
- 진행률 표시
- 완료 후 "Build complete!" 메시지
```

### 4.3 AAB (Android App Bundle) 빌드 생성
```
구글 플레이 스토어 제출용:

1. Build Settings에서 "Build App Bundle (Google Play)" 클릭
2. 저장 위치: "Builds" 폴더
3. 파일명: WhiskerTales_v1.0.aab
4. "Save" 클릭
5. 빌드 시작 (5-15분 소요)
```

### 4.4 빌드 파일 확인
```
Builds 폴더에서 다음 파일 확인:
- WhiskerTales_v1.0.apk (약 100-150MB)
- WhiskerTales_v1.0.aab (약 80-120MB)

파일 크기가 너무 크면 (500MB 이상):
- 텍스처 압축 확인
- 오디오 압축 확인
- 불필요한 리소스 제거
```

---

## 📱 Step 5: APK 테스트 (선택)

### 5.1 실제 기기에 설치
```
1. Android 기기를 USB로 컴퓨터 연결
2. 기기에서 "개발자 옵션" 활성화
3. "USB 디버깅" 활성화
4. APK 파일을 기기로 전송
5. 파일 매니저에서 APK 설치
6. 게임 실행 테스트
```

### 5.2 테스트 항목
```
✅ 게임 시작 정상 작동
✅ 메인 메뉴 정상 작동
✅ 게임플레이 정상 작동
✅ 퍼즐 매치 정상 작동
✅ 별 수집 정상 작동
✅ 카페 복원 진행도 정상 증가
✅ 배경 이미지 페이드 정상 작동
✅ 고양이 수집 정상 작동
✅ 도감 열기/닫기 정상 작동
✅ 사운드 재생 정상 작동
✅ 게임 저장/로드 정상 작동
✅ 크래시 없음
```

---

## 🎯 Step 6: Google Play Console 준비

### 6.1 Google Play Console 접속
```
https://play.google.com/console
```

### 6.2 신원 확인 완료 확인
```
1. 계정 설정에서 신원 확인 상태 확인
2. "신원 확인 완료" 상태 확인
3. 개발자 등록 완료 확인 (25 USD 결제)
```

### 6.3 새 앱 생성
```
1. "앱 만들기" 클릭
2. 앱 이름: "냥이의 집"
3. 기본 언어: 한국어
4. 앱 또는 게임: "게임" 선택
5. 무료 또는 유료: "무료" 선택
6. "앱 만들기" 클릭
```

---

## 📝 Step 7: 스토어 정보 입력

### 7.1 앱 정보
```
1. 왼쪽 메뉴: "스토어 정보"
2. 다음 항목 입력:

[기본 정보]
- 앱 이름: 냥이의 집
- 짧은 설명: 
  "고양이와 함께 한옥을 복원하는 매치-3 퍼즐 게임"
- 긴 설명:
  "낡은 한옥 카페를 복원하는 치유 게임입니다.
   매치-3 퍼즐을 풀어 별을 획득하고,
   귀여운 고양이들을 수집하세요!
   
   주요 기능:
   - 중독성 있는 매치-3 퍼즐
   - 15마리의 귀여운 고양이 수집
   - 한옥 카페 단계별 복원
   - 한국 전통 음악 배경음악
   - 오프라인 플레이 지원
   
   모든 연령대가 즐길 수 있는 게임입니다!"

[카테고리]
- 게임 > 퍼즐

[콘텐츠 등급]
- 모든 연령대 (3세 이상)
```

### 7.2 스크린샷 업로드
```
1. "스크린샷" 섹션
2. 다음 5개 스크린샷 업로드 (1080x1920px):
   1. 메인 메뉴
   2. 게임플레이 화면
   3. 카페 복원 화면
   4. 고양이 도감
   5. 게임 완료 화면

팁: 각 스크린샷에 한국어 텍스트 추가
```

### 7.3 앱 아이콘 업로드
```
1. "앱 아이콘" 섹션
2. 512x512px PNG 파일 업로드
3. 투명 배경 (선택)
```

### 7.4 개인정보 보호 정책
```
1. "개인정보 보호 정책" 섹션
2. 정책 URL 입력 또는 작성
3. 저장
```

---

## 🔒 Step 8: 콘텐츠 등급 설정

### 8.1 콘텐츠 등급 설정
```
1. 왼쪽 메뉴: "콘텐츠 등급"
2. "설정 시작" 클릭
3. 다음 질문에 답변:

[폭력]
- 폭력 없음: 선택

[성인용 콘텐츠]
- 성인용 콘텐츠 없음: 선택

[음주/약물]
- 음주/약물 없음: 선택

[도박]
- 도박 없음: 선택

[기타]
- 모든 항목 "아니오" 선택

4. "저장" 클릭
```

---

## 📤 Step 9: 빌드 업로드

### 9.1 Google Play Console에서 빌드 업로드
```
1. 왼쪽 메뉴: "출시" → "프로덕션"
2. "새 출시 만들기" 클릭
3. "APK/AAB 추가" 클릭
4. AAB 파일 선택: WhiskerTales_v1.0.aab
5. 업로드 (5-10분 소요)
```

### 9.2 출시 정보 입력
```
1. "출시 정보" 섹션
2. 출시 이름: "1.0.0 출시"
3. 출시 노트 (선택):
   "초기 출시 버전입니다!"
4. "저장" 클릭
```

### 9.3 심사 제출 전 확인
```
체크리스트:
✅ 앱 정보 완료
✅ 스크린샷 업로드
✅ 앱 아이콘 업로드
✅ 개인정보 보호 정책 입력
✅ 콘텐츠 등급 설정
✅ 빌드 업로드
✅ 출시 정보 입력
✅ 모든 필수 항목 완료
```

---

## 🚀 Step 10: 심사 제출

### 10.1 최종 확인
```
Google Play Console에서:
1. 모든 오류 메시지 확인 (빨간색)
2. 모든 경고 메시지 확인 (노란색)
3. 필수 항목 모두 완료 확인
```

### 10.2 심사 제출
```
1. "검토 시작" 또는 "제출" 버튼 클릭
2. 최종 확인 메시지 표시
3. "제출" 클릭
4. 심사 시작!
```

### 10.3 심사 진행 상황 확인
```
1. Google Play Console 메인 화면
2. "출시" → "프로덕션" 확인
3. 심사 상태 표시:
   - "심사 중": 진행 중
   - "승인됨": 승인 완료
   - "거절됨": 거절 (수정 후 재제출)
```

---

## ⏰ 심사 일정

| 단계 | 예상 시간 | 상태 |
|------|---------|------|
| 빌드 생성 | 1시간 | ⏳ 대기 |
| 빌드 테스트 | 1시간 | ⏳ 대기 |
| Google Play 업로드 | 30분 | ⏳ 대기 |
| 심사 진행 | 1-2주 | ⏳ 대기 |
| **총 소요 시간** | **1-2주** | - |

---

## 🎯 예상 타임라인

| 날짜 | 작업 | 상태 |
|------|------|------|
| **5월 5-6일** | Unity 빌드 생성 | ⏳ 대기 |
| **5월 6일** | APK 테스트 | ⏳ 대기 |
| **5월 6-7일** | Google Play 업로드 | ⏳ 대기 |
| **5월 7일** | 심사 제출 | ⏳ 대기 |
| **5월 17-21일** | 심사 진행 | ⏳ 대기 |
| **5월 21일 이후** | **게임 출시!** 🎊 | ⏳ 대기 |

---

## ❓ 자주 묻는 질문 (FAQ)

### Q1: 빌드가 너무 오래 걸려요
```
A: 정상입니다. 첫 빌드는 15-20분 소요될 수 있습니다.
   - IL2CPP 컴파일 시간 포함
   - 이후 빌드는 더 빠름
```

### Q2: 빌드 중 오류가 발생했어요
```
A: 다음을 확인하세요:
   1. 모든 스크립트 컴파일 오류 없는지 확인
   2. 모든 리소스 파일이 있는지 확인
   3. Android SDK 최신 버전 설치 확인
   4. Unity 최신 버전 설치 확인
```

### Q3: 심사가 거절되었어요
```
A: 다음을 확인하세요:
   1. 거절 사유 읽기
   2. 문제 해결
   3. 빌드 재생성
   4. 재제출
```

### Q4: 게임이 출시된 후 업데이트하려면?
```
A: 다음 단계를 따르세요:
   1. 게임 코드 수정
   2. Version 번호 증가 (1.0.1)
   3. Bundle Version Code 증가 (2)
   4. 새 빌드 생성
   5. Google Play에 업로드
   6. 심사 제출
```

---

## ✅ 최종 체크리스트

- [ ] Unity 프로젝트 최종 점검 완료
- [ ] Build Settings 설정 완료
- [ ] Keystore 생성 완료
- [ ] APK 빌드 생성 완료
- [ ] AAB 빌드 생성 완료
- [ ] APK 테스트 완료 (선택)
- [ ] Google Play Console 앱 생성 완료
- [ ] 스토어 정보 입력 완료
- [ ] 스크린샷 업로드 완료
- [ ] 콘텐츠 등급 설정 완료
- [ ] 빌드 업로드 완료
- [ ] 심사 제출 완료

---

## 🎉 준비 완료!

**모든 단계가 준비되었습니다!**

이제 Unity에서 빌드하고 Google Play에 제출하면 됩니다!

**화이팅!** 🚀🐱✨
