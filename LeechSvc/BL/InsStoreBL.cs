using Common;
using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommonData = Common.Data;

namespace LeechSvc.BL
{
    public interface IInsStoreBL
    {
        void InsertData(int insStoreID, IEnumerable<Bar> bars, DateTime date1, DateTime date2, bool isLastDirty, CancellationToken cancel);
        Task<int> LoadHistoryAsync(BarRow bars, int insID, DateTime date1, DateTime date2, int? insStoreID = null);
        void DeleteOldBars(int days);
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

        public async Task<int> LoadHistoryAsync(BarRow bars, int insID, DateTime date1, DateTime date2, int? insStoreID = null)
        {
            if (insStoreID != null)
            {
                await _insStoreDA.LoadHistoryAsync(bars, insStoreID.Value, date1, date2);
                return bars.Count;
            }
            else
            {
                var ss = GetLoadHistoryInsStore(insID, bars.Timeframe);
                if (ss == null) return 0;

                await _insStoreDA.LoadHistoryAsync(bars, ss.InsStoreID, date1, date2);
                return bars.Count;
            }
        }

        /// <summary>
        /// Получить наиболее подходящий InsStore
        /// </summary>
        /// <param name="insId"></param>
        /// <param name="tf"></param>
        /// <returns></returns>
        private CommonData.InsStore GetLoadHistoryInsStore(int insId, Timeframes tf)
        {
            CommonData.InsStore ss = null;
            var insStores = _insStoreDA.GetInsStores(insId);

            if (tf == Timeframes.Tick)
            {
                ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Tick);
            }

            if (tf == Timeframes.Min)
            {
                ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Tick);
            }

            if (tf == Timeframes.Min5)
            {
                ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min5);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Tick);
            }

            if (tf == Timeframes.Min10)
            {
                ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min10);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min5);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Tick);
            }

            if (tf == Timeframes.Min15)
            {
                ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min15);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min5);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Tick);
            }

            if (tf == Timeframes.Min20)
            {
                ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min20);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min10);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min5);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Tick);
            }

            if (tf == Timeframes.Min30)
            {
                ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min30);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min15);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min10);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min5);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Tick);
            }

            if (tf == Timeframes.Hour)
            {
                ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Hour);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min30);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min20);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min15);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min10);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min5);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Tick);
            }

            if (tf == Timeframes.Day)
            {
                ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Day);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Hour);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min30);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min20);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min15);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min10);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min5);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Tick);
            }

            if (tf == Timeframes.Week)
            {
                ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Week);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Day);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Hour);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min30);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min20);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min15);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min10);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min5);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Tick);
            }

            return ss;
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

        /// <summary>
        /// Удаление старых данных (баров) по всем потокам (insStore)
        /// </summary>
        /// <param name="days">Удаляются все бары по всем стримам старше указанного количества дней от текущей даты (today) на момент выполнения операции</param>
        public void DeleteOldBars(int days)
        {
            if (days <= 0) return;

            var beforeDate = DateTime.Today.AddDays(-days).Date;
            _insStoreDA.DeleteOldBars(beforeDate, new CancellationToken());
        }
    }
}
