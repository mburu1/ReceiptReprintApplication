using ReceiptPrinter.Core.DataAccess;
using ReceiptPrinter.Core.Services;
using ReceiptPrinter.Logging;
using ReceiptPrinter.Logging.Interfaces;
using ReceiptPrinter.Printing;
using ReceiptPrinter.UI.Controls;
using ReceiptPrinter.UI.Presenters;
using System;
using System.Windows.Forms;

namespace ReceiptPrinter.UI.Forms
{
    public partial class MainForm : Form
    {
        private readonly MainFormPresenter _presenter;
        private static readonly ILogger _logger = LogManager.GetLogger();

        public MainForm()
        {
            InitializeComponent();

            // Dependency injection
            var repository = new ReceiptRepository();
            var templateParser = new TemplateParser(repository);
            var duplicateDetector = new DuplicateDetector();
            var receiptService = new ReceiptService(repository, templateParser, duplicateDetector);
            var printSelector = new PrinterSelector();

            _presenter = new MainFormPresenter(this, receiptService, printSelector);

            // Wire events
            btnGenerate.Click += BtnGenerate_Click;
            btnRefresh.Click += BtnRefresh_Click;
            btnPrint.Click += BtnPrint_Click;
            txtTransactionNumber.Enter += TxtTransactionNumber_Enter;
            txtTransactionNumber.Leave += TxtTransactionNumber_Leave;

            // Enable/disable store combo when All Stores toggled
            chkAllStores.CheckedChanged += (s, e) =>
            {
                try
                {
                    cmbStores.Enabled = !chkAllStores.Checked;
                }
                catch
                {
                    // ignore during designer time or if control is missing
                }
            };
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _logger.Info("MainForm loaded.");

            try
            {
                await _presenter.InitializeAsync();

                // Populate printer dropdown (keep sample default)
                cmbPrinters.Items.Add("Send To OneNote 16");
                if (cmbPrinters.Items.Count > 0 && cmbPrinters.SelectedIndex < 0)
                    cmbPrinters.SelectedIndex = 0;

                UpdatePrintButtonState();
            }
            catch (Exception ex)
            {
                _logger.Error("Initialization failed", ex);
                MessageBox.Show($"Error initializing application: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _logger.Info("MainForm closing.");
            base.OnFormClosing(e);
        }

        // ———————————————————— Event Handlers ————————————————————

        private async void BtnGenerate_Click(object sender, EventArgs e)
        {
            try
            {
                string input = txtTransactionNumber.Text.Trim();

                if (string.IsNullOrWhiteSpace(input) || input == "Enter Transaction #")
                {
                    MessageBox.Show("Please enter a valid transaction number.",
                        "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtTransactionNumber.Focus();
                    return;
                }

                await _presenter.GenerateReceiptAsync(input);
                UpdatePrintButtonState();
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to generate receipt", ex);
                MessageBox.Show("Failed to generate receipt. Please try again.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                await _presenter.RefreshAllAsync();
                UpdatePrintButtonState();
                MessageBox.Show("Data refreshed successfully.",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to refresh data", ex);
                MessageBox.Show("Failed to refresh data. Please try again.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnPrint_Click(object sender, EventArgs e)
        {
            try
            {
                if (_presenter.HasReceipt)
                {
                    _presenter.PrintReceipt();
                    return;
                }

                var result = MessageBox.Show(
                    "No receipt has been generated.\n\nWould you like to print a test receipt?",
                    "Print Test Receipt?",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    _presenter.PrintEmptyReceipt();
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to print receipt", ex);
                MessageBox.Show("Failed to print receipt. Please check printer connection.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TxtTransactionNumber_Enter(object sender, EventArgs e)
        {
            if (txtTransactionNumber.Text == "Enter Transaction #")
            {
                txtTransactionNumber.Text = "";
                txtTransactionNumber.ForeColor = System.Drawing.SystemColors.WindowText;
            }
        }

        private void TxtTransactionNumber_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTransactionNumber.Text))
            {
                txtTransactionNumber.Text = "Enter Transaction #";
                txtTransactionNumber.ForeColor = System.Drawing.SystemColors.GrayText;
            }
        }

        // ———————————————————— Helper Methods ————————————————————

        private void UpdatePrintButtonState()
        {
            btnPrint.Enabled = _presenter.HasReceipt;
        }

        // ———————————————————— MVP Properties ————————————————————

        public ReceiptPreviewControl PreviewControl => pnlPreview;
        public TextBox TransactionTextBox => txtTransactionNumber;
        public ComboBox PrintersComboBox => cmbPrinters;
        public ComboBox StoresComboBox => cmbStores;
        public NumericUpDown FontSizeControl => numFontSize;
        public bool IsEscPosSelected => rbEscPos.Checked;
        public bool IsWindowsDriverSelected => rbWindowsDriver.Checked;
        public bool IsAllStoresChecked => chkAllStores.Checked;
        public Button PrintButton => btnPrint;
    }
}
