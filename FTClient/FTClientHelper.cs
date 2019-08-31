using AmiBroker.Data;
using IBApi;
using System;
using System.Globalization;
using System.Threading;

namespace AmiBroker.DataSources.FT
{
    internal static class IBClientHelper
    {
        internal const int MaxDownloadDaysOfSmallBars = 180;
        internal const int MaxDownloadDaysOfMediumBars = 950;

        private static int lastRequestId;

        /// <summary>
        /// Get the period (minutes) to calculate the start date of next IB histirical data request
        /// (This includes weekends as well)
        /// </summary>
        /// <param name="periodicity"></param>
        /// <returns></returns>
        internal static int GetDownloadStep(Periodicity periodicity)
        {
            int downloadStep;

            switch (periodicity)
            {
                case Periodicity.Tick:                          // tick,    30 minutes
                    downloadStep = 30;
                    break;
                case Periodicity.OneSecond:                     // 1 sec,   30 minutes
                    downloadStep = 30;
                    break;
                case Periodicity.FiveSeconds:                   // 5 secs,  2 hours
                    downloadStep = 2 * 60;
                    break;
                case Periodicity.FifteenSeconds:                // 15 secs, 8 hours
                    downloadStep = 8 * 60;
                    break;
                case Periodicity.OneMinute:                     // 1 min,   1 week
                    downloadStep = 168 * 60;
                    break;
                case Periodicity.FiveMinutes:                   // 5 mins,  1 week
                    downloadStep = 168 * 60;
                    break;
                case Periodicity.FifteenMinutes:                // 15 mins, 3 weeks
                    downloadStep = 3 * 168 * 60;
                    break;
                case Periodicity.OneHour:                       // 1 hour,  5 weeks
                    downloadStep = 5 * 168 * 60;
                    break;
                case Periodicity.EndOfDay:                      // 1 day,   8 years
                    downloadStep = (2 + 8 * 365) * 24 * 60;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
            return downloadStep;
        }

        /// <summary>
        /// Get the period (minutes) for the IB histirical data request
        /// (This does NOT include weekends)
        /// </summary>
        /// <param name="periodicity"></param>
        /// <returns></returns>
        internal static int GetDownloadInterval(Periodicity periodicity)
        {
            int downloadInterval;

            switch (periodicity)
            {
                case Periodicity.Tick:                          // tick,    30 minutes
                    downloadInterval = 30;
                    break;
                case Periodicity.OneSecond:                     // 1 sec,   30 minutes
                    downloadInterval = 30;
                    break;
                case Periodicity.FiveSeconds:                   // 5 secs,  2 hours
                    downloadInterval = 2 * 60;
                    break;
                case Periodicity.FifteenSeconds:                // 15 secs, 8 hours
                    downloadInterval = 8 * 60;
                    break;
                case Periodicity.OneMinute:                     // 1 min,   1 week (6 days to download!)
                    downloadInterval = 144 * 60;
                    break;
                case Periodicity.FiveMinutes:                   // 5  mins, 1 week (6 days to download!)
                    downloadInterval = 144 * 60;
                    break;
                case Periodicity.FifteenMinutes:                // 15 mins, 3 weeks (2 weeks + 6 days)
                    downloadInterval = 480 * 60;
                    break;
                case Periodicity.OneHour:                       // 1 hour,  5 weeks
                    downloadInterval = 816 * 60;
                    break;
                case Periodicity.EndOfDay:                      // 1 day,   8 years
                    downloadInterval = (2 + 8 * 365) * 24 * 60;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return downloadInterval;
        }

        /// <summary>
        /// Get the default refresh interval (minutes) 
        /// </summary>
        /// <param name="periodicity"></param>
        /// <returns></returns>
        internal static int GetDefaultDownloadPeriod(Periodicity periodicity)
        {
            int defaultDownloadPeriod = GetDownloadStep(periodicity);

            if (periodicity != Periodicity.EndOfDay)
                defaultDownloadPeriod *= 2;

            return defaultDownloadPeriod;
        }

        /// <summary>
        /// Check if the DateTime presents a banking hour (to skip hist. intraday request for weekends)
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        internal static bool IsBankingHour(DateTime dateTime)
        {
            if (dateTime.DayOfWeek == DayOfWeek.Sunday && dateTime.Hour < 12)
                return false;

            if (dateTime.DayOfWeek == DayOfWeek.Saturday)
                return false;

            return true;
        }

        /// <summary>
        /// Adjust the download start date to a "rounded date" depending on periodicty
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="periodicity"></param>
        /// <returns></returns>
        internal static DateTime GetAdjustedStartDate(DateTime startDate, Periodicity periodicity, DateTime earliestStartDate, bool adjustDownward)
        {
            DateTime adjustedStartDate = startDate;

            // if download start is before earliest data point
            if (startDate < earliestStartDate)
                adjustedStartDate = earliestStartDate;

            int requestStep = GetDownloadStep(periodicity);

            if (periodicity < Periodicity.EndOfDay)
            {
                if (requestStep > 168 * 60) // can download MORE THAN a week
                {
                    if (adjustedStartDate.DayOfWeek != DayOfWeek.Sunday)
                    {
                        DateTime sunday;
                        if (adjustDownward)
                            sunday = adjustedStartDate.Date.AddDays(-(int)adjustedStartDate.DayOfWeek);     // sunday of last week / Sunday 00 /
                        else
                            sunday = adjustedStartDate.Date.AddDays(7 - (int)adjustedStartDate.DayOfWeek);  // sunday of this week / Sunday 00 /

                        // adjust it to pervious sunday noon (so we request data from prev sunday noon to saturday 00
                        adjustedStartDate = sunday.AddHours(12); //sunday noon
                    }
                }

                else if (requestStep == 168 * 60) // can download a week
                {
                    // adjust it to midnight
                    if (adjustedStartDate.TimeOfDay.Minutes != 0)
                        if (adjustDownward)
                            adjustedStartDate = adjustedStartDate.Date.AddDays(-1);  //start of next day
                        else
                            adjustedStartDate = adjustedStartDate.Date.AddDays(1);  //start of next day
                }
                else // can download LESS THAN a week
                {
                    //int corr = 0;
                    //if (adjustDownward)
                    //    corr = 1;

                    int minutes = (int)adjustedStartDate.TimeOfDay.TotalMinutes;
                    int trimmedMinutes = (minutes / requestStep) * requestStep;
                    //if (trimmedMinutes != minutes)
                    //    trimmedMinutes += requestStep;

                    adjustedStartDate = adjustedStartDate.Date.AddMinutes(trimmedMinutes); //start of previous 30/60 min period
                }
            }

            return adjustedStartDate;
        }

        /// <summary>
        /// Convert AB database periodicity to IB bar size
        /// </summary>
        /// <param name="periodicity"></param>
        /// <returns></returns>
        internal static string ConvSecondsToIBBarSize(Periodicity periodicity)
        {
            switch (periodicity)
            {
                case Periodicity.Tick:
                    return "1 secs"; //BarSize.OneSecond;
                case Periodicity.OneSecond:
                    return "1 secs"; //BarSize.OneSecond;
                case Periodicity.FiveSeconds:
                    return "5 secs"; //BarSize.FiveSeconds;
                case Periodicity.FifteenSeconds:
                    return "15 secs"; //BarSize.FifteenSeconds;
                case Periodicity.OneMinute:
                    return "1 min"; // BarSize.OneMinute;
                case Periodicity.FiveMinutes:
                    return "5 mins"; //BarSize.FiveMinutes;
                case Periodicity.FifteenMinutes:
                    return "15 mins"; //BarSize.FifteenMinutes;
                case Periodicity.OneHour:
                    return "1 hour"; //BarSize.OneHour;
                case Periodicity.EndOfDay:
                    return "1 day"; //BarSize.OneDay;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Convert a date range to the period string for reqHistoricalData
        /// </summary>
        /// <param name="StartTime"></param>
        /// <param name="EndTime"></param>
        /// <returns></returns>
        internal static string ConvSecondsToIBPeriod(DateTime StartTime, DateTime EndTime)
        {
            TimeSpan period = EndTime.Subtract(StartTime);
            double secs = period.TotalSeconds;
            long unit;

            if (secs < 1)
            {
                throw new ArgumentOutOfRangeException("Period cannot be less than 1 second.");
            }
            else
            {
                if (secs < 86400)           // less then a day
                {
                    unit = (long)Math.Ceiling(secs);
                    return unit.ToString(CultureInfo.InvariantCulture) + " S";
                }
                else
                {
                    double days = secs / 86400;

                    unit = (long)Math.Ceiling(days);
                    if (unit <= 13)
                        return unit.ToString(CultureInfo.InvariantCulture) + " D";
                    else
                    {
                        double weeks = days / 7;

                        unit = (long)Math.Ceiling(weeks);
                        if (unit <= 7)
                            return unit.ToString(CultureInfo.InvariantCulture) + " W";
                        else
                        {
                            double months = weeks / 4;

                            unit = (long)Math.Ceiling(months);
                            if (unit <= 11)
                                return unit.ToString(CultureInfo.InvariantCulture) + " M";
                            else
                            {
                                double years = months / 12;

                                unit = (long)Math.Ceiling(years);
                                if (unit < 10)
                                    return unit.ToString(CultureInfo.InvariantCulture) + " Y";
                                else
                                    throw new ArgumentOutOfRangeException("Period cannot be bigger than 10 years.");
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Generate unique, sequential ids for requests
        /// </summary>
        /// <returns></returns>
        internal static int GetNextReqId()
        {
            return Interlocked.Increment(ref lastRequestId);
        }

        /// <summary>
        /// Convert IB error code range to log's MessageType
        /// </summary>
        /// <param name="errorCode"></param>
        /// <returns></returns>
        internal static MessageType GetMessageType(int errorCode)
        {
            MessageType messageType;

            if (errorCode > 2000)                        // HDMS and other "Warning Message Codes"
                messageType = MessageType.Info;
            else if (errorCode > 1000)                   // "System Message Codes"
                messageType = MessageType.Warning;
            else                                                // "API  Message Codes"
                messageType = MessageType.Error;
            return messageType;
        }

        /// <summary>
        /// Convert contract expiry string to DateTime
        /// </summary>
        /// <param name="contract"></param>
        /// <returns></returns>
        internal static DateTime GetContractExpiryDateTime(Contract contract)
        {
            if (contract == null || string.IsNullOrEmpty(contract.LastTradeDateOrContractMonth))
                return DateTime.MaxValue;

            try
            {
                if (contract.LastTradeDateOrContractMonth.Length == 8)
                    return DateTime.ParseExact(contract.LastTradeDateOrContractMonth, "yyyyMMdd", CultureInfo.InvariantCulture);

                if (contract.LastTradeDateOrContractMonth.Length == 6)
                {
                    // format: "yyyyMM"
                    int year = int.Parse(contract.LastTradeDateOrContractMonth.Substring(0, 4));
                    int month = int.Parse(contract.LastTradeDateOrContractMonth.Substring(4, 2));

                    DateTime lastTradeDay = new DateTime(year, month, 1);
                    lastTradeDay = lastTradeDay.AddMonths(1);
                    lastTradeDay = lastTradeDay.AddDays(-1);        // last day of the month
                    return lastTradeDay;
                }
                LogAndMessage.LogAndQueue(MessageType.Error, "Program error. Unknown format of last trade data/contract month:" + contract.LastTradeDateOrContractMonth);
            }
            catch
            {
                LogAndMessage.LogAndQueue(MessageType.Error, "Program error. Cannot interpret format of last trade data/contract month:" + contract.LastTradeDateOrContractMonth);
            }

            return DateTime.MinValue;
        }

        /// <summary>
        /// Check if the (future) contract is expired
        /// </summary>
        /// <param name="contract"></param>
        /// <returns></returns>
        internal static bool IsContractExpired(Contract contract)
        {
            if (contract == null || string.IsNullOrEmpty(contract.LastTradeDateOrContractMonth))
                return false;

            return GetContractExpiryDateTime(contract) < DateTime.Now.Date;
        }
    }
}