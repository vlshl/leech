using Common;
using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LeechSvc.BL
{
    public interface IInsStoreBL
    {
        void InsertData(int insStoreID, IEnumerable<Bar> bars, DateTime date1, DateTime date2, bool isLastDirty, CancellationToken cancel);
    }

    public class InsStoreBL : IInsStoreBL
    {
        private readonly IInsStoreDA _insStoreDA = null;

        public InsStoreBL(IInsStoreDA da)
        {
            _insStoreDA = da;
        }

        public void InsertData(int insStoreID, IEnumerable<Bar> bars, DateTime date1, DateTime date2, bool isLastDirty, CancellationToken cancel)
        {
            var calendar = GetInsStoreCalendar(insStoreID);
            if (calendar == null) return;

            calendar.AppendPeriod(new InsStorePeriod(date1, date2, isLastDirty));
            var freeDays = GetFreeDays(bars, date1, date2, isLastDirty);
            if (freeDays != null) calendar.AddFreeDays(freeDays);

            _insStoreDA.InsertBars(insStoreID, bars, date1, date2, cancel);
            if (cancel.IsCancellationRequested) return;

            _insStoreDA.UpdatePeriods(insStoreID, calendar.Periods);
            _insStoreDA.UpdateFreeDays(insStoreID, calendar.FreeDays);
        }

        private InsStoreCalendar GetInsStoreCalendar(int insStoreID)
        {
            if (_insStoreID_calendar.ContainsKey(insStoreID))
            {
                return _insStoreID_calendar[insStoreID];
            }
            else
            {
                var calendar = new InsStoreCalendar();
                var periods = _insStoreDA.GetPeriods(insStoreID);
                var freeDays = _insStoreDA.GetFreeDays(insStoreID);
                if (periods != null)
                {
                    foreach (var p in periods) calendar.AppendPeriod(p);
                }
                if (freeDays != null) calendar.AddFreeDays(freeDays);

                return calendar;
            }
        }
        private Dictionary<int, InsStoreCalendar> _insStoreID_calendar = new Dictionary<int, InsStoreCalendar>();

        /// <summary>
        /// Получить список дат из промежутка от date1 по date2, для которых нет данных,
        /// т.е. список дней когда торгов не было
        /// </summary>
        /// <param name="bars">Список баров</param>
        /// <param name="date1">Начальная дата</param>
        /// <param name="date2">Конечная дата</param>
        /// <param name="isLastDirty">Если последний день помечен как неполный, то он не может быть свободным</param>
        /// <returns>Список дат, когда не было сделок. Если в date2 сделок не было, но стоит флаг isLastDirty, то date2 не включается в список.</returns>
        private DateTime[] GetFreeDays(IEnumerable<Bar> bars, DateTime date1, DateTime date2,
            bool isLastDirty)
        {
            date1 = date1.Date; date2 = date2.Date;
            if (date1 > date2)
                throw new ArgumentException("date1 > date2");

            var tradeDays = (from b in bars select b.Time.Date).Distinct().ToList();
            List<DateTime> freeDates = new List<DateTime>();
            DateTime endDate = isLastDirty ? date2.AddDays(-1) : date2;
            for (DateTime d = date1; d <= endDate; d = d.AddDays(1))
            {
                if (tradeDays.Contains(d)) continue;
                freeDates.Add(d);
            }

            return freeDates.ToArray();
        }
    }
}
