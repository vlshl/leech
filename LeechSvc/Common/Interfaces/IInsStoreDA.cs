using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CommonData = Common.Data;

namespace Common.Interfaces
{
    /// <summary>
    /// InsStore da-layer interface
    /// </summary>
    public interface IInsStoreDA
    {
        int CreateInsStore(int insID, Timeframes tf, bool isEnable);
        CommonData.InsStore GetInsStoreByID(int insStoreID);
        CommonData.InsStore GetInsStore(int insID, Timeframes tf);
        void InsertBars(int insStoreID, IEnumerable<Bar> bars, DateTime date1, DateTime date2, CancellationToken cancel);
        void UpdateInsStore(CommonData.InsStore insStore);
        IEnumerable<CommonData.InsStore> GetActiveInsStores();
        IEnumerable<CommonData.InsStore> GetInsStores(int insID); 
        Task LoadHistoryAsync(BarRow bars, int insStoreID, DateTime date1, DateTime date2);
        IEnumerable<InsStorePeriod> GetPeriods(int insStoreID);
        void UpdatePeriods(int insStoreID, IEnumerable<Common.InsStorePeriod> periods);
        IEnumerable<DateTime> GetFreeDays(int insStoreID);
        void UpdateFreeDays(int insStoreID, IEnumerable<DateTime> freeDays);
        void DeleteOldBars(DateTime beforeDate, CancellationToken cancel);
    }
}
