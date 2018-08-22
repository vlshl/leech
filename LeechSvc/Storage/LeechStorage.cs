using SQLite.Net;
using Storage;
using System.IO;

namespace LeechSvc.Storage
{
    public class LeechStorage : IStorage
    {
        private string _dbfile = "";
        private SQLiteConnection _connection = null;

        public LeechStorage(ILeechConfig config)
        {
            string dbpath = config.GetDbPath();
            if (!Directory.Exists(dbpath))
            {
                Directory.CreateDirectory(dbpath);
            }
            _dbfile = Path.Combine(dbpath, "leech.db");
        }

        /// <summary>
        /// Get database opened context
        /// </summary>
        public SQLiteConnection DbContext
        {
            get
            {
                if (_connection == null)
                {
                    var p = new SQLite.Net.Platform.Win32.SQLitePlatformWin32();
                    p.SQLiteApi.Config(SQLite.Net.Interop.ConfigOption.Serialized);
                    _connection = new SQLiteConnection(p, _dbfile);
                }

                return _connection;
            }
        }

        /// <summary>
        /// Close database connection
        /// </summary>
        public void CloseDbContext()
        {
            if (_connection == null) return;

            _connection.Close();
            _connection = null;
        }

        /// <summary>
        /// Начать новую транзакцию
        /// </summary>
        /// <returns>true - транзакция была начата, false - транзакция уже была открыта ранее</returns>
        public bool BeginTransaction()
        {
            if (DbContext.IsInTransaction) return false;

            DbContext.BeginTransaction();
            return true;
        }

        /// <summary>
        /// Зафиксировать изменения
        /// </summary>
        /// <param name="isNewTran">Результат BeginTransaction</param>
        public void Commit(bool isNewTran)
        {
            if (!isNewTran) return;

            if (DbContext.IsInTransaction)
            {
                DbContext.Commit();
            }
        }

        /// <summary>
        /// Откатить изменения
        /// </summary>
        /// <param name="isNewTran">Результат BeginTransaction</param>
        public void Rollback(bool isNewTran)
        {
            if (!isNewTran) return;

            if (DbContext.IsInTransaction)
            {
                DbContext.Rollback();
            }
        }
    }
}
