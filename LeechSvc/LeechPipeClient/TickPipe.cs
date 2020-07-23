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

        public TickPipe(ILpCore core, ITickDispatcher tickDisp)
        {
            _core = core;
            _tickDisp = tickDisp;
        }

        public void OnRecv(byte[] data)
        {
            if (data == null) return;

            string cmd = Encoding.UTF8.GetString(data);
            string[] args = Regex.Split(cmd, @"\s+");

            if (args.Length == 2 && args[0] == "GetLastTicks")
            {
                var parts = Regex.Split(args[1], @"\s*,\s*");
                List<Tick> ticks = new List<Tick>();
                int id;
                foreach (var p in parts)
                {
                    if (!int.TryParse(p, out id)) continue;
                    var tick = _tickDisp.GetLastTick(id);
                    if (tick.InsID == 0) continue; // диспетчер вернул default value структуры Tick, значит тика еще нет
                    ticks.Add(tick);
                }

                var json = JsonConvert.SerializeObject(ticks);
                var bytes = Encoding.UTF8.GetBytes(json);
                _core.SendResponseAsync(this, bytes).Wait();
            }
        }
    }
}
