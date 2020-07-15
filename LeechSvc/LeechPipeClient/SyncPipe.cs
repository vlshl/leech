using LeechPipe;
using Newtonsoft.Json;
using System.Text;

namespace LeechSvc.LeechPipeClient
{
    public class SyncPipe : ILpReceiver
    {
        private ILpCore _core;
        private IInstrumTable _instrumTable;

        public SyncPipe(ILpCore core, IInstrumTable instrumTable)
        {
            _core = core;
            _instrumTable = instrumTable;
        }

        public void OnRecv(byte[] data)
        {
            if (data == null) return;

            string cmd = Encoding.UTF8.GetString(data);

            if (cmd == "GetInstrumList")
            {
                var instrums = _instrumTable.GetInstrums();
                var json = JsonConvert.SerializeObject(instrums);
                var bytes = Encoding.UTF8.GetBytes(json);
                _core.SendResponseAsync(this, bytes).Wait();
            }
            else
            {
                _core.SendResponseAsync(this, null).Wait();
            }
        }
    }
}
