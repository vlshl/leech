using Common.Data;
using Common.Interfaces;
using LeechSvc.BL;
using LeechSvc.Logger;
using Storage;
using System;
using System.Collections.Generic;

namespace LeechSvc
{
    /// <summary>
    /// Интерфейс таблицы фин. инструментов
    /// </summary>
    public interface IInstrumTable
    {
        Instrum SyncInstrum(string ticker, string shortname, string name, int lotsize, int decimals, decimal pricestep);
        Instrum GetInstrum(string ticker);
        Instrum GetInstrum(int insID);
        void Load();
    }

    /// <summary>
    /// Таблица фин. инструментов
    /// </summary>
    public class InstrumTable : IInstrumTable
    {
        private readonly IInstrumDA _da;
        private readonly IStorage _storage = null;
        private readonly ILogger _logger = null;
        private Dictionary<string, Instrum> _ticker_instrum;
        private Dictionary<int, Instrum> _insID_instrum;

        public InstrumTable(IInstrumDA da, IStorage storage, ILogger logger)
        {
            _da = da;
            _storage = storage;
            _logger = logger;
            _ticker_instrum = new Dictionary<string, Instrum>();
            _insID_instrum = new Dictionary<int, Instrum>();
        }

        /// <summary>
        /// Синхронизировать фин. инструмент (изменить или создать новый)
        /// </summary>
        /// <param name="ticker">Тикер</param>
        /// <param name="shortname">Краткое наименование</param>
        /// <param name="name">Полное наименование</param>
        /// <param name="lotsize">Размер лота</param>
        /// <param name="decimals">Кол-во десятичных знаков после запятой в цене</param>
        /// <param name="pricestep">Шаг цены</param>
        /// <returns>Фин. инструмент после синхронизации</returns>
        public Instrum SyncInstrum(string ticker, string shortname, string name, int lotsize, int decimals, decimal pricestep)
        {
            if (!_ticker_instrum.ContainsKey(ticker))
            {
                Instrum db_ins = _da.GetInstrum(0, ticker);
                if (db_ins == null)
                {
                    var ins = new Instrum
                    {
                        InsID = 0,
                        Ticker = ticker,
                        ShortName = shortname,
                        Name = name,
                        LotSize = lotsize,
                        Decimals = decimals,
                        PriceStep = pricestep
                    };
                    _da.InsertInstrum(ins);
                    _ticker_instrum.Add(ticker, ins);
                    _insID_instrum.Add(ins.InsID, ins);
                    return ins;
                }
                else
                {
                    UpdateInstrum(db_ins, shortname, name, lotsize, decimals, pricestep);
                    _ticker_instrum.Add(ticker, db_ins);
                    _insID_instrum.Add(db_ins.InsID, db_ins);
                    return db_ins;
                }
            }
            else
            {
                var ins = _ticker_instrum[ticker];
                UpdateInstrum(ins, shortname, name, lotsize, decimals, pricestep);
                return ins;
            }
        }

        private void UpdateInstrum(Instrum ins, string shortname, string name, int lotsize, int decimals, decimal pricestep)
        {
            if (ins.ShortName == shortname && ins.Name == name && ins.LotSize == lotsize && ins.Decimals == decimals && ins.PriceStep == pricestep)
                return;

            ins.ShortName = shortname;
            ins.Name = name;
            ins.LotSize = lotsize;
            ins.Decimals = decimals;
            ins.PriceStep = pricestep;
            _da.UpdateInstrum(ins);
        }

        /// <summary>
        /// Получить запись по тикеру
        /// </summary>
        /// <param name="ticker">Уникальный тикер</param>
        /// <returns>Фин. инструмент, если null - не найден</returns>
        public Instrum GetInstrum(string ticker)
        {
            if (!_ticker_instrum.ContainsKey(ticker)) return null;
            return _ticker_instrum[ticker];
        }

        /// <summary>
        /// Получить запись по ID
        /// </summary>
        /// <param name="insID">ID инструмента</param>
        /// <returns>Фин. инструмент, если null - не найден</returns>
        public Instrum GetInstrum(int insID)
        {
            if (!_insID_instrum.ContainsKey(insID)) return null;
            return _insID_instrum[insID];
        }

        /// <summary>
        /// Загрузить таблицу из базы (предварительно ее очистить)
        /// </summary>
        public void Load()
        {
            var list = _da.GetInstrumList();
            _ticker_instrum.Clear();
            _insID_instrum.Clear();
            foreach (var ins in list)
            {
                if (_ticker_instrum.ContainsKey(ins.Ticker)) continue;
                _ticker_instrum.Add(ins.Ticker, ins);
                _insID_instrum.Add(ins.InsID, ins);
            }
            _logger.AddInfo("InstrumTable", "Load instrums: " + _ticker_instrum.Keys.Count.ToString());
        }
    }
}
