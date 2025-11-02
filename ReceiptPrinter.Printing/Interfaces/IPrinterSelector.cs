using System.Collections.Generic;
using ReceiptPrinter.Printing.Models;

namespace ReceiptPrinter.Printing.Interfaces
{
    /// <summary>
    /// Defines the contract for selecting and validating printers.
    /// </summary>
    public interface IPrinterSelector
    {
        /// <summary>
        /// Retrieves a list of available printer names.
        /// </summary>
        /// <returns>List of printer names.</returns>
        List<string> GetAvailablePrinters();

        /// <summary>
        /// Validates if a printer name exists and is available.
        /// </summary>
        /// <param name="printerName">The printer name to check.</param>
        /// <returns>True if valid.</returns>
        bool ValidatePrinter(string printerName);

        /// <summary>
        /// Creates an IPrinter instance based on settings.
        /// </summary>
        /// <param name="settings">The PrinterSettings.</param>
        /// <returns>An IPrinter implementation.</returns>
        IPrinter GetPrinter(PrinterSettings settings);

        /// <summary>
        /// Prints a receipt using the specified print job.
        /// </summary>
        /// <param name="printJob">The print job containing receipt data and settings.</param>
        void PrintReceipt(PrintJob printJob);
    }
}