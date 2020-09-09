using Common;
using LeechSvc.Logger;
using System;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace LeechSvc
{
    public partial class LeechService : ServiceBase
    {
        private readonly ILeechApp _app;
        private readonly ILogger _logger;
        private readonly DataProtect _dataProtect;

        public LeechService(ILeechApp app, ILogger logger)
        {
            InitializeComponent();

            _app = app;
            _logger = logger;
            _dataProtect = IoC.Resolve<DataProtect>();
        }

        protected override void OnStart(string[] args)
        {
            _logger.AddInfo("Service", "OnStart");
            try
            {
                _app.Initialize();
            }
            catch (Exception ex)
            {
                _logger.AddException("Service", ex);
            }
        }

        protected override void OnStop()
        {
            _logger.AddInfo("Service", "OnStop");
            _app.Close();
        }

        protected override void OnShutdown()
        {
            base.OnShutdown();
        }

        internal void RunInteractive(string[] args)
        {
            if (args.Length == 1)
            {
                string arg0 = args[0];
                if (arg0 == "-help" || arg0 == "/?")
                {
                    Usage();
                    return;
                }

                if (arg0 == "-connection")
                {
                    Console.WriteLine("Input broker server connection params");
                    SetParams(true);
                    Console.WriteLine("Broker server connection params saved.");
                    return;
                }

                if (arg0 == "-pulxer")
                {
                    Console.WriteLine("Input pulxer server connection params");
                    SetParams(false);
                    Console.WriteLine("Pulxer server connection params saved.");
                    return;
                }
            }

            Usage();
            Console.WriteLine("\nТерминальный режим работы\n");
            Console.WriteLine("opensess\tОткрыть сессию (выполняется перед началом торгов)");
            Console.WriteLine("closesess\tЗакрыть сессию (выполняется после окончания торгов)");
            Console.WriteLine("openterm\tОткрыть торговый терминал");
            Console.WriteLine("closeterm\tЗакрыть торговый терминал");
            Console.WriteLine("conn\t\tУстановить соединение с сервером брокера (торговый терминал должен быть открыт)");
            Console.WriteLine("disconn\t\tРазорвать соединение с сервером брокера");
            Console.WriteLine("exit\t\tВыход из программы");

            this.OnStart(args);

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    Console.Write(">");
                    string line = Console.ReadLine();
                    if (line == "exit") { this.OnStop(); break; }
                    if (line == "opensess") _app.OpenSession();
                    if (line == "closesess") _app.CloseSession();
                    if (line == "openterm") _app.OpenTerminal();
                    if (line == "closeterm") _app.CloseTerminal();
                    if (line == "conn") _app.Connect();
                    if (line == "disconn") _app.Disconnect();
                }
            });

            //this.OnStop();
        }

        private void SetParams(bool isBrokerServer)
        {
            Console.Write("Protection scope (M - Local machine protection, U - Current user protection): ");
            string protectionScope = Console.ReadLine();
            bool isLocalMachineProtection = protectionScope.ToUpper() == "M";
            if (isLocalMachineProtection)
                Console.WriteLine("Local machine protection");
            else
                Console.WriteLine("Current user protection");

            Console.Write("Server: ");
            string server = Console.ReadLine();

            Console.Write("Login: ");
            string login = Console.ReadLine();

            Console.Write("Password: ");
            string password = Console.ReadLine();

            if (isBrokerServer)
            {
                _dataProtect.SetBrokerParams(server, login, password, isLocalMachineProtection);
            }
            else
            {
                _dataProtect.SetPulxerParams(server, login, password, isLocalMachineProtection);
            }
        }

        private void Usage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("    LeechSvc.exe                терминальный режим работы");
            Console.WriteLine("    LeechSvc.exe -connection    установка параметров соединения с сервером брокера");
            Console.WriteLine("    LeechSvc.exe -pulxer        установка параметров соединения с сервером Pulxer");
            Console.WriteLine("    LeechSvc.exe -help          справка");
        }
    }
}
