using Common;
using Common.Data;
using System;
using System.Linq;

namespace LeechSvc
{
    public class PosManager : IPosManager
    {
        private Instrum _instrum;
        private Account _account;
        private IHoldingTable _holdings;
        private IOrderTable _orders;
        private readonly AlorTradeWrapper _alorTrade;

        public PosManager(int insID, IInstrumTable instrums, IHoldingTable holdings, IOrderTable orders, IAccountTable accounts, AlorTradeWrapper alorTrade)
        {
            _holdings = holdings;
            _orders = orders;
            _instrum = instrums.GetInstrum(insID);
            if (_instrum == null) throw new Exception("Инструмент не найден");

            _account = accounts.GetDefaultAccount();
            if (_account == null) throw new Exception("Не найден торговый счет");

            _alorTrade = alorTrade;
        }

        public void ClosePosManager()
        {
        }

        public bool OpenLong(int lots)
        {
            if (IsActiveOrder()) return false; // есть активный ордер
            var hold = GetPos();
            if (hold != 0) return false; // позиция уже открыта (не важно какая)

            long orderNo = _alorTrade.AddOrder(_instrum.Ticker, BuySell.Buy, null, lots, _account.Code);

            return orderNo > 0;
        }

        public bool OpenShort(int lots)
        {
            if (IsActiveOrder()) return false; // есть активный ордер
            var hold = GetPos();
            if (hold != 0) return false; // позиция уже открыта (не важно какая)

            long orderNo = _alorTrade.AddOrder(_instrum.Ticker, BuySell.Sell, null, lots, _account.Code);

            return orderNo > 0;
        }

        public bool ClosePos()
        {
            if (IsActiveOrder()) return false; // есть активный ордер
            var hold = GetPos();
            if (hold == 0) return false; // позиции нет

            long orderNo;
            if (hold > 0)
            {
                orderNo = _alorTrade.AddOrder(_instrum.Ticker, BuySell.Sell, null, hold, _account.Code);
            }
            else
            {
                orderNo = _alorTrade.AddOrder(_instrum.Ticker, BuySell.Buy, null, hold, _account.Code);
            }

            return orderNo > 0;
        }

        public int GetPos()
        {
            var hold = _holdings.GetHoldings(_account.AccountID).FirstOrDefault(r => r.InsID == _instrum.InsID);
            if (hold == null) return 0;

            return hold.LotCount;
        }

        public bool IsReady
        {
            get
            {
                return !IsActiveOrder();
            }
        }

        private bool IsActiveOrder()
        {
            return _orders.GetOrders(_account.AccountID, 0)
                .Where(r => r.InsID == _instrum.InsID && r.Status == OrderStatus.Active)
                .Any();
        }
    }
}
