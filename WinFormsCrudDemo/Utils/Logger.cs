using System;
using System.IO;

namespace WinFormsCrudDemo.Utils
{
    public static class Logger
    {
        private static readonly string LogDir =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

        private static string TodayFile =>
            Path.Combine(LogDir, $"app_{DateTime.Now:yyyyMMdd}.log");

        public static void Log(string message)
        {
            try
            {
                Directory.CreateDirectory(LogDir);
                File.AppendAllText(TodayFile,
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}");
            }
            catch { /* no re-lanzar: el log nunca debe romper la app */ }
        }

        public static void Log(Exception ex, string? context = null)
        {
            var msg = context == null
                ? ex.ToString()
                : $"[{context}] {ex}";
            Log(msg);
        }

        public static string GetLogPath() => TodayFile;
    }
}
