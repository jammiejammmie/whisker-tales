using UnityEngine;
using UnityEngine.UI;

namespace WhiskerTales.Polish
{
    // Sine-wave alpha pulse for a soft warm "lantern" glow. Adds a subtle flicker overlay so
    // the steady sine doesn't feel mechanical.
    [RequireComponent(typeof(Image))]
    [DisallowMultipleComponent]
    public sealed class LanternGlowPulse : MonoBehaviour
    {
        [SerializeField] private float baseAlpha = 0.55f;
        [SerializeField] private float pulseAmplitude = 0.22f;
        [SerializeField] private float pulsePeriodSeconds = 3.4f;
        [SerializeField] private float flickerAmplitude = 0.08f;
        [SerializeField] private float flickerFrequency = 6.7f;

        private Image image;
        private float seed;

        private void Awake()
        {
            image = GetComponent<Image>();
            seed = Random.Range(0f, 100f);
        }

        private void OnEnable()
        {
            ApplyAlpha(0f);
        }

        private void Update()
        {
            ApplyAlpha(Time.time);
        }

        private void ApplyAlpha(float t)
        {
            if (image == null || pulsePeriodSeconds <= 0.0001f)
            {
                return;
            }

            float omega = (Mathf.PI * 2f) / pulsePeriodSeconds;
            float pulse = pulseAmplitude * Mathf.Sin(omega * t + seed);
            float flicker = flickerAmplitude * (Mathf.PerlinNoise(seed, t * flickerFrequency) - 0.5f) * 2f;
            float alpha = Mathf.Clamp01(baseAlpha + pulse + flicker);

            Color c = image.color;
            c.a = alpha;
            image.color = c;
        }
    }
}
