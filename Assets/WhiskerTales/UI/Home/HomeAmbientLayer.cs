using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace WhiskerTales.UI.Home
{
    /// <summary>
    /// 홈 화면의 ambient 모션 — 등불 glow + 나뭇잎 미세 sway만.
    /// 새/꽃잎/김 등 주의를 끄는 모션은 신규 구현하지 않는다 (원칙: 더 넣지 않기).
    /// </summary>
    public sealed class HomeAmbientLayer : MonoBehaviour
    {
        [Header("Lantern")]
        [SerializeField] private Image lanternGlow;

        [Header("Leaves")]
        [SerializeField] private RectTransform leafSway;

        private Tween lanternTween;
        private Tween leafTween;

        private void OnEnable()
        {
            StartLantern();
            StartLeafSway();
        }

        private void OnDisable()
        {
            lanternTween?.Kill();
            leafTween?.Kill();
        }

        private void StartLantern()
        {
            if (lanternGlow == null)
            {
                return;
            }

            lanternTween?.Kill();

            Color c = lanternGlow.color;
            c.a = UILayoutConstants.HomeLanternGlowMinAlpha;
            lanternGlow.color = c;

            lanternTween = lanternGlow
                .DOFade(UILayoutConstants.HomeLanternGlowMaxAlpha, UILayoutConstants.HomeLanternGlowSeconds)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(true);
        }

        private void StartLeafSway()
        {
            if (leafSway == null)
            {
                return;
            }

            leafTween?.Kill();

            float angle = UILayoutConstants.HomeLeafSwayAngle;

            leafSway.localEulerAngles = new Vector3(0f, 0f, -angle);

            leafTween = leafSway
                .DOLocalRotate(new Vector3(0f, 0f, angle), UILayoutConstants.HomeLeafSwaySeconds)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(true);
        }
    }
}
