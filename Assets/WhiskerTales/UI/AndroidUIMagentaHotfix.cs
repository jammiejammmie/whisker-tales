using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace WhiskerTales.UI
{
    public sealed class AndroidUIMagentaHotfix : MonoBehaviour
    {
        private const float DelayedSweepSeconds = 3f;
        private const int PeriodicSweepCount = 30;
        private const float PeriodicSweepIntervalSeconds = 1f;

        private static bool bootstrapped;
        private static Sprite cachedWhiteSprite;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (bootstrapped) return;
            bootstrapped = true;

            GameObject go = new GameObject("__AndroidUIMagentaHotfix");
            Object.DontDestroyOnLoad(go);
            go.AddComponent<AndroidUIMagentaHotfix>();
        }

        private void Start()
        {
            SweepNow("immediate");
            StartCoroutine(DelayedSweep());
            StartCoroutine(PeriodicSweep());
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private IEnumerator PeriodicSweep()
        {
            for (int i = 1; i <= PeriodicSweepCount; i++)
            {
                yield return new WaitForSeconds(PeriodicSweepIntervalSeconds);
                SweepNow($"periodic-{i}");
            }
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SweepNow($"sceneLoaded:{scene.name}");
            StartCoroutine(DelayedSweep());
        }

        private IEnumerator DelayedSweep()
        {
            yield return new WaitForSeconds(DelayedSweepSeconds);
            SweepNow("delayed");
        }

        private static Sprite GetWhiteSprite()
        {
            if (cachedWhiteSprite == null)
            {
                Texture2D tex = Texture2D.whiteTexture;
                cachedWhiteSprite = Sprite.Create(
                    tex,
                    new Rect(0, 0, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f));
                cachedWhiteSprite.name = "AndroidUIMagentaHotfix_White";
            }
            return cachedWhiteSprite;
        }

        private static void SweepNow(string phase)
        {
            Image[] images = FindObjectsOfType<Image>(true);
            int patchedNullSprite = 0;
            int patchedBrokenTexture = 0;
            Sprite white = GetWhiteSprite();

            for (int i = 0; i < images.Length; i++)
            {
                Image img = images[i];
                if (img == null) continue;

                string reason = null;
                if (img.sprite == null)
                {
                    reason = "null-sprite";
                    patchedNullSprite++;
                }
                else if (img.sprite.texture == null)
                {
                    reason = "broken-texture";
                    patchedBrokenTexture++;
                }

                if (reason != null)
                {
                    img.sprite = white;
                    Debug.Log($"[MagentaHotfix:{phase}] patched {img.gameObject.name} ({reason})", img);
                }
            }

            int totalPatched = patchedNullSprite + patchedBrokenTexture;
            if (totalPatched > 0)
            {
                Debug.Log($"[MagentaHotfix:{phase}] scanned={images.Length} patched={totalPatched} (nullSprite={patchedNullSprite}, brokenTex={patchedBrokenTexture})");
            }
        }
    }
}
