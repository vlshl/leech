using LeechPipe;
using System.Text;

namespace LeechSvc.LeechPipeClient
{
    public class LpAppFactory : ILpFactory
    {
        private ILpCore _core;
        private IInstrumTable _instrumTable;

        public LpAppFactory(ILpCore core, IInstrumTable instrumTable)
        {
            _core = core;
            _instrumTable = instrumTable;
        }

        public ILpReceiver CreatePipe(byte[] pipeInitData)
        {
            if (pipeInitData == null) return null;

            string cmd = Encoding.UTF8.GetString(pipeInitData);

            if (cmd == "sync")
            {
                return new SyncPipe(_core, _instrumTable);
            }

            return null;
        }
    }
}
