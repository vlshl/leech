using BL;
using Common;
using Common.Interfaces;
using LeechSvc;
using LeechSvc.BL;
using LeechSvc.Bots;
using LeechSvc.Logger;
using LeechSvc.Storage;
using Storage;
using System;
using Unity;
using Unity.Lifetime;

namespace Leech
{
    public class Container : IContainer
    {
        private UnityContainer uc;

        public Container()
        {
            uc = new UnityContainer();
            Initialize();
        }

        public T Resolve<T>()
        {
            return uc.Resolve<T>();
        }

        public object Resolve(Type type)
        {
            return uc.Resolve(type);
        }

        public void Register<TFrom, TTo>() where TTo : TFrom
        {
            uc.RegisterType<TFrom, TTo>();
        }

        public void RegisterInstance(Type type, object instance)
        {
            uc.RegisterInstance(type, null, instance, new ContainerControlledLifetimeManager());
        }

        private void Initialize()
        {
            uc.RegisterType<ILogger, DebugLogger>(new ContainerControlledLifetimeManager()); // singleton
            uc.RegisterType<ILeechApp, LeechApp>(new ContainerControlledLifetimeManager()); // singleton
            uc.RegisterType<IBotManager, BotManager>(new ContainerControlledLifetimeManager()); // singleton
            uc.RegisterType<IBotsConfiguration, BotsConfiguration>(new ContainerControlledLifetimeManager()); // singleton
            uc.RegisterType<IRepositoryBL, RepositoryBL>(new ContainerControlledLifetimeManager()); // singleton
            uc.RegisterType<ILeechConfig, LeechConfig>(new ContainerControlledLifetimeManager()); // singleton
            uc.RegisterType<IInstrumTable, InstrumTable>(new ContainerControlledLifetimeManager()); // singleton
            uc.RegisterType<IStopOrderTable, StopOrderTable>(new ContainerControlledLifetimeManager()); // singleton
            uc.RegisterType<IOrderTable, OrderTable>(new ContainerControlledLifetimeManager()); // singleton
            uc.RegisterType<ITradeTable, TradeTable>(new ContainerControlledLifetimeManager()); // singleton
            uc.RegisterType<IHoldingTable, HoldingTable>(new ContainerControlledLifetimeManager()); // singleton
            uc.RegisterType<IPositionTable, PositionTable>(new ContainerControlledLifetimeManager()); // singleton
            uc.RegisterType<IAccountTable, AccountTable>(new ContainerControlledLifetimeManager()); // singleton
            uc.RegisterType<ITickDispatcher, TickDispatcher>(new ContainerControlledLifetimeManager()); // singleton
            uc.RegisterType<IStorage, LeechStorage>(new ContainerControlledLifetimeManager()); // singleton
            uc.RegisterType<IDataStorage, DataStorage>(new ContainerControlledLifetimeManager()); // singleton
            uc.RegisterType<IInsStoreData, InsStoreData>(new ContainerControlledLifetimeManager()); // singleton
            uc.RegisterType<IInsStoreBL, InsStoreBL>(new ContainerControlledLifetimeManager()); // singleton

            uc.RegisterType<ILeechPlatform, LeechPlatform>();
            uc.RegisterType<IInstrumDA, InstrumDA>();
            uc.RegisterType<IAccountDA, AccountDA>();
            uc.RegisterType<IInsStoreDA, InsStoreDA>();
            uc.RegisterType<IRepositoryDA, RepositoryDA>();
        }
    }
}
