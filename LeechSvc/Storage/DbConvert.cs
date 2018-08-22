namespace Storage
{
    /// <summary>
    /// DbConvert contains converter scripts
    /// </summary>
    public class DbConvert
    {
        #region Script 0
        private const string SCRIPT_0 = @"
CREATE TABLE [Account] (
    [AccountID] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 
    [Code] VARCHAR(50) NOT NULL,
    [Name] TEXT NOT NULL, 
    [CommPerc] DECIMAL(18, 5) NOT NULL, 
    [IsShortEnable] BOOLEAN NOT NULL, 
    [AccountType] TINYINT NOT NULL DEFAULT 0);

CREATE TABLE [InsStore] (
    [InsStoreID] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 
    [InsID] INTEGER NOT NULL, 
    [Tf] TINYINT NOT NULL, 
    [IsEnable] BOOLEAN NOT NULL);

CREATE UNIQUE INDEX [Idx_InsStore_1] ON [InsStore] ([InsID] ASC, [Tf] ASC);

CREATE TABLE [BarHistory] (
    [InsStoreID] INTEGER NOT NULL CONSTRAINT [FK_BarHistory_InsStore] REFERENCES [InsStore] ([InsStoreID]), 
    [Time] INTEGER NOT NULL, 
    [OpenPrice] DECIMAL(18, 5) NOT NULL,
    [ClosePrice] DECIMAL(18, 5) NOT NULL,
    [HighPrice] DECIMAL(18, 5) NOT NULL,
    [LowPrice] DECIMAL(18, 5) NOT NULL,
    [Volume] BIGINT NOT NULL);

CREATE INDEX [Idx_BarHistory_1] ON [BarHistory] ([InsStoreID] ASC, [Time] ASC);

CREATE TABLE [Cash] (
    [CashID] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    [Initial] DECIMAL(18, 5) NOT NULL,
    [AccountID] INTEGER NOT NULL CONSTRAINT [FK_Cash_Account] REFERENCES [Account] ([AccountID]), 
    [Current] DECIMAL(18, 5) NOT NULL,
    [Sell] DECIMAL(18, 5) NOT NULL,
    [Buy] DECIMAL(18, 5) NOT NULL,
    [SellComm] INTEGER(18, 5) NOT NULL,
    [BuyComm] INTEGER(18, 5) NOT NULL);

CREATE TABLE [Instrum] (
    [InsID] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    [Ticker] VARCHAR(50) NOT NULL,
    [ShortName] VARCHAR(50) NOT NULL,
    [Name] VARCHAR(1000) NOT NULL,
    [LotSize] INTEGER NOT NULL, 
    [Decimals] INTEGER NOT NULL,
    [PriceStep] DECIMAL(18, 5) NOT NULL);

CREATE TABLE [StopOrder] (
    [StopOrderID] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    [Time] INTEGER NOT NULL, 
    [InsID] INTEGER NOT NULL CONSTRAINT [FK_StopOrder_Instrum] REFERENCES [Instrum] ([InsID]), 
    [BuySell] TINYINT NOT NULL, 
    [StopType] TINYINT NOT NULL, 
    [EndTime] INTEGER, 
    [AlertPrice] DECIMAL(18, 5) NOT NULL,
    [Price] DECIMAL(18, 5), 
    [LotCount] INTEGER NOT NULL, 
    [Status] TINYINT NOT NULL, 
    [AccountID] INTEGER NOT NULL CONSTRAINT [FK_StopOrder_Account] REFERENCES [Account] ([AccountID]), 
    [CompleteTime] INTEGER,
    [StopOrderNo] BIGINT NOT NULL);

CREATE TABLE [Order] (
    [OrderID] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    [Time] INTEGER NOT NULL, 
    [InsID] INTEGER NOT NULL CONSTRAINT [FK_Order_Instrum] REFERENCES [Instrum] ([InsID]), 
    [BuySell] TINYINT NOT NULL, 
    [LotCount] INTEGER NOT NULL, 
    [Price] DECIMAL(18, 5), 
    [Status] TINYINT NOT NULL, 
    [AccountID] INTEGER NOT NULL CONSTRAINT [FK_Order_Account] REFERENCES [Account] ([AccountID]), 
    [StopOrderID] INTEGER CONSTRAINT [FK_Order_StopOrder] REFERENCES [StopOrder] ([StopOrderID]),
    [OrderNo] BIGINT NOT NULL);

CREATE TABLE [Trade] (
    [TradeID] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    [OrderID] INTEGER NOT NULL CONSTRAINT [FK_Trade_Order] REFERENCES [Order] ([OrderID]), 
    [Time] INTEGER NOT NULL, 
    [InsID] INTEGER NOT NULL CONSTRAINT [FK_Trade_Instrum] REFERENCES [Instrum] ([InsID]), 
    [BuySell] TINYINT NOT NULL, 
    [LotCount] INTEGER NOT NULL, 
    [Price] DECIMAL(18, 5) NOT NULL,
    [AccountID] INTEGER NOT NULL CONSTRAINT [FK_Trade_Account] REFERENCES [Account] ([AccountID]), 
    [Comm] DECIMAL(18, 5) NOT NULL,
    [TradeNo] BIGINT NOT NULL);

CREATE TABLE [Cashflow] (
    [CashflowID] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    [Summa] DECIMAL(18, 5) NOT NULL,
    [Time] INTEGER NOT NULL, 
    [TradeID] INTEGER CONSTRAINT [FK_Cashflow_Trade] REFERENCES [Trade] ([TradeID]), 
    [Spend] TINYINT NOT NULL, 
    [AccountID] INTEGER NOT NULL CONSTRAINT [FK_Cashflow_Account] REFERENCES [Account] ([AccountID]));

CREATE TABLE [Holding] (
    [HoldingID] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    [InsID] INTEGER NOT NULL CONSTRAINT [FK_Holding_Instrum] REFERENCES [Instrum] ([InsID]), 
    [LotCount] INTEGER NOT NULL, 
    [AccountID] INTEGER NOT NULL CONSTRAINT [FK_Holding_Account] REFERENCES [Account] ([AccountID]));

CREATE TABLE [Repository] (
    [ReposID] INTEGER NOT NULL PRIMARY KEY, 
    [Key] TEXT NOT NULL, 
    [Data] TEXT NOT NULL);

CREATE INDEX [IX_Repository_Key] ON[Repository] ([Key]);

CREATE TABLE [InsStoreFreeDays] (
    [InsStoreID] INTEGER NOT NULL CONSTRAINT [FK_InsStoreFreeDays_InsStore] REFERENCES [InsStore] ([InsStoreID]), 
    [Date] INTEGER NOT NULL);

CREATE INDEX [IX_InsStoreFreeDays_Date] ON [InsStoreFreeDays] ([Date]);

CREATE TABLE [InsStorePeriods] (
    [InsStoreID] INTEGER NOT NULL CONSTRAINT [FK_InsStorePeriods_InsStore] REFERENCES [InsStore] ([InsStoreID]), 
    [StartDate] INTEGER NOT NULL, 
    [EndDate] INTEGER NOT NULL, 
    [IsLastDirty] BOOLEAN NOT NULL DEFAULT 0);

CREATE INDEX [IX_InsStorePeriods_InsStoreIDStartDate] ON [InsStorePeriods] ([InsStoreID], [StartDate]);

CREATE TABLE [DBVersion] ([Version] INTEGER);

INSERT INTO DBVersion (Version) values (0);
";
        #endregion

        private static string[] _scripts = { SCRIPT_0 };

        /// <summary>
        /// All scripts
        /// </summary>
        public static string[] Scripts
        {
            get
            {
                return _scripts;
            }
        }
    }
}
