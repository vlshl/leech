using Common;
using LeechPipe;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace LeechSvc.LeechPipeClient
{
    public class TickPipe : ILpReceiver
    {
        private readonly ILpCore _core;
        private readonly ITickDispatcher _tickDisp;
        private readonly IInstrumTable _instrumTable;

        public TickPipe(ILpCore core, ITickDispatcher tickDisp, IInstrumTable instrumTable)
        {
            _core = core;
            _tickDisp = tickDisp;
            _instrumTable = instrumTable;
        }

        public void OnRecv(byte[] data)
        {
            if (data == null) return;

            string cmd = Encoding.UTF8.GetString(data);
            string[] args = Regex.Split(cmd, @"\s+");

            if (args.Length == 2 && args[0] == "GetLastPrices")
            {
                var tickers = Regex.Split(args[1], @"\s*,\s*");
                List<LastPrice> prices = new List<LastPrice>();
                foreach (var t in tickers)
                {
                    var instrum = _instrumTable.GetInstrum(t);
                    if (instrum == null) continue;

                    var tick = _tickDisp.GetLastTick(instrum.InsID);
                    if (tick.InsID == 0) continue; // диспетчер вернул default value структуры Tick, значит тика еще нет
                    prices.Add(new LastPrice(instrum.Ticker, tick.Time, tick.Price, tick.Lots));
                }

                var json = JsonConvert.SerializeObject(prices);
                var bytes = Encoding.UTF8.GetBytes(json);
                _core.SendResponseAsync(this, bytes).Wait();
            }
        }
    }
}
