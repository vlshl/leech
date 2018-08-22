using System;

namespace Common.Data
{
    /// <summary>
    /// History bar data
    /// </summary>
    public class BarHistory
    {
        public BarHistory()
        {
            this.InsStoreID = 0;
            this.Time = DateTime.MinValue;
            this.OpenPrice = 0;
            this.ClosePrice = 0;
            this.HighPrice = 0;
            this.LowPrice = 0;
            this.Volume = 0;
        }

        /// <summary>
        /// InsStore reference
        /// </summary>
        public int InsStoreID { get; set; }

        /// <summary>
        /// Date and time of bar (accurate to a second)
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// Open bar price
        /// </summary>
        public decimal OpenPrice { get; set; }

        /// <summary>
        /// Close bar price
        /// </summary>
        public decimal ClosePrice { get; set; }

        /// <summary>
        /// High bar price
        /// </summary>
        public decimal HighPrice { get; set; }

        /// <summary>
        /// Low bar price
        /// </summary>
        public decimal LowPrice { get; set; }

        /// <summary>
        /// Total bar volume (in pieces, not lots)
        /// </summary>
        public long Volume { get; set; }
    }
}
