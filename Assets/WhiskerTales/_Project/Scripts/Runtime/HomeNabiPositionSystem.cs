using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using WhiskerTales.Core;
using WhiskerTales.UI;

namespace WhiskerTales.Runtime
{
    /// <summary>
    /// "원래 이 집에 살고 있는 고양이" — 위치 이동 + 정적 ambient + 공간 통합 (원근/그림자/시간대 tint).
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
            [Tooltip("원근 스케일. 가까움=1.0, 중간=0.85, 멀리=0.65. 0이면 1.0으로 처리 (마이그레이션 안전).")]
            [Range(0.3f, 1.2f)]
            public float perspectiveScale;
        }

        [Header("Body")]
        [Tooltip("이동/스케일이 적용되는 컨테이너 RectTransform.")]
        [SerializeField] private RectTransform body;
        [Tooltip("body의 자식인 Cat의 Image — 포즈 sprite 교체 + 시간대 tint.")]
        [SerializeField] private Image bodyImage;
        [Tooltip("body 전체 페이드용 CanvasGroup.")]
        [SerializeField] private CanvasGroup bodyCanvasGroup;
        [Tooltip("눈 깜빡임용 마스크 CanvasGroup. 비워두면 blink 비활성.")]
        [SerializeField] private CanvasGroup eyeMask;

        [Header("Shadow")]
        [Tooltip("body 하단 타원 그림자 Image. sprite 미할당 시 첫 OnEnable에서 절차적 soft ellipse 생성.")]
        [SerializeField] private Image shadowImage;

        [Header("Sprites")]
        [SerializeField] private Sprite poseIdleSit;
        [SerializeField] private Sprite poseSleepyLoaf;
        [SerializeField] private Sprite poseStretchSmall;

        [Header("Time-of-Day Integration")]
        [Tooltip("시간대 tint를 구독할 controller. 비워두면 tint 비활성.")]
        [SerializeField] private HomeTimeOfDayController timeOfDayController;

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
        private float currentPerspectiveScale = 1f;
        private PoseId currentPose;
        private Tween breathTween;
        private Tween blinkTween;
        private Tween stretchTween;
        private Tween moveTween;
        private Tween moveScheduleTween;
        private Tween stretchScheduleTween;
        private Tween tintTween;

        private static Sprite cachedShadowSprite;

        private void Reset()
        {
            positions = new PositionAnchor[]
            {
                new PositionAnchor { id = "pos_maru_end",    recommendedPose = PoseId.Random,     perspectiveScale = 1.00f },
                new PositionAnchor { id = "pos_puzzle_book", recommendedPose = PoseId.Random,     perspectiveScale = 0.85f },
                new PositionAnchor { id = "pos_cushion",     recommendedPose = PoseId.SleepyLoaf, perspectiveScale = 1.00f },
                new PositionAnchor { id = "pos_door_front",  recommendedPose = PoseId.IdleSit,    perspectiveScale = 0.85f },
                new PositionAnchor { id = "pos_sunlight",    recommendedPose = PoseId.SleepyLoaf, perspectiveScale = 0.85f },
                new PositionAnchor { id = "pos_eave",        recommendedPose = PoseId.Random,     perspectiveScale = 0.65f },
                new PositionAnchor { id = "pos_yard_view",   recommendedPose = PoseId.Random,     perspectiveScale = 0.65f },
                new PositionAnchor { id = "pos_lantern",     recommendedPose = PoseId.Random,     perspectiveScale = 0.65f }
            };
        }

        private void OnEnable()
        {
            if (ValidateRefs() == false)
            {
                return;
            }

            EnsureShadowSprite();
            SubscribeTimeOfDay();
            EnterInitialPosition();
            ApplyInitialTint();
            StartBreathing(currentPerspectiveScale);
            ScheduleNextBlink();
            ScheduleNextStretch();
            ScheduleNextMove();
        }

        private void OnDisable()
        {
            UnsubscribeTimeOfDay();
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
            tintTween?.Kill();
            breathTween = null;
            blinkTween = null;
            stretchTween = null;
            moveTween = null;
            moveScheduleTween = null;
            stretchScheduleTween = null;
            tintTween = null;
        }

        // ---------- Position movement ----------

        private void EnterInitialPosition()
        {
            int idx = PickNextVisibleIndex();

            if (idx < 0)
            {
                currentState = State.Hidden;
                bodyCanvasGroup.alpha = 0f;
                currentPerspectiveScale = UILayoutConstants.HomeNabiPerspectiveDefault;
                ApplyScaleAndShadow(currentPerspectiveScale);
                return;
            }

            SnapToAnchor(idx);
            float ps = EffectivePerspectiveScale(positions[idx].perspectiveScale);
            currentPerspectiveScale = ps;
            ApplyScaleAndShadow(ps);
            currentPose = ResolvePose(positions[idx].recommendedPose);
            bodyImage.sprite = ResolveSprite(currentPose);
            currentVisibleIndex = idx;
            currentState = State.Visible;
            bodyCanvasGroup.alpha = 1f;

            DebugLogger.Info(LogCategory.UI, "HomeNabiPositionSystem: initial pos=" + positions[idx].id + " pose=" + currentPose + " scale=" + ps);
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

            // 페이드아웃 종료 시점(alpha=0)에 위치/sprite/스케일/그림자 스냅 — 시각적으로 보이지 않음.
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
                float ps = EffectivePerspectiveScale(positions[idx].perspectiveScale);
                currentPerspectiveScale = ps;
                ApplyScaleAndShadow(ps);
                RestartBreathing(ps);
                currentPose = ResolvePose(positions[idx].recommendedPose);
                bodyImage.sprite = ResolveSprite(currentPose);
                currentVisibleIndex = idx;
                currentState = State.Visible;
                DebugLogger.Info(LogCategory.UI, "HomeNabiPositionSystem: -> " + positions[idx].id + " pose=" + currentPose + " scale=" + ps);
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

            if (body.parent == anchor.parent)
            {
                body.anchoredPosition = anchor.anchoredPosition;
            }
            else
            {
                body.position = anchor.position;
            }
        }

        // ---------- Perspective scale + shadow ----------

        private static float EffectivePerspectiveScale(float raw)
        {
            // 마이그레이션 안전: 0(직렬화 기본값) → 1.0 처리.
            return raw <= 0f ? UILayoutConstants.HomeNabiPerspectiveDefault : raw;
        }

        private void ApplyScaleAndShadow(float ps)
        {
            if (body != null)
            {
                body.localScale = Vector3.one * ps;
            }

            if (shadowImage != null)
            {
                Color c = shadowImage.color;
                // 멀리 갈수록 흐릿하게 — alpha를 perspective에 비례 감쇠.
                c.a = UILayoutConstants.HomeNabiShadowBaseAlpha * ps;
                shadowImage.color = c;
                // 크기는 body.localScale로 자동 (shadow가 body 자식이므로).
            }
        }

        // ---------- Time-of-day tint ----------

        private void SubscribeTimeOfDay()
        {
            if (timeOfDayController == null) { return; }
            timeOfDayController.TimeOfDayChanged += OnTimeOfDayChanged;
        }

        private void UnsubscribeTimeOfDay()
        {
            if (timeOfDayController == null) { return; }
            timeOfDayController.TimeOfDayChanged -= OnTimeOfDayChanged;
        }

        private void ApplyInitialTint()
        {
            if (timeOfDayController == null || bodyImage == null) { return; }

            Color target = ResolveTint(timeOfDayController.CurrentTimeOfDay);
            // 초기 진입은 페이드 없이 즉시 — 첫 프레임 깜빡임 방지.
            Color cur = bodyImage.color;
            target.a = cur.a;
            bodyImage.color = target;
        }

        private void OnTimeOfDayChanged(HomeTimeOfDayController.TimeOfDay tod)
        {
            if (bodyImage == null) { return; }

            tintTween?.Kill();

            Color target = ResolveTint(tod);
            target.a = bodyImage.color.a;

            DebugLogger.Info(LogCategory.UI, "HomeNabiPositionSystem: tint -> " + tod);

            tintTween = bodyImage
                .DOColor(target, UILayoutConstants.HomeNabiTintFadeSeconds)
                .SetEase(Ease.InOutSine)
                .SetUpdate(true);
        }

        private static Color ResolveTint(HomeTimeOfDayController.TimeOfDay tod)
        {
            switch (tod)
            {
                case HomeTimeOfDayController.TimeOfDay.Dawn:
                    return UILayoutConstants.HomeNabiTintDawn;
                case HomeTimeOfDayController.TimeOfDay.Day:
                    return UILayoutConstants.HomeNabiTintDay;
                case HomeTimeOfDayController.TimeOfDay.Evening:
                    return UILayoutConstants.HomeNabiTintEvening;
                case HomeTimeOfDayController.TimeOfDay.Night:
                    return UILayoutConstants.HomeNabiTintNight;
                default:
                    return Color.white;
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

        private void StartBreathing(float baseScale)
        {
            RestartBreathing(baseScale);
        }

        private void RestartBreathing(float baseScale)
        {
            if (body == null) { return; }

            breathTween?.Kill();

            float period = Random.Range(
                UILayoutConstants.HomeNabiQuietBreathSecondsMin,
                UILayoutConstants.HomeNabiQuietBreathSecondsMax);

            Vector3 from = Vector3.one * baseScale;
            Vector3 to = Vector3.one * (baseScale * UILayoutConstants.HomeNabiQuietBreathScale);

            body.localScale = from;

            breathTween = body
                .DOScale(to, period)
                .From(from)
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
            if (Random.value > UILayoutConstants.HomeNabiQuietStretchChance)
            {
                ScheduleNextStretch();
                return;
            }

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

        // ---------- Procedural shadow sprite ----------

        private void EnsureShadowSprite()
        {
            if (shadowImage == null) { return; }
            if (shadowImage.sprite != null) { return; }

            if (cachedShadowSprite == null)
            {
                cachedShadowSprite = CreateSoftEllipseSprite(128, 64);
            }

            shadowImage.sprite = cachedShadowSprite;
        }

        private static Sprite CreateSoftEllipseSprite(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.name = "NabiShadowEllipse";
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            Color[] pixels = new Color[width * height];
            float halfW = width * 0.5f;
            float halfH = height * 0.5f;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dx = (x + 0.5f - halfW) / halfW;
                    float dy = (y + 0.5f - halfH) / halfH;
                    float distSq = dx * dx + dy * dy;
                    float t = Mathf.Clamp01(1f - distSq);
                    // t^2로 더 부드러운 falloff — 가장자리가 점점 흐려짐.
                    float alpha = t * t;
                    pixels[y * width + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();

            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
        }
    }
}
