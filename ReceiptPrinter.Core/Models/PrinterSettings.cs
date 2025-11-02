using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// ReceiptPrinter.Core/Models/PrinterSettings.cs
namespace ReceiptPrinter.Core.Models
{
    public class PrinterSettings
    {
        public string PrinterName { get; set; }
        public int FontSize { get; set; } = 10;
        public PrintMethod Method { get; set; } = PrintMethod.WindowsDriver;
        public string PaperSizeName { get; set; } = "A4"; // Default to A4
        public bool IsValid =>
            !string.IsNullOrWhiteSpace(PrinterName) &&
            FontSize >= 6 &&
            FontSize <= 20;
    }
    public enum PrintMethod
    {
        EscPos,
        WindowsDriver
    }
}