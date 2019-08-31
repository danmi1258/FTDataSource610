using AmiBroker.Data;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using XDMessaging;

namespace AmiBroker.DataSources.FT
{
    /// <summary>
    /// Major interface class between AmiBroker and the TWS client.
    /// It overrides DataSourceBase's methods (managed data API of AmiBroker).
    /// It implements plugin's context menu.
    /// (Every event, method call, menu event from AB arrive to this class!)
    /// </summary>
    [ABDataSource("Futu NiuNiu")]
    public class FTDataSource : DataSourceBase, IDisposable
    {
        private const int TimerInterval = 500;                      // timer interval (millisecs)
        private const int ConnectionRetryInterval = 30;             // to wait for next attempt to connect (seconds)

        #region Context menu and form variables

        private ToolStripMenuItem mReconnect;
        private ToolStripMenuItem mDisconnect;

        private ToolStripMenuItem mBackfill;
        private ToolStripMenuItem mBackfillWL;
        private ToolStripMenuItem mBackfillAll;
        private ToolStripMenuItem menu1Day;
        private ToolStripMenuItem menu1Week;
        private ToolStripMenuItem menu2Weeks;
        private ToolStripMenuItem menu1Month;
        private ToolStripMenuItem menu3Months;
        private ToolStripMenuItem menu6Months;
        private ToolStripMenuItem menu1Year;
        private ToolStripMenuItem menuMax;
        private ToolStripMenuItem menu1DayWL;
        private ToolStripMenuItem menu1WeekWL;
        private ToolStripMenuItem menu2WeeksWL;
        private ToolStripMenuItem menu1MonthWL;
        private ToolStripMenuItem menu3MonthsWL;
        private ToolStripMenuItem menu6MonthsWL;
        private ToolStripMenuItem menu1YearWL;
        private ToolStripMenuItem menuMaxWL;
        private ToolStripMenuItem menu1DayAll;
        private ToolStripMenuItem menu1WeekAll;
        private ToolStripMenuItem menu2WeeksAll;
        private ToolStripMenuItem menu1MonthAll;
        private ToolStripMenuItem menu3MonthsAll;
        private ToolStripMenuItem menu6MonthsAll;
        private ToolStripMenuItem menu1YearAll;
        private ToolStripMenuItem menuMaxAll;
        private ToolStripMenuItem mCancel;

        private ToolStripMenuItem mUpdateSymbolInfo;
        private ToolStripMenuItem mOpenIBContract;
        private ToolStripMenuItem mFindIBContract;

        private ToolStripMenuItem mOpenLogFile;

        private ContextMenuStrip mContextMenu;

        private SearchForm searchForm;

        // the current active symbol in AB when context menu was activated
        private StockInfo currentSI;
        private string currentTicker;
        private string currentTickerShortend;

        #endregion

        #region Database level static variables

        // config setting of the db
        internal static FTConfiguration IBConfiguration;            // XML config created/saved by IBConfigureForm

        // database and workspace data
        internal static Workspace Workspace;                        // AB workspace
        private IntPtr MainWindowHandle;
        internal static string DatabasePath;
        internal static bool AllowMixedEODIntra;                    // intra day and end of day quotes are mixed in the database
        internal static bool RthOnly;                               // only trades in regular trading hours are collected 

        // Periodicity in seconds (GetQuotesEx)
        internal static Periodicity Periodicity;                    // Base time interval of the DB
        private bool firstGetQuotesExCall = true;

        #endregion

        #region Current status variables

        private FTController controller;                            // major IB logic (Accessed by search form as well!)

        private System.Threading.Timer timer;                       // timer to check connection and execute auto refresh
        private int inTimerTick;                                    // to prevent reentry to timer's event handler
        private DateTime nextAutoRefreshTime;                       // time of next auto refresh
        private DateTime connectionRetryTime = DateTime.MinValue;   // time of next attempt to reconnect
        private DateTime lastUptodateTime = DateTime.MinValue;      // until data of all tickers is uptodate

        // plug-in's state
        private IBPluginState prevPluginState;                      // last state of controller
        private bool manuallyDisconnected;                          // indicate the tws was disconnected on user request / first time connect
        private bool firstConnection;                               // indicates, that tws has not been connected yet.

        // plug-in's status message
        private string lastLongMessageText;
        private int lastLongMessageTime;

        private StringCollection rtWindowTickersBck;                // tickers for which we have requested recent info

        #endregion

        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern bool PostMessage(IntPtr hWnd, UInt32 msg, IntPtr wParam, IntPtr lParam);

        public FTDataSource(string config)
            : base(config)
        {
            LogAndMessage.LogSource = "FTDataSource";

            #region Context menu

            // main menu
            mReconnect = new ToolStripMenuItem("Connect", null, new EventHandler(mReconnect_Click));
            mDisconnect = new ToolStripMenuItem("Disconnect", null, new EventHandler(mDisconnect_Click));
            ToolStripSeparator mSeparator1 = new ToolStripSeparator();
            mBackfill = new ToolStripMenuItem("Backfill Current");
            mBackfillWL = new ToolStripMenuItem("Backfill Watchlist");
            mBackfillAll = new ToolStripMenuItem("Backfill All");
            mCancel = new ToolStripMenuItem("Cancel All Backfills", null, new EventHandler(mCancel_Click));
            ToolStripSeparator mSeparator2 = new ToolStripSeparator();
            mUpdateSymbolInfo = new ToolStripMenuItem("Update Symbol Data", null, mUpdateSymbolInfo_Click);
            mOpenIBContract = new ToolStripMenuItem("Open IB Contract", null, new EventHandler(mOpenIBContract_Click));
            mFindIBContract = new ToolStripMenuItem("Find IB Contract", null, new EventHandler(mFindIBContract_Click));
            ToolStripSeparator mSeparator3 = new ToolStripSeparator();
            mOpenLogFile = new ToolStripMenuItem("Open Log File", null, new EventHandler(mOpenLogFile_Click));

            // backfill submenu
            menu1Day = new ToolStripMenuItem("1 Day", null, new EventHandler(mBackfill_Click));
            menu1Week = new ToolStripMenuItem("1 Week", null, new EventHandler(mBackfill_Click));
            menu2Weeks = new ToolStripMenuItem("2 Weeks", null, new EventHandler(mBackfill_Click));
            menu1Month = new ToolStripMenuItem("1 Month", null, new EventHandler(mBackfill_Click));
            menu3Months = new ToolStripMenuItem("3 Months", null, new EventHandler(mBackfill_Click));
            menu6Months = new ToolStripMenuItem("6 Months", null, new EventHandler(mBackfill_Click));
            menu1Year = new ToolStripMenuItem("1 Year", null, new EventHandler(mBackfill_Click));
            menuMax = new ToolStripMenuItem("Maximum", null, new EventHandler(mBackfill_Click));

            // backfillWL submenu
            menu1DayWL = new ToolStripMenuItem("1 Day", null, new EventHandler(mBackfillWL_Click));
            menu1WeekWL = new ToolStripMenuItem("1 Week", null, new EventHandler(mBackfillWL_Click));
            menu2WeeksWL = new ToolStripMenuItem("2 Weeks", null, new EventHandler(mBackfillWL_Click));
            menu1MonthWL = new ToolStripMenuItem("1 Month", null, new EventHandler(mBackfillWL_Click));
            menu3MonthsWL = new ToolStripMenuItem("3 Months", null, new EventHandler(mBackfillWL_Click));
            menu6MonthsWL = new ToolStripMenuItem("6 Months", null, new EventHandler(mBackfillWL_Click));
            menu1YearWL = new ToolStripMenuItem("1 Year", null, new EventHandler(mBackfillWL_Click));
            menuMaxWL = new ToolStripMenuItem("Maximum", null, new EventHandler(mBackfillWL_Click));

            // backfillAll submenu
            menu1DayAll = new ToolStripMenuItem("1 Day", null, new EventHandler(mBackfillAll_Click));
            menu1WeekAll = new ToolStripMenuItem("1 Week", null, new EventHandler(mBackfillAll_Click));
            menu2WeeksAll = new ToolStripMenuItem("2 Weeks", null, new EventHandler(mBackfillAll_Click));
            menu1MonthAll = new ToolStripMenuItem("1 Month", null, new EventHandler(mBackfillAll_Click));
            menu3MonthsAll = new ToolStripMenuItem("3 Months", null, new EventHandler(mBackfillAll_Click));
            menu6MonthsAll = new ToolStripMenuItem("6 Months", null, new EventHandler(mBackfillAll_Click));
            menu1YearAll = new ToolStripMenuItem("1 Year", null, new EventHandler(mBackfillAll_Click));
            menuMaxAll = new ToolStripMenuItem("Maximum", null, new EventHandler(mBackfillAll_Click));

            // adding submenus
            mBackfill.DropDownItems.AddRange(new ToolStripItem[] { menu1Day, menu1Week, menu2Weeks, menu1Month, menu3Months, menu6Months, menu1Year, menuMax });
            mBackfillWL.DropDownItems.AddRange(new ToolStripItem[] { menu1DayWL, menu1WeekWL, menu2WeeksWL, menu1MonthWL, menu3MonthsWL, menu6MonthsWL, menu1YearWL, menuMaxWL });
            mBackfillAll.DropDownItems.AddRange(new ToolStripItem[] { menu1DayAll, menu1WeekAll, menu2WeeksAll, menu1MonthAll, menu3MonthsAll, menu6MonthsAll, menu1YearAll, menuMaxAll });

            mContextMenu = new ContextMenuStrip();
            mContextMenu.Items.AddRange(new ToolStripItem[] { mReconnect, mDisconnect, mSeparator1, mBackfill, mBackfillWL, mBackfillAll, mCancel, mSeparator2, mUpdateSymbolInfo, mOpenIBContract, mFindIBContract, mSeparator3, mOpenLogFile });

            SetContextMenuState();

            #endregion

            rtWindowTickersBck = new StringCollection();

            timer = new System.Threading.Timer(timer_Tick);
            timer.Change(TimerInterval, TimerInterval);

            XDMessagingClient client = new XDMessagingClient();
            // Create listener instance using HighPerformanceUI mode
            IXDListener listener = client.Listeners
                .GetListenerForMode(XDTransportMode.HighPerformanceUI);

            // Register channel to listen on
            listener.RegisterChannel("commands");
            listener.MessageReceived += Listener_MessageReceived;
        }

        private void Listener_MessageReceived(object sender, XDMessageEventArgs e)
        {
            if (e.DataGram.Message == "reconnect")
            {
                LogAndMessage.Log(MessageType.Info, "Reconencting...");
                //manuallyDisconnected = true;   
                //if (controller.IsIbConnected)
                controller.Disconnect();
                controller.Connect(false);
            }
        }

        #region AB API Methods

        public static new string Configure(string oldSettings, ref InfoSite infoSite)
        {
            FTConfiguration ibConfiguration = FTConfiguration.GetConfigObject(oldSettings);

            FTConfigureForm ibConfigureForm = new FTConfigureForm(ibConfiguration, ref infoSite);
            if (ibConfigureForm.ShowDialog() == DialogResult.OK)
                return FTConfiguration.GetConfigString(ibConfigureForm.GetNewSettings());
            else
                return oldSettings;
        }

        public override void GetQuotesEx(string ticker, ref QuotationArray quotes)
        {
            // if first call));
            if (firstGetQuotesExCall)
            {
                firstGetQuotesExCall = false;
                // save database periodicity
                Periodicity = quotes.Periodicity;
            }

            if (controller == null)
                return;

            controller.GetQuotesEx(ticker, ref quotes);

            return;
        }

        public override void GetRecentInfo(string ticker)
        {
            // save rt window tickers
            rtWindowTickersBck.Add(ticker);

            // start RT window update           
            controller.GetRecentInfo(ticker);
        }

        public override AmiVar GetExtraData(string ticker, string name, Periodicity periodicity, int arraySize)
        {
            if (controller == null)
                return new AmiVar();

            return controller.GetExtraData(ticker, name, periodicity, arraySize);
        }

        public override PluginStatus GetStatus()
        {
            PluginStatus status = new PluginStatus();

            IBPluginState ibPluginState = IBPluginState.Disconnected;

            if (controller != null)
                ibPluginState = controller.GetIBPluginState();

            switch (ibPluginState)
            {
                case IBPluginState.Disconnected:
                    status.Status = StatusCode.SevereError;
                    status.Color = System.Drawing.Color.IndianRed;
                    status.ShortMessage = "Off-line";

                    break;

                case IBPluginState.Busy:
                    status.Status = StatusCode.OK;
                    status.Color = System.Drawing.Color.Yellow;
                    status.ShortMessage = "Busy";

                    break;

                case IBPluginState.Ready:
                    status.Status = StatusCode.OK;
                    status.Color = System.Drawing.Color.ForestGreen;
                    status.ShortMessage = "Ready";

                    break;

            }

            status.LongMessage = LogAndMessage.GetMessages();

            // collect and display data issues
            string failedTickers = controller.GetFailedTickers();
            bool dataError = !string.IsNullOrEmpty(failedTickers);
            if (dataError && ibPluginState != IBPluginState.Disconnected)     // if disconnected, data issues are meaningless...
            {
                status.ShortMessage += " !";
                status.Status = StatusCode.Warning;
                if (string.IsNullOrEmpty(status.LongMessage))
                    status.LongMessage = "Failed tickers: " + failedTickers;
            }

            // if there is no message, we show the short message (Busy, Ok, etc)
            if (string.IsNullOrEmpty(status.LongMessage))
            {
                status.LongMessage = status.ShortMessage;

                // save as last shown message to avoid status popup
                lastLongMessageText = status.ShortMessage;
            }

            // if new message we use a new lastLongMessageTime value to cause status popup
            if (lastLongMessageText != status.LongMessage)
            {
                lastLongMessageText = status.LongMessage;
                lastLongMessageTime = (int)DateTime.Now.TimeOfDay.TotalMilliseconds;
            }

            // set status and "timestamp"
            status.Status = (StatusCode)((int)status.Status + lastLongMessageTime);

            return status;
        }

        public override int GetSymbolLimit()
        {
            // maximun of 100 concurrent tickers can get live data
            return 100;
        }

        public override bool Notify(ref PluginNotification notifyData)
        {
            bool result = true;

            switch (notifyData.Reason)
            {
                case Reason.DatabaseLoaded:

                    // if database is loaded
                    if (controller != null)
                    {
                        // disconnect from TWS and reset all data
                        controller.Disconnect();
                        ((IDisposable)controller).Dispose();
                        controller = null;
                    }

                    Workspace = notifyData.Workspace;
                    DatabasePath = notifyData.DatabasePath;
                    MainWindowHandle = notifyData.MainWnd;
                    AllowMixedEODIntra = Workspace.AllowMixedEODIntra != 0;

                    // start logging the opening of the database
                    LogAndMessage.Log(MessageType.Info, "Database: " + DatabasePath);
                    LogAndMessage.Log(MessageType.Info, "Mixed EOD/Intra: " + (Workspace.AllowMixedEODIntra != 0));
                    LogAndMessage.Log(MessageType.Info, "Number of bars: " + Workspace.NumBars);
                    LogAndMessage.Log(MessageType.Info, "Database config: " + Settings);

                    // create the config object
                    IBConfiguration = FTConfiguration.GetConfigObject(Settings);
                    LogAndMessage.VerboseLog = IBConfiguration.VerboseLog;
                    RthOnly = IBConfiguration.RthOnly;
                    CalcNextAutoRefreshTime();

                    // create new controller
                    connectionRetryTime = DateTime.Now.AddSeconds(ConnectionRetryInterval);
                    prevPluginState = IBPluginState.Disconnected;
                    firstConnection = true;
                    controller = new FTController();

                    // connect database to tws
                    controller.Connect(false);

                    if (rtWindowTickersBck.Count > 0)
                    {
                        for (int i = 0; i < rtWindowTickersBck.Count; i++)
                            controller.GetRecentInfo(rtWindowTickersBck[i]);
                    }

                    break;

                // user changed the db
                case Reason.DatabaseUnloaded:

                    // disconnect from TWS
                    if (controller != null)
                    {
                        controller.Disconnect();
                        ((IDisposable)controller).Dispose();
                        controller = null;
                    }

                    // clean up
                    Workspace = new Workspace();
                    DatabasePath = null;
                    MainWindowHandle = IntPtr.Zero;
                    searchForm = null;

                    break;

                // seams to be obsolete
                case Reason.SettingsChanged:

                    break;

                // user right clicks data plugin area in AB
                case Reason.RightMouseClick:

                    if (controller != null)
                    {
                        currentSI = notifyData.CurrentSI;
                        if (currentSI != null)
                        {
                            currentTicker = currentSI.ShortName;
                            if (currentTicker.Length > 10)
                                currentTickerShortend = currentTicker.Substring(0, 7) + "...";
                            else
                                currentTickerShortend = currentTicker;
                        }
                        else
                        {
                            currentTicker = null;
                            currentTickerShortend = null;
                        }
                    }

                    SetContextMenuState();

                    ShowContextMenu(mContextMenu);

                    break;

                default:
                    result = false;

                    break;
            }
            return result;
        }

        public override bool SetTimeBase(Periodicity timeBase)
        {
            // all timeframes are supported
            return true;
        }

        public override bool IsBackfillComplete(string ticker)
        {
            return controller.IsBackfillComplete(ticker);
        }

        public void Dispose()
        {
            if (mContextMenu != null)
                mContextMenu.Dispose();

            if (timer != null)
                timer.Dispose();

            if (searchForm != null)
                searchForm.Dispose();
        }

        #endregion

        #region  PlugIn Context Menu Events and Methods

        private void mReconnect_Click(object sender, EventArgs e)
        {
            LogAndMessage.Log(MessageType.Info, "TWS is manually reconnected.");
            manuallyDisconnected = false;
            controller.Connect(false);
        }

        private void mDisconnect_Click(object sender, EventArgs e)
        {
            LogAndMessage.Log(MessageType.Info, "TWS is manually disconnected.");
            manuallyDisconnected = true;
            controller.Disconnect();
        }

        private void mBackfill_Click(object sender, EventArgs e)
        {
            if (firstGetQuotesExCall)                   // db periodicity not known yet
                return;

            if (string.IsNullOrEmpty(currentTicker))    // no selected ticker
                return;

            DateTime refreshStartDate = GetRefreshStartDate(sender);
            refreshStartDate = IBClientHelper.GetAdjustedStartDate(refreshStartDate, Periodicity, DateTime.MinValue, false);

            LogAndMessage.Log(MessageType.Info, currentTicker + ": Manual backfill from " + refreshStartDate.ToShortDateString() + ".");

            StringCollection tickerToBackfill = new StringCollection();
            tickerToBackfill.Add(currentTicker);

            StartBackfills(refreshStartDate, tickerToBackfill);
        }

        private void mBackfillWL_Click(object sender, EventArgs e)
        {
            if (firstGetQuotesExCall)                   // db periodicity not known yet
                return;

            if (string.IsNullOrEmpty(currentTicker))    // no selected ticker
                return;

            DateTime refreshStartDate = GetRefreshStartDate(sender);
            refreshStartDate = IBClientHelper.GetAdjustedStartDate(refreshStartDate, Periodicity, DateTime.MinValue, false);

            WatchlistForm watchlistForm = new WatchlistForm(DatabasePath);
            if (DialogResult.OK != watchlistForm.ShowDialog() || watchlistForm.SelectedWatchlistIndices == null)
                return;

            int[] selectedWatchlistIndices = watchlistForm.SelectedWatchlistIndices;

            watchlistForm.Dispose();

            StringCollection tickersInWatchlists = new StringCollection();

            ulong watchlistBits = 0;

            foreach (int watchlistId in selectedWatchlistIndices)
            {
                watchlistBits |= (ulong)1 << watchlistId;
            }

            LogAndMessage.Log(MessageType.Info, "Manual backfill of watchlists (" + watchlistBits.ToString("0x") + ") from " + refreshStartDate.ToShortDateString() + ".");

            try
            {
                Type abAppType = Type.GetTypeFromProgID("Broker.Application");
                object abApp = Activator.CreateInstance(abAppType);

                // access AB COM interface to get current ticker
                if (abAppType != null && abApp != null)
                {
                    object abStocks = abAppType.InvokeMember("Stocks", BindingFlags.GetProperty, null, abApp, null);
                    Type abStocksType = abStocks.GetType();

                    // get the number of stocks in the db
                    int stockCount = (int)abStocksType.InvokeMember("Count", BindingFlags.GetProperty, null, abStocks, null);

                    if (stockCount > 0)
                    {
                        Type abStockType = abStocksType.InvokeMember("Item", BindingFlags.GetProperty, null, abStocks, new object[] { 0 }).GetType();

                        for (int i = 0; i < stockCount; i++)
                        {
                            object abStock = abStocksType.InvokeMember("Item", BindingFlags.GetProperty, null, abStocks, new object[] { i });
                            if (abStock != null)
                            {
                                string ticker = (string)abStockType.InvokeMember("Ticker", BindingFlags.GetProperty, null, abStock, null);
                                uint watchlistBits1 = (uint)(int)abStockType.InvokeMember("WatchListBits", BindingFlags.GetProperty, null, abStock, null);
                                uint watchlistBits2 = (uint)(int)abStockType.InvokeMember("WatchListBits2", BindingFlags.GetProperty, null, abStock, null);
                                ulong watchlistBitsCombined = (watchlistBits2 << 32) + watchlistBits1;

                                if ((watchlistBitsCombined & watchlistBits) != 0)
                                {
                                    if (!tickersInWatchlists.Contains(ticker))
                                        tickersInWatchlists.Add(ticker);
                                }
                            }
                        }
                    }
                    else
                        LogAndMessage.Log(MessageType.Trace, "Manual backfill of watchlists failed. Database has no symbols.");
                }
                else
                    LogAndMessage.Log(MessageType.Warning, "Manual backfill of watchlists failed. ActiveX interface error.");
            }
            catch (Exception ex)
            {
                LogAndMessage.LogAndQueue(MessageType.Error, "Manual backfill of watchlists failed. Exception: " + ex);
            }

            if (tickersInWatchlists.Count == 0)
                LogAndMessage.Log(MessageType.Trace, "Manual backfill of watchlists failed. Selected watchlists have no symbols.");
            else
                StartBackfills(refreshStartDate, tickersInWatchlists);
        }

        private void mBackfillAll_Click(object sender, EventArgs e)
        {
            if (firstGetQuotesExCall)                   // db periodicity not known yet
                return;

            if (string.IsNullOrEmpty(currentTicker))    // no selected ticker
                return;

            DateTime refreshStartDate = GetRefreshStartDate(sender);
            refreshStartDate = IBClientHelper.GetAdjustedStartDate(refreshStartDate, Periodicity, DateTime.MinValue, false);

            //
            // collecting all tickers
            //

            int stockCount = 0;
            StringCollection tickersInDatabase = new StringCollection();

            LogAndMessage.Log(MessageType.Info, "Manual backfill of all tickers from " + refreshStartDate.ToShortDateString() + ".");

            try
            {
                Type abAppType = Type.GetTypeFromProgID("Broker.Application");
                object abApp = Activator.CreateInstance(abAppType);

                // access AB COM interface to get current ticker
                if (abAppType != null && abApp != null)
                {
                    object abStocks = abAppType.InvokeMember("Stocks", BindingFlags.GetProperty, null, abApp, null);
                    Type abStocksType = abStocks.GetType();

                    // get the number of stocks in the db
                    stockCount = (int)abStocksType.InvokeMember("Count", BindingFlags.GetProperty, null, abStocks, null);

                    if (stockCount > 0)
                    {
                        Type abStockType = abStocksType.InvokeMember("Item", BindingFlags.GetProperty, null, abStocks, new object[] { 0 }).GetType();

                        for (int i = 0; i < stockCount; i++)
                        {
                            object abStock = abStocksType.InvokeMember("Item", BindingFlags.GetProperty, null, abStocks, new object[] { i });
                            if (abStock != null)
                            {
                                string ticker = (string)abStockType.InvokeMember("Ticker", BindingFlags.GetProperty, null, abStock, null);
                                if (!tickersInDatabase.Contains(ticker))
                                    tickersInDatabase.Add(ticker);
                            }
                        }
                    }
                    else
                        LogAndMessage.Log(MessageType.Trace, "Manual backfill of all symbols failed. Database has no symbols.");
                }
                else
                    LogAndMessage.Log(MessageType.Warning, "Manual backfill of all symbols failed. ActiveX interface error.");
            }
            catch (Exception ex)
            {
                LogAndMessage.LogAndQueue(MessageType.Error, "Manual backfill of all symbols failed. Exception: " + ex);
            }

            StartBackfills(refreshStartDate, tickersInDatabase);
        }

        private void mCancel_Click(object sender, EventArgs e)
        {
            LogAndMessage.Log(MessageType.Info, "All backfill operations are manually cancelled.");

            controller.CancelAllRefreshes();
        }

        private void mUpdateSymbolInfo_Click(object sender, EventArgs e)
        {
            if (currentSI == null)
                return;

            LogAndMessage.Log(MessageType.Info, currentTicker + ": Symbol info manually updated.");
            controller.UpdateSymbolInfo(currentTicker);
        }

        private void mOpenIBContract_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(currentTicker))
                return;

            // get the webid from the IB ContractDeatails in TickerData
            string webId = GetWebIdOfTicker(currentTicker);

            if (string.IsNullOrEmpty(webId))
            {
                MessageBox.Show("Invalid ticker or ticker is not yet refreshed.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Type shellType = Type.GetTypeFromProgID("Wscript.Shell");
            object shell = Activator.CreateInstance(shellType);
            shellType.InvokeMember("Run", BindingFlags.InvokeMethod, null, shell, new object[] { "https://pennies.interactivebrokers.com/cstools/contract_info/v3.9/index.php?action=CONTRACT_DETAILS&clt=1&site=IB&conid=" + webId });
        }

        private void mFindIBContract_Click(object sender, EventArgs e)
        {
            try
            {
                if (searchForm == null)
                    searchForm = new SearchForm(controller);
                searchForm.ShowDialog();

                PostMessage(MainWindowHandle, 0x001C, new IntPtr(1), new IntPtr(0)); // WM_ACTIVATEAPP = 0x001C
                PostMessage(MainWindowHandle, 0x0086, new IntPtr(1), new IntPtr(0));// WM_NCACTIVATE = 0x0086
                PostMessage(MainWindowHandle, 0x0006, new IntPtr(1), new IntPtr(0));// WM_ACTIVATE = 0x0006
                PostMessage(MainWindowHandle, 0x36E, new IntPtr(1), new IntPtr(0));// WM_ACTIVATETOPLEVEL = 0x36E
                PostMessage(MainWindowHandle, 0x2862, new IntPtr(0xb6d), new IntPtr(0)); //WM_USER + 9314
                PostMessage(MainWindowHandle, 0x0111, new IntPtr(0xb6d), new IntPtr(0)); //WM_COMMAND = 0x0111;
            }
            catch (Exception ex)
            {
                LogAndMessage.Log(MessageType.Error, "Error while opening Search window:" + ex);
            }
        }

        private void mOpenLogFile_Click(object sender, EventArgs e)
        {
            const string npp = @"C:\Program Files (x86)\Notepad++\notepad++.exe";
            const string nppx64 = @"C:\Program Files\Notepad++\notepad++.exe";

            ProcessStartInfo psi;

            try
            {
                // if notepad++ is installed
                if (File.Exists(npp))
                    // start notepad++ to open the log file
                    psi = new ProcessStartInfo(npp);

                // if notepad++ x64 is installed
                else if (File.Exists(nppx64))
                    // start notepad++ x64 to open the log file
                    psi = new ProcessStartInfo(nppx64);

                else
                    // start notepad to open the log file
                    psi = new ProcessStartInfo("notepad.exe");

                psi.WorkingDirectory = Path.GetDirectoryName(DataSourceBase.DotNetLogFile);
                psi.Arguments = "\"" + DataSourceBase.DotNetLogFile + "\"";

                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not start notepad.exe to open instace log file:" + Environment.NewLine + ex, "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
        }

        // set visibility of context menu items according to the state (conn, status, config) of the plugin
        private void SetContextMenuState()
        {
            bool headTimestampOk;

            // set menu text depending on if a ticker is selected
            if (string.IsNullOrEmpty(currentTicker))
            {
                mBackfill.Text = "Backfill Current";
                mUpdateSymbolInfo.Text = "Update Symbol Data";
                mOpenIBContract.Text = "Open IB Contract";
                mOpenIBContract.Enabled = false; // this (web page) can work even if TWS is disconnected
                headTimestampOk = false;
            }
            else
            {
                mBackfill.Text = "Backfill Current - " + currentTickerShortend;
                mUpdateSymbolInfo.Text = "Update Symbol Data - " + currentTickerShortend;
                mOpenIBContract.Text = "Open IB Contract - " + currentTickerShortend;
                mOpenIBContract.Enabled = true; // this (web page) can work even if TWS is disconnected
                headTimestampOk = GetHeadTimeStampOfTicker(currentTicker) != DateTime.MinValue;
            }

            // check if max, long, short download periods are enabled on IB API

            bool maxDownloadEnabled = Periodicity == Periodicity.EndOfDay && headTimestampOk || Periodicity < Periodicity.EndOfDay && Periodicity > Periodicity.FifteenSeconds;
            menuMax.Visible = maxDownloadEnabled;
            menuMaxWL.Visible = maxDownloadEnabled;
            menuMaxAll.Visible = maxDownloadEnabled;
            bool longDownloadEnabled = Periodicity > Periodicity.FifteenSeconds;
            menu1Year.Visible = longDownloadEnabled;
            menu1YearWL.Visible = longDownloadEnabled;
            menu1YearAll.Visible = longDownloadEnabled;
            bool shortDownloadEnabled = Periodicity <= Periodicity.FifteenSeconds;
            menu1Day.Visible = shortDownloadEnabled;
            menu1DayWL.Visible = shortDownloadEnabled;
            menu1DayAll.Visible = shortDownloadEnabled;

            if (controller == null)
            {
                // disable all ...
                mReconnect.Enabled = false;
                mDisconnect.Enabled = false;
                mBackfill.Enabled = false;
                mBackfillWL.Enabled = false;
                mBackfillAll.Enabled = false;
                mCancel.Enabled = false;
                mUpdateSymbolInfo.Enabled = false;
                mFindIBContract.Enabled = false;
            }
            else
            {
                // TWS is connected (data stream may be lost)
                bool connected = controller.IsIbConnected();

                // TWS is connected and database has at least one symbol(!) and one of them is selected (current symbol)
                bool isSymbolSelected = connected && !string.IsNullOrEmpty(currentTicker);

                mReconnect.Enabled = !connected;
                mDisconnect.Enabled = connected;
                mBackfill.Enabled = isSymbolSelected;
                mBackfillWL.Enabled = isSymbolSelected;
                mBackfillAll.Enabled = isSymbolSelected;
                mCancel.Enabled = connected;
                mUpdateSymbolInfo.Enabled = isSymbolSelected; // && !config.SymbolUpdate;
                mFindIBContract.Enabled = connected && (searchForm == null || !searchForm.Visible);
            }
        }

        // get the backfill start date: "Now" - "interval of clicked menu"
        private DateTime GetRefreshStartDate(object sender)
        {
            DateTime refreshStartDate = DateTime.Now.Date;
            int daysofWeek = -(int)refreshStartDate.DayOfWeek;
            if (daysofWeek == 0)
                daysofWeek = -7;

            switch (((ToolStripMenuItem)sender).Text)
            {
                case "1 Day":
                    refreshStartDate = refreshStartDate.AddDays(-1);
                    break;

                case "1 Week":
                    refreshStartDate = refreshStartDate.AddDays(daysofWeek);
                    break;

                case "2 Weeks":
                    refreshStartDate = refreshStartDate.AddDays(daysofWeek - 7);
                    break;

                case "1 Month":
                    refreshStartDate = refreshStartDate.AddMonths(-1);
                    break;

                case "3 Months":
                    refreshStartDate = refreshStartDate.AddMonths(-3);
                    break;

                case "6 Months":
                    refreshStartDate = refreshStartDate.AddMonths(-6);
                    break;

                case "1 Year":
                    refreshStartDate = refreshStartDate.AddDays(-360);
                    break;

                case "Maximum":
                    if (Periodicity == Periodicity.EndOfDay)
                        refreshStartDate = refreshStartDate.AddYears(-FTDataSource.Workspace.NumBars / 200);    // ~years calculated based on DB size
                    else if (Periodicity > Periodicity.FifteenSeconds)
                        refreshStartDate = DateTime.Now.AddDays(-IBClientHelper.MaxDownloadDaysOfMediumBars);   // ~2.5 years
                    else
                        refreshStartDate = DateTime.Now.AddDays(-IBClientHelper.MaxDownloadDaysOfSmallBars);    // ~180 days
                    break;

                default:
                    refreshStartDate = refreshStartDate.AddDays(-7);
                    break;
            }
            return refreshStartDate;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Timer event handler 
        /// </summary>
        /// <param name="sender"></param>
        /// <remarks>
        /// If connection is broken, it tries to reconnect in every 30 secs.
        /// If it reconnects, it starts a backfill of all tickers
        /// If status is ready and autorefresh is configured, it starts the scheduled refresh 
        /// </remarks>
        private void timer_Tick(object sender)
        {
            if (controller == null)
                return;

            // check and indicate thread entry
            if (Interlocked.CompareExchange(ref inTimerTick, 1, 0) != 0)
                return;

            IBPluginState currPluginState = IBPluginState.Disconnected;

            try
            {
                currPluginState = controller.GetIBPluginState();

                if (currPluginState != prevPluginState)
                    LogAndMessage.Log(MessageType.Info, "Plugin status: " + currPluginState);

                // if no connection between the data plugin and the TWS client
                if (currPluginState == IBPluginState.Disconnected)
                {
                    // if not manually disconnected, try to reconnect
                    if (manuallyDisconnected == false)
                    {
                        // if retry period has elapsed
                        if (connectionRetryTime < DateTime.Now)
                        {
                            // if connection has just got broken (prevent repeated log entries)
                            if (prevPluginState != IBPluginState.Disconnected)
                                LogAndMessage.LogAndQueue(MessageType.Warning, "Data plugin has been disconnected from TWS. Trying to reconnect in every " + ConnectionRetryInterval + " sec.");

                            // set new retry time and increase interval up to 30 secs
                            connectionRetryTime = DateTime.Now.AddSeconds(ConnectionRetryInterval);

                            // try to reconnect
                            controller.Connect(true);
                        }
                    }
                }

                // data plugin is connected to TWS
                else
                {
                    // reset connection retrying
                    connectionRetryTime = DateTime.MinValue;

                    // if an existing connection  was disconnected
                    if (prevPluginState == IBPluginState.Disconnected && !firstConnection || controller.RestartStreaming)
                    {
                        LogAndMessage.LogAndQueue(MessageType.Warning, "TWS has been reconnected. Starting database refresh.");

                        // request refresh of all tickers, restart subscriptions, etc.
                        controller.RestartAfterReconnect(lastUptodateTime);

                        return;
                    }

                    // clear flag of first connection
                    firstConnection = false;

                    // check if it's time to run AutoRefresh
                    if (prevPluginState == IBPluginState.Ready)             // Plugin works with up to date data
                    {
                        lastUptodateTime = DateTime.Now;

                        if (IBConfiguration != null && IBConfiguration.AutoRefreshEnabled     // Auto refresh is enabled
                         && nextAutoRefreshTime < DateTime.Now)             // The time of auto refresh has passed
                        {
                            DateTime refreshStartDate = DateTime.Now.Date.AddDays(-IBConfiguration.AutoRefreshDays);
                            LogAndMessage.LogAndQueue(MessageType.Warning, "Starting automatic database refresh (" + refreshStartDate.ToShortDateString() + ").");

                            controller.RefreshAllUsed(refreshStartDate);

                            CalcNextAutoRefreshTime();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogAndMessage.Log(MessageType.Error, "Error in data source timer event handler: " + ex);
            }
            finally
            {
                // update plugin state
                prevPluginState = currPluginState;

                // indicate thread exit
                Interlocked.Exchange(ref inTimerTick, 0);
            }
        }

        /// <summary>
        /// Calculate the date and time when next auto refresh is due
        /// </summary>
        private void CalcNextAutoRefreshTime()
        {
            DateTime todaysRefreshTime = DateTime.Now.Date.Add(IBConfiguration.AutoRefreshTime.TimeOfDay);
            if (DateTime.Now < todaysRefreshTime)
                nextAutoRefreshTime = todaysRefreshTime;
            else
                nextAutoRefreshTime = todaysRefreshTime.AddDays(1);
        }

        private string GetWebIdOfTicker(string ticker)
        {
            if (controller == null)
                return null;

            TickerData tickerData = controller.GetTickerData(ticker);

            if (tickerData == null || tickerData.ContractDetails == null || tickerData.ContractDetails.Contract == null)
                return null;

            return tickerData.ContractDetails.Contract.ConId.ToString();
        }

        private DateTime GetHeadTimeStampOfTicker(string ticker)
        {
            if (controller == null)
                return DateTime.MinValue;

            TickerData tickerData = controller.GetTickerData(ticker);

            if (tickerData == null || tickerData.HeadTimestampStatus != HeadTimestampStatus.Ok)
                return DateTime.MinValue;

            return tickerData.EarliestDataPoint;
        }

        private void StartBackfills(DateTime refreshStartDate, StringCollection tickersToBackfill)
        {
            if (tickersToBackfill.Count == 0)
                return;

            //
            // checking if long download is accepted
            //

            int stepSize = IBClientHelper.GetDownloadStep(Periodicity);
            TimeSpan ts = DateTime.Now.Subtract(refreshStartDate);
            int requestsNo = (int)Math.Ceiling(ts.TotalMinutes / stepSize * tickersToBackfill.Count);
            // if mixed database, add number of EOD requests
            requestsNo = (Periodicity != Periodicity.EndOfDay && Workspace.AllowMixedEODIntra != 0) ? requestsNo + tickersToBackfill.Count : requestsNo;

            //
            // checking if database can accomodate the data
            //

            // quoteNum is aproximate value
            int quoteNum = (int)(ts.TotalSeconds / (int)Periodicity);       // total number of bars if traded 24 hours
            if (Periodicity != Periodicity.EndOfDay)
            {
                if (IBConfiguration.RthOnly)
                    quoteNum /= 3;
            }

            //
            // building and showing warning
            //

            bool tooManyRequest = requestsNo > 3;
            bool tooManyQuotes = quoteNum / 1.2 > Workspace.NumBars;

            if (tooManyRequest || tooManyQuotes)
            {
                TimeSpan mts = new TimeSpan(0, 0, requestsNo * 10);

                StringBuilder msg = new StringBuilder(500);

                msg.Append("The requested data refresh");

                if (tooManyRequest)
                {
                    msg.Append(" may download more bar data (~");
                    msg.Append((quoteNum / 1.2).ToString("N0"));
                    msg.Append(") than your database can accomodate (");
                    msg.Append(Workspace.NumBars.ToString());
                    msg.Append(")");
                }
                if (tooManyRequest)
                {
                    if (tooManyRequest)
                        msg.Append(" and it");

                    msg.Append(" may start a long data download operation (~");
                    msg.Append(mts);
                    msg.Append(")");
                }

                msg.AppendLine(".");
                msg.AppendLine("Do you still want it?");

                if (MessageBox.Show(msg.ToString(),
                        "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                    return;
            }

            //
            // start backfills of selected length
            //

            foreach (var ticker in tickersToBackfill)
            {
                if (!controller.RefreshTicker(ticker, refreshStartDate))
                    LogAndMessage.LogAndQueue(MessageType.Warning, ticker + ": Cannot start manual backfill because ticker is being backfilled.");
            }
        }

        #endregion
    }
}
