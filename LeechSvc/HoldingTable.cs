using Common.Data;
using Common.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace LeechSvc
{
    public interface IHoldingTable
    {
        void SetHolding(int accountID, int insID, int lots);
        IEnumerable<Holding> GetHoldings(int accountId);
    }

    public class HoldingTable : IHoldingTable
    {
        private readonly IAccountDA _da = null;

        public HoldingTable(IAccountDA da)
        {
            _da = da;
        }

        public void SetHolding(int accountID, int insID, int lots)
        {
            if (lots == 0) return;

            var holds = _da.GetHoldings(accountID);
            var hold = holds.FirstOrDefault(h => h.InsID == insID);
            if (hold == null && lots != 0)
            {
                Holding h = new Holding();
                h.AccountID = accountID;
                h.InsID = insID;
                h.LotCount = lots;
                _da.InsertHolding(h);
            }
            else if (hold != null && lots == 0)
            {
                _da.DeleteHolding(hold.HoldingID);
            }
            else if (hold != null && lots != 0 && hold.LotCount != lots)
            {
                hold.LotCount = lots;
                _da.UpdateHolding(hold);
            }
        }

        public IEnumerable<Holding> GetHoldings(int accountId)
        {
            return _da.GetHoldings(accountId).ToList();
        }
    }
}
