using DG.Tweening;
using TMPro;
using UnityEngine;

namespace WhiskerTales.UI.Home
{
    /// <summary>
    /// 홈 카피 "오늘도 여기 있어요." — 앱 진입 후 천천히 fade-in.
    /// 단일 문장. 향후 로테이션 문장 리스트로 확장 가능 (현재는 1개 고정).
    /// </summary>
    public sealed class HomeCopyFader : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        // 마침표 색을 명시적으로 크림(#F5F1E8)으로 강제 — 기본 TMP 폰트의 sprite/material 영향으로
        // 마침표가 다른 색으로 보이는 이슈 회피. richText는 TMP 기본값(true)이라 별도 설정 불필요.
        [SerializeField] private string copy = "오늘도 여기 있어요<color=#F5F1E8>.</color>";

        private Tween fadeTween;

        private void OnEnable()
        {
            if (text == null)
            {
                return;
            }

            text.text = copy;
            text.alpha = 0f;

            fadeTween?.Kill();

            fadeTween = DOVirtual
                .DelayedCall(UILayoutConstants.HomeCopyFadeStartSeconds, StartFade, false)
                .SetUpdate(true);
        }

        private void OnDisable()
        {
            fadeTween?.Kill();
        }

        private void StartFade()
        {
            if (text == null)
            {
                return;
            }

            fadeTween = DOTween
                .To(() => text.alpha, a => text.alpha = a, 1f, UILayoutConstants.HomeCopyFadeDurationSeconds)
                .SetEase(Ease.InOutSine)
                .SetUpdate(true);
        }
    }
}
