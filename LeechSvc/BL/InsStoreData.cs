using BL;
using Common;
using Common.Interfaces;
using LeechSvc.Logger;
using Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LeechSvc.BL
{
    public interface IInsStoreData
    {
        void InitInsStores(int insID);
        void AddTick(int insID, DateTime time, decimal price, int count);
        void SaveData();
    }

    public class InsStoreData : IInsStoreData
    {
        private readonly IInsStoreDA _insStoreDA = null;
        private readonly IStorage _storage = null;
        private readonly IRepositoryBL _reposBL = null;
        private readonly IInsStoreBL _insStoreBL = null;
        private readonly ILogger _logger = null;
        private readonly ILeechConfig _config = null;
        private Timeframes[] _tfs;
        private Dictionary<Common.Data.InsStore, BarRow> _insStore_barRow = null;
        private Dictionary<int, List<BarRow>> _insID_barRows = null;

        public InsStoreData(IInsStoreDA insStoreDA, InsStoreBL insStoreBL, IStorage storage, IRepositoryBL reposBL, ILeechConfig config, ILogger logger)
        {
            _insStoreDA = insStoreDA;
            _insStoreBL = insStoreBL;
            _storage = storage;
            _reposBL = reposBL;
            _config = config;
            _logger = logger;

            _tfs = new Timeframes[]
            {
                Timeframes.Min,
                Timeframes.Min5,
                Timeframes.Hour,
                Timeframes.Day
            };
            _insStore_barRow = new Dictionary<Common.Data.InsStore, BarRow>();
            _insID_barRows = new Dictionary<int, List<BarRow>>();
        }

        /// <summary>
        /// Инициализация потоков исторических данных
        /// </summary>
        /// <param name="insID">Инструмент</param>
        public void InitInsStores(int insID)
        {
            var isNewTran = _storage.BeginTransaction();
            try
            {
                foreach (var tf in _tfs)
                {
                    var insStore = _insStoreDA.GetInsStore(insID, tf);
                    if (insStore == null)
                    {
                        int insStoreID = _insStoreDA.CreateInsStore(insID, tf, true);
                        insStore = _insStoreDA.GetInsStoreByID(insStoreID);
                    }
                    BarRow bars = new BarRow(tf, insStore.InsID);
                    _insStore_barRow.Add(insStore, bars);

                    if (!_insID_barRows.ContainsKey(insID))
                    {
                        _insID_barRows.Add(insStore.InsID, new List<BarRow>());
                    }
                    _insID_barRows[insID].Add(bars);
                }
                _storage.Commit(isNewTran);
            }
            catch(Exception ex)
            {
                _storage.Rollback(isNewTran);
                _logger.AddException("InsStoreBL:CreateInsStore", ex);
            }
        }

        /// <summary>
        /// Накопление данных в барах
        /// </summary>
        /// <param name="insID">Инструмент</param>
        /// <param name="time">Время сделки</param>
        /// <param name="price">Цена</param>
        /// <param name="lots">Кол-во лотов</param>
        public void AddTick(int insID, DateTime time, decimal price, int lots)
        {
            if (!_insID_barRows.ContainsKey(insID)) return;

            var barRowList = _insID_barRows[insID];
            foreach (var br in barRowList)
            {
                br.AddTick(time, price, lots);
            }
        }

        public void SaveData()
        {
            DateTime firstDate; DateTime lastDate;

            var lastHistData = _reposBL.GetIntParam(LAST_HISTORY_DATA);
            if (lastHistData == null)
            {
                firstDate = DateTime.Today.AddHours(_config.CorrectHours).Date;
            }
            else
            {
                firstDate = StorageLib.ToDateTime(lastHistData.Value).AddDays(1);
            }
            lastDate = firstDate;

            _logger.AddInfo("InsStoreData", "Bars saving ...");

            bool isNewTran = _storage.BeginTransaction();
            try
            {
                foreach (var insStore in _insStore_barRow.Keys)
                {
                    var bars = _insStore_barRow[insStore].Bars;
                    if (!bars.Any()) continue;

                    var lastBarDate = bars.Last().Time.Date;
                    if (lastBarDate > lastDate) lastDate = lastBarDate;

                    _insStoreBL.InsertData(insStore.InsStoreID, bars, firstDate, lastBarDate, false, new CancellationToken());
                }

                _reposBL.SetIntParam(LAST_HISTORY_DATA, StorageLib.ToDbTime(lastDate));
                _storage.Commit(isNewTran);

                _logger.AddInfo("InsStoreData", string.Format("Bars saved: {0} - {1}", firstDate.ToString("dd.MM.yyyy"), lastDate.ToString("dd.MM.yyyy")));
            }
            catch (Exception ex)
            {
                _storage.Rollback(isNewTran);
                _logger.AddError("InsStoreData", "Save data error. " + string.Format("First = {0}, last = {1}", firstDate.ToString("dd.MM.yyyy HH:mm:ss"), lastDate.ToString("dd.MM.yyyy HH:mm:ss")));
                _logger.AddException("InsStoreData", ex);
            }

        }
        private string LAST_HISTORY_DATA = "LastHistoryData";
    }
}
