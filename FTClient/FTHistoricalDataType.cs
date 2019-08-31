using System;

namespace AmiBroker.DataSources.FT
{
    internal static class IBHistoricalDataType
    {
        /// <summary>
        /// Return Trade (Last) data only
        /// </summary>
        internal const string Trades = "T";

        /// <summary>
        /// Return the mid point between the bid and ask
        /// </summary>
        internal const string Midpoint = "M";

        /// <summary>
        /// Return bid Prices only
        /// </summary>
        internal const string Bid = "B";

        /// <summary>
        /// Return ask prices only
        /// </summary>
        internal const string Ask = "A";

        /// <summary>
        /// Return bid or ask price only
        /// </summary>
        internal const string BidAsk = "BA";

        /// <summary>
        /// Return dividend adjusted trade prices
        /// </summary>
        internal const string Adjusted = "DA";

        /// <summary>
        /// Return historical volatility
        /// </summary>
        internal const string HistoricalVolatility = "HV";

        /// <summary>
        /// Return implied volatility
        /// </summary>
        internal const string ImpliedVolatility = "IV";

        // consts  of converting DataType
        private readonly static string[] historicalDataTypeShort = { "A", "B", "BA", "M", "T", "DA", "HV", "IV" };
        private readonly static string[] historicalDataTypeFull = { "ASK", "BID", "BID_ASK", "MIDPOINT", "TRADES", "ADJUSTED_LAST", "HISTORICAL_VOLATILITY", "OPTION_IMPLIED_VOLATILITY" };

        /// <summary>
        /// Check if dataType is a valid value for a securityType
        /// </summary>
        /// <param name="dataTypeString"></param>
        /// <returns></returns>
        internal static bool IsValidIBHistoricalDataType(string dataTypeString, string securityType)
        {
            for (int i = 0; i < historicalDataTypeShort.GetLength(0); i++)
                if (dataTypeString == historicalDataTypeShort[i])
                {
                    if (securityType == "STK")
                        return true;    // all data type
                    else if (securityType == "IND")
                    {
                        if (i >= 4)     // trades and up
                            return true;
                    }
                    else if (securityType == "CASH" || securityType == "CFD" || securityType == "FUND" || securityType == "CMDTY")
                    {
                        if (i <= 3)     // bid/ask/midpoint
                            return true;
                    }
                    else if (i <= 4)    // bid/ask/midpoint/tredes
                        return true;
                    else
                        return false;

                    break;
                }

            return false;
        }

        /// <summary>
        /// Returns the IB "long" data type string for a reqHistoricalData request
        /// </summary>
        /// <param name="dataTypeString"></param>
        /// <returns></returns>
        internal static string GetIBHistoricalDataType(string dataTypeString)
        {
            for (int i = 0; i < historicalDataTypeShort.GetLength(0); i++)
                if (dataTypeString == historicalDataTypeShort[i])
                    return historicalDataTypeFull[i];

            throw new ArgumentOutOfRangeException("Invalid AmiBroker symbol. Unknown data specifier.");
        }
    }
}