using Common;
using Common.Data;
using LeechSvc.BL;
using LeechSvc.Logger;
using System;
using System.Collections.Generic;
using TEClientLib;

namespace LeechSvc
{
    public class AlorTradeWrapper
    {
        private SlotFace _sf = null;
        private Slot _slot = null;
        private SlotTables _tables = null;
        private SlotTable _securities_table = null;
        private SlotTable _trdacc_table = null;
        private SlotTable _all_trades_table = null;
        private SlotTable _orders_table = null;
        private SlotTable _stoporders_table = null;
        private SlotTable _trades_table = null;
        private SlotTable _holding_table = null;
        private SlotTable _positions_table = null;
        private IInstrumTable _instrumTable = null;
        private IStopOrderTable _stopOrderTable = null;
        private IOrderTable _orderTable = null;
        private ITradeTable _tradeTable = null;
        private IHoldingTable _holdingTable = null;
        private IPositionTable _positionTable = null;
        private IAccountTable _accountTable = null;
        private readonly ITickDispatcher _tickDispatcher = null;
        private readonly ILeechConfig _leechConfig = null;
        private readonly ILogger _logger = null;
        private string _secBoard = "";
        private int _addHours = 0;
        private TimeSpan _startSessionTime, _endSessionTime;

        public AlorTradeWrapper(IInstrumTable insTable, IStopOrderTable stopOrderTable, IOrderTable orderTable, 
            ITradeTable tradeTable, IHoldingTable holdingTable, IPositionTable positionTable, 
            IAccountTable accountTable, ITickDispatcher tickDisp, ILeechConfig config, ILogger logger)
        {
            _instrumTable = insTable;
            _stopOrderTable = stopOrderTable;
            _orderTable = orderTable;
            _tradeTable = tradeTable;
            _holdingTable = holdingTable;
            _positionTable = positionTable;
            _accountTable = accountTable;
            _tickDispatcher = tickDisp;
            _leechConfig = config;
            _logger = logger;
            _secBoard = _leechConfig.SecBoard;
            _addHours = _leechConfig.CorrectHours;
            _startSessionTime = _leechConfig.StartSessionTime;
            _endSessionTime = _leechConfig.EndSessionTime;
        }

        public void OpenTerminal()
        {
            Type type = Type.GetTypeFromProgID("TEClient.SlotFace");
            _sf = (SlotFace)Activator.CreateInstance(type);
            _sf.Synchronized += OnSynchronized;
            _sf.Open(0);
            _logger.AddInfo("AlorTradeWrapper", "App opened");
        }

        public void CloseTerminal()
        {
            if (_sf == null) return;

            if (_securities_table != null)
            {
                _securities_table.AddRow -= OnAddRow;
                _securities_table.ReplEnd -= OnReplEnd;
            }

            if (_trdacc_table != null)
            {
                _trdacc_table.AddRow -= OnAddRow;
            }

            if (_all_trades_table != null)
            {
                _all_trades_table.AddRow -= OnAddRow;
            }

            if (_orders_table != null)
            {
                _orders_table.AddRow -= OnAddRow;
                _orders_table.UpdateRow -= OnUpdateRow;
            }

            if (_stoporders_table != null)
            {
                _stoporders_table.AddRow -= OnAddRow;
                _stoporders_table.UpdateRow -= OnUpdateRow;
            }

            if (_trades_table != null)
            {
                _trades_table.AddRow -= OnAddRow;
            }

            if (_holding_table != null)
            {
                _holding_table.AddRow -= OnAddRow;
                _holding_table.UpdateRow -= OnUpdateRow;
            }

            if (_positions_table != null)
            {
                _positions_table.AddRow -= OnAddRow;
                _positions_table.UpdateRow -= OnUpdateRow;
            }

            _sf.Close(0);
            _sf.Synchronized -= OnSynchronized;
            _sf.CloseApp();

            _logger.AddInfo("AlorTradeWrapper", "App closed");
        }

        public int Connect(string server, string login, string password)
        {
            _logger.AddInfo("AlorTradeWrapper", "Connect ...");
            if (_sf == null) return -1;

            string res;
            _sf.Connect(0, server, login, password, out res);

            if (res != "OK") return -2;
            _logger.AddInfo("AlorTradeWrapper", "Connected");

            return 0;
        }

        public int Disconnect()
        {
            _logger.AddInfo("AlorTradeWrapper", "Disconnect ...");
            if (_sf == null) return -1;

            string res;
            _sf.Disconnect(0, out res);

            if (res != "OK") return -2;
            _logger.AddInfo("AlorTradeWrapper", "Disconnected");

            return 0;
        }

        private void OnSynchronized(int OpenID, int SlotID)
        {
            _slot = (Slot)_sf.GetSlot(0);
            _tables = (SlotTables)_slot.tables;
            int count = _tables.Count;

            for (int i = 1; i <= count; i++)
            {
                SlotTable table = (SlotTable)_tables.Item[i];

                if (table.Name == "SECURITIES")
                {
                    _securities_table = table;
                    _securities_table.AddRow += OnAddRow;
                    _securities_table.ReplEnd += OnReplEnd;
                    _securities_table.Open(_slot.ID, _securities_table.Name, ""); // таблицу инструментов открываем сразу, а остальные таблицы после
                }
                if (table.Name == "TRDACC")
                {
                    _trdacc_table = table;
                    _trdacc_table.AddRow += OnAddRow;
                }
                else if (table.Name == "ALL_TRADES")
                {
                    _all_trades_table = table;
                    _all_trades_table.AddRow += OnAddRow;
                }
                else if (table.Name == "ORDERS")
                {
                    _orders_table = table;
                    _orders_table.AddRow += OnAddRow;
                    _orders_table.UpdateRow += OnUpdateRow;
                }
                else if (table.Name == "STOPORDERS")
                {
                    _stoporders_table = table;
                    _stoporders_table.AddRow += OnAddRow;
                    _stoporders_table.UpdateRow += OnUpdateRow;
                }
                else if (table.Name == "TRADES")
                {
                    _trades_table = table;
                    _trades_table.AddRow += OnAddRow;
                }
                else if (table.Name == "HOLDING")
                {
                    _holding_table = table;
                    _holding_table.AddRow += OnAddRow;
                    _holding_table.UpdateRow += OnUpdateRow;
                }
                else if (table.Name == "POSITIONS")
                {
                    _positions_table = table;
                    _positions_table.AddRow += OnAddRow;
                    _positions_table.UpdateRow += OnUpdateRow;
                }
            }
        }

        private int _allTrades_lastRowID = -1;
        private int _securities_lastRowID = -1;

        /// <summary>
        /// Инициализация перед началом торговой сессии
        /// </summary>
        public void Initialize()
        {
            _allTrades_lastRowID = -1;
            _securities_lastRowID = -1;
        }

        /// <summary>
        /// Завершение после окончания торговой сессии
        /// </summary>
        public void Close()
        {
        }

        private void OnAddRow(int OpenID, int RowID, object Fields)
        {
            object[] fields = (object[])Fields;

            if (_all_trades_table != null && OpenID == _all_trades_table.ID)
            {
                if (RowID <= _allTrades_lastRowID) return;
                _allTrades_lastRowID = RowID;

                string secboard = Convert.ToString(fields[5]).Trim();
                if (secboard != _secBoard) return;

                string ticker = Convert.ToString(fields[6]).Trim();
                long tradeNo = Convert.ToInt64(fields[2]);
                DateTime time = Convert.ToDateTime(fields[3]).AddHours(_addHours);
                if (time.TimeOfDay < _startSessionTime) time = new DateTime(time.Year, time.Month, time.Day, _startSessionTime.Hours, _startSessionTime.Minutes, _startSessionTime.Seconds);
                if (time.TimeOfDay > _endSessionTime) time = new DateTime(time.Year, time.Month, time.Day, _endSessionTime.Hours, _endSessionTime.Minutes, _endSessionTime.Seconds);

                int lots = Convert.ToInt32(fields[8]);
                double price = Convert.ToDouble(fields[9]);

                Common.Data.Instrum ins = _instrumTable.GetInstrum(ticker);
                if (ins == null) return;

                _tickDispatcher.AddTick(new Tick(tradeNo, time, ins.InsID, lots, (decimal)price));
            }
            else if (_securities_table != null && OpenID == _securities_table.ID)
            {
                if (RowID <= _securities_lastRowID) return;
                _securities_lastRowID = RowID;

                string secboard = Convert.ToString(fields[1]).Trim();
                if (secboard != _secBoard) return;

                string ticker = Convert.ToString(fields[2]).Trim();
                string name = Convert.ToString(fields[3]).Trim();
                string shortname = Convert.ToString(fields[4]).Trim();
                int lotsize = Convert.ToInt32(fields[13]);
                decimal pricestep = Convert.ToDecimal(fields[15]);
                int decimals = Convert.ToInt32(fields[17]);

                _instrumTable.SyncInstrum(ticker, shortname, name, lotsize, decimals, pricestep);
            }
            else if (_trdacc_table != null && OpenID == _trdacc_table.ID)
            {
                string account = Convert.ToString(fields[1]).Trim();
                string name = Convert.ToString(fields[2]).Trim();
                _accountTable.AddAccount(account, name);
            }
            else if (_orders_table != null && OpenID == _orders_table.ID)
            {
                var ticker = Convert.ToString(fields[21]).Trim();
                var ins = _instrumTable.GetInstrum(ticker);
                if (ins == null) return;

                string accCode = Convert.ToString(fields[19]).Trim();
                var acc = _accountTable.GetAccount(accCode);
                if (acc == null) return;

                Order order = new Order();
                order.OrderNo = Convert.ToInt64(fields[1]);
                order.Time = Convert.ToDateTime(fields[3]).AddHours(_addHours);
                order.InsID = ins.InsID;
                order.AccountID = acc.AccountID;

                var bs = Convert.ToString(fields[12]).Trim();
                order.BuySell = bs == "B" ? BuySell.Buy : BuySell.Sell;

                order.LotCount = Convert.ToInt32(fields[25]);

                order.Price = Convert.ToDecimal(fields[23]);
                if (order.Price == 0) order.Price = null;

                var status = Convert.ToString(fields[10]).Trim();
                if (status == "M")
                    order.Status = OrderStatus.Trade;
                else if (status == "C")
                    order.Status = OrderStatus.EndTime;
                else if (status == "N")
                    order.Status = OrderStatus.Reject;
                else if (status == "W")
                    order.Status = OrderStatus.Remove;
                else
                    order.Status = OrderStatus.Active; // A, O

                _orderTable.AddOrder(order);
            }
            else if (_stoporders_table != null && OpenID == _stoporders_table.ID)
            {
                string secboard = Convert.ToString(fields[4]).Trim();
                if (secboard != _secBoard) return;

                var ticker = Convert.ToString(fields[5]).Trim();
                var ins = _instrumTable.GetInstrum(ticker);
                if (ins == null) return;

                string accCode = Convert.ToString(fields[21]).Trim();
                var acc = _accountTable.GetAccount(accCode);
                if (acc == null) return;

                StopOrder so = new StopOrder();
                so.StopOrderNo = Convert.ToInt64(fields[3]);
                so.Time = Convert.ToDateTime(fields[1]).AddHours(_addHours);
                so.InsID = ins.InsID;
                so.AccountID = acc.AccountID;

                var bs = Convert.ToString(fields[6]).Trim();
                so.BuySell = bs == "B" ? BuySell.Buy : BuySell.Sell;

                var st = Convert.ToString(fields[10]).Trim();
                so.StopType = StopOrderType.TakeProfit;
                if (st == "L")
                    so.StopType = StopOrderType.StopLoss;
                else if (st == "P")
                    so.StopType = StopOrderType.TakeProfit;

                so.AlertPrice = Convert.ToDecimal(fields[13]);

                so.Price = Convert.ToDecimal(fields[14]);
                if (so.Price == 0) so.Price = null;

                so.LotCount = Convert.ToInt32(fields[15]);
                var status = Convert.ToString(fields[18]).Trim();
                if (status == "M")
                    so.Status = StopOrderStatus.Order;
                else if (status == "E")
                    so.Status = StopOrderStatus.EndTime;
                else if (status == "R")
                    so.Status = StopOrderStatus.Reject;
                else if (status == "W")
                    so.Status = StopOrderStatus.Remove;
                else
                    so.Status = StopOrderStatus.Active; // A, O, space

                so.EndTime = (DateTime?)fields[11]; // если время не указать, то сначала оно будет null, а потом сработает update, поменяется статус на Активный и время завершения установится на конец торговой сессии
                if (so.EndTime != null) so.EndTime = so.EndTime.Value.AddHours(_addHours);
                _stopOrderTable.AddStopOrder(so, GetCurrentTime());
            }
            else if (_trades_table != null && OpenID == _trades_table.ID)
            {
                var ticker = Convert.ToString(fields[6]).Trim();
                var ins = _instrumTable.GetInstrum(ticker);
                if (ins == null) return;

                string accCode = Convert.ToString(fields[29]).Trim();
                var acc = _accountTable.GetAccount(accCode);
                if (acc == null) return;

                long orderNo = Convert.ToInt64(fields[2]);
                Order order = _orderTable.GetOrder(orderNo);
                if (order == null) return;

                Trade trade = new Trade();
                trade.TradeNo = Convert.ToInt64(fields[1]);
                trade.OrderID = order.OrderID;
                trade.Time = Convert.ToDateTime(fields[3]).AddHours(_addHours);
                trade.InsID = ins.InsID;
                trade.AccountID = acc.AccountID;
                var bs = Convert.ToString(fields[8]).Trim();
                trade.BuySell = bs == "B" ? BuySell.Buy : BuySell.Sell;
                trade.LotCount = Convert.ToInt32(fields[9]);
                trade.Price = Convert.ToDecimal(fields[10]);

                _tradeTable.AddTrade(trade);
            }
            else if (_holding_table != null && OpenID == _holding_table.ID)
            {
                var ticker = Convert.ToString(fields[5]).Trim();
                var ins = _instrumTable.GetInstrum(ticker);
                if (ins == null) return;

                var acc = _accountTable.GetDefaultAccount();
                if (acc == null) return;

                int lots = Convert.ToInt32(fields[8]);
                _holdingTable.SetHolding(acc.AccountID, ins.InsID, lots);
            }
            else if (_positions_table != null && OpenID == _positions_table.ID)
            {
                string accCode = Convert.ToString(fields[2]).Trim();
                var acc = _accountTable.GetAccount(accCode);
                if (acc == null) return;

                double pos = Convert.ToDouble(fields[11]);
                decimal curPos = (decimal)pos;
                _positionTable.SetPosition(acc.AccountID, curPos);
            }
        }

        private void OnUpdateRow(int OpenID, int RowID, object Fields)
        {
            object[] fields = (object[])Fields;

            if (OpenID == _stoporders_table.ID)
            {
                long stopOrderNo = Convert.ToInt64(fields[3]);
                DateTime? endTime = (DateTime?)fields[11]; // время может поменяться на конец текущей торговой сессии, если оно сначало было null. Потом время окончания уже не меняется, например, при снятии заявки пользователем.
                if (endTime != null) endTime = endTime.Value.AddHours(_addHours);
                var status = Convert.ToString(fields[18]).Trim();
                StopOrderStatus soStatus;
                if (status == "M")
                    soStatus = StopOrderStatus.Order;
                else if (status == "E")
                    soStatus = StopOrderStatus.EndTime;
                else if (status == "R")
                    soStatus = StopOrderStatus.Reject;
                else if (status == "W")
                    soStatus = StopOrderStatus.Remove;
                else
                    soStatus = StopOrderStatus.Active; // A, O, space

                _stopOrderTable.UpdateStopOrder(stopOrderNo, endTime, soStatus, GetCurrentTime());
            }
            else if (OpenID == _orders_table.ID)
            {
                long orderNo = Convert.ToInt64(fields[1]);
                var status = Convert.ToString(fields[10]).Trim();
                OrderStatus orderStatus;
                if (status == "M")
                    orderStatus = OrderStatus.Trade;
                else if (status == "C")
                    orderStatus = OrderStatus.EndTime;
                else if (status == "N")
                    orderStatus = OrderStatus.Reject;
                else if (status == "W")
                    orderStatus = OrderStatus.Remove;
                else
                    orderStatus = OrderStatus.Active; // A, O

                _orderTable.UpdateOrder(orderNo, orderStatus);
            }
            else if (OpenID == _holding_table.ID)
            {
                var ticker = Convert.ToString(fields[5]).Trim();
                var ins = _instrumTable.GetInstrum(ticker);
                if (ins == null) return;

                var acc = _accountTable.GetDefaultAccount();
                if (acc == null) return;

                int lots = Convert.ToInt32(fields[8]);
                _holdingTable.SetHolding(acc.AccountID, ins.InsID, lots);
            }
            else if (OpenID == _positions_table.ID)
            {
                string accCode = Convert.ToString(fields[2]).Trim();
                var acc = _accountTable.GetAccount(accCode);
                if (acc == null) return;

                double pos = Convert.ToDouble(fields[11]);
                decimal curPos = (decimal)pos;
                _positionTable.SetPosition(acc.AccountID, curPos);
            }
        }

        private void OnReplEnd(int OpenID)
        {
            if (_securities_table != null && OpenID == _securities_table.ID)
            {
                _trdacc_table.Open(_slot.ID, _trdacc_table.Name, "");
                _all_trades_table.Open(_slot.ID, _all_trades_table.Name, "");
                _orders_table.Open(_slot.ID, _orders_table.Name, "");
                _stoporders_table.Open(_slot.ID, _stoporders_table.Name, "");
                _trades_table.Open(_slot.ID, _trades_table.Name, "");
                _holding_table.Open(_slot.ID, _holding_table.Name, "");
                _positions_table.Open(_slot.ID, _positions_table.Name, "");
            }
        }

        private DateTime GetCurrentTime()
        {
            var now = DateTime.Now.AddHours(_addHours);
            return new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
        }
    }
}
