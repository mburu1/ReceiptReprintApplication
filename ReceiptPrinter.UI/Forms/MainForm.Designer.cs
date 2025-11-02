namespace ReceiptPrinter.UI.Forms
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.txtTransactionNumber = new System.Windows.Forms.TextBox();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnPrint = new System.Windows.Forms.Button();
            this.chkAllStores = new System.Windows.Forms.CheckBox();
            this.lblStore = new System.Windows.Forms.Label();
            this.cmbStores = new System.Windows.Forms.ComboBox();
            this.numFontSize = new System.Windows.Forms.NumericUpDown();
            this.lblFontSize = new System.Windows.Forms.Label();
            this.rbEscPos = new System.Windows.Forms.RadioButton();
            this.rbWindowsDriver = new System.Windows.Forms.RadioButton();
            this.cmbPrinters = new System.Windows.Forms.ComboBox();
            this.lblSendTo = new System.Windows.Forms.Label();
            this.pnlPreview = new ReceiptPrinter.UI.Controls.ReceiptPreviewControl();
            ((System.ComponentModel.ISupportInitialize)(this.numFontSize)).BeginInit();
            this.SuspendLayout();
            // 
            // txtTransactionNumber
            // 
            this.txtTransactionNumber.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtTransactionNumber.ForeColor = System.Drawing.SystemColors.GrayText;
            this.txtTransactionNumber.Location = new System.Drawing.Point(12, 12);
            this.txtTransactionNumber.Name = "txtTransactionNumber";
            this.txtTransactionNumber.Size = new System.Drawing.Size(140, 23);
            this.txtTransactionNumber.TabIndex = 0;
            this.txtTransactionNumber.Text = "Enter Transaction #";
            // 
            // btnGenerate
            // 
            this.btnGenerate.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnGenerate.Location = new System.Drawing.Point(158, 11);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(75, 25);
            this.btnGenerate.TabIndex = 1;
            this.btnGenerate.Text = "Generate";
            this.btnGenerate.UseVisualStyleBackColor = true;
            // 
            // btnRefresh
            // 
            this.btnRefresh.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnRefresh.Location = new System.Drawing.Point(239, 11);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(75, 25);
            this.btnRefresh.TabIndex = 2;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            // 
            // btnPrint
            // 
            this.btnPrint.Enabled = false;
            this.btnPrint.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnPrint.Location = new System.Drawing.Point(350, 11);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new System.Drawing.Size(75, 25);
            this.btnPrint.TabIndex = 3;
            this.btnPrint.Text = "Print";
            this.btnPrint.UseVisualStyleBackColor = true;
            // 
            // chkAllStores
            // 
            this.chkAllStores.AutoSize = true;
            this.chkAllStores.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.chkAllStores.Location = new System.Drawing.Point(12, 45);
            this.chkAllStores.Name = "chkAllStores";
            this.chkAllStores.Size = new System.Drawing.Size(76, 19);
            this.chkAllStores.TabIndex = 4;
            this.chkAllStores.Text = "All Stores";
            this.chkAllStores.UseVisualStyleBackColor = true;
            // 
            // lblStore
            // 
            this.lblStore.AutoSize = true;
            this.lblStore.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblStore.Location = new System.Drawing.Point(100, 46);
            this.lblStore.Name = "lblStore";
            this.lblStore.Size = new System.Drawing.Size(36, 15);
            this.lblStore.TabIndex = 12;
            this.lblStore.Text = "Store:";
            // 
            // cmbStores
            // 
            this.cmbStores.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbStores.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cmbStores.FormattingEnabled = true;
            this.cmbStores.Location = new System.Drawing.Point(140, 42);
            this.cmbStores.Name = "cmbStores";
            this.cmbStores.Size = new System.Drawing.Size(185, 23);
            this.cmbStores.TabIndex = 13;
            // 
            // numFontSize
            // 
            this.numFontSize.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.numFontSize.Location = new System.Drawing.Point(12, 88);
            this.numFontSize.Maximum = new decimal(new int[] { 20, 0, 0, 0 });
            this.numFontSize.Minimum = new decimal(new int[] { 6, 0, 0, 0 });
            this.numFontSize.Name = "numFontSize";
            this.numFontSize.Size = new System.Drawing.Size(50, 23);
            this.numFontSize.TabIndex = 5;
            this.numFontSize.Value = new decimal(new int[] { 10, 0, 0, 0 });
            // 
            // lblFontSize
            // 
            this.lblFontSize.AutoSize = true;
            this.lblFontSize.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblFontSize.Location = new System.Drawing.Point(68, 91);
            this.lblFontSize.Name = "lblFontSize";
            this.lblFontSize.Size = new System.Drawing.Size(57, 15);
            this.lblFontSize.TabIndex = 6;
            this.lblFontSize.Text = "Font Size";
            // 
            // rbEscPos
            // 
            this.rbEscPos.AutoSize = true;
            this.rbEscPos.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.rbEscPos.Location = new System.Drawing.Point(12, 120);
            this.rbEscPos.Name = "rbEscPos";
            this.rbEscPos.Size = new System.Drawing.Size(71, 19);
            this.rbEscPos.TabIndex = 7;
            this.rbEscPos.Text = "ESC/POS";
            this.rbEscPos.UseVisualStyleBackColor = true;
            // 
            // rbWindowsDriver
            // 
            this.rbWindowsDriver.AutoSize = true;
            this.rbWindowsDriver.Checked = true;
            this.rbWindowsDriver.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.rbWindowsDriver.Location = new System.Drawing.Point(89, 120);
            this.rbWindowsDriver.Name = "rbWindowsDriver";
            this.rbWindowsDriver.Size = new System.Drawing.Size(110, 19);
            this.rbWindowsDriver.TabIndex = 8;
            this.rbWindowsDriver.TabStop = true;
            this.rbWindowsDriver.Text = "Windows Driver";
            this.rbWindowsDriver.UseVisualStyleBackColor = true;
            // 
            // lblSendTo
            // 
            this.lblSendTo.AutoSize = true;
            this.lblSendTo.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblSendTo.Location = new System.Drawing.Point(12, 147);
            this.lblSendTo.Name = "lblSendTo";
            this.lblSendTo.Size = new System.Drawing.Size(51, 15);
            this.lblSendTo.TabIndex = 9;
            this.lblSendTo.Text = "Send To:";
            // 
            // cmbPrinters
            // 
            this.cmbPrinters.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPrinters.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cmbPrinters.FormattingEnabled = true;
            this.cmbPrinters.Location = new System.Drawing.Point(12, 165);
            this.cmbPrinters.Name = "cmbPrinters";
            this.cmbPrinters.Size = new System.Drawing.Size(280, 23);
            this.cmbPrinters.TabIndex = 10;
            // 
            // pnlPreview
            // 
            this.pnlPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlPreview.BackColor = System.Drawing.Color.White;
            this.pnlPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlPreview.Location = new System.Drawing.Point(12, 200);
            this.pnlPreview.Name = "pnlPreview";
            this.pnlPreview.Size = new System.Drawing.Size(413, 338);
            this.pnlPreview.TabIndex = 11;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(437, 550);
            // Add controls in a logical visual order
            this.Controls.Add(this.pnlPreview);
            this.Controls.Add(this.cmbPrinters);
            this.Controls.Add(this.lblSendTo);
            this.Controls.Add(this.rbWindowsDriver);
            this.Controls.Add(this.rbEscPos);
            this.Controls.Add(this.lblFontSize);
            this.Controls.Add(this.numFontSize);
            this.Controls.Add(this.cmbStores);
            this.Controls.Add(this.lblStore);
            this.Controls.Add(this.chkAllStores);
            this.Controls.Add(this.btnPrint);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.btnGenerate);
            this.Controls.Add(this.txtTransactionNumber);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(450, 500);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Receipt Reprint";
            ((System.ComponentModel.ISupportInitialize)(this.numFontSize)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TextBox txtTransactionNumber;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnPrint;
        private System.Windows.Forms.CheckBox chkAllStores;
        private System.Windows.Forms.Label lblStore;
        private System.Windows.Forms.ComboBox cmbStores;
        private System.Windows.Forms.NumericUpDown numFontSize;
        private System.Windows.Forms.Label lblFontSize;
        private System.Windows.Forms.RadioButton rbEscPos;
        private System.Windows.Forms.RadioButton rbWindowsDriver;
        private System.Windows.Forms.ComboBox cmbPrinters;
        private System.Windows.Forms.Label lblSendTo;
        private ReceiptPrinter.UI.Controls.ReceiptPreviewControl pnlPreview;
    }
}
