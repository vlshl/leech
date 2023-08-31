using Common;
using LeechPipe;
using LeechSvc.Logger;
using System.Text;

namespace LeechSvc.LeechPipeClient
{
    public class LpAppFactory : ILpFactory
    {
        private readonly ILpCore _core;
        private readonly IInstrumTable _instrumTable;
        private readonly IAccountTable _accountTable;
        private readonly IStopOrderTable _stopOrderTable;
        private readonly IOrderTable _orderTable;
        private readonly ITradeTable _tradeTable;
        private readonly ICashTable _positionTable;
        private readonly IHoldingTable _holdingTable;
        private readonly ITickDispatcher _tickDisp;
        private readonly ILeechConfig _config;
        private readonly ILogger _logger;

        public LpAppFactory(ILpCore core, IInstrumTable instrumTable, IAccountTable accountTable, IStopOrderTable stopOrderTable,
            IOrderTable orderTable, ITradeTable tradeTable, ICashTable positionTable, IHoldingTable holdingTable, ITickDispatcher tickDisp, ILeechConfig config, ILogger logger)
        {
            _core = core;
            _instrumTable = instrumTable;
            _accountTable = accountTable;
            _stopOrderTable = stopOrderTable;
            _orderTable = orderTable;
            _tradeTable = tradeTable;
            _positionTable = positionTable;
            _holdingTable = holdingTable;
            _tickDisp = tickDisp;
            _config = config;
            _logger = logger;
        }

        public ILpReceiver CreatePipe(byte[] pipeInitData)
        {
            if (pipeInitData == null) return null;

            string cmd = Encoding.UTF8.GetString(pipeInitData);

            if (cmd == "sync")
            {
                return new SyncPipe(_core, _instrumTable, _accountTable, _stopOrderTable, _orderTable, _tradeTable, _positionTable, _holdingTable);
            }
            else if (cmd == "tick")
            {
                return new TickPipe(_core, _tickDisp, _instrumTable);
            }
            else if (cmd == "tick-history")
            {
                return new TickHistoryPipe(_core, _config, _logger);
            }

            return null;
        }
    }
}
