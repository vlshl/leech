using Common;
using Common.Interfaces;
using Storage.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using CommonData = Common.Data;

namespace Storage
{
    /// <summary>
    /// Trade account da-layer
    /// </summary>
    public class AccountDA : IAccountDA
    {
        private IStorage _da;

        public AccountDA(IStorage da)
        {
            _da = da;
        }

        /// <summary>
        /// Get Account by Id
        /// </summary>
        /// <param name="accountID">Id</param>
        /// <returns>Account data</returns>
        public CommonData.Account GetAccountByID(int accountID)
        {
            Account account = null;
            account = _da.DbContext.Table<Account>().FirstOrDefault(a => a.AccountID == accountID);
            if (account == null) return null;

            return new CommonData.Account()
            {
                AccountID = account.AccountID,
                Code = account.Code,
                Name = account.Name,
                CommPerc = account.CommPerc,
                IsShortEnable = account.IsShortEnable,
                AccountType = (CommonData.AccountTypes)account.AccountType
            };
        }

        /// <summary>
        /// Get all accounts
        /// </summary>
        /// <returns>Accounts list</returns>
        public IEnumerable<CommonData.Account> GetAccounts()
        {
            List<CommonData.Account> accounts = new List<CommonData.Account>();

            var db_accounts = _da.DbContext.Table<Account>().ToList();
            foreach (var db_account in db_accounts)
            {
                var account = new CommonData.Account()
                {
                    AccountID = db_account.AccountID,
                    Code = db_account.Code,
                    Name = db_account.Name,
                    CommPerc = db_account.CommPerc,
                    IsShortEnable = db_account.IsShortEnable,
                    AccountType = (CommonData.AccountTypes)db_account.AccountType
                };
                accounts.Add(account);
            }

            return accounts;
        }

        /// <summary>
        /// Insert new account into db
        /// </summary>
        /// <param name="account">Account (Id = 0)</param>
        /// <returns>New Id (also set new Id in account data)</returns>
        public int InsertAccount(CommonData.Account account)
        {
            Account db_account = new Account()
            {
                AccountID = account.AccountID,
                Code = account.Code,
                Name = account.Name,
                CommPerc = account.CommPerc,
                IsShortEnable = account.IsShortEnable,
                AccountType = (byte)account.AccountType
            };

            _da.DbContext.Insert(db_account);
            account.AccountID = db_account.AccountID;

            return account.AccountID;
        }

        /// <summary>
        /// Update account data
        /// </summary>
        /// <param name="account">Account data</param>
        public void UpdateAccount(CommonData.Account account)
        {
            Account db_account = new Account()
            {
                AccountID = account.AccountID,
                Code = account.Code,
                Name = account.Name,
                CommPerc = account.CommPerc,
                IsShortEnable = account.IsShortEnable,
                AccountType = (byte)account.AccountType
            };

            _da.DbContext.Update(db_account);
        }

        /// <summary>
        /// Get cashes by account Id
        /// </summary>
        /// <param name="accountID">account Id</param>
        /// <returns>Cashes list</returns>
        public IEnumerable<CommonData.Cash> GetCashes(int accountID)
        {
            List<CommonData.Cash> cashes = new List<CommonData.Cash>();

            var db_cashes = _da.DbContext.Table<Cash>().Where(r => r.AccountID == accountID).ToList();
            foreach (var db_cash in db_cashes)
            {
                var cash = new CommonData.Cash()
                {
                    CashID = db_cash.CashID,
                    Initial = db_cash.Initial,
                    AccountID = db_cash.AccountID,
                    Current = db_cash.Current,
                    Sell = db_cash.Sell,
                    Buy = db_cash.Buy,
                    SellComm = db_cash.SellComm,
                    BuyComm = db_cash.BuyComm
                };
                cashes.Add(cash);
            }

            return cashes;
        }

        /// <summary>
        /// Get Cashflows by account Id
        /// </summary>
        /// <param name="accountID">Account Id</param>
        /// <returns>Cashflows list</returns>
        public IEnumerable<CommonData.Cashflow> GetCashflows(int accountID)
        {
            List<CommonData.Cashflow> cashflows = new List<CommonData.Cashflow>();

            var db_cashflows = _da.DbContext.Table<Cashflow>().Where(r => r.AccountID == accountID).ToList();
            foreach (var db_cashflow in db_cashflows)
            {
                var cashflow = new CommonData.Cashflow()
                {
                    CashflowID = db_cashflow.CashflowID,
                    Summa = db_cashflow.Summa,
                    Time = StorageLib.ToDateTime(db_cashflow.Time),
                    TradeID = db_cashflow.TradeID,
                    Spend = (CashflowSpend)db_cashflow.Spend,
                    AccountID = db_cashflow.AccountID
                };
                cashflows.Add(cashflow);
            }

            return cashflows;
        }

        /// <summary>
        /// Get Holdings by account Id
        /// </summary>
        /// <param name="accountID">account Id</param>
        /// <returns>Holdings list</returns>
        public IEnumerable<CommonData.Holding> GetHoldings(int accountID)
        {
            List<CommonData.Holding> holdings = new List<CommonData.Holding>();

            var db_holdings = _da.DbContext.Table<Holding>().Where(r => r.AccountID == accountID).ToList();
            foreach (var db_holding in db_holdings)
            {
                var holding = new CommonData.Holding()
                {
                    HoldingID = db_holding.HoldingID,
                    InsID = db_holding.InsID,
                    LotCount = db_holding.LotCount,
                    AccountID = db_holding.AccountID
                };
                holdings.Add(holding);
            }

            return holdings;
        }

        /// <summary>
        /// Get Orders list
        /// </summary>
        /// <param name="accountID">account Id</param>
        /// <param name="isActiveOnly">true - active orders only, false - all orders</param>
        /// <returns>Orders list</returns>
        public IEnumerable<CommonData.Order> GetOrders(int accountID, bool isActiveOnly)
        {
            List<CommonData.Order> orders = new List<CommonData.Order>();
            var db_allOrders = _da.DbContext.Table<Order>().Where(r => r.AccountID == accountID);
            List<Order> db_orders;
            if (isActiveOnly)
            {
                db_orders = db_allOrders.Where(r => (OrderStatus)r.Status == OrderStatus.Active).ToList();
            }
            else
            {
                db_orders = db_allOrders.ToList();
            }

            foreach (var db_order in db_orders)
            {
                var order = new CommonData.Order()
                {
                    OrderID = db_order.OrderID,
                    Time = StorageLib.ToDateTime(db_order.Time),
                    InsID = db_order.InsID,
                    BuySell = (BuySell)db_order.BuySell,
                    LotCount = db_order.LotCount,
                    Price = db_order.Price,
                    Status = (OrderStatus)db_order.Status,
                    AccountID = db_order.AccountID,
                    StopOrderID = db_order.StopOrderID,
                    OrderNo = db_order.OrderNo
                };
                orders.Add(order);
            }

            return orders;
        }

        /// <summary>
        /// Get order by orderNo
        /// </summary>
        /// <param name="orderNo">OrderNo</param>
        /// <returns>Order or null if not found</returns>
        public CommonData.Order GetOrder(long orderNo)
        {
            List<CommonData.Order> orders = new List<CommonData.Order>();
            var db_order = _da.DbContext.Table<Order>().FirstOrDefault(r => r.OrderNo == orderNo);
            if (db_order == null) return null;

            return new CommonData.Order()
            {
                OrderID = db_order.OrderID,
                Time = StorageLib.ToDateTime(db_order.Time),
                InsID = db_order.InsID,
                BuySell = (BuySell)db_order.BuySell,
                LotCount = db_order.LotCount,
                Price = db_order.Price,
                Status = (OrderStatus)db_order.Status,
                AccountID = db_order.AccountID,
                StopOrderID = db_order.StopOrderID,
                OrderNo = db_order.OrderNo
            };
        }

        /// <summary>
        /// Get StopOrders list
        /// </summary>
        /// <param name="accountID">account Id</param>
        /// <param name="isActiveOnly">true - active stop orders only, false - all stop orders</param>
        /// <returns>StopOrders list</returns>
        public IEnumerable<CommonData.StopOrder> GetStopOrders(int accountID, bool isActiveOnly)
        {
            List<CommonData.StopOrder> stopOrders = new List<CommonData.StopOrder>();

            var db_allStopOrders = _da.DbContext.Table<StopOrder>().Where(r => r.AccountID == accountID);
            List<StopOrder> db_stopOrders;
            if (isActiveOnly)
            {
                db_stopOrders = db_allStopOrders.Where(r => (StopOrderStatus)r.Status == StopOrderStatus.Active).ToList();
            }
            else
            {
                db_stopOrders = db_allStopOrders.ToList();
            }

            foreach (var db_stopOrder in db_stopOrders)
            {
                var stopOrder = new CommonData.StopOrder()
                {
                    StopOrderID = db_stopOrder.StopOrderID,
                    Time = StorageLib.ToDateTime(db_stopOrder.Time),
                    InsID = db_stopOrder.InsID,
                    BuySell = (BuySell)db_stopOrder.BuySell,
                    StopType = (StopOrderType)db_stopOrder.StopType,
                    EndTime = StorageLib.ToDateTime(db_stopOrder.EndTime),
                    AlertPrice = db_stopOrder.AlertPrice,
                    Price = db_stopOrder.Price,
                    LotCount = db_stopOrder.LotCount,
                    Status = (StopOrderStatus)db_stopOrder.Status,
                    AccountID = db_stopOrder.AccountID,
                    CompleteTime = StorageLib.ToDateTime(db_stopOrder.CompleteTime),
                    StopOrderNo = db_stopOrder.StopOrderNo
                };
                stopOrders.Add(stopOrder);
            }

            return stopOrders;
        }

        /// <summary>
        /// Get stopOrder by stopOrderNo
        /// </summary>
        /// <param name="stopOrderNo">StopOrderNo</param>
        /// <returns>StopOrder</returns>
        public CommonData.StopOrder GetStopOrder(long stopOrderNo)
        {
            var db_stopOrder = _da.DbContext.Table<StopOrder>().FirstOrDefault(r => r.StopOrderNo == stopOrderNo);
            if (db_stopOrder == null) return null;

            return new CommonData.StopOrder()
            {
                StopOrderID = db_stopOrder.StopOrderID,
                Time = StorageLib.ToDateTime(db_stopOrder.Time),
                InsID = db_stopOrder.InsID,
                BuySell = (BuySell)db_stopOrder.BuySell,
                StopType = (StopOrderType)db_stopOrder.StopType,
                EndTime = StorageLib.ToDateTime(db_stopOrder.EndTime),
                AlertPrice = db_stopOrder.AlertPrice,
                Price = db_stopOrder.Price,
                LotCount = db_stopOrder.LotCount,
                Status = (StopOrderStatus)db_stopOrder.Status,
                AccountID = db_stopOrder.AccountID,
                CompleteTime = StorageLib.ToDateTime(db_stopOrder.CompleteTime),
                StopOrderNo = db_stopOrder.StopOrderNo
            };
        }

        /// <summary>
        /// Get trades list by account Id
        /// </summary>
        /// <param name="accountID">Account Id</param>
        /// <returns>Trades list</returns>
        public IEnumerable<CommonData.Trade> GetTrades(int accountID, DateTime? dateFrom)
        {
            List<CommonData.Trade> trades = new List<CommonData.Trade>();

            List<Trade> db_trades;
            var db_alltrades = _da.DbContext.Table<Trade>().Where(r => r.AccountID == accountID);
            if (dateFrom != null)
            {
                int df = StorageLib.ToDbTime(dateFrom.Value);
                db_trades = db_alltrades.Where(r => r.Time >= df).ToList();
            }
            else
            {
                db_trades = db_alltrades.ToList();
            }

            foreach (var db_trade in db_trades)
            {
                var trade = new CommonData.Trade()
                {
                    TradeID = db_trade.TradeID,
                    OrderID = db_trade.OrderID,
                    Time = StorageLib.ToDateTime(db_trade.Time),
                    InsID = db_trade.InsID,
                    BuySell = (BuySell)db_trade.BuySell,
                    LotCount = db_trade.LotCount,
                    Price = db_trade.Price,
                    AccountID = db_trade.AccountID,
                    Comm = db_trade.Comm,
                    TradeNo = db_trade.TradeNo
                };
                trades.Add(trade);
            }

            return trades;
        }

        /// <summary>
        /// Insert new Cash object into db
        /// </summary>
        /// <param name="cash">Cash object (CashID = 0)</param>
        /// <returns>New cash Id (also set CashID)</returns>
        public int InsertCash(CommonData.Cash cash)
        {
            Cash db_cash = new Cash()
            {
                CashID = cash.CashID,
                Initial = cash.Initial,
                AccountID = cash.AccountID,
                Current = cash.Current,
                Sell = cash.Sell,
                Buy = cash.Buy,
                SellComm = cash.SellComm,
                BuyComm = cash.BuyComm
            };

            _da.DbContext.Insert(db_cash);
            cash.CashID = db_cash.CashID;

            return cash.CashID;
        }

        /// <summary>
        /// Update cash object
        /// </summary>
        /// <param name="cash">Cash object (CashID > 0)</param>
        public void UpdateCash(CommonData.Cash cash)
        {
            Cash db_cash = new Cash()
            {
                CashID = cash.CashID,
                Initial = cash.Initial,
                AccountID = cash.AccountID,
                Current = cash.Current,
                Sell = cash.Sell,
                Buy = cash.Buy,
                SellComm = cash.SellComm,
                BuyComm = cash.BuyComm
            };

            _da.DbContext.Update(db_cash);
        }

        /// <summary>
        /// Insert new Cashflow object into db
        /// </summary>
        /// <param name="cashflow">Cashflow object (CashflowID = 0)</param>
        /// <returns>New Id (also set CashflowID)</returns>
        public int InsertCashflow(CommonData.Cashflow cashflow)
        {
            Cashflow db_cashflow = new Cashflow()
            {
                CashflowID = cashflow.CashflowID,
                Summa = cashflow.Summa,
                Time = StorageLib.ToDbTime(cashflow.Time),
                TradeID = cashflow.TradeID,
                Spend = (byte)cashflow.Spend,
                AccountID = cashflow.AccountID
            };

            _da.DbContext.Insert(db_cashflow);
            cashflow.CashflowID = db_cashflow.CashflowID;

            return cashflow.CashflowID;
        }

        /// <summary>
        /// Insert new Holding object into db
        /// </summary>
        /// <param name="holding">Holding object (HoldingID = 0)</param>
        /// <returns>retuan new Id and set HoldingID to new value</returns>
        public int InsertHolding(CommonData.Holding holding)
        {
            Holding db_holding = new Holding()
            {
                HoldingID = holding.HoldingID,
                InsID = holding.InsID,
                LotCount = holding.LotCount,
                AccountID = holding.AccountID
            };

            _da.DbContext.Insert(db_holding);
            holding.HoldingID = db_holding.HoldingID;

            return holding.HoldingID;
        }

        /// <summary>
        /// Update Holding object
        /// </summary>
        /// <param name="holding">Holding object</param>
        public void UpdateHolding(CommonData.Holding holding)
        {
            Holding db_holding = new Holding()
            {
                HoldingID = holding.HoldingID,
                InsID = holding.InsID,
                LotCount = holding.LotCount,
                AccountID = holding.AccountID
            };

            _da.DbContext.Update(db_holding);
        }

        /// <summary>
        /// Delete Holding object
        /// </summary>
        /// <param name="holdingID">Holding identity</param>
        public void DeleteHolding(int holdingID)
        {
            _da.DbContext.Execute("delete from [Holding] where HoldingID = " + holdingID.ToString());
        }

        /// <summary>
        /// Insert Order object into db
        /// </summary>
        /// <param name="order">Order with OrderID = 0</param>
        /// <returns>New Id and set OrderID to new value </returns>
        public int InsertOrder(CommonData.Order order)
        {
            Order db_order = new Order()
            {
                OrderID = order.OrderID,
                Time = StorageLib.ToDbTime(order.Time),
                InsID = order.InsID,
                BuySell = (byte)order.BuySell,
                LotCount = order.LotCount,
                Price = order.Price,
                Status = (byte)order.Status,
                AccountID = order.AccountID,
                StopOrderID = order.StopOrderID,
                OrderNo = order.OrderNo
            };

            _da.DbContext.Insert(db_order);
            order.OrderID = db_order.OrderID;

            return order.OrderID;
        }

        /// <summary>
        /// Update Order object
        /// </summary>
        /// <param name="order">Order object (OrderID > 0)</param>
        public void UpdateOrder(CommonData.Order order)
        {
            Order db_order = new Order()
            {
                OrderID = order.OrderID,
                Time = StorageLib.ToDbTime(order.Time),
                InsID = order.InsID,
                BuySell = (byte)order.BuySell,
                LotCount = order.LotCount,
                Price = order.Price,
                Status = (byte)order.Status,
                AccountID = order.AccountID,
                StopOrderID = order.StopOrderID,
                OrderNo = order.OrderNo
            };

            _da.DbContext.Update(db_order);
        }

        /// <summary>
        /// Insert StopOrder object into db
        /// </summary>
        /// <param name="stopOrder">StopOrder object with StopOrderID = 0</param>
        /// <returns>New Id and set new value to StopOrderID</returns>
        public int InsertStopOrder(CommonData.StopOrder stopOrder)
        {
            StopOrder db_stopOrder = new StopOrder()
            {
                StopOrderID = stopOrder.StopOrderID,
                Time = StorageLib.ToDbTime(stopOrder.Time),
                InsID = stopOrder.InsID,
                BuySell = (byte)stopOrder.BuySell,
                StopType = (byte)stopOrder.StopType,
                EndTime = StorageLib.ToDbTime(stopOrder.EndTime),
                AlertPrice = stopOrder.AlertPrice,
                Price = stopOrder.Price,
                LotCount = stopOrder.LotCount,
                Status = (byte)stopOrder.Status,
                AccountID = stopOrder.AccountID,
                CompleteTime = StorageLib.ToDbTime(stopOrder.CompleteTime),
                StopOrderNo = stopOrder.StopOrderNo
            };

            _da.DbContext.Insert(db_stopOrder);
            stopOrder.StopOrderID = db_stopOrder.StopOrderID;

            return stopOrder.StopOrderID;
        }

        /// <summary>
        /// Update StopOrder object
        /// </summary>
        /// <param name="stopOrder">StopOrder with StopOrderID > 0</param>
        public void UpdateStopOrder(CommonData.StopOrder stopOrder)
        {
            StopOrder db_stopOrder = new StopOrder()
            {
                StopOrderID = stopOrder.StopOrderID,
                Time = StorageLib.ToDbTime(stopOrder.Time),
                InsID = stopOrder.InsID,
                BuySell = (byte)stopOrder.BuySell,
                StopType = (byte)stopOrder.StopType,
                EndTime = StorageLib.ToDbTime(stopOrder.EndTime),
                AlertPrice = stopOrder.AlertPrice,
                Price = stopOrder.Price,
                LotCount = stopOrder.LotCount,
                Status = (byte)stopOrder.Status,
                AccountID = stopOrder.AccountID,
                CompleteTime = StorageLib.ToDbTime(stopOrder.CompleteTime),
                StopOrderNo = stopOrder.StopOrderNo
            };

            _da.DbContext.Update(db_stopOrder);
        }

        /// <summary>
        /// Insert Trade object
        /// </summary>
        /// <param name="trade">Trade object with TradeID = 0</param>
        /// <returns>New TradeID value and set it to object</returns>
        public int InsertTrade(CommonData.Trade trade)
        {
            Trade db_trade = new Trade()
            {
                TradeID = trade.TradeID,
                OrderID = trade.OrderID,
                Time = StorageLib.ToDbTime(trade.Time),
                InsID = trade.InsID,
                BuySell = (byte)trade.BuySell,
                LotCount = trade.LotCount,
                Price = trade.Price,
                AccountID = trade.AccountID,
                Comm = trade.Comm,
                TradeNo = trade.TradeNo
            };

            _da.DbContext.Insert(db_trade);
            trade.TradeID = db_trade.TradeID;

            return trade.TradeID;
        }

        /// <summary>
        /// Delete all account data by accountID (trades, stoporders, orders, holdings, cashflows, cashes and account)
        /// </summary>
        /// <param name="accountID">AccountID</param>
        public void DeleteAccountData(int accountID)
        {
            _da.DbContext.Execute("delete from [Trade] where AccountID = " + accountID.ToString());
            _da.DbContext.Execute("delete from [StopOrder] where AccountID = " + accountID.ToString());
            _da.DbContext.Execute("delete from [Order] where AccountID = " + accountID.ToString());
            _da.DbContext.Execute("delete from [Holding] where AccountID = " + accountID.ToString());
            _da.DbContext.Execute("delete from [Cashflow] where AccountID = " + accountID.ToString());
            _da.DbContext.Execute("delete from [Cash] where AccountID = " + accountID.ToString());
            _da.DbContext.Execute("delete from [Account] where AccountID = " + accountID.ToString());
        }
    }
}
