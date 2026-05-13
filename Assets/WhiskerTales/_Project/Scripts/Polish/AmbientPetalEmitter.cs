using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WhiskerTales.Core;

namespace WhiskerTales.Polish
{
    // Slow drifting cherry-blossom petals for Home/CatRoom ambience. Self-contained — instantiates
    // its own Image children, recycles them via a queue. Loads petal sprites at runtime via Resources.
    // Sprite resources live at Assets/WhiskerTales/Art/Effects/petal_0X.png but we load via AssetDB
    // path at edit time (prefab builder bakes the references). At runtime, prefab keeps the
    // sprite list.
    [DisallowMultipleComponent]
    public sealed class AmbientPetalEmitter : MonoBehaviour
    {
        [SerializeField] private List<Sprite> petalSprites = new List<Sprite>();
        [SerializeField] private RectTransform spawnArea;
        [SerializeField] private float spawnIntervalSeconds = 0.7f;
        [SerializeField] private float petalLifetime = 9.5f;
        [SerializeField] private float fallSpeed = 115f;
        [SerializeField] private float swayAmplitude = 80f;
        [SerializeField] private float swayFrequency = 0.55f;
        [SerializeField] private float minScale = 0.7f;
        [SerializeField] private float maxScale = 1.25f;
        [SerializeField] private int maxActive = 28;
        [SerializeField] private float petalImageSize = 100f;
        [SerializeField, Range(0f, 1f)] private float petalAlpha = 1f;

        private float timer;
        private readonly List<Petal> active = new List<Petal>();
        private readonly Queue<RectTransform> pool = new Queue<RectTransform>();
        private RectTransform self;

        private struct Petal
        {
            public RectTransform rect;
            public Image image;
            public Vector2 origin;
            public float seed;
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
            if (petalSprites == null || petalSprites.Count == 0)
            {
                return;
            }

            timer += Time.deltaTime;

            if (timer >= spawnIntervalSeconds && active.Count < maxActive)
            {
                timer = 0f;
                SpawnOne();
            }

            for (int i = active.Count - 1; i >= 0; i--)
            {
                Petal p = active[i];
                p.age += Time.deltaTime;
                float t = p.age;

                if (t >= petalLifetime || p.rect == null)
                {
                    if (p.rect != null)
                    {
                        p.rect.gameObject.SetActive(false);
                        pool.Enqueue(p.rect);
                    }
                    active.RemoveAt(i);
                    continue;
                }

                float sway = swayAmplitude * Mathf.Sin(swayFrequency * Mathf.PI * 2f * t + p.seed);
                float fall = fallSpeed * t;
                p.rect.anchoredPosition = new Vector2(p.origin.x + sway, p.origin.y - fall);
                p.rect.localRotation = Quaternion.Euler(0f, 0f, p.seed * 30f + t * 28f);

                if (p.image != null)
                {
                    float fadeT = Mathf.Clamp01((t - petalLifetime * 0.7f) / (petalLifetime * 0.3f));
                    float a = petalAlpha * (1f - fadeT);
                    Color c = p.image.color;
                    c.a = a;
                    p.image.color = c;
                }

                active[i] = p;
            }
        }

        private void SpawnOne()
        {
            RectTransform rect = pool.Count > 0 ? pool.Dequeue() : CreatePetalImage();

            if (rect == null)
            {
                return;
            }

            Image image = rect.GetComponent<Image>();
            image.sprite = petalSprites[Random.Range(0, petalSprites.Count)];
            Color c = image.color;
            c.a = petalAlpha;
            image.color = c;

            float scale = Random.Range(minScale, maxScale);
            rect.localScale = new Vector3(scale, scale, 1f);
            rect.gameObject.SetActive(true);

            float spawnX = spawnArea != null ? Random.Range(-spawnArea.rect.width * 0.5f, spawnArea.rect.width * 0.5f) : Random.Range(-540f, 540f);
            float spawnY = spawnArea != null ? spawnArea.rect.height * 0.5f + 100f : 1300f;

            Petal petal = new Petal
            {
                rect = rect,
                image = image,
                origin = new Vector2(spawnX, spawnY),
                seed = Random.Range(0f, Mathf.PI * 2f),
                age = 0f
            };
            rect.anchoredPosition = petal.origin;
            active.Add(petal);
        }

        private RectTransform CreatePetalImage()
        {
            GameObject go = new GameObject("Petal", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(self, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(petalImageSize, petalImageSize);

            Image image = go.GetComponent<Image>();
            image.raycastTarget = false;
            image.preserveAspect = true;
            return rect;
        }
    }
}
