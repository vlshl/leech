using System;
using System.IO;

namespace LeechSvc.Logger
{
    public interface ILogger
    {
        void AddInfo(string source, string msg);
        void AddError(string source, string msg);
        void AddException(string source, Exception ex);
    }

    public class DebugLogger : ILogger
    {
        private string _logPath = "";
        private object _lock = new object();

        public DebugLogger(ILeechConfig config)
        {
            _logPath = config.GetLogPath();
            if (!Directory.Exists(_logPath))
            {
                Directory.CreateDirectory(_logPath);
            }
        }

        public void AddInfo(string source, string msg)
        {
            string text = string.Format("{0} {1}:{2}\t{3}\n", DateTime.Now.ToString("HH:mm:ss.fff"), "INF", source, msg);
            lock (_lock)
            {
                File.AppendAllText(GetLogFilename(), text);
            }
        }

        public void AddError(string source, string msg)
        {
            string text = string.Format("{0} {1}:{2}\t{3}\n", DateTime.Now.ToString("HH:mm:ss.fff"), "ERR", source, msg);
            lock (_lock)
            {
                File.AppendAllText(GetLogFilename(), text);
            }
        }

        public void AddException(string source, Exception ex)
        {
            string text = string.Format("{0} {1}:{2}\t{3}\n", DateTime.Now.ToString("HH:mm:ss.fff"), "EXP", source, ex.ToString());
            lock (_lock)
            {
                File.AppendAllText(GetLogFilename(), text);
            }
        }

        private string GetLogFilename()
        {
            return Path.Combine(_logPath, DateTime.Today.ToString("yyyy-MM-dd") + ".log");
        }
    }
}
