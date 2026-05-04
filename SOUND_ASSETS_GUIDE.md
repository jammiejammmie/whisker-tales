# 사운드 자산 준비 가이드

**작성일:** 2026년 5월 4일
**목적:** 게임에 필요한 모든 사운드 파일 준비 및 생성 방법

---

## 🎵 필요한 사운드 파일 (총 4가지)

### 1. 배경음악 (BGM) - 1개

**파일명:** `hanok_ambient_bgm.mp3` 또는 `.wav`

**사양:**
- 길이: 1-2분 (반복 재생)
- 포맷: MP3 또는 WAV
- 비트레이트: 128-192 kbps (MP3) 또는 44.1kHz 16-bit (WAV)
- 음향: 한국 전통 악기 + 현대적 힐링 음악
- 분위기: 평온, 따뜻함, 한옥 감성
- 음량: -6dB (게임플레이 방해 X)

**생성 방법:**

#### 옵션 A: AI 음악 생성 (추천)
1. **Suno.ai** (https://www.suno.ai)
   - 프롬프트: "Traditional Korean Hanok ambient music, healing, peaceful, 1-2 minutes, loopable"
   - 생성 후 다운로드

2. **Udio** (https://www.udio.com)
   - 프롬프트: "한옥 감성 힐링 음악, 전통 악기, 현대적, 평온한 분위기"
   - 생성 후 다운로드

#### 옵션 B: 로열티 프리 음악 라이브러리
1. **Epidemic Sound** (https://www.epidemicsound.com)
   - 검색: "Hanok", "Korean traditional", "healing ambient"
   - 필터: 1-2분, 배경음악

2. **Artlist** (https://artlist.io)
   - 검색: "Korean ambient", "traditional healing"
   - 필터: 1-2분

3. **YouTube Audio Library** (https://www.youtube.com/audiolibrary)
   - 검색: "ambient", "Korean", "healing"
   - 필터: 무료, 저작권 없음

#### 옵션 C: 전문 작곡가 의뢰
- 피버 (https://www.fiverr.com) - 작곡가 검색
- 업워크 (https://www.upwork.com) - 음악 프로듀서 검색
- 예산: $20-50

---

### 2. 효과음 (SFX) - 3개

#### 2-1. 매치 성공음
**파일명:** `match_success.wav`

**사양:**
- 길이: 0.3-0.5초
- 포맷: WAV
- 음향: 밝고 경쾌한 핑 소리
- 음량: -12dB
- 용도: 매치-3 타일 제거 시

**생성 방법:**

1. **Freesound.org** (https://freesound.org)
   - 검색: "match success", "pop sound", "bright ping"
   - 필터: 0.3-0.5초, CC 라이선스

2. **Zapsplat** (https://www.zapsplat.com)
   - 검색: "success sound", "positive chime"
   - 다운로드 (무료)

3. **AI 효과음 생성**
   - Foley AI (https://www.foleys.ai)
   - 프롬프트: "bright ping sound, 0.5 seconds"

---

#### 2-2. 버튼 클릭음
**파일명:** `button_click.wav`

**사양:**
- 길이: 0.2-0.3초
- 포맷: WAV
- 음향: 부드러운 클릭 소리
- 음량: -15dB
- 용도: 모든 버튼 클릭 시

**생성 방법:**

1. **Freesound.org**
   - 검색: "button click", "soft click", "ui click"
   - 필터: 0.2-0.3초, CC 라이선스

2. **Zapsplat**
   - 검색: "button click", "interface sound"
   - 다운로드 (무료)

3. **Unity Asset Store**
   - 검색: "UI Sounds"
   - 무료 패키지 다운로드

---

#### 2-3. 보상 획득음
**파일명:** `reward_get.wav`

**사양:**
- 길이: 0.5-0.8초
- 포맷: WAV
- 음향: 밝고 긍정적인 소리 (예: 종소리, 별 획득음)
- 음량: -10dB
- 용도: 별, 코인 획득 시

**생성 방법:**

1. **Freesound.org**
   - 검색: "reward", "coin", "success chime", "positive sound"
   - 필터: 0.5-0.8초, CC 라이선스

2. **Zapsplat**
   - 검색: "reward sound", "achievement", "coin collect"
   - 다운로드 (무료)

3. **AI 효과음 생성**
   - Foley AI
   - 프롬프트: "bright reward sound, positive, 0.7 seconds"

---

### 3. 고양이 울음소리 (이미 있음)

**파일명:** `Cat_Meow_Sound.wav`
**상태:** ✅ 이미 준비됨 (`/home/ubuntu/WhiskerTales/Cat_Meow_Sound.wav`)
**용도:** 고양이 상호작용 시

---

## 📁 파일 저장 위치

모든 사운드 파일을 다음 위치에 저장하세요:

```
WhiskerTales/
└── Assets/
    └── Resources/
        └── Sounds/
            ├── hanok_ambient_bgm.mp3 (또는 .wav)
            ├── match_success.wav
            ├── button_click.wav
            ├── reward_get.wav
            └── cat_meow.wav (기존 파일)
```

**주의:** `Resources/Sounds/` 폴더가 없으면 만들어야 합니다.

---

## 🔧 Unity에서 임포트 설정

### 1. 오디오 파일 임포트 설정

1. Unity Editor에서 사운드 파일 선택
2. Inspector에서 다음 설정 확인:
   - **Audio Format:** Compressed (MP3) 또는 PCM (WAV)
   - **Sample Rate:** 44100 Hz
   - **Channels:** Stereo 또는 Mono
   - **Load Type:** Decompress On Load (작은 파일) 또는 Streaming (큰 파일)

### 2. AudioManager에 클립 할당

1. Scene에서 AudioManager 오브젝트 선택
2. Inspector에서 AudioManager 컴포넌트 찾기
3. 각 필드에 해당 오디오 클립 드래그 & 드롭:
   - **BGM Clip:** `hanok_ambient_bgm`
   - **Match Success Clip:** `match_success`
   - **Button Click Clip:** `button_click`
   - **Reward Get Clip:** `reward_get`
   - **Cat Meow Clip:** `cat_meow`

---

## 📊 사운드 파일 체크리스트

### 배경음악:
- [ ] `hanok_ambient_bgm.mp3` 또는 `.wav` 다운로드/생성
- [ ] 길이 확인 (1-2분)
- [ ] 음질 확인 (128-192 kbps 이상)
- [ ] `Assets/Resources/Sounds/` 폴더에 저장
- [ ] Unity에서 임포트 확인

### 효과음 - 매치 성공음:
- [ ] `match_success.wav` 다운로드/생성
- [ ] 길이 확인 (0.3-0.5초)
- [ ] 음질 확인 (44.1kHz 16-bit)
- [ ] `Assets/Resources/Sounds/` 폴더에 저장
- [ ] Unity에서 임포트 확인

### 효과음 - 버튼 클릭음:
- [ ] `button_click.wav` 다운로드/생성
- [ ] 길이 확인 (0.2-0.3초)
- [ ] 음질 확인 (44.1kHz 16-bit)
- [ ] `Assets/Resources/Sounds/` 폴더에 저장
- [ ] Unity에서 임포트 확인

### 효과음 - 보상 획득음:
- [ ] `reward_get.wav` 다운로드/생성
- [ ] 길이 확인 (0.5-0.8초)
- [ ] 음질 확인 (44.1kHz 16-bit)
- [ ] `Assets/Resources/Sounds/` 폴더에 저장
- [ ] Unity에서 임포트 확인

### 고양이 울음소리:
- [ ] `cat_meow.wav` 확인 (이미 있음)
- [ ] `Assets/Resources/Sounds/` 폴더에 저장
- [ ] Unity에서 임포트 확인

---

## 🎯 추천 사운드 소스

### 무료 사운드 라이브러리 (추천 순서):

1. **Freesound.org** (https://freesound.org)
   - 장점: 무료, 다양한 효과음, CC 라이선스
   - 단점: 라이선스 확인 필요

2. **Zapsplat** (https://www.zapsplat.com)
   - 장점: 무료, 게임 효과음 많음, 라이선스 명확
   - 단점: 한정된 선택지

3. **YouTube Audio Library** (https://www.youtube.com/audiolibrary)
   - 장점: 무료, 배경음악 풍부, 저작권 없음
   - 단점: 게임 효과음 적음

### 유료 사운드 라이브러리:

1. **Epidemic Sound** (https://www.epidemicsound.com)
   - 가격: $9.99/월 ~ $99.99/년
   - 장점: 고품질, 무제한 다운로드

2. **Artlist** (https://artlist.io)
   - 가격: $14.99/월 ~ $179/년
   - 장점: 고품질, 음악 + 효과음

3. **Unity Asset Store** (https://assetstore.unity.com)
   - 가격: 무료 ~ $50+
   - 장점: Unity 통합, 게임 최적화

---

## 💡 팁

1. **라이선스 확인:** 상업용 게임에 사용하려면 반드시 라이선스 확인
2. **음질:** 게임에는 128 kbps 이상의 음질 권장
3. **길이:** 배경음악은 반복 재생하므로 1-2분 정도가 적당
4. **음량 정규화:** 모든 사운드를 -3dB ~ -6dB 범위로 정규화
5. **테스트:** 게임에서 재생해서 음질과 타이밍 확인

---

## 🚀 다음 단계

1. 위의 사운드 파일 준비
2. `Assets/Resources/Sounds/` 폴더에 저장
3. Unity에서 임포트 및 설정
4. AudioManager에 클립 할당
5. 게임에서 테스트

---

**작성자:** Manus AI
**최종 수정:** 2026년 5월 4일
