using Common;
using Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeechSvc
{
    public interface ILeechPlatform
    {
        void AddLog(string source, string text);
        Instrum GetInstrum(string ticker);
        //void Subscribe(int insID, OnTick onTick);
        BarRow CreateBarRow(int insID, Timeframes tf);
        void Close();
    }

    //public delegate void OnTick(DateTime time, double price, int lots);
}
