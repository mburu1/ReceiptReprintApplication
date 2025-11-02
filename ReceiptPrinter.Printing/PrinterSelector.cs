// ReceiptPrinter.Printing/PrinterSelector.cs
using ReceiptPrinter.Logging;
using ReceiptPrinter.Logging.Interfaces;
using ReceiptPrinter.Printing.Interfaces;
using ReceiptPrinter.Printing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using SystemPrinterSettings = System.Drawing.Printing.PrinterSettings;

namespace ReceiptPrinter.Printing
{
    public class PrinterSelector : IPrinterSelector
    {
        private static readonly ILogger _logger = LogManager.GetLogger();

        public List<string> GetAvailablePrinters()
        {
            _logger.Debug("Fetching available printers.");
            try
            {
                return SystemPrinterSettings.InstalledPrinters
                    .Cast<string>()
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.Error("Error fetching printers", ex);
                return new List<string>();
            }
        }

        public bool ValidatePrinter(string printerName)
        {
            if (string.IsNullOrWhiteSpace(printerName))
            {
                _logger.Warning("Printer name is empty.");
                return false;
            }

            var printers = GetAvailablePrinters();
            bool exists = printers.Contains(printerName);
            _logger.Debug($"Printer '{printerName}' validation: {exists}");
            return exists;
        }

        public IPrinter GetPrinter(PrinterSettings settings)
        {
            if (settings == null || !settings.IsValid())
            {
                _logger.Warning("Invalid PrinterSettings.");
                return null;
            }

            _logger.Info($"Creating printer for method: {settings.Method}");

            if (settings.Method == PrintMethod.EscPos)
                return new EscPosPrinter(settings);
            else if (settings.Method == PrintMethod.WindowsDriver)
                return new WindowsDriverPrinter(settings);
            else
                return null;
        }

        public void PrintReceipt(PrintJob printJob)
        {
            if (printJob == null || !printJob.IsValid())
            {
                _logger.Warning("Invalid PrintJob.");
                throw new ArgumentException("PrintJob is invalid.");
            }

            _logger.Info($"Printing receipt for transaction: {printJob.ReceiptData.TransactionNumber}");

            var printer = GetPrinter(printJob.Settings);
            if (printer == null)
            {
                _logger.Error("Failed to create printer instance.");
                throw new InvalidOperationException("Unable to create printer instance.");
            }

            printer.Print(printJob);
            _logger.Info("Receipt printed successfully.");
        }
    }
}