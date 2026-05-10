# 🐾 Whisker Tales (냥이의 집) — Master Briefing v4.0

> **이 문서 하나로** 새 노부장(Claude) / 새 노실장(Claude Code) / 새 협업자가 프로젝트의 철학·디자인·기술·현황·다음 할 일을 5분 안에 완벽히 파악할 수 있도록 구성.
>
> **v4.0 핵심 변경 (vs v3.1):**
> - v3.0 전체 디자인 콘텐츠 보존 + v3.1 Phase 로드맵 통합
> - 2026-05-10 개발 세션 전 결과 반영 (Phase 0→A+B 모두 완료/검증)
> - 채과장 합류 + 4가지 코드 수정 패턴 정리
> - 채과장 현실 평가 수치 + 핵심 조언 7항목 추가
> - 절대 원칙 (개발 규칙 포함 — brace 규칙, namespace, LogCategory, GameConstants)
> - DOTween 설치 방식 확정 (.unitypackage 직접 + WHISKER_DOTWEEN define)
>
> **작성일:** 2026-05-10 (노부장 Claude + 노실장 Claude Code 공동 정리)
> **이전 버전:** v3.1 (2026-05-10 오전), v3.0 (2026-05-08), v2.1 (2026-05-07)
> **다음 갱신:** Phase A+B 화면 컨트롤러 14개 완료 후 또는 출시 1주 전

---

## 📑 목차

```
PART A — 프로젝트 정체성
  §1.  Executive Brief (60초 요약)
  §2.  디지털 디톡스 철학 — "쉬어도 괜찮다고 말해주는 게임"
  §3.  슬로건 & 핵심 카피
  §4.  경쟁사 대비 포지셔닝

PART B — 캐노니컬 결정 & 팀
  §5.  캐노니컬 결정사항 22개
  §6.  팀 구성 (대표/노부장/노실장/채과장/채집사/마대리)
  §7.  채과장 코드 4가지 수정 패턴 (필독)

PART C — 게임 디자인
  §8.  핵심 루프 흐름도
  §9.  매치-3 시스템 + 채과장 레벨 권고
  §10. 카페 복원 15단계 + 감정 연출
  §11. 고양이 5마리 상세 (특징/색상/목걸이/사연)
  §12. 호감도 5단계 + 단골 손님 5명
  §13. 경제 시스템 (멸치/실타래/💝)
  §14. IAP 패키지 "집사의 정성"
  §15. 아이템샵 (코스튬/이름표/카페인테리어/간식)
  §16. Nyang TV (선택형 광고)
  §17. 방치형 보상 + 수면 모드
  §18. 레퍼럴 시스템
  §19. 디톡스 모드 + 챌린지

PART D — 사운드 / 에셋 / UI
  §20. 사운드 시스템 + 보유 파일 + 미확보 소싱
  §21. 에셋 인벤토리 (채집사 수집 완료)
  §22. UI 스펙 16개 화면 + 글로벌 시스템 7개

PART E — 기술 현황 (2026-05-10)
  §23. 빌드 상태 + 채과장 현실 평가 수치
  §24. 코드 인벤토리 (Phase별)
  §25. Strangler Fig 재건 — Phase 0~12 완료 상태
  §26. Phase A+B (UI Foundation) 적용 현황
  §27. 알려진 문제 / 블로커
  §28. 절대 원칙 (개발 규칙)

PART F — 다음 할 일
  §29. 우선순위 (즉시 / 1주 / 1개월)
  §30. v1.0 출시 탑재 콘텐츠
  §31. v1.1 / v1.2 / v2.0 로드맵
  §32. 채과장 핵심 조언 7항목

PART G — 마케팅 & 비전
  §33. 냥사일런스 캠페인
  §34. 인스타그램 운영
  §35. 글로벌 전략 (인도 우선)
  §36. KPI & 광고 예산 + ASO
  §37. 장기 비전 — 유니버스 / 냥스타그램 / O2O
  §38. 시상 목표 (Mobile Games Awards / TGA / BAFTA)

PART H — 자산 & 링크
  §39. 핵심 링크 (GitHub/랜딩/개인정보)
  §40. 핵심 문서 위치
```

---

# PART A — 프로젝트 정체성

## §1. Executive Brief (60초 요약)

**무엇:** 한국 한옥 카페를 매치-3 퍼즐로 복원하면서 길고양이를 구조하는 힐링 모바일 게임. **세계 최초 "디지털 디톡스 게임"** — 게임이 플레이어에게 *"그만하고 진짜 휴식을 취하라"*고 말하는 역발상 컨셉.

**왜 특별한가:** 매치-3 + 한옥 + 고양이 수집 + 디톡스 + 방치형 + 레퍼럴 조합은 시장에 **유일**.

**개발 체제:** Nyang Studio (1인 개발) + 노부장(Claude) + 노실장(Claude Code) + 채과장/채집사(ChatGPT) + 마대리(Manus).

**핵심 정보:**
- **GitHub:** https://github.com/jammiejammmie/whisker-tales
- **인스타:** @whisker_tales_official (활성)
- **패키지명:** `com.nyangstudio.whiskertales`
- **Unity:** 2022.3.62f3
- **빌드 경로:** `C:\Builds\WhiskerTales.apk`
- **랜딩페이지:** https://whiskertales-mwjyt48n.manus.space
- **개인정보처리방침:** https://whiskertales-mwjyt48n.manus.space/privacy-policy
- **마케팅 예산:** $500 (출시 후 2주 집중)

**가장 중요한 한 가지:** 2026-05-10 기준 **Strangler Fig 재건 Phase 0~12 + Phase A+B Foundation 모두 완료 / 11/11 테스트 PASS**. 다음 단계는 16개 화면 컨트롤러 중 14개 완성 + 레벨 디자인 80~120개 + 퍼즐 feel 완성.

---

## §2. 디지털 디톡스 철학 — "쉬어도 괜찮다고 말해주는 게임"

### 2.1 한 문장 비전
> **"즐겁게 성취하되, 멈춰야 할 때를 알려주는 사려 깊은 친구 같은 게임"**

### 2.2 사용자가 직접 쓴 말들 (철학의 원천)

| 발화 | 의미 |
|---|---|
| *"쉬기 위한 게임인 거 맞지 우리가 만든 것! ㅎㅎ"* | 브랜드 정체성의 결정적 발화 |
| *"유니버스를 만들 수 있을 것 같은 느낌이 들어 ㅎㅎ"* | 단일 게임이 아닌 IP 비전 |
| *"긴장감의 연속, 잠시 눈을 가리고 반응 기다리는 두려움"* | 출시 전 심리 — 시각화 능력 |
| *"철학을 갖고 개발을 시작했어"* | 기능 이전에 존재 이유가 있음 |
| *"커뮤니티 운영은 안 하고 싶어!"* | 자동화 지향 |

### 2.3 4대 디자인 원칙
1. **Serenity First** — 모든 요소는 평온과 치유를 환기
2. **Cultural Authenticity** — 진정성 있는 한국 디자인, 클리셰 없이
3. **Emotional Connection** — 고양이와 즉각적 유대감
4. **Digital Detox Message** — "잠시 쉬세요" 메시지를 곳곳에

### 2.4 디자인 만트라
> *"한옥이 손님을 맞이하듯, 게임이 플레이어를 안아주는 느낌"*

### 2.5 첫 10분 안에 유저가 느껴야 할 것
- **0~30초:** "어? 이 게임 분위기 다르다" (한옥 BGM + 고양이 골골송)
- **1~3분:** 첫 매치 → 기쁜 "야옹야옹~" → "퍼즐 풀고 고양이 도와주는 거구나"
- **3~5분:** 첫 고양이 구조 → 할머니 편지 회상 → "사연 있는 게임이네"
- **5~10분:** "막혀도 괜찮아요. 고양이가 기다려줄게요." 메시지 → "와, 다른 게임이랑 다르다"

### 2.6 시각 디자인 톤
- **컬러:** 나무톤 #8B7355 / 한지 크림 #F5F1E8 / 따뜻한 코랄 #E8A87C / 차콜 #2C2C2C
- **폰트:** Noto Serif KR (헤딩) + Noto Sans KR (본문)
- **애니메이션:** 300-500ms, ease-in-out
- **시그니처 요소:** 한옥 지붕선 디바이더, 캣 발자국 패턴, 한지 텍스처, 따뜻한 글로우

---

## §3. 슬로건 & 핵심 카피

### 3.1 슬로건 (확정)

**메인 슬로건:**
> **"비 오는 한옥 툇마루, 고양이와 함께 머무는 쉼표. [Whisker Tales]"**

**서브 카피:**
> **"한옥을 복원하는 즐거운 퍼즐과 진짜 휴식의 시작."**

**임팩트형 (인스타/광고용):**
> *"세계 최초, 게임을 끄라고 말하는 디지털 디톡스 게임."*

**영어권:**
> *"The first game that tells you to STOP playing for your own healing."*

**경쟁사 대비:**
> *"강요 없이, 쉬면 보상받는 게임"*
> *"폰을 내려놓을수록 더 좋아지는 게임"*
> *"막혀도 괜찮아요. 고양이가 기다려줄게요."*

**슬로건 원칙:** 메인 슬로건에 *"퍼즐"* 단어 금지 (브랜딩 차별화 — 사용자 직접 결정).

### 3.2 게임 내 카피 모음

**로딩/대기 화면:**
- "오늘 당신의 시간은 어떤 빛깔이었나요?"
- "당신의 속도에 맞춰 곁을 지킬게요"
- "따스한 온기가 머무는 곳"

**디톡스 메시지:**
- "고양이가 잠들었어요. 당신도 쉬어가세요 🐾"
- "잠시 폰을 내려놓고, 진짜 당신의 삶을 돌보세요."

**게임플레이 피드백:**
- "나이스!", "최고예요!", "고양이가 좋아해요!"
- "막혀도 괜찮아요. 고양이가 당신 곁에 있으니까요."

**Nyang TV:**
- "Would you like to watch Nyang TV for a little help?"
- "고양이가 TV 보다 잠드는 동안, 당신도 쉬어가세요 🐾"

### 3.3 영어권 핵심 문구
- "Welcome home. We've been waiting for you."
- "You did great today. Take a deep breath."
- "You're not alone anymore."
- "It's okay to get stuck. Your cat is right here with you."

---

## §4. 경쟁사 대비 포지셔닝

### 4.1 직접 비교 (로얄매치 vs 우리)

| 항목 | 로얄매치 | Whisker Tales |
|---|---|---|
| 강제 광고 | 없음 | 없음 (Nyang TV는 자발적) |
| 하트 시스템 | "막혔죠? 돈 내세요" | "막혔어요? 쉬어가세요 🐾" |
| 과금 감정 | 고통 탈출 → 찜찜함 | 갖고 싶어서 → 기분 좋음 |
| 디톡스 | 없음 | 쉬면 오히려 보상 |

### 4.2 다른 게임 분석

| 경쟁작 | 평점 | 우리와의 차이 |
|---|---|---|
| Cat Crunch | 4.8 (39.6K) | 고양이 캐릭터 아님, 교감 없음 |
| Starry Whiskers | 4.7 (56.9K) | 한옥 없음, 개별 사연 없음 |
| Furistas Cat Cafe | - | 퍼즐 없음 |
| Cat Cafe Manager | - | PC 전용 |

**우리 게임만의 조합:** 매치-3 + 한옥 + 고양이 수집 + 개별 사연 + 방치형 + 디톡스 = 시장에 없음.

---

# PART B — 캐노니컬 결정 & 팀

## §5. 캐노니컬 결정사항 22개

| # | 항목 | 확정값 | 결정일 |
|---|---|---|---|
| 1 | 영문 게임명 | **Whisker Tales** | 2026-05-01 |
| 2 | 한글 게임명 | **냥이의 집** | 2026-05-01 |
| 3 | 개발사명 | **Nyang Studio** | 2026-05-04 |
| 4 | 패키지명 | `com.nyangstudio.whiskertales` | 2026-05-05 |
| 5 | 첫 출시 플랫폼 | Android 우선, iOS 추후 | 2026-04 |
| 6 | 마케팅 예산 | **$500**, 출시 후 2주 집중 | 2026-05-01 |
| 7 | 광고 정책 | **선택형 광고만, 강제 광고 절대 금지** | 2026-05-01 |
| 8 | Unity 프로젝트 | 기존 프로젝트 활용 (재생성 X) | 2026-05-04 |
| 9 | 인스타 계정 | `@whisker_tales_official` (활성) | 2026-05-03 |
| 10 | 인스타 프로필 | "냥이의 집" 등불 + 담벼락 빼꼼 고양이 | 2026-05-04 |
| 11 | 개발자 이름 | Nyang Studio | 2026-05-04 |
| 12 | 협업 역할 | **사용자 = 최종 / 노부장 = 중계 / 노실장 = 코드 / 채과장 = 설계** | 2026-05-07 |
| 13 | 메시지 중계 | 사용자가 양쪽 복붙 중계 | 2026-05-07 |
| 14 | Claude 운영 원칙 | 자발적 캡처·문서화, 갈림길만 피드백 요청 | 2026-05-07 |
| 15 | 플레이어 성별 | 중립 ("나"로 표현) | 2026-05-07 |
| 16 | 카페 복원 구조 | 3구역 × 5단계 = 15배경 | 2026-05-07 |
| 17 | 경쟁 레퍼런스 기준 | 분야별 벤치마크 게임 지정 | 2026-05-07 |
| 18 | 고양이 성장 시스템 | 카페 복원 진행도 연동, v1.1 예정 | 2026-05-08 |
| 19 | 아트 스타일 | **수채화 → 3D 렌더링 전면 전환** | 2026-05-08 |
| 20 | 고양이 확정 이름 | **사미 / 벨라 / 나비 / 구름이 / 호두** | 2026-05-08 |
| 21 | 고양이 생성 툴 | **ChatGPT GPT-4o + DALL-E 3 (마대리는 고양이 못 만듦)** | 2026-05-08 |
| 22 | 이미지 퀄리티 기준 | **로얄매치 미만 반려** | 2026-05-08 |

### 5.1 v3.0 이후 추가 결정 (오늘 확정)

| 항목 | 확정값 | 결정일 |
|---|---|---|
| 개발 전략 | **Strangler Fig 방식** (기존 유지하면서 내부부터 점진 교체) | 2026-05-10 |
| Phase 로드맵 | Phase 0 baseline → 1 Stabilization → 2 Adapter → 3 Drag → 4-12 Systems → A+B UI → AppBootstrap 분해(마지막) | 2026-05-10 |
| 채과장 합류 | ChatGPT가 코드 설계 / UI 스펙 / 이미지 생성 담당 | 2026-05-10 |
| DOTween 설치 | **.unitypackage 직접 임포트** (`Assets/Plugins/Demigiant/DOTween/`) — OpenUPM 미존재 | 2026-05-10 |
| Scripting Define | `WHISKER_DOTWEEN`, `DOTWEEN` 모든 플랫폼 추가 | 2026-05-10 |
| `.gitignore` | `Assets/WhiskerTales/`, `Assets/Plugins/` `.meta` whitelist 추가 | 2026-05-10 |
| 코드 스타일 규칙 | **모든 if/else/else if는 한 줄이어도 `{ }` 필수** | 2026-05-10 |
| 베이스라인 브랜치 | 큰 작업 전 항상 `baseline-YYYYMMDD` 새로 생성 | 2026-05-10 |

---

## §6. 팀 구성 (v4.0)

| 역할 | 담당 | 책임 | 잘하는 것 / 못하는 것 |
|---|---|---|---|
| **대표** | 지원님 (사용자) | 비전 / 방향 / 최종 판단 / 갈림길 결정 | — |
| **노부장** | Claude (텍스트 채팅) | 중계 / 명세 작성 / 검수 / 마대리 지시서 / 큰 그림 정리 | 명세·문서·검증·전략 / Unity 빌드는 노실장에 위임 |
| **노실장** | Claude Code (CLI) | 코드 구현 / 빌드 / Git 작업 / Unity batchmode 검증 | Unity batch / Git / 코드 편집·디버깅 / 새 마스터 브리핑 같은 큰 문서는 노부장 영역 |
| **채과장** | ChatGPT (코드 설계 채널) | 코드 설계 / UI 스펙 / 매치-3 알고리즘 설계 / 이미지 프롬프트 | 알고리즘·아키텍처·미감 좋은 코드 / 본 프로젝트 정확한 식별자/네임스페이스 모름 — §7 참조 |
| **채집사** | ChatGPT (이미지 채널) | 고양이 이미지 + 게임 자산 이미지 전담 | DALL-E 3 / 한옥 / 고양이 / 마대리는 못 만드는 영역 |
| **마대리** | Manus | 웹 / 마케팅 카피 / 문서 자산 (이미지·GitHub 제외) | 카피·기획 / 자산 생성 (고양이 제외) — 이미지/GitHub 직접 작업 불가 |

### 6.1 협업 운영 원칙
- 새 결정/아이디어 → 즉시 캡처 → 마스터 브리핑 갱신
- 갈림길 / 큰 결정 / 비용 투자 → 사용자 피드백 요청
- 마대리 세션 새로 열 때 → 마스터 브리핑 + 온보딩 패키지 첨부

### 6.2 사용자 자율 운영 철학
> *"커뮤니티 운영은 안 하고 싶어!"*
> *"인스타보다 게임개발 완료가 우선이지!"*
> *"퇴근 후에는 컴퓨터를 할 수가 없는데"*

→ **직접 운영 최소화, 자동화 최대화.** Claude/Manus가 최대한 자율적으로 처리, 사용자에겐 *"승인만"* 요청하는 구조가 이상적.

---

## §7. 채과장 코드 4가지 수정 패턴 (필독)

채과장이 보내주는 코드는 알고리즘은 우수하지만 본 프로젝트의 **정확한 식별자 / 네임스페이스를 모름**. 노실장은 적용 전 항상 다음 4가지를 점검:

### 패턴 1 — `using WhiskerTales.Core;` 누락
- **증상:** `DebugLogger`, `LogCategory`, `GameEvents`, `GameConstants` 미해결 컴파일 에러
- **원인:** 이들은 모두 `WhiskerTales.Core` 네임스페이스 안에 있음
- **고침:** 파일 상단에 `using WhiskerTales.Core;` 추가

### 패턴 2 — `DebugCategory` 오타
- **증상:** `DebugCategory.Puzzle` 등 미해결
- **원인:** 실제 enum 이름은 `LogCategory`
- **고침:** `DebugCategory` → `LogCategory`로 일괄 치환

### 패턴 3 — `GameConstants.BoardSize` 잘못된 경로
- **증상:** `GameConstants.BoardSize` 미해결
- **원인:** 실제 경로는 nested class — `GameConstants.Board.Size`
- **고침:** `GameConstants.BoardSize` → `GameConstants.Board.Size`. 다른 카테고리도 마찬가지: `GameConstants.Timing.{...}`, `GameConstants.Currency.{...}` 등.

### 패턴 4 — brace 없는 한 줄 if (사용자 규칙 위반)
- **증상:** `if (cond) doThing();` 또는 `if (cond)\n    doThing();`
- **원인:** 채과장은 brace 없는 한 줄 스타일 자주 사용
- **고침:** 프로젝트 규칙상 **모든 if/else는 `{ }` 필수** — 한 줄도 예외 없이 brace.
- **이유:** Phase 1에서 `Board.cs:211-213`이 brace 누락으로 if 외부에 다음 줄이 빠지면서 `RaiseSpecialTileCreated`가 잘못 발화하는 버그 발생. 들여쓰기와 실제 블록 범위가 어긋나는 패턴 자체를 차단.

### 추가 패턴 — DOTween 체인 호출
- **증상:** `DG.Tweening.ShortcutExtensions.DOScale(...).SetEase(...)`에서 `SetEase` 미해결
- **원인:** `SetEase`는 extension method라 `using DG.Tweening;`이 있어야 체인 호출 해결
- **고침:** `#if WHISKER_DOTWEEN using DG.Tweening; #endif` 조건부 추가

---

# PART C — 게임 디자인

## §8. 핵심 루프 흐름도

```
[매치-3 퍼즐 클리어] → [⭐별 + 💰멸치 획득]
        ↓                     ↓
[카페 복원 작업 1개 완료] ← [⭐별 소비]
        ↓
[복원 마일스톤 도달] → [새 길고양이 구조 / 사연 해금]
        ↓
[고양이와 교감 → 호감도 상승]
        ↓
[단골 손님 증가] → [추가 멸치 획득]
        ↓
[오프라인 방치 → 재접속 보상] → [🌙수면 모드 → 💝 냥이 마음 적립] → 반복
```

**한 세션 길이 목표:** 5~10분
**일일 감정 루프 (채과장 권고):** 오늘의 차 한 잔 → 오늘의 한마디 → 오늘 고양이 기분

---

## §9. 매치-3 시스템 + 채과장 레벨 권고

### 9.1 보드
- **8×8 그리드**
- **6가지 타일** (생선/우유/털실/캣닢/발도장/생선뼈)
- 코드: `Match3Core.cs` (Phase 2) — 시드 가능, 캐스케이드 + 중력 + 채우기 결정론적

### 9.2 매치 보상 규칙

| 형태 | 결과 | 코드 |
|---|---|---|
| 3매치 | 일반 제거 | `Match3Core.FindAllMatches` |
| 4 직선 (가로) | 🚀 가로 로켓 | `SpecialItemType.RocketHorizontal` |
| 4 직선 (세로) | 🚀 세로 로켓 | `SpecialItemType.RocketVertical` |
| 5개 L/T | 💣 폭탄 | `SpecialItemType.Bomb` (활성화는 `SpecialActivator`) |
| 5 직선 | 🌈 무지개 털실 | `SpecialItemType.Rainbow` |

### 9.3 부스터 상호작용
- 무지개 + 무지개 = **모든 타일 제거**
- 그 외 조합 → 효과 중첩

### 9.4 레벨 곡선 (채과장 권고)

| 구간 | 권장 레벨 | 비고 |
|---|---|---|
| Tutorial | 1~5 | 스토리 연결, 메커닉 1개씩 도입 |
| Easy | 6~10 | 부스터 조합 시작 |
| Normal | 11~30 | 다양한 목표 타입 등장 |
| Hard | 31~50 | 장애물 + 시간 압박 |
| Very Hard | 51~80 | 출시 최소 80레벨 |
| Endgame | 81~120 | **내부 목표 120** (출시 직후 무한 챌린지로 연결) |

> **채과장 핵심 조언:** *출시 시 최소 80개 레벨, 내부 목표 120개.* 50개 미만은 광고 + 인플루언서 시연으로 빠르게 이탈.

### 9.5 장애물 5종 (채과장 권고)

| 장애물 | 특성 | 클리어 조건 |
|---|---|---|
| **Dust** (먼지) | 인접 매치 1회로 제거 | 가장 약함 — 튜토리얼 |
| **TeaCup** (찻잔) | 인접 매치 1~2회로 제거, 깨지는 연출 | 카페 분위기 |
| **Rope** (밧줄) | 인접 매치 2회 / 가로/세로 한 줄 동시 | 한옥 매듭 모티프 |
| **Frozen** (얼음 타일) | 인접 매치 1회로 깨짐, 안에 든 타일 해금 | 시각 임팩트 |
| **Crate** (상자) | 매치로는 안 깨짐, 폭탄/로켓으로만 | 고난이도 |

### 9.6 플레이 감각 — 타이밍/리듬 설계 (채과장 핵심)

> *"퍼즐 게임의 plough = 타이밍과 리듬."* — 채과장
> 모든 액션은 **체감 ms 단위**로 설계:

| 액션 | 시간 | 비고 |
|---|---|---|
| 타일 스왑 (성공) | 0.16s | `GameConstants.Timing.TileSwapSeconds` |
| 타일 스왑 (실패 — 무매치) | 0.12s | `InvalidSwapReturnSeconds` |
| 매치 팝 stagger | 0.04s | `MatchPopStaggerSeconds` |
| 타일 드롭 | 0.22~0.34s | `TileDropMin/MaxSeconds` (랜덤) |
| 새 타일 스폰 | 0.18s | `TileSpawnSeconds` |
| 특수 타일 생성 연출 | 0.25s | `SpecialCreateSeconds` |
| 캐스케이드 간 휴지 | 0.08s | `CascadeDelaySeconds` |
| 레벨 클리어 별 팝 | 0.18s × 별 수 | `LevelClearStarPopSeconds` |
| 힌트 idle 발동 | 5s | `HintIdleSeconds` |

→ **DOTween Sequence**로 모두 `BoardAnimator.cs` (Phase 4)에 통합. WHISKER_DOTWEEN 미설정 시 코루틴 fallback.

---

## §10. 카페 복원 시스템 + 감정 연출

### 10.1 구조 (캐노니컬, `CafeRestorationData.json`)
**3구역 × 5단계 = 15단계**

#### 1구역: 카페 입구/마당 (stage1~5)
낡은 간판 복원(50별) → 담장 정리(75) → 마당 청소(100) → 화단 꾸미기(125) → 입구 완성(150)

#### 2구역: 카페 실내 (stage1~5)
지붕 기와(200) → 창문(225) → 벽 칠하기(250) → 문 교체(275) → 본채 완성(300)

#### 3구역: 카페 뒷마당/비밀공간 (stage1~5)
연못 청소(350) → 나무 정리(375) → 벤치 설치(400) → 등불 달기(425) → 완성(450)

### 10.2 카페 복원 감정 연출 (채과장 핵심 조언)

> *"복원이 끝났을 때 사용자가 울컥해야 한다."*

각 마일스톤 클리어 시 자동 연출:

| 연출 | 트리거 | 효과 |
|---|---|---|
| **Lantern light-up** (등불 점화) | stage1~5 첫 클리어 | 한지 등불 하나씩 켜짐 + 따뜻한 글로우 |
| **Petals drift** (꽃잎 흩날림) | 구역 완성 (stage5) | 벚꽃잎 파티클 (`ParticlePoolManager` Petal) |
| **Camera drift** (카메라 천천히 패닝) | 1/2/3구역 완성 | 0.42s ScreenFade + 카메라 0.8s drift |
| **Cat reveal** (고양이 등장) | 새 고양이 구조 시점 | 실루엣 → 페이드인 → 한 줄 사연 |
| **Daily ambience** (날씨/시간대) | 항상 | morning / afternoon / sunset / rainy 동적 변화 |

### 10.3 Dynamic Ambience (채과장 권고)

> *"같은 한옥이라도 시간대마다 다르게 느껴져야 한다."*

| 시간대/날씨 | 배경 톤 | 사운드 |
|---|---|---|
| Morning (06~10) | 푸른 새벽 + 햇살 시작 | 새소리 + 물 끓는 소리 |
| Afternoon (11~16) | 밝은 햇살, 선명 | 풍경 소리 + 매미(여름) |
| Sunset (17~19) | 따뜻한 오렌지/핑크 조명 | 잔잔한 BGM + 멀리서 짖는 개 |
| Rainy (날씨 API) | 흐림 + 빗방울 파티클 | 빗소리 ASMR |

→ v1.1에서 GPS 날씨 API 연동 (`OpenWeatherMap` 무료 티어).

---

## §11. 고양이 5마리 상세

### 11.1 확정 5마리 (3D 렌더링, 2026-05-08 확정)

| ID | 이름 | 영문 | 품종 / 특징 | 색상 테마 | 목걸이 / 장신구 | 시그니처 사운드 |
|---|---|---|---|---|---|---|
| 1 | **사미** | Sami | 시암 / 호기심 많은 탐험가 / 파란 눈 | 갈색 포인트 | 방울 목걸이 (은색) | "므야아~" (길고 낮게) |
| 2 | **벨라** | Bella | 흰털 / 우아한 공주님 / 초록 눈 | 흰색 | 핑크 리본 | "먀~" (짧고 우아) |
| 3 | **나비** | Nabi | 삼색 / 발랄한 장난꾸러기 | 오렌지/흰/검 | 초록 방울 | "냥!" (빠르고 높게) |
| 4 | **구름이** | Gurumi | 회색 / 조용한 꿈꾸는 자 | 회색 | 파란 스카프 | "음냥..." (작고 조용히) |
| 5 | **호두** | Hodu | 태비 / 친근한 개구쟁이 | 갈색 줄무늬 | 노란 스카프 | "야옹!" (굵고 친근) |

**레퍼런스 마스터:** 방석 위 5마리 이미지 (ChatGPT GPT-4o + DALL-E 3 생성)
**프롬프트 보존:** `TODAY_SESSION_0508.md`

### 11.2 고양이 사연 구조 (구조 시 해금)

| 고양이 | 성격 키워드 | 구조 사연 키워드 |
|---|---|---|
| 나비 | 소심하지만 정이 많음 | 비 오는 날 시장 골목 구조, 할머니 간호 |
| 호두 | 장난기 많고 호기심 강함 | 카페 몰래 들어왔다 할머니께 들킴 |
| 구름이 | 온순하고 조용함, 손길 좋아함 | 부잣집에서 버려짐, 할머니가 구조 |
| 사미 | 활발하고 독립적, 사냥 본능 강함 | 베테랑 길고양이, 겨울에 카페로 들어옴 |
| 벨라 | 도도하고 우아, 관심 즐김 | 고양이 미인대회 출신, 나이 들어 버려짐 |

### 11.3 고양이 성장 시스템 (v1.1)
- **아기 → 청소년 → 어른** (카페 복원 진행도와 연동)
- 각 단계마다 외형 변화 + 새로운 사연 공개
- 과금 포인트: 성장 단계별 코스튬 (한복/리본/왕관)

### 11.4 추후 확장 (v2.0+)
**총 15마리** = Common(5 — 출시 5마리) + Uncommon(5) + Rare(3) + Legendary(2)

---

## §12. 호감도 5단계 + 단골 손님 5명

### 12.1 호감도 시스템

| 레벨 | 필요 호감도 | 명칭 | 보상 |
|---|---|---|---|
| Lv 1 | 0 | Stranger | 초기 |
| Lv 2 | 100 | — | 코인 100 + 새 대사 |
| Lv 3 | 250 | Friend | 보석 10 + 특별 가구 |
| Lv 4 | 500 | Best Friend | 보석 20 + 레어 아이템 |
| Lv 5 | 1000 | **Family** | 보석 50 + 유니크 코스튬 + 특별 스토리 |

### 12.2 호감도 획득 방법
- **쓰다듬기 (Pet)** — 핑크 버튼 / 짧은 골골송
- **간식 주기 (Give Treat)** — 노랑 버튼 / 멸치 소비
- **놀아주기 (Play)** — 초록 버튼 / 낚싯대/털실

### 12.3 일일 로테이션 보너스
"매일 다른 방법으로 교감하면 더 많은 보상"
→ 하루에 쓰다듬기만 반복하면 보상↓, 3가지 다 쓰면 보너스↑
→ **매일 접속 동기 강화**

### 12.4 단골 손님 5명 (구→신 매핑)

| 손님 | 좋아하는 고양이 (신버전) | 보너스 |
|---|---|---|
| 미라 | 사미 | +10% 코인 |
| 준호 | 호두 | +10% |
| 지은 | 나비 | +10% |
| 철수 | 구름이 | +10% |
| 영희 | 벨라 | +10% |

⚠️ JSON에 신 이름으로 매핑 갱신 필요 (`CatDexData.json`).

---

## §13. 경제 시스템 (멸치 / 무지개 실타래 / 💝)

### 13.1 화폐 3종

| 종류 | 명칭 | 획득 | 용도 | IAP |
|---|---|---|---|---|
| 소프트 | **멸치** 🐟 | 퍼즐 클리어, 교감, Nyang TV | 가구, 간식, 일반 아이템 | ✅ 구매 가능 |
| 하드 | **무지개 실타래** 🧶 | IAP, 업적 | 프리미엄 가구, 즉시완료, 강력 부스터 | ✅ 메인 IAP |
| 디톡스 | **냥이 마음** 💝 | **수면 모드 / 디톡스 미션 전용** | 한정판 코스튬 / 카페 복원 가속 (소량) / 고양이 성장 부스터 | ❌ **절대 IAP 불가** |

### 13.2 냥이 마음 💝 — 세상에 없는 디톡스 BM

> **핵심 철학:** *"폰 내려놓을수록 게임이 더 풍성해진다"*

| 디톡스 미션 | 냥이 마음 지급 |
|---|---|
| 산책하기 (20분) | 💝 3 |
| 책 읽기 (30분) | 💝 5 |
| 차 마시기 (15분) | 💝 2 |
| 낮잠 자기 (1시간) | 💝 8 |
| 수면 모드 (8시간) | 💝 10 |
| 매일 연속 디톡스 보너스 | 💝 +3 |

- **일일 캡:** 30 (`GameConstants.Economy.DailyNyangiHeartCap`)
- **자정(KST) 리셋:** `CurrencyManager.CheckMidnightReset()`
- **코드:** `CurrencyManager.cs` — `TryAwardNyangiHeart` / `TrySpendNyangiHeart` / Editor-only `DebugResetDailyGained`

### 13.3 마케팅 포인트
> *"이 게임은 폰을 내려놓을수록 더 좋은 아이템을 줍니다."*
→ 앱스토어 리뷰 / 인스타 / 바이럴의 핵심 소재.

---

## §14. IAP 패키지 "집사의 정성"

| 상품 | 구성 | 가격 |
|---|---|---|
| 첫 만남 패키지 | 실타래 50 + 하트 무제한(2h) + 멸치 2,000 | $0.99 (계정당 1회) |
| 한옥 단장 세트 | 실타래 200 + 한정판 자개장 | $4.99 |
| 집사 패스 (시즌) | 월간 구독, 풍성한 보상 + 전용 고양이 | 월 2,000~3,000원 |

### 14.1 BM 시뮬레이션 핵심 수치

| 유저 구분 | 일일 멸치 획득 | 가구 구매 주기 |
|---|---|---|
| 라이트 (30분/일) | 1,500~2,000 | 2~3일당 1개 |
| 코어 (2시간/일) | 5,000~7,000 | 1일당 1~2개 |
| 과금 (패키지) | 10,000+ | 즉시 방 하나 완공 |

**광고 수익 예상 비중:** 전체 매출의 40~50%
**첫 만남 패키지 ($0.99):** 유저 10% 이상 구매 전환 목표
**무과금 검증:** 결제 없이도 모든 고양이 구조 + 한옥 전체 복원 가능 ✅

### 14.2 절대 원칙
- ❌ "이동 횟수 막고 돈 내야 진행" — **절대 금지**
- ❌ 강제 전면 광고 — **절대 금지**
- ❌ 냥이 마음 💝 IAP — **코드 레벨에서 차단** (`AwardNyangiHeartFromIAP`은 `[Obsolete(true)]`)

> 철학: *"내 고양이 사랑하니까 기꺼이 쓰는 구조"* — 착취가 아닌 애정의 표현.

---

## §15. 아이템샵 (코스튬 / 이름표 / 카페인테리어 / 간식)

### 15.1 이름표 커스터마이징 🆕
- **1회 무료 변경** (최초 설정)
- **이후 변경:** 무지개 실타래 50
- **모양:** 하트 / 발바닥 / 별 / 꽃 / 뼈다귀
- **색상:** 금 / 은 / 로즈골드 / 크리스탈 / 무지개
- **폰트:** 귀여운체 / 클래식체 / 손글씨체

### 15.2 코스튬 — 글로벌 전통 의상 시리즈

| 지역 | 코스튬 | 시즌 |
|---|---|---|
| 🇰🇷 한국 | 한복, 갓, 도포 | 설날/추석 |
| 🇮🇳 인도 | 사리, 레헹가, 빈디 장식 | 디왈리 |
| 🇯🇵 일본 | 유카타, 기모노 | 여름/신년 |
| 🇸🇦 중동 | 카프탄, 히잡 장식 | 라마단 |
| 🇺🇸 서구 | 할로윈, 크리스마스 | 시즌 |
| 🇨🇳 중국 | 치파오, 탕장 | 춘절 |
| 🇹🇭 태국 | 전통 의상 | 송크란 |

### 15.3 코스튬 카테고리

**의상류:**
한복(설빔/추석빔) / 후드(곰돌이/토끼귀) / 망토(크리스마스/할로윈) / 잠옷 / 수영복

**악세서리류:**
리본 / 나비넥타이 / 왕관 / 티아라 / 꽃관(봄) / 선글라스 / 스카프 / 반다나 / 방울 목걸이(금/은/크리스탈) / 머리핀

### 15.4 카페 인테리어 아이템
캣타워(기본/럭셔리/한옥형) / 방석(비단/왕골) / 숨숨집(항아리형/한옥 처마형) / 스크래쳐 / 도자기 밥그릇 / 청자 물그릇

### 15.5 게임플레이 아이템
**간식:** 멸치(기본) / 참치캔(프리미엄) / 고양이 케이크(이벤트) / 한방 간식(특별 효과)
**장난감:** 낚싯대 / 털실 뭉치 / 레이저 포인터 / 깃털 / 쥐 인형

### 15.6 시즌 한정 세트
개별보다 20% 저렴하게 묶음 판매 (예: "봄 나들이 세트" = 벚꽃 화관 + 연두 한복 + 꽃 방석)

### 15.7 일일 무료 간식
매일 접속 시 간식 1개 무료 → 잔존율 강화

---

## §16. Nyang TV (선택형 광고)

**컨셉:** 한옥 마당 옛날 TV 오브젝트. 고양이가 TV 앞에서 졸린 듯 앉아 있음.

**철칙:**
- ✅ **자발적 시청만** — 유저가 클릭해야 시작
- ✅ 보상형 광고 (멸치 / 부스터 / 무지개 실타래)
- ❌ **강제 광고 절대 없음** (시작 시 / 레벨 사이 / 카페 진입 시 모두)

**카피:** *"Would you like to watch Nyang TV for a little help?"*

**수면 모드 시 광고:**
고양이가 TV 보다 잠든 동안 광고 자동 노출 — 강제 아님 (유저는 자고 있음). 자연스럽고 귀여운 연출.

---

## §17. 방치형 보상 + 수면 모드

### 17.1 방치형 보상
- **ASMR 한옥 휴식**: 새소리 / 빗소리 / 모닥불
- **고양이의 보은**: 일정 시간마다 선물 — 화면 밖에서 뭔가 물고 들어옴
- **오프라인 수익**: 최대 8시간 자동 적립
- **단골 손님**: 5분 간격 자동 방문, 일 5명

### 17.2 수면 모드 상세

**유저 경험:**
- 화면 어둡게 (배터리 절약, brightness 0.2 — `GameConstants.Sleep.TargetBrightness`)
- 고양이 골골송 ASMR 재생 (작은 볼륨)
- 고양이가 TV 보다 잠든 애니메이션
- 수면 중 방치 보상 (멸치/아이템) — 기상 시 수령

**최대 수면 시간:** 8시간 (`GameConstants.Sleep.MaxSleepHours`)

**보상 계산 (`SleepModeManager.CalculateAndApplyRewards`):**
- 멸치 = 시간 비례
- 하트 = 시간당 일정량 (`SleepModeHeartsPerSession = 2`)
- 냥이 마음 💝 = 8시간 풀 수면 시 +10 (`SleepModeNyangiHeartFullReward = 10`)
- 멸치(anchovy) = 시간당 10 (`SleepModeAnchovyPerHour = 10`)
- 친밀도 = 30분당 +1 (`MinutesPerAffinityReward = 30f`)

**개발자가 얻는 것:**
- 앱 체류율 (세션 시간, MAU)
- **고양이 TV 속 광고 노출** ← 핵심 수익 모델
- 철학과 충돌 없는 광고 수익

### 17.3 디톡스 챌린지 (v1.1)

| 미션명 | 내용 | 시간 | 보상 |
|---|---|---|---|
| 냥이와 산책 | 실제 걷기 (양심 버튼) | 5~10분 | 멸치 + 마당 꾸미기 꽃 씨앗 |
| 서재의 독서 | 실제 책 읽기 | 15~30분 | 고양이 지혜 포인트 + 멸치 |
| 차 한 잔의 여유 | 차 마시기 | 10~15분 | 코인 + 호감도 |
| 낮잠 | 실제 낮잠 | 30~60분 | 레어 아이템 |
| 하늘 보기 | 밖에서 하늘 보기 | 5분 | 소량 보상 + 날씨 연동 특별 보상 |

**양심 시스템:** 미션 완료는 100% 유저 양심에 맡김. 강제 검증 없음.
→ *"우리는 당신을 믿습니다"* = 디톡스 철학과 일치.

**핵심 역설:**
> *"다른 게임은 체류율 올리려고 유저를 붙잡는다. 이 게임은 유저를 내보내면서 체류율을 올린다."*

---

## §18. 레퍼럴 시스템

### 18.1 기본 구조
- **8자리 추천 코드** + 딥링크 자동 삽입
- 공유 시 예쁜 카드 이미지 자동 생성
- 조건: 5레벨 클리어 또는 첫 고양이 구조
- 어뷰징 방지: ADID/IP/행동 패턴 분석

### 18.2 스마트 스냅샷 트리거
카페 복원 완료 / 고양이 구조 / 호감도 레벨업 / 특정 레벨 클리어
→ 자동으로 예쁜 공유 카드 생성 + 추천 코드 삽입

### 18.3 딥링크 구조
```
https://catcafe.game/?ref=CATCAFE1
```
Firebase Dynamic Links 활용

### 18.4 보상 조건
초대받은 유저가 **5레벨 클리어 또는 첫 고양이 구조** 시 양쪽 보상 지급

### 18.5 부정행위 방지
- ADID/IDFA 기기 식별
- Google Play Referrer API
- IP/네트워크 패턴 분석
- 행동 패턴 분석 (봇 탐지)
- 일일 보상 횟수 제한
- 블랙리스트 관리

### 18.6 백엔드 로드맵
- v1.0: Google Play Games Save / Apple iCloud
- v2.0+: PlayFab 등 전문 백엔드로 확장

---

## §19. 디톡스 모드 + 챌린지

### 19.1 디톡스 메시지 (v1.0 탑재)
1. 일정 플레이 후 *"고양이가 잠들었어요. 당신도 쉬어가세요 🐾"* 메시지
2. 레벨 클리어 후 간헐적 *"오늘 당신의 시간은 어떤 빛깔이었나요?"* 화면
3. 설정 내 디톡스 모드 토글
4. 발동 확률: 33% (`GameConstants.Detox.ModalProbability`)
5. 발동 주기: 3레벨마다 (`ShowAfterLevels`)

### 19.2 디톡스 메시지 저장소
- `Assets/WhiskerTales/Detox/DetoxMessageRepository.cs` (ScriptableObject)
- `Assets/WhiskerTales/Detox/DetoxMomentService.cs` (타이밍 서비스)

### 19.3 디톡스 완료 후 인스타 공유 (v1.2)
> *"오늘 산책하고 왔어요 🐱 #냥이의집 #디지털디톡스"*
→ 레퍼럴 시스템과 연결. 유저가 스스로 홍보하는 구조.

---

# PART D — 사운드 / 에셋 / UI

## §20. 사운드 시스템

### 20.1 핵심 철학
*"고양이 소리로 된 UI 효과음"* — 세상에 없는 시그니처. 버튼 하나하나에서 고양이가 느껴지는 경험.

### 20.2 고양이 모드 효과음 매핑

| 상황 | 소리 | 파일 |
|---|---|---|
| 버튼 클릭 | 짧은 "냥" | (Freesound 소싱) |
| 타일 매치 | "뿅" + 짧은 골골 | (Freesound 소싱) |
| 콤보 | 고양이 흥분한 "냐~!" | (소싱 필요) |
| 레벨 클리어 | 기쁜 "야옹야옹~" | (소싱 필요) |
| 코인 획득 | 방울 소리 (딸랑~) — 벨라/사미 목방울 연결 | (소싱 필요) |
| 레벨 실패 | 실망한 "으~냥..." | (소싱 필요) |
| 쓰다듬기 | 골골송 짧게 | `cat_purring.wav` ✅ |
| 수면 모드 | 골골송 루프 | `cat_purring.wav` ✅ |

### 20.3 사운드 설정 3가지
```
🔊 일반 모드 — 기본 게임 효과음
🐱 고양이 모드 — 고양이 소리만 (시그니처)
🔇 음소거
```

**코드:** `Feel/AudioService.cs` — `SoundMode { Normal, CatOnly, Muted }`, `SfxId` enum 14종, `BgmId` enum 5종, AudioMixer + 풀링.

### 20.4 보유 사운드 파일 (확보됨 ✅)

| 파일 | 용도 |
|---|---|
| `cat_meow_bella.wav` | 벨라 시그니처 ("먀~") |
| `cat_meow_nabi.wav` | 나비 시그니처 ("냥!") |
| `cat_meow_sami.wav` | 사미 시그니처 ("므야아~") |
| `cat_meow_gureumi.wav` | 구름이 시그니처 ("음냥...") |
| `cat_meow_hodu.wav` | 호두 시그니처 ("야옹!") |
| `cat_purring.wav` | 골골송 (쓰다듬기 / 수면 모드) |
| `hanok_ambient_bgm.wav` | 한옥 BGM |
| `game_sfx_pack.wav` | 게임 일반 효과음 묶음 |
| `Cat_Meow_Sound.wav` | 일반 야옹 |

### 20.5 미확보 사운드 — Freesound.org 소싱 가이드

| 검색어 | 라이선스 | 용도 |
|---|---|---|
| match pop casual | CC0 (무료 상업용) | 타일 매치 |
| level complete cute | CC0 | 레벨 클리어 |
| coin collect soft | CC0 | 코인 획득 |
| button click gentle | CC0 | 버튼 클릭 |
| rocket whoosh casual | CC0 | 가로/세로 로켓 활성화 |
| explosion soft | CC0 | 폭탄 활성화 |
| rainbow chime sparkle | CC0 | 무지개 털실 활성화 |

→ 마대리에게 일괄 소싱 지시 가능. 다운로드 후 `Assets/Resources/Audio/` 배치.

### 20.6 차별점 — 앱스토어 리뷰 단골 소재 예상
> *"버튼 누를 때마다 냥냥거려요 ㅋㅋ 너무 귀여움"*
다른 게임은 크게/작게/음소거만 제공. 우리 게임은 **고양이 소리 모드 = 아이덴티티**.

---

## §21. 에셋 인벤토리

### 21.1 채집사 수집 완료 ✅

| 카테고리 | 항목 | Unity 적용 |
|---|---|---|
| UI 버튼 / 아이콘 / 로고 / 스피너 | ✅ 수집 완료 | 🟡 prefab 연결 대기 |
| 타일 6종 + 특수타일 3종 | ✅ 3D 렌더링 | 🟡 TileSpriteBinder 주입 |
| 배경 15장 (zone1~3 × stage1~5) | ✅ 3D 렌더링 | 🟡 prefab 연결 대기 |
| 야간 배경 5장 (수면 모드용) | ✅ 수집 완료 | 🟡 |
| 오프닝 3장 | ✅ | 🟡 |
| 레벨 클리어 배경 | ✅ | 🟡 |
| NPC 실루엣 5종 | ✅ | 🟡 |
| 앱 아이콘 + 피처드 이미지 | ✅ | ✅ 일부 적용 |
| 스크린샷 5장 | ✅ Final_Screenshot_1~5.png | 🟡 스토어 제출용 |
| 네비게이션 / 버튼 아이콘 세트 | ✅ | 🟡 |
| 팝업 배경 4종 | ✅ | 🟡 |
| 벚꽃 파티클 8종 | ✅ | 🟡 ParticlePoolManager에 주입 |
| 반짝임 / 매치이펙트 / 코인 / 색종이 | ✅ | 🟡 |
| 냥이 하트 아이콘 | ✅ | 🟡 |
| 감정 아이콘 6종 | ✅ | 🟡 |
| 튜토리얼 요소 7종 | ✅ | 🟡 |
| 화살표 2종 | ✅ | 🟡 |

### 21.2 마케팅 자산 ✅
- `Logo_NyangStudio_WhiskerTales_Option3.png` ← **확정**
- `cat_home_marketing_banner.png`
- `nyang_silence_episode_1~3.png` (냥사일런스 캠페인)
- `Post_1~4.png` (인스타 정기 포스팅)
- `Post_2_Wednesday_게임플레이.png` (**타일/매치-3 UI 핵심 레퍼런스**)
- `WhiskerTales_Teaser_Final_With_Logo.mp4` (5.7MB)
- `WhiskerTales_Profile_KR.png`

### 21.3 UI 레퍼런스 (Stage 4 / Phase A+B용)
- `Final_Screenshot_1_Real_Hanok.png` (타이틀/메인)
- `Final_Screenshot_2_Interior.png` (카페 인테리어)
- `Final_Screenshot_3_Garden.png` (뒷마당)
- `Final_Screenshot_4_Social.png` (냥 클럽/소셜)
- `Final_Screenshot_5_Healing.png` (힐링/디톡스)

### 21.4 무시해도 되는 플레이스홀더 ❌
- `WhiskerTales_Android.apk` (72B — 빈 파일)
- `cat_01~15.png` (37B 각)
- `MainScene.unity` (185B)
- `Match3Engine.cs` (160B)
- 구버전 일러스트 고양이 (`cat_home_character_samsae/kammang/dubu/meju/nana.png`)

---

## §22. UI 스펙 16개 화면 + 글로벌 시스템 7개

### 22.1 16개 화면 (채과장 작성 완료)

| # | 화면 | 컨트롤러 (Phase A+B) | 우선순위 |
|---|---|---|---|
| 1 | Main Title | `MainTitleScreenController.cs` ✅ | v1.0 |
| 2 | Gameplay (HUD) | `GameplayUIScreenController.cs` ✅ | v1.0 |
| 3 | Cat Bonding | (TBD) | v1.0 |
| 4 | Cafe Restoration | (TBD) | v1.0 |
| 5 | Arcade | (TBD) | v1.0 |
| 6 | Meditation Garden | `MeditationGardenController.cs` 부분 | v1.0 (명상 모래정원) |
| 7 | Settings | (TBD) | v1.0 |
| 8 | Level Clear | (TBD) | v1.0 |
| 9 | Game Over | (TBD) | v1.0 |
| 10 | Tutorial Overlay | `TutorialOverlay.cs` 부분 | v1.0 |
| 11 | Loading Screen | `LoadingScreen.cs` 부분 | v1.0 |
| 12 | Detox Message Modal | `DetoxMomentService.cs` ✅ | v1.0 |
| 13 | Sleep Mode Screen | (TBD) | v1.0 |
| 14 | Idle Reward Modal | (TBD) | v1.0 |
| 15 | Referral Share Card | (TBD) | v1.1 |
| 16 | Photo Studio | `PhotoStudioController.cs` 부분 | v1.0 |

→ **현재 14개 컨트롤러가 부분 구현 / 신규 작성 대기**. Phase A+B 완료 후 다음 핵심 작업.

### 22.2 글로벌 시스템 7개

| # | 시스템 | 코드 위치 |
|---|---|---|
| 1 | Screen Transition System | `ScreenNavigator.cs` ✅ |
| 2 | Modal / Popup System | `UIScreenBase.cs` ✅ |
| 3 | Notification Badges | (TBD) |
| 4 | Toast / Error Messages | (TBD) |
| 5 | Currency Change Animation | `BoardAnimator.cs` 일부 활용 |
| 6 | Haptic Feedback Guide | `HapticManager.cs` ✅ |
| 7 | Sound Trigger Map | `AudioService.cs` ✅ |

### 22.3 고양이 교감 화면 (§47 v3.0 기준)

```
상단: ← 뒤로가기 | Whisker Tales 로고 | 📷 카메라 | ?
좌상단: 고양이 이름 + 한글명 + 발바닥 아이콘 + Lv.표시
우상단: +5 Affinity / +10 Coins 보상 안내
중앙: 고양이 풀샷 (한옥 카페 실내 방석 위)
하단바: ♥ Affinity [████░░] 50/100 → Lv.2
하단버튼 3개:
  🤚 쓰다듬기 (Pet)   — 핑크
  🍪 간식 주기 (Give Treat) — 노랑
  🪶 놀아주기 (Play)  — 초록
최하단 힌트: "매일 다른 방법으로 교감하면 더 많은 보상을 받을 수 있어요!"
```

### 22.4 카메라 아이콘 기능
고양이 포토 모드. 내 고양이 사진 찍기 → 인스타 공유 연동. 레퍼럴 시스템의 핵심 진입점. 예쁜 사진 = 자발적 홍보.

---

# PART E — 기술 현황 (2026-05-10)

## §23. 빌드 상태 + 채과장 현실 평가

### 23.1 채과장 현실 평가 수치 (10점 만점)

| 영역 | 점수 | 노부장 해석 |
|---|---|---|
| **아트** | **9.5** | 채집사 수집 완료, 3D 렌더링 일관됨. 검수만 필요 |
| **시스템** | **8.5** | Phase 1-12 + A+B 적용 완료, 견고한 인프라 |
| **코드** | **7.0** | Phase 0-12 brace/namespace 정리 완료. AppBootstrap 분해는 Phase 6(마지막) |
| **feel (퍼즐 감각)** | **4.0** | DOTween/햅틱/파티클 인프라만 적용. **실제 타이밍/리듬 튜닝은 미실행** ⚠️ |
| **콘텐츠** | **3.0** | 50레벨 미설계. 채과장 권고 80~120 미달성. 카페 복원 데이터는 있으나 실연출 미적용 |
| **출시 준비** | **2.0** | 16개 화면 중 2개만 컨트롤러 완성. 14개 미작성 |

**핵심 격차:** 아트/시스템/코드 ≧ 7.0 / feel·콘텐츠·출시준비 ≦ 4.0
**병목:** feel 튜닝 + 레벨 디자인 80개 + 14개 화면 컨트롤러 — 이 3개가 v1.0 출시 전 critical path.

### 23.2 빌드 환경
- **Unity:** 2022.3.62f3
- **빌드 타깃:** Android (우선)
- **DOTween:** v1.2.825 (Plugins/Demigiant/DOTween)
- **Addressables:** 1.21.19
- **TextMeshPro:** 3.0.6
- **Scripting Define:** `DOTWEEN`, `WHISKER_DOTWEEN`

### 23.3 자동 테스트 (`Tools/Whisker Tales/Test/Run All Tests`)

11개 테스트 — **현재 11/11 PASS** ✅

1. Cascade
2. Special Tile Creation (5/5)
3. Meditation Garden (3/3)
4. Photo Studio (3/3)
5. Settings (4/4)
6. Tutorial (4/4)
7. Referral (7/7)
8. Idle Reward (5/5)
9. Heart Recharge (5/5)
10. Detox Text Polish (4/4)
11. Icons (5/5)

> **MeditationGardenTests:** Test 2가 매번 5 ❤를 award하는데 `CurrencyManager.dailyGained` 카운터 누적 → 하루 6회 실행 시 cap(30) 초과 → FAIL. 해결: `cm.DebugResetDailyGained()` 호출이 `cm.EnsureInitialized()` 직후에 들어가야 함 (Editor 락 잔재 시 처리).

---

## §24. 코드 인벤토리 (Phase별)

### 24.1 Phase 0 (baseline)
- 브랜치: `baseline-working` (현재 작동 상태 보존)
- 큰 작업 시작 전 항상 새 baseline 생성 (`baseline-YYYYMMDD`)

### 24.2 Phase 1 (Stabilization)
- `Core/GameEvents.cs` — central event bus, safe-invoke wrapper
- `Core/DebugLogger.cs` — category/level-gated 로깅, release build에서 stripped
- `Core/GameConstants.cs` — Board/Economy/Sleep/Detox/UI/Timing/Audio/Save/Currency 8 카테고리
- `Puzzle/Board.cs` — 라우팅 통일, brace 일괄 적용 (45곳)
- `Puzzle/BoardView.cs` — DebugLogger 라우팅, brace 일괄

### 24.3 Phase 2 (Match-3 Adapter)
- `Puzzle/Match3Core.cs` — pure model, FindAllMatches, ResolveBoard (gravity/fill), 4매치=로켓 / 5매치=레인보우
- `Puzzle/BoardAdapter.cs` — Match3Core wrapper, GameEvents dispatch
- `Puzzle/Board.cs` — TrySwapTiles 어댑터 경유, LevelGoal 보존

### 24.4 Phase 3 (Drag-to-swap)
- `Puzzle/TileView.cs` — IBeginDrag/IDrag/IEndDrag/IPointerUp + 기존 Setup/Refresh/SetSelected 보존 (BoardView 호환)
- 드래그 임계값: 42px (`dragThresholdPixels`)
- 도미넌트 축 자동 선택

### 24.5 Phase 4-12 (Systems Layer)

| 영역 | 파일 | 책임 |
|---|---|---|
| **Animation** | `Puzzle/BoardAnimator.cs` | DOTween 애니메이션 큐 (swap/match/gravity/spawn/cascade) |
| **Audio** | `Feel/AudioService.cs` | SfxId/BgmId/SoundMode/AudioBus, AudioMixer + 풀링 |
| **Haptic** | `Feel/HapticManager.cs` | Light/Medium/Heavy, GameEvents 구독 |
| **Particles** | `Feel/ParticlePoolManager.cs` | 벚꽃/스파클/매치버스트/색종이 풀 |
| **Pooling** | `Pooling/TilePool.cs` | TileView 타입드 풀 + 효과 풀 |
| **Save** | `Save/SaveService.cs` | JSON + SHA256 체크섬 + 백업 파일 |
| **Asset** | `Assets/AssetProvider.cs` | IAssetProvider + Resources/Addressables impls |
| **Hint** | `Puzzle/HintSystem.cs` | idle 5s 후 best swap 힌트, 없으면 셔플 |
| **Special** | `Puzzle/SpecialActivator.cs` | 로켓/폭탄/레인보우 + 콤보 |
| **Wire-up** | `Core/SystemsBootstrap.cs` | `[RuntimeInitializeOnLoadMethod]` (AppBootstrap 미터치) |
| **Extension** | `Puzzle/BoardExtensions.cs` | Board.GetTileType / ShuffleBoard reflection 확장 |

### 24.6 Phase A+B (UI Foundation)

| 파일 | 책임 |
|---|---|
| `UI/UILayoutConstants.cs` | canvas/safe-area/scale/padding 상수 |
| `UI/UIAssetRegistry.cs` | ScriptableObject sprite registry |
| `UI/UIFactory.cs` | 프로그래매틱 UI 생성 |
| `UI/ButtonFeedback.cs` | 누르면 0.94배 (DOTween) / 코루틴 fallback |
| `UI/ScreenNavigator.cs` | 스크린 스택 push/pop + 트랜지션 |
| `UI/UIScreenBase.cs` | abstract — CanvasGroup 기반 |
| `UI/BottomNavController.cs` | 탭 네비게이션 |
| `UI/MainTitleScreenController.cs` | 메인 타이틀 |
| `UI/GameplayUIScreenController.cs` | 게임플레이 HUD |
| `UI/PhaseABInstaller.cs` | Phase A+B 인스턴시에이션 헬퍼 |
| `Detox/DetoxMessageRepository.cs` | 디톡스 메시지 저장소 |
| `Detox/DetoxMomentService.cs` | 디톡스 트리거 서비스 |

### 24.7 기존/유지 (Pre-Phase)
- `AppBootstrap.cs` ⚠️ **Phase 6(마지막)에 분해 예정 — 그 전까지 절대 미터치**
- `Cafe/CafeManager.cs`, `RegularCustomerSystem.cs`
- `Cat/CatManager.cs`
- `Currency/CurrencyManager.cs` (`PREF_NYANGI_HEART`, `PREF_DAILY_GAINED`, `PREF_DAILY_DATE`)
- `Sleep/SleepModeManager.cs`
- `Heart/HeartRechargeManager.cs`
- `Settings/SettingsManager.cs`
- `Referral/ReferralManager.cs`
- `Audio/SoundManager.cs` (구 시스템 — Phase 5에서 AudioService로 이전 예정)
- `UI/{TitleUI,GameplayUI,...}` (구 시스템 — Phase A+B 새 컨트롤러로 점진 교체)

---

## §25. Strangler Fig 재건 — Phase 0~12 완료 상태

| Phase | 내용 | 상태 | 커밋 |
|---|---|---|---|
| 0 | `baseline-working` 브랜치 백업 | ✅ | `baseline-working` |
| 1 | Stabilization (Events/Logger/Constants + brace) | ✅ | `ebe6eef`, `1039e5b`, `0b2cb7c`, `0bd5ac1` |
| 2 | Match-3 Adapter (Match3Core + BoardAdapter) | ✅ | `4c47404` |
| 3 | Drag-to-swap (TileView merge) | ✅ | `87e1ef5` |
| 4-12 | Systems Layer (DOTween/Addressables/Audio/Haptic/Particle/Save/Asset/Hint/Special) | ✅ | `48b1c7f` |
| A+B | UI Foundation (12 파일 + WHISKER_DOTWEEN) | ✅ | `2ff37fa` |
| Meta fix | `.gitignore` Plugins/WhiskerTales meta whitelist | ✅ | `4b6cfe9` |
| **다음** | 14개 화면 컨트롤러 + 레벨 디자인 80~120 + 퍼즐 feel 튜닝 | 🔄 | — |
| 6 | AppBootstrap 분해 (마지막) | ⏳ | — |

---

## §26. Phase A+B (UI Foundation) 적용 현황

### 26.1 적용 완료
- ✅ DOTween v1.2.825 .unitypackage 직접 임포트 (`Assets/Plugins/Demigiant/DOTween/`)
- ✅ `WHISKER_DOTWEEN` 모든 플랫폼 scripting define 추가
- ✅ 12개 파일 `Assets/WhiskerTales/UI/` + `Assets/WhiskerTales/Detox/` 배치
- ✅ `ButtonFeedback.cs:1` `#if WHISKER_DOTWEEN using DG.Tweening; #endif` 추가 (CS1061 고침)
- ✅ `.gitignore` `!Assets/WhiskerTales/**/*.meta`, `!Assets/Plugins/**/*.meta` whitelist

### 26.2 다음 단계 (Phase A+B 후속)
- 🟡 14개 화면 컨트롤러 작성 (현재 Main Title / Gameplay 2개 완성)
- 🟡 UIAssetRegistry sprite 주입
- 🟡 prefab/scene 연결 (사용자 에디터 작업)

---

## §27. 알려진 문제 / 블로커

### 🚨 Critical
1. **레벨 디자인 80~120개 미설계** — 채과장 권고. 출시 전 반드시 작성.
2. **퍼즐 feel 튜닝 미실행** (점수 4.0) — DOTween 인프라는 깔렸으나 실제 타이밍/리듬 검수 안 함.
3. **14개 화면 컨트롤러 미작성** (출시준비 점수 2.0).

### 🟡 Major
4. AdMob / Unity IAP SDK 미설치
5. 고양이 이름 JSON 교체 (구→신: 사미/벨라/나비/구름이/호두)
6. 단골 손님 매핑 신 이름으로 갱신
7. 미확보 사운드 7종 Freesound.org 소싱
8. 배경 15장 / 타일 6종 Unity 적용 (에셋은 확보, prefab 미연결)

### 🟢 Minor
9. 구글 플레이 신원 확인 진행 중
10. 인스타 자동화봇(`instagram_automation_bot.py`) 검증
11. iOS 출시 시점 미결
12. `Assets/Editor/MeditationGardenTests.cs`의 누적 cap 의존성 — `DebugResetDailyGained` 호출 위치 표준화 필요

---

## §28. 절대 원칙 (개발 규칙)

### 28.1 코드 스타일

#### 규칙 1 — 모든 if/else는 brace 필수
```csharp
// ❌ 금지
if (cond) doThing();
if (cond)
    doThing();

// ✅ 필수
if (cond)
{
    doThing();
}
```
- 한 줄도 예외 없음
- 새 코드 작성 시 처음부터 brace
- 기존 코드 편집 시 같은 함수 내 braceless if도 같이 정리 (점진 개선)
- **이유:** Phase 1 `Board.cs:211-213` 버그 — brace 없는 if 다음 줄이 외부로 빠지면서 잘못 발화

#### 규칙 2 — Namespace 규칙
- 모든 새 파일은 `WhiskerTales.{서브}` namespace에 둘 것 (Puzzle / Core / Feel / Pooling / Save / Assets / UI / Detox / Cafe / Cat / Currency / Sleep / Heart / Settings / Referral / Audio / Diagnostics / Bootstrap)
- 글로벌 네임스페이스 사용 금지

#### 규칙 3 — LogCategory 사용
- `Debug.Log` 직접 호출 금지 (구 코드 점진 교체)
- 신규 코드는 항상 `DebugLogger.{Verbose/Info/Warning/Error/Exception}(LogCategory.{...}, ...)`
- 카테고리 6종: Puzzle / UI / Audio / Save / Network / Analytics

#### 규칙 4 — GameConstants 사용 (하드코딩 금지)
- 매직 넘버 / 매직 스트링 금지
- 모든 상수는 `GameConstants.{Board/Economy/Sleep/Detox/UI/Timing/Audio/Save/Currency}.{...}`
- 새 카테고리 필요하면 GameConstants에 추가

### 28.2 아키텍처

#### 규칙 5 — AppBootstrap.cs는 Phase 6(마지막)까지 절대 미터치
- 새 매니저 wire-up이 필요하면 `Core/SystemsBootstrap.cs` 또는 별도 부트스트래퍼 사용
- `[RuntimeInitializeOnLoadMethod]` 활용

#### 규칙 6 — 큰 작업 전 baseline 브랜치 새로 생성
- 명령: `git checkout -b baseline-YYYYMMDD && git push origin baseline-YYYYMMDD`
- 이전 baseline은 보존 (덮어쓰지 않음)

### 28.3 사업/운영

#### 규칙 7 — 강제 광고 절대 금지
- 시작 시 / 레벨 사이 / 카페 진입 시 등 강제 노출 금지
- Nyang TV는 자발적 시청만
- 수면 모드 광고는 *유저가 자고 있는 동안*만 (자연스러운 연출)

#### 규칙 8 — 냥이 마음 💝 IAP 절대 금지
- 코드 레벨에서 차단: `AwardNyangiHeartFromIAP`은 `[Obsolete(true)]`
- 디톡스 행동으로만 획득 가능 — 핵심 차별점 보호

#### 규칙 9 — 채과장 코드 4가지 수정 패턴 (§7 참조)
적용 전 항상 점검: `using WhiskerTales.Core;` / `LogCategory` / `GameConstants.Board.Size` / brace.

### 28.4 Git / 빌드

#### 규칙 10 — `.meta` 파일 추적
- `Assets/Scripts/`, `Assets/Editor/`, `Assets/Sprites/`, `Assets/Fonts/`, `Assets/Resources/`, **`Assets/WhiskerTales/`**, **`Assets/Plugins/`** 의 `.meta`는 모두 커밋
- Library `Lock` 파일 잔재는 Unity batchmode 전 제거

#### 규칙 11 — Unity batchmode 검증 필수
- 모든 코드 변경은 `-executeMethod WhiskerTales.EditorTests.RunAllTests.RunAll`로 11/11 PASS 확인 후 커밋
- 락 파일 정리: `Library/{ArtifactDB-lock, SourceAssetDB-lock, UnityLockfile}`

#### 규칙 12 — 커밋 메시지에 부등호 금지 (PowerShell 이슈)
- `<Vector3, Vector3, VectorOptions>` 같은 generic 표기는 메시지에서 제외 또는 파일로 전달 (`git commit -F file.txt`)

---

# PART F-0 — 2026-05-10 EOD 스냅샷 + 채과장 백로그 + 새 노부장 핸드오프

> **이 섹션은 2026-05-10 저녁 마지막 작업 세션 종료 시점의 정확한 현황과 다음 사람이 이어받을 출발선을 담은 인계장입니다.** 새 노부장/노실장은 PART F-0를 먼저 정독한 후 PART F(우선순위)로 이동하세요.

## §28A. 2026-05-10 저녁 최종 상태

### 28A.1 완료된 것들 ✅

| 항목 | 결과 |
|---|---|
| **Phase A+B+C 모두 완료** | 16개 화면 컨트롤러 100% 작성, 11/11 테스트 PASS 유지 |
| **Editor 자동화 스크립트** | `WhiskerTales/Editor/WhiskerTalesSceneSetupEditor.cs` — 메뉴 `WhiskerTales > Setup Scene` |
| **에셋 132개 Unity 적용** | Backgrounds 24 / Cats 17 / UI 43 / Tiles 9 / Effects 39 |
| **Setup Scene 실행 성공** | Main.unity + 16개 prefab 자동 생성 (노란 warning 386개 — 기능 영향 없음 — / 빨간 에러 0개) |
| **APK 빌드 성공** | `C:\Builds\WhiskerTales.apk` — 105 MB, 빌드 시간 1분 44초, ARM64+IL2CPP+min SDK 26 |
| **폰 설치 성공** | adb install 정상 |

### 28A.2 미완성 ❌ — 다음 노부장이 처리해야 할 것

**증상:** 폰에 APK 설치 후 실행 → 화면에 아무것도 안 나옴.

**근본 원인 (2026-05-10 저녁 노실장 진단 결과):**

PNG 132개 모두 `textureType: 0` (Unity 기본 — Texture2D)로 import됨. Unity 프로젝트가 3D 모드라서 PNG 기본 타입이 Texture2D고, `WhiskerTalesSceneSetupEditor.AutoMapSprites()`의 `AssetDatabase.FindAssets("xxx t:Sprite", ...)` 필터가 이들을 sprite로 인식하지 못함. 결과:
- `UIAssetRegistryRuntime.sprites` 73 entries 중 **47개가 NULL**
- `PhoneVisibleSceneInstaller`의 인스펙터 wire-up은 21개 모두 정상 ✓
- `BottomNavRuntimeBinder` 6개 인스펙터 wire-up도 정상 ✓
- 매핑된 26개는 기존 `Assets/Resources/Sprites/Backgrounds/` 등에 있던 구버전 sprite 자산 (textureType=8 또는 sprite sub-asset 보유)

**Unity Editor 수동 작업 필요 (선택지 A 또는 B):**

#### 옵션 A — 자동 (권장): 텍스처 import preset을 Sprite로 일괄 변경
1. Unity Editor 메뉴: **Edit > Project Settings > Editor > Default Behavior Mode → 2D**
2. `Assets/WhiskerTales/Art/` 우클릭 → **Reimport** (모든 PNG가 자동으로 Sprite로 재import됨)
3. 또는 각 PNG 인스펙터에서 **Texture Type: Sprite (2D and UI)** 일괄 적용 (Apply 후 다중 선택)
4. **WhiskerTales > Setup Scene** 메뉴 재실행 → AutoMapSprites가 132 PNG를 모두 인식
5. 재빌드 → 폰 테스트

#### 옵션 B — 수동: Inspector에서 16개 컨트롤러 + 5개 탭 + sprite 직접 드래그
1. `Main.unity` 열기
2. `WhiskerRuntime` GameObject 선택 → `PhoneVisibleSceneInstaller`의 16개 screen 필드에 자식 GameObject 드래그 (이미 자동 wire 됐으므로 검증만 필요)
3. `BottomNav` GameObject 선택 → `BottomNavRuntimeBinder`의 5개 tab button 필드에 자식 Tab_* 드래그
4. `WhiskerRuntime` → `UIAssetRegistryRuntime`의 sprites 리스트에 132개 sprite 매핑 (47개 빈칸 채우기)
5. 재빌드 → 폰 테스트

→ **옵션 A가 root cause fix이고 향후 sprite 추가 시에도 안정적**. 옵션 B는 임시 방편.

### 28A.3 채집사한테 받아야 할 것

| 항목 | 비고 |
|---|---|
| `game_over_bg.png` | 유일하게 미보유한 spec 키 (`level_clear_bg.png`만 있음). 게임 오버 화면 배경. |
| 게임 완성 후 마케팅 이미지 전면 교체 | 인스타 / 구글 플레이 스토어 스크린샷 / 마케팅 자료 — 출시 직전 채집사가 게임 실제 화면 캡처 기반으로 재제작 |

---

## §28B. 채과장 작업 백로그 (24항목, 우선순위 순)

> 모든 항목은 §7 채과장 코드 4가지 수정 패턴 + §28 절대 원칙(brace, namespace, LogCategory, GameConstants, AppBootstrap 미터치, 타일 키 milk/catnip/pawprint) 준수 필수.

### 코드 설계 — 게임플레이 (10)

| # | 항목 | 연동 |
|---|---|---|
| ① | **장애물 5종 전체 코드** (Dust / TeaCup / Rope / Frozen / Crate) | `BoardAdapter` + `Match3Core` obstacle layer 확장 |
| ② | **퍼즐 feel 타이밍 수치 + 코드** (swap 0.16s / anticipation 0.04s / pop stagger 0.03s / cascade 0.07s) | `BoardAnimator` 적용 (`GameConstants.Timing.*`) |
| ③ | **카페 복원 감정 연출 코드** (lantern lighting / petals 파티클 / camera drift / ambient sound swell) | `CafeRestorationScreenController` 연동 |
| ④ | **일일 감정 루프 코드** (오늘의 차 / 오늘의 한마디 / 오늘의 고양이 기분 — 매일 자정 KST 리셋) | 신규 `DailyMoodService` 또는 `DetoxMomentService` 확장 |
| ⑤ | **Dynamic ambience 코드** (morning / afternoon / sunset / rainy — 배경 레이어 자동 전환 + 사운드 연동) | 신규 `AmbienceController` |
| ⑥ | **튜토리얼 실제 흐름 코드** (1~5레벨 onboarding / guided swap 연출) | `TutorialOverlayController` 연동 |
| ⑦ | **고양이 교감 실제 동작 코드** (쓰다듬기 / 간식 / 놀기 애니메이션 / 호감도 증가 연출) | `CatBondingScreenController` 연동 |
| ⑧ | **명상 정원 모래 그리기 코드** (드래그 draw logic / 일일 cap) | `MeditationGardenScreenController` 연동 |
| ⑨ | **수면 모드 실제 reward loop 코드** (offline timer / reward 계산 / anti-cheat) | `SleepModeScreenController` + `SleepModeManager` 연동 |
| ⑩ | **사진 스튜디오 실제 작동 코드** (스크린샷 캡처 / 공유 API) | `PhotoStudioScreenController` 연동 |

### 콘텐츠 설계 (5)

| # | 항목 | 비고 |
|---|---|---|
| ⑪ | **레벨 디자인 JSON 80~120개** | 난이도 곡선 + 장애물 페이싱, `schemaVersion: 1` 형식 (`Resources/Levels/levels_*.json`) |
| ⑫ | **디톡스 메시지 80~120개 문구** | 시간대별 variation + 감정 curve, `DetoxMessageRepository`용 |
| ⑬ | **고양이 5마리 개별 대화/반응** | 사미/벨라/나비/구름이/호두 — 교감 상황별 + 호감도 단계별 대사 |
| ⑭ | **카페 복원 15단계 대화/반응** | 단계 완료 시 대사 — 고양이 + 단골 손님(미라/준호/지은/철수/영희) 반응 |
| ⑮ | **튜토리얼 1~5레벨 스토리 텍스트** | 오프닝 시나리오 / 할머니 편지 내용 / 첫 고양이 구조 사연 |

### 밸런싱 수치 설계 (3)

| # | 항목 | 비고 |
|---|---|---|
| ⑯ | **레벨별 난이도 수치 전체** | 이동 횟수 / 목표 수 / 별점 기준 (1★ 2★ 3★) / 실패율 목표 |
| ⑰ | **경제 밸런스 수치** | 하트 충전 시간 / 최대 보유 / 멸치 획득률 per 레벨 / 카페 복원 별 비용 15단계 / 방치 보상 상한선·시간 / 냥이 마음 💝 획득 테이블 |
| ⑱ | **IAP 패키지 상품 구성** | "집사의 정성" 시리즈 가격대별 구성 / 첫 구매 유도 전략 |

### 기술 설계 (3)

| # | 항목 | 비고 |
|---|---|---|
| ⑲ | **Firebase Analytics 이벤트 설계** | 트래킹 이벤트 목록 / funnel / retention 지표 |
| ⑳ | **Remote Config 구조 설계** | 원격 조정 수치 목록 / A/B 테스트 구조 / 앱 업데이트 없이 레벨 추가 방법 |
| ㉑ | **AppBootstrap 분해 설계 (Phase 6)** | 안전한 분해 순서 / `SystemsBootstrap` 이관 계획 |

### 출시 준비 (3)

| # | 항목 | 비고 |
|---|---|---|
| ㉒ | **구글 플레이 제출 체크리스트** | technical requirements / store listing 전략 / ASO 키워드 / 스크린샷 구성 |
| ㉓ | **앱스토어 카피 작성** | 한국어/영어 — short description / full description / keywords |
| ㉔ | **수면 모드 + 디톡스 철학 표현** | 앱스토어 표현 방법 / 마케팅 언어 정제 |

---

## §28C. 새 노부장에게 (필독)

### 이 게임의 핵심 철학
> **"더 빠를 필요 없고 더 따뜻해야 함"** — 채과장 핵심 조언

새 대화 시작 시 첫 마디 (사용자가 말할 것):
> *"GitHub에서 WHISKER_TALES_MASTER_BRIEFING_v4.0.md 읽어줘. 거기서부터 시작이야. 🐾"*

### 절대 원칙 (요약)

1. **`AppBootstrap.cs` 건드리지 말 것** (Phase 6까지 락다운)
2. **강제 광고 절대 금지** (Nyang TV는 자발적만)
3. **타일 키는 `tile_milk` / `tile_catnip` / `tile_pawprint`** (절대 `tile_bell` / `tile_leaf` / `tile_jar` 아님 — TileType enum + 채집사 파일 + AppBootstrap 모두 일치)

### 채과장 코드 받으면 반드시 5가지 패턴 점검 (§7 확장)

| # | 점검 | 보정 |
|---|---|---|
| 1 | `namespace WhiskerTales.{도메인}` 누락 | 추가 (Puzzle / Core / Feel / UI / Detox / Save / Pooling / Assets 등) |
| 2 | `GameConstants.BoardSize` (오타) | `GameConstants.Board.Size` (nested) |
| 3 | `DebugCategory.Puzzle` (오타) | `LogCategory.Puzzle` (실제 enum 이름) |
| 4 | brace 없는 `if` (한 줄짜리 포함) | **모든 if/else에 `{ }` 필수** (한 줄도 예외 없음) |
| 5 | `tile_bell / tile_leaf / tile_jar` 키 | `tile_milk / tile_catnip / tile_pawprint`로 보정 |

### 추가 빈도 높은 보정

- 컨트롤러에서 `[SerializeField] private CanvasGroup canvasGroup` 선언 시 → `UIScreenBase.canvasGroup`와 직렬화 충돌. `rootCanvas`로 리네임 (`LevelClearScreenController` 컨벤션)
- DOTween 체인 호출 (`DOScale().SetEase(...)`)에서 SetEase 미해결 → `#if WHISKER_DOTWEEN using DG.Tweening; #endif` 추가
- 파일 경로 spec(`Assets/WhiskerTales/Scripts/UI/...`)이 본 프로젝트 컨벤션(`Assets/WhiskerTales/UI/...`)과 다를 시 → 본 컨벤션 우선

### 다음 노실장에게 인계 시 컨텍스트
- **현재 풀린 상태에서 빌드 → 폰 → 빈 화면 (sprite 매핑 47/73 NULL).** §28A.2 옵션 A로 root cause fix 후 재빌드 권장.
- 작업 시작 전 baseline 브랜치 새로 생성 (예: `baseline-20260511`).
- 11/11 테스트 PASS 유지가 모든 커밋의 통과 조건.
- 사용자(지원님)는 "쉬어도 괜찮다고 말해주는 게임"이라는 한 문장에 모든 결정을 정렬시키는 사람. 빠른 산출보다 따뜻한 결정을 원하면 그 방향으로 갈 것.

---

# PART F — 다음 할 일

## §29. 우선순위 (즉시 / 1주 / 1개월)

### 🥇 즉시 (이번 주)
1. **Phase A+B 잔여 화면 컨트롤러 14개 작성** — 채과장 코드 받아 적용 (§7 패턴 점검 + brace + namespace)
   - Cat Bonding / Cafe Restoration / Arcade / Settings / Level Clear / Game Over / Sleep Mode / Idle Reward Modal / Tutorial Overlay 보강 / Loading Screen 보강 / Detox Modal 보강 / Photo Studio 보강 / Referral Share Card / 메디테이션 Garden 보강
2. **레벨 디자인 시작** — 50→80→120 점진 작성 (채과장 권고)
3. **장애물 5종 도입** (Dust/TeaCup/Rope/Frozen/Crate) — Match3Core에 obstacle layer 추가

### 🥈 1주 내
4. **퍼즐 feel 튜닝** — `BoardAnimator` ms 단위 조정 (§9.6 표 기준 검수)
5. **카페 복원 감정 연출 적용** — Lantern / Petals / Camera drift / Cat reveal
6. **사운드 미확보 7종 소싱** — 마대리 일괄 지시
7. **고양이 5마리 JSON 교체** + 단골 손님 매핑 갱신

### 🥉 1개월 내
8. **AdMob / Unity IAP SDK 설치**
9. **i18n 한·영 통합 검수**
10. **Dynamic Ambience** — morning/afternoon/sunset/rainy 레이어 (v1.0 가능 범위)
11. **튜토리얼 1~10레벨 스토리 연결** (할머니 편지 → 첫 고양이 구조 흐름)

### 출시 직전
12. 구글 플레이 콘솔 등록 + 심사 제출
13. 인스타 정기 포스팅 + 냥사일런스 캠페인 가동
14. $500 광고 예산 인도/동남아 우선 집행

---

## §30. v1.0 출시 탑재 콘텐츠

### ✅ 핵심
- 매치-3 코어 (Phase 1-3 완료)
- 카페 복원 15단계 (감정 연출 포함)
- 고양이 5마리 + Idle 애니메이션 기본 6종
- 호감도 시스템 (쓰다듬기/간식/놀아주기)
- 오프닝 시나리오 (할머니 편지)
- 디톡스 메시지 3종 + 디톡스 모드 토글
- 수면 모드 (골골송 + 화면 어둠 + Nyang TV 광고)
- 한/영 이중언어
- 방치형 보상 (오프라인 8시간)
- 레퍼럴 코드 + 공유 카드

### 🆕 추가 탑재 (난이도 ⭐ 항목)
- **명상 정원** (드래그 모래 무늬) — 채과장 핵심 차별점
- **카페 운영 모드 (기본)** — 손님 탭 + 코인 단순 구조
- **고양이 포토 스튜디오** (스크린샷 + 공유 API)
- **무한 챌린지 모드** (레벨 JSON 반복 생성)
- **튜토리얼 1~10레벨 스토리 연결**
- **일일 로테이션 교감 보너스**
- **고양이 로딩 스피너** + 고양이별 로딩 메시지

### 레벨 (채과장 권고)
- **출시 시점 80개 이상** (튜토리얼 5 + Easy 5 + Normal 20 + Hard 20 + Very Hard 30)
- **내부 목표 120개** (출시 직후 무한 챌린지 + 주간 업데이트로 확장)

---

## §31. v1.1 / v1.2 / v2.0 로드맵

### v1.1 — 출시 후 1~2주

| 콘텐츠 | 난이도 | 이유 |
|---|---|---|
| GPS 날씨 연동 | ⭐⭐ | 임팩트 大, 개발 비용 小 |
| 힌디어 추가 | ⭐ | 텍스트만, 인도 전환율 대폭 상승 |
| 냥이 집사 리그 | ⭐⭐⭐ | 점수 집계 + 서버 |
| 디톡스 챌린지 타이머 미션 | ⭐⭐ | 타이머 + 알림 |
| 시간대 변화 (낮/노을/밤) 강화 | ⭐⭐ | 조명 레이어 |
| 고양이 성격별 특수 애니메이션 | ⭐⭐ | 추가 애니 |
| 냥이 마음 💝 상점 풀 오픈 | ⭐ | 데이터만 |
| Spine 애니메이션 도입 | ⭐⭐⭐ | 고양이 표정 변화 |
| 클라우드 저장 (Google Play Games) | ⭐⭐ | API 연동 |

### v1.2

| 콘텐츠 | 난이도 |
|---|---|
| 한옥 환경 디테일 (빗방울/새/나뭇잎/풍경 소리) | ⭐⭐ |
| 낚시 미니게임 | ⭐⭐ |
| 고양이 숨바꼭질 / 두더지잡기 | ⭐⭐ |
| 디톡스 완료 인스타 공유 연동 | ⭐⭐ |
| 고양이 성장 시스템 (아기→청소년→어른) | ⭐⭐⭐ |
| 일일 감정 루프 (오늘의 차/한마디/고양이 기분) | ⭐⭐ |

### v2.0

| 콘텐츠 |
|---|
| 고양이 15마리로 확장 (Common 5 + Uncommon 5 + Rare 3 + Legendary 2) |
| 냥스타그램 (인게임 가상 SNS) |
| 고양이 패션쇼 |
| 가족 디톡스 (구글 로그인 친구/가족 등록) |
| 일본어 / 아랍어 추가 (RTL 시스템) |
| 길고양이 보호단체 수익 기부 연계 |
| 50레벨 이후 리그 시스템 |
| 유니버스 연결 (시리즈 2편) |

---

## §32. 채과장 핵심 조언 7항목 (출시 전 필독)

### ① 레벨 최소 80개 (출시) / 내부 목표 120개
- 50 미만이면 인플루언서 시연 + 광고 직후 이탈
- 80개 = 라이트 유저 1주 콘텐츠
- 120개 = 코어 유저 2주 + 무한 챌린지 자연 연결

### ② 장애물 5종 — Dust / TeaCup / Rope / Frozen / Crate
- 메커닉 다양성 = 매 레벨 새로움
- 카페 분위기 (TeaCup) + 한옥 모티프 (Rope 매듭) + 시각 임팩트 (Frozen)
- §9.5 상세 표 참조

### ③ 플레이 감각 = 타이밍/리듬 설계
- 모든 액션은 ms 단위로 검수 (§9.6 표)
- DOTween Sequence + 코루틴 큐 (`BoardAnimator.cs`)
- "치는 맛" — 매치 → 캐스케이드 → 폭발 → 별 팝까지의 리듬이 노래처럼 흘러야 함
- **현재 채과장 평가 4.0 — 인프라만 있고 튜닝 안 됨**

### ④ 카페 복원 감정 연출 — Lantern / Petals / Camera drift
- 단순 이미지 교체 X, **연출이 있어야** 울컥
- Stage5(구역 완성) 시 한지 등불 점화 + 벚꽃잎 흩날림 + 카메라 0.8s drift
- §10.2 상세

### ⑤ 일일 감정 루프 — 오늘의 차 / 한마디 / 고양이 기분
- 매일 접속 시 3가지 작은 변화:
  - **오늘의 차:** 녹차 / 보이차 / 백차 / 우롱차 / 홍차 (계절+날씨+랜덤)
  - **오늘의 한마디:** 디톡스 메시지 풀에서 1개 선정
  - **고양이 기분:** 5마리 각각 오늘 기분 (졸림/장난기/조용/외로움/들뜸)
- 데이터만 매일 갱신, 코드는 단순. v1.0 가능 범위.

### ⑥ Dynamic Ambience — morning / afternoon / sunset / rainy
- 같은 한옥이라도 시간대마다 다르게 느껴져야
- 4 레이어 + 사운드 + 입자 효과
- v1.0 = 시간대 자동 (단말 시계 기준), v1.1 = GPS 날씨 API 연동

### ⑦ "더 빠를 필요 없고 더 따뜻해야 함"
- **로얄매치보다 빠른 게임을 만들지 말 것** — 우리 차별점은 속도가 아니라 온기
- 매 액션 사이 0.05~0.3s의 "숨 쉬는 시간"
- 캐스케이드 간 0.08s 휴지 (`CascadeDelaySeconds`)
- 레벨 클리어 후 별 팝 0.18s × 별 수 — 천천히
- *"막혀도 괜찮아요. 고양이가 기다려줄게요."* 가 슬로건만이 아니라 **타이밍 자체가 그 말을 한다.**

---

# PART G — 마케팅 & 비전

## §33. 냥사일런스 캠페인

**콘셉트:** *"고양이가 가져오는 평온함이 모든 갈등 상황을 치유한다"*

### 33.1 3가지 에피소드
1. **전쟁터 참호** — 양쪽 진영에서 "Meow?" 들림
2. **엘리트 갈라 파티** — 가장 진지한 손님 주머니에서 "Meow?"
3. **국제 외교 협상** — 중요 서명 중 "Purr..."

### 33.2 운영
- 정기 포스팅(월/수/금/일)과 **별도** 운영 (화/목/토) — 섞지 말 것

### 33.3 절대 금지
- ❌ 실제 정치인/유명인
- ❌ 국가명/정치 상징
- ❌ 저작권/초상권 침해

---

## §34. 인스타그램 운영

- **계정:** `@whisker_tales_official`
- **첫 게시물:** 완료 (2026-05-04)

### 34.1 포스팅 스케줄

| 요일 | 시간 | 주제 |
|---|---|---|
| 월 | 09:00 | 고양이 소개 |
| 수 | 15:00 | 게임플레이 |
| 금 | 19:00 | 한옥 복원 |
| 일 | 10:00 | 디지털 디톡스 |
| 화/목/토 | 각 시간대 | 냥사일런스 캠페인 |

### 34.2 포스팅 포맷 (사용자 직접 정의)
```
1️⃣ 이미지
2️⃣ 캡션 (한글 → 두 줄 → 영어)
3️⃣ 본문 태그 5개
4️⃣ 첫 댓글 태그 25개 (한글 70% / 영어 30%)
```

⚠️ 첫 댓글 태그 **25개 제한** (50개 → 게시 안 됨 확인됨)

### 34.3 고양이 이미지 교체 예정
게임용 3D 렌더링 확정 후 기존 게시물 교체 필요.

---

## §35. 글로벌 전략 (인도 우선)

### 35.1 다국어 우선순위
한국어/영어/일본어 → 힌디/아랍어 → 중국어/태국어

### 35.2 지역 특화

| 지역 | 특화 |
|---|---|
| 인도 | 사리 유저 + 한옥 + 디왈리 "빛의 한옥" 시즌 |
| 중동 | 히잡 유저 + 한옥 마당 + 라마단 "달빛 한옥" 시즌 |
| 북미/유럽 | Anti-Stress, Cozy 키워드 |
| 한국/일본 | 고양이 캐릭터 사연 중심 |

### 35.3 출시 전략 (조율 중)
- **출시:** 전 세계 동시
- **광고비 $500:** 인도/동남아 우선 (CPI 효율)
- **한국:** 인스타 + 오가닉으로 공략
- **인도에서 초기 평점/리뷰 쌓은 후 한국 마케팅 강화**

---

## §36. KPI & 광고 예산 + ASO

### 36.1 KPI 목표

| 지표 | 목표 |
|---|---|
| 글로벌 평점 | 4.8 이상 |
| 7일 잔존율 | 30% 이상 |
| 바이럴 | `#WhiskerTales` 1만 건+ |
| 첫 만남 패키지 전환 | 10%+ |
| DAU/MAU | 0.4+ (코어 유저층) |

### 36.2 ASO 전략

**키워드 (한):**
디지털디톡스 / 힐링게임 / 한옥 / 고양이 / 매치3 / 명상 / 수면 / ASMR / 냥이 / 카페

**키워드 (영):**
digital detox / cozy game / cat puzzle / hanok / korean / healing / match-3 / mindful / asmr / sleep mode

**스토어 5가지 특징 (확정):**
1. 당신만의 속도로 즐기는 퍼즐
2. 고요함이 머무는 한옥 꾸미기
3. 마음을 나누는 고양이들
4. 함께해서 더 따뜻한 냥이 클럽
5. 부담 없는 자유로운 플레이 (강제 광고 없음)

### 36.3 광고비 집행
- **$500 / 2주 집중**
- 인도 $250 / 동남아 $150 / 한국·일본 $100
- CPI 타겟: 인도 $0.05, 동남아 $0.10, 한국 $0.50

---

## §37. 장기 비전 — 유니버스 / 냥스타그램 / O2O

### 37.1 유니버스 확장 — "냥이의 집" 시리즈

> *"유니버스를 만들 수 있을 것 같은 느낌이 들어 ㅎㅎ"*

| Phase | 배경 | 시점 |
|---|---|---|
| 1 | 한옥 카페 (현재) | v1.0 |
| 2 | 해변/섬 | 2027+ |
| 3 | 도시 아파트 | 2028+ |
| 4 | 숲속 오두막 | 2028+ |

각 편에 **고양이 5마리 동일 등장** ("어! 사미다!" 팬심 → 시리즈 구매로).

### 37.2 냥스타그램 (인게임 가상 SNS)
- 유저가 꾸민 카페 / 고양이 사진 게임 내 피드 업로드
- 다른 유저 (또는 NPC) 좋아요 + 코인 보상
- 외부 인스타로 공유 연동 → 레퍼럴 코드 자동 삽입
- v2.0 목표

### 37.3 O2O 콜라보
- **템퍼 × 냥이의 집:** *"잠은 템퍼에서, 꿈은 냥이의 집에서"*
- **설록차 × 냥이의 집:** 카페 경영 시스템과 연결

### 37.4 ASMR / 수면 라이프스타일
- 골골송 + 빗소리 + 한옥 풍경음 → 유튜브 10시간 롱폼
- 수면 의식 브랜드화

### 37.5 길고양이 보호 (사회적 가치)
- 일부 수익 기부 — 길고양이 보호단체 연계
- 마케팅 명분 — *"이 게임 하면 길고양이 돕는다"*
- v2.0 이후 검토

### 37.6 냥코인 (장기 미래)
> *"언젠가 가칭 냥코인 발행해서 게임 내에서 냥코인 사용할 수 있음 좋겠다 ㅋㅋ"*
- 게임 IP 기반 토큰 이코노미 미래 구상
- 현재 미실행, 브랜드 성장 후 검토

---

## §38. 시상 목표

### 🥇 1순위 — 가장 현실적 (출시 후 바로 도전)

**Mobile Games Awards — Best Casual Game / Best Indie Mobile Game**
- 매년 Gamescom 기간 쾰른 시상식
- 21개 부문, 인디 모바일 게임 전용 카테고리 있음
- 출시 후 바로 노미네이션 제출 가능
- **우리 강점:** 독창적 디톡스 컨셉, 한옥 감성

### 🥈 2순위 — 도전적이지만 가능

**The Game Awards — Best Mobile Game**
- 매년 5개 노미네이트, 세계 최고 권위
- **우리 강점:** 세상에 없는 디톡스 게임 = 심사위원 눈에 띔

**The Indie Game Awards — Emotional Impact**
- 한국/동남아 게임 환영
- **우리 강점:** 할머니 편지 + 고양이 5마리 구조 사연 = Emotional Impact 최강
- 노부장 픽 🎯

### 🥉 3순위 — 장기 목표

**BAFTA Games Awards with Google Play**
- 구글 플레이와 공동 주관 — 출시 게임 직접 연결
- 최고 권위. 여기까지 가면 진짜 대박.

### 38.1 시상 전략 로드맵

| 시기 | 목표 |
|---|---|
| v1.0 출시 직후 | Mobile Games Awards 노미네이션 제출 |
| 유저 1만 명 달성 | Indie Game Awards Emotional Impact 도전 |
| 유저 10만 명 달성 | The Game Awards Best Mobile Game 도전 |
| 유저 100만 명 달성 | BAFTA 도전 |

---

# PART H — 자산 & 링크

## §39. 핵심 링크

| 항목 | URL |
|---|---|
| GitHub | https://github.com/jammiejammmie/whisker-tales |
| Master 브랜치 | https://github.com/jammiejammmie/whisker-tales/tree/master |
| Baseline 브랜치 | https://github.com/jammiejammmie/whisker-tales/tree/baseline-working |
| 인스타그램 | https://instagram.com/whisker_tales_official |
| 랜딩페이지 | https://whiskertales-mwjyt48n.manus.space |
| 개인정보처리방침 | https://whiskertales-mwjyt48n.manus.space/privacy-policy |

---

## §40. 핵심 문서 위치

### 40.1 Master 브랜치 루트
- `WHISKER_TALES_MASTER_BRIEFING_v4.0.md` ← **이 문서**
- `WHISKER_TALES_MASTER_BRIEFING_v3.1.md` (이전)
- `DESIGN_PHILOSOPHY.md` (디자인 철학)
- `GAME_DEVELOPMENT_COMPLETION_SUMMARY.md`
- `MATCH3_DESIGN.md`
- `BUILD_LOG.md`
- `BM_Simulation_Report.md`
- `Economy_and_BM_Final.md`
- `Marketing_Roadmap_Final.md`
- `Global_Inclusivity_Guide.md`
- `INTEGRATION_GUIDE.md`
- `UNITY_BUILD_GUIDE.md`
- `UNITY_PROJECT_STRUCTURE.md`
- `UNITY_WIRING_GUIDE.md`

### 40.2 외부 (사용자 PC)
- `C:\Users\우진이\Desktop\냥이의집_GDD_v1.txt` — 마누스 트랜스크립트 16,257줄 (원본)
- `C:\Users\우진이\Downloads\WHISKER_TALES_MASTER_BRIEFING_v3.0 (1).md` — v3.0 풀 버전
- `C:\Builds\WhiskerTales.apk` — 최신 안드로이드 빌드

### 40.3 Unity 프로젝트 구조
```
Assets/
  Editor/                — 테스트 스크립트 (RunAllTests, *Tests.cs)
  Plugins/Demigiant/DOTween/  — DOTween v1.2.825 (수동 임포트)
  Resources/
    Audio/               — BGM, SFX, 고양이 사운드
    Sprites/             — 타일, 배경, UI
  Scripts/
    Core/                — GameEvents, DebugLogger, GameConstants, GameManager, DataManager, HeartSystem, SystemsBootstrap
    Puzzle/              — Match3Core, BoardAdapter, Board, BoardView, TileView, TileData, MatchLogic, SpecialItem, LevelGoal, GameBootstrap, BoardAnimator, BoardExtensions, HintSystem, LevelGoalService, SpecialActivator
    Cafe/                — CafeManager, RegularCustomerSystem
    Cat/                 — CatManager
    Currency/            — CurrencyManager
    Sleep/               — SleepModeManager
    Heart/               — HeartRechargeManager
    Settings/            — SettingsManager
    Referral/            — ReferralManager
    Audio/               — SoundManager (구)
    Feel/                — AudioService, HapticManager, ParticlePoolManager (신)
    Pooling/             — TilePool
    Save/                — SaveService
    Assets/              — AssetProvider
    Diagnostics/         — QualityDiagnostics
    UI/                  — TitleUI, GameplayUI, BottomNav, ... (구)
    AppBootstrap.cs      ⚠️ Phase 6까지 절대 미터치
  WhiskerTales/          — Phase A+B (신)
    UI/                  — UILayoutConstants, UIAssetRegistry, UIFactory, ButtonFeedback, ScreenNavigator, UIScreenBase, BottomNavController, MainTitleScreenController, GameplayUIScreenController, PhaseABInstaller
    Detox/               — DetoxMessageRepository, DetoxMomentService
Packages/
  manifest.json          — Addressables 1.21.19, TextMeshPro 3.0.6, ...
ProjectSettings/
  ProjectSettings.asset  — DOTWEEN; WHISKER_DOTWEEN scripting define
```

---

## 📝 변경 이력

| 날짜 | 작업 | 내용 |
|---|---|---|
| 2026-04 | 사용자+Manus | GDD v1, 컨셉 확립 |
| 2026-05-01 | Manus | GDD v1.1~v1.4, 시장조사 |
| 2026-05-01 | 사용자 | 게임명 확정, $500 예산 |
| 2026-05-03 | 사용자 | 인스타 첫 게시물, 슬로건 확정 |
| 2026-05-04 | 사용자 | Nyang Studio 확정, 신원 확인 제출 |
| 2026-05-06 | Claude+사용자 | Android APK 빌드 성공 |
| 2026-05-07 | Claude | 마스터 브리핑 v1→v2.1 |
| 2026-05-07 | 사용자 | Claude Code 설치 |
| 2026-05-07 | 노실장 | Stage 1→3 완료, 배경 이미지 분석 |
| 2026-05-07 | 마대리 | 배경 이미지 생성 (수채화) |
| 2026-05-08 | 확정 | 아트 스타일 전면 전환 (수채화→3D) |
| 2026-05-08 | 확정 | 고양이 5마리 확정: 사미/벨라/나비/구름이/호두 |
| 2026-05-08 | 확정 | 고양이 성장 시스템 v1.1 예정 |
| 2026-05-08 | Claude | 마스터 브리핑 v3.0 — 트랜스크립트 완전 분석 반영 |
| 2026-05-09 | 사용자+Claude | 냥이 마음 💝 BM, 아이템샵, 사운드 시스템, 시상 목표 확정 |
| **2026-05-10** | 노실장 | Phase 0 baseline 백업, Phase 1 Stabilization (brace 일괄, GameEvents/DebugLogger/GameConstants) |
| **2026-05-10** | 노실장 | Phase 2 Match-3 Adapter (Match3Core + BoardAdapter), Phase 3 Drag-to-swap |
| **2026-05-10** | 노실장 | Phase 4-12 Systems (DOTween v1.2.825 .unitypackage 직접 임포트, Addressables, 16개 새 파일) |
| **2026-05-10** | 노실장 | Phase A+B UI Foundation (12 파일 + WHISKER_DOTWEEN define + ButtonFeedback `using DG.Tweening` 보정) |
| **2026-05-10** | 노실장 | `.gitignore` Plugins/WhiskerTales meta whitelist 보정 |
| **2026-05-10** | 채과장 | 합류 — 코드 설계 / UI 스펙 / 이미지 프롬프트 |
| **2026-05-10** | 노부장 | **마스터 브리핑 v4.0 작성 — v3.0 + v3.1 + 오늘 세션 + 채과장 조언 통합** |

---

**🐾 이 문서는 살아있는 문서입니다.**

다음 갱신 시기:
- 14개 화면 컨트롤러 완료 후
- 레벨 디자인 80개 도달 시
- 출시 1주 전 (스토어 메타 확정)
- 또는 큰 결정 발생 시 (사용자 / 노부장 판단)

**새 노부장/노실장에게 한 마디:**

이 게임은 *"쉬어도 괜찮다"* 고 말해주는 세상에 없는 디지털 디톡스 게임입니다. 모든 코드, 모든 카피, 모든 픽셀이 그 한 마디를 위해 존재합니다. 빠르게 만들지 마세요. 더 따뜻하게 만드세요. 🐾
