﻿using Common;
using Common.Data;
using LeechPipe;
using System;
using System.Text;
using System.Threading.Tasks;

namespace LeechSvc.LeechPipeClient
{
    public class LpClientApp
    {
        private const int PERIOD = 60 * 1000;
        private LpClientSocket _socket;
        private LpCore _core;
        private SystemLp _sysPipe;
        private ILpFactory _pipeFactory;
        private bool _isWorking = true;
        private ILeechConfig _config;
        private DataProtect _dataProtect;

        public LpClientApp(ILeechConfig config, DataProtect dataProtect, IInstrumTable instrumTable, 
            IAccountTable accountTable, IStopOrderTable stopOrderTable, IOrderTable orderTable, ITradeTable tradeTable, 
            IPositionTable positionTable, IHoldingTable holdingTable, ITickDispatcher tickDisp)
        {
            _config = config;
            _dataProtect = dataProtect;
            _socket = new LpClientSocket();
            _core = new LpCore(_socket, false); // клиент
            _pipeFactory = new LpAppFactory(_core, instrumTable, accountTable, stopOrderTable, orderTable, tradeTable, positionTable, holdingTable, tickDisp);
            _sysPipe = new SystemLp(_pipeFactory, _core);
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
            _dataProtect.GetPulxerParams(out url, out login, out password);

            bool isSuccess = await _socket.ConnectAsync(url, login, password);
            if (!isSuccess) return false;

            _core.Initialize(_sysPipe, "Leech App");
            return true;
        }

        public async Task<bool> CloseAsync()
        {
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
