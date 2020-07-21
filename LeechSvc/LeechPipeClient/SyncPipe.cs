using LeechPipe;
using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace LeechSvc.LeechPipeClient
{
    public class SyncPipe : ILpReceiver
    {
        private readonly ILpCore _core;
        private readonly IInstrumTable _instrumTable;
        private readonly IAccountTable _accountTable;
        private readonly IStopOrderTable _stopOrderTable;
        private readonly IOrderTable _orderTable;
        private readonly ITradeTable _tradeTable;
        private readonly IPositionTable _positionTable;
        private readonly IHoldingTable _holdingTable;

        public SyncPipe(ILpCore core, IInstrumTable instrumTable, IAccountTable accountTable, IStopOrderTable stopOrderTable, 
            IOrderTable orderTable, ITradeTable tradeTable, IPositionTable positionTable, IHoldingTable holdingTable)
        {
            _core = core;
            _instrumTable = instrumTable;
            _accountTable = accountTable;
            _stopOrderTable = stopOrderTable;
            _orderTable = orderTable;
            _tradeTable = tradeTable;
            _positionTable = positionTable;
            _holdingTable = holdingTable;
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
            else if (cmd == "GetAccountList")
            {
                var accounts = _accountTable.GetAccounts();
                var json = JsonConvert.SerializeObject(accounts);
                var bytes = Encoding.UTF8.GetBytes(json);
                _core.SendResponseAsync(this, bytes).Wait();
            }
            else if (cmd.StartsWith("GetStopOrderList"))
            {
                int id = GetParam(cmd);
                if (id > 0)
                {
                    var list = _stopOrderTable.GetStopOrders(id);
                    var json = JsonConvert.SerializeObject(list);
                    var bytes = Encoding.UTF8.GetBytes(json);
                    _core.SendResponseAsync(this, bytes).Wait();
                }
                else
                {
                    _core.SendResponseAsync(this, null).Wait();
                }
            }
            else if (cmd.StartsWith("GetOrderList"))
            {
                int id = GetParam(cmd);
                if (id > 0)
                {
                    var list = _orderTable.GetOrders(id);
                    var json = JsonConvert.SerializeObject(list);
                    var bytes = Encoding.UTF8.GetBytes(json);
                    _core.SendResponseAsync(this, bytes).Wait();
                }
                else
                {
                    _core.SendResponseAsync(this, null).Wait();
                }
            }
            else if (cmd.StartsWith("GetTradeList"))
            {
                int id = GetParam(cmd);
                if (id > 0)
                {
                    var list = _tradeTable.GetTrades(id);
                    var json = JsonConvert.SerializeObject(list);
                    var bytes = Encoding.UTF8.GetBytes(json);
                    _core.SendResponseAsync(this, bytes).Wait();
                }
                else
                {
                    _core.SendResponseAsync(this, null).Wait();
                }
            }
            else if (cmd.StartsWith("GetCash"))
            {
                int id = GetParam(cmd);
                if (id > 0)
                {
                    var cash = _positionTable.GetCash(id);
                    var json = JsonConvert.SerializeObject(cash);
                    var bytes = Encoding.UTF8.GetBytes(json);
                    _core.SendResponseAsync(this, bytes).Wait();
                }
                else
                {
                    _core.SendResponseAsync(this, null).Wait();
                }
            }
            else if (cmd.StartsWith("GetHoldingList"))
            {
                int id = GetParam(cmd);
                if (id > 0)
                {
                    var list = _holdingTable.GetHoldings(id);
                    var json = JsonConvert.SerializeObject(list);
                    var bytes = Encoding.UTF8.GetBytes(json);
                    _core.SendResponseAsync(this, bytes).Wait();
                }
                else
                {
                    _core.SendResponseAsync(this, null).Wait();
                }
            }
            else
            {
                _core.SendResponseAsync(this, null).Wait();
            }
        }

        private int GetParam(string cmd)
        {
            string[] parts = Regex.Split(cmd, @"\s+");
            if (parts.Length < 2) return 0;
            int id;
            if (!int.TryParse(parts[1], out id)) return 0;
            
            return id;
        }
    }
}
