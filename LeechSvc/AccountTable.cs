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
    public interface IAccountTable
    {
        void AddAccount(string account, string name);
        void Load();
        IEnumerable<Account> GetAccounts();
        Account GetAccount(string code);
        Account GetDefaultAccount();
    }

    public class AccountTable : IAccountTable
    {
        private readonly IAccountDA _da = null;
        private Dictionary<string, Account> _code_account = null;

        public AccountTable(IAccountDA da)
        {
            _da = da;
            _code_account = new Dictionary<string, Account>();
        }

        public void AddAccount(string account, string name)
        {
            if (_code_account.ContainsKey(account))
            {
                _code_account[account].Name = name;
            }
            else
            {
                _code_account.Add(account, new Account()
                {
                    AccountType = AccountTypes.Real,
                    Code = account,
                    Name = name,
                    CommPerc = 0,
                    IsShortEnable = false
                });
            }

            var db_acc = _da.GetAccounts().FirstOrDefault(r => r.Code == account);
            if (db_acc == null)
            {
                _da.InsertAccount(_code_account[account]);
            }
            else if (db_acc.Name != name)
            {
                db_acc.Name = name;
                _da.UpdateAccount(db_acc);
            }
        }

        public void Load()
        {
            var accs = _da.GetAccounts();
            _code_account.Clear();
            foreach (var acc in accs) _code_account.Add(acc.Code, acc);
        }

        public IEnumerable<Account> GetAccounts()
        {
            return _code_account.Values.ToList();
        }

        public Account GetAccount(string code)
        {
            if (!_code_account.ContainsKey(code)) return null;
            return _code_account[code];
        }

        public Account GetDefaultAccount()
        {
            if (!_code_account.Values.Any()) return null;
            return _code_account.Values.FirstOrDefault();
        }
    }
}
