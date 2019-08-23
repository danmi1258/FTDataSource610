using System;
using System.Collections.Generic;
using System.Linq;
using IBApi;
using AmiBroker.Data;

namespace AmiBroker.DataSources.IB
{
    internal class StreamingDataRequest : Request
    {
        internal StreamingDataRequest(TickerData tickerData) : base(tickerData)
        { }

        internal override bool Process(IBController ibController, bool allowNewRequest)
        {
            // if no contract received yet 
            if (TickerData.ContractStatus == ContractStatus.SendRequest || TickerData.ContractStatus == ContractStatus.WaitForResponse)
                return allowNewRequest;

            if (allowNewRequest)
            {
                if (TickerData.ContractStatus == ContractStatus.Ok)
                {
                    ibController.SendSubscriptionRequest(Id, TickerData, true);
                    TickerData.RealTimeWindowStatus = true;
                }
                else
                    TickerData.RealTimeWindowStatus = false;

                IsFinished = true;
                return false;
            }

            return allowNewRequest;
        }
    }
}
