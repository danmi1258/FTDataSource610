using System;

namespace AmiBroker.DataSources.IB
{
    internal class SymbolRequest : Request
    {
        internal SymbolRequest(TickerData tickerData) : base(tickerData)
        { }

        internal override bool Process(IBController ibController, bool allowNewRequest)
        {
            // if no contract received yet
            if (TickerData.ContractStatus == ContractStatus.SendRequest || TickerData.ContractStatus == ContractStatus.WaitForResponse)
                return allowNewRequest;

            // if GetQuotesEx was not yet called (stockInfo is not ready yet), symbol data cannot be updated
            if (TickerData.StockInfo == null)
            {
                IsFinished = true;
                return allowNewRequest;
            }

            // if not waiting for response 
            switch (TickerData.SymbolStatus)
            {
                // if marked to update AB symbol's data
                case SymbolStatus.WaitForContractUpdate:

                    // if no contract found
                    if (TickerData.ContractStatus != ContractStatus.Ok)
                    {
                        LogAndMessage.LogAndQueue(TickerData, MessageType.Error, "Getting contract info failed, AmiBroker symbol data cannot be updated.");

                        TickerData.SymbolStatus = SymbolStatus.Failed;

                        goto case SymbolStatus.Failed;
                    }

                    // plugin may call it when StockInfo is not available (E.g. start watchlist backfill)
                    if (TickerData.StockInfo == null)
                    {
                        LogAndMessage.Log(TickerData, MessageType.Trace, "StockInfo data is not available. AmiBroker symbol data is not updated.");
                        return false;
                    }

                    try
                    {
                        // update AB's information
                        TickerData.StockInfo.AliasName = TickerData.ContractDetails.Contract.LocalSymbol + '/' + TickerData.ContractDetails.Contract.Symbol;
                        TickerData.StockInfo.FullName = TickerData.ContractDetails.LongName;
                        TickerData.StockInfo.PointValue = TickerData.ContractDetails.PriceMagnifier;
                        TickerData.StockInfo.TickSize = (float)TickerData.ContractDetails.MinTick;
                        TickerData.StockInfo.WebId = TickerData.ContractDetails.Contract.ConId.ToString();
                        TickerData.StockInfo.Currency = TickerData.ContractDetails.Contract.Currency;

                        TickerData.SymbolStatus = SymbolStatus.Ok;

                        LogAndMessage.LogAndQueue(TickerData, MessageType.Info, "AmiBroker symbol data is updated.");
                    }
                    catch (Exception ex)
                    {
                        TickerData.SymbolStatus = SymbolStatus.Failed;

                        LogAndMessage.LogAndQueue(TickerData, MessageType.Error, "AmiBroker symbol data update failed:" + ex);
                        return false;
                    }

                    goto case SymbolStatus.Ok;

                // if new ticker
                case SymbolStatus.Offline:
                // AB symbol's data are updated
                case SymbolStatus.Ok:
                // AB symbol's data NOT updated, e.g: no contract found
                case SymbolStatus.Failed:

                    IsFinished = true;
                    return allowNewRequest;

                default:

                    TickerData.SymbolStatus = SymbolStatus.Failed;

                    IsFinished = true;
                    return allowNewRequest;
            }
        }
    }
}
