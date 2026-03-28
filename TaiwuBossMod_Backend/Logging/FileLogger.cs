using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaiwuBossMod
{
    public static class FileLogger
    {
        private static readonly object _lock = new object();
        private static string _logPath;
        private static bool _initialized = false;

        public static void Init(string fileName = "backend_log.txt")
        {
            if (_initialized) return;

            try
            {
                // Get plugin directory
                string dir = TaiwuBossMod_Backend.BossPlugin.pluginDir;

                _logPath = Path.Combine(dir, fileName);

                // Overwrite the file to clear previous logs
                File.WriteAllText(_logPath, $"=== Backend Log Started: {DateTime.Now} ===\n", Encoding.UTF8);

                _initialized = true;
            }
            catch
            {
                try
                {
                    File.WriteAllText(_logPath, $"=== Backend Log Started: {DateTime.Now} ===\n", Encoding.UTF8);
                    _initialized = true;
                }
                catch
                {
                    // silently fail if even this fails
                }
            }
        }

        private static void Write(string level, string message)
        {
            if (!_initialized)
                Init();

            string logLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";

            lock (_lock)
            {
                try
                {
                    File.AppendAllText(_logPath, logLine + Environment.NewLine, Encoding.UTF8);
                }
                catch
                {
                    // swallow errors to avoid crashing the game
                }
            }
        }

        public static void Info(string message) => Write("INFO", message);
        public static void Warning(string message) => Write("WARN", message);
        public static void Error(string message) => Write("ERROR", message);
    }

}
