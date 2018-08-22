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
    public interface IPositionTable
    {
        void SetPosition(int accountID, decimal curPos);
        void Load();
    }

    public class PositionTable : IPositionTable
    {
        private Dictionary<int, decimal> _accID_curpos = null;
        private readonly IAccountDA _da = null;

        public PositionTable(IAccountDA da)
        {
            _da = da;
            _accID_curpos = new Dictionary<int, decimal>();
        }

        public void SetPosition(int accountID, decimal curPos)
        {
            if (_accID_curpos.ContainsKey(accountID))
            {
                _accID_curpos[accountID] = curPos;
            }
            else
            {
                _accID_curpos.Add(accountID, curPos);
            }

            var cash = _da.GetCashes(accountID).FirstOrDefault();
            if (cash == null)
            {
                Cash c = new Cash();
                c.AccountID = accountID;
                c.Current = curPos;
                _da.InsertCash(c);
            }
            else if (cash.Current != curPos)
            {
                cash.Current = curPos;
                _da.UpdateCash(cash);
            }
        }

        public void Load()
        {
            _accID_curpos.Clear();
            var accounts = _da.GetAccounts();
            foreach (var acc in accounts)
            {
                var cash = _da.GetCashes(acc.AccountID).FirstOrDefault();
                if (cash != null)
                {
                    _accID_curpos.Add(acc.AccountID, cash.Current);
                }
                else
                {
                    _accID_curpos.Add(acc.AccountID, 0);
                }
            }
        }
    }
}
