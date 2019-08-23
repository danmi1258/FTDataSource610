using System;
using System.Collections.Generic;
using System.Linq;
using IBApi;
using AmiBroker.Data;

namespace AmiBroker.DataSources.IB
{
    internal class ContractRequest : Request
    {
        internal ContractRequest(TickerData tickerData) : base(tickerData)
        { }

        internal override bool Process(IBController ibController, bool allowNewRequest)
        {
            const int requestTimeoutPeriod = 10;

            lock (TickerData)  // request handling
            {
                // if not waiting for response 
                switch (TickerData.ContractStatus)
                {
                    // if marked to get contract details
                    case ContractStatus.SendRequest:

                        if (allowNewRequest)
                        {
                            LogAndMessage.Log(TickerData, MessageType.Trace, "Getting contract. " + ToString(false, LogAndMessage.VerboseLog));

                            Contract contract = new Contract();

                            contract.Exchange = TickerData.SymbolParts.Exchange;
                            contract.SecType = TickerData.SymbolParts.SecurityType;
                            contract.Currency = TickerData.SymbolParts.Currency;
                            contract.IncludeExpired = true;

                            if (TickerData.SymbolParts.IsContinuous)
                            {
                                if (!string.IsNullOrEmpty(TickerData.SymbolParts.Underlying))
                                    contract.Symbol = TickerData.SymbolParts.Underlying;
                                else
                                    contract.Symbol = TickerData.SymbolParts.Symbol;
                            }
                            else
                                contract.LocalSymbol = TickerData.SymbolParts.Symbol;

                            TickerData.contractDetailsList.Clear();
                            TickerData.ContractStatus = ContractStatus.WaitForResponse;
                            RequestTime = DateTime.Now;

                            ibController.SendContractDetailsRequest(Id, contract);
                        }

                        return false;

                    // if request is sent, but response has not arrived yet
                    // see ibclient_ContractDetails and ibclient_ContractDetailsEnd event handlers
                    case ContractStatus.WaitForResponse:

                        // if no answer in Time
                        if (RequestTime.AddSeconds(requestTimeoutPeriod) < DateTime.Now)
                        {
                            LogAndMessage.LogAndQueue(TickerData, MessageType.Error, "Getting contract info timed out, symbol is offline. " + ToString(true, LogAndMessage.VerboseLog));

                            TickerData.ContractStatus = ContractStatus.Failed;

                            goto case ContractStatus.Failed;
                        }

                        return allowNewRequest;

                    // if new, offline ticker 
                    case ContractStatus.Offline:
                        goto case ContractStatus.Failed;

                    // contract found
                    case ContractStatus.Ok:
                        goto case ContractStatus.Failed;

                    // no contract found
                    case ContractStatus.Failed:

                        IsFinished = true;
                        return allowNewRequest;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// Start processing the list of contract details in the event of ContractDetailsEnd
        /// </summary>
        internal void ContractDetailsReceived(List<ContractDetails> list)
        {
            bool result = false;

            lock (TickerData)   // event handling
            {
                result = ProcessContractDetailsList(list);

                TickerData.ContractStatus = result ? ContractStatus.Ok : ContractStatus.Failed;
            }

            if (result)
                LogAndMessage.Log(TickerData, MessageType.Info, "Contract is updated. " + ToString(true, LogAndMessage.VerboseLog));
            else
                LogAndMessage.Log(TickerData, MessageType.Error, "Failed to update contract. " + ToString(true, LogAndMessage.VerboseLog));
        }

        /// <summary>
        /// Process the list of contract details received from TWS, and selects the current front month contract
        /// </summary>
        /// <param name="TickerData"></param>
        /// <returns></returns>
        private bool ProcessContractDetailsList(List<ContractDetails> list)
        {
            try
            {
                TickerData.contractDetailsList = list;

                // if no contract found
                if (TickerData.contractDetailsList.Count == 0)
                    return false;

                // not a continuos contract there must be exactly 1 found contract
                if (TickerData.contractDetailsList.Count == 1 && !TickerData.SymbolParts.IsContinuous)
                {
                    TickerData.ContractDetails = TickerData.contractDetailsList[0];
                    return true;
                }

                // continuos contract
                if (TickerData.SymbolParts.IsContinuous)
                {
                    //
                    // only expired and the nearest expiration may remain in the list
                    //

                    // sort contract details on expiry
                    TickerData.contractDetailsList.Sort(new ContractDetailsComparer());

                    // current date in the format of contract expiry
                    string frontMonthExpiry = DateTime.Now.ToString("yyyyMMdd");

                    // this may be an already expired contract or a contract that will expire in the far future
                    ContractDetails temp = TickerData.contractDetailsList[0];

                    // find the contract that ...
                    for (int i = 1; i < TickerData.contractDetailsList.Count; i++)
                        if (TickerData.contractDetailsList[i].Contract.LastTradeDateOrContractMonth.CompareTo(frontMonthExpiry) >= 0)       // expires in the future or today
                        {
                            temp = TickerData.contractDetailsList[i];
                            break;
                        }

                    // setting the found CURRENT (front month) contract as the contractdetails
                    TickerData.ContractDetails = temp;

                    frontMonthExpiry = temp.Contract.LastTradeDateOrContractMonth;

                    // remove all future contract with later expiry then current front month
                    for (int i = TickerData.contractDetailsList.Count - 1; i >= 0; i--)
                        if (TickerData.contractDetailsList[i].Contract.LastTradeDateOrContractMonth.CompareTo(frontMonthExpiry) > 0)
                            TickerData.contractDetailsList.RemoveAt(i);

                    return true;
                }

                return false;
            }
            finally
            {
                try
                {
                    // update trading days using current contract details
                    if (TickerData.ContractDetails != null)
                    {
                        //tickerData.LiquidHours = new TradingDayList(tickerData.ContractDetails.LiquidHours, true);
                        //if (IBDataSource.RthOnly && string.IsNullOrEmpty(tickerData.ContractDetails.LiquidHours))
                        //    LogAndMessage.LogAndQueue(tickerData, MessageType.Warning, "No liquid hours data is available.");
                        //else
                        //    LogAndMessage.Log(tickerData, MessageType.Trace, "Liquid hour:" + tickerData.ContractDetails.LiquidHours);

                        TickerData.TradingDays = new TradingDayList(TickerData.ContractDetails.TradingHours, false);
                        if ((IBDataSource.AllowMixedEODIntra || IBDataSource.Periodicity == Periodicity.EndOfDay) && string.IsNullOrEmpty(TickerData.ContractDetails.TradingHours))
                            LogAndMessage.Log(TickerData, MessageType.Warning, "No trading hours data is available. Daily quotation data may not be correct.");
                        //else
                        //    LogAndMessage.Log(tickerData, MessageType.Trace, "Trading hour:" + tickerData.ContractDetails.TradingHours);
                    }
                    else
                    {
                        //tickerData.LiquidHours = new TradingDayList(null, true);
                        TickerData.TradingDays = new TradingDayList(null, false);
                    }
                }
                catch (Exception e)
                {
                    LogAndMessage.Log(TickerData, MessageType.Error, "Failed to parse (" + TickerData.ContractDetails.TradingHours + ") and update trading hours:" + e.ToString());
                }
            }
        }
    }
}
