using System.Collections.Generic;
using System.Linq;
using CommonData = Common.Data;
using Common;
using Common.Interfaces;
using Storage.Data;

namespace Storage
{
    /// <summary>
    /// Instrum da-layer
    /// </summary>
    public class InstrumDA : IInstrumDA
    {
        private IStorage _da;

        public InstrumDA(IStorage da)
        {
            _da = da;
        }

        /// <summary>
        /// Get all instrums
        /// </summary>
        /// <returns>All instrum list</returns>
        public IEnumerable<CommonData.Instrum> GetInstrumList()
        {
            return _da.DbContext.Table<Instrum>()
                .Select(r => new CommonData.Instrum()
                {
                    InsID = r.InsID,
                    Ticker = r.Ticker,
                    ShortName = r.ShortName,
                    Name = r.Name,
                    LotSize = r.LotSize,
                    Decimals = r.Decimals,
                    PriceStep = r.PriceStep
                }).ToList();
        }

        /// <summary>
        /// Get instrument by id or ticker
        /// </summary>
        /// <param name="insID">Instrument id or 0</param>
        /// <param name="ticker">Ticker or null/empty string</param>
        /// <returns></returns>
        public CommonData.Instrum GetInstrum(int insID, string ticker = null)
        {
            Instrum ins = null;
            if (insID > 0)
                ins = _da.DbContext.Table<Instrum>().FirstOrDefault(s => s.InsID == insID);
            else if (ticker != null && ticker.Length > 0)
                ins = _da.DbContext.Table<Instrum>().FirstOrDefault(s => s.Ticker == ticker);
            if (ins == null) return null;

            return new CommonData.Instrum()
            {
                InsID = ins.InsID,
                Ticker = ins.Ticker,
                ShortName = ins.ShortName,
                Name = ins.Name,
                LotSize = ins.LotSize,
                Decimals = ins.Decimals,
                PriceStep = ins.PriceStep
            };
        }

        /// <summary>
        /// Insert Instrum object into db
        /// </summary>
        /// <param name="ins">Instrum object with id = 0</param>
        /// <returns>New Id and set this value to InsID</returns>
        public int InsertInstrum(CommonData.Instrum ins)
        {
            Instrum db_ins = new Instrum()
            {
                InsID = ins.InsID,
                Ticker = ins.Ticker,
                ShortName = ins.ShortName,
                Name = ins.Name,
                LotSize = ins.LotSize,
                Decimals = ins.Decimals,
                PriceStep = ins.PriceStep
            };

            _da.DbContext.Insert(db_ins);
            ins.InsID = db_ins.InsID;

            return ins.InsID;
        }

        /// <summary>
        /// Update instrum object
        /// </summary>
        /// <param name="ins">Instrum object with InsID > 0</param>
        public void UpdateInstrum(CommonData.Instrum ins)
        {
            Instrum db_ins = new Instrum()
            {
                InsID = ins.InsID,
                Ticker = ins.Ticker,
                ShortName = ins.ShortName,
                Name = ins.Name,
                LotSize = ins.LotSize,
                Decimals = ins.Decimals,
                PriceStep = ins.PriceStep
            };

            _da.DbContext.Update(db_ins);
        }

        /// <summary>
        /// Delete Instrum object by Id
        /// </summary>
        /// <param name="insID">Id</param>
        public void DeleteInstrumByID(int insID)
        {
            _da.DbContext.Delete<Instrum>(insID);
        }
    }
}
