using UnityEngine;
using WhiskerTales.Polish;

namespace WhiskerTales.UI.Screens
{
    public sealed class CatRoomScreenController : BackNavScreenBase
    {
        [Header("Cat")]
        [SerializeField] private RectTransform catRect;
        [SerializeField] private CatIdleLifeController idleAnimator;

        public override void Show(bool instant)
        {
            base.Show(instant);

            if (idleAnimator != null)
            {
                idleAnimator.enabled = true;
            }
        }

        public override void Hide(bool instant)
        {
            if (idleAnimator != null)
            {
                idleAnimator.enabled = false;
            }

            base.Hide(instant);
        }
    }
}
