#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace WhiskerTales.EditorTools
{
    // White-to-alpha processor for the V2 cat sprite. cat_nabi.png ships with a white backdrop
    // that draws a visible square inside CatRoom. Output is consumed by V2ScreenPrefabBuilders.
    // Slightly aggressive low threshold (0.80) — the cat's near-white fur edges should still
    // be fully opaque; only the off-white background fades.
    public static class CatAlphaProcessor
    {
        public const string SourcePath = "Assets/WhiskerTales/Art/Cats/cat_nabi.png";
        public const string OutputPath = "Assets/WhiskerTales/_Project/Art/Generated/cat_nabi_alpha.png";

        private const float Low = 0.92f;
        private const float High = 0.985f;

        [InitializeOnLoadMethod]
        private static void AutoProcessOnEditorLoad()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode == true)
            {
                return;
            }
            if (File.Exists(OutputPath) == true)
            {
                return;
            }
            EditorApplication.delayCall += TryProcess;
        }

        private static void TryProcess()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode == true)
            {
                return;
            }
            if (EditorApplication.isCompiling == true || EditorApplication.isUpdating == true)
            {
                EditorApplication.delayCall += TryProcess;
                return;
            }
            if (File.Exists(OutputPath) == true)
            {
                return;
            }
            Process();
        }

        [MenuItem("Whisker Tales/V2/Process Cat Alpha")]
        public static void Process()
        {
            WhiteToAlphaProcessor.ProcessFile(SourcePath, OutputPath, Low, High);
        }
    }
}
#endif
