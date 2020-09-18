using Common.Data;
using Common.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace LeechSvc
{
    public interface ITradeTable
    {
        bool AddTrade(Trade trade);
        IEnumerable<Trade> GetTrades(int accountId, int idFrom);
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
            var db_trade = _da.GetTrades(trade.AccountID, trade.Time.Date, 0)
                .FirstOrDefault(t => t.TradeNo == trade.TradeNo);
            if (db_trade == null)
            {
                _da.InsertTrade(trade);
            }

            return true;
        }

        public IEnumerable<Trade> GetTrades(int accountId, int idFrom)
        {
            return _da.GetTrades(accountId, null, idFrom).ToList();
        }
    }
}
