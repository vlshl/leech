using LeechPipe;
using Newtonsoft.Json;
using System.Collections.Generic;
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
        private readonly ICashTable _cashTable;
        private readonly IHoldingTable _holdingTable;

        public SyncPipe(ILpCore core, IInstrumTable instrumTable, IAccountTable accountTable, IStopOrderTable stopOrderTable, 
            IOrderTable orderTable, ITradeTable tradeTable, ICashTable positionTable, IHoldingTable holdingTable)
        {
            _core = core;
            _instrumTable = instrumTable;
            _accountTable = accountTable;
            _stopOrderTable = stopOrderTable;
            _orderTable = orderTable;
            _tradeTable = tradeTable;
            _cashTable = positionTable;
            _holdingTable = holdingTable;
        }

        public void OnRecv(byte[] data)
        {
            if (data == null) return;

            string str;
            int[] ids;

            try
            {
                str = Encoding.UTF8.GetString(data);
            }
            catch
            {
                return;
            }

            string cmd = CmdParse(str, out ids);
            if (string.IsNullOrEmpty(cmd)) return;

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
            else if (cmd == "GetStopOrders")
            {
                if (ids.Length == 2)
                {
                    var list = _stopOrderTable.GetStopOrders(ids[0], ids[1]);
                    var json = JsonConvert.SerializeObject(list);
                    var bytes = Encoding.UTF8.GetBytes(json);
                    _core.SendResponseAsync(this, bytes).Wait();
                }
                else
                {
                    _core.SendResponseAsync(this, null).Wait();
                }
            }
            else if (cmd == "GetStopOrdersByIds")
            {
                if (ids.Length > 0)
                {
                    var list = _stopOrderTable.GetStopOrdersByIds(ids);
                    var json = JsonConvert.SerializeObject(list);
                    var bytes = Encoding.UTF8.GetBytes(json);
                    _core.SendResponseAsync(this, bytes).Wait();
                }
                else
                {
                    _core.SendResponseAsync(this, null).Wait();
                }
            }
            else if (cmd == "GetOrders")
            {
                if (ids.Length == 2)
                {
                    var list = _orderTable.GetOrders(ids[0], ids[1]);
                    var json = JsonConvert.SerializeObject(list);
                    var bytes = Encoding.UTF8.GetBytes(json);
                    _core.SendResponseAsync(this, bytes).Wait();
                }
                else
                {
                    _core.SendResponseAsync(this, null).Wait();
                }
            }
            else if (cmd == "GetOrdersByIds")
            {
                if (ids.Length > 0)
                {
                    var list = _orderTable.GetOrdersByIds(ids);
                    var json = JsonConvert.SerializeObject(list);
                    var bytes = Encoding.UTF8.GetBytes(json);
                    _core.SendResponseAsync(this, bytes).Wait();
                }
                else
                {
                    _core.SendResponseAsync(this, null).Wait();
                }
            }
            else if (cmd == "GetTrades") // GetTrades accountId fromId
            {
                if (ids.Length == 2)
                {
                    var list = _tradeTable.GetTrades(ids[0], ids[1]);
                    var json = JsonConvert.SerializeObject(list);
                    var bytes = Encoding.UTF8.GetBytes(json);
                    _core.SendResponseAsync(this, bytes).Wait();
                }
                else
                {
                    _core.SendResponseAsync(this, null).Wait();
                }
            }
            else if (cmd == "GetCash")
            {
                if (ids.Length == 1)
                {
                    var cash = _cashTable.GetCash(ids[0]);
                    var json = JsonConvert.SerializeObject(cash);
                    var bytes = Encoding.UTF8.GetBytes(json);
                    _core.SendResponseAsync(this, bytes).Wait();
                }
                else
                {
                    _core.SendResponseAsync(this, null).Wait();
                }
            }
            else if (cmd == "GetHoldingList")
            {
                if (ids.Length == 1)
                {
                    var list = _holdingTable.GetHoldings(ids[0]);
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

        private string CmdParse(string str, out int[] ids)
        {
            ids = new int[] { };
            if (string.IsNullOrWhiteSpace(str)) return "";

            string[] parts = Regex.Split(str, @"\s+");
            if (parts.Length < 1) return "";
            if (parts.Length == 1) return parts[0];

            List<int> idList = new List<int>();
            int id;
            for (int i = 1; i < parts.Length; i++)
            {
                if (!int.TryParse(parts[i], out id)) continue;
                idList.Add(id);
            }
            ids = idList.ToArray();

            return parts[0];
        }
    }
}
