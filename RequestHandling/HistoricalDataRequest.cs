using System;
using System.Collections.Generic;
using System.Linq;
using IBApi;
using AmiBroker.Data;
using System.Globalization;

namespace AmiBroker.DataSources.IB
{
    internal class HistoricalDataRequest : Request
    {
        internal Contract downloadContract = null;
        int downloadStep;
        int downloadInterval;
        internal int errorCode;
        internal DateTime downloadStart;
        internal DateTime downloadEnd;
        internal Periodicity downloadPeriodicity;

        internal HistoricalDataRequest(TickerData tickerData) : base(tickerData)
        { }

        internal override bool Process(IBController ibController, bool allowNewRequest)
        {
            int requestTimeoutPeriod = 75;

            // if contract of the ticker is still being retrieved or headtimestamp of the ticker is needed (not Offline) AND not yet retrieved 
            if (TickerData.ContractStatus <= ContractStatus.WaitForResponse
            || ((IBDataSource.Periodicity == Periodicity.EndOfDay || IBDataSource.AllowMixedEODIntra) && TickerData.HeadTimestampStatus <= HeadTimestampStatus.WaitForResponse))
                return allowNewRequest;

            if (TickerData.ContractStatus == ContractStatus.Failed || TickerData.ContractStatus == ContractStatus.Offline
             || TickerData.HeadTimestampStatus == HeadTimestampStatus.Failed || (TickerData.HeadTimestampStatus == HeadTimestampStatus.Offline && (IBDataSource.Periodicity == Periodicity.EndOfDay || IBDataSource.AllowMixedEODIntra)))
            {
                TickerData.QuoteDataStatus = QuotationStatus.Failed;

                IsFinished = true;
                return allowNewRequest;
            }

            lock (TickerData)   // request handling
            {
                // if reqHistoricalData is send to IB and we are waiting for answer
                if (WaitingForResponse)
                {
                    // request is not yet timed out...
                    if (RequestTime.AddSeconds(requestTimeoutPeriod) > DateTime.Now)
                        return allowNewRequest;

                    // no response arrived in time, request is timed out...
                    LogAndMessage.LogAndQueue(TickerData, MessageType.Info, "Historical data request has timed out. " + ToString(true, LogAndMessage.VerboseLog));

                    RequestTimeouts++;
                    WaitingForResponse = false;

                    // if there were too many reqHistoricalData timeouts
                    if (RequestTimeouts > 2)
                    {
                        // drop this ticker...
                        TickerData.QuoteDataStatus = QuotationStatus.Failed;

                        IsFinished = true;
                        return allowNewRequest;
                    }
                }

                // if no new request can be sent (request pacing)
                bool histThrottling = !allowNewRequest || TickerData.QuoteDataStatus > QuotationStatus.New && RequestTime.AddSeconds(6.5) > DateTime.Now;

                // process the ticker depending on its state
                switch (TickerData.QuoteDataStatus)
                {
                    case QuotationStatus.Offline:

                        LogAndMessage.Log(MessageType.Error, "Program error. Offline ticker cannot get historical update.");

                        IsFinished = true;
                        return allowNewRequest;

                    // All historical data requests are processed for the ticker
                    // (the last CalcNextHistoricalDataRequest call sets this state)
                    case QuotationStatus.DownloadedEod:

                        #region Merging and backadjusting downloaded quotes of different contracts/expiry into a simgle QuotationList of the continuous contract

                        if (TickerData.SymbolParts.IsContinuous)
                        {
                            QuotationList mergedQuotes = new QuotationList(IBDataSource.Periodicity);

                            int newQuoteIndex;

                            foreach (ContractDetails cd in TickerData.contractDetailsList)
                            {
                                // if there were no quotes receiced for this contract...
                                if (!TickerData.ContinuousQuotesDictionary.ContainsKey(cd.Contract.LocalSymbol))
                                    continue;

                                newQuoteIndex = 0;

                                if (mergedQuotes.Count > 0)
                                {
                                    int mergedQuoteIndex = mergedQuotes.Count - 1;
                                    AmiDate mergedQuoteDateTime = mergedQuotes[mergedQuoteIndex].DateTime;

                                    // move forward to the first quote not overlqapping with prev contract
                                    while (newQuoteIndex < TickerData.ContinuousQuotesDictionary[cd.Contract.LocalSymbol].Count - 1 && TickerData.ContinuousQuotesDictionary[cd.Contract.LocalSymbol][newQuoteIndex].DateTime.Date < mergedQuoteDateTime.Date)
                                    {
                                        newQuoteIndex++;
                                    }

                                    // at this point newQuoteIndex points to a quote of the "same" date as mergedQuoteDateTime (if there are quotes for the same day, if not, then the next day)

                                    // if daily database then we look for a day where volume on older contract is greater (switch over day)
                                    if (IBDataSource.Periodicity == Periodicity.EndOfDay)
                                    {
                                        // find the quote that has a lower volume
                                        while (newQuoteIndex > 0 && mergedQuoteIndex > 0
                                            && TickerData.ContinuousQuotesDictionary[cd.Contract.LocalSymbol][newQuoteIndex].DateTime.Date == mergedQuotes[mergedQuoteIndex].DateTime.Date       // quotes are of same date
                                            && TickerData.ContinuousQuotesDictionary[cd.Contract.LocalSymbol][newQuoteIndex].Volume > mergedQuotes[mergedQuoteIndex].Volume)                     // new contract's volume is higher then old contract's volume
                                        {
                                            newQuoteIndex--;
                                            mergedQuoteIndex--;
                                        }
                                        // at this point newQuoteIndex and lastQuoteDateTime point to quote at which contract is replaced
                                    }

                                    if (TickerData.ContinuousQuotesDictionary[cd.Contract.LocalSymbol][newQuoteIndex].DateTime.Date != mergedQuotes[mergedQuoteIndex].DateTime.Date)
                                        LogAndMessage.Log(MessageType.Info, TickerData.ToString(cd.Contract) + ": No overlapping quote found. Used dates to change contracts: " + mergedQuotes[mergedQuoteIndex].DateTime + " and " + TickerData.ContinuousQuotesDictionary[cd.Contract.LocalSymbol][newQuoteIndex].DateTime + ".");
                                    else
                                        LogAndMessage.Log(MessageType.Info, TickerData.ToString(cd.Contract) + ": Switching to new contract on " + mergedQuotes[mergedQuoteIndex].DateTime + ".");

                                    // get "closing prices" of the contract on the same day
                                    float closeOfNewer = TickerData.ContinuousQuotesDictionary[cd.Contract.LocalSymbol][newQuoteIndex].Price;
                                    float closeOfOlder = mergedQuotes[mergedQuoteIndex].Price;
                                    double priceMult = closeOfNewer / closeOfOlder;

                                    // back-adjust prev contracts' prices
                                    QuotationList tempList = new QuotationList(IBDataSource.Periodicity);
                                    for (int i = 0; i < mergedQuoteIndex; i++)
                                    {
                                        Quotation quote = mergedQuotes[i];
                                        quote.Open = (float)(quote.Open * priceMult);
                                        quote.High = (float)(quote.High * priceMult);
                                        quote.Low = (float)(quote.Low * priceMult);
                                        quote.Price = (float)(quote.Price * priceMult);
                                        tempList.Merge(quote);
                                    }
                                    mergedQuotes.Clear();
                                    mergedQuotes = tempList;
                                }

                                // add quotes of newer contract
                                for (; newQuoteIndex < TickerData.ContinuousQuotesDictionary[cd.Contract.LocalSymbol].Count; newQuoteIndex++)
                                    mergedQuotes.Merge(TickerData.ContinuousQuotesDictionary[cd.Contract.LocalSymbol][newQuoteIndex]);
                            }

                            TickerData.Quotes = mergedQuotes;
                        }

                        #endregion

                        // this is not THROTTLED, but counted in general throttling queue
                        ibController.SendSubscriptionRequest(0, TickerData, false);

                        TickerData.QuoteDataStatus = QuotationStatus.Online;

                        return allowNewRequest;

                    // this should never happen (ticker with online status should not be in the queue...)
                    case QuotationStatus.Online:

                        LogAndMessage.LogAndQueue(TickerData, MessageType.Info, "Backfill finished, symbol is ready. ");

                        IsFinished = true;
                        return allowNewRequest;

                    // if any error happend
                    case QuotationStatus.Failed:

                        // if intraday download received no data response
                        if (errorCode == 162 && IBDataSource.Periodicity < Periodicity.EndOfDay && IBDataSource.Periodicity > Periodicity.FifteenSeconds)
                        {
                            errorCode = 0;
                            
                            // move forward 4 periods to speed up download/find first valid period with available data
                            CalcNextBackfillRequest();
                            CalcNextBackfillRequest();
                            CalcNextBackfillRequest();
                            LogAndMessage.Log(TickerData, MessageType.Trace, "No data returned, fast forward download period.");

                            // start next download
                            TickerData.QuoteDataStatus = QuotationStatus.DownloadingIntra;

                            return allowNewRequest;
                        }
                        else
                        {
                            LogAndMessage.LogAndQueue(TickerData, MessageType.Info, "Backfill failed, symbol is offline.");

                            IsFinished = true;
                            return allowNewRequest;
                        }

                    // start historical data refresh
                    case QuotationStatus.New:

                        if (histThrottling)
                            return false;

                        // calc download properties
                        downloadPeriodicity = IBDataSource.Periodicity;
                        downloadStep = IBClientHelper.GetDownloadStep(IBDataSource.Periodicity);
                        downloadInterval = IBClientHelper.GetDownloadInterval(IBDataSource.Periodicity);
                        downloadStart = IBClientHelper.GetAdjustedStartDate(TickerData.RefreshStartDate, IBDataSource.Periodicity, GetEarliestDownloadDate(), true);
                        downloadEnd = downloadStart.AddMinutes(downloadInterval);
                        downloadContract = GetCurrentContract(downloadStart);

                        // remove quotes already stored
                        TickerData.Quotes.Clear();

                        // set next state
                        if (IBDataSource.Periodicity == Periodicity.EndOfDay)
                            TickerData.QuoteDataStatus = QuotationStatus.DownloadingEod;
                        else
                            TickerData.QuoteDataStatus = QuotationStatus.DownloadingIntra;

                        // not to wait to send next request
                        RequestTime = DateTime.MinValue;

                        // download historical data
                        SendBackfillRequest(ibController);

                        return false;

                    case QuotationStatus.DownloadingEod:
                    case QuotationStatus.DownloadingIntra:

                        if (histThrottling)
                            return false;

                        // if previous request timed out
                        if (RequestTimeouts != 0)
                            SendBackfillRequest(ibController);

                        // download historical data
                        else if (CalcNextBackfillRequest())
                            SendBackfillRequest(ibController);

                        return false;

                    // last CalcNextHistoricalDataRequest call for intraday bars should have set this state
                    case QuotationStatus.DownloadedIntra:

                        // if we need EOD data as well
                        if (IBDataSource.AllowMixedEODIntra)
                        {
                            if (histThrottling)
                                return false;

                            // calc download properties for EOD
                            downloadPeriodicity = Periodicity.EndOfDay;
                            downloadStep = IBClientHelper.GetDownloadStep(Periodicity.EndOfDay);
                            downloadInterval = IBClientHelper.GetDownloadInterval(Periodicity.EndOfDay);
                            downloadStart = IBClientHelper.GetAdjustedStartDate(TickerData.RefreshStartDate, Periodicity.EndOfDay, GetEarliestDownloadDate(), true);
                            downloadEnd = downloadStart.AddMinutes(downloadInterval);
                            downloadContract = GetCurrentContract(downloadStart);

                            SendBackfillRequest(ibController);

                            TickerData.QuoteDataStatus = QuotationStatus.DownloadingEod;
                        }
                        else
                        {
                            TickerData.QuoteDataStatus = QuotationStatus.DownloadedEod;
                        }

                        return false;

                    default:

                        LogAndMessage.LogAndQueue(TickerData, MessageType.Info, "Program error in backfill logic.");

                        IsFinished = true;
                        return true;
                }
            }
        }

        private void SendBackfillRequest(IBController ibController)
        {
            if (downloadContract != null
            // && (string.IsNullOrEmpty(contract.LastTradeDateOrContractMonth) || histStart.ToString("yyyyMMdd").CompareTo(contract.LastTradeDateOrContractMonth) <= 0)
            && (downloadStep > 24 * 60 || IBClientHelper.IsBankingHour(downloadStart) || IBClientHelper.IsBankingHour(downloadEnd)))    //check if we need to queue a requests
            {
                //try to avoid intraday request for saturday-sunday
                Id = IBClientHelper.GetNextReqId();

                // add quotelist of subcontracts of continuous contract
                if (TickerData.SymbolParts.IsContinuous)
                    if (!TickerData.ContinuousQuotesDictionary.ContainsKey(downloadContract.LocalSymbol))
                        TickerData.ContinuousQuotesDictionary.Add(downloadContract.LocalSymbol, new QuotationList(IBDataSource.Periodicity));

                if (downloadPeriodicity <= Periodicity.FifteenSeconds)      // download step is smaller than a day
                    LogAndMessage.LogAndQueue(MessageType.Info, TickerData.ToString(downloadContract) + ": Requesting data from " + downloadStart.ToShortDateString() + " " + downloadStart.ToShortTimeString() + " to " + downloadEnd.ToShortDateString() + " " + downloadEnd.ToShortTimeString() + " " + ToString(false, LogAndMessage.VerboseLog));
                else
                    LogAndMessage.LogAndQueue(MessageType.Info, TickerData.ToString(downloadContract) + ": Requesting data from " + downloadStart.ToShortDateString() + " to " + downloadEnd.ToShortDateString() + " " + ToString(false, LogAndMessage.VerboseLog));

                WaitingForResponse = true;
                RequestTime = DateTime.Now;

                ibController.SendHistoricalDataRequest(Id, downloadContract, downloadEnd, downloadStart, downloadPeriodicity, TickerData.SymbolParts.DataType);
            }
            else
            {
                // calc next download period
                if (CalcNextBackfillRequest())
                    SendBackfillRequest(ibController);
            }
        }

        /// <summary>
        /// Get the contract for the historical data request for a specified start date
        /// </summary>
        /// <param name="startDate"></param>
        /// 
        /// <returns></returns>
        private Contract GetCurrentContract(DateTime startDate)
        {
            Contract contract;

            if (TickerData.SymbolParts.IsContinuous)
            {
                string exStr = startDate.ToString("yyyyMMdd");
                int i = 0;
                for (; i < TickerData.contractDetailsList.Count - 1; i++)
                    if (TickerData.contractDetailsList[i].Contract.LastTradeDateOrContractMonth.CompareTo(exStr) >= 0)
                        break;

                contract = TickerData.contractDetailsList[i].Contract;
            }
            else
                contract = TickerData.ContractDetails.Contract;

            contract.IncludeExpired = IBClientHelper.IsContractExpired(contract);

            // TODO: Do we need it? Does not hurt...
            if (string.IsNullOrEmpty(contract.LocalSymbol))
            {
                LogAndMessage.Log(MessageType.Trace, "No valid security for continuous contract.");
                return null;
            }

            return contract;
        }

        private bool CalcNextBackfillRequest()
        {
            downloadStart = downloadStart.AddMinutes(downloadStep);

            // if contiguous contract is being backfilled
            if (TickerData.SymbolParts.IsContinuous)
            {
                DateTime currExp = IBClientHelper.GetContractExpiryDateTime(downloadContract);

                if (currExp < downloadStart)
                {
                    downloadContract = GetCurrentContract(currExp.AddDays(1));
                    downloadStart = currExp.AddMinutes(-downloadStep);
                    downloadStart = IBClientHelper.GetAdjustedStartDate(downloadStart, downloadPeriodicity, GetEarliestDownloadDate(), true);
                }
            }

            // if no more to download
            if (downloadPeriodicity < Periodicity.EndOfDay && downloadStart > DateTime.Now
             || downloadPeriodicity == Periodicity.EndOfDay && downloadStart.Date >= DateTime.Now.Date)
            {
                if (TickerData.QuoteDataStatus == QuotationStatus.DownloadingIntra)
                    TickerData.QuoteDataStatus = QuotationStatus.DownloadedIntra;
                if (TickerData.QuoteDataStatus == QuotationStatus.DownloadingEod)
                    TickerData.QuoteDataStatus = QuotationStatus.DownloadedEod;

                return false;
            }

            // set the end of the download period
            downloadEnd = downloadStart.AddMinutes(downloadInterval);

            // indicate that more request is needed
            return true;
        }

        /// <summary>
        /// Get the earliest download start date for which historical data can be requested from IB without issues...
        /// </summary>
        /// <returns></returns>
        private DateTime GetEarliestDownloadDate()
        {
            DateTime earliestDownloadDate;

            if (downloadPeriodicity == Periodicity.EndOfDay)
            {
                // if headTimestamp is available
                if (TickerData != null && TickerData.HeadTimestampStatus == HeadTimestampStatus.Ok)
                    earliestDownloadDate = TickerData.EarliestDataPoint;
                else
                    earliestDownloadDate = new DateTime(1995, 1, 1);
            }
            else
            {
                if (downloadPeriodicity > Periodicity.FifteenSeconds)
                    earliestDownloadDate = DateTime.Now.Date.AddDays(-IBClientHelper.MaxDownloadDaysOfMediumBars);      // ~ 2.5 years
                else
                    earliestDownloadDate = DateTime.Now.Date.AddDays(-IBClientHelper.MaxDownloadDaysOfSmallBars);       // ~ 180 days

                // adjust to a weekend
                if (earliestDownloadDate.DayOfWeek != 0)
                    earliestDownloadDate = earliestDownloadDate.AddDays(7 - (int)earliestDownloadDate.DayOfWeek);
            }

            return earliestDownloadDate;
        }

        /// <summary>
        /// Event handler called when all bar are collected for a request 
        /// </summary>
        /// <param name="bars"></param>
        internal void QuoteReceived(List<Bar> bars)
        {
            lock (TickerData)    // event handling 
            {
                foreach (Bar bar in bars)
                {
                    DateTime date;

                    if (bar.Time.Length == 8)
                        date = DateTime.ParseExact(bar.Time, "yyyyMMdd", CultureInfo.InvariantCulture);
                    else
                        date = DateTime.ParseExact(bar.Time, "yyyyMMdd  HH:mm:ss", CultureInfo.InvariantCulture);

                    if (downloadStart.CompareTo(date) <= 0)  // if newer data (may overlap w/ prev request)
                    {
                        Quotation quoteData = new Quotation();
                        quoteData.DateTime = (AmiDate)date;
                        quoteData.Open = (float)bar.Open;
                        quoteData.High = (float)bar.High;
                        quoteData.Low = (float)bar.Low;
                        quoteData.Price = (float)bar.Close;
                        quoteData.Volume = bar.Volume;

                        if (TickerData.SymbolParts.IsContinuous)
                        {
                            if (IBDataSource.Periodicity == Periodicity.EndOfDay || TickerData.QuoteDataStatus == QuotationStatus.DownloadingEod)
                                TickerData.ContinuousQuotesDictionary[downloadContract.LocalSymbol].MergeEod(quoteData, quoteData.DateTime);
                            else
                                TickerData.ContinuousQuotesDictionary[downloadContract.LocalSymbol].Merge(quoteData);
                        }
                        else
                        {
                            lock (TickerData.Quotes)
                            {     // store in the list directly only for non contiguous contracts
                                if (IBDataSource.Periodicity == Periodicity.EndOfDay || TickerData.QuoteDataStatus == QuotationStatus.DownloadingEod)
                                    TickerData.Quotes.MergeEod(quoteData, quoteData.DateTime);
                                else
                                    TickerData.Quotes.Merge(quoteData);
                            }
                        }
                    }
                }

                // it is finished with succes
                WaitingForResponse = false;

                LogAndMessage.Log(TickerData, MessageType.Info, "Received " + bars.Count + " bars. " + ToString(true, LogAndMessage.VerboseLog));
            }
        }
    }
}
