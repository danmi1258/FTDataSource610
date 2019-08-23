namespace AmiBroker.DataSources.IB
{
    partial class WatchlistForm
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
            this.components = new System.ComponentModel.Container();
            this.labelBoxWatchlists = new System.Windows.Forms.Label();
            this.listBoxWatchlists = new System.Windows.Forms.CheckedListBox();
            this.buttonOk = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // labelBoxWatchlists
            // 
            this.labelBoxWatchlists.AutoSize = true;
            this.labelBoxWatchlists.ForeColor = System.Drawing.Color.Gray;
            this.labelBoxWatchlists.Location = new System.Drawing.Point(13, 362);
            this.labelBoxWatchlists.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelBoxWatchlists.Name = "labelBoxWatchlists";
            this.labelBoxWatchlists.Size = new System.Drawing.Size(73, 17);
            this.labelBoxWatchlists.TabIndex = 5;
            this.labelBoxWatchlists.Text = "Database:";
            this.toolTip1.SetToolTip(this.labelBoxWatchlists, "Save the database (File - Save Database) to see changes of watchlists.");
            // 
            // listBoxWatchlists
            // 
            this.listBoxWatchlists.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxWatchlists.CheckOnClick = true;
            this.listBoxWatchlists.FormattingEnabled = true;
            this.listBoxWatchlists.Location = new System.Drawing.Point(13, 14);
            this.listBoxWatchlists.Margin = new System.Windows.Forms.Padding(4);
            this.listBoxWatchlists.Name = "listBoxWatchlists";
            this.listBoxWatchlists.Size = new System.Drawing.Size(296, 344);
            this.listBoxWatchlists.TabIndex = 4;
            // 
            // buttonOk
            // 
            this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOk.Location = new System.Drawing.Point(103, 382);
            this.buttonOk.Margin = new System.Windows.Forms.Padding(4);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(100, 28);
            this.buttonOk.TabIndex = 2;
            this.buttonOk.Text = "&Ok";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(211, 382);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(4);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(100, 28);
            this.buttonCancel.TabIndex = 2;
            this.buttonCancel.Text = "&Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // toolTip1
            // 
            this.toolTip1.IsBalloon = true;
            this.toolTip1.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.toolTip1.ToolTipTitle = "Note:";
            // 
            // WatchlistForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(325, 422);
            this.Controls.Add(this.labelBoxWatchlists);
            this.Controls.Add(this.listBoxWatchlists);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.buttonCancel);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(686, 973);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(341, 235);
            this.Name = "WatchlistForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Select watchlists";
            this.toolTip1.SetToolTip(this, "Save the database (File - Save Database) to see changes of watchlists.");
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelBoxWatchlists;
        private System.Windows.Forms.CheckedListBox listBoxWatchlists;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}