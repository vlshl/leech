using Common;
using System;
using System.Threading.Tasks;

namespace LeechSvc
{
    public delegate void OnBarEventHandler(int index);
    public delegate void OnTimerDelegate(DateTime time, int delay);
    public delegate void OnTickDelegate(DateTime time, decimal price, int lots);

    public interface IBarRow
    {
        event OnBarEventHandler OnCloseBar;

        Timeline Dates { get; }
        Bar this[int i] { get; }
        ValueRow Open { get; }
        ValueRow Close { get; }
        ValueRow High { get; }
        ValueRow Low { get; }
        ValueRow Median { get; }
        ValueRow Typical { get; }
    }

    public interface IInstrum
    {
        int InsID { get; set; }
    }

    public interface ILeechPlatform
    {
        void AddLog(string source, string text);
        IInstrum GetInstrum(string ticker);
        Task<IBarRow> CreateBarRow(int insID, Timeframes tf, int historyDays);
        void Close();
        void OnTimer(OnTimerDelegate onTimer);
        void OnTick(int insID, OnTickDelegate onTick, bool isSubscribe);
        IPosManager GetPosManager(int insID);
    }

    public interface IPosManager
    {
        bool IsReady { get; }

        bool ClosePos();
        void ClosePosManager();
        int GetPos();
        bool OpenLong(int lots);
        bool OpenShort(int lots);
    }
}
