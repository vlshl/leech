using Common;
using Common.Data;
using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeechSvc
{
    public interface ITradeTable
    {
        bool AddTrade(Trade trade);
    }

    public class TradeTable : ITradeTable
    {
        private readonly IAccountDA _da = null;

        public TradeTable(IAccountDA da)
        {
            _da = da;
        }

        public bool AddTrade(Trade trade)
        {
            var db_trade = _da.GetTrades(trade.AccountID, trade.Time.Date)
                .FirstOrDefault(t => t.TradeNo == trade.TradeNo);
            if (db_trade == null)
            {
                _da.InsertTrade(trade);
            }

            return true;
        }
    }
}
