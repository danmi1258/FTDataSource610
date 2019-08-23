using System;
using System.Collections.Generic;
namespace AmiBroker.DataSources.IB
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// This list helps mapping a quotation's date and time to a trade date. 
    /// This is needed to build continuous intradays bars mixed with EOD bars.
    /// </remarks>
    internal class TradingDayList : List<TradingDay>
    {
        /// <summary>
        /// Constructor to build trading day time ranges from ContractDetails.TradingHours string
        /// These ranges will be used to decide the trading date of incoming quotes
        /// </summary>
        /// <param name="tradingHours">ContractDetails.TradingHours</param>
        internal TradingDayList(string tradingHours, bool useRanges)
        {
            // if no ContractDetails or TradingHours is not defined in it
            if (string.IsNullOrEmpty(tradingHours))
            {
                return;
            }

            //
            // Parsing the TradingHours string
            // sample TradingHours string: 
            // TYPE I:
            // 20090507:0700-1830,1830-2330;20090508:CLOSED
            // TYPE II: (TWS version 970+)
            // 20180323:0400-20180323:2000;20180326:0400-20180326:2000 

            string[] tdDays = tradingHours.Split(';');

            foreach (string tdDay in tdDays)
            {
                // E.g: "20090507:0700-1830,1830-2330"
                // E.g: "20180323:0400-20180323:2000"

                // sometimes the string starts with an empty daystring (";")
                if (string.IsNullOrWhiteSpace(tdDay))
                    continue;

                ReadTradeDayString(tdDay);
            }

            //
            // Setting the trading day hours according to trading days.
            // 5 seconds is added to the "end time", to tolerate delays in all systems. 
            // This should not cause any issue as it is after exchange's close time...
            // Only one or two actual items are used from this list even if all weeks data is available!!!
            //
            if (!useRanges)         // if we are loking for trading days
                for (int i = 0; i < this.Count; i++)
                {
                    if (i == 0)
                    {
                        TradingDay t = this[i];
                        t.From = t.To.AddDays(-1);
                        t.To = t.To.AddSeconds(5);
                        this[i] = t;
                    }
                    else
                    {
                        TradingDay t = this[i];
                        t.From = this[i - 1].To;
                        t.To = t.To.AddSeconds(5);
                        this[i] = t;
                    }
                }
        }

        /// <summary>
        /// Get the trading day (EOD bar's date) of an intraday time
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        internal DateTime GetTradeDate(DateTime date)
        {
            foreach (var tradingDayTimeRange in this)
            {
                if (date >= tradingDayTimeRange.From && date < tradingDayTimeRange.To)
                    return tradingDayTimeRange.TradeDate;
            }

            return DateTime.MinValue;
        }

        private void ReadTradeDayString(string tdDay)
        {
            // E.g: "20090507:0700-20090507:1830,20090507:1900-20090507:2130"
            // E.g: "20090507:0700-20090507:1830,1900-2130"
            // E.g: "20090508:CLOSED"

            // get the default date part (all starts with a date)
            DateTime defaultDate = DateTime.ParseExact(tdDay.Substring(0, 8), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);

            string[] tdDayRanges = tdDay.Split(',');

            foreach (string tdDayRange in tdDayRanges)
            {
                // E.g: "20090507:0700-20090507:1830"
                // E.g: "20090507:0700-1830"
                // E.g: "1830-2330"
                // E.g: "20090508:CLOSED"

                string[] tdDayRangeTimes = tdDayRange.Split('-');

                DateTime dateFrom = ReadTradeDayTimeString(defaultDate, tdDayRangeTimes[0]);
                if (dateFrom == DateTime.MinValue)
                    return;

                DateTime dateTo = ReadTradeDayTimeString(defaultDate, tdDayRangeTimes[1]);

                // get the opening hours part. We do not need closed days 
                TradingDay tradingDay = new TradingDay();
                tradingDay.TradeDate = dateFrom.Date;

                tradingDay.From = dateFrom;
                tradingDay.To = dateTo;
                if (tradingDay.To < tradingDay.From)
                    tradingDay.From = tradingDay.From.AddDays(-1);

                Add(tradingDay);
            }
        }

        private DateTime ReadTradeDayTimeString(DateTime defaultDate, string tdDayRangeParts)
        {
            DateTime date;
            TimeSpan time;

            string[] tdDayRangePartFrom = tdDayRangeParts.Split(':');

            if (tdDayRangePartFrom.Length == 2 && tdDayRangePartFrom[1].CompareTo("CLOSED") == 0)
                return DateTime.MinValue;

            if (tdDayRangePartFrom.Length == 2)
            {
                // get the date part
                date = DateTime.ParseExact(tdDayRangePartFrom[0].Substring(0, 8), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                time = TimeSpan.ParseExact(tdDayRangePartFrom[1].Substring(0, 4), "hhmm", System.Globalization.CultureInfo.InvariantCulture);
            }
            else
            {
                date = defaultDate;
                time = TimeSpan.ParseExact(tdDayRangePartFrom[0].Substring(0, 4), "hhmm", System.Globalization.CultureInfo.InvariantCulture);
            }

            return date.Add(time);
        }
    }
}
