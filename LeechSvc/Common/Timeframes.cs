﻿using System;
using System.Collections.Generic;

namespace Common
{
    public enum Timeframes
    {
        Tick = 0,
        Min = 1, Min5 = 2, Min10 = 3, Min15 = 4, Min20 = 5, Min30 = 6,
        Hour = 7, Day = 8, Week = 9
    }

    public static class TfHelper
    {
        /// <summary>
        /// Get timeframe short and full name pair
        /// </summary>
        /// <param name="tf">Timeframe</param>
        /// <returns>[shortName, Name]</returns>
        public static string[] GetTimeframeNames(Timeframes tf)
        {
            string shortName = ""; string name = "";
            switch (tf)
            {
                case Timeframes.Tick:
                    shortName = "0"; name = "Tick"; break;
                case Timeframes.Min:
                    shortName = "1"; name = "Minute"; break;
                case Timeframes.Min5:
                    shortName = "5"; name = "5 minutes"; break;
                case Timeframes.Min10:
                    shortName = "10"; name = "10 minutes"; break;
                case Timeframes.Min15:
                    shortName = "15"; name = "15 minutes"; break;
                case Timeframes.Min20:
                    shortName = "20"; name = "20 minutes"; break;
                case Timeframes.Min30:
                    shortName = "30"; name = "30 minutes"; break;
                case Timeframes.Hour:
                    shortName = "H"; name = "Hour"; break;
                case Timeframes.Day:
                    shortName = "D"; name = "Day"; break;
                case Timeframes.Week:
                    shortName = "W"; name = "Week"; break;
            }

            return new string[] { shortName, name };
        }

        /// <summary>
        /// Get timeframe short name (0, 1, 5, 10, 15, 20, 30, H, D, W)
        /// </summary>
        /// <param name="tf"></param>
        /// <returns></returns>
        public static string GetTimeframeShortName(Timeframes tf)
        {
            return GetTimeframeNames(tf)[0];
        }

        /// <summary>
        /// Get timeframe full name
        /// </summary>
        /// <param name="tf"></param>
        /// <returns></returns>
        public static string GetTimeframeName(Timeframes tf)
        {
            return GetTimeframeNames(tf)[1];
        }

        /// <summary>
        /// Get time period for date and timeframe
        /// </summary>
        /// <param name="date">Date</param>
        /// <param name="tf">Timeframe</param>
        /// <returns>Begin time of interval, begin time of the next interval</returns>
        public static DateTime[] GetDates(DateTime date, Timeframes tf)
        {
            DateTime d1 = DateTime.MinValue; DateTime d2 = DateTime.MinValue;

            switch (tf)
            {
                case Timeframes.Tick:
                    d1 = date; d2 = date;
                    break;
                case Timeframes.Min:
                    d1 = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, 0);
                    d2 = d1.AddMinutes(1);
                    break;
                case Timeframes.Min5:
                    d1 = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, 0);
                    d2 = d1.AddMinutes(5);
                    break;
                case Timeframes.Min10:
                    d1 = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, 0);
                    d2 = d1.AddMinutes(10);
                    break;
                case Timeframes.Min15:
                    d1 = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, 0);
                    d2 = d1.AddMinutes(15);
                    break;
                case Timeframes.Min20:
                    d1 = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, 0);
                    d2 = d1.AddMinutes(20);
                    break;
                case Timeframes.Min30:
                    d1 = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, 0);
                    d2 = d1.AddMinutes(30);
                    break;
                case Timeframes.Hour:
                    d1 = new DateTime(date.Year, date.Month, date.Day, date.Hour, 0, 0);
                    d2 = d1.AddHours(1);
                    break;
                case Timeframes.Day:
                    d1 = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
                    d2 = d1.AddDays(1);
                    break;
                case Timeframes.Week:
                    d1 = ToStartWeek(date); d2 = ToStartNextWeek(date);
                    d2 = d1.AddHours(1);
                    break;
            }

            DateTime[] dates = { d1, d2 };

            return dates;
        }

        private static DateTime ToStartWeek(DateTime d)
        {
            int year = d.Year;
            d = new DateTime(d.Year, d.Month, d.Day, 0, 0, 0);
            switch (d.DayOfWeek)
            {
                case DayOfWeek.Tuesday:
                    d = d.AddDays(-1); break;
                case DayOfWeek.Wednesday:
                    d = d.AddDays(-2); break;
                case DayOfWeek.Thursday:
                    d = d.AddDays(-3); break;
                case DayOfWeek.Friday:
                    d = d.AddDays(-4); break;
                case DayOfWeek.Saturday:
                    d = d.AddDays(-5); break;
                case DayOfWeek.Sunday:
                    d = d.AddDays(-6); break;
            }

            if (d.Year < year)
                d = new DateTime(year, 1, 1, 0, 0, 0);

            return d;
        }

        private static DateTime ToStartNextWeek(DateTime d)
        {
            int year = d.Year;
            d = new DateTime(d.Year, d.Month, d.Day, 0, 0, 0);
            switch (d.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    d = d.AddDays(7); break;
                case DayOfWeek.Tuesday:
                    d = d.AddDays(6); break;
                case DayOfWeek.Wednesday:
                    d = d.AddDays(5); break;
                case DayOfWeek.Thursday:
                    d = d.AddDays(4); break;
                case DayOfWeek.Friday:
                    d = d.AddDays(3); break;
                case DayOfWeek.Saturday:
                    d = d.AddDays(2); break;
                case DayOfWeek.Sunday:
                    d = d.AddDays(1); break;
            }

            if (d.Year > year)
                d = new DateTime(d.Year, 1, 1, 0, 0, 0);

            return d;
        }

        /// <summary>
        /// Get all timeframe items
        /// </summary>
        /// <param name="withTicks">Tick item included</param>
        /// <returns></returns>
        public static IEnumerable<TimeframeItem> GetTimeframeItems(bool withTicks = true)
        {
            List<TimeframeItem> items = new List<TimeframeItem>();
            if (withTicks) items.Add(new TimeframeItem(Timeframes.Tick));
            items.Add(new TimeframeItem(Timeframes.Min));
            items.Add(new TimeframeItem(Timeframes.Min5));
            items.Add(new TimeframeItem(Timeframes.Min10));
            items.Add(new TimeframeItem(Timeframes.Min15));
            items.Add(new TimeframeItem(Timeframes.Min20));
            items.Add(new TimeframeItem(Timeframes.Min30));
            items.Add(new TimeframeItem(Timeframes.Hour));
            items.Add(new TimeframeItem(Timeframes.Day));
            items.Add(new TimeframeItem(Timeframes.Week));

            return items;
        }

        /// <summary>
        /// Is tf1 less then tf2
        /// </summary>
        /// <param name="a">Timeframe 1</param>
        /// <param name="b">Timeframe 2</param>
        /// <returns></returns>
        public static bool IsLess(Timeframes a, Timeframes b)
        {
            int ai = (int)a; int bi = (int)b;
            return ai < bi;
        }

        /// <summary>
        /// Is tf1 less or equal then tf2
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool IsLessOrEqual(Timeframes a, Timeframes b)
        {
            int ai = (int)a; int bi = (int)b;
            return ai <= bi;
        }
    }

    /// <summary>
    /// Use for visualization
    /// </summary>
    public class TimeframeItem
    {
        public TimeframeItem(Timeframes tf)
        {
            this.Tf = tf;
            string[] names = TfHelper.GetTimeframeNames(tf);
            this.ShortName = names[0];
            this.Name = names[1];
        }

        public Timeframes Tf { get; private set; }
        public string Name { get; private set; }
        public string ShortName { get; private set; }
    }
}
