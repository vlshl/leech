using Common;
using Common.Data;
using LeechSvc.BL;
using LeechSvc.Logger;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LeechSvc
{
    public class LeechPlatform : ILeechPlatform
    {
        private readonly ITickDispatcher _tickDisp;
        private readonly ILogger _logger;
        private readonly IInstrumTable _instrumTable;
        private readonly IHoldingTable _holdingTable;
        private readonly IOrderTable _orderTable;
        private readonly IAccountTable _accountTable;
        private readonly AlorTradeWrapper _alorTrade;
        private readonly IInsStoreBL _insStoreBL;
        private readonly ILeechConfig _leechConfig;
        private List<BarRow> _barRows;
        private OnTimerDelegate _onTimer;
        private Task _onTimerTask;
        private Dictionary<int, OnTickDelegate> _ins_onTick;
        private Dictionary<int, IPosManager> _insID_pm;

        public LeechPlatform(ITickDispatcher tickDisp, IInstrumTable insTable, IHoldingTable holdTable, IOrderTable orderTable, IAccountTable accountTable, 
            AlorTradeWrapper alorTrade, ILogger logger, IInsStoreBL insStoreBL, ILeechConfig leechConfig)
        {
            _tickDisp = tickDisp;
            _instrumTable = insTable;
            _holdingTable = holdTable;
            _orderTable = orderTable;
            _accountTable = accountTable;
            _alorTrade = alorTrade;
            _logger = logger;
            _insStoreBL = insStoreBL;
            _leechConfig = leechConfig;
            _barRows = new List<BarRow>();
            _onTimer = null;
            _onTimerTask = null;
            _ins_onTick = new Dictionary<int, OnTickDelegate>();
            _insID_pm = new Dictionary<int, IPosManager>();
        }

        public void AddLog(string source, string text)
        {
            _logger.AddInfo("Bot:" + source, text);
        }

        public IInstrum GetInstrum(string ticker)
        {
            return _instrumTable.GetInstrum(ticker);
        }

        public void Close()
        {
            foreach (var br in _barRows) br.CloseBarRow();
            foreach (var insID in _insID_pm.Keys) _insID_pm[insID].ClosePosManager();
            _insID_pm.Clear();
        }

        public async Task<IBarRow> CreateBarRow(int insId, Timeframes tf, int historyDays)
        {
            BarRow bars = new BarRow(tf, _tickDisp, insId);
            DateTime curDate = DateTime.Today;
            var endHistoryDate = curDate.AddDays(-1);
            var startHistoryDate = endHistoryDate.AddDays(-historyDays);
            await _insStoreBL.LoadHistoryAsync(bars, insId, startHistoryDate, endHistoryDate);
            _barRows.Add(bars);

            return bars;
        }

        public void OnTimer(OnTimerDelegate onTimer)
        {
            _onTimer = onTimer;
            if (_onTimer != null && _onTimerTask == null)
            {
                _onTimerTask = Task.Factory.StartNew(() =>
                {
                    DateTime prev = DateTime.MinValue;
                    while (_onTimer != null)
                    {
                        var mskNow = DateTime.Now.AddHours(_leechConfig.CorrectHours); // сразу переходим на время биржи, т.к. в тиках тоже стоит биржевое время, а не местное
                        DateTime t = new DateTime(mskNow.Year, mskNow.Month, mskNow.Day, mskNow.Hour, mskNow.Minute, mskNow.Second).AddSeconds(1);
                        long waitToNextSec = t.Ticks - mskNow.Ticks;
                        if (waitToNextSec > 0)
                        {
                            Thread.Sleep(new TimeSpan(waitToNextSec));
                        }
                        if (t == prev) continue;

                        prev = t;
                        var lastTick = _tickDisp.GetLastTick();
                        if (lastTick.InsID == 0)
                        {
                            _onTimer(t, 0);
                        }
                        else
                        {
                            int delay = (int)Math.Round(new TimeSpan(t.Ticks - lastTick.Time.Ticks).TotalMilliseconds);
                            _onTimer(t, delay);
                        }
                    }
                });
            }
            else if (_onTimer == null && _onTimerTask != null)
            {
                _onTimerTask = null;
            }
        }

        public void OnTick(int insID, OnTickDelegate onTick, bool isSubscribe)
        {
            if (isSubscribe)
            {
                if (!_ins_onTick.ContainsKey(insID))
                {
                    _ins_onTick.Add(insID, onTick);
                    _tickDisp.Subscribe(this, insID, OnTick);
                }
            }
            else
            {
                if (_ins_onTick.ContainsKey(insID))
                {
                    _ins_onTick.Remove(insID);
                    _tickDisp.Unsubscribe(this, insID);
                }
            }
        }

        private void OnTick(Tick tick)
        {
            if (_ins_onTick.ContainsKey(tick.InsID))
            {
                _ins_onTick[tick.InsID].Invoke(tick.Time, tick.Price, tick.Lots);
            }
        }

        public IPosManager GetPosManager(int insID)
        {
            if (_insID_pm.ContainsKey(insID))
            {
                return _insID_pm[insID];
            }
            else
            {
                var pm = new PosManager(insID, _instrumTable, _holdingTable, _orderTable, _accountTable, _alorTrade);
                _insID_pm.Add(insID, pm);

                return pm;
            }
        }
    }
}
