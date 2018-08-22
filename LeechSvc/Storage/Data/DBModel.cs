using SQLite.Net.Attributes;

namespace Storage.Data
{
    public class Instrum
    {
        [PrimaryKey, AutoIncrement]
        public int InsID { get; set; }
        public string Ticker { get; set; }
        public string ShortName { get; set; }
        public string Name { get; set; }
        public int LotSize { get; set; }
        public int Decimals { get; set; }
        public decimal PriceStep { get; set; }
    }

    public class InsStore
    {
        [PrimaryKey, AutoIncrement]
        public int InsStoreID { get; set; }
        public int InsID { get; set; }
        public byte Tf { get; set; }
        public bool IsEnable { get; set; }
    }

    public class BarHistory
    {
        public int InsStoreID { get; set; }
        public int Time { get; set; }
        public decimal OpenPrice { get; set; }
        public decimal ClosePrice { get; set; }
        public decimal HighPrice { get; set; }
        public decimal LowPrice { get; set; }
        public long Volume { get; set; }
    }

    public class Account
    {
        [PrimaryKey, AutoIncrement]
        public int AccountID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public decimal CommPerc { get; set; }
        public bool IsShortEnable { get; set; }
        public byte AccountType { get; set; }
    }

    public class Cash
    {
        [PrimaryKey, AutoIncrement]
        public int CashID { get; set; }
        public decimal Initial { get; set; }
        public int AccountID { get; set; }
        public decimal Current { get; set; }
        public decimal Sell { get; set; }
        public decimal Buy { get; set; }
        public decimal SellComm { get; set; }
        public decimal BuyComm { get; set; }
    }

    public class Cashflow
    {
        [PrimaryKey, AutoIncrement]
        public int CashflowID { get; set; }
        public decimal Summa { get; set; }
        public int Time { get; set; }
        public int? TradeID { get; set; }
        public byte Spend { get; set; }
        public int AccountID { get; set; }
    }

    public class Holding
    {
        [PrimaryKey, AutoIncrement]
        public int HoldingID { get; set; }
        public int InsID { get; set; }
        public int LotCount { get; set; }
        public int AccountID { get; set; }
    }

    public class Order
    {
        [PrimaryKey, AutoIncrement]
        public int OrderID { get; set; }
        public int Time { get; set; }
        public int InsID { get; set; }
        public byte BuySell { get; set; }
        public int LotCount { get; set; }
        public decimal? Price { get; set; }
        public byte Status { get; set; }
        public int AccountID { get; set; }
        public int? StopOrderID { get; set; }
        public long OrderNo { get; set; }
    }

    public class StopOrder
    {
        [PrimaryKey, AutoIncrement]
        public int StopOrderID { get; set; }
        public int Time { get; set; }
        public int InsID { get; set; }
        public byte BuySell { get; set; }
        public byte StopType { get; set; }
        public int? EndTime { get; set; }
        public decimal AlertPrice { get; set; }
        public decimal? Price { get; set; }
        public int LotCount { get; set; }
        public byte Status { get; set; }
        public int AccountID { get; set; }
        public int? CompleteTime { get; set; }
        public long StopOrderNo { get; set; }
    }

    public class Trade
    {
        [PrimaryKey, AutoIncrement]
        public int TradeID { get; set; }
        public int OrderID { get; set; }
        public int Time { get; set; }
        public int InsID { get; set; }
        public byte BuySell { get; set; }
        public int LotCount { get; set; }
        public decimal Price { get; set; }
        public int AccountID { get; set; }
        public decimal Comm { get; set; }
        public long TradeNo { get; set; }
    }

    public class InsStorePeriods
    {
        public int InsStoreID { get; set; }
        public int StartDate { get; set; }
        public int EndDate { get; set; }
        public bool IsLastDirty { get; set; }
    }

    public class InsStoreFreeDays
    {
        public int InsStoreID { get; set; }
        public int Date { get; set; }
    }

    public class Repository
    {
        [PrimaryKey, AutoIncrement]
        public int ReposID { get; set; }
        public string Key { get; set; }
        public string Data { get; set; }
    }
}
