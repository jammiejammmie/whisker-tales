using UnityEngine;
using UnityEngine.EventSystems;

namespace WhiskerTales.Polish
{
    // Single-PNG cat doesn't have an eye rig, so "gaze reaction" is a brief tilt + scale bump
    // toward the tap direction. Caller (CatRoom prefab builder) attaches this on the cat Image
    // and ensures the Image is raycastTarget=true so the click reaches us.
    // Combines with CatIdleLifeController — both write to localRotation/localScale; CatGazeReaction's
    // window is short (~0.5s) and additive-on-top via a runtime offset accumulator stored on this
    // component (not transform). The animator base scale/rotation continues; we add a transient
    // "look" delta during the reaction.
    [DisallowMultipleComponent]
    public sealed class CatGazeReaction : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private float maxTiltDegrees = 5f;
        [SerializeField] private float maxScaleBump = 0.06f;
        [SerializeField] private float reactSeconds = 0.55f;

        private float reactStartTime = float.NegativeInfinity;
        private float reactDirectionSign = 0f;
        private RectTransform rect;

        public bool IsReacting
        {
            get { return Time.time - reactStartTime < reactSeconds; }
        }

        public float TiltDeltaDegrees
        {
            get
            {
                if (IsReacting == false)
                {
                    return 0f;
                }

                float t = (Time.time - reactStartTime) / reactSeconds;
                float fall = 1f - Mathf.Clamp01(t);
                return -reactDirectionSign * maxTiltDegrees * fall * fall;
            }
        }

        public float ScaleDelta
        {
            get
            {
                if (IsReacting == false)
                {
                    return 0f;
                }

                float t = (Time.time - reactStartTime) / reactSeconds;
                float pulse = Mathf.Sin(Mathf.PI * Mathf.Clamp01(t));
                return maxScaleBump * pulse;
            }
        }

        private void Awake()
        {
            rect = (RectTransform)transform;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData == null || rect == null)
            {
                return;
            }

            Camera cam = eventData.pressEventCamera;
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, eventData.position, cam, out localPoint);
            reactDirectionSign = localPoint.x >= 0f ? 1f : -1f;
            reactStartTime = Time.time;
        }
    }
}
