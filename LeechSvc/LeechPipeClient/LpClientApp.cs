using Common;
using Common.Data;
using LeechPipe;
using LeechSvc.Logger;
using System;
using System.Text;
using System.Threading.Tasks;

namespace LeechSvc.LeechPipeClient
{
    public class LpClientApp
    {
        private const int PERIOD = 10 * 1000;
        private LpClientSocket _socket;
        private LpCore _core;
        private SystemLp _sysPipe;
        private ILpFactory _pipeFactory;
        private bool _isWorking = true;
        private DataProtect _dataProtect;
        private ILogger _logger;
        private readonly ILeechConfig _config;

        public LpClientApp(DataProtect dataProtect, IInstrumTable instrumTable, 
            IAccountTable accountTable, IStopOrderTable stopOrderTable, IOrderTable orderTable, ITradeTable tradeTable, 
            ICashTable positionTable, IHoldingTable holdingTable, ITickDispatcher tickDisp, ILeechConfig config, ILogger logger)
        {
            _dataProtect = dataProtect;
            _socket = new LpClientSocket(logger);
            _core = new LpCore(_socket, false); // клиент
            _pipeFactory = new LpAppFactory(_core, instrumTable, accountTable, stopOrderTable, orderTable, tradeTable, positionTable, holdingTable, tickDisp, config, logger);
            _sysPipe = new SystemLp(_pipeFactory, _core);
            _logger = logger;
            _config = config;
        }

        public void Initialize()
        {
            Task.Run(async () => 
            {
                while (_isWorking)
                {
                    if (!_core.IsWorking)
                    {
                        await ConnectAsync();
                    }
                    await Task.Delay(PERIOD);
                }
            });
        }

        public void Close()
        {
            _isWorking = false;
            CloseAsync().Wait();
        }

        private async Task<bool> ConnectAsync()
        {
            string url; string login; string password;
            bool isSuccess = _dataProtect.GetPulxerParams(out url, out login, out password);
            if (!isSuccess)
            {
                _logger.AddError("LpClientApp", "Get connection params error");
                return false;
            }

            var res = await _socket.ConnectAsync(url, login, password);
            if (!res.IsSuccess)
            {
                _logger.AddError("LpClientApp", "Pulxer connection error: " + res.Message);
                return false;
            }

            _core.Initialize(_sysPipe, "Leech Agent");
            _logger.AddInfo("LpClientApp", "Pulxer connection successful.");

            return true;
        }

        public async Task<bool> CloseAsync()
        {
            _logger.AddInfo("LpClientApp", "Close");
            _core.Close();
            return await _socket.CloseAsync();
        }

        public void Cancel()
        {
            _socket.Cancel();
        }

        public async Task<string> GetRemoteIdentity()
        {
            return await _sysPipe.GetRemoteIdentity();
        }
    }
}
