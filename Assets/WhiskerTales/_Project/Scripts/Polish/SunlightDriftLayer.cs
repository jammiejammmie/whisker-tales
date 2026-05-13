using UnityEngine;
using UnityEngine.UI;

namespace WhiskerTales.Polish
{
    // Soft warm light shape that slowly drifts horizontally, suggesting sunlight from a window
    // sliding across the room. Pairs with a radial-gradient sprite (lantern_glow works) tinted
    // warm-yellow at low alpha.
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    public sealed class SunlightDriftLayer : MonoBehaviour
    {
        [SerializeField] private float driftAmplitude = 220f;
        [SerializeField] private float driftPeriodSeconds = 22f;
        [SerializeField] private float verticalAmplitude = 40f;
        [SerializeField] private float verticalPeriodSeconds = 31f;

        private RectTransform rect;
        private Vector2 origin;
        private float seed;

        private void Awake()
        {
            rect = (RectTransform)transform;
            origin = rect.anchoredPosition;
            seed = Random.Range(0f, 10f);
        }

        private void Update()
        {
            float omegaH = (Mathf.PI * 2f) / Mathf.Max(0.001f, driftPeriodSeconds);
            float omegaV = (Mathf.PI * 2f) / Mathf.Max(0.001f, verticalPeriodSeconds);
            float t = Time.time;
            float dx = driftAmplitude * Mathf.Sin(omegaH * t + seed);
            float dy = verticalAmplitude * Mathf.Sin(omegaV * t + seed * 0.7f);
            rect.anchoredPosition = new Vector2(origin.x + dx, origin.y + dy);
        }
    }
}
