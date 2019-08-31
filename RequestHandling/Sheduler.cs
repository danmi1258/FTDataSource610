using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AmiBroker.Data;
using IBApi;

namespace AmiBroker.DataSources.FT
{
    class Scheduler : IDisposable
    {
        // work queues to store requests
        private RequestQueue contractQueue;                         // queue of tickers that needs to get ContractDetails
        private HistRequestQueue headTimestampQueue;                // queue of tickers that needs to get headTimestamp
        private RequestQueue symbolQueue;                           // queue of tickers that needs update of AB symbol data
        private RequestQueue subscriptionQueue;                     // queue of tickers that need market data subscription
        private HistRequestQueue backfillQueue;                     // queue of tickers that need historical data refresh

        // IB request scheduling/throttling queues
        private ThrottlingQueue ibAllRequestTimeQueue;              // queue of date and time of any general requests (max 50 request/sec)
        private ThrottlingQueue ibHstRequestTimeQueue;              // queue of date and time of historical data requests (max 60 requests in any 10 minutes period)
        private DateTime ibHstThrottlingEndTime;                    // time until new historical request can not be sent because of historical pacing violation error

        // plugin's background thread management
        private bool pluginIsRunning;                               // flag to let run/stop bck thread
        private Thread pluginThread;
        private AutoResetEvent pluginAutoResetEvent;

        // ab notification
        private DateTime nextQuoteNotificationTime;                 // when to send next notification of new quotes to AB

        private FTController ibController;

        public void Dispose()
        {
            pluginAutoResetEvent.Dispose();
        }

        internal Scheduler(FTController ibController)
        {
            this.ibController = ibController;

            ibAllRequestTimeQueue = new ThrottlingQueue(1, 50, 0);
            ibHstRequestTimeQueue = new ThrottlingQueue(600, 100, 2);

            contractQueue = new RequestQueue("Contracts");
            headTimestampQueue = new HistRequestQueue("HeadTmstp");
            symbolQueue = new RequestQueue("Symbols  ");
            subscriptionQueue = new RequestQueue("Subscript");
            backfillQueue = new HistRequestQueue("Backfill ");

            pluginIsRunning = true;
            pluginAutoResetEvent = new AutoResetEvent(false);
            pluginThread = new Thread(PluginExecutionLoop);
            pluginThread.Name = "IB DataSource processing thread";
            pluginThread.IsBackground = true;
        }

        #region Managing request scheduler

        internal void Start()
        {
            pluginThread.Start();
        }

        internal void Stop()
        {
            // stop background thread's loop
            pluginIsRunning = false;
            // if thread is waiting then let it run
            pluginAutoResetEvent.Set();
            // wait for thread to exit
            pluginThread.Join();

            pluginThread = null;

            pluginAutoResetEvent.Dispose();
        }

        internal bool IsBusy
        {
            get { return backfillQueue.IsBusy || subscriptionQueue.IsBusy || symbolQueue.IsBusy || headTimestampQueue.IsBusy || contractQueue.IsBusy; }
        }

        internal void ResetAllRequests()
        {
            // ordered according to dependency and load
            contractQueue.Clear();
            backfillQueue.Clear();
            subscriptionQueue.Clear();
            headTimestampQueue.Clear();
            symbolQueue.Clear();
        }

        internal void ResetBackfillRequests()
        {
            backfillQueue.Clear();
        }

        #endregion

        internal void ReqisterGeneralRequest()
        {
            ibAllRequestTimeQueue.AddRequest();
        }

        internal void ReqisterHistoricalDataRequest()
        {
            ibHstRequestTimeQueue.AddRequest();
            ibAllRequestTimeQueue.AddRequest();
        }

        internal void RegisterHstPacingViolation()
        {
            ibHstThrottlingEndTime = DateTime.Now.AddSeconds(30);
            LogAndMessage.Log(MessageType.Warning, "Throttling historical data requests until " + ibHstThrottlingEndTime.ToShortTimeString() + ".");
        }

        /// <summary>
        /// Start a search for IB contract(s) that maps to AB symbol
        /// </summary>
        /// <param name="tickerData"></param>
        internal void QueueContractRequest(TickerData tickerData)
        {
            lock (tickerData)       // request sending
            {
                // to prevent multiple running requests
                if (tickerData.ContractStatus == ContractStatus.SendRequest || tickerData.ContractStatus == ContractStatus.WaitForResponse)
                    return;

                tickerData.ContractStatus = ContractStatus.SendRequest;
            }

            contractQueue.Enqueue(new ContractRequest(tickerData));
        }

        /// <summary>
        /// Start the process of HeadTimestamp update
        /// </summary>
        /// <param name="tickerData"></param>
        internal void QueueHeadTimestampRequest(TickerData tickerData)
        {
            lock (tickerData)       // request sending
            {
                // to prevent multiple running requests
                if (tickerData.HeadTimestampStatus == HeadTimestampStatus.SendRequest || tickerData.HeadTimestampStatus == HeadTimestampStatus.WaitForResponse)
                    return;

                tickerData.HeadTimestampStatus = HeadTimestampStatus.SendRequest;
            }

            headTimestampQueue.Enqueue(new HeadTimestampRequest(tickerData));
        }

        /// <summary>
        /// Start the process of AB's symbol data update
        /// </summary>
        /// <param name="tickerData"></param>
        internal void QueueSymbolUpdateRequest(TickerData tickerData)
        {
            lock (tickerData)       // request sending
            {
                tickerData.SymbolStatus = SymbolStatus.WaitForContractUpdate;
            }

            symbolQueue.Enqueue(new SymbolRequest(tickerData));
        }

        /// <summary>
        /// Start a streaming data subscription process
        /// </summary>
        /// <param name="tickerData"></param>
        internal void QueueSubscriptionRequest(TickerData tickerData)
        {
            lock (tickerData)       // request sending
            {
                // mark ticker for RI update
                tickerData.RealTimeWindowStatus = true;

                // set default RI data (data is updated in IBFXClient_TickPrice and IBFXClient_TickSize)
                tickerData.RealTimeWindow = new RecentInfo();
                tickerData.RealTimeWindow.Name = tickerData.Ticker;
                tickerData.RealTimeWindow.Bitmap = RecentInfoField.DateUpdate;
            }

            subscriptionQueue.Enqueue(new StreamingDataRequest(tickerData));
        }

        /// <summary>
        /// Start the historical data backfill process
        /// </summary>
        /// <param name="tickerData"></param>
        /// <param name="filter"></param>
        /// <param name="refreshStartDate"></param>
        /// 
        /// 
        internal void QueueBackfillRequest(TickerData tickerData, bool filter, DateTime refreshStartDate)
        {
            lock (tickerData)       // request sending
            {
                tickerData.QuoteDataStatus = QuotationStatus.New;

                tickerData.Quotes = new QuotationList(FTDataSource.Periodicity);
                if (tickerData.SymbolParts.IsContinuous)
                    tickerData.ContinuousQuotesDictionary = new Dictionary<string, QuotationList>();
                tickerData.Filter = new RTTickFilter(tickerData, filter);
                tickerData.RefreshStartDate = refreshStartDate;
            }
            backfillQueue.Enqueue(new HistoricalDataRequest(tickerData));
        }

        internal ContractRequest TryGetForContractRequest(int requestId)
        {
            return (ContractRequest)contractQueue.TryGetRequest(requestId);
        }

        internal HeadTimestampRequest TryGetForHeadTimestampRequest(int requestId)
        {
            return (HeadTimestampRequest)headTimestampQueue.TryGetRequest(requestId);
        }

        internal HistoricalDataRequest TryGetForHistoricalDataRequest(int requestId)
        {
            return (HistoricalDataRequest)backfillQueue.TryGetRequest(requestId);
        }

        /// <summary>
        /// Plugin's main processing loop executed on a background thread
        /// </summary>
        /// <returns></returns>
        private void PluginExecutionLoop()
        {
#if DEBUG
            int longWait = 50;
            int shortWait = 50;
#else
            int longWait = 18;
            int shortWait = 5;
#endif
            while (pluginIsRunning)
            {
                // if last queue operation causes longer delay
                if (ProcessQueues())
                    pluginAutoResetEvent.WaitOne(longWait);
                else
                    pluginAutoResetEvent.WaitOne(shortWait);
            }
        }

        /// <summary>
        /// Processing all queues/request
        /// </summary>
        /// <returns>true, if any request was sent</returns>
        private bool ProcessQueues()
        {
            try
            {
#if DEBUG
                bool traceMode = backfillQueue.IsBusy || subscriptionQueue.IsBusy
                  || symbolQueue.IsBusy || headTimestampQueue.IsBusy || contractQueue.IsBusy;

                traceMode = false;

                if (traceMode)
                    LogAndMessage.Log(MessageType.Trace, "Start of ProcessAllQueues");
#else
                bool traceMode = false;
#endif
                //
                // sending notificaton to AB to get quotes from the plugin (GetQuotesEx will be called)
                //
                SendQuoteNotificationToAB();

                if (!ibController.IsIbConnected())
                    return true;

                // remove timed out entries from request throlling queue and check  if any request can be sent
                bool throttling = ibAllRequestTimeQueue.IsThrottled();
                if (throttling)
                    LogAndMessage.LogAndQueue(MessageType.Info, "Throttling all requests.");

                // remove timed out entries from hist request throlling queue and check if historical request can be sent
                bool histThrottling = ibHstRequestTimeQueue.IsThrottled() || ibHstThrottlingEndTime >= DateTime.Now;

                //
                // main queue processing logic
                //

                // check if general request can be sent
                bool allowNewRequest = !throttling;

                if (!histThrottling)
                    allowNewRequest &= backfillQueue.ProcessQueuedRequests(ibController, allowNewRequest, traceMode);

                allowNewRequest &= subscriptionQueue.ProcessQueuedRequests(ibController, allowNewRequest, traceMode);

                allowNewRequest &= headTimestampQueue.ProcessQueuedRequests(ibController, allowNewRequest, traceMode);

                allowNewRequest &= contractQueue.ProcessQueuedRequests(ibController, allowNewRequest, traceMode);

                allowNewRequest &= symbolQueue.ProcessQueuedRequests(ibController, allowNewRequest, traceMode);

                if (allowNewRequest)
                    allowNewRequest &= ibController.SendCurrentTimeRequest();

#if DEBUG
                if (traceMode)
                    LogAndMessage.Log(MessageType.Trace, "End of ProcessAllQueues");
#endif

                // other scheduled jobs
                ibController.StartContractRefresh();

                ibController.UpdateFailedTickers();

                return !allowNewRequest;                // indicate newly sent request -> longer wait time
            }
            catch (Exception ex)
            {
                LogAndMessage.LogAndQueue(MessageType.Error, "Program error. ProcessQueues exception: " + ex);
                return true;
            }
        }

        internal void SendQuoteNotificationToAB()
        {
            DateTime now = DateTime.Now;

            // periodically send new quote notification to AB
            if (nextQuoteNotificationTime < now)
            {
                nextQuoteNotificationTime = now.AddMilliseconds(500);

                // notify AB to collect new bar (Quotation) and real time data (RealTimeWindow)
                DataSourceBase.NotifyQuotesUpdate();
            }
        }
    }
}
