using System;
using System.Globalization;

namespace AmiBroker.DataSources.FT
{
    internal abstract class Request
    {
        internal TickerData TickerData;
        internal int Id;                                // Id of the last sent request
        internal DateTime RequestTime;                  // Indicates when the last request was sent
        internal bool WaitingForResponse;               // Indicates that the last request is still active/waiting for response
        internal int RequestTimeouts;                   // How many times the the request was resent to get the data form IB
        internal bool IsFinished;                       // Response/Timeout/Error closed this request

        internal Request(TickerData tickerData)
        {
            if (tickerData == null)
                throw new ArgumentNullException();

            Id = IBClientHelper.GetNextReqId(); // this Id is used (instead of collections key) in case of HistoricalDataRequest and HeadTimestampRequest
                                                // because the same logical request may result in more actual request (e.g timeouts and multiple download periods)
            TickerData = tickerData;
            IsFinished = false;
            RequestTimeouts = 0;
        }

        internal virtual bool Process(FTController ibController, bool allowNewRequest)
        {
            return allowNewRequest;
        }

        internal string ToString(bool closing, bool showDetails)
        {
            //!closing & !showDetails = 0, 0
            //!closing &  showDetails = 1, 0
            // closing &  showDetails = 1, 1
            // closing & !showDetails = 0, 1

            if (!closing & !showDetails)
                return string.Empty;

            string result = string.Empty;

            if (showDetails)
                result = " reqId: " + Id.ToString();

            if (closing)
            {
                double elapsedTime = Math.Round(DateTime.Now.Subtract(RequestTime).TotalMilliseconds, 1);
                if (elapsedTime >= 1000.0)
                    result += " (" + (elapsedTime / 1000.0).ToString("N1", CultureInfo.InvariantCulture) + "s)";
                else
                    result += " (" + elapsedTime.ToString("N1", CultureInfo.InvariantCulture) + "ms)";
            }

            return result;
        }
    }
}
