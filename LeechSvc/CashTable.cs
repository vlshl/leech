using Common.Data;
using Common.Interfaces;
using System.Linq;

namespace LeechSvc
{
    public interface ICashTable
    {
        void SetPosition(int accountID, decimal curPos);
        Cash GetCash(int accountId);
    }

    public class CashTable : ICashTable
    {
        private readonly IAccountDA _da = null;

        public CashTable(IAccountDA da)
        {
            _da = da;
        }

        public void SetPosition(int accountID, decimal curPos)
        {
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

        public Cash GetCash(int accountId)
        {
            return _da.GetCashes(accountId).FirstOrDefault();
        }
    }
}
