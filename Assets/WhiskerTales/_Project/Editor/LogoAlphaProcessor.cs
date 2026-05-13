#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace WhiskerTales.EditorTools
{
    // Thin wrapper around WhiteToAlphaProcessor for the home logo. The shipped logo PNG has a
    // solid white background that visibly squares against the cream HomeScreen.
    public static class LogoAlphaProcessor
    {
        public const string SourcePath = "Assets/WhiskerTales/Art/UI/logo_whisker_tales.png";
        public const string OutputPath = "Assets/WhiskerTales/_Project/Art/Generated/logo_whisker_tales_alpha.png";

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

        [MenuItem("Whisker Tales/V2/Process Logo Alpha")]
        public static void Process()
        {
            WhiteToAlphaProcessor.ProcessFile(SourcePath, OutputPath);
        }
    }
}
#endif
