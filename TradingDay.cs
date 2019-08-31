using System;

namespace AmiBroker.DataSources.FT
{
    /// <summary>
    /// Trading day and its full time range
    /// </summary>
    internal struct TradingDay
    {
        internal DateTime TradeDate;
        internal DateTime From;
        internal DateTime To;
    }
}