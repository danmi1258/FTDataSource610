using System.Collections.Generic;
using AmiBroker.Data;

namespace AmiBroker.DataSources.IB
{
    internal class HistRequestQueue : RequestQueue
    {
        internal HistRequestQueue(string queueName) : base(queueName)
        { }

        internal override bool ProcessQueuedRequests(IBController ibController, bool allowNewRequest, bool writeLog)
        {
            int cntAtStart;
            int cntAtEnd;

            bool savedAllowNewRequest = allowNewRequest;

            //
            // we must limit the open requests to 1
            //

            lock (requestList)
            {
                cntAtStart = requestList.Count;

                if (cntAtStart > 0)
                    if (requestList.Values[0].IsFinished)
                        requestList.RemoveAt(0);

                cntAtEnd = requestList.Count;
            }

            if (cntAtEnd > 0)
                allowNewRequest &= requestList.Values[0].Process(ibController, allowNewRequest);

#if DEBUG
            if (writeLog)
                LogAndMessage.Log(MessageType.Trace, queueName + ": Allow new request: " + (savedAllowNewRequest ? "1" : "0") + "/" + (allowNewRequest ? "1" : "0") + "  Requests: " + cntAtStart.ToString("#0") + "/" + cntAtEnd.ToString("#0"));
#endif
            return allowNewRequest;
        }

        /// <summary>
        /// Historical data download uses usually MORE THAN 1 ibClient.reqHistoricalData calls/request
        /// Request.Id is used for identifying sent requests. The collections key can be out of sync from the second sent request, but used for ordering!!!
        /// </summary>
        /// <param name="reqId"></param>
        /// <returns></returns>
        internal override Request TryGetRequest(int reqId)
        {
            lock (requestList)
                for (int i = 0; i < requestList.Count; i++)
                    if (requestList.Values[i].Id == reqId)
                        return requestList.Values[i];

            return null;
        }
    }
}
