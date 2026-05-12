using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace WhiskerTales.UI
{
    public sealed class UIMaterialDiagnostic : MonoBehaviour
    {
        private const float DelayedScanSeconds = 3f;
        private static bool bootstrapped;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (bootstrapped) { return; }
            bootstrapped = true;
            GameObject go = new GameObject("__UIMaterialDiagnostic");
            Object.DontDestroyOnLoad(go);
            go.AddComponent<UIMaterialDiagnostic>();
        }

        private void Start()
        {
            ScanNow("immediate");
            StartCoroutine(DelayedScan());
        }

        private IEnumerator DelayedScan()
        {
            yield return new WaitForSeconds(DelayedScanSeconds);
            ScanNow("delayed");
        }

        private static void ScanNow(string phase)
        {
            ScanGraphics(phase);
            ScanRenderers(phase);
        }

        private static void ScanGraphics(string phase)
        {
            Graphic[] graphics = FindObjectsOfType<Graphic>(true);
            int unsupportedCount = 0;
            int magentaSuspectCount = 0;
            for (int i = 0; i < graphics.Length; i++)
            {
                Graphic g = graphics[i];
                if (g == null) { continue; }

                Material mat = g.material;
                Shader shader = mat != null ? mat.shader : null;
                string shaderName = shader != null ? shader.name : "NULL_SHADER";
                string matName = mat != null ? mat.name : "NULL_MAT";
                string typeName = g.GetType().Name;
                Color c = g.color;
                string colorStr = $"({c.r:F2},{c.g:F2},{c.b:F2},{c.a:F2})";

                string spriteSuffix = "";
                if (g is Image img)
                {
                    if (img.sprite != null)
                    {
                        string texName = img.sprite.texture != null ? img.sprite.texture.name : "NULL_TEX";
                        spriteSuffix = $" | sprite={img.sprite.name} | tex={texName}";
                    }
                    else
                    {
                        spriteSuffix = " | sprite=NULL";
                    }
                }

                bool isMagenta = c.r > 0.9f && c.g < 0.1f && c.b > 0.9f;

                Debug.Log(
                    $"[DIAG:{phase}] {typeName} '{g.gameObject.name}' | mat={matName} | shader={shaderName} | color={colorStr}{spriteSuffix}",
                    g);

                if (isMagenta)
                {
                    Debug.LogWarning(
                        $"[DIAG:{phase}] MAGENTA COLOR on {typeName} '{g.gameObject.name}': {colorStr}",
                        g);
                    magentaSuspectCount++;
                }

                if (shader != null && shader.isSupported == false)
                {
                    Debug.LogWarning(
                        $"[DIAG:{phase}] UNSUPPORTED SHADER on {typeName} '{g.gameObject.name}': {shaderName}",
                        g);
                    unsupportedCount++;
                }
            }
            Debug.Log($"[DIAG:{phase}] Graphic scan complete. count={graphics.Length} unsupported={unsupportedCount} magentaColor={magentaSuspectCount}");
        }

        private static void ScanRenderers(string phase)
        {
            Renderer[] renderers = FindObjectsOfType<Renderer>(true);
            int unsupportedCount = 0;
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer r = renderers[i];
                if (r == null) { continue; }

                Material mat = r.sharedMaterial;
                Shader shader = mat != null ? mat.shader : null;
                string shaderName = shader != null ? shader.name : "NULL_SHADER";
                string matName = mat != null ? mat.name : "NULL_MAT";
                string typeName = r.GetType().Name;

                string spriteSuffix = "";
                if (r is SpriteRenderer sr)
                {
                    if (sr.sprite != null)
                    {
                        string texName = sr.sprite.texture != null ? sr.sprite.texture.name : "NULL_TEX";
                        spriteSuffix = $" | sprite={sr.sprite.name} | tex={texName}";
                    }
                    else
                    {
                        spriteSuffix = " | sprite=NULL";
                    }
                    Color c = sr.color;
                    spriteSuffix += $" | color=({c.r:F2},{c.g:F2},{c.b:F2},{c.a:F2})";
                }

                Debug.Log(
                    $"[DIAG-R:{phase}] {typeName} '{r.gameObject.name}' | mat={matName} | shader={shaderName}{spriteSuffix}",
                    r);

                if (shader != null && shader.isSupported == false)
                {
                    Debug.LogWarning(
                        $"[DIAG-R:{phase}] UNSUPPORTED SHADER on {typeName} '{r.gameObject.name}': {shaderName}",
                        r);
                    unsupportedCount++;
                }
            }
            Debug.Log($"[DIAG-R:{phase}] Renderer scan complete. count={renderers.Length} unsupported={unsupportedCount}");
        }
    }
}
