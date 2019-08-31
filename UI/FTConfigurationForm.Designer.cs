namespace AmiBroker.DataSources.FT
{
    partial class FTConfigureForm
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
            this.buttonOk = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.checkBoxFilter = new System.Windows.Forms.CheckBox();
            this.checkBoxVerboseLog = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxHostId = new System.Windows.Forms.TextBox();
            this.textBoxIPPort = new System.Windows.Forms.TextBox();
            this.textBoxIPAddress = new System.Windows.Forms.TextBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.numericUpDownAutoRefreshDays = new System.Windows.Forms.NumericUpDown();
            this.dateTimePickerAutoRefreshTime = new System.Windows.Forms.DateTimePicker();
            this.label11 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.checkBoxAutoRefreshEnabled = new System.Windows.Forms.CheckBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.checkBoxSymbolUpdate = new System.Windows.Forms.CheckBox();
            this.checkBoxRthOnly = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownAutoRefreshDays)).BeginInit();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonOk
            // 
            this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOk.Location = new System.Drawing.Point(167, 373);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 4;
            this.buttonOk.Text = "&Ok";
            this.buttonOk.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(248, 374);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "&Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // checkBoxFilter
            // 
            this.checkBoxFilter.AutoSize = true;
            this.checkBoxFilter.Checked = true;
            this.checkBoxFilter.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxFilter.Location = new System.Drawing.Point(97, 50);
            this.checkBoxFilter.Name = "checkBoxFilter";
            this.checkBoxFilter.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.checkBoxFilter.Size = new System.Drawing.Size(94, 17);
            this.checkBoxFilter.TabIndex = 0;
            this.checkBoxFilter.Text = "Bad Tick Filter";
            this.checkBoxFilter.UseVisualStyleBackColor = true;
            // 
            // checkBoxVerboseLog
            // 
            this.checkBoxVerboseLog.AutoSize = true;
            this.checkBoxVerboseLog.Location = new System.Drawing.Point(97, 96);
            this.checkBoxVerboseLog.Name = "checkBoxVerboseLog";
            this.checkBoxVerboseLog.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.checkBoxVerboseLog.Size = new System.Drawing.Size(106, 17);
            this.checkBoxVerboseLog.TabIndex = 1;
            this.checkBoxVerboseLog.Text = "Verbose Logging";
            this.checkBoxVerboseLog.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.textBoxHostId);
            this.groupBox1.Controls.Add(this.textBoxIPPort);
            this.groupBox1.Controls.Add(this.textBoxIPAddress);
            this.groupBox1.Location = new System.Drawing.Point(5, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(329, 118);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Connection";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(189, 82);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(132, 13);
            this.label5.TabIndex = 17;
            this.label5.Text = "(0 means using process id)";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(51, 82);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(45, 13);
            this.label3.TabIndex = 16;
            this.label3.Text = "Client Id";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(29, 56);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(67, 13);
            this.label2.TabIndex = 15;
            this.label2.Text = "TWS IP Port";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(86, 13);
            this.label1.TabIndex = 14;
            this.label1.Text = "TWS IP Address";
            // 
            // textBoxHostId
            // 
            this.textBoxHostId.Location = new System.Drawing.Point(97, 79);
            this.textBoxHostId.Name = "textBoxHostId";
            this.textBoxHostId.Size = new System.Drawing.Size(92, 20);
            this.textBoxHostId.TabIndex = 2;
            this.textBoxHostId.TextChanged += new System.EventHandler(this.textBox_TextChanged);
            // 
            // textBoxIPPort
            // 
            this.textBoxIPPort.Location = new System.Drawing.Point(97, 53);
            this.textBoxIPPort.Name = "textBoxIPPort";
            this.textBoxIPPort.Size = new System.Drawing.Size(92, 20);
            this.textBoxIPPort.TabIndex = 1;
            this.textBoxIPPort.TextChanged += new System.EventHandler(this.textBox_TextChanged);
            // 
            // textBoxIPAddress
            // 
            this.textBoxIPAddress.AutoCompleteCustomSource.AddRange(new string[] {
            "127.0.0.1",
            "10.0.0.1",
            "192.168.0.1"});
            this.textBoxIPAddress.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.textBoxIPAddress.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.textBoxIPAddress.Location = new System.Drawing.Point(97, 27);
            this.textBoxIPAddress.Name = "textBoxIPAddress";
            this.textBoxIPAddress.Size = new System.Drawing.Size(184, 20);
            this.textBoxIPAddress.TabIndex = 0;
            this.textBoxIPAddress.TextChanged += new System.EventHandler(this.textBox_TextChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.numericUpDownAutoRefreshDays);
            this.groupBox3.Controls.Add(this.dateTimePickerAutoRefreshTime);
            this.groupBox3.Controls.Add(this.label11);
            this.groupBox3.Controls.Add(this.label8);
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Controls.Add(this.checkBoxAutoRefreshEnabled);
            this.groupBox3.Location = new System.Drawing.Point(5, 131);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(329, 100);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Auto refresh";
            // 
            // numericUpDownAutoRefreshDays
            // 
            this.numericUpDownAutoRefreshDays.Enabled = false;
            this.numericUpDownAutoRefreshDays.Location = new System.Drawing.Point(97, 67);
            this.numericUpDownAutoRefreshDays.Maximum = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.numericUpDownAutoRefreshDays.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownAutoRefreshDays.Name = "numericUpDownAutoRefreshDays";
            this.numericUpDownAutoRefreshDays.Size = new System.Drawing.Size(62, 20);
            this.numericUpDownAutoRefreshDays.TabIndex = 2;
            this.numericUpDownAutoRefreshDays.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // dateTimePickerAutoRefreshTime
            // 
            this.dateTimePickerAutoRefreshTime.CustomFormat = "hh:mm tt";
            this.dateTimePickerAutoRefreshTime.Enabled = false;
            this.dateTimePickerAutoRefreshTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePickerAutoRefreshTime.Location = new System.Drawing.Point(97, 40);
            this.dateTimePickerAutoRefreshTime.Name = "dateTimePickerAutoRefreshTime";
            this.dateTimePickerAutoRefreshTime.ShowUpDown = true;
            this.dateTimePickerAutoRefreshTime.Size = new System.Drawing.Size(92, 20);
            this.dateTimePickerAutoRefreshTime.TabIndex = 1;
            this.dateTimePickerAutoRefreshTime.Value = new System.DateTime(2000, 1, 1, 1, 0, 0, 0);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(65, 71);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(31, 13);
            this.label11.TabIndex = 3;
            this.label11.Text = "Days";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(45, 44);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(51, 13);
            this.label8.TabIndex = 2;
            this.label8.Text = "Start time";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(56, 20);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(40, 13);
            this.label7.TabIndex = 1;
            this.label7.Text = "Enable";
            // 
            // checkBoxAutoRefreshEnabled
            // 
            this.checkBoxAutoRefreshEnabled.AutoSize = true;
            this.checkBoxAutoRefreshEnabled.Location = new System.Drawing.Point(97, 19);
            this.checkBoxAutoRefreshEnabled.Name = "checkBoxAutoRefreshEnabled";
            this.checkBoxAutoRefreshEnabled.Size = new System.Drawing.Size(15, 14);
            this.checkBoxAutoRefreshEnabled.TabIndex = 0;
            this.checkBoxAutoRefreshEnabled.UseVisualStyleBackColor = true;
            this.checkBoxAutoRefreshEnabled.CheckedChanged += new System.EventHandler(this.checkBoxAutoRefreshEnabled_CheckedChanged);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.checkBoxSymbolUpdate);
            this.groupBox4.Controls.Add(this.checkBoxRthOnly);
            this.groupBox4.Controls.Add(this.checkBoxFilter);
            this.groupBox4.Controls.Add(this.checkBoxVerboseLog);
            this.groupBox4.Location = new System.Drawing.Point(5, 238);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(329, 129);
            this.groupBox4.TabIndex = 3;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Miscallenous";
            // 
            // checkBoxSymbolUpdate
            // 
            this.checkBoxSymbolUpdate.AutoSize = true;
            this.checkBoxSymbolUpdate.Checked = true;
            this.checkBoxSymbolUpdate.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxSymbolUpdate.Location = new System.Drawing.Point(97, 73);
            this.checkBoxSymbolUpdate.Name = "checkBoxSymbolUpdate";
            this.checkBoxSymbolUpdate.Size = new System.Drawing.Size(172, 17);
            this.checkBoxSymbolUpdate.TabIndex = 3;
            this.checkBoxSymbolUpdate.Text = "Auto Symbol Update at Backfill";
            this.checkBoxSymbolUpdate.UseVisualStyleBackColor = true;
            // 
            // checkBoxRthOnly
            // 
            this.checkBoxRthOnly.AutoSize = true;
            this.checkBoxRthOnly.Location = new System.Drawing.Point(97, 25);
            this.checkBoxRthOnly.Name = "checkBoxRthOnly";
            this.checkBoxRthOnly.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.checkBoxRthOnly.Size = new System.Drawing.Size(177, 17);
            this.checkBoxRthOnly.TabIndex = 2;
            this.checkBoxRthOnly.Text = "Backfill Reg.Trading Hours Only";
            this.checkBoxRthOnly.UseVisualStyleBackColor = true;
            // 
            // IBConfigureForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(339, 404);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "IBConfigureForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Configure Interactive Brokers Data Source";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownAutoRefreshDays)).EndInit();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.CheckBox checkBoxFilter;
        private System.Windows.Forms.CheckBox checkBoxVerboseLog;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxHostId;
        private System.Windows.Forms.TextBox textBoxIPPort;
        private System.Windows.Forms.TextBox textBoxIPAddress;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox checkBoxAutoRefreshEnabled;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.DateTimePicker dateTimePickerAutoRefreshTime;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.NumericUpDown numericUpDownAutoRefreshDays;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.CheckBox checkBoxRthOnly;
        private System.Windows.Forms.CheckBox checkBoxSymbolUpdate;
    }
}