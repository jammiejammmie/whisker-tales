using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using WhiskerTales.Core;
using WhiskerTales.UI;

namespace WhiskerTales.Runtime
{
    /// <summary>
    /// "원래 이 집에 살고 있는 고양이" — 위치 이동 + 정적 ambient 애니메이션.
    /// 모든 톤은 "보이는 연출"이 아니라 "어, 뭔가 살아있었던 것 같은데?" 수준.
    /// 끄면 약간 허전한 정도 — 게임 idle animation 느낌 금지.
    /// </summary>
    public sealed class HomeNabiPositionSystem : MonoBehaviour
    {
        public enum PoseId
        {
            Random = 0,
            IdleSit = 1,
            SleepyLoaf = 2,
            StretchSmall = 3
        }

        [System.Serializable]
        public struct PositionAnchor
        {
            [Tooltip("Inspector 라벨용 id — pos_maru_end / pos_puzzle_book / pos_cushion / pos_door_front / pos_sunlight / pos_eave / pos_yard_view / pos_lantern")]
            public string id;
            public RectTransform anchor;
            [Tooltip("이 위치에서 추천하는 포즈. Random이면 가중 랜덤 (정면 응시 빈도 낮음).")]
            public PoseId recommendedPose;
        }

        [Header("Body")]
        [Tooltip("이동할 본체 RectTransform — anchoredPosition 또는 worldPosition을 anchor에 맞춤.")]
        [SerializeField] private RectTransform body;
        [Tooltip("body의 sprite — 포즈에 따라 교체.")]
        [SerializeField] private Image bodyImage;
        [Tooltip("body 전체 페이드용 CanvasGroup (페이드 인/아웃 + hidden 상태).")]
        [SerializeField] private CanvasGroup bodyCanvasGroup;
        [Tooltip("눈 깜빡임용 마스크 CanvasGroup. 비워두면 blink 비활성.")]
        [SerializeField] private CanvasGroup eyeMask;

        [Header("Sprites")]
        [SerializeField] private Sprite poseIdleSit;
        [SerializeField] private Sprite poseSleepyLoaf;
        [SerializeField] private Sprite poseStretchSmall;

        [Header("Positions")]
        [Tooltip("8개 가시 위치. pos_hidden은 별도 상태로 처리 — 배열에 포함하지 않음.")]
        [SerializeField] private PositionAnchor[] positions;

        private enum State
        {
            Visible,
            Hidden
        }

        private State currentState;
        private int currentVisibleIndex = -1;
        private PoseId currentPose;
        private Tween breathTween;
        private Tween blinkTween;
        private Tween stretchTween;
        private Tween moveTween;
        private Tween moveScheduleTween;
        private Tween stretchScheduleTween;

        private void Reset()
        {
            positions = new PositionAnchor[]
            {
                new PositionAnchor { id = "pos_maru_end",    recommendedPose = PoseId.Random },
                new PositionAnchor { id = "pos_puzzle_book", recommendedPose = PoseId.Random },
                new PositionAnchor { id = "pos_cushion",     recommendedPose = PoseId.SleepyLoaf },
                new PositionAnchor { id = "pos_door_front",  recommendedPose = PoseId.IdleSit },
                new PositionAnchor { id = "pos_sunlight",    recommendedPose = PoseId.SleepyLoaf },
                new PositionAnchor { id = "pos_eave",        recommendedPose = PoseId.Random },
                new PositionAnchor { id = "pos_yard_view",   recommendedPose = PoseId.Random },
                new PositionAnchor { id = "pos_lantern",     recommendedPose = PoseId.Random }
            };
        }

        private void OnEnable()
        {
            if (ValidateRefs() == false)
            {
                return;
            }

            EnterInitialPosition();
            StartBreathing();
            ScheduleNextBlink();
            ScheduleNextStretch();
            ScheduleNextMove();
        }

        private void OnDisable()
        {
            KillAll();
        }

        private bool ValidateRefs()
        {
            if (body == null || bodyImage == null || bodyCanvasGroup == null)
            {
                DebugLogger.Warning(LogCategory.UI, "HomeNabiPositionSystem: body/bodyImage/bodyCanvasGroup not assigned.");
                return false;
            }

            if (positions == null || positions.Length == 0)
            {
                DebugLogger.Warning(LogCategory.UI, "HomeNabiPositionSystem: positions empty.");
                return false;
            }

            return true;
        }

        private void KillAll()
        {
            breathTween?.Kill();
            blinkTween?.Kill();
            stretchTween?.Kill();
            moveTween?.Kill();
            moveScheduleTween?.Kill();
            stretchScheduleTween?.Kill();
            breathTween = null;
            blinkTween = null;
            stretchTween = null;
            moveTween = null;
            moveScheduleTween = null;
            stretchScheduleTween = null;
        }

        // ---------- Position movement ----------

        private void EnterInitialPosition()
        {
            int idx = PickNextVisibleIndex();

            if (idx < 0)
            {
                currentState = State.Hidden;
                bodyCanvasGroup.alpha = 0f;
                return;
            }

            SnapToAnchor(idx);
            currentPose = ResolvePose(positions[idx].recommendedPose);
            bodyImage.sprite = ResolveSprite(currentPose);
            currentVisibleIndex = idx;
            currentState = State.Visible;
            bodyCanvasGroup.alpha = 1f;

            DebugLogger.Info(LogCategory.UI, "HomeNabiPositionSystem: initial pos=" + positions[idx].id + " pose=" + currentPose);
        }

        private void ScheduleNextMove()
        {
            float wait = Random.Range(
                UILayoutConstants.HomeNabiPositionStayMinSeconds,
                UILayoutConstants.HomeNabiPositionStayMaxSeconds);

            moveScheduleTween?.Kill();
            moveScheduleTween = DOVirtual.DelayedCall(wait, DoMove, false).SetUpdate(true);
        }

        private void DoMove()
        {
            moveTween?.Kill();

            bool nextHidden = ShouldGoHiddenNext();

            Sequence seq = DOTween.Sequence().SetUpdate(true);

            if (currentState == State.Visible)
            {
                seq.Append(bodyCanvasGroup
                    .DOFade(0f, UILayoutConstants.HomeNabiPositionFadeSeconds)
                    .SetEase(Ease.InOutSine));
            }

            seq.AppendCallback(() =>
            {
                if (nextHidden == true)
                {
                    currentState = State.Hidden;
                    DebugLogger.Info(LogCategory.UI, "HomeNabiPositionSystem: -> hidden");
                    return;
                }

                int idx = PickNextVisibleIndex();
                if (idx < 0)
                {
                    currentState = State.Hidden;
                    return;
                }

                SnapToAnchor(idx);
                currentPose = ResolvePose(positions[idx].recommendedPose);
                bodyImage.sprite = ResolveSprite(currentPose);
                currentVisibleIndex = idx;
                currentState = State.Visible;
                DebugLogger.Info(LogCategory.UI, "HomeNabiPositionSystem: -> " + positions[idx].id + " pose=" + currentPose);
            });

            if (nextHidden == false)
            {
                seq.Append(bodyCanvasGroup
                    .DOFade(1f, UILayoutConstants.HomeNabiPositionFadeSeconds)
                    .SetEase(Ease.InOutSine));
            }

            seq.OnComplete(ScheduleNextMove);
            moveTween = seq;
        }

        private bool ShouldGoHiddenNext()
        {
            // 연속 hidden 금지 — 현재 hidden이면 다음은 무조건 visible로.
            if (currentState == State.Hidden)
            {
                return false;
            }

            return Random.value < UILayoutConstants.HomeNabiHiddenProbability;
        }

        private int PickNextVisibleIndex()
        {
            if (positions == null || positions.Length == 0)
            {
                return -1;
            }

            List<int> candidates = new List<int>(positions.Length);
            for (int i = 0; i < positions.Length; i++)
            {
                if (positions[i].anchor == null) { continue; }
                if (i == currentVisibleIndex) { continue; }
                candidates.Add(i);
            }

            if (candidates.Count == 0)
            {
                // 후보가 없으면 (anchor가 1개뿐인 케이스) 현재 위치 재사용 허용.
                for (int i = 0; i < positions.Length; i++)
                {
                    if (positions[i].anchor != null) { return i; }
                }
                return -1;
            }

            return candidates[Random.Range(0, candidates.Count)];
        }

        private void SnapToAnchor(int idx)
        {
            RectTransform anchor = positions[idx].anchor;
            if (anchor == null) { return; }

            // anchor가 같은 부모면 anchoredPosition을 직접 복사 (가장 정확).
            // 부모가 다르면 worldPosition으로 복사.
            if (body.parent == anchor.parent)
            {
                body.anchoredPosition = anchor.anchoredPosition;
            }
            else
            {
                body.position = anchor.position;
            }
        }

        // ---------- Pose ----------

        private PoseId ResolvePose(PoseId recommended)
        {
            if (recommended == PoseId.Random)
            {
                return PickWeightedRandomPose();
            }

            return recommended;
        }

        private PoseId PickWeightedRandomPose()
        {
            // 정면 응시(IdleSit) 빈도 낮게. 나머지는 균등.
            if (Random.value < UILayoutConstants.HomeNabiFrontalPoseChance)
            {
                return PoseId.IdleSit;
            }

            return Random.value < 0.5f ? PoseId.SleepyLoaf : PoseId.StretchSmall;
        }

        private Sprite ResolveSprite(PoseId pose)
        {
            switch (pose)
            {
                case PoseId.IdleSit:
                    return poseIdleSit;
                case PoseId.SleepyLoaf:
                    return poseSleepyLoaf;
                case PoseId.StretchSmall:
                    return poseStretchSmall;
                default:
                    return poseIdleSit;
            }
        }

        // ---------- Breathing ----------

        private void StartBreathing()
        {
            if (body == null) { return; }

            breathTween?.Kill();

            float period = Random.Range(
                UILayoutConstants.HomeNabiQuietBreathSecondsMin,
                UILayoutConstants.HomeNabiQuietBreathSecondsMax);

            body.localScale = Vector3.one;

            breathTween = body
                .DOScale(UILayoutConstants.HomeNabiQuietBreathScale, period)
                .From(Vector3.one)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(true);
        }

        // ---------- Blink ----------

        private void ScheduleNextBlink()
        {
            if (eyeMask == null) { return; }

            float wait = Random.Range(
                UILayoutConstants.HomeNabiQuietBlinkIntervalMin,
                UILayoutConstants.HomeNabiQuietBlinkIntervalMax);

            blinkTween?.Kill();
            blinkTween = DOVirtual.DelayedCall(wait, PlayBlink, false).SetUpdate(true);
        }

        private void PlayBlink()
        {
            if (eyeMask == null)
            {
                ScheduleNextBlink();
                return;
            }

            Sequence seq = DOTween.Sequence().SetUpdate(true);
            seq.Append(eyeMask
                .DOFade(1f, UILayoutConstants.HomeNabiQuietBlinkCloseSeconds)
                .SetEase(Ease.OutQuad));
            seq.Append(eyeMask
                .DOFade(0f, UILayoutConstants.HomeNabiQuietBlinkOpenSeconds)
                .SetEase(Ease.InQuad));
            seq.OnComplete(ScheduleNextBlink);

            blinkTween = seq;
        }

        // ---------- Stretch ----------

        private void ScheduleNextStretch()
        {
            float wait = Random.Range(
                UILayoutConstants.HomeNabiQuietStretchIntervalMin,
                UILayoutConstants.HomeNabiQuietStretchIntervalMax);

            stretchScheduleTween?.Kill();
            stretchScheduleTween = DOVirtual.DelayedCall(wait, TryStretch, false).SetUpdate(true);
        }

        private void TryStretch()
        {
            // 매우 드물게만 실제 발생 — 게임 idle animation 느낌 회피.
            if (Random.value > UILayoutConstants.HomeNabiQuietStretchChance)
            {
                ScheduleNextStretch();
                return;
            }

            // hidden 상태이거나 sprite ref가 없으면 스킵.
            if (currentState != State.Visible || poseStretchSmall == null || bodyImage == null)
            {
                ScheduleNextStretch();
                return;
            }

            PlayStretch();
        }

        private void PlayStretch()
        {
            stretchTween?.Kill();

            PoseId previousPose = currentPose;
            Sprite previousSprite = bodyImage.sprite;

            bodyImage.sprite = poseStretchSmall;
            currentPose = PoseId.StretchSmall;
            DebugLogger.Info(LogCategory.UI, "HomeNabiPositionSystem: stretch");

            stretchTween = DOVirtual.DelayedCall(
                UILayoutConstants.HomeNabiQuietStretchHoldSeconds,
                () =>
                {
                    if (bodyImage != null && currentState == State.Visible)
                    {
                        bodyImage.sprite = previousSprite;
                        currentPose = previousPose;
                    }
                    ScheduleNextStretch();
                },
                false).SetUpdate(true);
        }
    }
}
