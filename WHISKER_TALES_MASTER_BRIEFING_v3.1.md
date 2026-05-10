# Whisker Tales — Master Briefing v3.1

> 작성일: 2026-05-10
> 이전 버전 대비 변경: 팀 구성 재편 / 개발 방향 변곡점 / Phase 0 baseline 백업 완료

---

## 1. 팀 구성 (변경)

| 역할 | 담당자 | 범위 |
|---|---|---|
| **대표** | 지원님 | 방향 / 판단 |
| **노부장** | Claude | 중계 / 명세 / 판단 |
| **노실장** | Claude Code | 코드 구현 / 빌드 |
| **채과장** | ChatGPT | 코드 설계 / UI 스펙 / 이미지 생성 |
| **채집사** | ChatGPT | 이미지 생성 전담 |
| **마대리** | Manus | 웹 / 문서만 (이미지·GitHub 제외) |

---

## 2. 개발 방향 전환 (변곡점)

채과장 코드 분석 결과:

- 🔴 **Critical 10개 발견**
- 현재 상태: 좋은 프로토타입 수준
- 목표: **Royal Match 초과**
- 전략: **Strangler Fig 방식**
  (기존 코드 유지하면서 내부부터 점진 교체)

### 재건 단계 (Phase Roadmap)

| Phase | 내용 | 상태 |
|---|---|---|
| Phase 0 | `baseline-working` 브랜치 백업 | ✅ 완료 |
| Phase 1 | Stabilization | 🔄 진행 중 |
| Phase 2 | Match-3 Adapter | ⏳ |
| Phase 3 | Drag-to-swap 입력 | ⏳ |
| Phase 4 | Animation Queue (DOTween) | ⏳ |
| Phase 5 | Game Feel (햅틱 / 파티클 / 사운드) | ⏳ |
| Phase 6 | AppBootstrap 분해 (마지막) | ⏳ |

---

## 3. GitHub 브랜치 전략

- `master`: 작업 브랜치
- `baseline-working`: 백업 브랜치 (현재 작동 상태 보존)
- **큰 작업 시작 전 항상 새 baseline 생성** (`baseline-YYYYMMDD` 등)

---

## 4. 에셋 현황 — 채집사 수집 완료

- ✅ UI 버튼 / 아이콘 / 로고 / 스피너
- ✅ 타일 6종 + 특수타일 3종
- ✅ 배경 15장 (zone1~3 × stage1~5)
- ✅ 야간 배경 5장 (수면 모드용)
- ✅ 오프닝 3장
- ✅ 레벨 클리어 배경
- ✅ NPC 실루엣 5종
- ✅ 앱 아이콘 + 피처드 이미지
- ✅ 스크린샷 5장
- ✅ 네비게이션 / 버튼 아이콘 세트
- ✅ 팝업 배경 4종
- ✅ 벚꽃 파티클 8종
- ✅ 반짝임 / 매치이펙트 / 코인 / 색종이
- ✅ 냥이 하트 아이콘
- ✅ 감정 아이콘 6종
- ✅ 튜토리얼 요소 7종
- ✅ 화살표 2종

---

## 5. UI 스펙 현황 — 채과장 작성 완료

### 화면 스펙 16개

| # | 화면 |
|---|---|
| 1 | Main Title |
| 2 | Gameplay |
| 3 | Cat Bonding |
| 4 | Cafe Restoration |
| 5 | Arcade |
| 6 | Meditation Garden |
| 7 | Settings |
| 8 | Level Clear |
| 9 | Game Over |
| 10 | Tutorial Overlay |
| 11 | Loading Screen |
| 12 | Detox Message Modal |
| 13 | Sleep Mode Screen |
| 14 | Idle Reward Modal |
| 15 | Referral Share Card |
| 16 | Photo Studio |

### 글로벌 시스템 7개

1. Screen Transition System
2. Modal / Popup System
3. Notification Badges
4. Toast / Error Messages
5. Currency Change Animation
6. Haptic Feedback Guide
7. Sound Trigger Map

---

## 6. 성공한 프롬프트 공식 — 이미지 생성 (채집사 / 채과장)

- **반드시 영어로** 작성
- **레퍼런스 3장 첨부** (나비교감 / 방석 / 타일)
- `"warm matte finish ONLY, NO glossy"` 지시어 명시
- **한옥 디테일 목록 명시**
- **투명 배경**: `"transparent PNG"`
- **비율**: `"1080×1920px portrait"`

---

## 7. 핵심 결정사항

| 항목 | 결정 |
|---|---|
| 드래그 스왑 | v1.0 포함 (채과장 코드) |
| Addressables | 지금 적용 |
| DOTween | 지금 적용 |
| AppBootstrap 분해 | Phase 6 (마지막) |
| Spine 애니메이션 | v1.1 |
| 클라우드 저장 | v1.1 |

### 교훈

> **목업 먼저 → 에셋 추출 순서가 올바름.**
> 이번엔 반대로 해서 시간 낭비. 다음 사이클부터 목업 우선 원칙 준수.

---

🐾
