using System;
using System.Collections.Generic;
using System.Linq;
using Futu.OpenApi;
using Futu.OpenApi.Pb;

namespace AmiBroker.DataSources.FT.Events
{
    #region QotCallbacks

    public delegate void GlobalStateHandler(object sender, GlobalStateEventArgs e);
    public class GlobalStateEventArgs : EventArgs
    {
        public FTAPI_Conn Client { get; private set; }
        public int SerialNo { get; private set; }
        public GetGlobalState.S2C Result { get; private set; }
        public GlobalStateEventArgs(FTAPI_Conn client, int nSerialNo, GetGlobalState.S2C result)
        {
            Client = client;
            SerialNo = nSerialNo;
            Result = result;
        }
    }

    public delegate void SubscriptionHandler(object sender, SubscriptionEventArgs e);
    public class SubscriptionEventArgs : EventArgs
    {
        public FTAPI_Conn Client { get; private set; }
        public int SerialNo { get; private set; }
        public QotSub.S2C Result { get; private set; }
        public SubscriptionEventArgs(FTAPI_Conn client, int nSerialNo, QotSub.S2C result)
        {
            Client = client;
            SerialNo = nSerialNo;
            Result = result;
        }
    }

    public delegate void RegQotPushHandler(object sender, RegQotPushEventArgs e);
    public class RegQotPushEventArgs : EventArgs
    {
        public FTAPI_Conn Client { get; private set; }
        public int SerialNo { get; private set; }
        public QotRegQotPush.S2C Result { get; private set; }
        public RegQotPushEventArgs(FTAPI_Conn client, int nSerialNo, QotRegQotPush.S2C result)
        {
            Client = client;
            SerialNo = nSerialNo;
            Result = result;
        }
    }

    public delegate void GetSubInfoHandler(object sender, GetSubInfoEventArgs e);
    public class GetSubInfoEventArgs : EventArgs
    {
        public FTAPI_Conn Client { get; private set; }
        public int SerialNo { get; private set; }
        public QotGetSubInfo.S2C Result { get; private set; }
        public GetSubInfoEventArgs(FTAPI_Conn client, int nSerialNo, QotGetSubInfo.S2C result)
        {
            Client = client;
            SerialNo = nSerialNo;
            Result = result;
        }
    }

    public delegate void GetTickerHandler(object sender, GetTickerEventArgs e);
    public class GetTickerEventArgs : EventArgs
    {
        public FTAPI_Conn Client { get; private set; }
        public int SerialNo { get; private set; }
        public QotGetTicker.S2C Result { get; private set; }
        public GetTickerEventArgs(FTAPI_Conn client, int nSerialNo, QotGetTicker.S2C result)
        {
            Client = client;
            SerialNo = nSerialNo;
            Result = result;
        }
    }

    public delegate void GetBasicQotHandler(object sender, GetBasicQotEventArgs e);
    public class GetBasicQotEventArgs : EventArgs
    {
        public FTAPI_Conn Client { get; private set; }
        public int SerialNo { get; private set; }
        public QotGetBasicQot.S2C Result { get; private set; }
        public GetBasicQotEventArgs(FTAPI_Conn client, int nSerialNo, QotGetBasicQot.S2C result)
        {
            Client = client;
            SerialNo = nSerialNo;
            Result = result;
        }
    }

    public delegate void GetOrderBookHandler(object sender, GetOrderBookEventArgs e);
    public class GetOrderBookEventArgs : EventArgs
    {
        public FTAPI_Conn Client { get; private set; }
        public int SerialNo { get; private set; }
        public QotGetOrderBook.S2C Result { get; private set; }
        public GetOrderBookEventArgs(FTAPI_Conn client, int nSerialNo, QotGetOrderBook.S2C result)
        {
            Client = client;
            SerialNo = nSerialNo;
            Result = result;
        }
    }

    public delegate void GetKLHandler(object sender, GetKLEventArgs e);
    public class GetKLEventArgs : EventArgs
    {
        public FTAPI_Conn Client { get; private set; }
        public int SerialNo { get; private set; }
        public QotGetKL.S2C Result { get; private set; }
        public GetKLEventArgs(FTAPI_Conn client, int nSerialNo, QotGetKL.S2C result)
        {
            Client = client;
            SerialNo = nSerialNo;
            Result = result;
        }
    }

    public delegate void GetRTHandler(object sender, GetRTEventArgs e);
    public class GetRTEventArgs : EventArgs
    {
        public FTAPI_Conn Client { get; private set; }
        public int SerialNo { get; private set; }
        public QotGetRT.S2C Result { get; private set; }
        public GetRTEventArgs(FTAPI_Conn client, int nSerialNo, QotGetRT.S2C result)
        {
            Client = client;
            SerialNo = nSerialNo;
            Result = result;
        }
    }

    public delegate void GetBrokerHandler(object sender, GetBrokerEventArgs e);
    public class GetBrokerEventArgs : EventArgs
    {
        public FTAPI_Conn Client { get; private set; }
        public int SerialNo { get; private set; }
        public QotGetBroker.S2C Result { get; private set; }
        public GetBrokerEventArgs(FTAPI_Conn client, int nSerialNo, QotGetBroker.S2C result)
        {
            Client = client;
            SerialNo = nSerialNo;
            Result = result;
        }
    }

    public delegate void RequestRehabHandler(object sender, RequestRehabEventArgs e);
    public class RequestRehabEventArgs : EventArgs
    {
        public FTAPI_Conn Client { get; private set; }
        public int SerialNo { get; private set; }
        public QotRequestRehab.S2C Result { get; private set; }
        public RequestRehabEventArgs(FTAPI_Conn client, int nSerialNo, QotRequestRehab.S2C result)
        {
            Client = client;
            SerialNo = nSerialNo;
            Result = result;
        }
    }

    public delegate void RequestHistoricalKLQuotaHandler(object sender, RequestHistoricalKLQuotaEventArgs e);
    public class RequestHistoricalKLQuotaEventArgs : EventArgs
    {
        public FTAPI_Conn Client { get; private set; }
        public int SerialNo { get; private set; }
        public QotRequestHistoryKLQuota.S2C Result { get; private set; }
        public RequestHistoricalKLQuotaEventArgs(FTAPI_Conn client, int nSerialNo, QotRequestHistoryKLQuota.S2C result)
        {
            Client = client;
            SerialNo = nSerialNo;
            Result = result;
        }
    }

    public delegate void RequestHistoricalKLHandler(object sender, RequestHistoricalKLEventArgs e);
    public class RequestHistoricalKLEventArgs : EventArgs
    {
        public FTAPI_Conn Client { get; private set; }
        public int SerialNo { get; private set; }
        public QotRequestHistoryKL.S2C Result { get; private set; }
        public RequestHistoricalKLEventArgs(FTAPI_Conn client, int nSerialNo, QotRequestHistoryKL.S2C result)
        {
            Client = client;
            SerialNo = nSerialNo;
            Result = result;
        }
    }

    public delegate void GetTradeDateHandler(object sender, GetTradeDateEventArgs e);
    public class GetTradeDateEventArgs : EventArgs
    {
        public FTAPI_Conn Client { get; private set; }
        public int SerialNo { get; private set; }
        public QotGetTradeDate.S2C Result { get; private set; }
        public GetTradeDateEventArgs(FTAPI_Conn client, int nSerialNo, QotGetTradeDate.S2C result)
        {
            Client = client;
            SerialNo = nSerialNo;
            Result = result;
        }
    }

    public delegate void GetStaticInfoHandler(object sender, GetStaticInfoEventArgs e);
    public class GetStaticInfoEventArgs : EventArgs
    {
        public FTAPI_Conn Client { get; private set; }
        public int SerialNo { get; private set; }
        public QotGetStaticInfo.S2C Result { get; private set; }
        public GetStaticInfoEventArgs(FTAPI_Conn client, int nSerialNo, QotGetStaticInfo.S2C result)
        {
            Client = client;
            SerialNo = nSerialNo;
            Result = result;
        }
    }

    public delegate void GetSecuritySnapshotHandler(object sender, GetSecuritySnapshotEventArgs e);
    public class GetSecuritySnapshotEventArgs : EventArgs
    {
        public FTAPI_Conn Client { get; private set; }
        public int SerialNo { get; private set; }
        public QotGetSecuritySnapshot.S2C Result { get; private set; }
        public GetSecuritySnapshotEventArgs(FTAPI_Conn client, int nSerialNo, QotGetSecuritySnapshot.S2C result)
        {
            Client = client;
            SerialNo = nSerialNo;
            Result = result;
        }
    }

    public delegate void GetPlateSetHandler(object sender, GetPlateSetEventArgs e);
    public class GetPlateSetEventArgs : EventArgs
    {
        public FTAPI_Conn Client { get; private set; }
        public int SerialNo { get; private set; }
        public QotGetPlateSet.S2C Result { get; private set; }
        public GetPlateSetEventArgs(FTAPI_Conn client, int nSerialNo, QotGetPlateSet.S2C result)
        {
            Client = client;
            SerialNo = nSerialNo;
            Result = result;
        }
    }

    public delegate void GetPlateSecurityHandler(object sender, GetPlateSecurityEventArgs e);
    public class GetPlateSecurityEventArgs : EventArgs
    {
        public FTAPI_Conn Client { get; private set; }
        public int SerialNo { get; private set; }
        public QotGetPlateSecurity.S2C Result { get; private set; }
        public GetPlateSecurityEventArgs(FTAPI_Conn client, int nSerialNo, QotGetPlateSecurity.S2C result)
        {
            Client = client;
            SerialNo = nSerialNo;
            Result = result;
        }
    }

    public delegate void GetReferenceHandler(object sender, GetReferenceEventArgs e);
    public class GetReferenceEventArgs : EventArgs
    {
        public FTAPI_Conn Client { get; private set; }
        public int SerialNo { get; private set; }
        public QotGetReference.S2C Result { get; private set; }
        public GetReferenceEventArgs(FTAPI_Conn client, int nSerialNo, QotGetReference.S2C result)
        {
            Client = client;
            SerialNo = nSerialNo;
            Result = result;
        }
    }

    public delegate void GetOwnerPlateHandler(object sender, GetOwnerPlateEventArgs e);
    public class GetOwnerPlateEventArgs : EventArgs
    {
        public FTAPI_Conn Client { get; private set; }
        public int SerialNo { get; private set; }
        public QotGetOwnerPlate.S2C Result { get; private set; }
        public GetOwnerPlateEventArgs(FTAPI_Conn client, int nSerialNo, QotGetOwnerPlate.S2C result)
        {
            Client = client;
            SerialNo = nSerialNo;
            Result = result;
        }
    }

    public delegate void GetHoldingChangeListHandler(object sender, GetHoldingChangeListEventArgs e);
    public class GetHoldingChangeListEventArgs : EventArgs
    {
        public FTAPI_Conn Client { get; private set; }
        public int SerialNo { get; private set; }
        public QotGetHoldingChangeList.S2C Result { get; private set; }
        public GetHoldingChangeListEventArgs(FTAPI_Conn client, int nSerialNo, QotGetHoldingChangeList.S2C result)
        {
            Client = client;
            SerialNo = nSerialNo;
            Result = result;
        }
    }

    public delegate void GetOptionChainHandler(object sender, GetOptionChainEventArgs e);
    public class GetOptionChainEventArgs : EventArgs
    {
        public FTAPI_Conn Client { get; private set; }
        public int SerialNo { get; private set; }
        public QotGetOptionChain.S2C Result { get; private set; }
        public GetOptionChainEventArgs(FTAPI_Conn client, int nSerialNo, QotGetOptionChain.S2C result)
        {
            Client = client;
            SerialNo = nSerialNo;
            Result = result;
        }
    }

    public delegate void GetGetWarrantHandler(object sender, GetGetWarrantEventArgs e);
    public class GetGetWarrantEventArgs : EventArgs
    {
        public FTAPI_Conn Client { get; private set; }
        public int SerialNo { get; private set; }
        public QotGetWarrant.S2C Result { get; private set; }
        public GetGetWarrantEventArgs(FTAPI_Conn client, int nSerialNo, QotGetWarrant.S2C result)
        {
            Client = client;
            SerialNo = nSerialNo;
            Result = result;
        }
    }

    public delegate void GetGetCapitalFlowHandler(object sender, GetGetWarrantEventArgs e);
    public class GetGetCapitalFlowEventArgs : EventArgs
    {
        public FTAPI_Conn Client { get; private set; }
        public int SerialNo { get; private set; }
        public QotGetCapitalFlow.S2C Result { get; private set; }
        public GetGetCapitalFlowEventArgs(FTAPI_Conn client, int nSerialNo, QotGetCapitalFlow.S2C result)
        {
            Client = client;
            SerialNo = nSerialNo;
            Result = result;
        }
    }

    public delegate void GetGetCapitalDistributionHandler(object sender, GetGetWarrantEventArgs e);
    public class GetGetCapitalDistributionEventArgs : EventArgs
    {
        public FTAPI_Conn Client { get; private set; }
        public int SerialNo { get; private set; }
        public QotGetCapitalDistribution.S2C Result { get; private set; }
        public GetGetCapitalDistributionEventArgs(FTAPI_Conn client, int nSerialNo, QotGetCapitalDistribution.S2C result)
        {
            Client = client;
            SerialNo = nSerialNo;
            Result = result;
        }
    }

    public delegate void GetGetUserSecurityHandler(object sender, GetGetWarrantEventArgs e);
    public class GetGetUserSecurityEventArgs : EventArgs
    {
        public FTAPI_Conn Client { get; private set; }
        public int SerialNo { get; private set; }
        public QotGetUserSecurity.S2C Result { get; private set; }
        public GetGetUserSecurityEventArgs(FTAPI_Conn client, int nSerialNo, QotGetUserSecurity.S2C result)
        {
            Client = client;
            SerialNo = nSerialNo;
            Result = result;
        }
    }

    public delegate void ModifyUserSecurityHandler(object sender, ModifyUserSecurityEventArgs e);
    public class ModifyUserSecurityEventArgs : EventArgs
    {
        public FTAPI_Conn Client { get; private set; }
        public int SerialNo { get; private set; }
        public QotModifyUserSecurity.S2C Result { get; private set; }
        public ModifyUserSecurityEventArgs(FTAPI_Conn client, int nSerialNo, QotModifyUserSecurity.S2C result)
        {
            Client = client;
            SerialNo = nSerialNo;
            Result = result;
        }
    }

    public delegate void NotifyHandler(object sender, NotifyEventArgs e);
    public class NotifyEventArgs : EventArgs
    {
        public FTAPI_Conn Client { get; private set; }
        public int SerialNo { get; private set; }
        public Notify.S2C Result { get; private set; }
        public NotifyEventArgs(FTAPI_Conn client, int nSerialNo, Notify.S2C result)
        {
            Client = client;
            SerialNo = nSerialNo;
            Result = result;
        }
    }

    public delegate void UpdateBasicQotHandler(object sender, UpdateBasicQotEventArgs e);
    public class UpdateBasicQotEventArgs : EventArgs
    {
        public FTAPI_Conn Client { get; private set; }
        public int SerialNo { get; private set; }
        public QotUpdateBasicQot.S2C Result { get; private set; }
        public UpdateBasicQotEventArgs(FTAPI_Conn client, int nSerialNo, QotUpdateBasicQot.S2C result)
        {
            Client = client;
            SerialNo = nSerialNo;
            Result = result;
        }
    }

    public delegate void UpdateKLHandler(object sender, UpdateKLEventArgs e);
    public class UpdateKLEventArgs : EventArgs
    {
        public FTAPI_Conn Client { get; private set; }
        public int SerialNo { get; private set; }
        public QotUpdateKL.S2C Result { get; private set; }
        public UpdateKLEventArgs(FTAPI_Conn client, int nSerialNo, QotUpdateKL.S2C result)
        {
            Client = client;
            SerialNo = nSerialNo;
            Result = result;
        }
    }

    public delegate void UpdateRTFHandler(object sender, UpdateRTEventArgs e);
    public class UpdateRTEventArgs : EventArgs
    {
        public FTAPI_Conn Client { get; private set; }
        public int SerialNo { get; private set; }
        public QotUpdateRT.S2C Result { get; private set; }
        public UpdateRTEventArgs(FTAPI_Conn client, int nSerialNo, QotUpdateRT.S2C result)
        {
            Client = client;
            SerialNo = nSerialNo;
            Result = result;
        }
    }

    public delegate void UpdateTickerHandler(object sender, UpdateTickerEventArgs e);
    public class UpdateTickerEventArgs : EventArgs
    {
        public FTAPI_Conn Client { get; private set; }
        public int SerialNo { get; private set; }
        public QotUpdateTicker.S2C Result { get; private set; }
        public UpdateTickerEventArgs(FTAPI_Conn client, int nSerialNo, QotUpdateTicker.S2C result)
        {
            Client = client;
            SerialNo = nSerialNo;
            Result = result;
        }
    }

    public delegate void UpdateOrderBookHandler(object sender, UpdateOrderBookEventArgs e);
    public class UpdateOrderBookEventArgs : EventArgs
    {
        public FTAPI_Conn Client { get; private set; }
        public int SerialNo { get; private set; }
        public QotUpdateOrderBook.S2C Result { get; private set; }
        public UpdateOrderBookEventArgs(FTAPI_Conn client, int nSerialNo, QotUpdateOrderBook.S2C result)
        {
            Client = client;
            SerialNo = nSerialNo;
            Result = result;
        }
    }

    public delegate void UpdateBrokerHandler(object sender, UpdateBrokerEventArgs e);
    public class UpdateBrokerEventArgs : EventArgs
    {
        public FTAPI_Conn Client { get; private set; }
        public int SerialNo { get; private set; }
        public QotUpdateBroker.S2C Result { get; private set; }
        public UpdateBrokerEventArgs(FTAPI_Conn client, int nSerialNo, QotUpdateBroker.S2C result)
        {
            Client = client;
            SerialNo = nSerialNo;
            Result = result;
        }
    }

    public delegate void UpdateOrderDetailHandler(object sender, UpdateOrderDetailEventArgs e);
    public class UpdateOrderDetailEventArgs : EventArgs
    {
        public FTAPI_Conn Client { get; private set; }
        public int SerialNo { get; private set; }
        public QotUpdateOrderDetail.S2C Result { get; private set; }
        public UpdateOrderDetailEventArgs(FTAPI_Conn client, int nSerialNo, QotUpdateOrderDetail.S2C result)
        {
            Client = client;
            SerialNo = nSerialNo;
            Result = result;
        }
    }
    #endregion


    #region TrdCallbacks
    public delegate void GetAccListHandler(object sender, GetAccListEventArgs e);
    public class GetAccListEventArgs : EventArgs
    {
        public FTAPI_Conn Client { get; private set; }
        public int SerialNo { get; private set; }
        public TrdGetAccList.S2C Result { get; private set; }
        public GetAccListEventArgs(FTAPI_Conn client, int nSerialNo, TrdGetAccList.S2C result)
        {
            Client = client;
            SerialNo = nSerialNo;
            Result = result;
        }
    }

    public delegate void UnlockTradeHandler(object sender, UnlockTradeEventArgs e);
    public class UnlockTradeEventArgs : EventArgs
    {
        public FTAPI_Conn Client { get; private set; }
        public int SerialNo { get; private set; }
        public TrdUnlockTrade.S2C Result { get; private set; }
        public UnlockTradeEventArgs(FTAPI_Conn client, int nSerialNo, TrdUnlockTrade.S2C result)
        {
            Client = client;
            SerialNo = nSerialNo;
            Result = result;
        }
    }

    public delegate void SubAccPushHandler(object sender, SubAccPushEventArgs e);
    public class SubAccPushEventArgs : EventArgs
    {
        public FTAPI_Conn Client { get; private set; }
        public int SerialNo { get; private set; }
        public TrdSubAccPush.S2C Result { get; private set; }
        public SubAccPushEventArgs(FTAPI_Conn client, int nSerialNo, TrdSubAccPush.S2C result)
        {
            Client = client;
            SerialNo = nSerialNo;
            Result = result;
        }
    }

    public delegate void GetFundsHandler(object sender, GetFundsEventArgs e);
    public class GetFundsEventArgs : EventArgs
    {
        public FTAPI_Conn Client { get; private set; }
        public int SerialNo { get; private set; }
        public TrdGetFunds.S2C Result { get; private set; }
        public GetFundsEventArgs(FTAPI_Conn client, int nSerialNo, TrdGetFunds.S2C result)
        {
            Client = client;
            SerialNo = nSerialNo;
            Result = result;
        }
    }

    public delegate void GetPositionListHandler(object sender, GetPositionListEventArgs e);
    public class GetPositionListEventArgs : EventArgs
    {
        public FTAPI_Conn Client { get; private set; }
        public int SerialNo { get; private set; }
        public TrdGetPositionList.S2C Result { get; private set; }
        public GetPositionListEventArgs(FTAPI_Conn client, int nSerialNo, TrdGetPositionList.S2C result)
        {
            Client = client;
            SerialNo = nSerialNo;
            Result = result;
        }
    }

    public delegate void GetMaxTrdQtysHandler(object sender, GetMaxTrdQtysEventArgs e);
    public class GetMaxTrdQtysEventArgs : EventArgs
    {
        public FTAPI_Conn Client { get; private set; }
        public int SerialNo { get; private set; }
        public TrdGetMaxTrdQtys.S2C Result { get; private set; }
        public GetMaxTrdQtysEventArgs(FTAPI_Conn client, int nSerialNo, TrdGetMaxTrdQtys.S2C result)
        {
            Client = client;
            SerialNo = nSerialNo;
            Result = result;
        }
    }

    public delegate void GetOrderListHandler(object sender, GetOrderListEventArgs e);
    public class GetOrderListEventArgs : EventArgs
    {
        public FTAPI_Conn Client { get; private set; }
        public int SerialNo { get; private set; }
        public TrdGetOrderList.S2C Result { get; private set; }
        public GetOrderListEventArgs(FTAPI_Conn client, int nSerialNo, TrdGetOrderList.S2C result)
        {
            Client = client;
            SerialNo = nSerialNo;
            Result = result;
        }
    }

    public delegate void GetOrderFillListHandler(object sender, GetOrderFillListEventArgs e);
    public class GetOrderFillListEventArgs : EventArgs
    {
        public FTAPI_Conn Client { get; private set; }
        public int SerialNo { get; private set; }
        public TrdGetOrderFillList.S2C Result { get; private set; }
        public GetOrderFillListEventArgs(FTAPI_Conn client, int nSerialNo, TrdGetOrderFillList.S2C result)
        {
            Client = client;
            SerialNo = nSerialNo;
            Result = result;
        }
    }

    public delegate void GetHistoryOrderListHandler(object sender, GetHistoryOrderListEventArgs e);
    public class GetHistoryOrderListEventArgs : EventArgs
    {
        public FTAPI_Conn Client { get; private set; }
        public int SerialNo { get; private set; }
        public TrdGetHistoryOrderList.S2C Result { get; private set; }
        public GetHistoryOrderListEventArgs(FTAPI_Conn client, int nSerialNo, TrdGetHistoryOrderList.S2C result)
        {
            Client = client;
            SerialNo = nSerialNo;
            Result = result;
        }
    }

    public delegate void GetHistoryOrderFillListHandler(object sender, GetHistoryOrderFillListEventArgs e);
    public class GetHistoryOrderFillListEventArgs : EventArgs
    {
        public FTAPI_Conn Client { get; private set; }
        public int SerialNo { get; private set; }
        public TrdGetHistoryOrderFillList.S2C Result { get; private set; }
        public GetHistoryOrderFillListEventArgs(FTAPI_Conn client, int nSerialNo, TrdGetHistoryOrderFillList.S2C result)
        {
            Client = client;
            SerialNo = nSerialNo;
            Result = result;
        }
    }

    public delegate void UpdateOrderHandler(object sender, UpdateOrderEventArgs e);
    public class UpdateOrderEventArgs : EventArgs
    {
        public FTAPI_Conn Client { get; private set; }
        public int SerialNo { get; private set; }
        public TrdUpdateOrder.S2C Result { get; private set; }
        public UpdateOrderEventArgs(FTAPI_Conn client, int nSerialNo, TrdUpdateOrder.S2C result)
        {
            Client = client;
            SerialNo = nSerialNo;
            Result = result;
        }
    }

    public delegate void UpdateOrderFillHandler(object sender, UpdateOrderFillEventArgs e);
    public class UpdateOrderFillEventArgs : EventArgs
    {
        public FTAPI_Conn Client { get; private set; }
        public int SerialNo { get; private set; }
        public TrdUpdateOrderFill.S2C Result { get; private set; }
        public UpdateOrderFillEventArgs(FTAPI_Conn client, int nSerialNo, TrdUpdateOrderFill.S2C result)
        {
            Client = client;
            SerialNo = nSerialNo;
            Result = result;
        }
    }

    public delegate void PlaceOrderHandler(object sender, PlaceOrderEventArgs e);
    public class PlaceOrderEventArgs : EventArgs
    {
        public FTAPI_Conn Client { get; private set; }
        public int SerialNo { get; private set; }
        public TrdPlaceOrder.S2C Result { get; private set; }
        public PlaceOrderEventArgs(FTAPI_Conn client, int nSerialNo, TrdPlaceOrder.S2C result)
        {
            Client = client;
            SerialNo = nSerialNo;
            Result = result;
        }
    }

    public delegate void ModifyOrderHandler(object sender, ModifyOrderEventArgs e);
    public class ModifyOrderEventArgs : EventArgs
    {
        public FTAPI_Conn Client { get; private set; }
        public int SerialNo { get; private set; }
        public TrdModifyOrder.S2C Result { get; private set; }
        public ModifyOrderEventArgs(FTAPI_Conn client, int nSerialNo, TrdModifyOrder.S2C result)
        {
            Client = client;
            SerialNo = nSerialNo;
            Result = result;
        }
    }
    #endregion
}
