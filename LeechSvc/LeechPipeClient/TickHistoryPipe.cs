using Common;
using LeechPipe;
using LeechSvc.Logger;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LeechSvc.LeechPipeClient
{
    public class TickHistoryPipe : ILpReceiver
    {
        private readonly ILpCore _core;
        private readonly ILeechConfig _config;
        private readonly ILogger _logger;

        public TickHistoryPipe(ILpCore core, ILeechConfig config, ILogger logger)
        {
            _core = core;
            _config = config;
            _logger = logger;
        }

        public void OnRecv(byte[] data)
        {
            if (data == null) return;

            try
            {
                string cmd = Encoding.UTF8.GetString(data);
                string[] args = Regex.Split(cmd, @"\s+");

                if (args.Length == 2 && args[0] == "GetDates") // GetDates <yyyy>
                {
                    int year;
                    if (!int.TryParse(args[1], out year))
                    {
                        _core.SendResponseAsync(this, new byte[] { 0xfe }).Wait();
                        return;
                    }
                    string yearS = year.ToString();

                    var dbPath = _config.GetDbPath();
                    var dbDateDirs = Directory.EnumerateDirectories(dbPath);
                    List<string> list = new List<string>();
                    foreach (var dbDateDir in dbDateDirs)
                    {
                        string date = new DirectoryInfo(dbDateDir).Name;
                        if (!date.StartsWith(yearS + "-")) continue;
                        string allTradesPath = Path.Combine(dbDateDir, "AllTrades");
                        if (!Directory.Exists(allTradesPath)) continue;
                        if (!Directory.EnumerateFiles(allTradesPath).Any()) continue;

                        list.Add(date);
                    }

                    var json = JsonConvert.SerializeObject(list);
                    var bytes = Encoding.UTF8.GetBytes(json);
                    _core.SendResponseAsync(this, bytes).Wait();
                }

                if (args.Length == 2 && args[0] == "GetTickers") // GetTickers <yyyy-MM-dd>
                {
                    var dbPath = _config.GetDbPath();
                    string tickersPath = Path.Combine(dbPath, args[1], "AllTrades");
                    if (Directory.Exists(tickersPath))
                    {
                        var tickers = Directory.GetFiles(tickersPath).Select(f => Path.GetFileNameWithoutExtension(f)).ToArray();
                        var json = JsonConvert.SerializeObject(tickers);
                        var bytes = Encoding.UTF8.GetBytes(json);
                        _core.SendResponseAsync(this, bytes).Wait();
                    }
                    else
                    {
                        var json = JsonConvert.SerializeObject(new string[0]);
                        var bytes = Encoding.UTF8.GetBytes(json);
                        _core.SendResponseAsync(this, bytes).Wait();
                    }
                }

                if (args.Length == 3 && args[0] == "GetData") // GetData <yyyy-MM-dd> <ticker>
                {
                    var date = args[1];
                    var ticker = args[2];
                    var dbPath = _config.GetDbPath();
                    string file = Path.Combine(dbPath, date, "AllTrades", ticker);
                    if (File.Exists(file))
                    {
                        var bytes = File.ReadAllBytes(file);
                        _core.SendResponseAsync(this, bytes).Wait();
                    }
                    else
                    {
                        _core.SendResponseAsync(this, new byte[0]).Wait();
                    }
                }
            }
            catch (Exception ex)
            {
                _core.SendResponseAsync(this, new byte[] { 0xff }).Wait();
                _logger?.AddException("TickHistoryPipe", ex);
            }
        }
    }
}
