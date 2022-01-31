using System;
using System.Collections.Generic;

namespace Common
{
    public delegate void OnTickEH(Tick tick);
    public delegate void OnChangeDateEventHandler(DateTime time);

    /// <summary>
    /// Use for all tick sources
    /// </summary>
    public interface ITickSource
    {
        /// <summary>
        /// On every tick event
        /// </summary>
        event OnTickEH OnTick;

        /// <summary>
        /// On change from time
        /// </summary>
        event OnChangeDateEventHandler OnChangeDate;

        /// <summary>
        /// Current time or null (if not set)
        /// </summary>
        DateTime? CurrentTime { get; }
    }

    /// <summary>
    /// The all ticks dispatcher 
    /// </summary>
    public interface ITickDispatcher
    {
        /// <summary>
        /// Subscribe to all ticks for instrument
        /// </summary>
        /// <param name="subscriber">Subscriber</param>
        /// <param name="insID">Instrument</param>
        /// <param name="onTick">OnTick callback</param>
        void Subscribe(object subscriber, int insID, OnTickEH onTick);

        /// <summary>
        /// Unsubscribe from instrument
        /// </summary>
        /// <param name="subscriber">Subscriber</param>
        /// <param name="insID">Instrument</param>
        void Unsubscribe(object subscriber, int insID);

        /// <summary>
        /// Unsubscribe from all instruments
        /// </summary>
        void UnsubscribeAll();

        /// <summary>
        /// Add new trade
        /// </summary>
        /// <param name="tick">Trade info</param>
        void AddTick(Tick tick);

        /// <summary>
        /// Последний тик по указанному инструменту (за текущую сессию)
        /// </summary>
        /// <param name="insID">Инструмент</param>
        /// <returns>Если тиков еще нет, то default value</returns>
        Tick GetLastTick(int insID);

        /// <summary>
        /// Последний тик за текущую сессию
        /// </summary>
        /// <returns>Если тиков еще нет, то default value</returns>
        Tick GetLastTick();

        /// <summary>
        /// Инициализация перед началом торговой сессии.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Список фин инструментов, для которых есть накопленные данные
        /// </summary>
        IEnumerable<int> GetInstrumIDs();
        
        /// <summary>
        /// Список всех сделок по указанному инструменту
        /// </summary>
        /// <param name="insID">Инструмент</param>
        /// <returns></returns>
        IEnumerable<Tick> GetTicks(int insID);

        /// <summary>
        /// Список тиков начиная с указанного и до последнего (на данный момент)
        /// </summary>
        /// <param name="insID">Инструмент</param>
        /// <param name="skip">Сколько тиков пропустить в начале</param>
        /// <returns></returns>
        Tick[] GetLastTicks(int insID, int skip);
    }

    /// <summary>
    /// Tick structure
    /// </summary>
    public struct Tick
    {
        /// <summary>
        /// Trade number (from trading system)
        /// </summary>
        public long TradeNo;

        /// <summary>
        /// Trade time
        /// </summary>
        public DateTime Time;

        /// <summary>
        /// Instrument
        /// </summary>
        public int InsID;

        /// <summary>
        /// Quantity
        /// </summary>
        public int Lots;

        /// <summary>
        /// Price
        /// </summary>
        public decimal Price;

        public Tick(long tradeNo, DateTime time, int insID, int lots, decimal price)
        {
            this.TradeNo = tradeNo;
            this.Time = time;
            this.InsID = insID;
            this.Price = price;
            this.Lots = lots;
        }
    }
}
