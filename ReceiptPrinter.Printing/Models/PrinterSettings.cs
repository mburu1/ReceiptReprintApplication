// ReceiptPrinter.Printing.Models/PrinterSettings.cs
using System;

namespace ReceiptPrinter.Printing.Models
{
    /// <summary>
    /// Printer configuration: name, font size, method, paper size.
    /// </summary>
    public class PrinterSettings
    {
        public string PrinterName { get; set; } = "Microsoft Print to PDF";
        public int FontSize { get; set; } = 10;
        public PrintMethod Method { get; set; } = PrintMethod.WindowsDriver;
        public string PaperSizeName { get; set; } = "A4";

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(PrinterName) &&
                   FontSize >= 6 && FontSize <= 20 &&
                   !string.IsNullOrWhiteSpace(PaperSizeName);
        }
    }

    public enum PrintMethod
    {
        EscPos,
        WindowsDriver
    }
}