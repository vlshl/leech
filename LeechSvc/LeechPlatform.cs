using Common;
using Common.Data;
using LeechSvc.Logger;
using System.Collections.Generic;

namespace LeechSvc
{
    public class LeechPlatform : ILeechPlatform
    {
        private readonly ITickDispatcher _tickDisp = null;
        private readonly ILogger _logger = null;
        private readonly IInstrumTable _instrumTable = null;
        private List<BarRow> _barRows;

        public LeechPlatform(ITickDispatcher tickDisp, IInstrumTable insTable, ILogger logger)
        {
            _tickDisp = tickDisp;
            _instrumTable = insTable;
            _logger = logger;
            _barRows = new List<BarRow>();
        }

        public void AddLog(string source, string text)
        {
            _logger.AddInfo("Bot:" + source, text);
        }

        public Instrum GetInstrum(string ticker)
        {
            return _instrumTable.GetInstrum(ticker);
        }

        public BarRow CreateBarRow(int insID, Timeframes tf)
        {
            BarRow bars = new BarRow(tf, _tickDisp, insID);
            _barRows.Add(bars);

            return bars;
        }

        //public void Subscribe(int insID, OnTick onTick)
        //{
        //    _tickDisp.Subscribe(this, insID, onTick);
        //}

        public void Close()
        {
            foreach (var br in _barRows) br.CloseBarRow();
        }
    }
}
