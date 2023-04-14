using System;
using System.IO;
using System.Text;

namespace LeechSvc
{
    /// <summary>
    /// Файловое хранение информации о сделках по конкретному тикеру
    /// Версия 1.0 - у сделок передается местное время
    /// Версия 1.1 - у сделок передается время биржи (МСК) в формате кол-во секунд с начала торговой сессии (4 байта)
    /// Версия 1.2 - у сделок передается дата и время биржи в формате кол-во секунд с 01.01.2000 (4 байта)
    /// </summary>
    public class AllTradesPersist
    {
        private FileStream _fs;

        /// <summary>
        /// Конструктор
        /// </summary>
        public AllTradesPersist()
        {
        }

        /// <summary>
        /// Инициализация, создание нового файлового потока по тикеру.
        /// Запись в файловый поток начальных данных (заголовка).
        /// </summary>
        /// <param name="path">Каталог для сохранения данных за текущую дату</param>
        /// <param name="ticker">Тикер</param>
        public void Initialize(string path, string ticker)
        {
            const string version = "AllTrades 1.2   "; // header size = 16

            string filename = Path.Combine(path, ticker);
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
