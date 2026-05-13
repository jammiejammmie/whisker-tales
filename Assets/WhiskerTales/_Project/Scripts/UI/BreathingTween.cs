using UnityEngine;

namespace WhiskerTales.UI
{
    // Subtle idle "breathing" scale animation for cats / decorative sprites.
    // Pure transform sine wave; no DOTween dependency at the screen layer.
    [DisallowMultipleComponent]
    public sealed class BreathingTween : MonoBehaviour
    {
        [SerializeField] private float baseScale = 1f;
        [SerializeField] private float amplitude = 0.025f;
        [SerializeField] private float periodSeconds = 2.6f;
        [SerializeField] private float phaseOffset = 0f;

        private void OnEnable()
        {
            ApplyScale(0f);
        }

        private void Update()
        {
            ApplyScale(Time.time);
        }

        private void ApplyScale(float t)
        {
            if (periodSeconds <= 0.0001f)
            {
                return;
            }

            float omega = (Mathf.PI * 2f) / periodSeconds;
            float s = baseScale + amplitude * Mathf.Sin(omega * t + phaseOffset);
            transform.localScale = new Vector3(s, s, 1f);
        }
    }
}
