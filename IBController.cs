using AmiBroker.Data;
using IBApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace AmiBroker.DataSources.IB
{
    internal enum IBPluginState : int { Disconnected, Ready, Busy }

    internal delegate void ContractListReadyDelegate();
    internal delegate void ContractReadyDelegate(ContractDetails contractDetails);

    internal class IBController : IDisposable
    {
        // event to notify SearchForm 
        internal event ContractListReadyDelegate OnContractListReady;
        internal event ContractReadyDelegate OnContractReady;

        // data of tickers
        private TickerDataCollection tickers;

        // IB interface
        private IBClient ibClient;
        private int ibClientId;
        private bool isIBConnected;                                 // if TWS is connected to an IB gateway server
        internal bool RestartStreaming;                             // if connection loss causes resubmitting stream requests

        // SearchForm request
        private int searchReqId;
        private int searchContractCnt;

        // Server time & house keeping
        private TimeSpan serverTimeCorr = new TimeSpan(0);              // difference between local time and server time (delays, e.g: networks, software, etc.)
        private DateTime nextUpdateOfCurrentTime = DateTime.MinValue;   // when to send time sync request
        private DateTime nextUpdateOfContracts = DateTime.Now;          // when to refresh contracts

        private Scheduler scheduler;

        private DateTime nextUpdateOfFailedTickers;
        private string failedTickers;                                   // list of all tickers with any problem
        private string properties;                                      // list of property name usable in GetExtraData

        private SortedList<int, List<Bar>> histTempResults = new SortedList<int, List<Bar>>();

        internal IBController()
        {
            try
            {
                ibClientId = IBDataSource.IBConfiguration.ClientId;
                if (ibClientId == 0)
                    ibClientId = Process.GetCurrentProcess().Id;

                tickers = new TickerDataCollection();

                scheduler = new Scheduler(this);
                scheduler.Start();
            }
            catch (Exception ex)
            {
                LogAndMessage.Log(MessageType.Error, "IBController's constructor failed: " + ex);
                throw;
            }
        }

        #region Connection and Status

        /// <summary>
        ///  connect to TWS
        /// </summary>
        /// <param name="autoReconnect"></param>
        internal void Connect(bool autoReconnect)
        {
            try
            {
                if (ibClient == null)
                    ibClient = new IBClient(this);

                if (!ibClient.IsConnected)
                {
                    isIBConnected = false;                          // SendCurrentTimeRequest's response will set it true, if IB is realy connected
                    ibClient.Connect(IBDataSource.IBConfiguration.Host, IBDataSource.IBConfiguration.Port, ibClientId);
                    LogAndMessage.Log(MessageType.Info, "TWS connected. Server version:" + ibClient.ServerVersion);

                    // Note:
                    // plugin is considered connected only after reqCurrenTime request got a response from tws
                }
            }
            catch (System.Net.Sockets.SocketException)
            {
                if (!autoReconnect)
                    LogAndMessage.LogAndQueue(MessageType.Error, "Cannot connect to TWS: Cannot connect to IP address and port. Check plugin configuration!");

                ibClient.Dispose();
                ibClient = null;
                isIBConnected = false;
            }
            catch (Exception ex)
            {
                if (!autoReconnect)
                    LogAndMessage.LogAndQueue(MessageType.Error, "Cannot connect to TWS:" + ex);

                ibClient.Dispose();
                ibClient = null;
                isIBConnected = false;
            }
        }

        /// <summary>
        /// disconnect from TWS API
        /// </summary>
        internal void Disconnect()
        {
            try
            {
                isIBConnected = false;

                //
                // clear up queues and stop running tasks
                //

                scheduler.ResetAllRequests();

                //
                // dispose IBClient and log disconnect event
                //

                if (ibClient != null)
                {
                    ibClient.Dispose();
                    ibClient = null;
                    LogAndMessage.Log(MessageType.Info, "TWS disconnected.");
                }
                else
                    LogAndMessage.Log(MessageType.Info, "TWS is already disconnected.");
            }
            catch (Exception ex)
            {
                LogAndMessage.Log(MessageType.Error, "Failed to disconnect from TWS gracefully:" + ex);

                if (ibClient != null)
                {
                    ibClient.Dispose();
                    ibClient = null;
                }
                isIBConnected = false;
            }
        }

        void IDisposable.Dispose()
        {
            if (scheduler != null)
            {
                scheduler.ResetAllRequests();
                scheduler.Stop();
            }

            if (ibClient != null)
                ibClient.Dispose();
        }

        internal bool IsIbConnected()
        {
            return ibClient != null
                && ibClient.IsConnected // IBClient is connected to TWS
                && isIBConnected;       // TWS is connected to IB servers
        }

        internal IBPluginState GetIBPluginState()
        {
            if (!IsIbConnected())
                return IBPluginState.Disconnected;

            else if (scheduler.IsBusy)
                return IBPluginState.Busy;

            else
                return IBPluginState.Ready;
        }

        internal string GetFailedTickers()
        {
            return failedTickers;
        }

        internal void RestartAfterReconnect(DateTime refreshStartDate)
        {
            CancelAllRefreshes();

            tickers.ResetReqMktDataMappings();

            RestartStreaming = false;

            TickerData[] allTickers = tickers.GetAllTickers();
            for (int i = 0; i <= allTickers.GetLength(0) - 1; i++)
            {
                TickerData tickerData = allTickers[i];

                if (!tickerData.IsValid)
                    continue;

                // if ticker is used (quotes or RT Window)
                if (tickerData.QuoteDataStatus != QuotationStatus.Offline || tickerData.RealTimeWindowStatus)
                {
                    scheduler.QueueContractRequest(tickerData);
                    scheduler.QueueSymbolUpdateRequest(tickerData);
                }

                if (tickerData.RealTimeWindowStatus)
                    scheduler.QueueSubscriptionRequest(tickerData);

                if (tickerData.QuoteDataStatus != QuotationStatus.Offline)
                    scheduler.QueueBackfillRequest(tickerData, IBDataSource.IBConfiguration.BadTickFilter, refreshStartDate);
            }
        }

        internal void StartContractRefresh()
        {
            DateTime now = DateTime.Now;

            if (nextUpdateOfContracts < now)
            {
                nextUpdateOfContracts = now.AddHours(6);

                TickerData[] allTickers = tickers.GetAllTickers();
                for (int i = 0; i <= allTickers.GetLength(0) - 1; i++)
                    scheduler.QueueContractRequest(allTickers[i]);
            }
        }

        internal void UpdateFailedTickers()
        {
            DateTime now = DateTime.Now;

            if (nextUpdateOfFailedTickers < now)
            {
                nextUpdateOfFailedTickers = DateTime.Now.AddSeconds(3);

                StringBuilder stringBuilder = new StringBuilder(500);

                foreach (TickerData tickerData in tickers.GetAllTickers())
                {
                    if (tickerData.ContractStatus == ContractStatus.Failed
                     || tickerData.SymbolStatus == SymbolStatus.Failed
                     || tickerData.QuoteDataStatus == QuotationStatus.Failed)
                    {
                        if (stringBuilder.Length > 0)
                            stringBuilder.Append(", ");

                        stringBuilder.Append(tickerData.Ticker);
                    }
                }

                failedTickers = stringBuilder.ToString();
            }
        }

        #endregion

        #region AmiBroker API calls

        internal void GetQuotesEx(string ticker, ref QuotationArray quotes)
        {
            TickerData tickerData = tickers.RegisterTickerData(ticker);

            lock (tickerData) // no real need for this. very first call. In these 3 cases no thread is working on this tickerdata (unless ticker is in RT window?).
            {
                // if StockInfo of ticker is not saved yet (this is the first time this method is called for the ticker)
                if (tickerData.StockInfo == null)
                {
                    tickerData.StockInfo = quotes.StockInfo;

                    // if contract data is available (Watchlist refresh) but AB symbol not updated yet because this is the first call of GetQuotesEx (see ProcessContractDetailsRequests)
                    if (tickerData.ContractStatus == ContractStatus.Ok && IBDataSource.IBConfiguration.SymbolUpdate)
                        scheduler.QueueSymbolUpdateRequest(tickerData);
                }

                // if failed ticker ...
                if (tickerData.QuoteDataStatus == QuotationStatus.Failed)
                    return;

                // if this is the first time this method is called for the ticker and the ticker is not known yet
                if (tickerData.QuoteDataStatus == QuotationStatus.Offline)
                {
                    scheduler.QueueContractRequest(tickerData);
                    // if no need for this data
                    if (IBDataSource.AllowMixedEODIntra || IBDataSource.Periodicity == Periodicity.EndOfDay)
                        scheduler.QueueHeadTimestampRequest(tickerData);
                    scheduler.QueueSymbolUpdateRequest(tickerData);
                    DateTime refreshStartDate = GetBackfillStartDate(tickerData, quotes);
                    refreshStartDate = IBClientHelper.GetAdjustedStartDate(refreshStartDate, IBDataSource.Periodicity, DateTime.MinValue, true);
                    scheduler.QueueBackfillRequest(tickerData, IBDataSource.IBConfiguration.BadTickFilter, refreshStartDate);

                    return;
                }
            }

            try
            {
                lock (tickerData.Quotes)
                {
                    #region backadjusting already stored quotes (quotationarray) of contuinuous ticker

                    // reduce the number of "old quotes" (reduce backadjusting work)
                    if (tickerData.SymbolParts.IsContinuous)
                        quotes.Free(tickerData.Quotes.Count);

                    // if continuous AND quotationArray has gata and dnew quote data is downloaded
                    if (tickerData.SymbolParts.IsContinuous && quotes.Count > 0 && tickerData.Quotes.Count > 0)
                    {
                        int index;
                        for (index = 0; index < quotes.Count; index++)
                        {
                            if (quotes[index].DateTime.Date >= tickerData.Quotes[0].DateTime.Date)
                                break;
                        }

                        if (quotes[index].DateTime.Date != tickerData.Quotes[0].DateTime.Date)
                            quotes.Clear();         // TODO: futher check ... backfill made a gap...
                        else
                        {
                            float closeOfOlder = quotes[index].Price;
                            float closeOfNewer = tickerData.Quotes[0].Price;
                            double priceMult = closeOfNewer / closeOfOlder;

                            if (priceMult != 1f)
                            {
                                LogAndMessage.Log(tickerData, MessageType.Info, "Back-adjusting already stored quotes upto " + quotes[index].DateTime.ToString());

                                for (index--; index >= 0; index--)
                                {
                                    Quotation quote = quotes[index];

                                    quote.Open = (float)(quote.Open * priceMult);
                                    quote.High = (float)(quote.High * priceMult);
                                    quote.Low = (float)(quote.Low * priceMult);
                                    quote.Price = (float)(quote.Price * priceMult);

                                    quotes[index] = quote;
                                }
                            }
                        }
                    }

                    #endregion

                    // merge quotes into AB's QuotationArray (even while backfilling)
                    quotes.Merge(tickerData.Quotes);

                    if (tickerData.QuoteDataStatus == QuotationStatus.Online)
                        // remove excess quotes from quote list to save memory
                        if (tickerData.Quotes.Count > 4)
                            tickerData.Quotes.RemoveRange(0, tickerData.Quotes.Count - 4);
                }
            }
            catch (Exception ex)
            {
                LogAndMessage.Log(tickerData, MessageType.Error, "Error while merging quotes: " + ex);
            }
            finally
            {
            }
        }

        /// <summary>
        /// Calc the start data of backfill
        /// </summary>
        /// <param name="quotes"></param>
        /// <returns></returns>
        private DateTime GetBackfillStartDate(TickerData tickerData, QuotationArray quotes)
        {
            DateTime refreshStart;

            // if there are no quotes yet
            if (quotes.Count == 0)
            {
                // we download data of ca. 2 requests
                refreshStart = DateTime.Now.AddMinutes(-IBClientHelper.GetDefaultDownloadPeriod(IBDataSource.Periodicity));
            }

            // if database already has some quotes we backfill continuously unless...
            else
            {
                //
                // calc the number of quotes to backfilled up to now
                //

                int lastIndex = quotes.Count - 1;

                // if mixed quotes we need the last intraday quote
                if (IBDataSource.Periodicity != Periodicity.EndOfDay && IBDataSource.AllowMixedEODIntra)
                {
                    for (; lastIndex >= 0; lastIndex--)
                        if (!quotes[lastIndex].DateTime.IsEod)
                            break;
                }

                DateTime lastQuoteDate = (DateTime)quotes[lastIndex].DateTime;
                TimeSpan intervalToBackfill = DateTime.Now.Subtract(lastQuoteDate);

                int quotesToDownload = 0;
                if (IBDataSource.Periodicity == Periodicity.EndOfDay)
                    quotesToDownload = (int)((intervalToBackfill.TotalDays + 1) * 5 / 7);       // a week has only 5 working days
                else if (tickerData.SymbolParts.SecurityType == "CASH")
                    quotesToDownload = (int)(intervalToBackfill.TotalSeconds / (int)IBDataSource.Periodicity * 5.25 / 7);   // intraday, cash market may be open for 24 hours 5 days a week and part of sunday !!!
                else
                    quotesToDownload = (int)(intervalToBackfill.TotalSeconds / (int)IBDataSource.Periodicity * 5 / 7 / 3);  // intraday, market may be open for 8 hours 5 days a week 

                // if db cannot accomodate the number of quotes to download we need to reset the quotes array and refresh default period
                if (quotesToDownload > quotes.Length)
                {
                    quotes.Clear();
                    refreshStart = DateTime.Now.AddMinutes(-IBClientHelper.GetDefaultDownloadPeriod(IBDataSource.Periodicity));
                }

                else
                {
                    // the "single request" start date of backfill
                    int downloadstep = IBClientHelper.GetDownloadStep(IBDataSource.Periodicity);
                    DateTime suggestedStart = DateTime.Now.AddMinutes(-downloadstep);

                    // the start date from the last quote
                    refreshStart = lastQuoteDate.Date;

                    if (refreshStart > suggestedStart)
                        refreshStart = suggestedStart;
                }
            }

            return refreshStart;
        }

        internal void GetRecentInfo(string ticker)
        {
            TickerData tickerData = tickers.RegisterTickerData(ticker);

            // if invalid ticker ...
            if (!tickerData.IsValid)
                return;

            scheduler.QueueContractRequest(tickerData);
            scheduler.QueueSymbolUpdateRequest(tickerData);
            scheduler.QueueSubscriptionRequest(tickerData);
        }

        // to prevent AFL engine to report AFL method call failure it will always return at least a Null value!
        internal AmiVar GetExtraData(string ticker, string name, Periodicity periodicity, int arraySize)
        {
            if (string.IsNullOrEmpty(properties))
            {
                properties = CollectProperties(typeof(TickerData), "");
                properties = "Help,IsIBConnected," + properties.Substring(0, properties.Length - 1);
            }

            // if wrong input data
            if (string.IsNullOrEmpty(name))
                return new AmiVar(ATFloat.Null);

            if (string.Compare(name, "Help", true) == 0)
                return new AmiVar(properties);

            if (string.Compare(name, "IsIBConnected", true) == 0)
                return new AmiVar(isIBConnected ? 1f : 0f);

            TickerData tickerData = tickers.GetTickerData(ticker);

            // if wrong input data, new ticker, data is not available, invalid ticker ...
            if (tickerData == null)
                return new AmiVar(ATFloat.Null);

            try
            {
                string[] parts = name.Split('.');
                object property = tickerData;
                Type type;

                for (int i = 0; i < parts.GetLength(0); i++)
                {
                    type = property.GetType();
                    property = type.InvokeMember(parts[i], BindingFlags.Public | BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.Instance, null, property, null);
                    if (property == null && i < parts.GetLength(0) - 1)
                    {
                        LogAndMessage.Log(tickerData, MessageType.Trace, "Extra data field is not yet initialized: " + name);
                        return new AmiVar(ATFloat.Null);
                    }

                    type = null;
                }

                if (property == null)           // it was a string or an object
                    return new AmiVar("");

                Type valType = property.GetType();
                if (valType == typeof(int))
                    return new AmiVar((int)property);

                if (valType.BaseType == typeof(System.Enum))
                    return new AmiVar((float)((int)property));

                if (valType == typeof(float))
                    return new AmiVar((float)property);

                if (valType == typeof(double))
                    return new AmiVar((float)(double)property);

                if (valType == typeof(bool))
                    return new AmiVar((bool)property ? 1.0f : 0.0f);

                if (valType == typeof(string))
                    return new AmiVar((string)property);

                return new AmiVar(ATFloat.Null);
            }
            catch (MissingMethodException)
            {
                LogAndMessage.LogAndQueue(MessageType.Warning, "Extra data field does not exist: " + name);
                return new AmiVar(ATFloat.Null);
            }
            catch (Exception ex)
            {
                LogAndMessage.LogAndQueue(MessageType.Error, "Failed to get extra data: " + ex);
                return new AmiVar(ATFloat.Null);
            }
        }

        private string CollectProperties(Type type, string prefix)
        {
            string p = "";

            if (string.IsNullOrEmpty(prefix))
                prefix = "";
            else
                prefix += ".";

            PropertyInfo[] pis = type.GetProperties(BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo pi in pis)
                if (pi.CanRead)
                    if (!pi.PropertyType.Namespace.StartsWith("System") && !pi.PropertyType.IsEnum)
                        p += CollectProperties(pi.PropertyType, prefix + pi.Name);
                    else if (pi.Name != "Notes")        // Notes causes an exception !!!
                        p += prefix + pi.Name + ",";

            pis = null;

            FieldInfo[] fis = type.GetFields(BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance);

            foreach (FieldInfo fi in fis)
                if (!fi.FieldType.Namespace.StartsWith("System") && !fi.FieldType.IsEnum)
                    p += CollectProperties(fi.FieldType, prefix + fi.Name);
                else
                    p += prefix + fi.Name + ",";

            fis = null;

            return p;
        }

        internal bool IsBackfillComplete(string ticker)
        {
            TickerData tickerData = tickers.RegisterTickerData(ticker);

            return tickerData.QuoteDataStatus == QuotationStatus.Online
                || tickerData.QuoteDataStatus == QuotationStatus.Failed;
        }

        #endregion

        #region Menu and auto refresh

        /// <summary>
        /// Refresh a ticker's historical quote data from a specified date
        /// </summary>
        /// <param name="ticker"></param>
        /// <param name="refreshStartDate"></param>
        /// <returns></returns>
        internal bool RefreshTicker(string ticker, DateTime refreshStartDate)
        {
            TickerData tickerData = tickers.RegisterTickerData(ticker);

            // if ticker is not in backfillQueue
            if (tickerData.QuoteDataStatus == QuotationStatus.Offline || tickerData.QuoteDataStatus == QuotationStatus.Online || tickerData.QuoteDataStatus == QuotationStatus.Failed)
            {
                scheduler.QueueContractRequest(tickerData);
                scheduler.QueueBackfillRequest(tickerData, IBDataSource.IBConfiguration.BadTickFilter, refreshStartDate);

                return true;
            }

            lock (tickerData) // UI
            {
                // if ticker is already in backfillQueue but not yet being refreshed
                if (tickerData.QuoteDataStatus == QuotationStatus.New && tickerData.RefreshStartDate < refreshStartDate)
                {
                    // update refresh start date to earlier date if needed
                    tickerData.RefreshStartDate = refreshStartDate;
                    return true;
                }

                // ticker is being refreshed
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Cancels  current quote refresh requests, not processed tickers get Failed status
        /// </summary>
        internal void CancelAllRefreshes()
        {
            scheduler.ResetBackfillRequests();

            TickerData[] allTickers = tickers.GetAllTickers();
            for (int i = 0; i <= allTickers.GetLength(0) - 1; i++)
            {
                TickerData tickerData = allTickers[i];

                lock (tickerData) // UI
                {
                    if (tickerData.QuoteDataStatus != QuotationStatus.Online && tickerData.QuoteDataStatus != QuotationStatus.Offline)
                        tickerData.QuoteDataStatus = QuotationStatus.Failed;
                }
            }
        }

        /// <summary>
        /// Refresh all currently used (known by the IB data plugin) tickers
        /// </summary>
        /// <param name="refreshStartDate"></param>
        internal void RefreshAllUsed(DateTime refreshStartDate)
        {
            CancelAllRefreshes();

            int interval = IBClientHelper.GetDownloadStep(IBDataSource.Periodicity);
            DateTime maxRefreshStartDate = DateTime.Now.AddMinutes(-interval);          // we cannot download shorter period....

            refreshStartDate = maxRefreshStartDate < refreshStartDate ? maxRefreshStartDate : refreshStartDate;

            TickerData[] allTickers = tickers.GetAllTickers();
            for (int i = 0; i <= allTickers.GetLength(0) - 1; i++)
            {
                TickerData tickerData = allTickers[i];

                lock (tickerData) // UI
                {
                    if (tickerData.QuoteDataStatus == QuotationStatus.Online || tickerData.QuoteDataStatus == QuotationStatus.Failed)
                        scheduler.QueueBackfillRequest(tickerData, IBDataSource.IBConfiguration.BadTickFilter, refreshStartDate);
                }
            }
        }

        internal void SearchContract(Contract contract)
        {
            searchReqId = IBClientHelper.GetNextReqId();
            searchContractCnt = 0;

            LogAndMessage.Log(MessageType.Trace, "Search for symbol (" + searchReqId + "): " + contract.Symbol + "/" + contract.SecType);

            SendContractDetailsRequest(searchReqId, contract);
        }

        internal TickerData GetTickerData(string ticker)
        {
            return tickers.GetTickerData(ticker);
        }

        internal int GetNumberOfTickers()
        {
            return tickers.Count;
        }

        /// <summary>
        /// Update the symbol information data in AB
        /// </summary>
        /// <param name="ticker"></param>
        internal void UpdateSymbolInfo(string ticker)
        {
            TickerData tickerData = tickers.RegisterTickerData(ticker);

            scheduler.QueueContractRequest(tickerData);
            scheduler.QueueSymbolUpdateRequest(tickerData);
        }

        #endregion

        #region IBClient requests sent by the plugin and their event handlers

        #region Current time

        /// <summary>
        /// Sends current time request if allowed
        /// </summary>
        /// <param name="allowNewRequest"></param>
        /// <returns>false, if a request was sent</returns>
        internal bool SendCurrentTimeRequest()
        {
            DateTime now = DateTime.Now;

            // periodically sync local time with IB server and check IB availability(!)
            if (nextUpdateOfCurrentTime < now)
            {
                nextUpdateOfCurrentTime = now.AddSeconds(15);

                scheduler.ReqisterGeneralRequest();
                ibClient.RequestCurrentTime();

                return false;
            }

            return true;
        }

        /// <summary>
        /// Process the receive current time
        /// </summary>
        /// <param name="time"></param>
        internal void ProcessCurrentTimeEvent(DateTime time)
        {
            isIBConnected = true;
            serverTimeCorr = new TimeSpan((time.TimeOfDay - DateTime.UtcNow.TimeOfDay).Ticks);
        }

        #endregion

        #region Contract details

        internal void SendContractDetailsRequest(int requestId, Contract contract)
        {
            scheduler.ReqisterGeneralRequest();
            ibClient.RequestContractDetails(requestId, contract);
        }

        private SortedList<int, List<ContractDetails>> cdTempResults = new SortedList<int, List<ContractDetails>>();

        internal void ProcessContractDetailsEvent(int reqId, ContractDetails contractDetails)
        {
            if (searchReqId == reqId)
            {
                searchContractCnt++;

                if (OnContractReady != null)
                    OnContractReady(contractDetails);

                return;
            }

            try
            {
                List<ContractDetails> list = null;
                if (!cdTempResults.TryGetValue(reqId, out list))
                {
                    list = new List<ContractDetails>();
                    cdTempResults.Add(reqId, list);
                }
                list.Add(contractDetails);
            }
            catch (Exception ex)
            {
                LogAndMessage.Log(MessageType.Error, "Contract details event failed: " + ex);
            }
        }

        internal void ProcessContractDetailsEndEvent(int reqId)
        {
            // if search form was used
            if (searchReqId == reqId)
            {
                LogAndMessage.Log(MessageType.Trace, "Search for symbol (" + reqId + ") ends. " + searchContractCnt);

                searchReqId = 0;
                searchContractCnt = 0;

                if (OnContractListReady != null)
                    OnContractListReady();

                return;
            }

            List<ContractDetails> list = null;
            if (!cdTempResults.TryGetValue(reqId, out list))
            {
                LogAndMessage.Log(MessageType.Trace, "Unknow contract details data request (1). reqId: " + reqId);
                return;
            }

            cdTempResults.Remove(reqId);

            ContractRequest request = scheduler.TryGetForContractRequest(reqId);
            // if response is too late
            if (request == null)
            {
                LogAndMessage.Log(MessageType.Trace, "Unknow contract details data request (2). reqId: " + reqId);
                return;
            }

            request.ContractDetailsReceived(list);
        }

        #endregion

        #region Head timestamp

        internal void SendHeadTimestampRequest(int requestId, TickerData tickerData)
        {
            scheduler.ReqisterGeneralRequest();
            ibClient.RequestHeadTimestamp(requestId,
                tickerData.ContractDetails.Contract,
                IBHistoricalDataType.GetIBHistoricalDataType(tickerData.SymbolParts.DataType),
                IBDataSource.RthOnly);
        }

        internal void ProcessHeadTimestampEvent(int reqId, string headTimestamp)
        {
            HeadTimestampRequest request = scheduler.TryGetForHeadTimestampRequest(reqId);

            // if response is too late
            if (request == null)
                return;

            request.HeadTimestampReceived(headTimestamp);
        }

        #endregion

        #region Subscription

        internal void SendSubscriptionRequest(int reqId, TickerData tickerData, bool restart)
        {
            // check if the same IB contract is already subscribed (with different AB symbol)
            int regMktDataId = tickers.GetReqMktDataForContract(tickerData.SymbolParts.NormalizedTicker);

            // if there is a working request for the same contract
            if (regMktDataId > 0)
            {    // if need to restart subscription to get OHL values for RI
                if (restart)
                {
                    int newReqMktDataId = IBClientHelper.GetNextReqId();

                    scheduler.ReqisterGeneralRequest();
                    ibClient.CancelMarketData(regMktDataId);

                    tickers.ReregisterReqMktData(regMktDataId, newReqMktDataId);

                    scheduler.ReqisterGeneralRequest();
                    ibClient.RequestMarketData(newReqMktDataId, tickerData.ContractDetails.Contract, tickerData.SymbolParts.DataType);

                    LogAndMessage.Log(tickerData, MessageType.Info, "Streaming data subscription restarted.");
                }
                // if there is a working request for the same contract, and no need to restart
                else
                {
                    // assign the reqId to this tickerData (same contract but different AB Symbol/data type)
                    tickers.RegisterReqMktData(regMktDataId, tickerData);

                    LogAndMessage.Log(tickerData, MessageType.Info, "Streaming data subscription reused.");
                }
            }

            // IB contract is not subscribed yet, we need to send new request
            else
            {
                // if contract has expiry
                if (IBClientHelper.IsContractExpired(tickerData.ContractDetails.Contract))
                {
                    LogAndMessage.Log(tickerData, MessageType.Info, "Expired contract. Streaming data subscription is not started.");
                    return;
                }

                // get a new reqId if needed (after historicaldata)
                if (reqId <= 0)
                    reqId = IBClientHelper.GetNextReqId();

                // assign the reqId
                tickers.RegisterReqMktData(reqId, tickerData);

                scheduler.ReqisterGeneralRequest();
                ibClient.RequestMarketData(reqId, tickerData.ContractDetails.Contract, tickerData.SymbolParts.DataType);

                LogAndMessage.Log(tickerData, MessageType.Info, "Streaming data subscription started.");
            }
        }

        internal void ibClient_TickPrice(int tickerId, int tickType, float price)
        {
            DateTime now = DateTime.Now + serverTimeCorr;
            int lastTickDate = now.Year * 10000 + now.Month * 100 + now.Day;
            int lastTickTime = now.Hour * 10000 + now.Minute * 100 + now.Second;

            List<TickerData> tickerDatas = tickers.GetTickerDataForReqMktData(tickerId);
            if (tickerDatas == null)
                return;

            foreach (TickerData tickerData in tickerDatas)
            {
                // save tick data for ticker's quotation update
                if (tickerData.QuoteDataStatus == QuotationStatus.Online)
                {
                    tickerData.Filter.MergePrice(tickerId, tickType, price, now);
                    tickerData.LastTickDate = lastTickDate;
                    tickerData.LastTickTime = lastTickTime;
                }

                // update recentinfo of ticker
                if (tickerData.RealTimeWindowStatus && price > 0)
                {
                    switch (tickType)
                    {
                        case TickType.ASK:

                            tickerData.RealTimeWindow.Ask = price;
                            tickerData.RealTimeWindow.Bitmap |= RecentInfoField.Ask;
                            tickerData.RealTimeWindow.Status = RecentInfoStatus.Update | RecentInfoStatus.BidAsk;

                            if (tickerData.SymbolParts.SecurityType == "CASH")
                            {
                                // in case of cash Last Price is the midprice (like in TWS)
                                tickerData.RealTimeWindow.Last = (tickerData.RealTimeWindow.Bid + tickerData.RealTimeWindow.Ask) / 2;
                                tickerData.RealTimeWindow.Bitmap |= RecentInfoField.Last;

                                // in case of cash change is the diff between MID and yesterday's close (like in TWS)
                                if (tickerData.RealTimeWindow.Prev != 0.0)
                                    tickerData.RealTimeWindow.Change = tickerData.RealTimeWindow.Last - tickerData.RealTimeWindow.Prev;
                            }

                            break;

                        case TickType.BID:

                            tickerData.RealTimeWindow.Bid = price;
                            tickerData.RealTimeWindow.Bitmap |= RecentInfoField.Bid;
                            tickerData.RealTimeWindow.Status = RecentInfoStatus.Update | RecentInfoStatus.BidAsk;

                            if (tickerData.SymbolParts.SecurityType == "CASH")
                            {
                                // in case of cash Last Price is the midprice (like in TWS)
                                tickerData.RealTimeWindow.Last = (tickerData.RealTimeWindow.Bid + tickerData.RealTimeWindow.Ask) / 2;
                                tickerData.RealTimeWindow.Bitmap |= RecentInfoField.Last;

                                // in case of cash change is the diff between MID and yesterday's close (like in TWS)
                                if (tickerData.RealTimeWindow.Prev != 0.0)
                                    tickerData.RealTimeWindow.Change = tickerData.RealTimeWindow.Last - tickerData.RealTimeWindow.Prev;
                            }

                            break;

                        case TickType.OPEN:

                            tickerData.RealTimeWindow.Open = price;
                            tickerData.RealTimeWindow.Bitmap |= RecentInfoField.Open;

                            break;

                        case TickType.HIGH:

                            tickerData.RealTimeWindow.High = price;
                            tickerData.RealTimeWindow.Bitmap |= RecentInfoField.HighLow;

                            break;

                        case TickType.LOW:

                            tickerData.RealTimeWindow.Low = price;
                            tickerData.RealTimeWindow.Bitmap |= RecentInfoField.HighLow;

                            break;

                        case TickType.CLOSE:

                            // set last session's close price and change
                            tickerData.RealTimeWindow.Prev = price;
                            tickerData.RealTimeWindow.Change = 0.0f;
                            tickerData.RealTimeWindow.Bitmap |= RecentInfoField.PrevChange | RecentInfoField.Last;

                            // if last trade price is not set yet, we need to set it because if market is closed it will not be set
                            if (tickerData.RealTimeWindow.Last == 0.0f)
                                tickerData.RealTimeWindow.Last = price;

                            break;

                        case TickType.LAST:

                            // NOTE: 
                            // AB creates "Trade" in T&S window on its own when LastPrice is changed.
                            // LastPrice is only one part of one trade event, it still needs a LastSize event!
                            tickerData.LastPrice = price;

                            break;

                        case TickType.HIGH_52_WEEK:

                            tickerData.RealTimeWindow.Week52High = price;
                            tickerData.RealTimeWindow.Bitmap |= RecentInfoField.Week52;

                            break;

                        case TickType.LOW_52_WEEK:

                            tickerData.RealTimeWindow.Week52Low = price;
                            tickerData.RealTimeWindow.Bitmap |= RecentInfoField.Week52;

                            break;

                        default:

                            break;
                    }

                    // indicate to wait for backfill
                    if (tickerData.QuoteDataStatus != QuotationStatus.Online)
                        tickerData.RealTimeWindow.Status |= RecentInfoStatus.Incomplete;

                    tickerData.RealTimeWindow.DateChange = lastTickDate;
                    tickerData.RealTimeWindow.TimeChange = lastTickTime;
                    tickerData.RealTimeWindow.DateUpdate = lastTickDate;
                    tickerData.RealTimeWindow.TimeUpdate = lastTickTime;

                    // notify AB of the change
                    DataSourceBase.NotifyRecentInfoUpdate(tickerData.Ticker, ref tickerData.RealTimeWindow);
                }
            }
        }

        internal void ibClient_TickSize(int tickerId, int tickType, int size)
        {
            DateTime now = DateTime.Now + serverTimeCorr;
            int tickDate = now.Year * 10000 + now.Month * 100 + now.Day;
            int tickTime = now.Hour * 10000 + now.Minute * 100 + now.Second;

            List<TickerData> tickerDatas = tickers.GetTickerDataForReqMktData(tickerId);
            if (tickerDatas == null)
                return;

            foreach (TickerData tickerData in tickerDatas)
            {
                // save tick data for ticker's quotation update
                if (tickerData.QuoteDataStatus == QuotationStatus.Online && (tickType == TickType.VOLUME || tickType == TickType.LAST_SIZE))
                {
                    tickerData.Filter.MergeVolume(tickerId, tickType, size, now);
                    tickerData.LastTickDate = tickDate;
                    tickerData.LastTickTime = tickTime;
                }

                // update recentinfo of ticker
                if (tickerData.RealTimeWindowStatus)
                {
                    switch (tickType)
                    {
                        case TickType.ASK_SIZE:

                            tickerData.RealTimeWindow.AskSize = size;
                            tickerData.RealTimeWindow.Status = RecentInfoStatus.Update | RecentInfoStatus.BidAsk;

                            break;

                        case TickType.BID_SIZE:

                            tickerData.RealTimeWindow.BidSize = size;
                            tickerData.RealTimeWindow.Status = RecentInfoStatus.Update | RecentInfoStatus.BidAsk;

                            break;

                        case TickType.LAST_SIZE:

                            // NOTE: AB creates "Trade" in T&S window on its own when LastPrice is set.
                            if (size > 0)             // valid trade...
                            {
                                tickerData.LastSizeSum += size;
                                // cummulate trades (can be used for correction...)

                                tickerData.RealTimeWindow.iTradeVol = size;
                                tickerData.RealTimeWindow.TradeVol = size;
                                tickerData.RealTimeWindow.Last = tickerData.LastPrice;

                                tickerData.RealTimeWindow.Bitmap |= RecentInfoField.TradeVol | RecentInfoField.Last;
                                tickerData.RealTimeWindow.Status = RecentInfoStatus.Update | RecentInfoStatus.Trade;

                                if (tickerData.RealTimeWindow.Prev != 0.0)
                                    tickerData.RealTimeWindow.Change = tickerData.RealTimeWindow.Last - tickerData.RealTimeWindow.Prev;
                            }

                            break;

                        case TickType.VOLUME:

                            if (size != 0)
                            {
                                tickerData.LastVolume = size;
                                tickerData.RealTimeWindow.TotalVol = size;
                                tickerData.RealTimeWindow.iTotalVol = size;
                                tickerData.RealTimeWindow.Bitmap |= RecentInfoField.TotalVol;
                                tickerData.LastSizeSum = 0;
                            }

                            break;

                        default:

                            break;
                    }

                    // indicate to wait for backfill
                    if (tickerData.QuoteDataStatus != QuotationStatus.Online)
                        tickerData.RealTimeWindow.Status |= RecentInfoStatus.Incomplete;

                    tickerData.RealTimeWindow.DateChange = tickDate;
                    tickerData.RealTimeWindow.TimeChange = tickTime;
                    tickerData.RealTimeWindow.DateUpdate = tickDate;
                    tickerData.RealTimeWindow.TimeUpdate = tickTime;

                    // notify AB of the change
                    DataSourceBase.NotifyRecentInfoUpdate(tickerData.Ticker, ref tickerData.RealTimeWindow);
                }
            }
        }

        public void ibClient_TickGeneric(int tickerId, int field, float value)
        {
            DateTime now = DateTime.Now + serverTimeCorr;
            int lastTickDate = now.Year * 10000 + now.Month * 100 + now.Day;
            int lastTickTime = now.Hour * 10000 + now.Minute * 100 + now.Second;

            List<TickerData> tickerDatas = tickers.GetTickerDataForReqMktData(tickerId);
            if (tickerDatas == null)
                return;

            foreach (TickerData tickerData in tickerDatas)
            {
                // save tick data for ticker's quotation update
                if (tickerData.QuoteDataStatus == QuotationStatus.Online)
                {
                    tickerData.Filter.MergePrice(tickerId, field, value, now);
                    tickerData.LastTickDate = lastTickDate;
                    tickerData.LastTickTime = lastTickTime;
                }
            }
        }

        #endregion

        #region Backfill

        internal void SendHistoricalDataRequest(int id, Contract histReqContract, DateTime histEnd, DateTime histStart, Periodicity histPeriodicity, string showWhat)
        {
            scheduler.ReqisterHistoricalDataRequest();
            // counts as 2
            if (showWhat == IBHistoricalDataType.BidAsk)
                scheduler.ReqisterHistoricalDataRequest();

            ibClient.RequestHistoricalData(id,
                                            histReqContract,
                                            histEnd.ToUniversalTime(),
                                            IBClientHelper.ConvSecondsToIBPeriod(histStart, histEnd),
                                            IBClientHelper.ConvSecondsToIBBarSize(histPeriodicity),
                                            IBHistoricalDataType.GetIBHistoricalDataType(showWhat),
                                            IBDataSource.RthOnly);
        }

        internal void ProcessQuotesReceivedEvent(int reqId, Bar bar)
        {
            try
            {
                List<Bar> list = null;
                if (!histTempResults.TryGetValue(reqId, out list))
                {
                    list = new List<Bar>();
                    histTempResults.Add(reqId, list);
                }
                list.Add(bar);
            }
            catch (Exception ex)
            {
                LogAndMessage.Log(MessageType.Error, "Historical quote event failed: " + ex);
            }
        }

        internal void ProcessQuotesReceivedEndEvent(int reqId)
        {
            List<Bar> list = null;
            if (!histTempResults.TryGetValue(reqId, out list))
            {
                LogAndMessage.Log(MessageType.Trace, "Unknow historical data request (1). reqId: " + reqId);
                return;
            }

            histTempResults.Remove(reqId);

            HistoricalDataRequest request = scheduler.TryGetForHistoricalDataRequest(reqId);
            // if response is too late
            if (request == null)
            {
                LogAndMessage.Log(MessageType.Trace, "Unknow historical data request (2). reqId: " + reqId);
                return;
            }

            request.QuoteReceived(list);
        }

        #endregion

        #region Error and messaging

        internal void ibClient_Error(object sender, int id, int errorCode, string errorMsg)
        {
            ContractRequest contractRequest;
            HistoricalDataRequest historicalDataRequest;

            // not ticker related messages
            if (id <= 0)
            {
                ibClient_Error_General(id, errorCode, errorMsg);
            }

            // if it was a historical data request (backfillQueue)
            else if ((historicalDataRequest = scheduler.TryGetForHistoricalDataRequest(id)) != null)
            {
                ibClient_Error_HistQueue(historicalDataRequest, id, errorCode, errorMsg);
            }

            // if it was a search request
            else if (searchReqId == id)
            {
                LogAndMessage.Log(null, "Contract search failed", id, errorCode, errorMsg);
                LogAndMessage.LogAndQueue(MessageType.Warning, "Contract search failed.");

                searchReqId = 0;
                OnContractListReady();
            }

            // if it was a contract details request (contractQueue)
            else if ((contractRequest = scheduler.TryGetForContractRequest(id)) != null)
            {
                LogAndMessage.Log(contractRequest.TickerData, "Getting contract info failed. Symbol is unknown.", id, errorCode, errorMsg);
                contractRequest.TickerData.ContractStatus = ContractStatus.Failed;
            }

            // if it is related to market data subscription (subscriptionQueue)
            else if (tickers.GetTickerDataForReqMktData(id) != null)
            {
                foreach (var tickerData in tickers.GetTickerDataForReqMktData(id))
                {
                    //  354 Requested market data is not subscribed.
                    if ((int)errorCode == 354 && tickerData.QuoteDataStatus > QuotationStatus.Offline)
                    {
                        tickerData.QuoteDataStatus = QuotationStatus.Failed;
                        tickerData.RealTimeWindowStatus = false;

                        LogAndMessage.LogAndQueue(tickerData, "Subscriptions", id, errorCode, errorMsg);
                    }
                    else
                    {
                        LogAndMessage.LogAndQueue(tickerData, "Subscriptions", id, errorCode, errorMsg);
                    }
                }
            }

            else
            {
                LogAndMessage.LogAndQueue(null, "-", id, errorCode, errorMsg);
            }
        }

        // handling messages not related to any ticker
        private void ibClient_Error_General(int id, int errorCode, string errorMsg)
        {
            if (errorCode == 1100   // Connectivity between IB and TWS has been lost.
             || errorCode == 1300   // TWS socket port has been reset and this connection is being dropped. Please reconnect on the new port - < port_num >
             || errorCode == 503    // The TWS is out of date and must be upgraded.
             || errorCode == 504    // Not connected
             || errorCode == 531)   // Request Current Time - Sending error: ??? 9.72-ben már nincs???
            {
                isIBConnected = false;
                RestartStreaming = true;

                LogAndMessage.LogAndQueue(null, "General", id, errorCode, errorMsg);
            }

            // Connectivity between IB and TWS has been restored - data lost. Market and account data subscription requests must be resubmitted
            else if (errorCode == 1101)
            {
                isIBConnected = true;
                RestartStreaming = true;

                LogAndMessage.LogAndQueue(null, "General", id, errorCode, errorMsg);
            }


            else if (errorCode == 1102  // Connectivity between IB and TWS has been restored - data maintained.
                  || errorCode == 501)  // Already connected
            {
                isIBConnected = true;

                LogAndMessage.LogAndQueue(null, "General", id, errorCode, errorMsg);
            }

            // warning messages (data farm connected)
            else if (errorCode >= 2100 && errorCode <= 2200)
            {
                LogAndMessage.Log(null, "General", id, errorCode, errorMsg);
            }

            else
                LogAndMessage.LogAndQueue(null, "General", id, errorCode, errorMsg);
        }

        // handling messages not related to historical data backfills
        private void ibClient_Error_HistQueue(HistoricalDataRequest historicalDataRequest, int id, int errorCode, string errorMsg)
        {
            if (historicalDataRequest == null)
                return;

            TickerData tickerData = historicalDataRequest.TickerData;

            if (errorCode == 101                            // Max number of tickers has been reached.
             || errorCode == 165                            // Historical market Data Service query message. (Issue with the request/query...)
             || errorCode == 166                            // HMDS Expired Contract Violation.
             || errorCode == 200                            // No security definition has been found for the request.
             || errorCode == 203                            // The security <security> is not available or allowed for this account.
             || errorCode == 320                            // Server error when reading an API client request.
             || errorCode == 321                            // Server error when validating an API client request.
             || errorCode == 322                            // Server error when processing an API client request.
             || errorCode == 323                            // Server error: cause - s 
             || errorCode == 366                            // Historical market data request with this ticker id has either been cancelled or is not found. 
             || errorCode == 354)                           // Requested market data is not subscribed.
            {
                tickerData.QuoteDataStatus = QuotationStatus.Failed;
                historicalDataRequest.WaitingForResponse = false;

                LogAndMessage.LogAndQueue(tickerData, "Backfill", id, errorCode, errorMsg);
            }

            else if (errorCode == 162                       // Historical market data Service error message.
                  && errorMsg.Contains("pacing violation"))
            {
                scheduler.RegisterHstPacingViolation();
                historicalDataRequest.WaitingForResponse = false;
                LogAndMessage.LogAndQueue(tickerData, "Backfill", id, errorCode, errorMsg);
            }

            //else if (errorCode == 162 // Historical market data Service error message.
            //         && errorMsg.Contains("No market data permissions"))
            //{
            //    tickerData.QuoteDataStatus = QuotationStatus.Failed;
            //    historicalDataRequest.WaitingForResponse = false;

            //    LogAndMessage.LogAndQueue(tickerData, "Backfill", id, errorCode, errorMsg);
            //}

            else if (errorCode == 162 // Historical market data Service error message.
                   && errorMsg.Contains("HMDS query returned no data"))
            {
                historicalDataRequest.errorCode = errorCode;            // indicate missing data, so we might download data with later starting date
                tickerData.QuoteDataStatus = QuotationStatus.Failed;
                historicalDataRequest.WaitingForResponse = false;
                LogAndMessage.LogAndQueue(tickerData, "Backfill", id, errorCode, errorMsg);
            }
            //else if (errorCode == 162)
            //{
            //    tickerData.QuoteDataStatus = QuotationStatus.Failed;
            //    historicalDataRequest.WaitingForResponse = false;
            //    LogAndMessage.LogAndQueue(tickerData, "Backfill", id, errorCode, errorMsg);
            //}

            else
            {
                tickerData.QuoteDataStatus = QuotationStatus.Failed;
                historicalDataRequest.WaitingForResponse = false;
                LogAndMessage.LogAndQueue(tickerData, "Unknown backfill error:", id, errorCode, errorMsg);
            }
        }

        internal void ibclient_ConnectionClosed()
        {
            isIBConnected = false;
            RestartStreaming = true;

            // set status for all tickers
            // send empty RI structure for all tickers (clears up RT window)
            TickerData[] allTickers = tickers.GetAllTickers();
            for (int i = 0; i <= allTickers.GetLength(0) - 1; i++)
            {
                TickerData tickerData = allTickers[i];

                tickerData.QuoteDataStatus = QuotationStatus.Offline;
                tickerData.SymbolStatus = SymbolStatus.Offline;
                tickerData.ContractStatus = ContractStatus.Offline;

                if (tickerData.RealTimeWindowStatus)
                {
                    tickerData.RealTimeWindow = new RecentInfo();

                    DataSourceBase.NotifyRecentInfoUpdate(tickerData.Ticker, ref tickerData.RealTimeWindow);
                }
            }

            UpdateFailedTickers();
        }

        #endregion

        #endregion
    }
}
