using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WhiskerTales.Polish
{
    // Very faint dust motes drifting in the room. Smaller and slower than petals, alpha ~0.15,
    // with Perlin-noise sway so the motion looks airborne (not gravitational).
    [DisallowMultipleComponent]
    public sealed class DustMoteEmitter : MonoBehaviour
    {
        [SerializeField] private List<Sprite> sprites = new List<Sprite>();
        [SerializeField] private RectTransform spawnArea;
        [SerializeField] private float spawnIntervalSeconds = 1.4f;
        [SerializeField] private float moteLifetime = 14f;
        [SerializeField] private float driftSpeed = 18f;
        [SerializeField] private float swayAmplitude = 90f;
        [SerializeField] private float swayFrequency = 0.18f;
        [SerializeField] private float minSize = 14f;
        [SerializeField] private float maxSize = 28f;
        [SerializeField, Range(0f, 1f)] private float moteAlpha = 0.16f;
        [SerializeField] private int maxActive = 24;

        private float timer;
        private readonly List<Mote> active = new List<Mote>();
        private readonly Queue<RectTransform> pool = new Queue<RectTransform>();
        private RectTransform self;

        private struct Mote
        {
            public RectTransform rect;
            public Image image;
            public Vector2 origin;
            public float seedX;
            public float seedY;
            public float age;
        }

        private void Awake()
        {
            self = transform as RectTransform;

            if (spawnArea == null)
            {
                spawnArea = self;
            }
        }

        private void OnDisable()
        {
            for (int i = 0; i < active.Count; i++)
            {
                if (active[i].rect != null)
                {
                    active[i].rect.gameObject.SetActive(false);
                    pool.Enqueue(active[i].rect);
                }
            }
            active.Clear();
        }

        private void Update()
        {
            if (sprites == null || sprites.Count == 0)
            {
                return;
            }

            timer += Time.deltaTime;

            if (timer >= spawnIntervalSeconds && active.Count < maxActive)
            {
                timer = 0f;
                Spawn();
            }

            for (int i = active.Count - 1; i >= 0; i--)
            {
                Mote m = active[i];
                m.age += Time.deltaTime;

                if (m.age >= moteLifetime || m.rect == null)
                {
                    if (m.rect != null)
                    {
                        m.rect.gameObject.SetActive(false);
                        pool.Enqueue(m.rect);
                    }
                    active.RemoveAt(i);
                    continue;
                }

                float t = m.age;
                float swayX = swayAmplitude * (Mathf.PerlinNoise(m.seedX, t * swayFrequency) - 0.5f) * 2f;
                float swayY = swayAmplitude * 0.6f * (Mathf.PerlinNoise(m.seedY, t * swayFrequency) - 0.5f) * 2f;
                float drift = driftSpeed * t * 0.4f;

                m.rect.anchoredPosition = new Vector2(m.origin.x + swayX, m.origin.y + drift + swayY);

                if (m.image != null)
                {
                    float fadeIn = Mathf.Clamp01(t / 1.5f);
                    float fadeOut = 1f - Mathf.Clamp01((t - moteLifetime * 0.7f) / (moteLifetime * 0.3f));
                    float a = moteAlpha * Mathf.Min(fadeIn, fadeOut);
                    Color c = m.image.color;
                    c.a = a;
                    m.image.color = c;
                }

                active[i] = m;
            }
        }

        private void Spawn()
        {
            RectTransform rect = pool.Count > 0 ? pool.Dequeue() : CreateMoteImage();

            if (rect == null)
            {
                return;
            }

            Image image = rect.GetComponent<Image>();
            image.sprite = sprites[Random.Range(0, sprites.Count)];
            Color c = image.color;
            c.a = 0f;
            image.color = c;

            float size = Random.Range(minSize, maxSize);
            rect.sizeDelta = new Vector2(size, size);
            rect.localScale = Vector3.one;
            rect.gameObject.SetActive(true);

            float spawnX = spawnArea != null ? Random.Range(-spawnArea.rect.width * 0.5f, spawnArea.rect.width * 0.5f) : Random.Range(-540f, 540f);
            float spawnY = spawnArea != null ? Random.Range(-spawnArea.rect.height * 0.5f, spawnArea.rect.height * 0.5f) : Random.Range(-1100f, 1100f);

            Mote mote = new Mote
            {
                rect = rect,
                image = image,
                origin = new Vector2(spawnX, spawnY),
                seedX = Random.Range(0f, 1000f),
                seedY = Random.Range(0f, 1000f),
                age = 0f
            };
            rect.anchoredPosition = mote.origin;
            active.Add(mote);
        }

        private RectTransform CreateMoteImage()
        {
            GameObject go = new GameObject("Mote", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(self, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);

            Image image = go.GetComponent<Image>();
            image.raycastTarget = false;
            image.preserveAspect = true;
            return rect;
        }
    }
}
