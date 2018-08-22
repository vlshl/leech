using System.Collections.Generic;
using CommonData = Common.Data;

namespace Common.Interfaces
{
    /// <summary>
    /// Instrum da-layer interface
    /// </summary>
    public interface IInstrumDA
    {
        CommonData.Instrum GetInstrum(int insID, string ticker = null);
        int InsertInstrum(CommonData.Instrum ins);
        void UpdateInstrum(CommonData.Instrum ins);
        void DeleteInstrumByID(int insID);
        IEnumerable<CommonData.Instrum> GetInstrumList();
    }
}
