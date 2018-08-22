using Common.Interfaces;
using LeechSvc.Logger;
using SQLite.Net;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Storage
{
    /// <summary>
    /// Database initialization and convertation
    /// </summary>
    public class DataStorage : IDataStorage
    {
        private readonly IStorage _da = null;
        private readonly ILogger _logger = null;

        public DataStorage(IStorage da, ILogger logger)
        {
            _da = da;
            _logger = logger;
        }

        /// <summary>
        /// Initialization database (create if need, convertation)
        /// </summary>
        /// <returns></returns>
        public void Initialize()
        {
            _logger.AddInfo("DataStorage", "Initialize ...");
            int version = GetDBVersion();
            for (int v = 0; v < DbConvert.Scripts.Length; v++)
            {
                if (version < v) Convert(DbConvert.Scripts[v], v);
            }
            _logger.AddInfo("DataStorage", "Initialized");
        }

        /// <summary>
        /// Close database connection
        /// </summary>
        public void Close()
        {
            _logger.AddInfo("DataStorage", "Closing ...");
            _da.CloseDbContext();
            _logger.AddInfo("DataStorage", "Closed");
        }

        /// <summary>
        /// Get database version
        /// </summary>
        /// <returns>-1 - empty database (no tables)</returns>
        private int GetDBVersion()
        {
            int version = -1;

            try
            {
                var vv = _da.DbContext.Query<VersionValue>("SELECT Version FROM DBVersion").FirstOrDefault();
                if (vv != null) version = vv.Version;
            }
            catch (SQLiteException e)
            {
                // 'no DBVersion table' on first launch
            }
            catch (Exception ex)
            {
                throw new Exception("Database error", ex);
            }

            return version;
        }

        /// <summary>
        /// Execute database converter
        /// </summary>
        /// <param name="script">SQL script</param>
        /// <param name="version">Convert to version</param>
        private void Convert(string script, int version)
        {
            string[] statements = script.Split(new String[] { "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            try
            {
                _da.DbContext.BeginTransaction();

                foreach (var statement in statements)
                {
                    if (statement.Trim().Length == 0) continue;
                    _da.DbContext.Execute(statement.Trim());
                }
                _da.DbContext.Execute(@"UPDATE DBVersion SET Version = " + version.ToString());

                _da.DbContext.Commit();
            }
            catch (Exception ex)
            {
                _da.DbContext.Rollback();
                throw new Exception("Error database convertation to version " + version.ToString(), ex);
            }
        }
    }

    internal class VersionValue
    {
        public int Version { get; set; }
    }
}
