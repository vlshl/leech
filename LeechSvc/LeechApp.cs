using Common;
using Common.Interfaces;
using Leech;
using LeechSvc.BL;
using LeechSvc.Bots;
using LeechSvc.LeechPipeClient;
using LeechSvc.Logger;
using System;
using System.IO;

namespace LeechSvc
{
    public interface ILeechApp
    {
        void Initialize();
        void Close();
        void OpenSession();
        void CloseSession();
        void OpenTerminal();
        void CloseTerminal();
        void Connect();
        void Disconnect();
    }

    /// <summary>
    /// Основной объект
    /// </summary>
    public class LeechApp : ILeechApp
    {
        private readonly ILeechConfig _config = null;
        private Scheduler _scheduler = null;
        private AlorTradeWrapper _alorTrade = null;
        private readonly IBotManager _botManager = null;
        private readonly IBotsConfiguration _botsConfig = null;
        private readonly ITickDispatcher _tickDispatcher = null;
        private readonly IDataStorage _dataStorage = null;
        private readonly IInstrumTable _instrumTable = null;
        private readonly IAccountTable _accountTable = null;
        private readonly IInsStoreData _insStoreData = null;
        private readonly IOrderTable _orderTable = null;
        private readonly ITradeTable _tradeTable = null;
        private readonly IStopOrderTable _stopOrderTable = null;
        private readonly IHoldingTable _holdingTable = null;
        private readonly ICashTable _cashTable = null;
        private AllTradesData _allTradesData = null;
        private readonly ILogger _logger = null;
        private readonly DataProtect _dataProtect = null;
        private LpClientApp _lpClientApp;
        private string _sessionDbPath = "";

        public LeechApp(ILeechConfig config, IBotManager botManager, IBotsConfiguration botsConfig, ITickDispatcher tickDisp, IDataStorage dataStorage, 
            IInstrumTable insTable, IStopOrderTable stopOrderTable, IOrderTable orderTable, ITradeTable tradeTable, 
            IHoldingTable holdingTable, ICashTable positionTable, AccountTable accountTable, IInsStoreData insStoreData, ILogger logger)
        {
            _config = config;
            _scheduler = new Scheduler(_config, this, logger);
            _botsConfig = botsConfig;
            _tickDispatcher = tickDisp;
            _botManager = botManager;
            _dataStorage = dataStorage;
            _accountTable = accountTable;
            _instrumTable = insTable;
            _orderTable = orderTable;
            _tradeTable = tradeTable;
            _stopOrderTable = stopOrderTable;
            _holdingTable = holdingTable;
            _cashTable = positionTable;
            _insStoreData = insStoreData;
            _logger = logger;
            _dataProtect = IoC.Resolve<DataProtect>();
            _lpClientApp = new LpClientApp(_dataProtect, _instrumTable, _accountTable, _stopOrderTable, _orderTable,
                _tradeTable, _cashTable, _holdingTable, _tickDispatcher, _logger);

            _allTradesData = new AllTradesData(_instrumTable, _insStoreData, _logger);
            _alorTrade = new AlorTradeWrapper(_instrumTable, _stopOrderTable, _orderTable, _tradeTable, 
                _holdingTable, _cashTable, _accountTable,
                _tickDispatcher, _config, _logger);
        }

        /// <summary>
        /// Инициализация перед началом торговой сессии.
        /// </summary>
        public void OpenSession()
        {
            _logger.AddInfo("LeechApp", "OpenSession");

            _instrumTable.Load();
            _accountTable.Load();

            // создание каталогов
            _sessionDbPath = _config.GetSessionDbPath(DateTime.Today);
            if (!Directory.Exists(_sessionDbPath))
            {
                Directory.CreateDirectory(_sessionDbPath);
            }

            _tickDispatcher.Initialize();
            _botsConfig.Load();
            _botManager.Initialize();
            _alorTrade.Initialize();
            _lpClientApp.Initialize();
            _logger.AddInfo("LeechApp", "Session opened");
        }

        /// <summary>
        /// Завершение после окончания торговой сессии.
        /// </summary>
        public void CloseSession()
        {
            _logger.AddInfo("LeechApp", "Close session ...");
            _botManager.Close();
            _alorTrade.Close();
            _allTradesData.SaveData(_tickDispatcher, _sessionDbPath);
            _tickDispatcher.Close();
            _logger.AddInfo("LeechApp", "Session closed");
        }

        public void OpenTerminal()
        {
            _logger.AddInfo("LeechApp", "OpenTerminal");
            _alorTrade.OpenTerminal();
        }

        public void CloseTerminal()
        {
            _logger.AddInfo("LeechApp", "CloseTerminal");
            _alorTrade.CloseTerminal();
        }

        public void Connect()
        {
            _logger.AddInfo("LeechApp", "Connection ...");
            string server;
            string login;
            string password;

            bool isSuccess = _dataProtect.GetBrokerParams(out server, out login, out password);
            if (!isSuccess)
            {
                _logger.AddInfo("LeechApp", "Broker connection params error");
                return;
            }

            _alorTrade.Connect(server, login, password);
            _logger.AddInfo("LeechApp", "Connected");
        }

        public void Disconnect()
        {
            _logger.AddInfo("LeechApp", "Disconnect");
            _alorTrade.Disconnect();
        }

        /// <summary>
        /// Инициализация работы службы
        /// </summary>
        public void Initialize()
        {
            _logger.AddInfo("LeechApp", "Initialize");
            _dataStorage.Initialize();

            // создание списка запланированных действий
            _scheduler.ClearAllItems();
            _scheduler.AddItem(_config.GetOpenSessionLocalTime(), () => { OpenSession(); });
            _scheduler.AddItem(_config.GetOpenTerminalLocalTime(), () => { OpenTerminal(); });
            _scheduler.AddItem(_config.GetConnectLocalTime(), () => { Connect(); });
            _scheduler.AddItem(_config.GetDisconnectLocalTime(), () => { Disconnect(); });
            _scheduler.AddItem(_config.GetCloseTerminalLocalTime(), () => { CloseTerminal(); });
            _scheduler.AddItem(_config.GetCloseSessionLocalTime(), () => { CloseSession(); });
            _scheduler.Start(); // запуск планировщика
        }

        /// <summary>
        /// Завершение работы службы
        /// </summary>
        public void Close()
        {
            _logger.AddInfo("LeechApp", "Close");
            _lpClientApp.Close();
            _scheduler.Stop();
            _dataStorage.Close();
        }
    }
}
