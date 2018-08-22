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
    public interface IHoldingTable
    {
        void SetHolding(int accountID, int insID, int lots);
        void Load();
    }

    public class HoldingTable : IHoldingTable
    {
        private Dictionary<int, Dictionary<int, int>> _acc_insID_lots = null;
        private readonly IAccountDA _da = null;

        public HoldingTable(IAccountDA da)
        {
            _da = da;
            _acc_insID_lots = new Dictionary<int, Dictionary<int, int>>();
        }

        public void SetHolding(int accountID, int insID, int lots)
        {
            Dictionary<int, int> insID_lots;
            if (_acc_insID_lots.ContainsKey(accountID))
            {
                insID_lots = _acc_insID_lots[accountID];
            }
            else
            {
                insID_lots = new Dictionary<int, int>();
                _acc_insID_lots.Add(accountID, insID_lots);
            }

            if (insID_lots.ContainsKey(insID))
            {
                if (lots == 0)
                {
                    insID_lots.Remove(insID);
                }
                else
                {
                    insID_lots[insID] = lots;
                }
            }
            else
            {
                if (lots != 0)
                {
                    insID_lots.Add(insID, lots);
                }
            }

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

        public void Load()
        {
            _acc_insID_lots.Clear();
            var accounts = _da.GetAccounts();
            foreach (var acc in accounts)
            {
                var insID_lots = new Dictionary<int, int>();
                _acc_insID_lots.Add(acc.AccountID, insID_lots);

                var holdings = _da.GetHoldings(acc.AccountID);
                foreach (var hold in holdings)
                {
                    if (insID_lots.ContainsKey(hold.InsID))
                    {
                        insID_lots[hold.InsID] = hold.LotCount;
                    }
                    else
                    {
                        insID_lots.Add(hold.InsID, hold.LotCount);
                    }
                }
            }
        }
    }
}
