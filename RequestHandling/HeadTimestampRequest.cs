using System;
using System.Collections.Generic;
using System.Linq;
using IBApi;
using AmiBroker.Data;
using System.Globalization;

namespace AmiBroker.DataSources.IB
{
    internal class HeadTimestampRequest : Request
    {
        internal HeadTimestampRequest(TickerData tickerData) : base(tickerData)
        { }

        internal override bool Process(IBController ibController, bool allowNewRequest)
        {
            const int requestTimeoutPeriod = 20;

            // if no contract received yet 
            if (TickerData.ContractStatus == ContractStatus.SendRequest || TickerData.ContractStatus == ContractStatus.WaitForResponse)
                return allowNewRequest;

            // if no contract found
            if (TickerData.ContractStatus != ContractStatus.Ok)
            {
                TickerData.HeadTimestampStatus = HeadTimestampStatus.Failed;
                IsFinished = true;
                return allowNewRequest;
            }

            lock (TickerData)  // request handling
            {
                // if not waiting for response 
                switch (TickerData.HeadTimestampStatus)
                {
                    // if marked to get headtimestamp
                    case HeadTimestampStatus.SendRequest:

                        if (allowNewRequest)
                        {
                            LogAndMessage.Log(TickerData, MessageType.Trace, "Requesting earliest data point. " + ToString(false, LogAndMessage.VerboseLog));

                            TickerData.HeadTimestampStatus = HeadTimestampStatus.WaitForResponse;
                            RequestTime = DateTime.Now;

                            ibController.SendHeadTimestampRequest(Id, TickerData);
                        }

                        return false;

                    // if request is sent, but response has not arrived yet
                    // see ibClient_HeadTimestamp event handler
                    case HeadTimestampStatus.WaitForResponse:

                        // if no answer in time
                        if (RequestTime.AddSeconds(requestTimeoutPeriod) < DateTime.Now)
                        {
                            if (RequestTimeouts == 0)
                            {
                                LogAndMessage.LogAndQueue(TickerData, MessageType.Error, "Request of earliest data point timed out. Retrying. " + ToString(true, LogAndMessage.VerboseLog));

                                RequestTimeouts++;
                                TickerData.HeadTimestampStatus = HeadTimestampStatus.SendRequest;
                                Id = IBClientHelper.GetNextReqId();
                                goto case HeadTimestampStatus.SendRequest;
                            }

                            LogAndMessage.LogAndQueue(TickerData, MessageType.Error, "Request of earliest data point timed out. " + ToString(true, LogAndMessage.VerboseLog));

                            TickerData.HeadTimestampStatus = HeadTimestampStatus.Failed;

                            goto case HeadTimestampStatus.Failed;
                        }

                        return allowNewRequest;

                    // if new, offline ticker 
                    case HeadTimestampStatus.Offline:
                    // ticker's HeadTimestamp is updated
                    case HeadTimestampStatus.Ok:
                    // ticker's HeadTimestamp is NOT updated (we do not mark ticker as failed. it still may work!)
                    case HeadTimestampStatus.Failed:

                        IsFinished = true;
                        return allowNewRequest;

                    // this is program error
                    default:

                        TickerData.HeadTimestampStatus = HeadTimestampStatus.Failed;
                        IsFinished = true;
                        return allowNewRequest;
                }
            }
        }

        internal void HeadTimestampReceived(string headTimestamp)
        {
            DateTime date;
            bool result = DateTime.TryParseExact(headTimestamp, "yyyyMMdd  HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);

            lock (TickerData)  // event handling
            {
                if (result)
                {
                    TickerData.EarliestDataPoint = date;
                    TickerData.HeadTimestampStatus = HeadTimestampStatus.Ok;
                }
                else
                {
                    TickerData.EarliestDataPoint = DateTime.MinValue;
                    TickerData.HeadTimestampStatus = HeadTimestampStatus.Failed;
                }
            }

            if (result)
                LogAndMessage.Log(TickerData, MessageType.Info, "Earliest data point value is updated. " + ToString(true, LogAndMessage.VerboseLog));
            else
                LogAndMessage.Log(TickerData, MessageType.Error, "Invalid earliest data point value received: " + headTimestamp + " " + ToString(true, LogAndMessage.VerboseLog));
        }
    }
}