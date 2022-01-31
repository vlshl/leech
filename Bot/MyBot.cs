using Common;
using LeechSvc;
using System;

namespace Bot
{
    public class MyBot : BotBase
    {
        private readonly ILeechPlatform _platform;
        private IBarRow _bars;
        private long _tickCount = 0;
        private int _gazpId = 0;

        public MyBot(ILeechPlatform platform)
        {
            _platform = platform;
        }

        public override void Close()
        {
            _platform.AddLog("MyBot", "Closed");
            _platform.OnTick(_gazpId, OnTick, false);
            _platform.OnTimer(null);
        }

        public override async void Initialize(string data)
        {
            _platform.AddLog("MyBot", "Initialize ...");
            var gazp = _platform.GetInstrum("GAZP");
            if (gazp == null) return;

            _gazpId = gazp.InsID;
            _bars = await _platform.CreateBarRow(gazp.InsID, Timeframes.Min, 5);
            _bars.OnCloseBar += Bars_OnCloseBar;
            _bars.Close.Change += Close_Change;
            _platform.AddLog("MyBot", "Initialized");

            _platform.OnTick(gazp.InsID, OnTick, true);
            _platform.OnTimer(OnTimer);
        }

        private void OnTimer(DateTime time, int delay)
        {
            if (time.Second == 0)
            {
                _platform.AddLog("MyBot", string.Format("OnTimer: time={0}, delay={1}",
                    time.ToString("yyyy-MM-dd HH:mm:ss"), delay.ToString()));
            }
        }

        private void OnTick(DateTime time, decimal price, int lots)
        {
            _tickCount++;
            if (_tickCount % 1000 == 0)
            {
                _platform.AddLog("MyBot", string.Format("OnTick: time={0}, price={1}, lots={2}",
                    time.ToString("dd.MM.yyyy HH:mm:ss"), price.ToString(), lots.ToString()));
            }
        }

        private void Close_Change(bool isReset)
        {
            //_platform.AddLog("MyBot", "CloseChange, isReset=" + isReset.ToString());
        }

        private void Bars_OnCloseBar(int index)
        {
            //var time = _bars[index].Time;
            //decimal? close = _bars.Close[index];

            //_platform.AddLog("MyBot", string.Format("CloseBar: time={0}, close={1}, index={2}", 
            //    time.ToString("HH:mm:ss"), (close != null ? close.Value.ToString() : "null"), index.ToString()));
        }
    }
}
