// ReceiptPrinter.Printing.Models/PrintJob.cs
using ReceiptPrinter.Core.Models;

namespace ReceiptPrinter.Printing.Models
{
    public class PrintJob
    {
        public ReceiptData ReceiptData { get; set; } = new ReceiptData();
        public string FormattedContent { get; set; }
        public PrinterSettings Settings { get; set; } = new PrinterSettings();

        public PrintJob() { }

        public PrintJob(ReceiptData receiptData, PrinterSettings settings)
        {
            ReceiptData = receiptData ?? new ReceiptData();
            Settings = settings ?? new PrinterSettings();
        }

        public bool IsValid(bool strict = true)
        {
            bool dataOk = ReceiptData?.IsValid(strict) ?? false;
            bool settingsOk = Settings?.IsValid() ?? false;
            return dataOk && settingsOk;
        }

        public bool IsValid() => IsValid(strict: true);

        public static PrintJob CreateEmptyTestJob(PrinterSettings settings = null)
        {
            var receipt = ReceiptData.CreateEmptyTestReceipt();
            var safeSettings = settings ?? new PrinterSettings
            {
                PrinterName = "Microsoft Print to PDF",
                FontSize = 10,
                Method = PrintMethod.WindowsDriver,
                PaperSizeName = "A4"
            };
            return new PrintJob(receipt, safeSettings);
        }
    }
}