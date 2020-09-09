using Common;
using Common.Data;
using LeechSvc.BL;
using LeechSvc.Logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LeechSvc
{
    /// <summary>
    /// Сохранение данных по всем сделкам
    /// </summary>
    public class AllTradesData
    {
        private readonly IInstrumTable _instrumTable = null;
        private readonly IInsStoreData _insStoreData = null;
        private readonly ILogger _logger = null;

        public AllTradesData(IInstrumTable insTable, IInsStoreData insStoreData, ILogger logger)
        {
            _instrumTable = insTable ?? throw new ArgumentNullException("inscTable");
            _insStoreData = insStoreData ?? throw new ArgumentNullException("insStoreData");
            _logger = logger ?? throw new ArgumentNullException("logger");
        }

        /// <summary>
        /// Сохранение данных по всем сделкам для всех инструментов
        /// </summary>
        /// <param name="tickDispatcher">Диспетчер тиковых данных</param>
        /// <param name="sessionDbPath">Каталог данных текущей сессии (определяется датой)</param>
        public void SaveData(ITickDispatcher tickDispatcher, string sessionDbPath)
        {
            if (tickDispatcher == null)
                throw new ArgumentNullException("tickDispatcher");
            if (string.IsNullOrWhiteSpace(sessionDbPath))
                throw new ArgumentException("SessionDbPath is empty, session not opened.");

            _logger.AddInfo("AllTradesData", "Save data ...");
            try
            {
                var allTradesDir = Path.Combine(sessionDbPath, "AllTrades");

                if (!Directory.Exists(allTradesDir))
                {
                    Directory.CreateDirectory(allTradesDir);
                }

                var insIDs = tickDispatcher.GetInstrumIDs();
                foreach (var insID in insIDs)
                {
                    Instrum ins = _instrumTable.GetInstrum(insID);
                    if (ins == null) continue;
                    var ticks = tickDispatcher.GetTicks(insID);
                    if (ticks == null || !ticks.Any()) continue;

                    var encoder = new AllTradesEncoder(ins.Decimals);
                    var persist = new AllTradesPersist();
                    persist.Initialize(allTradesDir, ins.Ticker);
                    _insStoreData.InitInsStores(insID);

                    foreach (Tick tick in ticks)
                    {
                        uint seconds = (uint)(tick.Time.Hour * 60 * 60 + tick.Time.Minute * 60 + tick.Time.Second);
                        byte[] buf = encoder.AddTick(seconds, tick.Price, tick.Lots);
                        persist.Write(buf);
                        _insStoreData.AddTick(insID, tick.Time, tick.Price, tick.Lots);
                    }
                    persist.Close();
                }
                _insStoreData.SaveData();
            }
            catch (Exception ex)
            {
                _logger.AddException("AllTradesData", ex);
            }
            _logger.AddInfo("AllTradesData", "Data saved");
        }
    }
}
