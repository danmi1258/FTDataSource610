namespace AmiBroker.DataSources.FT
{
    partial class SearchForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.labelContractsNo = new System.Windows.Forms.Label();
            this.labelStatus = new System.Windows.Forms.Label();
            this.textBoxSymbol = new System.Windows.Forms.TextBox();
            this.textBoxExchange = new System.Windows.Forms.TextBox();
            this.textBoxCurrency = new System.Windows.Forms.TextBox();
            this.textBoxStrike = new System.Windows.Forms.TextBox();
            this.buttonSearch = new System.Windows.Forms.Button();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.buttonClose = new System.Windows.Forms.Button();
            this.dataGridViewResult = new System.Windows.Forms.DataGridView();
            this.LongName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Symbol = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.LocalSymbol = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PrimaryExchange = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SecType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Exchange = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Currency = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.LastTradeDateOrContractMonth = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColRight = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Strike = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.groupBox = new System.Windows.Forms.GroupBox();
            this.comboBoxSecType = new System.Windows.Forms.ComboBox();
            this.checkBoxExpired = new System.Windows.Forms.CheckBox();
            this.checkBoxAddContinuous = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewResult)).BeginInit();
            this.groupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 28);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 17);
            this.label1.TabIndex = 4;
            this.label1.Text = "Symbol:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(276, 28);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(49, 17);
            this.label3.TabIndex = 2;
            this.label3.Text = "Type:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 62);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(74, 17);
            this.label4.TabIndex = 11;
            this.label4.Text = "Exchange:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(276, 62);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(69, 17);
            this.label5.TabIndex = 13;
            this.label5.Text = "Currency:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 114);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(162, 17);
            this.label6.TabIndex = 1;
            this.label6.Text = "Total matching contacts:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(540, 62);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(83, 17);
            this.label7.TabIndex = 15;
            this.label7.Text = "Strike price:";
            // 
            // labelContractsNo
            // 
            this.labelContractsNo.AutoSize = true;
            this.labelContractsNo.Location = new System.Drawing.Point(180, 114);
            this.labelContractsNo.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelContractsNo.Name = "labelContractsNo";
            this.labelContractsNo.Size = new System.Drawing.Size(16, 17);
            this.labelContractsNo.TabIndex = 2;
            this.labelContractsNo.Text = "0";
            // 
            // labelStatus
            // 
            this.labelStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(321, 114);
            this.labelStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(0, 17);
            this.labelStatus.TabIndex = 0;
            this.labelStatus.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // textBoxSymbol
            // 
            this.textBoxSymbol.AutoCompleteCustomSource.AddRange(new string[] {
            "GOOG",
            "F",
            "M",
            "MSFT",
            "AUD",
            "CAD",
            "CHF",
            "EUR",
            "GBP",
            "HKD",
            "NZD",
            "SGD",
            "USD",
            "COMP",
            "NDX",
            "SPX"});
            this.textBoxSymbol.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.textBoxSymbol.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.textBoxSymbol.CausesValidation = false;
            this.textBoxSymbol.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.textBoxSymbol.Location = new System.Drawing.Point(91, 23);
            this.textBoxSymbol.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxSymbol.Name = "textBoxSymbol";
            this.textBoxSymbol.Size = new System.Drawing.Size(133, 22);
            this.textBoxSymbol.TabIndex = 0;
            // 
            // textBoxExchange
            // 
            this.textBoxExchange.AutoCompleteCustomSource.AddRange(new string[] {
            "SMART",
            "IDEALPRO",
            "GLOBEX",
            "ONE",
            "NASDAQ",
            "NYSE",
            "ISLAND",
            "ICE",
            "CBOE"});
            this.textBoxExchange.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.textBoxExchange.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.textBoxExchange.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.textBoxExchange.Location = new System.Drawing.Point(91, 57);
            this.textBoxExchange.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxExchange.Name = "textBoxExchange";
            this.textBoxExchange.Size = new System.Drawing.Size(133, 22);
            this.textBoxExchange.TabIndex = 3;
            // 
            // textBoxCurrency
            // 
            this.textBoxCurrency.AutoCompleteCustomSource.AddRange(new string[] {
            "AUD",
            "CAD",
            "CHF",
            "CNH",
            "EUR",
            "GBP",
            "HKD",
            "JPY",
            "SGD",
            "USD",
            "NZD"});
            this.textBoxCurrency.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.textBoxCurrency.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.textBoxCurrency.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.textBoxCurrency.Location = new System.Drawing.Point(348, 57);
            this.textBoxCurrency.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxCurrency.Name = "textBoxCurrency";
            this.textBoxCurrency.Size = new System.Drawing.Size(133, 22);
            this.textBoxCurrency.TabIndex = 4;
            // 
            // textBoxStrike
            // 
            this.textBoxStrike.Location = new System.Drawing.Point(628, 57);
            this.textBoxStrike.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxStrike.Name = "textBoxStrike";
            this.textBoxStrike.Size = new System.Drawing.Size(132, 22);
            this.textBoxStrike.TabIndex = 5;
            // 
            // buttonSearch
            // 
            this.buttonSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSearch.Location = new System.Drawing.Point(844, 55);
            this.buttonSearch.Margin = new System.Windows.Forms.Padding(4);
            this.buttonSearch.Name = "buttonSearch";
            this.buttonSearch.Size = new System.Drawing.Size(100, 28);
            this.buttonSearch.TabIndex = 6;
            this.buttonSearch.Text = "&Search";
            this.buttonSearch.UseVisualStyleBackColor = true;
            this.buttonSearch.Click += new System.EventHandler(this.buttonSearch_Click);
            // 
            // buttonAdd
            // 
            this.buttonAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAdd.Location = new System.Drawing.Point(768, 470);
            this.buttonAdd.Margin = new System.Windows.Forms.Padding(4);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(100, 28);
            this.buttonAdd.TabIndex = 2;
            this.buttonAdd.Text = "&Add";
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // buttonClose
            // 
            this.buttonClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonClose.Location = new System.Drawing.Point(876, 470);
            this.buttonClose.Margin = new System.Windows.Forms.Padding(4);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(100, 28);
            this.buttonClose.TabIndex = 3;
            this.buttonClose.Text = "&Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // dataGridViewResult
            // 
            this.dataGridViewResult.AllowUserToAddRows = false;
            this.dataGridViewResult.AllowUserToDeleteRows = false;
            this.dataGridViewResult.AllowUserToResizeRows = false;
            this.dataGridViewResult.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewResult.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataGridViewResult.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewResult.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridViewResult.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewResult.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.LongName,
            this.Symbol,
            this.LocalSymbol,
            this.PrimaryExchange,
            this.SecType,
            this.Exchange,
            this.Currency,
            this.LastTradeDateOrContractMonth,
            this.ColRight,
            this.Strike});
            this.dataGridViewResult.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dataGridViewResult.Location = new System.Drawing.Point(12, 134);
            this.dataGridViewResult.Margin = new System.Windows.Forms.Padding(4);
            this.dataGridViewResult.Name = "dataGridViewResult";
            this.dataGridViewResult.ReadOnly = true;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewResult.RowHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridViewResult.RowHeadersVisible = false;
            this.dataGridViewResult.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            this.dataGridViewResult.RowTemplate.DefaultCellStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dataGridViewResult.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewResult.Size = new System.Drawing.Size(964, 329);
            this.dataGridViewResult.TabIndex = 3;
            this.dataGridViewResult.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridViewResult_ColumnHeaderMouseClick);
            this.dataGridViewResult.SelectionChanged += new System.EventHandler(this.dataGridViewResult_SelectionChanged);
            // 
            // LongName
            // 
            this.LongName.DataPropertyName = "LongName";
            this.LongName.HeaderText = "Long Name";
            this.LongName.Name = "LongName";
            this.LongName.ReadOnly = true;
            this.LongName.Width = 101;
            // 
            // Symbol
            // 
            this.Symbol.DataPropertyName = "Symbol";
            this.Symbol.HeaderText = "Symbol";
            this.Symbol.Name = "Symbol";
            this.Symbol.ReadOnly = true;
            this.Symbol.Width = 83;
            // 
            // LocalSymbol
            // 
            this.LocalSymbol.DataPropertyName = "LocalSymbol";
            this.LocalSymbol.HeaderText = "Local Symbol";
            this.LocalSymbol.Name = "LocalSymbol";
            this.LocalSymbol.ReadOnly = true;
            this.LocalSymbol.Width = 111;
            // 
            // PrimaryExchange
            // 
            this.PrimaryExchange.DataPropertyName = "PrimaryExchange";
            this.PrimaryExchange.HeaderText = "Primary Exchange";
            this.PrimaryExchange.Name = "PrimaryExchange";
            this.PrimaryExchange.ReadOnly = true;
            this.PrimaryExchange.Width = 138;
            // 
            // SecType
            // 
            this.SecType.DataPropertyName = "SecType";
            this.SecType.HeaderText = "Type";
            this.SecType.Name = "SecType";
            this.SecType.ReadOnly = true;
            this.SecType.Width = 69;
            // 
            // Exchange
            // 
            this.Exchange.DataPropertyName = "Exchange";
            this.Exchange.HeaderText = "Exchange";
            this.Exchange.Name = "Exchange";
            this.Exchange.ReadOnly = true;
            this.Exchange.Width = 99;
            // 
            // Currency
            // 
            this.Currency.DataPropertyName = "Currency";
            this.Currency.HeaderText = "Currency";
            this.Currency.Name = "Currency";
            this.Currency.ReadOnly = true;
            this.Currency.Width = 94;
            // 
            // LastTradeDateOrContractMonth
            // 
            this.LastTradeDateOrContractMonth.DataPropertyName = "LastTradeDateOrContractMonth";
            this.LastTradeDateOrContractMonth.HeaderText = "Contract Month";
            this.LastTradeDateOrContractMonth.Name = "LastTradeDateOrContractMonth";
            this.LastTradeDateOrContractMonth.ReadOnly = true;
            this.LastTradeDateOrContractMonth.Width = 122;
            // 
            // ColRight
            // 
            this.ColRight.DataPropertyName = "Right";
            this.ColRight.HeaderText = "Right";
            this.ColRight.Name = "ColRight";
            this.ColRight.ReadOnly = true;
            this.ColRight.Width = 70;
            // 
            // Strike
            // 
            this.Strike.DataPropertyName = "Strike";
            this.Strike.HeaderText = "Strike";
            this.Strike.Name = "Strike";
            this.Strike.ReadOnly = true;
            this.Strike.Width = 73;
            // 
            // groupBox
            // 
            this.groupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox.Controls.Add(this.comboBoxSecType);
            this.groupBox.Controls.Add(this.checkBoxExpired);
            this.groupBox.Controls.Add(this.textBoxStrike);
            this.groupBox.Controls.Add(this.label7);
            this.groupBox.Controls.Add(this.textBoxCurrency);
            this.groupBox.Controls.Add(this.label5);
            this.groupBox.Controls.Add(this.textBoxExchange);
            this.groupBox.Controls.Add(this.label4);
            this.groupBox.Controls.Add(this.label3);
            this.groupBox.Controls.Add(this.textBoxSymbol);
            this.groupBox.Controls.Add(this.buttonSearch);
            this.groupBox.Controls.Add(this.label1);
            this.groupBox.Location = new System.Drawing.Point(12, 15);
            this.groupBox.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox.Name = "groupBox";
            this.groupBox.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox.Size = new System.Drawing.Size(964, 95);
            this.groupBox.TabIndex = 0;
            this.groupBox.TabStop = false;
            this.groupBox.Text = "Search parameters";
            // 
            // comboBoxSecType
            // 
            this.comboBoxSecType.FormattingEnabled = true;
            this.comboBoxSecType.Items.AddRange(new object[] {
            "Stock",
            "Option",
            "Future",
            "Index",
            "Future Option",
            "Cash",
            "Bond",
            "Warrant",
            "Commodity",
            "Mutual Fund",
            "CFD"});
            this.comboBoxSecType.Location = new System.Drawing.Point(348, 23);
            this.comboBoxSecType.Margin = new System.Windows.Forms.Padding(4);
            this.comboBoxSecType.Name = "comboBoxSecType";
            this.comboBoxSecType.Size = new System.Drawing.Size(133, 24);
            this.comboBoxSecType.TabIndex = 1;
            this.comboBoxSecType.SelectedIndexChanged += new System.EventHandler(this.comboBoxSecType_SelectedIndexChanged);
            // 
            // checkBoxExpired
            // 
            this.checkBoxExpired.AutoSize = true;
            this.checkBoxExpired.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxExpired.Location = new System.Drawing.Point(515, 27);
            this.checkBoxExpired.Margin = new System.Windows.Forms.Padding(4);
            this.checkBoxExpired.Name = "checkBoxExpired";
            this.checkBoxExpired.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.checkBoxExpired.Size = new System.Drawing.Size(126, 21);
            this.checkBoxExpired.TabIndex = 2;
            this.checkBoxExpired.Text = "Include Expired";
            this.checkBoxExpired.UseVisualStyleBackColor = true;
            // 
            // checkBoxAddContinuous
            // 
            this.checkBoxAddContinuous.AutoSize = true;
            this.checkBoxAddContinuous.Checked = true;
            this.checkBoxAddContinuous.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxAddContinuous.Location = new System.Drawing.Point(12, 474);
            this.checkBoxAddContinuous.Margin = new System.Windows.Forms.Padding(4);
            this.checkBoxAddContinuous.Name = "checkBoxAddContinuous";
            this.checkBoxAddContinuous.Size = new System.Drawing.Size(335, 21);
            this.checkBoxAddContinuous.TabIndex = 1;
            this.checkBoxAddContinuous.Text = "Add back-adjusted continuous contract for FUTs";
            this.checkBoxAddContinuous.UseVisualStyleBackColor = true;
            // 
            // SearchForm
            // 
            this.AcceptButton = this.buttonSearch;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonClose;
            this.ClientSize = new System.Drawing.Size(992, 510);
            this.Controls.Add(this.checkBoxAddContinuous);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.groupBox);
            this.Controls.Add(this.dataGridViewResult);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.buttonAdd);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.labelContractsNo);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1061, 728);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(1007, 543);
            this.Name = "SearchForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Shown += new System.EventHandler(this.SearchForm_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewResult)).EndInit();
            this.groupBox.ResumeLayout(false);
            this.groupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label labelContractsNo;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.DataGridView dataGridViewResult;
        private System.Windows.Forms.TextBox textBoxSymbol;
        private System.Windows.Forms.TextBox textBoxExchange;
        private System.Windows.Forms.TextBox textBoxCurrency;
        private System.Windows.Forms.TextBox textBoxStrike;
        private System.Windows.Forms.Button buttonSearch;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.CheckBox checkBoxExpired;
        private System.Windows.Forms.ComboBox comboBoxSecType;
        private System.Windows.Forms.DataGridViewTextBoxColumn LongName;
        private System.Windows.Forms.DataGridViewTextBoxColumn Symbol;
        private System.Windows.Forms.DataGridViewTextBoxColumn LocalSymbol;
        private System.Windows.Forms.DataGridViewTextBoxColumn PrimaryExchange;
        private System.Windows.Forms.DataGridViewTextBoxColumn SecType;
        private System.Windows.Forms.DataGridViewTextBoxColumn Exchange;
        private System.Windows.Forms.DataGridViewTextBoxColumn Currency;
        private System.Windows.Forms.DataGridViewTextBoxColumn LastTradeDateOrContractMonth;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColRight;
        private System.Windows.Forms.DataGridViewTextBoxColumn Strike;
        private System.Windows.Forms.CheckBox checkBoxAddContinuous;
    }
}