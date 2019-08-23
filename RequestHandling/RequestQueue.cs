using System.Collections.Generic;

namespace AmiBroker.DataSources.IB
{
    internal class RequestQueue
    {
        protected bool noTimeOuts = true;     // true indicates that there were no request timeouts
        protected string queueName;
        protected SortedList<int, Request> requestList;

        internal RequestQueue(string queueName)
        {
            const int queueNameLen = 10;

            // to easy debug log reading
            if (queueName.Length >= queueNameLen)
                this.queueName = queueName.Substring(0, queueNameLen);
            else
                this.queueName = queueName.PadRight(10);

            requestList = new SortedList<int, Request>(50);
        }

        internal bool IsBusy
        {
            get
            {
                lock (requestList)
                    return requestList.Count > 0;
            }
        }

        internal void Enqueue(Request request)
        {
            lock (requestList)
                requestList.Add(request.Id, request);
        }

        internal void Clear()
        {
            lock (requestList)
                requestList.Clear();
        }

        internal virtual bool ProcessQueuedRequests(IBController ibController, bool allowNewRequest, bool writeLog)
        {
            int cntAtStart;
            int cntAtEnd;

            bool savedAllowNewRequest = allowNewRequest;

            lock (requestList)
            {
                cntAtStart = requestList.Count;

                for (int i = cntAtStart - 1; i >= 0; i--)
                {
                    noTimeOuts &= requestList.Values[i].RequestTimeouts == 0;

                    if (requestList.Values[i].IsFinished)
                        requestList.RemoveAt(i);
                }

                cntAtEnd = requestList.Count;
            }

            // we must limit the open requests to 5
            for (int i = 0; i < cntAtEnd && i < 5; i++)
                allowNewRequest &= requestList.Values[i].Process(ibController, allowNewRequest);

            if (cntAtEnd == 0)
                noTimeOuts = true;
#if DEBUG
            if (writeLog)
                LogAndMessage.Log(MessageType.Trace, queueName + ": Allow new request: " + (savedAllowNewRequest ? "1" : "0") + "/" + (allowNewRequest ? "1" : "0") + "  Requests: " + cntAtStart.ToString("#0") + "/" + cntAtEnd.ToString("#0"));
#endif
            return allowNewRequest;
        }

        internal virtual Request TryGetRequest(int reqId)
        {
            Request request = null;
            lock (requestList)
                requestList.TryGetValue(reqId, out request);
            return request;
        }
    }
}
