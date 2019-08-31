using System;
using System.Text;
using IBApi;

namespace AmiBroker.DataSources.FT
{
    // SYMBOL FORMAT:
    // symbol[/underlying][@primary exchange][-exchange[-security type[-currency[:data specifier]]]]
    //
    // SYMBOL:
    // TWS ticker symbol in symbol mode (LocalSymbol). Right click on the TWS page, select Contract Dispaly Mode menu, select Symbol Mode menu. Add Symbol column to the page.
    // Some symbol includes spaces. Spaces must be retained. E.g.: "YM   SEP 11". Must include all 4 spaces!
    //
    // EXCHANGE:
    // See IB site. E.g.: SMART, NASDAQ, NYSE, IDEALPRO, IDEAL, etc.
    //
    // SECURITY TYPES: 
    //  STK = Stock
    //  OPT = Option
    //  FUT = Future
    //  IND = Index
    //  FOP = Future Option
    //  CASH = Forex pair
    //  BAG = Bag
    //  BOND = Bond
    //  CFD = Contract For Difference
    //  FUND = Mutual fund
    //  CMDTY = Commodity
    // 
    // CURRENCY:
    // use 3 letter currency specifiers. E.g.: USD, EUR, AUD, etc.
    //
    // DATA SPECIFIERS:
    //  A = Ask
    //  B = Bid
    //  BA = Bid and Ask
    //  T = Trades
    //  M = Midpoint
    //  DA = Dividend adjusted
    //  HV = Historical volatitlty
    //  IV = Option implied volatility
    //
    // DEFAULTS:
    // exchange: SMART
    // security type: STK
    // currency: USD
    // data specifier: MIDPOINT for CASH, CFD, FUND, CMDTY; TRADES for all other
    //
    // E.g:
    // --- FOREX ---
    // EUR.USD-IDEALPRO-CASH:M
    // EUR.USD-IDEALPRO-CASH
    //
    // --- STOCK ---
    // MSFT-SMART-STK-USD:T
    // MSFT-SMART-STK-USD
    // MSFT-SMART
    // MSFT
    //
    // --- INDEX ---
    // INDU-NYSE-IND-USD
    // INDU-NYSE-IND
    //
    // --- OPTION ---
    // MSFT  110319C00030000-SMART-OPT-USD
    // MSFT  110319C00030000-SMART-OPT
    //
    // --- FUTURES ---
    // YM   SEP 11-ECBOT-FUT-USD
    // YM   SEP 11-ECBOT-FUT
    //
    //
    // CONTIGUOUS FUTURE CONTRACT: (~ indicates contiguous contract
    // ES~-GLOBEX-FUT-USD, MSFT1D~/MSFT-ONE-FUT-USD

    public class SymbolParts
    {
        #region Consts

        // const of converting SecurityType
        internal static string[] SecTypeStrings = {
                                                      "S", "STK",
                                                      "O", "OPT",
                                                      "F", "FUT",
                                                      "I", "IND",
                                                      "P", "FOP",
                                                      "C", "CASH",
                                                      "G", "BAG",
                                                      "W", "WAR",
                                                      "B", "BOND",
                                                      "D", "CFD",
                                                      "U", "FUND",
                                                      "Y","CMDTY",
                                                     // "T","IOPT",
                                                     // "N","NEWS"
                                                  };

        internal const int MaxABSymbolLength = 25;

        #endregion

        // the two basic part of an AB Symbol
        public readonly string Ticker;                  // Contract part of the AmiBroker symbol. This is used to build IB contract.
        public readonly string Specifiers;              // Specifier part of the AmiBroker symbol. Specifies how data is collected for this ticker. (reqHistoricalData and ReqMktData parameter to collect MIDPOINT, BID, ASK, BIDASK data)

        // These fields are parsed from the Ticker
        public string Symbol;                           // IB Contract's LocalSymbol
        internal string Underlying;                     // IB Contract's Underlying security
        internal string PrimaryExchange;                // IB Contract's primary exchange
        public readonly string Exchange;                // IB Contract's Exchange and PrimaryExchange
        internal readonly string Type;                  // IB Contract's Sec. Type. E.g: F, FUT, S, STK, C, CASH, O, OPT, etc.
        public string Currency;                         // IB Contract's Currency
        public bool IsContinuous;                       // Continuous, back-adjusted future contact

        // These fields are "normalized" and "default"
        public string SecurityType;                     // IB Contract's Sec. Type. E.g: FUT, STK, CASH, OPT. If symbol does not define it, default value is set
        internal string DataType;                       // data to collect. E.g: A, B, BA, M, T. If symbol does not define it, default value is set
        public string NormalizedTicker;                 // Normalized ticker that has all symbol parts set.

        /// <summary>
        /// Constructor to parse an AB symbol and split it to TWS contract properties
        /// </summary>
        /// <param name="abSymbol"></param>
        public SymbolParts(string abSymbol)
        {
            abSymbol = abSymbol.ToUpper();

            //
            // separate the ticker and the data specifier (e.g.: "EUR.USD-IDEALPRO-CASH:M" -> "EUR.USD-IDEALPRO-CASH" and "M")
            //

            int indexOfColon = abSymbol.IndexOf(':');
            if (indexOfColon >= 0)
            {
                Ticker = abSymbol.Substring(0, indexOfColon);
                Specifiers = abSymbol.Substring(indexOfColon + 1);
            }
            else
            {
                Ticker = abSymbol;
                Specifiers = string.Empty;
            }

            //
            // separate and process the AB symbol
            //

            string[] tickerParts = Ticker.Split('-');

            switch (tickerParts.Length)
            {
                case 4:
                    Currency = tickerParts[3].Trim();
                    if (string.IsNullOrEmpty(Currency))
                        goto default;
                    goto case 3;

                case 3:
                    Type = tickerParts[2].Trim();
                    if (string.IsNullOrEmpty(Type))
                        goto default;
                    goto case 2;

                case 2:
                    Exchange = tickerParts[1].Trim();
                    if (string.IsNullOrEmpty(Exchange))
                        goto default;
                    goto case 1;

                case 1:
                    if (string.IsNullOrEmpty(tickerParts[0]))
                        goto default;
                    SecurityType = GetSecurityType();
                    DataType = GetDataType();
                    ParseSymbol(tickerParts[0]);
                    break;

                default:
                    throw new ArgumentOutOfRangeException("Invalid AmiBroker symbol.");
            }

            // setting missing values to defaults
            if (SecurityType != "CASH" && string.IsNullOrEmpty(Currency))
                Currency = "USD";

            if (string.IsNullOrEmpty(Exchange))
                Exchange = "SMART";

            NormalizedTicker = BuildNormalizedTicker();
        }

        /// <summary>
        /// Constructor to build AB symbol from TWS contract properties 
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="exchange"></param>
        /// <param name="securityType"></param>
        /// <param name="currency"></param>
        /// <param name="specifiers"></param>
        public SymbolParts(string symbol, string exchange, string securityType, string currency, string specifiers)
        {
            if (string.IsNullOrEmpty(symbol))
                throw new ArgumentNullException("symbol");

            Exchange = exchange.ToUpper();

            Type = securityType.ToUpper();
            SecurityType = GetSecurityType();

            if (!string.IsNullOrEmpty(securityType) && string.IsNullOrEmpty(exchange))
                throw new ArgumentNullException("exchange", "The exchange parameter must be specified if securityType is provided.");

            Currency = currency.ToUpper();
            if (!string.IsNullOrEmpty(currency) && string.IsNullOrEmpty(exchange))
                throw new ArgumentNullException("exchange", "The exchange parameter must be specified if currency is provided.");
            if (!string.IsNullOrEmpty(currency) && string.IsNullOrEmpty(securityType))
                throw new ArgumentNullException("securityType", "The securityType parameter must be specified if currency is provided.");

            Specifiers = specifiers.ToUpper();
            DataType = GetDataType();

            Symbol = symbol.ToUpper();

            ParseSymbol(Symbol);

            NormalizedTicker = BuildNormalizedTicker();
            Ticker = NormalizedTicker;
        }

        public override string ToString()
        {
            return Ticker + (string.IsNullOrEmpty(Specifiers) ? string.Empty : ":" + Specifiers);
        }

        #region helpers

        /// <summary>
        /// Returns the security type (STK, CASH, BOND, FUT, FOP, etc.) of the ticker (even if it defaults)
        /// </summary>
        /// <returns></returns>
        private string GetSecurityType()
        {
            // default (STK)
            if (string.IsNullOrEmpty(Type))
                return "STK";

            // check short & long sectype strings for match and return long format (e.g.: S -> STK)
            for (int i = 0; i < SecTypeStrings.GetLength(0); i++)
                if (Type == SecTypeStrings[i])
                {
                    if ((i & 1) == 0)
                        i++;
                    return SecTypeStrings[i];
                }

            // not empty and no match
            throw new ArgumentOutOfRangeException("Invalid AmiBroker symbol. Unknown security type.");
        }

        /// <summary>
        /// Determine the data type (trades, ask, bid, midpoint, etc.) that needs to be collected from AB streams (even if it defaults)
        /// </summary>
        /// <returns></returns>
        private string GetDataType()
        {
            // if no data specifier, then security type sets the default data type
            if (string.IsNullOrEmpty(Specifiers))
            {
                if (SecurityType == "CASH" || SecurityType == "CFD" || SecurityType == "FUND" || SecurityType == "CMDTY")
                    return IBHistoricalDataType.Midpoint;
                else
                    return IBHistoricalDataType.Trades;
            }

            // use data specifier in AB symbol
            else if (IBHistoricalDataType.IsValidIBHistoricalDataType(Specifiers, SecurityType))
                return Specifiers;

            throw new ArgumentOutOfRangeException("Invalid AmiBroker symbol. Data type '" + Specifiers + "' is not valid for the '" + SecurityType + "' security type.");
        }

        /// <summary>
        /// Parses and sets security's symbol of the ticker (even if it is continuous)
        /// </summary>
        /// <param name="symbol"></param>
        private void ParseSymbol(string symbol)
        {
            // read primary exchage if any
            int idx = symbol.IndexOf('@');
            if (idx > 0)
            {
                PrimaryExchange = symbol.Substring(idx + 1);
                if (PrimaryExchange.Length == 0)
                    throw new ArgumentOutOfRangeException("Invalid AmiBroker symbol. PrimaryExchange must have a valid value.");

                symbol = symbol.Substring(0, idx);
            }
            else
                PrimaryExchange = string.Empty;

            idx = symbol.IndexOf('/');
            if (idx > 0)
            {
                Underlying = symbol.Substring(idx + 1);
                if (Underlying.Length == 0)
                    throw new ArgumentOutOfRangeException("Invalid AmiBroker symbol. Underlying must have a valid value.");

                symbol = symbol.Substring(0, idx);
            }
            else
                Underlying = string.Empty;

            // if it has a "continuous symbol name"
            idx = symbol.IndexOf('~');
            if (idx > 0)
            {
                Symbol = symbol.Substring(0, idx);
                IsContinuous = true;
                if (Symbol.Length == 0)
                    throw new ArgumentOutOfRangeException("Invalid AmiBroker symbol. Continuous contract must have a valid local id.");
                if (SecurityType != "FUT")
                    throw new ArgumentOutOfRangeException("Invalid AmiBroker symbol. Only FUT contracts can have contiguous, back-adjusted data from multiple .");
            }
            else
            {
                Symbol = symbol;
                IsContinuous = false;
            }

            if (SecurityType == "CASH")
                if (Symbol.IndexOf('.') != 3 || Symbol.Length != 7)
                    throw new ArgumentOutOfRangeException("Invalid AmiBroker symbol. CASH contracts must have a base currency in the SYMBOL.");
                else
                    Currency = Symbol.Substring(4);
        }

        /// <summary>
        /// Builds the symbol part of the ticker
        /// </summary>
        /// <returns></returns>
        private string BuildSymbol()
        {
            string s = "";

            if (!string.IsNullOrEmpty(PrimaryExchange))
                s = "@" + PrimaryExchange;

            if (!string.IsNullOrEmpty(Underlying))
                s = "/" + Underlying + s;

            if (IsContinuous)
                s = "~" + s;

            return Symbol + s;
        }

        /// <summary>
        /// Builds the normalized ticker.
        /// </summary>
        /// <param name="longTicker"></param>
        /// <returns></returns>
        private string BuildNormalizedTicker(/* bool longTicker */)
        {
            StringBuilder sb = new StringBuilder(50);

            sb.Append(BuildSymbol());

            if (!string.IsNullOrEmpty(Exchange))
            {
                sb.Append('-');
                sb.Append(Exchange);

                if (!string.IsNullOrEmpty(Type))
                {
                    sb.Append('-');
                    //sb.Append(longTicker ? SecurityType: Type);
                    sb.Append(SecurityType);

                    if (!string.IsNullOrEmpty(Currency))
                    {
                        if (SecurityType != "CASH")
                        {
                            sb.Append('-');
                            sb.Append(Currency);
                        }
                    }
                    else // if (longTicker)
                        sb.Append("-USD");
                }
                else // if (longTicker)
                    sb.Append("-STK-USD");
            }
            else // if (longTicker)
                sb.Append("-SMART-STK-USD");

            return sb.ToString().ToUpper();
        }

        #endregion
    }
}
