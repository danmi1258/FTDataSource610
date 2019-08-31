using AmiBroker.Data;
using IBApi;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace AmiBroker.DataSources.FT
{
    public enum ContractStatus
    {
        [Description("Offline")]
        Offline,                            // ContractDetails not downloaded, ticker's StockInfo not updated
        [Description("SendRequest")]
        SendRequest,                        // ticker's ContractDetails needs to be updated from TWS
        [Description("WaitForResponse")]
        WaitForResponse,
        [Description("Ok")]
        Ok,                                 // StockInfo is up to date
        [Description("Failed")]
        Failed                              // ticker is not valid or IB does not find contract for it
    }

    public enum HeadTimestampStatus
    {
        [Description("Offline")]
        Offline,                            // HeadTimestamp not downloaded
        [Description("SendRequest")]
        SendRequest,                        // ticker's HeadTimestamp needs to be updated from TWS
        [Description("WaitForResponse")]
        WaitForResponse,
        [Description("Ok")]
        Ok,                                 // HeadTimestamp is up to date
        [Description("Failed")]
        Failed                              // ticker is not valid or IB does not find HeadTimestamp for it
    }

    public enum SymbolStatus
    {
        [Description("Offline")]
        Offline,                            // ticker's StockInfo must not be updated
        [Description("WaitForContractUpdate")]
        WaitForContractUpdate,              // tickerData's contract is being updated
        [Description("Ok")]
        Ok,                                 // ticker's StockInfo is up to date
        [Description("Failed")]
        Failed                              // ticker's StockInfo is not up to date because of some failer
    }

    public enum QuotationStatus
    {
        [Description("Offline")]
        Offline,                    // no quote update
        [Description("New")]
        New,                        // start updating historical quote
        [Description("DownloadingIntra")]
        DownloadingIntra,           // downloading historical intraday quotes from IB HMDS
        [Description("DownloadedIntra")]
        DownloadedIntra,            // historical intraday quotes are downloaded but need to be processed
        [Description("DownloadingEod")]
        DownloadingEod,             // downloading EOD historical quotes from IB HMDS
        [Description("DownloadedEod")]
        DownloadedEod,              // historical EOD quotes are downloaded but need to be processed
        [Description("Online")]
        Online,                     // receive RT streaming data, ticker is usable in charts and scans
        [Description("Failed")]
        Failed                      // ticker is not valid (symbology, See IsValid) or IB does not find contract for it (See IsKnown)
    }

    /// <summary>
    /// Primary ticker data
    /// </summary>
    /// <remarks>
    /// Each ticker that is used by AmiBroker (chart or RT window) has a corresponding object of this type.
    /// All public properties of TickerData can be read by any AFL script using the GetExtraData AFL method.
    /// </remarks>
    public class TickerData
    {
        public string DataSource = "IB";                // Identifies the datasource type (needed for GetExtraData)

        // AB symbol and ticker info
        public string Ticker;                           // Symbol as it is present in AmiBroker
        public SymbolParts SymbolParts;                 // Parsed AmiBroker symbol

        // IB's contract data
        public ContractStatus ContractStatus;           // IB contract data refresh status
        public ContractDetails ContractDetails;         // IB contract description for the AmiBroker symbol (Many AmiBroker symbols may map to the same IB contract. E.g.: MSFT, MSFT-STK, MSFT-STK-SMART, MSFT-STK-SMART-USD, MSFT-STK-SMART-USD:BA)
        internal List<ContractDetails> contractDetailsList = new List<ContractDetails>();
        //internal TradingDayList LiquidHours;            // List of datetime ranges when contract is traded (Regular Trading Hours)
        internal TradingDayList TradingDays;            // List of datetime ranges when contract is traded outside/overlapping of LiquidHours

        // Info for "Maximum" EOD backfill
        internal HeadTimestampStatus HeadTimestampStatus; // IB HeadTimestampStatus request status
        internal DateTime EarliestDataPoint = DateTime.MinValue;            // Used for max historical download (reqHeadTimestamp)

        // AB's symbol information update (StockInfo struct)
        public SymbolStatus SymbolStatus;               // Indicates if StockInfo needs to be updated
        internal StockInfo StockInfo;                   // AB's StockInfo

        // AB's quotes
        internal DateTime RefreshStartDate;             // Historical data refresh start date & time
        public QuotationStatus QuoteDataStatus;         // Status of quotation data of the ticker
        internal QuotationList Quotes;                  // Temporary quotes of proper periodicity. Received ticks are merged into this quote list. This list is merged into AB's quotation array.
        internal Dictionary<string, QuotationList> ContinuousQuotesDictionary;
        internal RTTickFilter Filter;                   // RT ticks filtered. It stores received ticks as well.

        // AB's RecentInfo
        public bool RealTimeWindowStatus;               // AmiBroker uses this ticker in RT window
        public RecentInfo RealTimeWindow;               // RecentInfo for AmiBroker's Real Time Window
        internal int LastSizeSum;                       // Stores summarized LastSize events between two Volume events
        internal int LastVolume;                        // Stores last Volume event
        internal float LastPrice;                       // Stores last price until next LastSize event... (Trades)

        // Stream/TWS status
        public int LastTickTime;                        // last tick's time
        public int LastTickDate;                        // last tick's date

        // General
        public bool IsValid                             // if ticker is formed according to symbology rules
        {
            get { return SymbolParts != null; }
        }
        public bool IsKnown                             // if it is known by IB contract db
        {
            get { return ContractDetails != null; }
        }

        public override string ToString()
        {
            if (SymbolParts.IsContinuous && ContractDetails != null)
                return Ticker + " (" + ContractDetails.Contract.LocalSymbol + ")";
            else
                return Ticker;
        }

        public string ToString(Contract currContract)
        {
            if (SymbolParts.IsContinuous && currContract != null)
                return Ticker + " (" + currContract.LocalSymbol + ")";
            else
                return Ticker;
        }
    }
}