using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using WhiskerTales.Core;
namespace WhiskerTales.Feel
{
    public enum ParticleKind
    {
        Petal,
        Sparkle,
        MatchBurst,
        Confetti
    }

    public sealed class ParticlePoolManager : MonoBehaviour
    {
        public static ParticlePoolManager Instance { get; private set; }

        [SerializeField] private RectTransform particleRoot;
        [SerializeField] private List<Sprite> petals = new List<Sprite>();
        [SerializeField] private List<Sprite> sparkles = new List<Sprite>();
        [SerializeField] private List<Sprite> matchBursts = new List<Sprite>();
        [SerializeField] private List<Sprite> confetti = new List<Sprite>();
        [SerializeField] private int prewarmPerKind = 20;

        private readonly Queue<Image> pool = new Queue<Image>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (particleRoot == null)
            {
                particleRoot = transform as RectTransform;
            }

            Prewarm();
        }

        private void OnEnable()
        {
            WhiskerTales.Core.GameEvents.OnMatchFound += HandleMatchFound;
            WhiskerTales.Core.GameEvents.OnLevelCompleted += HandleLevelCompleted;
            WhiskerTales.Core.GameEvents.OnSpecialTileCreated += HandleSpecialTileCreated;
        }

        private void OnDisable()
        {
            WhiskerTales.Core.GameEvents.OnMatchFound -= HandleMatchFound;
            WhiskerTales.Core.GameEvents.OnLevelCompleted -= HandleLevelCompleted;
            WhiskerTales.Core.GameEvents.OnSpecialTileCreated -= HandleSpecialTileCreated;
        }

        public void Play(ParticleKind kind, Vector2 anchoredPosition, int count = 1)
        {
            List<Sprite> sprites = GetSprites(kind);

            if (sprites == null || sprites.Count == 0)
            {
                DebugLogger.Warning(LogCategory.UI, $"Particle sprites missing: {kind}");
                return;
            }

            int safeCount = Mathf.Max(1, count);

            for (int i = 0; i < safeCount; i++)
            {
                Image image = GetImage();

                if (image == null)
                {
                    continue;
                }

                image.sprite = sprites[Random.Range(0, sprites.Count)];
                image.rectTransform.anchoredPosition = anchoredPosition + Random.insideUnitCircle * 70f;
                image.rectTransform.localScale = Vector3.one * Random.Range(0.75f, 1.25f);
                image.rectTransform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
                StartCoroutine(PlayAndRelease(image, kind));
            }
        }

        public void PlayPetalRain(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 pos = new Vector2(Random.Range(-520f, 520f), Random.Range(100f, 960f));
                Play(ParticleKind.Petal, pos, 1);
            }
        }

        private void HandleMatchFound(int count)
        {
            Play(ParticleKind.Sparkle, Vector2.zero, Mathf.Clamp(count, 1, 8));
            Play(ParticleKind.MatchBurst, Vector2.zero, 1);
        }

        private void HandleLevelCompleted(int level, int stars)
        {
            PlayPetalRain(24);
        }

        private void HandleSpecialTileCreated(WhiskerTales.Puzzle.SpecialItemType type)
        {
            Play(ParticleKind.Sparkle, Vector2.zero, 6);
        }

        private void Prewarm()
        {
            int total = Mathf.Max(1, prewarmPerKind) * 4;

            for (int i = 0; i < total; i++)
            {
                Image image = CreateImage();
                Release(image);
            }
        }

        private Image GetImage()
        {
            Image image = null;

            while (pool.Count > 0 && image == null)
            {
                image = pool.Dequeue();
            }

            if (image == null)
            {
                image = CreateImage();
            }

            image.gameObject.SetActive(true);
            Color color = image.color;
            color.a = 1f;
            image.color = color;
            return image;
        }

        private Image CreateImage()
        {
            GameObject go = new GameObject("ParticleImage", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(particleRoot != null ? particleRoot : transform, false);
            Image image = go.GetComponent<Image>();
            image.raycastTarget = false;
            return image;
        }

        private IEnumerator PlayAndRelease(Image image, ParticleKind kind)
        {
            if (image == null)
            {
                yield break;
            }

            RectTransform rt = image.rectTransform;
            Vector2 start = rt.anchoredPosition;
            float duration = kind == ParticleKind.Petal ? 1.8f : 0.65f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                rt.anchoredPosition = start + new Vector2(0f, -120f * t);
                rt.localScale = Vector3.one * Mathf.Lerp(rt.localScale.x, 0.2f, t);
                Color color = image.color;
                color.a = 1f - t;
                image.color = color;
                yield return null;
            }

            Release(image);
        }

        private void Release(Image image)
        {
            if (image == null)
            {
                return;
            }

            image.sprite = null;
            image.rectTransform.anchoredPosition = Vector2.zero;
            image.rectTransform.localScale = Vector3.one;
            image.gameObject.SetActive(false);
            pool.Enqueue(image);
        }

        private List<Sprite> GetSprites(ParticleKind kind)
        {
            if (kind == ParticleKind.Petal)
            {
                return petals;
            }

            if (kind == ParticleKind.Sparkle)
            {
                return sparkles;
            }

            if (kind == ParticleKind.MatchBurst)
            {
                return matchBursts;
            }

            return confetti;
        }
    }
}
