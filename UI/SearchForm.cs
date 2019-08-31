using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using IBApi;

namespace AmiBroker.DataSources.FT
{
    internal partial class SearchForm : Form
    {
        private const string MsgBoxCaption = "Search input error";
        private static string[] SecTypes = { "STK", "OPT", "FUT", "IND", "FOP", "CASH", "BOND", "WAR", "CMDTY", "FUND", "CFD" };
        private const int maxDisplayedContract = 1000;

        private FTController controller;
        private string searchedType;

        private List<SecurityData> securityDatas;
        private BindingSource bindingSource = new BindingSource();

        private bool sorting;
        private Contract lastSearchedContract;
        private System.Windows.Forms.Timer timer;

        public SearchForm(FTController controller)
        {
            this.controller = controller;
            controller.OnContractListReady += SearchContractReady;
            controller.OnContractReady += ContractReady;

            InitializeComponent();

            dataGridViewResult.DataSource = bindingSource;
            dataGridViewResult.AutoGenerateColumns = false;
            comboBoxSecType.SelectedIndex = 0;

            timer = new System.Windows.Forms.Timer();
            timer.Interval = 500;
            timer.Tick += timer_tick;
        }

        private void buttonSearch_Click(object sender, EventArgs e)
        {
            //
            // check input values' validity
            //
            if (textBoxSymbol.Text.Trim().Length == 0)
            {
                MessageBox.Show("Symbol is a required search parameter.",
                    MsgBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            double stripePrice;
            if (textBoxStrike.Text.Trim().Length > 0)
                if (!double.TryParse(textBoxStrike.Text, out stripePrice))
                {
                    MessageBox.Show("Strike price must be a valid number!",
                        MsgBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }

            if (textBoxCurrency.Text.Trim().Length != 0)
                if (textBoxCurrency.Text.Trim().Length != 3)
                {
                    MessageBox.Show("Currency must be a three letter text value.",
                        MsgBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }

            //
            // build contract to search for
            //

            searchedType = SecTypes[comboBoxSecType.SelectedIndex];

            Contract contract = new Contract();
            contract.Symbol = textBoxSymbol.Text.Trim();
            contract.SecType = searchedType;
            contract.Exchange = textBoxExchange.Text.Trim();
            contract.Currency = textBoxCurrency.Text.Trim();
            contract.IncludeExpired = checkBoxExpired.Checked;
            if (textBoxStrike.Text.Trim().Length > 0)
                contract.Strike = double.Parse(textBoxStrike.Text);
            else
                contract.Strike = 0;

            //
            // compare current to previous search
            //

            // if previous seearch matches current search...
            if (lastSearchedContract != null
             && lastSearchedContract.Symbol == contract.Symbol
             && lastSearchedContract.SecType == contract.SecType
             && lastSearchedContract.Exchange == contract.Exchange
             && lastSearchedContract.Currency == contract.Currency
             && lastSearchedContract.IncludeExpired == contract.IncludeExpired
             && lastSearchedContract.Strike == contract.Strike)
            {
                return;
            }

            // save as last search
            lastSearchedContract = contract;

            // reset result and UI
            securityDatas = new List<SecurityData>();
            labelContractsNo.Text = "0";
            labelStatus.Text = "Requesting contracts from TWS...";
            labelStatus.ForeColor = Label.DefaultForeColor;
            sorting = false;

            // reset and disable UI
            bindingSource.DataSource = null;

            textBoxSymbol.Enabled = false;
            comboBoxSecType.Enabled = false;
            textBoxExchange.Enabled = false;
            textBoxCurrency.Enabled = false;
            textBoxStrike.Enabled = false;
            checkBoxExpired.Enabled = false;

            buttonSearch.Enabled = false;

            dataGridViewResult.Enabled = false;
            dataGridViewResult.Columns.Clear();
            dataGridViewResult.SuspendLayout();

            buttonAdd.Enabled = false;

            // start search on bck thread
            ThreadPool.QueueUserWorkItem(new WaitCallback(SearchContract), contract);

            // start timer to update contract counter on the UI
            timer.Start();
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            if (dataGridViewResult.SelectedRows.Count == 0)
                return;

            try
            {
                Type appType = Type.GetTypeFromProgID("Broker.Application");
                object abApp = Activator.CreateInstance(appType);

                object abStocks = appType.InvokeMember("Stocks", System.Reflection.BindingFlags.GetProperty, null, abApp, null);
                Type stocksType = abStocks.GetType();

                for (int i = 0; i < dataGridViewResult.SelectedRows.Count; i++)
                {
                    SecurityData sd = (SecurityData)dataGridViewResult.SelectedRows[i].DataBoundItem; ;

                    SymbolParts ibTicker = new SymbolParts(sd.LocalSymbol, sd.Exchange, sd.SecType, sd.Currency, "");

                    object stock = stocksType.InvokeMember("Add", System.Reflection.BindingFlags.InvokeMethod, null,
                                                           abStocks, new object[] { ibTicker.Ticker });
                    if (stock != null)
                    {
                        Type stockType = stock.GetType();
                        stockType.InvokeMember("Alias", System.Reflection.BindingFlags.SetProperty, null, stock,
                                               new object[] { sd.Symbol });
                        stockType.InvokeMember("Currency", System.Reflection.BindingFlags.SetProperty, null, stock,
                                               new object[] { sd.Currency });
                        stockType.InvokeMember("FullName", System.Reflection.BindingFlags.SetProperty, null, stock,
                                               new object[] { sd.LongName });
                        stockType.InvokeMember("PointValue", System.Reflection.BindingFlags.SetProperty, null, stock,
                                               new object[] { sd.PriceMagnifier });
                        stockType.InvokeMember("TickSize", System.Reflection.BindingFlags.SetProperty, null, stock,
                                               new object[] { sd.MinTick });
                        stockType.InvokeMember("WebID", System.Reflection.BindingFlags.SetProperty, null, stock,
                                               new object[] { sd.ContractId });
                    }

                    if (checkBoxAddContinuous.Checked && sd.SecType.ToUpper() == "FUT")
                    {
                        string localNamePart = sd.LocalSymbol.Substring(0, sd.LocalSymbol.Length - 2);
                        SymbolParts contTicker = new SymbolParts(localNamePart + "~/" + sd.Symbol, sd.Exchange, sd.SecType, sd.Currency, "");

                        stock = stocksType.InvokeMember("Add", System.Reflection.BindingFlags.InvokeMethod, null,
                                                               abStocks, new object[] { contTicker.Ticker });
                    }
                }
            }
            catch (Exception ex)
            {
                LogAndMessage.Log(MessageType.Error, "Search form could not add tickers: " + ex);
                MessageBox.Show("Error while adding tickers: " + ex, "Search form error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void comboBoxSecType_SelectedIndexChanged(object sender, EventArgs e)
        {
            checkBoxAddContinuous.Visible = ((string)comboBoxSecType.SelectedItem).CompareTo("Future") == 0;
        }

        private void dataGridViewResult_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            // if there are contracts in the result set
            if (securityDatas != null && securityDatas.Count > 1)
            {
                labelContractsNo.Text = securityDatas.Count + " Sorting ...";

                SortOrder sortOrder;

                string columnName = (string)dataGridViewResult.Columns[e.ColumnIndex].Name;
                if (dataGridViewResult.Columns[e.ColumnIndex].HeaderCell.SortGlyphDirection == SortOrder.None
                 || dataGridViewResult.Columns[e.ColumnIndex].HeaderCell.SortGlyphDirection == SortOrder.Descending)
                    sortOrder = SortOrder.Ascending;
                else
                    sortOrder = SortOrder.Descending;

                securityDatas.Sort(new SecurityDataComparer((SecurityDataField)Enum.Parse(typeof(SecurityDataField), columnName), sortOrder));

                bindingSource.ResetBindings(false);
                dataGridViewResult.Columns[e.ColumnIndex].HeaderCell.SortGlyphDirection = sortOrder;

                labelContractsNo.Text = securityDatas.Count.ToString();
            }
        }

        private void dataGridViewResult_SelectionChanged(object sender, EventArgs e)
        {
            buttonAdd.Enabled = dataGridViewResult.SelectedRows.Count > 0;
            checkBoxAddContinuous.Enabled = dataGridViewResult.SelectedRows.Count > 0;
        }

        private void timer_tick(object sender, EventArgs e)
        {
            labelContractsNo.Text = securityDatas.Count.ToString();
            labelStatus.Text = sorting ? " Sorting ..." : " Getting contracts from TWS...";
        }

        private void SearchForm_Shown(object sender, EventArgs e)
        {
            // if form was closed while still getting result from controller...
            if (timer.Enabled)
            {
                // we need to show result on next window show event
                ShowContracts();
            }
        }

        // show the collected contracts and reset UI elements
        private void ShowContracts()
        {
            timer.Stop();
            labelStatus.Text = "";

            labelContractsNo.Text = securityDatas.Count.ToString();
            if (securityDatas.Count >= maxDisplayedContract)
            {
                labelStatus.Text = "Set more search parameters. Result was limited to the first " + maxDisplayedContract + " contracts.";
                labelStatus.ForeColor = System.Drawing.Color.Red;
            }

            bindingSource.DataSource = securityDatas;
            dataGridViewResult.Columns.Clear();

            if (searchedType == "IND" || searchedType == "CASH" || searchedType == "CFD")
            {
                dataGridViewResult.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[]
                                                        {
                                                            this.LongName,
                                                            this.Symbol,
                                                            this.LocalSymbol,
                                                            this.SecType,
                                                            //this.PrimaryExchange,
                                                            this.Exchange,
                                                            this.Currency
                                                            //this.LastTradeDateOrContractMonth,
                                                            //this.ColRight,
                                                            //this.Strike
                                                        });
            }
            else if (searchedType == "STK")
            {
                dataGridViewResult.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[]
                                                        {
                                                            this.LongName,
                                                            this.Symbol,
                                                            this.LocalSymbol,
                                                            this.SecType,
                                                            this.PrimaryExchange,
                                                            this.Exchange,
                                                            this.Currency
                                                            //this.LastTradeDateOrContractMonth,
                                                            //this.ColRight,
                                                            //this.Strike
                                                        });
            }
            else if (searchedType == "FUT")
            {
                dataGridViewResult.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[]
                                                        {
                                                            this.LongName,
                                                            this.Symbol,
                                                            this.LocalSymbol,
                                                            this.SecType,
                                                            //this.PrimaryExchange,
                                                            this.Exchange,
                                                            this.Currency,
                                                            this.LastTradeDateOrContractMonth
                                                            //this.ColRight,
                                                            //this.Strike
                                                        });
            }
            else // "OPT", "FOP", "BOND", "WAR"
            {
                dataGridViewResult.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[]
                                                        {
                                                            this.LongName,
                                                            this.Symbol,
                                                            this.LocalSymbol,
                                                            this.SecType,
                                                            // this.PrimaryExchange,
                                                            this.Exchange,
                                                            this.Currency,
                                                            this.LastTradeDateOrContractMonth,
                                                            this.ColRight,
                                                            this.Strike
                                                        });
            }

            dataGridViewResult.Enabled = true;

            if (securityDatas.Count > 1)
            {
                dataGridViewResult.Columns["LocalSymbol"].HeaderCell.SortGlyphDirection = SortOrder.Ascending;
            }

            dataGridViewResult.ResumeLayout(true);

            textBoxSymbol.Enabled = true;
            comboBoxSecType.Enabled = true;
            textBoxExchange.Enabled = true;
            textBoxCurrency.Enabled = true;
            textBoxStrike.Enabled = true;
            checkBoxExpired.Enabled = true;

            buttonSearch.Enabled = true;

            buttonAdd.Enabled = true;
        }

        #region using IBController - asynch request/response

        // search method to be started on bck thread
        private void SearchContract(object contract)
        {
            controller.SearchContract((Contract)contract);
        }

        // callback (from controller) to store a matching contract
        private void ContractReady(ContractDetails contractDetails)
        {
            SecurityData sd = new SecurityData();

            sd.ContractId = contractDetails.Contract.ConId;
            sd.Currency = contractDetails.Contract.Currency;
            sd.Exchange = contractDetails.Contract.Exchange;
            sd.LastTradeDateOrContractMonth = contractDetails.Contract.LastTradeDateOrContractMonth;
            sd.LocalSymbol = contractDetails.Contract.LocalSymbol;
            sd.PrimaryExchange = contractDetails.Contract.PrimaryExch;
            sd.Right = contractDetails.Contract.Right;
            sd.SecType = contractDetails.Contract.SecType;
            sd.Strike = contractDetails.Contract.Strike;
            sd.Symbol = contractDetails.Contract.Symbol;
            sd.LongName = contractDetails.LongName;
            sd.MinTick = (float)contractDetails.MinTick;
            sd.PriceMagnifier = contractDetails.PriceMagnifier;

            securityDatas.Add(sd);
        }

        // callback (from controller) to indicate no more contracts
        private void SearchContractReady()
        {
            try
            {
                // limit result list
                if (securityDatas.Count > maxDisplayedContract)
                    securityDatas.RemoveRange(maxDisplayedContract, securityDatas.Count - maxDisplayedContract);

                // InvokeRequired did not always work! May be called back on UI thread as well.
                // However, there can be a lengthy operation to do it on background thread.
                sorting = true;

                if (securityDatas.Count > 1)
                    securityDatas.Sort(new SecurityDataComparer(SecurityDataField.LocalSymbol, SortOrder.Ascending));

                // if window is not shown, we cannot invoke a method through window message loop
                if (IsHandleCreated)
                    BeginInvoke(new MethodInvoker(ShowContracts));
            }
            catch (Exception ex)
            {
                LogAndMessage.Log(MessageType.Error, "Search form could not collect contracts: " + ex);
                MessageBox.Show("Error while retrieving contracts:" + ex, "Search form error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
        }

        #endregion
    }
}