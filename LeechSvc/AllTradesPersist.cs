using System;
using System.IO;
using System.Text;

namespace LeechSvc
{
    /// <summary>
    /// Файловое хранение информации о сделках по конкретному тикеру
    /// </summary>
    public class AllTradesPersist
    {
        private ILeechConfig _config;
        private FileStream _fs;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="config">Конфигурация</param>
        public AllTradesPersist(ILeechConfig config)
        {
            if (config == null)
                throw new ArgumentNullException("config");
            _config = config;
        }

        /// <summary>
        /// Инициализация, создание нового файлового потока по тикеру.
        /// Запись в файловый поток начальных данных (заголовка).
        /// </summary>
        /// <param name="ticker">Тикер</param>
        public void Initialize(string ticker)
        {
            const string version = "AllTrades 1.1   "; // header size = 16

            string allTradesDir = _config.GetAllTradesDbPath();
            string filename = allTradesDir + "\\" + ticker;

            _fs = File.Create(filename);
            byte[] ver = new ASCIIEncoding().GetBytes(version);
            _fs.Write(ver, 0, ver.Length);
        }

        /// <summary>
        /// Записать данные в поток
        /// </summary>
        /// <param name="buffer">Массив байтов для записи</param>
        public void Write(byte[] buffer)
        {
            if (_fs == null) return;
            _fs.Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Закрыть поток
        /// </summary>
        public void Close()
        {
            if (_fs != null) _fs.Close();
        }
    }
}
