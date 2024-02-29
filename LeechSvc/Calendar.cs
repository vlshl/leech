using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace LeechSvc
{
    public class Calendar
    {
        private readonly ILeechConfig _config;
        private List<DayOfWeek> _weekends;
        private List<DateTime> _holidays;
        private List<DateTime> _workdays;

        public Calendar(ILeechConfig config)
        {
            _config = config;
            _weekends = new List<DayOfWeek>();
            _holidays = new List<DateTime>();
            _workdays = new List<DateTime>();
        }

        /// <summary>
        /// Инициализация календаря, загрузка информации из конфигурационного файла
        /// Параметры в конфиге:
        /// Weekends - список через запятую выходных дней недели в виде Mon, Tue, Wed, Thu, Fri, Sat, Sun (регистр игнорируется)
        /// Holidays - список праздников (через запятую без года в формате dd/mm, т.е. такой список может использоваться для разных годов)
        /// Workdays - список рабочих дней (формат как у Holidays) 
        /// </summary>
        public void Initialize()
        {
            _weekends.Clear();
            var ss1 = Regex.Split(_config.Weekends, @"\s*,\s*");
            foreach (var s in ss1)
            {
                var sl = s.ToLower();
                if (sl == "mon") _weekends.Add(DayOfWeek.Monday);
                else if (sl == "tue") _weekends.Add(DayOfWeek.Tuesday);
                else if (sl == "wed") _weekends.Add(DayOfWeek.Wednesday);
                else if (sl == "thu") _weekends.Add(DayOfWeek.Thursday);
                else if (sl == "fri") _weekends.Add(DayOfWeek.Friday);
                else if (sl == "sat") _weekends.Add(DayOfWeek.Saturday);
                else if (sl == "sun") _weekends.Add(DayOfWeek.Sunday);
            }

            _holidays.Clear();
            var ss2 = Regex.Split(_config.Holidays, @"\s*,\s*");
            foreach (var s in ss2)
            {
                var dms = s.Split('/');
                if (dms.Length < 2) continue;

                int d, m;
                if (!int.TryParse(dms[0], out d)) continue;
                if (!int.TryParse(dms[1], out m)) continue;

                var dt = new DateTime(2000, m, d); // год фиктивный, но должен быть високосный, чтобы дата 29.02 была корректной для этого года
                _holidays.Add(dt);
            }

            _workdays.Clear();
            var ss3 = Regex.Split(_config.Workdays, @"\s*,\s*");
            foreach (var s in ss3)
            {
                var dms = s.Split('/');
                if (dms.Length < 2) continue;

                int d, m;
                if (!int.TryParse(dms[0], out d)) continue;
                if (!int.TryParse(dms[1], out m)) continue;

                var dt = new DateTime(2000, m, d);
                _workdays.Add(dt);
            }
        }

        /// <summary>
        /// Является ли указанное локальное время рабочим.
        /// Локальное время сначала переводится в MSK, потом выполняются проверки.
        /// День считается рабочим, если он не попадает на weekends, не попадает на holidays и явно не указан в workdays
        /// Если дата перечислена в workdays, она является рабочей независиво от того, попадает ли она на список weekends или holidays.
        /// </summary>
        /// <param name="time">Время по локальному часовому поясу</param>
        /// <returns>true-рабочее время, иначе - выходной/праздник</returns>
        public bool IsWorkLocalTime(DateTime time)
        {
            var mskTime = time.AddHours(_config.CorrectHours);

            bool isWorkTime = true;
            if (_weekends.Contains(mskTime.DayOfWeek)) isWorkTime = false;
            
            DateTime d = new DateTime(2000, mskTime.Month, mskTime.Day);
            if (_holidays.Contains(d)) isWorkTime = false;
            if (_workdays.Contains(d)) isWorkTime = true;

            return isWorkTime;
        }
    }
}
