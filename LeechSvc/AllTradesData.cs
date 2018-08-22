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
        private readonly ILeechConfig _config = null;
        private readonly IInsStoreData _insStoreData = null;
        private readonly ILogger _logger = null;

        public AllTradesData(IInstrumTable insTable, ILeechConfig config, IInsStoreData insStoreData, ILogger logger)
        {
            _instrumTable = insTable ?? throw new ArgumentNullException("inscTable");
            _config = config ?? throw new ArgumentNullException("config");
            _insStoreData = insStoreData ?? throw new ArgumentNullException("insStoreData");
            _logger = logger ?? throw new ArgumentNullException("logger");
        }

        /// <summary>
        /// Сохранение данных по всем сделкам для всех инструментов
        /// </summary>
        public void SaveData(ITickDispatcher tickDispatcher)
        {
            if (tickDispatcher == null) throw new ArgumentNullException("tickDispatcher");

            _logger.AddInfo("AllTradesData", "Save data ...");
            try
            {
                if (!Directory.Exists(_config.GetAllTradesDbPath()))
                {
                    Directory.CreateDirectory(_config.GetAllTradesDbPath());
                }

                var insIDs = tickDispatcher.GetInstrumIDs();
                foreach (var insID in insIDs)
                {
                    Instrum ins = _instrumTable.GetInstrum(insID);
                    if (ins == null) continue;
                    var ticks = tickDispatcher.GetTicks(insID);
                    if (ticks == null || !ticks.Any()) continue;

                    var encoder = new AllTradesEncoder(ins.Decimals);
                    var persist = new AllTradesPersist(_config);
                    persist.Initialize(ins.Ticker);
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
