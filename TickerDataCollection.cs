using System;
using System.Collections.Generic;

namespace AmiBroker.DataSources.FT
{
    /// <summary>
    /// This class helps mapping and accessing TickerData objects used by the data plugin
    /// </summary>
    /// <remarks>
    /// When AmiBroker calls the plugin using its symbol, the plugin uses mapTickerToTickerData list to map the AB symbol to TickerData.
    /// When the TickSize or TickPrice events occure plugin uses the mapReqMktDataToTickerData list to map the event's reqId to TickerData.
    /// When AmiBroker calls the plugin using a new symbol, it is registered in mapTickerToTickerData and the string representation of the IB contract is registered in mapContractToReqMktData.
    /// mapContractToReqMktData can save redundant market data subscriptions in case different tickers map to the same IB contract.
    /// </remarks>
    internal class TickerDataCollection
    {
        private Dictionary<string, TickerData> mapTickerToTickerData;
        private Dictionary<int, List<TickerData>> mapReqMktDataToTickerData;
        private Dictionary<string, int> mapContractToReqMktData;

        internal TickerDataCollection()
        {
            mapTickerToTickerData = new Dictionary<string, TickerData>(StringComparer.OrdinalIgnoreCase);
            mapReqMktDataToTickerData = new Dictionary<int, List<TickerData>>();
            mapContractToReqMktData = new Dictionary<string, int>(StringComparer.Ordinal);
        }

        #region Mapping AB symbol to TickerData

        /// <summary>
        /// This method creates a new or gets an existing TickerData object for a ticker
        /// </summary>
        /// <param name="ticker"></param>
        /// <returns></returns>
        /// <remarks>It looks for registered ticker object first and returns it. If ticker is not registered yet, it registers it and returns the new object. </remarks>
        internal TickerData RegisterTickerData(string ticker)
        {
            lock (mapTickerToTickerData)
            {
                TickerData tickerData;

                if (mapTickerToTickerData.TryGetValue(ticker, out tickerData))
                    return tickerData;

                tickerData = new TickerData();

                tickerData.Ticker = ticker;

                mapTickerToTickerData.Add(ticker, tickerData);

                SymbolParts ftTicker;
                try
                {
                    ftTicker = new SymbolParts(ticker);
                }
                catch (Exception)
                {
                    ftTicker = null;
                    tickerData.QuoteDataStatus = QuotationStatus.Failed;
                    LogAndMessage.LogAndQueue(tickerData, MessageType.Warning, "Invalid symbol.");
                }
                tickerData.SymbolParts = ftTicker;

                return tickerData;
            }
        }

        /// <summary>
        /// This method gets an existing TickerData object for an ticker
        /// </summary>
        /// <param name="ticker"></param>
        /// <returns></returns>
        /// <remarks>If ticker is not yet registered, it returns null</remarks>
        internal TickerData GetTickerData(string ticker)
        {
            TickerData result;

            lock (mapTickerToTickerData)
            {
                mapTickerToTickerData.TryGetValue(ticker, out result);
            }

            return result;
        }

        #endregion

        #region Mapping market data subscription to TickerData and contract

        /// <summary>
        /// This method registers a reqMktData's id for a tickerData
        /// </summary>
        /// <param name="reqMktDataId"></param>
        /// <param name="tickerData"></param>
        /// <remarks>
        /// More AB symbols (ticker) can be mapped to the same IB contract! This is why mapContractToReqMktData is used.
        /// </remarks>
        internal void RegisterReqMktData(int reqMktDataId, TickerData tickerData)
        {
            lock (mapReqMktDataToTickerData)
            {
                if (!mapReqMktDataToTickerData.ContainsKey(reqMktDataId))
                    mapReqMktDataToTickerData.Add(reqMktDataId, new List<TickerData>());

                if (!mapReqMktDataToTickerData[reqMktDataId].Contains(tickerData))
                    mapReqMktDataToTickerData[reqMktDataId].Add(tickerData);
            }

            string contract = tickerData.SymbolParts.NormalizedTicker;
            lock (mapContractToReqMktData)
            {
                if (!mapContractToReqMktData.ContainsKey(contract))
                    mapContractToReqMktData.Add(contract, reqMktDataId);
            }
        }

        /// <summary>
        /// Some cases we need to cancel and send a new reqMktData...
        /// </summary>
        /// <param name="reqMktDataId"></param>
        /// <param name="newReqMktDataId"></param>
        internal void ReregisterReqMktData(int reqMktDataId, int newReqMktDataId)
        {
            lock (mapReqMktDataToTickerData)
            {
                if (mapReqMktDataToTickerData.ContainsKey(reqMktDataId))
                {
                    string contract = mapReqMktDataToTickerData[reqMktDataId][0].SymbolParts.NormalizedTicker;

                    mapReqMktDataToTickerData.Add(newReqMktDataId, mapReqMktDataToTickerData[reqMktDataId]);
                    mapReqMktDataToTickerData.Remove(reqMktDataId);

                    lock (mapContractToReqMktData)
                    {
                        mapContractToReqMktData[contract] = newReqMktDataId;
                    }
                }
            }
        }

        /// <summary>
        /// Get the list of TickerData objects, that are needed to be updated by this request
        /// </summary>
        /// <param name="reqMktDataId"></param>
        /// <returns></returns>
        internal List<TickerData> GetTickerDataForReqMktData(int reqMktDataId)
        {
            List<TickerData> result;

            mapReqMktDataToTickerData.TryGetValue(reqMktDataId, out result);

            return result;
        }

        /// <summary>
        /// Check if same contract (but different ticker) already has reqMktData
        /// </summary>
        /// <param name="contract"></param>
        /// <returns></returns>
        internal int GetReqMktDataForContract(string contract)
        {
            int result = -1;

            lock (mapContractToReqMktData)
            {
                mapContractToReqMktData.TryGetValue(contract, out result);
            }

            return result;
        }

        /// <summary>
        /// Used when IB gets disconnected to reset status of tickers
        /// It forces resubmitting subscriptions
        /// </summary>
        internal void ResetReqMktDataMappings()
        {
            mapReqMktDataToTickerData.Clear();
            mapContractToReqMktData.Clear();
        }

        #endregion

        internal TickerData[] GetAllTickers()
        {
            TickerData[] result = new TickerData[mapTickerToTickerData.Count];

            mapTickerToTickerData.Values.CopyTo(result, 0);

            return result;
        }

        internal int Count
        {
            get { return mapTickerToTickerData.Count; }
        }
    }
}