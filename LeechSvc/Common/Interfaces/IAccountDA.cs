using System;
using System.Collections.Generic;
using CommonData = Common.Data;

namespace Common.Interfaces
{
    /// <summary>
    /// Account da-layer interface
    /// </summary>
    public interface IAccountDA
    {
        CommonData.Account GetAccountByID(int accountID);
        IEnumerable<CommonData.Account> GetAccounts();
        IEnumerable<CommonData.Cash> GetCashes(int accountID);
        IEnumerable<CommonData.Cashflow> GetCashflows(int accountID);
        IEnumerable<CommonData.Holding> GetHoldings(int accountID);
        IEnumerable<CommonData.Order> GetOrders(int accountID, bool isActiveOnly, int idFrom);
        IEnumerable<CommonData.Order> GetOrdersByIds(int[] ids);
        CommonData.Order GetOrder(long orderNo);
        IEnumerable<CommonData.StopOrder> GetStopOrders(int accountID, bool isActiveOnly, int idFrom);
        IEnumerable<CommonData.StopOrder> GetStopOrdersByIds(int[] ids);
        CommonData.StopOrder GetStopOrder(long stopOrderNo);
        IEnumerable<CommonData.Trade> GetTrades(int accountID, DateTime? dateFrom, int idFrom);
        int InsertAccount(CommonData.Account account);
        void UpdateAccount(CommonData.Account account);
        int InsertCash(CommonData.Cash cash);
        void UpdateCash(CommonData.Cash cash);
        int InsertCashflow(CommonData.Cashflow cashflow);
        int InsertHolding(CommonData.Holding holding);
        void UpdateHolding(CommonData.Holding holding);
        void DeleteHolding(int holdingID);
        int InsertOrder(CommonData.Order order);
        void UpdateOrder(CommonData.Order order);
        int InsertStopOrder(CommonData.StopOrder stopOrder);
        void UpdateStopOrder(CommonData.StopOrder stopOrder);
        int InsertTrade(CommonData.Trade trade);
        void DeleteAccountData(int accountID);
    }  
}
