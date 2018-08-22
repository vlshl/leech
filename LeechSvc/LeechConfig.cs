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
        int GetOpenSessionTime();
        int GetCloseSessionTime();
        int GetOpenTerminalTime();
        int GetConnectTime();
        int GetDisconnectTime();
        int GetCloseTerminalTime();
        string GetRootPath();
        string GetDbPath();
        string GetTodayDbPath();
        string GetAllTradesDbPath();
        string GetLogPath();
        string GetBotsConfigPath();
        string GetBotsPath();
        string SecBoard { get; }
        int CorrectHours { get; }
        TimeSpan StartSessionTime { get; }
        TimeSpan EndSessionTime { get; }
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
        public int GetOpenSessionTime()
        {
            var ts = Properties.Settings.Default.OpenSessionTime;
            return ts.Hours * 10000 + ts.Minutes * 100 + ts.Seconds;
        }

        /// <summary>
        /// Время открытия терминала
        /// </summary>
        /// <returns>ччммсс</returns>
        public int GetOpenTerminalTime()
        {
            var ts = Properties.Settings.Default.OpenTerminalTime;
            return ts.Hours * 10000 + ts.Minutes * 100 + ts.Seconds;
        }

        /// <summary>
        /// Время установки соединения
        /// </summary>
        /// <returns>ччммсс</returns>
        public int GetConnectTime()
        {
            var ts = Properties.Settings.Default.ConnectTime;
            return ts.Hours * 10000 + ts.Minutes * 100 + ts.Seconds;
        }

        /// <summary>
        /// Время разрыва соединения
        /// </summary>
        /// <returns>ччммсс</returns>
        public int GetDisconnectTime()
        {
            var ts = Properties.Settings.Default.DisconnectTime;
            return ts.Hours * 10000 + ts.Minutes * 100 + ts.Seconds;
        }

        /// <summary>
        /// Время закрытия терминала
        /// </summary>
        /// <returns>ччммсс</returns>
        public int GetCloseTerminalTime()
        {
            var ts = Properties.Settings.Default.CloseTerminalTime;
            return ts.Hours * 10000 + ts.Minutes * 100 + ts.Seconds;
        }

        /// <summary>
        /// Время завершения (после окончания торговой сессии)
        /// </summary>
        /// <returns>ччммсс</returns>
        public int GetCloseSessionTime()
        {
            var ts = Properties.Settings.Default.CloseSessionTime;
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
        /// <returns></returns>
        public string GetTodayDbPath()
        {
            return Path.Combine(GetDbPath(),
                DateTime.Today.ToString("yyyy-MM-dd"));
        }

        /// <summary>
        /// Каталог данных по всем сделкам
        /// </summary>
        /// <returns></returns>
        public string GetAllTradesDbPath()
        {
            return Path.Combine(GetTodayDbPath(), "AllTrades");
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
        public TimeSpan StartSessionTime
        {
            get
            {
                return Properties.Settings.Default.StartSessionTime;
            }
        }

        /// <summary>
        /// Время окончания торговой сессии по часовому поясу биржи
        /// </summary>
        public TimeSpan EndSessionTime
        {
            get
            {
                return Properties.Settings.Default.EndSessionTime;
            }
        }
    }
}
