using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Storage.Data;
using CommonData = Common.Data;
using DBModel = Storage.Data;
using Common.Interfaces;
using System.Threading;

namespace Storage
{
    /// <summary>
    /// Fin. instrument data storage da-layer
    /// </summary>
    public class InsStoreDA : IInsStoreDA
    {
        private IStorage _da;

        public InsStoreDA(IStorage da)
        {
            _da = da;
        }

        /// <summary>
        /// Create InsStore (array of fin. instrument quotes)
        /// </summary>
        /// <param name="insID">Instrument</param>
        /// <param name="tf">Timeframe</param>
        /// <param name="isEnable">Active for sync</param>
        /// <returns></returns>
        public int CreateInsStore(int insID, Timeframes tf, bool isEnable)
        {
            int id = 0;
            var db_InsStore = new DBModel.InsStore()
            {
                InsID = insID,
                Tf = (byte)tf,
                IsEnable = isEnable
            };
            _da.DbContext.Insert(db_InsStore);
            id = db_InsStore.InsStoreID;

            return id;
        }

        /// <summary>
        /// Get InsStore by Id
        /// </summary>
        /// <param name="insStoreID"></param>
        /// <returns></returns>
        public CommonData.InsStore GetInsStoreByID(int insStoreID)
        {
            DBModel.InsStore ss = null;
            ss = _da.DbContext.Table<InsStore>().FirstOrDefault(s => s.InsStoreID == insStoreID);
            if (ss == null) return null;

            return new CommonData.InsStore()
            {
                InsStoreID = ss.InsStoreID,
                InsID = ss.InsID,
                Tf = (Timeframes)ss.Tf,
                IsEnable = ss.IsEnable
            };
        }

        /// <summary>
        /// Get InsStore by instrument and timeframe
        /// </summary>
        /// <param name="insID"></param>
        /// <param name="tf"></param>
        /// <returns></returns>
        public CommonData.InsStore GetInsStore(int insID, Timeframes tf)
        {
            DBModel.InsStore ss = null;
            ss = _da.DbContext.Table<InsStore>().FirstOrDefault(s => s.InsID == insID && s.Tf == (byte)tf);
            if (ss == null) return null;

            return new CommonData.InsStore()
            {
                InsStoreID = ss.InsStoreID,
                InsID = ss.InsID,
                Tf = (Timeframes)ss.Tf,
                IsEnable = ss.IsEnable
            };
        }

        /// <summary>
        /// Get active InsStores
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CommonData.InsStore> GetActiveInsStores()
        {
            List<DBModel.InsStore> insStores;
            insStores = _da.DbContext.Table<InsStore>().Where(s => s.IsEnable).ToList();

            var res = (from ss in insStores
                       select new CommonData.InsStore()
                       {
                          InsStoreID = ss.InsStoreID,
                          InsID = ss.InsID,
                          Tf = (Timeframes)ss.Tf,
                          IsEnable = ss.IsEnable
                       }).ToList();

            return res;
        }

        /// <summary>
        /// Get InsStore list by instrument
        /// </summary>
        /// <param name="insID"></param>
        /// <returns></returns>
        public IEnumerable<CommonData.InsStore> GetInsStores(int insID)
        {
            List<DBModel.InsStore> insStores;
            insStores = _da.DbContext.Table<InsStore>().Where(s => s.InsID == insID).ToList();

            var res = (from ss in insStores
                       select new CommonData.InsStore()
                       {
                           InsStoreID = ss.InsStoreID,
                           InsID = ss.InsID,
                           Tf = (Timeframes)ss.Tf,
                           IsEnable = ss.IsEnable
                       }).ToList();

            return res;
        }

        /// <summary>
        /// Store price bars to InsStore
        /// </summary>
        /// <param name="insStoreID">InsStore Id</param>
        /// <param name="bars">Bars array</param>
        /// <param name="date1">First date (ignore time)</param>
        /// <param name="date2">Last date (ignore time)</param>
        /// <param name="cancel">Cancel object (use for cancel continuous operation)</param>
        public void InsertBars(int insStoreID, IEnumerable<Bar> bars, DateTime date1, DateTime date2, CancellationToken cancel)
        {
            if (bars == null) return;

            if (date2 == DateTime.MaxValue) date2 = date2.AddDays(-1);

            bool isNewTran = _da.BeginTransaction();
            try
            {
                _da.DbContext.Execute("delete from BarHistory where InsStoreID = ? and Time >= ? and Time < ?",
                    insStoreID, StorageLib.ToDbTime(date1.Date), StorageLib.ToDbTime(date2.Date.AddDays(1)));

                foreach (var bar in bars)
                {
                    if (cancel.IsCancellationRequested) break;

                    var db_BarHistory = new DBModel.BarHistory()
                    {
                        InsStoreID = insStoreID,
                        Time = StorageLib.ToDbTime(bar.Time),
                        OpenPrice = bar.Open,
                        ClosePrice = bar.Close,
                        HighPrice = bar.High,
                        LowPrice = bar.Low,
                        Volume = bar.Volume
                    };
                    _da.DbContext.Insert(db_BarHistory);
                }

                _da.Commit(isNewTran);
            }
            catch (Exception ex)
            {
                _da.Rollback(isNewTran);
                throw new Exception("Database error occurred while inserting bars", ex);
            }
        }

        /// <summary>
        /// Delete price bars from db
        /// </summary>
        /// <param name="insStoreID">InsStore</param>
        /// <param name="date1">First date (without time)</param>
        /// <param name="date2">Last date (without time)</param>
        public void DeleteBars(int insStoreID, DateTime date1, DateTime date2)
        {
            if (date2 == DateTime.MaxValue) date2 = date2.AddDays(-1);

            try
            {
                _da.DbContext.Execute("delete from BarHistory where InsStoreID = ? and Time >= ? and Time < ?",
                    insStoreID, StorageLib.ToDbTime(date1.Date), StorageLib.ToDbTime(date2.Date.AddDays(1)));
            }
            catch (Exception ex)
            {
                throw new Exception("Database error occurred while deleting bars", ex);
            }
        }

        /// <summary>
        /// Update InsStore
        /// </summary>
        /// <param name="insStore">InsStore</param>
        public void UpdateInsStore(CommonData.InsStore insStore)
        {
            InsStore db_secStore = new InsStore()
            {
                InsStoreID = insStore.InsStoreID,
                InsID = insStore.InsID,
                Tf = (byte)insStore.Tf,
                IsEnable = insStore.IsEnable
            };

            _da.DbContext.Update(db_secStore);
        }

        /// <summary>
        /// Load historical data into bars
        /// </summary>
        /// <param name="bars">Bars object</param>
        /// <param name="insStoreID">InsStote Id</param>
        /// <param name="date1">First date (without time)</param>
        /// <param name="date2">Last date (without time)</param>
        /// <returns>Async task</returns>
        public Task LoadHistoryAsync(BarRow bars, int insStoreID, DateTime date1, DateTime date2)
        {
            if (date2 == DateTime.MaxValue) date2 = date2.AddDays(-1);
            int d1 = StorageLib.ToDbTime(date1.Date);
            int d2 = StorageLib.ToDbTime(date2.Date.AddDays(1));

            return Task.Run(() =>
            {
                var loadedBars = _da.DbContext.Table<BarHistory>()
                    .Where(b => b.InsStoreID == insStoreID && b.Time >= d1 && b.Time < d2)
                    .OrderBy(b => b.Time);

                bars.SuspendEvents();
                foreach (var bar in loadedBars)
                {
                    bars.AddTick(StorageLib.ToDateTime(bar.Time), bar.OpenPrice, 0);
                    bars.AddTick(StorageLib.ToDateTime(bar.Time), bar.HighPrice, 0);
                    bars.AddTick(StorageLib.ToDateTime(bar.Time), bar.LowPrice, 0);
                    bars.AddTick(StorageLib.ToDateTime(bar.Time), bar.ClosePrice, bar.Volume);
                }
                bars.CloseLastBar();
                bars.ResumeEvents();
            });
        }

        /// <summary>
        /// Get all InsStore periods by InsStoreId
        /// </summary>
        /// <param name="insStoreID">InsStore Id</param>
        /// <returns>Periods</returns>
        public IEnumerable<InsStorePeriod> GetPeriods(int insStoreID)
        {
            List<DBModel.InsStorePeriods> periods;
            periods = _da.DbContext.Table<InsStorePeriods>()
                .Where(s => s.InsStoreID == insStoreID)
                .OrderBy(p => p.StartDate)
                .ToList();

            return periods.Select(p => new InsStorePeriod(StorageLib.ToDateTime(p.StartDate), StorageLib.ToDateTime(p.EndDate), p.IsLastDirty)).ToList();
        }

        /// <summary>
        /// Update InsStore periods (delete old data and insert new data)
        /// </summary>
        /// <param name="insStoreID">InsStore Id</param>
        /// <param name="periods">InsStore periods</param>
        public void UpdatePeriods(int insStoreID, IEnumerable<Common.InsStorePeriod> periods)
        {
            bool isNewTran = _da.BeginTransaction();
            try
            {
                _da.DbContext.Execute("delete from InsStorePeriods where InsStoreID = " + insStoreID.ToString());

                foreach (var period in periods)
                {
                    var db_InsStorePeriods = new DBModel.InsStorePeriods()
                    {
                        InsStoreID = insStoreID,
                        StartDate = StorageLib.ToDbTime(period.StartDate),
                        EndDate = StorageLib.ToDbTime(period.EndDate),
                        IsLastDirty = period.IsLastDirty
                    };
                    _da.DbContext.Insert(db_InsStorePeriods);
                }

                _da.Commit(isNewTran);
            }
            catch (Exception ex)
            {
                _da.Rollback(isNewTran);
                throw new Exception("Database error occurred while updating periods.", ex);
            }
        }

        /// <summary>
        /// Get free days (weekends)
        /// </summary>
        /// <param name="insStoreID">InsStore Id</param>
        /// <returns>Weekends (without time)</returns>
        public IEnumerable<DateTime> GetFreeDays(int insStoreID)
        {
            List<DBModel.InsStoreFreeDays> freeDays;
            freeDays = _da.DbContext.Table<InsStoreFreeDays>()
                .Where(s => s.InsStoreID == insStoreID)
                .OrderBy(p => p.Date)
                .ToList();

            return freeDays.Select(p => StorageLib.ToDateTime(p.Date)).ToList();
        }

        /// <summary>
        /// Update free days data (delete old free days and insert new free days)
        /// </summary>
        /// <param name="insStoreID">InsStore Id</param>
        /// <param name="freeDays">Free days list</param>
        public void UpdateFreeDays(int insStoreID, IEnumerable<DateTime> freeDays)
        {
            bool isNewTran = _da.BeginTransaction();
            try
            {
                _da.DbContext.Execute("delete from InsStoreFreeDays where InsStoreID = " + insStoreID.ToString());

                foreach (var day in freeDays)
                {
                    var db_InsStoreFreeDays = new DBModel.InsStoreFreeDays()
                    {
                        InsStoreID = insStoreID,
                        Date = StorageLib.ToDbTime(day.Date)
                    };
                    _da.DbContext.Insert(db_InsStoreFreeDays);
                }

                _da.Commit(isNewTran);
            }
            catch (Exception ex)
            {
                _da.Rollback(isNewTran);
                throw new Exception("Database error occurred while updating free days.", ex);
            }
        }
    }
}
