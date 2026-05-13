using UnityEngine;

namespace WhiskerTales.Polish
{
    // Non-Spine "living still" controller for a monolithic cat sprite. Replaces CatIdleAnimator.
    //
    // Design goals:
    //   1. Body breathes at ~4s — but it's actually three sines at incommensurate periods
    //      (4.0 / 6.3 / 9.7) so the visible envelope never quite repeats.
    //   2. Tail sways always-on, very subtly — two sines at 5.5 / 8.3 layered.
    //   3. A slow head drift (13s) adds long-period micro-tilt that decoheres the others.
    //   4. Eyes blink + ears twitch are random-pulse channels: each draws its own next-fire
    //      time from a uniform interval, INDEPENDENT of the other channels and of the sine
    //      phases. Result: viewer can't predict when a blink or ear-flick happens.
    //   5. All channel phases are seeded randomly OnEnable so two cats wouldn't sync.
    //   6. Amplitudes are intentionally small — the silhouette stays mostly still. "살아있는
    //      정적 느낌" — alive while still.
    //
    // Honest limitation: cat_nabi.png is a single PNG (no rigged parts). Per-anatomy motion is
    // approximated via whole-sprite transform tweaks. A future art pass that splits the sprite
    // into body/head/ears/eyes/tail layers could move each independently.
    [DisallowMultipleComponent]
    public sealed class CatIdleLifeController : MonoBehaviour
    {
        [Header("Body Breathing (layered, incommensurate)")]
        [SerializeField] private float breathPeriod1 = 4.0f;
        [SerializeField] private float breathPeriod2 = 6.3f;
        [SerializeField] private float breathPeriod3 = 9.7f;
        [SerializeField] private float breathAmp1 = 0.024f;
        [SerializeField] private float breathAmp2 = 0.010f;
        [SerializeField] private float breathAmp3 = 0.006f;

        [Header("Tail Sway (2 sines)")]
        [SerializeField] private float tailPeriod1 = 5.5f;
        [SerializeField] private float tailPeriod2 = 8.3f;
        [SerializeField] private float tailAmp1Degrees = 0.9f;
        [SerializeField] private float tailAmp2Degrees = 0.4f;

        [Header("Head Drift (slow)")]
        [SerializeField] private float driftPeriod = 13.0f;
        [SerializeField] private float driftAmpDegrees = 0.5f;

        [Header("Eye Blink (random pulse)")]
        [SerializeField] private float blinkMinDelay = 3.0f;
        [SerializeField] private float blinkMaxDelay = 6.5f;
        [SerializeField] private float blinkDuration = 0.085f;
        [SerializeField] private float blinkSquashY = 0.95f;

        [Header("Ear Twitch (random pulse)")]
        [SerializeField] private float earMinDelay = 5.0f;
        [SerializeField] private float earMaxDelay = 11.0f;
        [SerializeField] private float earDuration = 0.10f;
        [SerializeField] private float earPulseX = 1.014f;

        // Per-channel phase seeds (set at OnEnable). Random Mathf.PI*2 range keeps each cat unique.
        private float seedB1, seedB2, seedB3;
        private float seedT1, seedT2;
        private float seedDrift;

        // Random-trigger state for blink/ear. Sentinel float.PositiveInfinity means "currently in
        // pulse window — don't reschedule yet". -1f means "scheduled, not yet firing".
        private float blinkUntil;
        private float nextBlinkAt;
        private float earUntil;
        private float nextEarAt;

        private Vector3 baseScale = Vector3.one;
        private CatGazeReaction gaze;

        private void OnEnable()
        {
            baseScale = transform.localScale;

            float twoPi = Mathf.PI * 2f;
            seedB1 = Random.Range(0f, twoPi);
            seedB2 = Random.Range(0f, twoPi);
            seedB3 = Random.Range(0f, twoPi);
            seedT1 = Random.Range(0f, twoPi);
            seedT2 = Random.Range(0f, twoPi);
            seedDrift = Random.Range(0f, twoPi);

            if (gaze == null)
            {
                gaze = GetComponent<CatGazeReaction>();
            }

            blinkUntil = -1f;
            earUntil = -1f;
            ScheduleNextBlink();
            ScheduleNextEar();
            Apply();
        }

        private void OnDisable()
        {
            // Snap back to neutral on screen-hide so the cat doesn't freeze mid-pulse.
            transform.localScale = baseScale;
            transform.localRotation = Quaternion.identity;
        }

        private void Update()
        {
            float t = Time.time;

            if (t >= nextBlinkAt)
            {
                blinkUntil = t + blinkDuration;
                nextBlinkAt = float.PositiveInfinity;
            }
            if (nextBlinkAt == float.PositiveInfinity && t >= blinkUntil)
            {
                ScheduleNextBlink();
            }

            if (t >= nextEarAt)
            {
                earUntil = t + earDuration;
                nextEarAt = float.PositiveInfinity;
            }
            if (nextEarAt == float.PositiveInfinity && t >= earUntil)
            {
                ScheduleNextEar();
            }

            Apply();
        }

        private void ScheduleNextBlink()
        {
            nextBlinkAt = Time.time + Random.Range(blinkMinDelay, blinkMaxDelay);
            blinkUntil = -1f;
        }

        private void ScheduleNextEar()
        {
            nextEarAt = Time.time + Random.Range(earMinDelay, earMaxDelay);
            earUntil = -1f;
        }

        private void Apply()
        {
            float t = Time.time;

            float breath = 0f;
            breath += breathAmp1 * Mathf.Sin(2f * Mathf.PI * t / SafePeriod(breathPeriod1) + seedB1);
            breath += breathAmp2 * Mathf.Sin(2f * Mathf.PI * t / SafePeriod(breathPeriod2) + seedB2);
            breath += breathAmp3 * Mathf.Sin(2f * Mathf.PI * t / SafePeriod(breathPeriod3) + seedB3);

            bool blinkActive = t < blinkUntil;
            bool earActive = t < earUntil;

            float gazeScale = gaze != null ? gaze.ScaleDelta : 0f;
            float gazeTilt = gaze != null ? gaze.TiltDeltaDegrees : 0f;

            float scaleY = baseScale.y * (1f + breath - (blinkActive ? (1f - blinkSquashY) : 0f) + gazeScale);
            float scaleX = baseScale.x * (1f + breath * 0.35f + (earActive ? (earPulseX - 1f) : 0f) + gazeScale);
            transform.localScale = new Vector3(scaleX, scaleY, baseScale.z);

            float tail = 0f;
            tail += tailAmp1Degrees * Mathf.Sin(2f * Mathf.PI * t / SafePeriod(tailPeriod1) + seedT1);
            tail += tailAmp2Degrees * Mathf.Sin(2f * Mathf.PI * t / SafePeriod(tailPeriod2) + seedT2);

            float drift = driftAmpDegrees * Mathf.Sin(2f * Mathf.PI * t / SafePeriod(driftPeriod) + seedDrift);

            transform.localRotation = Quaternion.Euler(0f, 0f, tail + drift + gazeTilt);
        }

        private static float SafePeriod(float p)
        {
            return Mathf.Max(0.001f, p);
        }
    }
}
