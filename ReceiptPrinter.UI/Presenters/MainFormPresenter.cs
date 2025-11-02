using ReceiptPrinter.Core.Models;
using ReceiptPrinter.Core.Services.Interfaces;
using ReceiptPrinter.Logging;
using ReceiptPrinter.Logging.Interfaces;
using ReceiptPrinter.Printing.Interfaces;
using ReceiptPrinter.Printing.Models;
using ReceiptPrinter.UI.Controls;
using ReceiptPrinter.UI.Forms;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

// Aliases to avoid ambiguity
using PrintSettings = ReceiptPrinter.Printing.Models.PrinterSettings;
using PrintingMethod = ReceiptPrinter.Printing.Models.PrintMethod;

namespace ReceiptPrinter.UI.Presenters
{
    public class MainFormPresenter
    {
        private readonly MainForm _view;
        private readonly IReceiptService _receiptService;
        private readonly IPrinterSelector _printSelector;
        private ReceiptData _currentReceiptData;
        private static readonly ILogger _logger = LogManager.GetLogger();

        // Strongly-typed UI accessors (use public properties on MainForm)
        private ReceiptPreviewControl PnlPreview => _view.PreviewControl;
        private ComboBox CmbStores => _view.StoresComboBox;
        private ComboBox CmbPrinters => _view.PrintersComboBox;
        private NumericUpDown NumFontSize => _view.FontSizeControl;
        private Button BtnPrint => _view.PrintButton;
        private TextBox TxtTransactionNumber => _view.TransactionTextBox;
        private CheckBox ChkAllStores => _view.Controls.OfType<CheckBox>().FirstOrDefault(c => c.Name == "chkAllStores");
        private RadioButton RbWindowsDriver => _view.Controls.OfType<RadioButton>().FirstOrDefault(r => r.Name == "rbWindowsDriver");

        public bool HasReceipt => _currentReceiptData != null;

        public MainFormPresenter(MainForm view, IReceiptService receiptService, IPrinterSelector printSelector)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _receiptService = receiptService ?? throw new ArgumentNullException(nameof(receiptService));
            _printSelector = printSelector ?? throw new ArgumentNullException(nameof(printSelector));
        }

        public async Task InitializeAsync()
        {
            try
            {
                await LoadStoresAsync();
                await LoadPrintersAsync();
                UpdatePrintButtonState();
                _logger.Info("MainFormPresenter initialized successfully.");
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to initialize presenter", ex);
                throw;
            }
        }

        private async Task LoadStoresAsync()
        {
            try
            {
                // If the form doesn't have a stores combo, skip population gracefully.
                if (CmbStores == null) return;

                var stores = await _receiptService.GetAllStoresAsync();
                _view.Invoke((Action)(() =>
                {
                    CmbStores.Items.Clear();
                    if (stores?.Any() == true)
                    {
                        foreach (var store in stores)
                            CmbStores.Items.Add(store);
                        CmbStores.SelectedIndex = 0;
                    }
                }));
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to load stores", ex);
                throw;
            }
        }

        private async Task LoadPrintersAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    var printers = System.Drawing.Printing.PrinterSettings.InstalledPrinters;
                    _view.Invoke((Action)(() =>
                    {
                        CmbPrinters?.Items.Clear();
                        foreach (string printer in printers)
                            CmbPrinters?.Items.Add(printer);
                        if (CmbPrinters != null && CmbPrinters.Items.Count > 0)
                            CmbPrinters.SelectedIndex = 0;
                    }));
                }
                catch (Exception ex)
                {
                    _logger.Error("Failed to load printers", ex);
                    throw;
                }
            });
        }

        public async Task RefreshAllAsync()
        {
            try
            {
                _view.Invoke((Action)(() =>
                {
                    // Reset input
                    if (TxtTransactionNumber != null)
                    {
                        TxtTransactionNumber.Text = "Enter Transaction #";
                        TxtTransactionNumber.ForeColor = System.Drawing.SystemColors.GrayText;
                    }

                    PnlPreview?.Clear();
                    _currentReceiptData = null;

                    if (ChkAllStores != null) ChkAllStores.Checked = false;
                    if (CmbStores != null) CmbStores.SelectedIndex = -1;

                    if (NumFontSize != null) NumFontSize.Value = 10;
                    if (RbWindowsDriver != null) RbWindowsDriver.Checked = true;
                }));

                await LoadPrintersAsync();
                UpdatePrintButtonState();
                _logger.Info("Form refreshed.");
            }
            catch (Exception ex)
            {
                _logger.Error("Refresh failed", ex);
                throw;
            }
        }

        public async Task GenerateReceiptAsync(string transactionText)
        {
            try
            {
                _logger.Info($"Generating receipt for: {transactionText}");
                if (!int.TryParse(transactionText, out int txn) || txn <= 0)
                {
                    ShowError("Invalid transaction number.");
                    ClearPreview();
                    return;
                }

                int? storeId = null;
                // If CmbStores is present and AllStores not checked, use selected store
                if (CmbStores != null && (CmbStores.SelectedItem as StoreInfo) != null && !(CmbStores.SelectedItem is string && (string)CmbStores.SelectedItem == "-- All --"))
                {
                    if (!(_view.IsAllStoresChecked))
                    {
                        storeId = (CmbStores.SelectedItem as StoreInfo)?.ID;
                    }
                }
                else
                {
                    // fallback to checkbox if combo isn't present
                    if (!_view.IsAllStoresChecked)
                    {
                        storeId = null; // can't determine; treat as all
                    }
                }

                _currentReceiptData = await _receiptService.GetReceiptDataAsync(txn, storeId);
                if (_currentReceiptData == null)
                {
                    MessageBox.Show("Transaction not found.", "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearPreview();
                }
                else
                {
                    UpdatePreview(_currentReceiptData);
                }
                UpdatePrintButtonState();
            }
            catch (Exception ex)
            {
                _logger.Error("Generate failed", ex);
                ClearPreview();
                throw;
            }
        }

        private void UpdatePreview(ReceiptData data)
        {
            _view.Invoke((Action)(() => PnlPreview?.UpdatePreview(data)));
        }

        private void ClearPreview()
        {
            _currentReceiptData = null;
            _view.Invoke((Action)(() => PnlPreview?.Clear()));
            UpdatePrintButtonState();
        }

        public void UpdatePrintButtonState()
        {
            _view.Invoke((Action)(() =>
            {
                if (BtnPrint != null)
                    BtnPrint.Enabled = HasReceipt;
            }));
        }

        public void PrintReceipt()
        {
            if (!HasReceipt) return;
            try
            {
                var settings = CreatePrintSettings();
                var job = new PrintJob(_currentReceiptData, settings);
                _printSelector.PrintReceipt(job);
                _logger.Info($"Printed to {settings.PrinterName}");
            }
            catch (Exception ex)
            {
                _logger.Error("Print failed", ex);
                ShowError("Failed to print receipt.");
            }
        }

        public void PrintEmptyReceipt()
        {
            try
            {
                var job = PrintJob.CreateEmptyTestJob(CreatePrintSettings());
                _printSelector.PrintReceipt(job);
                _logger.Info("Test receipt printed.");
            }
            catch (Exception ex)
            {
                _logger.Error("Test print failed", ex);
                ShowError("Failed to print test receipt.");
            }
        }

        // Helper: Extract settings creation
        private PrintSettings CreatePrintSettings()
        {
            return new PrintSettings
            {
                PrinterName = CmbPrinters?.SelectedItem?.ToString() ?? "Microsoft Print to PDF",
                FontSize = (int)(NumFontSize?.Value ?? 10),
                Method = _view.IsWindowsDriverSelected ? PrintingMethod.WindowsDriver : PrintingMethod.EscPos,
                PaperSizeName = "A4" // Hardcoded — no paper size dropdown
            };
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
