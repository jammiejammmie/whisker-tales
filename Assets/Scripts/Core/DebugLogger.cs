using System;
using System.Diagnostics;
using UnityEngine;

namespace WhiskerTales.Core
{
    public enum LogCategory
    {
        Puzzle,
        UI,
        Audio,
        Save,
        Network,
        Analytics
    }

    public enum LogLevel
    {
        Verbose,
        Info,
        Warning,
        Error
    }

    /// <summary>
    /// Centralized logging for Whisker Tales.
    /// Calls are compiled only in UNITY_EDITOR or DEVELOPMENT_BUILD, so release builds are stripped.
    /// </summary>
    public static class DebugLogger
    {
        public static LogLevel MinimumLevel = LogLevel.Verbose;

        public static bool PuzzleEnabled = true;
        public static bool UIEnabled = true;
        public static bool AudioEnabled = true;
        public static bool SaveEnabled = true;
        public static bool NetworkEnabled = true;
        public static bool AnalyticsEnabled = true;

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Verbose(LogCategory category, string message, UnityEngine.Object context = null)
        {
            Log(category, LogLevel.Verbose, message, context);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Info(LogCategory category, string message, UnityEngine.Object context = null)
        {
            Log(category, LogLevel.Info, message, context);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Warning(LogCategory category, string message, UnityEngine.Object context = null)
        {
            Log(category, LogLevel.Warning, message, context);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Error(LogCategory category, string message, UnityEngine.Object context = null)
        {
            Log(category, LogLevel.Error, message, context);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Exception(LogCategory category, Exception exception, UnityEngine.Object context = null)
        {
            if (exception == null || !IsEnabled(category) || LogLevel.Error < MinimumLevel) return;
            UnityEngine.Debug.LogError($"[{category}] Exception: {exception.GetType().Name}: {exception.Message}\n{exception.StackTrace}", context);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void Log(LogCategory category, LogLevel level, string message, UnityEngine.Object context)
        {
            if (!IsEnabled(category) || level < MinimumLevel) return;

            string formatted = $"[{category}] {message}";
            switch (level)
            {
                case LogLevel.Warning:
                    UnityEngine.Debug.LogWarning(formatted, context);
                    break;
                case LogLevel.Error:
                    UnityEngine.Debug.LogError(formatted, context);
                    break;
                default:
                    UnityEngine.Debug.Log(formatted, context);
                    break;
            }
        }

        public static bool IsEnabled(LogCategory category)
        {
            switch (category)
            {
                case LogCategory.Puzzle: return PuzzleEnabled;
                case LogCategory.UI: return UIEnabled;
                case LogCategory.Audio: return AudioEnabled;
                case LogCategory.Save: return SaveEnabled;
                case LogCategory.Network: return NetworkEnabled;
                case LogCategory.Analytics: return AnalyticsEnabled;
                default: return true;
            }
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Whisker Tales/Debug Logging/Enable All")]
        private static void EditorEnableAll()
        {
            PuzzleEnabled = UIEnabled = AudioEnabled = SaveEnabled = NetworkEnabled = AnalyticsEnabled = true;
            UnityEngine.Debug.Log("[DebugLogger] Enabled all categories");
        }

        [UnityEditor.MenuItem("Whisker Tales/Debug Logging/Disable All")]
        private static void EditorDisableAll()
        {
            PuzzleEnabled = UIEnabled = AudioEnabled = SaveEnabled = NetworkEnabled = AnalyticsEnabled = false;
            UnityEngine.Debug.Log("[DebugLogger] Disabled all categories");
        }

        [UnityEditor.MenuItem("Whisker Tales/Debug Logging/Set Minimum/Verbose")]
        private static void EditorSetVerbose() => MinimumLevel = LogLevel.Verbose;

        [UnityEditor.MenuItem("Whisker Tales/Debug Logging/Set Minimum/Info")]
        private static void EditorSetInfo() => MinimumLevel = LogLevel.Info;

        [UnityEditor.MenuItem("Whisker Tales/Debug Logging/Set Minimum/Warning")]
        private static void EditorSetWarning() => MinimumLevel = LogLevel.Warning;

        [UnityEditor.MenuItem("Whisker Tales/Debug Logging/Set Minimum/Error")]
        private static void EditorSetError() => MinimumLevel = LogLevel.Error;
#endif
    }
}
