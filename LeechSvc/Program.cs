using Common;
using Leech;
using LeechSvc.Logger;
using System;
using System.ServiceProcess;

namespace LeechSvc
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            IoC.Container = new Container();

            try
            {
                if (Environment.UserInteractive)
                {
                    LeechService svc = IoC.Resolve<LeechService>();
                    svc.RunInteractive(args);
                }
                else
                {
                    ServiceBase[] ServicesToRun;
                    ServicesToRun = new ServiceBase[]
                    {
                        IoC.Resolve<LeechService>()
                    };
                    ServiceBase.Run(ServicesToRun);
                }
            }
            catch (Exception ex)
            {
                IoC.Resolve<ILogger>().AddException("Program.Main", ex);
            }
        }
    }
}
