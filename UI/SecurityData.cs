using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace AmiBroker.DataSources.FT
{
    [Serializable()]
    public class SecurityData
    {
        internal string longName;
        internal string localSymbol;
        internal string symbol;
        internal string securityType;
        internal string right;
        internal double strike;
        internal string lastTradeDateOrContractMonth;
        internal string currency;
        internal int contractId;
        internal string exchange;
        internal string primaryExchange;
        internal float minTick;
        internal float priceMagnifier;

        public string PrimaryExchange
        {
            get { return primaryExchange; }
            set { primaryExchange = value; }
        }

        public string Exchange
        {
            get { return exchange; }
            set { exchange = value; }
        }

        public int ContractId
        {
            get { return contractId; }
            set { contractId = value; }
        }

        public string Currency
        {
            get { return currency; }
            set { currency = value; }
        }

        public string LastTradeDateOrContractMonth
        {
            get { return lastTradeDateOrContractMonth; }
            set { lastTradeDateOrContractMonth = value; }
        }

        public double Strike
        {
            get { return strike; }
            set { strike = value; }
        }

        public string Right
        {
            get { return right; }
            set { right = value; }
        }

        public string SecType
        {
            get { return securityType; }
            set { securityType = value; }
        }

        public string Symbol
        {
            get { return symbol; }
            set { symbol = value; }
        }

        public string LocalSymbol
        {
            get { return localSymbol; }
            set { localSymbol = value; }
        }

        public string LongName
        {
            get { return longName; }
            set { longName = value; }
        }

        public float MinTick
        {
            get { return minTick; }
            set { minTick = value; }
        }

        public float PriceMagnifier
        {
            get { return priceMagnifier; }
            set { priceMagnifier = value; }
        }
    }

    internal enum SecurityDataField
    {
        Currency, Exchange, LastTradeDateOrContractMonth, LongName, PrimaryExchange, Right, Strike, Symbol, LocalSymbol, SecType
    }

    internal class SecurityDataComparer : IComparer<SecurityData>
    {
        private SecurityDataField sortedField;
        private SortOrder sortOrder;

        /// <summary>
        /// constructor to set the sort column and sort order.
        /// </summary>
        /// <param name="memberName"></param>
        /// <param name="sortOrder"></param>
        internal SecurityDataComparer(SecurityDataField sortedField, SortOrder sortOrder)
        {
            this.sortedField = sortedField;
            this.sortOrder = sortOrder;
        }

        /// <summary>
        /// Compares two SecurityData based on member name and sort order
        /// and return the result.
        /// </summary>
        /// <param name="sd1"></param>
        /// <param name="sd2"></param>
        /// <returns></returns>
        public int Compare(SecurityData sd1, SecurityData sd2)
        {
            int returnValue;

            switch (sortedField)
            {
                case SecurityDataField.Currency:

                    return CompareByTwoFields(sd1.Currency, sd1.Symbol
                                            , sd2.Currency, sd2.Symbol);

                case SecurityDataField.Exchange:

                    return CompareByTwoFields(sd1.Exchange, sd1.Symbol
                                            , sd2.Exchange, sd2.Symbol);

                case SecurityDataField.LastTradeDateOrContractMonth:

                    return CompareByTwoFields(sd1.LastTradeDateOrContractMonth, sd1.Symbol
                                            , sd2.LastTradeDateOrContractMonth, sd2.Symbol);

                case SecurityDataField.LongName:

                    return CompareByTwoFields(sd1.LongName, sd1.Symbol
                                            , sd2.LongName, sd2.Symbol);

                case SecurityDataField.PrimaryExchange:

                    return CompareByTwoFields(sd1.PrimaryExchange, sd1.Symbol
                                            , sd2.PrimaryExchange, sd2.Symbol);

                case SecurityDataField.Right:     // backward!!!
                    return CompareByTwoFields(sd2.Right, sd1.Symbol
                                            , sd1.Right, sd2.Symbol);

                case SecurityDataField.Strike:
                    if (sortOrder == SortOrder.Ascending)
                    {
                        returnValue = sd1.Strike.CompareTo(sd2.Strike);
                        if (returnValue == 0)
                            returnValue = sd1.Symbol.CompareTo(sd2.Symbol);
                    }
                    else
                    {
                        returnValue = sd2.Strike.CompareTo(sd1.Strike);
                        if (returnValue == 0)
                            returnValue = sd2.Symbol.CompareTo(sd1.Symbol);

                    }
                    return returnValue;

                case SecurityDataField.Symbol:

                    return CompareByTwoFields(sd1.Symbol, sd1.LocalSymbol
                                            , sd2.Symbol, sd2.LocalSymbol);

                default:

                    return CompareByTwoFields(sd1.LocalSymbol, sd1.Symbol
                                            , sd2.LocalSymbol, sd2.Symbol);
            }
        }

        private int CompareByTwoFields(string a1, string a2, string b1, string b2)
        {
            int result = a1.CompareTo(b1);
            if (result == 0)
                result = a2.CompareTo(b2);

            if (sortOrder == SortOrder.Descending)
                result = -1 * result;

            return result;
        }
    }
}
