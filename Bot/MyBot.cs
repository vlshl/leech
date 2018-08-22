using Common;
using LeechSvc;
using System;

namespace Bot
{
    public class MyBot : BotBase
    {
        private readonly ILeechPlatform _platform;
        private BarRow _bars;

        public MyBot(ILeechPlatform platform)
        {
            _platform = platform;
        }

        public override void Close()
        {
            _platform.AddLog("MyBot", "Closed");
        }

        public override void Initialize(string data)
        {
            _platform.AddLog("MyBot", "Initialize ...");
            var gazp = _platform.GetInstrum("GAZP");
            if (gazp == null) return;

            _bars = _platform.CreateBarRow(gazp.InsID, Timeframes.Min);
            _bars.OnCloseBar += Bars_OnCloseBar;
            _bars.Close.Change += Close_Change;
            _platform.AddLog("MyBot", "Initialized");
        }

        private void Close_Change(bool isReset)
        {
            //_platform.AddLog("MyBot", "CloseChange, isReset=" + isReset.ToString());
        }

        private void Bars_OnCloseBar(int index)
        {
            var time = _bars[index].Time;
            decimal? close = _bars.Close[index];

            _platform.AddLog("MyBot", string.Format("CloseBar: time={0}, close={1}, index={2}", 
                time.ToString("HH:mm:ss"), (close != null ? close.Value.ToString() : "null"), index.ToString()));
        }
    }
}
