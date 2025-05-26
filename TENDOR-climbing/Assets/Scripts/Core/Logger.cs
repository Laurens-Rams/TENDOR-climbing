using UnityEngine;
using System;

namespace TENDOR.Core
{
    /// <summary>
    /// Centralized logging system with level-based filtering and subsystem tagging
    /// </summary>
    public static class Logger
    {
        public enum LogLevel
        {
            Debug = 0,
            Info = 1,
            Warning = 2,
            Error = 3
        }

        public static LogLevel MinimumLogLevel = LogLevel.Info;

        private static readonly string[] LogLevelPrefixes = { "🔍", "ℹ️", "⚠️", "❌" };
        private static readonly string[] LogLevelNames = { "DEBUG", "INFO", "WARN", "ERROR" };

        public static void Log(string message, string subsystem = "CORE")
        {
            LogInternal(LogLevel.Info, subsystem, message);
        }

        public static void LogDebug(string message, string subsystem = "CORE")
        {
            LogInternal(LogLevel.Debug, subsystem, message);
        }

        public static void LogWarning(string message, string subsystem = "CORE")
        {
            LogInternal(LogLevel.Warning, subsystem, message);
        }

        public static void LogError(string message, string subsystem = "CORE")
        {
            LogInternal(LogLevel.Error, subsystem, message);
        }

        public static void LogError(Exception exception, string subsystem = "CORE")
        {
            LogInternal(LogLevel.Error, subsystem, $"{exception.Message}\n{exception.StackTrace}");
        }

        private static void LogInternal(LogLevel level, string subsystem, string message)
        {
            if (level < MinimumLogLevel) return;

            string formattedMessage = $"{LogLevelPrefixes[(int)level]} [{subsystem}] {message}";

            switch (level)
            {
                case LogLevel.Debug:
                case LogLevel.Info:
                    Debug.Log(formattedMessage);
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning(formattedMessage);
                    break;
                case LogLevel.Error:
                    Debug.LogError(formattedMessage);
                    break;
            }
        }

        /// <summary>
        /// Set minimum log level at runtime
        /// </summary>
        public static void SetLogLevel(LogLevel level)
        {
            MinimumLogLevel = level;
            Log($"Log level set to {LogLevelNames[(int)level]}", "LOGGER");
        }
    }
} 