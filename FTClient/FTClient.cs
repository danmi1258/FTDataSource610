using Futu.OpenApi;
using Futu.OpenApi.Pb;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Google.ProtocolBuffers.Descriptors;
using System.Linq;
using AmiBroker.DataSources.FT.Events;
using System.Security.Cryptography;
using System.Text;

namespace AmiBroker.DataSources.FT
{
    class Limitation
    {
        public int maxNum = -1;
        public Dictionary<int, int> maxNumWithPri;
        public (int reqNum, int duration) freq1;
        public (int reqNum, int duration) freq2;
    }

    class RequestWithLimition
    {
        private static readonly object reqHisDataListLock = new object();

        private static readonly Limitation unLockLmt = new Limitation { freq1 = (10, 30) };
        private static readonly Limitation placeOrderLmt = new Limitation { freq1 = (15, 30), freq2 = (5, 1) };
        private static readonly Limitation modifyOrderLmt = new Limitation { freq1 = (20, 30), freq2 = (5, 1) };
        private static readonly Limitation getMaxTrdQtyLmt = new Limitation { freq1 = (10, 30) };
        private static readonly Limitation getOrderListLmt = new Limitation { freq1 = (10, 30) };
        private static readonly Limitation getOrderFillListLmt = new Limitation { freq1 = (10, 30) };
        private static readonly Limitation subRTLmt = new Limitation { maxNumWithPri = new Dictionary<int, int> { [1] = 1000, [2] = 300, [3] = 100 } };
        private static readonly Limitation getKLLmt = new Limitation { maxNum = 1000 };
        private static readonly Limitation getTickerLmt = new Limitation { maxNum = 1000 };
        private static readonly Limitation getHistoricalKLLmt = new Limitation { freq1 = (10, 30) };
        private static readonly Limitation getRehabLmt = new Limitation { freq1 = (10, 30) };

        private static Dictionary<QotRequestHistoryKL.Request.Builder, DateTime> reqHisDataList = new Dictionary<QotRequestHistoryKL.Request.Builder, DateTime>();

        public static void RequestHistoricalData(FTAPI_Qot qot, QotCommon.QotMarket market, string code, DateTime beginTime, DateTime endTime, QotCommon.KLType kLType)
        {
            QotRequestHistoryKL.Request.Builder reqBuilder = QotRequestHistoryKL.Request.CreateBuilder();
            QotRequestHistoryKL.C2S.Builder csReqBuilder = QotRequestHistoryKL.C2S.CreateBuilder();
            QotCommon.Security.Builder stock = QotCommon.Security.CreateBuilder();
            stock.SetCode(code);
            stock.SetMarket((int)market);
            csReqBuilder.Security = stock.Build();
            csReqBuilder.KlType = (int)kLType;
            csReqBuilder.BeginTime = beginTime.ToString("yyyy-MM-dd");
            csReqBuilder.EndTime = endTime.ToString("yyyy-MM-dd");
            reqBuilder.SetC2S(csReqBuilder);
            var lmt = getHistoricalKLLmt.freq1;
            foreach (var item in reqHisDataList.Where(x => (DateTime.Now - x.Value).TotalSeconds > lmt.duration).ToList())
            {
                lock (reqHisDataListLock)
                {
                    reqHisDataList.Remove(item.Key);
                }                
            }
            if (reqHisDataList.Count < lmt.reqNum)
            {
                qot.RequestHistoryKL(reqBuilder.Build());
                lock (reqHisDataListLock)
                {
                    reqHisDataList.Add(reqBuilder, DateTime.Now);
                }
            }
            else
            {

            }
        }

    }



    class ConnCallback : FTSPI_Conn
    {
        private FTClient ftClient = null;
        public ConnCallback(FTClient client)
        {
            ftClient = client;            
        }
        public void OnInitConnect(FTAPI_Conn client, long errCode, string desc)
        {
            Console.WriteLine("InitConnected");
            if (errCode == 0)
            {
                FTAPI_Qot qot = client as FTAPI_Qot;
                {
                    GetGlobalState.Request req = GetGlobalState.Request.CreateBuilder().SetC2S(GetGlobalState.C2S.CreateBuilder().SetUserID(900019)).Build();
                    uint serialNo = qot.GetGlobalState(req);
                    Console.WriteLine("Send GetGlobalState: {0}", serialNo);
                }
                ftClient.IsConnected = true;
            }
            else
            {
                ftClient.FTController.ibClient_Error(null, 0, 0, ((ConnectFailType)errCode).ToString());
            }
        }

        public void OnDisconnect(FTAPI_Conn client, long errCode)
        {            
            ftClient.FTController.ibclient_ConnectionClosed();
        }
    }

    class QotCallback : FTSPI_Qot
    {
        private FTClient ftClient = null;

        public event GlobalStateHandler OnGlobalState;
        public event SubscriptionHandler OnSubscription;
        public event RegQotPushHandler OnRegQotPush;
        public event GetSubInfoHandler OnGetSubInfo;
        public event GetTickerHandler OnGetTicker;
        public event GetBasicQotHandler OnGetBasicQot;
        public event GetOrderBookHandler OnGetOrderBook;
        public event GetKLHandler OnGetKL;
        public event GetRTHandler OnGetRT;
        public event GetBrokerHandler OnGetBroker;
        public event RequestRehabHandler OnRequestRehab;
        public event RequestHistoricalKLQuotaHandler OnHistoricalKLQuota;
        public event RequestHistoricalKLHandler OnRequestHistoricalKL;
        public event GetTradeDateHandler OnGetTradeDate;
        public event GetStaticInfoHandler OnGetStaticInfo;
        public event GetSecuritySnapshotHandler OnGetSecuritySnapshot;
        public event GetPlateSetHandler OnGetPlateSet;
        public event GetPlateSecurityHandler OnGetPlateSecurity;
        public event GetReferenceHandler OnGetReference;
        public event GetOwnerPlateHandler OnGetOwnerPlate;
        public event GetHoldingChangeListHandler OnGetHoldingChangeList;
        public event GetOptionChainHandler OnGetOptionChain;
        public event GetGetWarrantHandler OnGetGetWarrant;
        public event GetGetCapitalFlowHandler OnGetGetCapitalFlow;
        public event GetGetCapitalDistributionHandler OnGetGetCapitalDistribution;
        public event GetGetUserSecurityHandler OnGetGetUserSecurity;
        public event ModifyUserSecurityHandler OnModifyUserSecurity;
        public event NotifyHandler OnNotify;
        public event UpdateBasicQotHandler OnUpdateBasicQot;
        public event UpdateKLHandler OnUpdateKL;


        public QotCallback(FTClient client)
        {
            ftClient = client;
        }
        public void OnReply_GetGlobalState(FTAPI_Conn client, int nSerialNo, GetGlobalState.Response rsp)
        {
            Console.WriteLine("Recv GetGlobalState: {0} {1}", nSerialNo, rsp);
        }

        public void OnReply_Sub(FTAPI_Conn client, int nSerialNo, QotSub.Response rsp)
        {

        }

        public void OnReply_RegQotPush(FTAPI_Conn client, int nSerialNo, QotRegQotPush.Response rsp)
        {

        }

        public void OnReply_GetSubInfo(FTAPI_Conn client, int nSerialNo, QotGetSubInfo.Response rsp)
        {

        }

        public void OnReply_GetTicker(FTAPI_Conn client, int nSerialNo, QotGetTicker.Response rsp)
        {
            int market = rsp.S2C.Security.Market;
            string code = rsp.S2C.Security.Code;
            foreach (var ticker in rsp.S2C.TickerListList)
            {
                long vol = ticker.Volume;
                DateTime time = (new DateTime(1970, 1, 1, 0, 0, 0)).AddHours(8).AddSeconds(ticker.Timestamp);
                double price = ticker.Price;                
            }
        }

        public void OnReply_GetBasicQot(FTAPI_Conn client, int nSerialNo, QotGetBasicQot.Response rsp)
        {
            GetBasicQotEventArgs args = new GetBasicQotEventArgs(client, nSerialNo, rsp.S2C);
            OnBasicQot(this, args);
        }

        public void OnReply_GetOrderBook(FTAPI_Conn client, int nSerialNo, QotGetOrderBook.Response rsp)
        {

        }

        public void OnReply_GetKL(FTAPI_Conn client, int nSerialNo, QotGetKL.Response rsp)
        {

        }

        public void OnReply_GetRT(FTAPI_Conn client, int nSerialNo, QotGetRT.Response rsp)
        {

        }

        public void OnReply_GetBroker(FTAPI_Conn client, int nSerialNo, QotGetBroker.Response rsp)
        {

        }

        public void OnReply_GetHistoryKL(FTAPI_Conn client, int nSerialNo, QotGetHistoryKL.Response rsp)
        {
            
        }

        public void OnReply_GetHistoryKLPoints(FTAPI_Conn client, int nSerialNo, QotGetHistoryKLPoints.Response rsp)
        {

        }

        public void OnReply_GetRehab(FTAPI_Conn client, int nSerialNo, QotGetRehab.Response rsp)
        {

        }

        public void OnReply_RequestRehab(FTAPI_Conn client, int nSerialNo, QotRequestRehab.Response rsp)
        {

        }

        public void OnReply_RequestHistoryKL(FTAPI_Conn client, int nSerialNo, QotRequestHistoryKL.Response rsp)
        {
            foreach (var kl in rsp.S2C.KlListList)
            {
                long vol = kl.Volume;
                DateTime time = (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddSeconds(kl.Timestamp);
                double open = kl.OpenPrice;
                double close = kl.ClosePrice;
                double high = kl.HighPrice;
                double low = kl.LowPrice;
            }
        }

        public void OnReply_RequestHistoryKLQuota(FTAPI_Conn client, int nSerialNo, QotRequestHistoryKLQuota.Response rsp)
        {
            RequestHistoricalKLQuotaEventArgs args = new RequestHistoricalKLQuotaEventArgs(client, nSerialNo, rsp.S2C);
            OnHistoricalKLQuota(this, args);            
        }

        public void OnReply_GetTradeDate(FTAPI_Conn client, int nSerialNo, QotGetTradeDate.Response rsp)
        {

        }

        public void OnReply_GetStaticInfo(FTAPI_Conn client, int nSerialNo, QotGetStaticInfo.Response rsp)
        {
           
        }

        public void OnReply_GetSecuritySnapshot(FTAPI_Conn client, int nSerialNo, QotGetSecuritySnapshot.Response rsp)
        {
            double m = rsp.S2C.SnapshotListList[0].Basic.PriceSpread;

        }

        public void OnReply_GetPlateSet(FTAPI_Conn client, int nSerialNo, QotGetPlateSet.Response rsp)
        {

        }

        public void OnReply_GetPlateSecurity(FTAPI_Conn client, int nSerialNo, QotGetPlateSecurity.Response rsp)
        {

        }

        public void OnReply_GetReference(FTAPI_Conn client, int nSerialNo, QotGetReference.Response rsp)
        {

        }

        public void OnReply_GetOwnerPlate(FTAPI_Conn client, int nSerialNo, QotGetOwnerPlate.Response rsp)
        {

        }

        public void OnReply_GetHoldingChangeList(FTAPI_Conn client, int nSerialNo, QotGetHoldingChangeList.Response rsp)
        {

        }

        public void OnReply_GetOptionChain(FTAPI_Conn client, int nSerialNo, QotGetOptionChain.Response rsp)
        {

        }

        public void OnReply_GetWarrant(FTAPI_Conn client, int nSerialNo, QotGetWarrant.Response rsp)
        {

        }

        public void OnReply_GetCapitalFlow(FTAPI_Conn client, int nSerialNo, QotGetCapitalFlow.Response rsp)
        {

        }

        public void OnReply_GetCapitalDistribution(FTAPI_Conn client, int nSerialNo, QotGetCapitalDistribution.Response rsp)
        {

        }

        public void OnReply_GetUserSecurity(FTAPI_Conn client, int nSerialNo, QotGetUserSecurity.Response rsp)
        {

        }

        public void OnReply_ModifyUserSecurity(FTAPI_Conn client, int nSerialNo, QotModifyUserSecurity.Response rsp)
        {

        }

        public void OnReply_Notify(FTAPI_Conn client, int nSerialNo, Notify.Response rsp)
        {

        }

        public void OnReply_UpdateBasicQot(FTAPI_Conn client, int nSerialNo, QotUpdateBasicQot.Response rsp)
        {

        }

        public void OnReply_UpdateKL(FTAPI_Conn client, int nSerialNo, QotUpdateKL.Response rsp)
        {

        }

        public void OnReply_UpdateRT(FTAPI_Conn client, int nSerialNo, QotUpdateRT.Response rsp)
        {

        }

        public void OnReply_UpdateTicker(FTAPI_Conn client, int nSerialNo, QotUpdateTicker.Response rsp)
        {
            Console.WriteLine("Recv OnReply_UpdateTicker: {0} {1}", nSerialNo, rsp);
        }

        public void OnReply_UpdateOrderBook(FTAPI_Conn client, int nSerialNo, QotUpdateOrderBook.Response rsp)
        {

        }

        public void OnReply_UpdateBroker(FTAPI_Conn client, int nSerialNo, QotUpdateBroker.Response rsp)
        {

        }

        public void OnReply_UpdateOrderDetail(FTAPI_Conn client, int nSerialNo, QotUpdateOrderDetail.Response rsp)
        {

        }
    }

    public class TrdCallback : FTSPI_Trd
    {
        private ulong accID;

        public void OnReply_GetAccList(FTAPI_Conn client, int nSerialNo, TrdGetAccList.Response rsp)
        {
            Console.WriteLine("Recv GetAccList: {0} {1}", nSerialNo, rsp);
            if (rsp.RetType != (int)Common.RetType.RetType_Succeed)
            {
                Console.WriteLine("error code is {0}", rsp.RetMsg);
            }
            else
            {
                this.accID = rsp.S2C.AccListList[0].AccID;
                FTAPI_Trd trd = client as FTAPI_Trd;
                MD5 md5 = MD5.Create();
                byte[] encryptionBytes = md5.ComputeHash(Encoding.UTF8.GetBytes("123123"));
                string unlockPwdMd5 = BitConverter.ToString(encryptionBytes).Replace("-", "").ToLower();
                TrdUnlockTrade.Request req = TrdUnlockTrade.Request.CreateBuilder().SetC2S(TrdUnlockTrade.C2S.CreateBuilder().SetUnlock(true).SetPwdMD5(unlockPwdMd5)).Build();
                uint serialNo = trd.UnlockTrade(req);
                Console.WriteLine("Send UnlockTrade: {0}", serialNo);

            }
        }

        public void OnReply_UnlockTrade(FTAPI_Conn client, int nSerialNo, TrdUnlockTrade.Response rsp)
        {
            Console.WriteLine("Recv UnlockTrade: {0} {1}", nSerialNo, rsp);
            if (rsp.RetType != (int)Common.RetType.RetType_Succeed)
            {
                Console.WriteLine("error code is {0}", rsp.RetMsg);
            }
            else
            {
                FTAPI_Trd trd = client as FTAPI_Trd;

                TrdPlaceOrder.Request.Builder req = TrdPlaceOrder.Request.CreateBuilder();
                TrdPlaceOrder.C2S.Builder cs = TrdPlaceOrder.C2S.CreateBuilder();
                Common.PacketID.Builder packetID = Common.PacketID.CreateBuilder().SetConnID(trd.GetConnectID()).SetSerialNo(0);
                TrdCommon.TrdHeader.Builder trdHeader = TrdCommon.TrdHeader.CreateBuilder().SetAccID(this.accID).SetTrdEnv((int)TrdCommon.TrdEnv.TrdEnv_Real).SetTrdMarket((int)TrdCommon.TrdMarket.TrdMarket_HK);
                cs.SetPacketID(packetID).SetHeader(trdHeader).SetTrdSide((int)TrdCommon.TrdSide.TrdSide_Sell).SetOrderType((int)TrdCommon.OrderType.OrderType_AbsoluteLimit).SetCode("01810").SetQty(100.00).SetPrice(10.2).SetAdjustPrice(true);
                req.SetC2S(cs);

                uint serialNo = trd.PlaceOrder(req.Build());
                Console.WriteLine("Send PlaceOrder: {0}, {1}", serialNo, req);
            }

        }

        public void OnReply_SubAccPush(FTAPI_Conn client, int nSerialNo, TrdSubAccPush.Response rsp)
        {

        }

        public void OnReply_GetFunds(FTAPI_Conn client, int nSerialNo, TrdGetFunds.Response rsp)
        {

        }

        public void OnReply_GetPositionList(FTAPI_Conn client, int nSerialNo, TrdGetPositionList.Response rsp)
        {

        }

        public void OnReply_GetMaxTrdQtys(FTAPI_Conn client, int nSerialNo, TrdGetMaxTrdQtys.Response rsp)
        {

        }

        public void OnReply_GetOrderList(FTAPI_Conn client, int nSerialNo, TrdGetOrderList.Response rsp)
        {

        }

        public void OnReply_GetOrderFillList(FTAPI_Conn client, int nSerialNo, TrdGetOrderFillList.Response rsp)
        {

        }

        public void OnReply_GetHistoryOrderList(FTAPI_Conn client, int nSerialNo, TrdGetHistoryOrderList.Response rsp)
        {

        }

        public void OnReply_GetHistoryOrderFillList(FTAPI_Conn client, int nSerialNo, TrdGetHistoryOrderFillList.Response rsp)
        {

        }

        public void OnReply_UpdateOrder(FTAPI_Conn client, int nSerialNo, TrdUpdateOrder.Response rsp)
        {
            Console.WriteLine("Recv UpdateOrder: {0} {1}", nSerialNo, rsp);
        }

        public void OnReply_UpdateOrderFill(FTAPI_Conn client, int nSerialNo, TrdUpdateOrderFill.Response rsp)
        {
            Console.WriteLine("Recv UpdateOrderFill: {0} {1}", nSerialNo, rsp);
        }

        public void OnReply_PlaceOrder(FTAPI_Conn client, int nSerialNo, TrdPlaceOrder.Response rsp)
        {
            Console.WriteLine("Recv PlaceOrder: {0} {1}", nSerialNo, rsp);
            if (rsp.RetType != (int)Common.RetType.RetType_Succeed)
            {
                Console.WriteLine("error code is {0}", rsp.RetMsg);
            }
        }

        public void OnReply_ModifyOrder(FTAPI_Conn client, int nSerialNo, TrdModifyOrder.Response rsp)
        {

        }
    }

    internal class FTClient
    {
        FTAPI_Qot qot = new FTAPI_Qot();

        public bool IsConnected { get; set; }
        public int ServerVersion { get; set; }
        public FTController FTController { private set; get; }
        public QotCallback QotCallback { private set; get; }

        internal FTClient(FTController controller)
        {           
            FTController = controller;
            FTAPI.Init();            
            qot.SetConnCallback(new ConnCallback(this));
            QotCallback = new QotCallback(this);
            qot.SetQotCallback(QotCallback);
            qot.SetClientInfo("FT DataSource Plugin for AB", 1);
        }

        internal void Dispose()
        {           
            FTController = null;
            qot.Close();
        }

        #region Methods used by FTController

        internal void Connect(string host, ushort port)
        {
            qot.InitConnect(host, port, false);
            //eClientSocket.reqCurrentTime();
        }

        internal void Disconnect()
        {
            
        }

        internal void RequestCurrentTime()
        {
            //eClientSocket.reqCurrentTime();
        }

        internal void RequestContractDetails(QotCommon.QotMarket market, string code)
        {
            QotGetSecuritySnapshot.Request.Builder reqBuilder = QotGetSecuritySnapshot.Request.CreateBuilder();
            QotGetSecuritySnapshot.C2S.Builder csReqBuilder = QotGetSecuritySnapshot.C2S.CreateBuilder();
            QotCommon.Security.Builder stock = QotCommon.Security.CreateBuilder();
            stock.SetCode(code);
            stock.SetMarket((int)market);
            csReqBuilder.AddSecurityList(stock);
            reqBuilder.SetC2S(csReqBuilder);
            qot.GetSecuritySnapshot(reqBuilder.Build());
        }

        internal void RequestHistoricalData(QotCommon.QotMarket market, string code, DateTime beginTime, DateTime endTime, QotCommon.KLType kLType)
        {
            QotRequestHistoryKL.Request.Builder reqBuilder = QotRequestHistoryKL.Request.CreateBuilder();
            QotRequestHistoryKL.C2S.Builder csReqBuilder = QotRequestHistoryKL.C2S.CreateBuilder();
            QotCommon.Security.Builder stock = QotCommon.Security.CreateBuilder();
            stock.SetCode(code);
            stock.SetMarket((int)market);
            csReqBuilder.Security = stock.Build();
            csReqBuilder.KlType = (int)kLType;
            csReqBuilder.BeginTime = beginTime.ToString("yyyy-MM-dd");
            csReqBuilder.EndTime = endTime.ToString("yyyy-MM-dd");
            reqBuilder.SetC2S(csReqBuilder);
            qot.RequestHistoryKL(reqBuilder.Build());
        }

        internal void RequestMarketData(QotCommon.QotMarket market, string code)
        {
            QotSub.Request.Builder reqBuilder = QotSub.Request.CreateBuilder();
            QotSub.C2S.Builder csReqBuilder = QotSub.C2S.CreateBuilder();
            QotCommon.Security.Builder stock = QotCommon.Security.CreateBuilder();
            stock.SetCode(code);
            stock.SetMarket((int)market);
            csReqBuilder.AddSecurityList(stock);
            csReqBuilder.AddSubTypeList((int)QotCommon.SubType.SubType_Ticker);
            csReqBuilder.SetIsSubOrUnSub(true);
            csReqBuilder.SetIsRegOrUnRegPush(true);
            reqBuilder.SetC2S(csReqBuilder);
            uint serialNo = qot.Sub(reqBuilder.Build());
        }

        internal void CancelMarketData(QotCommon.QotMarket market, string code)
        {
            QotSub.Request.Builder reqBuilder = QotSub.Request.CreateBuilder();
            QotSub.C2S.Builder csReqBuilder = QotSub.C2S.CreateBuilder();
            QotCommon.Security.Builder stock = QotCommon.Security.CreateBuilder();
            stock.SetCode(code);
            stock.SetMarket((int)market);
            csReqBuilder.AddSecurityList(stock);
            csReqBuilder.AddSubTypeList((int)QotCommon.SubType.SubType_Ticker);
            csReqBuilder.SetIsSubOrUnSub(false);
            csReqBuilder.SetIsRegOrUnRegPush(false);
            reqBuilder.SetC2S(csReqBuilder);
            uint serialNo = qot.Sub(reqBuilder.Build());
        }

        #region Used TWSAPI Events (passed to IBController)

        public void currentTime(long time)
        {
            FTController.ProcessCurrentTimeEvent(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(time));
        }

        public void contractDetails(int reqId, ContractDetails contractDetails)
        {
            FTController.ProcessContractDetailsEvent(reqId, contractDetails);
        }

        public void contractDetailsEnd(int reqId)
        {
            FTController.ProcessContractDetailsEndEvent(reqId);
        }

        public void tickPrice(int tickerId, int field, double price, TickAttrib attribs)
        {
            FTController.ibClient_TickPrice(tickerId, field, (float)price);
        }

        public void tickSize(int tickerId, int field, int size)
        {
            FTController.ibClient_TickSize(tickerId, field, size);
        }

        public void tickGeneric(int tickerId, int field, double value)
        {
            FTController.ibClient_TickGeneric(tickerId, field, (float)value);
        }
        
        public void headTimestamp(int reqId, string headTimestamp)
        {
            FTController.ProcessHeadTimestampEvent(reqId, headTimestamp);
        }

        public void historicalData(int reqId, Bar bar)
        {
            FTController.ProcessQuotesReceivedEvent(reqId, bar);
        }

        public void historicalDataEnd(int reqId, string start, string end)
        {
            FTController.ProcessQuotesReceivedEndEvent(reqId);
        }

        #endregion

        
    }
}