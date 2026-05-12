using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WhiskerTales.Core;

namespace WhiskerTales.UI.Home
{
    /// <summary>
    /// 홈 화면의 나비 — idle breathing/blink + "알아차림" 시퀀스 + 터치 반응.
    /// 위치 이동 없음. "방금 뭔가 살아있었던 것 같은데?" 수준의 절제된 움직임만.
    /// </summary>
    public sealed class HomeNabi : MonoBehaviour, IPointerClickHandler
    {
        [Header("Targets")]
        [SerializeField] private RectTransform body;
        [SerializeField] private RectTransform earLeft;
        [SerializeField] private CanvasGroup eyeMask;
        [SerializeField] private Image touchZoneImage;

        private Tween breathTween;
        private Tween blinkTween;
        private Tween earTween;
        private float spawnTime;
        private bool earNoticeFired;
        private bool blinkNoticeFired;

        private void Awake()
        {
            if (touchZoneImage != null)
            {
                Color c = touchZoneImage.color;
                c.a = Mathf.Max(c.a, 0.01f);
                touchZoneImage.color = c;
                touchZoneImage.raycastTarget = true;
            }

            if (eyeMask != null)
            {
                eyeMask.alpha = 0f;
                eyeMask.blocksRaycasts = false;
                eyeMask.interactable = false;
            }
        }

        private void OnEnable()
        {
            spawnTime = Time.unscaledTime;
            earNoticeFired = false;
            blinkNoticeFired = false;
            StartBreathing();
            ScheduleNextBlink();
        }

        private void OnDisable()
        {
            breathTween?.Kill();
            blinkTween?.Kill();
            earTween?.Kill();
        }

        private void Update()
        {
            if (earNoticeFired == true && blinkNoticeFired == true)
            {
                return;
            }

            float t = Time.unscaledTime - spawnTime;

            if (earNoticeFired == false && t >= UILayoutConstants.HomeNabiNoticeEarTime)
            {
                earNoticeFired = true;
                PlayEarTwitch();
            }

            if (blinkNoticeFired == false && t >= UILayoutConstants.HomeNabiNoticeBlinkTime)
            {
                blinkNoticeFired = true;
                PlaySlowBlink();
            }
        }

        private void StartBreathing()
        {
            if (body == null)
            {
                return;
            }

            breathTween?.Kill();

            body.localScale = Vector3.one;

            breathTween = body
                .DOScale(UILayoutConstants.HomeNabiBreathScale, UILayoutConstants.HomeNabiBreathSeconds)
                .From(Vector3.one)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(true);
        }

        private void ScheduleNextBlink()
        {
            if (eyeMask == null)
            {
                return;
            }

            float wait = Random.Range(UILayoutConstants.HomeNabiBlinkMinSeconds, UILayoutConstants.HomeNabiBlinkMaxSeconds);

            blinkTween?.Kill();

            blinkTween = DOVirtual.DelayedCall(wait, PlayBlink, false).SetUpdate(true);
        }

        private void PlayBlink()
        {
            if (eyeMask == null)
            {
                return;
            }

            float half = UILayoutConstants.HomeNabiBlinkDurationSeconds * 0.5f;

            Sequence seq = DOTween.Sequence().SetUpdate(true);
            seq.Append(eyeMask.DOFade(1f, half).SetEase(Ease.OutQuad));
            seq.Append(eyeMask.DOFade(0f, half).SetEase(Ease.InQuad));
            seq.OnComplete(ScheduleNextBlink);

            blinkTween = seq;
        }

        private void PlaySlowBlink()
        {
            if (eyeMask == null)
            {
                return;
            }

            float duration = UILayoutConstants.HomeNabiBlinkDurationSeconds * 1.6f;
            float half = duration * 0.5f;

            blinkTween?.Kill();

            Sequence seq = DOTween.Sequence().SetUpdate(true);
            seq.Append(eyeMask.DOFade(1f, half).SetEase(Ease.InOutSine));
            seq.Append(eyeMask.DOFade(0f, half).SetEase(Ease.InOutSine));
            seq.OnComplete(ScheduleNextBlink);

            blinkTween = seq;
        }

        private void PlayEarTwitch()
        {
            if (earLeft == null)
            {
                return;
            }

            earTween?.Kill();

            float angle = UILayoutConstants.HomeNabiEarTwitchAngle;
            float dur = UILayoutConstants.HomeNabiEarTwitchSeconds;

            Sequence seq = DOTween.Sequence().SetUpdate(true);
            seq.Append(earLeft.DOLocalRotate(new Vector3(0f, 0f, -angle), dur).SetEase(Ease.OutQuad));
            seq.Append(earLeft.DOLocalRotate(new Vector3(0f, 0f, angle * 0.4f), dur).SetEase(Ease.InOutSine));
            seq.Append(earLeft.DOLocalRotate(Vector3.zero, dur).SetEase(Ease.InQuad));

            earTween = seq;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            DebugLogger.Info(LogCategory.UI, "Home → Nabi touched");
            PlayEarTwitch();
            PlaySlowBlink();
        }
    }
}
