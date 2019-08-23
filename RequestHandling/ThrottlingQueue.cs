using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AmiBroker.DataSources.IB
{
    /// <summary>
    /// Keeps track of requests (timestamps) sent in a given period of time to avoid pacing violation errors
    /// 
    /// </summary>
    internal class ThrottlingQueue
    {
        private int period;
        private int requestNo;
        private int minWait;
        private DateTime last;

        // queue to hold timestamps of requests
        private Queue<DateTime> queue;

        internal ThrottlingQueue(int period, int requestNo, int minWait)
        {
            this.period = period;
            this.requestNo = requestNo;
            this.minWait = minWait;
            this.last = DateTime.MinValue;

            queue = new Queue<DateTime>();
        }

        internal void AddRequest()
        {
            last = DateTime.Now;
            lock (queue)
                queue.Enqueue(last);
        }

        internal bool IsThrottled()
        {
            DateTime holdTime = DateTime.Now.AddSeconds(-period);

            lock (queue)
            {
                // remove timestamp of older request
                while (queue.Count > 0 && queue.Peek() < holdTime)
                    queue.Dequeue();

                // check if new request can NOT be sent
                return queue.Count >= requestNo                                         // to many request within the period period
                    || (minWait != 0 && last.AddSeconds(minWait) >= DateTime.Now);      // must wait at least minWait seconds since last request
            }
        }
    }
}
