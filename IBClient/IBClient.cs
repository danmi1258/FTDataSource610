using IBApi;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace AmiBroker.DataSources.IB
{
    internal class IBClient : EWrapper
    {
        EClientSocket eClientSocket;
        EReaderSignal eReaderSignal;
        EReader eReader;
        IBController ibController;

        internal IBClient(IBController controller)
        {
            eReaderSignal = new EReaderMonitorSignal();
            eClientSocket = new EClientSocket(this, eReaderSignal);
            ibController = controller;
        }

        internal void Dispose()
        {
            if (eClientSocket != null)
                eClientSocket.eDisconnect();

            eReader = null;
            eReaderSignal = null;
            eClientSocket = null;
            ibController = null;
        }

        #region Methods used by IBController

        internal void Connect(string host, int port, int clientId)
        {
            eClientSocket.eConnect(host, port, clientId, false);

            eReader = new EReader(eClientSocket, eReaderSignal);
            eReader.Start();

            Thread thread = new Thread(new ThreadStart(ReadLoop));
            thread.IsBackground = true;
            thread.Start();

            eClientSocket.reqCurrentTime();

            eClientSocket.reqMarketDataType(2);
        }

        private void ReadLoop()
        {
            try
            {
                while (eClientSocket.IsConnected())
                {
                    eReaderSignal.waitForSignal();
                    eReader.processMsgs();
                }
            }
            catch (Exception)
            {
            }
        }

        internal void Disconnect()
        {
            eClientSocket.eDisconnect();
        }

        internal void RequestCurrentTime()
        {
            eClientSocket.reqCurrentTime();
        }

        internal void RequestContractDetails(int reqId, Contract contract)
        {
            eClientSocket.reqContractDetails(reqId, contract);
        }

        internal void RequestHeadTimestamp(int reqId, Contract contract, string whatToShow, bool useRTH)
        {
            eClientSocket.reqHeadTimestamp(reqId, contract, whatToShow, useRTH ? 1 : 0, 1);
        }

        internal void RequestHistoricalData(int reqId, Contract contract, DateTime endDateTime, string durationString, string barSizeSetting, string whatToShow, bool useRTH)
        {
            string end = endDateTime.ToUniversalTime().ToString("yyyyMMdd HH:mm:ss", CultureInfo.InvariantCulture) + " GMT";
            eClientSocket.reqHistoricalData(reqId, contract, end, durationString, barSizeSetting, whatToShow, useRTH ? 1 : 0, 1, false, null);
        }

        internal void RequestMarketData(int newReqMktDataId, Contract contract, string whatToShow)
        {
            string ticks = string.Empty;

            if (whatToShow == "IV")
                ticks = "106";
            else if (whatToShow == "HV")
                ticks = "104 ";

            eClientSocket.reqMktData(newReqMktDataId, contract, ticks, false, false, null);
        }

        internal void CancelMarketData(int regMktDataId)
        {
            eClientSocket.cancelMktData(regMktDataId);
        }

        #endregion

        #region Properties used by IBController

        internal bool IsConnected
        { get { return eClientSocket.IsConnected(); } }

        internal int ServerVersion
        { get { return eClientSocket.ServerVersion; } }

        #endregion

        #region Used TWSAPI Events (passed to IBController)

        public void error(Exception e)
        {
            // plugin gets disconnected from TWS
            if (e is System.IO.EndOfStreamException)
                return;

            // if plugin tries to connect but TWS does not responde (cannot connect to TWS port)
            if (IsConnected == false && e is System.Net.Sockets.SocketException)
                return;

            ibController.ibClient_Error(null, 0, 0, e.ToString());
        }

        public void error(string str)
        {
            ibController.ibClient_Error(null, 0, 0, str);
        }

        public void error(int id, int errorCode, string errorMsg)
        {
            ibController.ibClient_Error(null, id, errorCode, errorMsg);
        }

        public void currentTime(long time)
        {
            ibController.ProcessCurrentTimeEvent(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(time));
        }

        public void connectionClosed()
        {
            ibController.ibclient_ConnectionClosed();
        }

        public void contractDetails(int reqId, ContractDetails contractDetails)
        {
            ibController.ProcessContractDetailsEvent(reqId, contractDetails);
        }

        public void contractDetailsEnd(int reqId)
        {
            ibController.ProcessContractDetailsEndEvent(reqId);
        }

        public void tickPrice(int tickerId, int field, double price, TickAttrib attribs)
        {
            ibController.ibClient_TickPrice(tickerId, field, (float)price);
        }

        public void tickSize(int tickerId, int field, int size)
        {
            ibController.ibClient_TickSize(tickerId, field, size);
        }

        public void tickGeneric(int tickerId, int field, double value)
        {
            ibController.ibClient_TickGeneric(tickerId, field, (float)value);
        }
        
        public void headTimestamp(int reqId, string headTimestamp)
        {
            ibController.ProcessHeadTimestampEvent(reqId, headTimestamp);
        }

        public void historicalData(int reqId, Bar bar)
        {
            ibController.ProcessQuotesReceivedEvent(reqId, bar);
        }

        public void historicalDataEnd(int reqId, string start, string end)
        {
            ibController.ProcessQuotesReceivedEndEvent(reqId);
        }

        #endregion

        #region Unused TWSAPI events

        public void accountDownloadEnd(string account) { }
        public void accountSummary(int reqId, string account, string tag, string value, string currency) { }
        public void accountSummaryEnd(int reqId) { }
        public void accountUpdateMulti(int reqId, string account, string modelCode, string key, string value, string currency) { }
        public void accountUpdateMultiEnd(int reqId) { }
        public void bondContractDetails(int reqId, ContractDetails contract) { }
        public void commissionReport(CommissionReport commissionReport) { }
        public void completedOrder(Contract contract, Order order, OrderState orderState) { }
        public void completedOrdersEnd() { }
        public void connectAck() { }
        public void deltaNeutralValidation(int reqId, DeltaNeutralContract deltaNeutralContract) { }
        public void displayGroupList(int reqId, string groups) { }
        public void displayGroupUpdated(int reqId, string contractInfo) { }
        public void execDetails(int reqId, Contract contract, Execution execution) { }
        public void execDetailsEnd(int reqId) { }
        public void familyCodes(FamilyCode[] familyCodes) { }
        public void fundamentalData(int reqId, string data) { }
        public void histogramData(int reqId, HistogramEntry[] data) { }
        public void historicalDataUpdate(int reqId, Bar bar) { }
        public void historicalNews(int requestId, string time, string providerCode, string articleId, string headline) { }
        public void historicalNewsEnd(int requestId, bool hasMore) { }
        public void historicalTicks(int reqId, HistoricalTick[] ticks, bool done) { }
        public void historicalTicksBidAsk(int reqId, HistoricalTickBidAsk[] ticks, bool done) { }
        public void historicalTicksLast(int reqId, HistoricalTickLast[] ticks, bool done) { }
        public void managedAccounts(string accountsList) { }
        public void marketDataType(int reqId, int marketDataType) { }
        public void marketRule(int marketRuleId, PriceIncrement[] priceIncrements) { }
        public void mktDepthExchanges(DepthMktDataDescription[] depthMktDataDescriptions) { }
        public void newsArticle(int requestId, int articleType, string articleText) { }
        public void newsProviders(NewsProvider[] newsProviders) { }
        public void nextValidId(int orderId) { }
        public void openOrder(int orderId, Contract contract, Order order, OrderState orderState) { }
        public void openOrderEnd() { }
        public void orderBound(long orderId, int apiClientId, int apiOrderId) { }
        public void orderStatus(int orderId, string status, double filled, double remaining, double avgFillPrice, int permId, int parentId, double lastFillPrice, int clientId, string whyHeld, double mktCapPrice) { }
        public void position(string account, Contract contract, double pos, double avgCost) { }
        public void positionEnd() { }
        public void positionMulti(int reqId, string account, string modelCode, Contract contract, double pos, double avgCost) { }
        public void positionMultiEnd(int reqId) { }
        public void pnl(int reqId, double dailyPnL, double unrealizedPnL, double realizedPnL) { }
        public void pnlSingle(int reqId, int pos, double dailyPnL, double unrealizedPnL, double realizedPnL, double value) { }
        public void realtimeBar(int reqId, long time, double open, double high, double low, double close, long volume, double WAP, int count) { }
        public void receiveFA(int faDataType, string faXmlData) { }
        public void rerouteMktDataReq(int reqId, int conId, string exchange) { }
        public void rerouteMktDepthReq(int reqId, int conId, string exchange) { }
        public void scannerData(int reqId, int rank, ContractDetails contractDetails, string distance, string benchmark, string projection, string legsStr) { }
        public void scannerDataEnd(int reqId) { }
        public void scannerParameters(string xml) { }
        public void securityDefinitionOptionParameter(int reqId, string exchange, int underlyingConId, string tradingClass, string multiplier, HashSet<string> expirations, HashSet<double> strikes) { }
        public void securityDefinitionOptionParameterEnd(int reqId) { }
        public void smartComponents(int reqId, Dictionary<int, KeyValuePair<string, char>> theMap) { }
        public void softDollarTiers(int reqId, SoftDollarTier[] tiers) { }
        public void symbolSamples(int reqId, ContractDescription[] contractDescriptions) { }
        public void tickByTickAllLast(int reqId, int tickType, long time, double price, int size, TickAttribLast tickAttribLast, string exchange, string specialConditions) { }
        public void tickByTickBidAsk(int reqId, long time, double bidPrice, double askPrice, int bidSize, int askSize, TickAttribBidAsk tickAttribBidAsk) { }
        public void tickByTickMidPoint(int reqId, long time, double midPoint) { }
        public void tickEFP(int tickerId, int tickType, double basisPoints, string formattedBasisPoints, double impliedFuture, int holdDays, string futureExpiry, double dividendImpact, double dividendsToExpiry) { }
        public void tickNews(int tickerId, long timeStamp, string providerCode, string articleId, string headline, string extraData) { }
        public void tickOptionComputation(int tickerId, int field, double impliedVolatility, double delta, double optPrice, double pvDividend, double gamma, double vega, double theta, double undPrice) { }
        public void tickReqParams(int tickerId, double minTick, string bboExchange, int snapshotPermissions) { }
        public void tickSnapshotEnd(int tickerId) { }
        public void tickString(int tickerId, int field, string value) { }
        public void updateAccountTime(string timestamp) { }
        public void updateAccountValue(string key, string value, string currency, string accountName) { }
        public void updateMktDepth(int tickerId, int position, int operation, int side, double price, int size) { }
        public void updateMktDepthL2(int tickerId, int position, string marketMaker, int operation, int side, double price, int size, bool isSmartDepth) { }
        public void updateNewsBulletin(int msgId, int msgType, String message, String origExchange) { }
        public void updatePortfolio(Contract contract, double position, double marketPrice, double marketValue, double averageCost, double unrealisedPNL, double realisedPNL, string accountName) { }
        public void verifyAndAuthCompleted(bool isSuccessful, string errorText) { }
        public void verifyAndAuthMessageAPI(string apiData, string xyzChallenge) { }
        public void verifyCompleted(bool isSuccessful, string errorText) { }
        public void verifyMessageAPI(string apiData) { }

        #endregion
    }
}