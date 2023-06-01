using BL;
using System;
using System.IO;

namespace LeechSvc
{
    /// <summary>
    /// Интерфейс конфигурации
    /// </summary>
    public interface ILeechConfig
    {
        int GetOpenSessionLocalTime();
        int GetCloseSessionLocalTime();
        int GetOpenTerminalLocalTime();
        int GetConnectLocalTime();
        int GetDisconnectLocalTime();
        int GetCloseTerminalLocalTime();
        string GetRootPath();
        string GetDbPath();
        string GetSessionDbPath(DateTime sessionDate);
        string GetLogPath();
        string GetBotsConfigPath();
        string GetBotsPath();
        string SecBoard { get; }
        int CorrectHours { get; }
        TimeSpan StartSessionMskTime { get; }
        TimeSpan EndSessionMskTime { get; }
        int DeleteBarHistoryOlderDays { get; }
        string Weekends { get; }
        string Holidays { get; }
        string Workdays { get; }
    }

    /// <summary>
    /// Конфигурация
    /// </summary>
    public class LeechConfig : ILeechConfig
    {
        private string _rootPath = "";

        public LeechConfig()
        {
            var location = System.Reflection.Assembly.GetEntryAssembly().Location;
            _rootPath = Path.GetDirectoryName(location);
        }

        /// <summary>
        /// Время инициализации (перед торговой сессией)
        /// </summary>
        /// <returns>ччммсс</returns>
        public int GetOpenSessionLocalTime()
        {
            var ts = Properties.Settings.Default.OpenSessionLocalTime;
            return ts.Hours * 10000 + ts.Minutes * 100 + ts.Seconds;
        }

        /// <summary>
        /// Время открытия терминала
        /// </summary>
        /// <returns>ччммсс</returns>
        public int GetOpenTerminalLocalTime()
        {
            var ts = Properties.Settings.Default.OpenTerminalLocalTime;
            return ts.Hours * 10000 + ts.Minutes * 100 + ts.Seconds;
        }

        /// <summary>
        /// Время установки соединения
        /// </summary>
        /// <returns>ччммсс</returns>
        public int GetConnectLocalTime()
        {
            var ts = Properties.Settings.Default.ConnectLocalTime;
            return ts.Hours * 10000 + ts.Minutes * 100 + ts.Seconds;
        }

        /// <summary>
        /// Время разрыва соединения
        /// </summary>
        /// <returns>ччммсс</returns>
        public int GetDisconnectLocalTime()
        {
            var ts = Properties.Settings.Default.DisconnectLocalTime;
            return ts.Hours * 10000 + ts.Minutes * 100 + ts.Seconds;
        }

        /// <summary>
        /// Время закрытия терминала
        /// </summary>
        /// <returns>ччммсс</returns>
        public int GetCloseTerminalLocalTime()
        {
            var ts = Properties.Settings.Default.CloseTerminalLocalTime;
            return ts.Hours * 10000 + ts.Minutes * 100 + ts.Seconds;
        }

        /// <summary>
        /// Время завершения (после окончания торговой сессии)
        /// </summary>
        /// <returns>ччммсс</returns>
        public int GetCloseSessionLocalTime()
        {
            var ts = Properties.Settings.Default.CloseSessionLocalTime;
            return ts.Hours * 10000 + ts.Minutes * 100 + ts.Seconds;
        }

        /// <summary>
        /// Базовый путь
        /// </summary>
        /// <returns>Путь</returns>
        public string GetRootPath()
        {
            return _rootPath;
        }

        /// <summary>
        /// Базовый путь данных
        /// </summary>
        /// <returns>Путь</returns>
        public string GetDbPath()
        {
            var path = Properties.Settings.Default.DbPath;
            if (string.IsNullOrWhiteSpace(path)) path = "db";
            if (Path.IsPathRooted(path))
                return path;
            else
                return Path.Combine(_rootPath, path);
        }

        /// <summary>
        /// Каталог сегодняшних данных
        /// </summary>
        /// <param name="sessionDate">Текущая дата</param>
        /// <returns></returns>
        public string GetSessionDbPath(DateTime sessionDate)
        {
            return Path.Combine(GetDbPath(), sessionDate.ToString("yyyy-MM-dd"));
        }

        /// <summary>
        /// Базовый путь логов
        /// </summary>
        /// <returns></returns>
        public string GetLogPath()
        {
            var path = Properties.Settings.Default.LogPath;
            if (string.IsNullOrWhiteSpace(path)) path = "logs";
            if (Path.IsPathRooted(path))
                return path;
            else
                return Path.Combine(_rootPath, path);
        }

        /// <summary>
        /// Путь к файлу конфигурации ботов
        /// </summary>
        /// <returns></returns>
        public string GetBotsConfigPath()
        {
            var path = Properties.Settings.Default.BotsConfigPath;
            if (string.IsNullOrWhiteSpace(path)) path = "BotsConfig.xml";
            if (Path.IsPathRooted(path))
                return path;
            else
                return Path.Combine(_rootPath, path);
        }

        /// <summary>
        /// Базовый путь расположения сборок с ботами
        /// </summary>
        /// <returns></returns>
        public string GetBotsPath()
        {
            var path = Properties.Settings.Default.BotsPath;
            if (string.IsNullOrWhiteSpace(path)) path = "bots";
            if (Path.IsPathRooted(path))
                return path;
            else
                return Path.Combine(_rootPath, path);
        }

        /// <summary>
        /// Режим торгов
        /// </summary>
        public string SecBoard
        {
            get
            {
                return Properties.Settings.Default.SecBoard;
            }
        }

        /// <summary>
        /// Корректировка времени.
        /// Торговый терминал выдает время в локальном часовом поясе, 
        /// нужно привести его к часовому поясу биржи.
        /// </summary>
        public int CorrectHours
        {
            get
            {
                return Properties.Settings.Default.CorrectHours;
            }
        }

        /// <summary>
        /// Время начала торговой сессии по часовому поясу биржи
        /// </summary>
        public TimeSpan StartSessionMskTime
        {
            get
            {
                return Properties.Settings.Default.StartSessionMskTime;
            }
        }

        /// <summary>
        /// Время окончания торговой сессии по часовому поясу биржи
        /// </summary>
        public TimeSpan EndSessionMskTime
        {
            get
            {
                return Properties.Settings.Default.EndSessionMskTime;
            }
        }

        public int DeleteBarHistoryOlderDays
        {
            get
            {
                return Properties.Settings.Default.DeleteBarHistoryOlderDays;
            }
        }

        public string Weekends
        {
            get
            {
                return Properties.Settings.Default.Weekends;
            }
        }

        public string Holidays
        {
            get
            {
                return Properties.Settings.Default.Holidays;
            }
        }

        public string Workdays
        {
            get
            {
                return Properties.Settings.Default.Workdays;
            }
        }
    }
}
