using System;

namespace Common.Data
{
    /// <summary>
    /// Trade account
    /// </summary>
    public class Account
    {
        /// <summary>
        /// Identifier
        /// </summary>
        public int AccountID { get; set; }

        /// <summary>
        /// Code
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Broker commission percentage
        /// </summary>
        public decimal CommPerc { get; set; }

        /// <summary>
        /// Short position enable
        /// </summary>
        public bool IsShortEnable { get; set; }

        /// <summary>
        /// Account type - for testing or real trading
        /// </summary>
        public AccountTypes AccountType { get; set; }

        public Account()
        {
            Name = ""; Code = "";
        }
    }

    /// <summary>
    /// Account type: for testing, for real trading
    /// </summary>
    public enum AccountTypes : byte
    {
        Test = 0,
        Real = 1
    }

    /// <summary>
    /// Use for display account list
    /// </summary>
    public class AccountListItem
    {
        public AccountListItem(int accountID, string name)
        {
            this.AccountID = accountID;
            this.Name = name;
        }

        /// <summary>
        /// Identifier
        /// </summary>
        public int AccountID { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// Positions for money
    /// </summary>
    public class Cash
    {
        /// <summary>
        /// Identifier
        /// </summary>
        public int CashID { get; set; }

        /// <summary>
        /// Initial summa
        /// </summary>
        public decimal Initial { get; set; }

        /// <summary>
        /// Account reference
        /// </summary>
        public int AccountID { get; set; }

        /// <summary>
        /// Current summa
        /// </summary>
        public decimal Current { get; set; }

        /// <summary>
        /// Total sell summa
        /// </summary>
        public decimal Sell { get; set; }

        /// <summary>
        /// Total buy summa
        /// </summary>
        public decimal Buy { get; set; }

        /// <summary>
        /// Total sell trades commission
        /// </summary>
        public decimal SellComm { get; set; }

        /// <summary>
        /// Total buy trades commission
        /// </summary>
        public decimal BuyComm { get; set; }
    }

    /// <summary>
    /// Cash flow
    /// </summary>
    public class Cashflow
    {
        /// <summary>
        /// Identifier
        /// </summary>
        public int CashflowID { get; set; }

        /// <summary>
        /// Summa
        /// </summary>
        public decimal Summa { get; set; }

        /// <summary>
        /// Date and time
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// Trade reference or null
        /// </summary>
        public int? TradeID { get; set; }

        /// <summary>
        /// Cash flow type
        /// </summary>
        public CashflowSpend Spend { get; set; }

        /// <summary>
        /// Account reference
        /// </summary>
        public int AccountID { get; set; }
    }

    /// <summary>
    /// Positions for fin. instruments
    /// </summary>
    public class Holding
    {
        /// <summary>
        /// Identifier
        /// </summary>
        public int HoldingID { get; set; }

        /// <summary>
        /// Fin. instrument reference
        /// </summary>
        public int InsID { get; set; }

        /// <summary>
        /// Lots count
        /// </summary>
        public int LotCount { get; set; }

        /// <summary>
        /// Account reference
        /// </summary>
        public int AccountID { get; set; }
    }

    /// <summary>
    /// Trade order
    /// </summary>
    public class Order
    {
        /// <summary>
        /// Identifier
        /// </summary>
        public int OrderID { get; set; }

        /// <summary>
        /// Number in trade system
        /// </summary>
        public long OrderNo { get; set; }

        /// <summary>
        /// Create date and time
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// Fin. instrument reference
        /// </summary>
        public int InsID { get; set; }

        /// <summary>
        /// Buy or sell order
        /// </summary>
        public BuySell BuySell { get; set; }

        /// <summary>
        /// Lots count
        /// </summary>
        public int LotCount { get; set; }

        /// <summary>
        /// Price or null (null - current market price)
        /// </summary>
        public decimal? Price { get; set; }

        /// <summary>
        /// Order status
        /// </summary>
        public OrderStatus Status { get; set; }

        /// <summary>
        /// Account reference
        /// </summary>
        public int AccountID { get; set; }

        /// <summary>
        /// Stop order reference or null
        /// </summary>
        public int? StopOrderID { get; set; }
    }

    /// <summary>
    /// Stop order
    /// </summary>
    public class StopOrder
    {
        /// <summary>
        /// Identifier
        /// </summary>
        public int StopOrderID { get; set; }

        /// <summary>
        /// Number in trade system
        /// </summary>
        public long StopOrderNo { get; set; }

        /// <summary>
        /// Create date and time
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// Fin. instrument reference
        /// </summary>
        public int InsID { get; set; }

        /// <summary>
        /// Buy or sell
        /// </summary>
        public BuySell BuySell { get; set; }

        /// <summary>
        /// Stop order type (stoploss or takeprofit)
        /// </summary>
        public StopOrderType StopType { get; set; }

        /// <summary>
        /// End time or null (null - infinite)
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// Activation price (create order)
        /// </summary>
        public decimal AlertPrice { get; set; }

        /// <summary>
        /// Order price or null (null - current market price)
        /// </summary>
        public decimal? Price { get; set; }

        /// <summary>
        /// Order lots count
        /// </summary>
        public int LotCount { get; set; }

        /// <summary>
        /// Stop order status
        /// </summary>
        public StopOrderStatus Status { get; set; }

        /// <summary>
        /// Account reference
        /// </summary>
        public int AccountID { get; set; }

        /// <summary>
        /// Date and time of activation or completion
        /// </summary>
        public DateTime? CompleteTime { get; set; }
    }

    /// <summary>
    /// Trade
    /// </summary>
    public class Trade
    {
        /// <summary>
        /// Identifier
        /// </summary>
        public int TradeID { get; set; }

        /// <summary>
        /// Order reference (not null)
        /// </summary>
        public int OrderID { get; set; }

        /// <summary>
        /// Create date and time
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// Fin. instrument reference
        /// </summary>
        public int InsID { get; set; }

        /// <summary>
        /// Buy or sell
        /// </summary>
        public BuySell BuySell { get; set; }

        /// <summary>
        /// Lots count
        /// </summary>
        public int LotCount { get; set; }

        /// <summary>
        /// Trade price
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Account reference
        /// </summary>
        public int AccountID { get; set; }

        /// <summary>
        /// Broker commission
        /// </summary>
        public decimal Comm { get; set; }

        /// <summary>
        /// Trade number (from external trading system)
        /// </summary>
        public long TradeNo { get; set; }
    }
}
