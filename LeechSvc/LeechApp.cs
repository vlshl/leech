using Common;
using Common.Interfaces;
using Leech;
using LeechSvc.BL;
using LeechSvc.Bots;
using LeechSvc.LeechPipeClient;
using LeechSvc.Logger;
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
        private readonly IPositionTable _positionTable = null;
        private AllTradesData _allTradesData = null;
        private readonly ILogger _logger = null;
        private readonly DataProtect _dataProtect = null;
        private LpClientApp _lpClientApp;

        public LeechApp(ILeechConfig config, IBotManager botManager, IBotsConfiguration botsConfig, ITickDispatcher tickDisp, IDataStorage dataStorage, 
            IInstrumTable insTable, IStopOrderTable stopOrderTable, IOrderTable orderTable, ITradeTable tradeTable, 
            IHoldingTable holdingTable, IPositionTable positionTable, AccountTable accountTable, IInsStoreData insStoreData, ILogger logger)
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
            _positionTable = positionTable;
            _insStoreData = insStoreData;
            _logger = logger;
            _dataProtect = IoC.Resolve<DataProtect>();
            _lpClientApp = new LpClientApp(_config, _dataProtect, _instrumTable, _accountTable, _stopOrderTable, _orderTable,
                _tradeTable, _positionTable, _holdingTable, _tickDispatcher);

            _allTradesData = new AllTradesData(_instrumTable, _config, _insStoreData, _logger);
            _alorTrade = new AlorTradeWrapper(_instrumTable, _stopOrderTable, _orderTable, _tradeTable, 
                _holdingTable, _positionTable, _accountTable,
                _tickDispatcher, _config, _logger);

            //connBroker = new ConnBroker();
        }

        /// <summary>
        /// Инициализация перед началом торговой сессии.
        /// </summary>
        public void OpenSession()
        {
            _logger.AddInfo("LeechApp", "OpenSession");

            _instrumTable.Load();
            _accountTable.Load();
            _orderTable.Load();
            _stopOrderTable.Load();
            _holdingTable.Load();
            _positionTable.Load();

            // создание каталогов
            if (!Directory.Exists(_config.GetTodayDbPath()))
            {
                Directory.CreateDirectory(_config.GetTodayDbPath());
            }

            _tickDispatcher.Initialize();
            //_insStoreBL.OpenSession();
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
            _allTradesData.SaveData(_tickDispatcher);
            _tickDispatcher.Close();
            //_insStoreBL.CloseSession();
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
            _scheduler.AddItem(_config.GetOpenSessionTime(), () => { OpenSession(); });
            _scheduler.AddItem(_config.GetOpenTerminalTime(), () => { OpenTerminal(); });
            _scheduler.AddItem(_config.GetConnectTime(), () => { Connect(); });
            _scheduler.AddItem(_config.GetDisconnectTime(), () => { Disconnect(); });
            _scheduler.AddItem(_config.GetCloseTerminalTime(), () => { CloseTerminal(); });
            _scheduler.AddItem(_config.GetCloseSessionTime(), () => { CloseSession(); });
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
