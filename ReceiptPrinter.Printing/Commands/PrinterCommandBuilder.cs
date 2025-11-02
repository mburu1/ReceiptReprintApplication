using ReceiptPrinter.Core.Models;
using ReceiptPrinter.Core.Utilities;
using ReceiptPrinter.Logging;
using ReceiptPrinter.Logging.Interfaces;
using ReceiptPrinter.Printing.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ReceiptPrinter.Printing.Commands
{
    /// <summary>
    /// Builds ESC/POS command byte arrays for receipts.
    /// Constructs the full print buffer based on ReceiptData.
    /// </summary>
    public class PrinterCommandBuilder
    {
        private static readonly ILogger _logger = LogManager.GetLogger();
        private const int LineWidth = 42; // Standard for 80mm thermal paper

        /// <summary>
        /// Builds the complete byte array for printing.
        /// </summary>
        /// <param name="job">The PrintJob containing data and settings.</param>
        /// <returns>Byte array of ESC/POS commands.</returns>
        public byte[] Build(PrintJob job)
        {
            if (job == null || !job.IsValid())
            {
                _logger.Warning("Invalid PrintJob; cannot build commands.");
                return new byte[0];
            }

            _logger.Debug($"Building ESC/POS commands for Transaction: {job.ReceiptData.TransactionNumber}");

            var buffer = new List<byte>();
            var data = job.ReceiptData;
            var settings = job.Settings;

            // Map font size to ESC/POS scale (0..3)
            byte fontScale = EscPosCommands.MapFontSize(settings.FontSize);

            // Initialize printer
            buffer.AddRange(EscPosCommands.Initialize);
            buffer.AddRange(EscPosCommands.SetFontSize(fontScale));

            // Header
            BuildHeader(buffer, data.CompanyInfo);

            // Transaction Info
            BuildTransactionInfo(buffer, data);

            // Items
            BuildItemsSection(buffer, data.Items);

            // Totals
            BuildTotals(buffer, data);

            // Footer
            BuildFooter(buffer);

            // Partial cut
            buffer.AddRange(EscPosCommands.PartialCut);

            _logger.Info("ESC/POS command buffer built successfully.");
            return buffer.ToArray();
        }

        private void BuildHeader(List<byte> buffer, CompanyHeader header)
        {
            var h = header ?? new CompanyHeader();

            buffer.AddRange(EscPosCommands.AlignCenter());
            buffer.AddRange(EscPosCommands.BoldOn());
            buffer.AddRange(EscPosCommands.Text("Duplicate Receipt"));
            buffer.AddRange(EscPosCommands.BoldOff());
            buffer.AddRange(EscPosCommands.LineFeed);
            buffer.AddRange(EscPosCommands.DoubleLineSeparator(LineWidth));
            buffer.AddRange(EscPosCommands.LineFeed);

            buffer.AddRange(EscPosCommands.AlignLeft());
            buffer.AddRange(EscPosCommands.Text(h.CompanyName ?? string.Empty));
            buffer.AddRange(EscPosCommands.LineFeed);
            buffer.AddRange(EscPosCommands.Text(h.Address1 ?? string.Empty));
            buffer.AddRange(EscPosCommands.LineFeed);
            buffer.AddRange(EscPosCommands.Text(h.PinNumber ?? string.Empty));
            buffer.AddRange(EscPosCommands.LineFeed);
            buffer.AddRange(EscPosCommands.Text(h.PhoneNumber ?? string.Empty));
            buffer.AddRange(EscPosCommands.LineFeed);
            buffer.AddRange(EscPosCommands.Text(h.Country ?? string.Empty));
            buffer.AddRange(EscPosCommands.LineFeed);

            buffer.AddRange(EscPosCommands.AlignRight());
            buffer.AddRange(EscPosCommands.Text("Sales Receipt Slip"));
            buffer.AddRange(EscPosCommands.LineFeed);
            buffer.AddRange(EscPosCommands.LineFeed);
        }

        private void BuildTransactionInfo(List<byte> buffer, ReceiptData data)
        {
            buffer.AddRange(EscPosCommands.AlignLeft());
            buffer.AddRange(EscPosCommands.Text($"Transaction No: {data.TransactionNumber}"));
            buffer.AddRange(EscPosCommands.LineFeed);
            buffer.AddRange(EscPosCommands.Text($"Date: {DateTimeFormatter.FormatForReceipt(data.TransactionTime)}"));
            buffer.AddRange(EscPosCommands.LineFeed);
            buffer.AddRange(EscPosCommands.Text($"Cashier: {data.CashierName ?? string.Empty}"));
            buffer.AddRange(EscPosCommands.LineFeed);
            buffer.AddRange(EscPosCommands.Text($"Register: {data.RegisterNumber ?? string.Empty}"));
            buffer.AddRange(EscPosCommands.LineFeed);
            buffer.AddRange(EscPosCommands.LineFeed);
        }

        private void BuildItemsSection(List<byte> buffer, List<TransactionEntry> items)
        {
            buffer.AddRange(EscPosCommands.SingleLineSeparator(LineWidth));
            buffer.AddRange(EscPosCommands.LineFeed);

            // Column headers
            buffer.AddRange(EscPosCommands.Text("Code  Description           QTY    Price    Total"));
            buffer.AddRange(EscPosCommands.LineFeed);
            buffer.AddRange(EscPosCommands.SingleLineSeparator(LineWidth));
            buffer.AddRange(EscPosCommands.LineFeed);

            if (items == null || items.Count == 0)
            {
                buffer.AddRange(EscPosCommands.Text("   (no items)"));
                buffer.AddRange(EscPosCommands.LineFeed);
                buffer.AddRange(EscPosCommands.SingleLineSeparator(LineWidth));
                buffer.AddRange(EscPosCommands.LineFeed);
                return;
            }

            foreach (var item in items)
            {
                string code = (item.ItemLookupCode ?? "").PadRight(6).Substring(0, Math.Min(6, (item.ItemLookupCode ?? "").Length)).PadRight(6);
                string desc = item.Description ?? "";
                string qty = item.Quantity.ToString("F0").PadLeft(3);
                string price = item.Price.ToString("F2").PadLeft(8);
                decimal totalDec = item.Price * item.Quantity;
                string total = totalDec.ToString("F2").PadLeft(8);

                // Ensure description fits within column; wrap if necessary
                if (desc.Length > 20)
                {
                    // First line: code + first 20 chars of desc
                    string line1 = $"{code} {desc.Substring(0, 20).PadRight(20)} {qty} {price} {total}";
                    buffer.AddRange(EscPosCommands.Text(line1));
                    buffer.AddRange(EscPosCommands.LineFeed);

                    // Wrap remaining description
                    string remaining = desc.Substring(20).Trim();
                    while (!string.IsNullOrEmpty(remaining))
                    {
                        string chunk = remaining.Length > 20 ? remaining.Substring(0, 20) : remaining;
                        buffer.AddRange(EscPosCommands.Text("      " + chunk));
                        buffer.AddRange(EscPosCommands.LineFeed);
                        remaining = remaining.Length > 20 ? remaining.Substring(20).Trim() : string.Empty;
                    }
                }
                else
                {
                    string line = $"{code} {desc.PadRight(20)} {qty} {price} {total}";
                    buffer.AddRange(EscPosCommands.Text(line));
                    buffer.AddRange(EscPosCommands.LineFeed);
                }
            }

            buffer.AddRange(EscPosCommands.SingleLineSeparator(LineWidth));
            buffer.AddRange(EscPosCommands.LineFeed);
        }

        private void BuildTotals(List<byte> buffer, ReceiptData data)
        {
            buffer.AddRange(EscPosCommands.AlignRight());
            buffer.AddRange(EscPosCommands.Text($"Sub Total: {data.CalculateSubtotal():F2}"));
            buffer.AddRange(EscPosCommands.LineFeed);
            buffer.AddRange(EscPosCommands.Text($"Sales Tax: {data.CalculateSalesTax():F2}"));
            buffer.AddRange(EscPosCommands.LineFeed);
            buffer.AddRange(EscPosCommands.BoldOn());
            buffer.AddRange(EscPosCommands.Text($"Grand Total: {data.GrandTotal:F2}"));
            buffer.AddRange(EscPosCommands.BoldOff());
            buffer.AddRange(EscPosCommands.LineFeed);
            buffer.AddRange(EscPosCommands.LineFeed);
        }

        private void BuildFooter(List<byte> buffer)
        {
            buffer.AddRange(EscPosCommands.DoubleLineSeparator(LineWidth));
            buffer.AddRange(EscPosCommands.LineFeed);
            buffer.AddRange(EscPosCommands.AlignCenter());
            buffer.AddRange(EscPosCommands.Text("Thank you for your business!"));
            buffer.AddRange(EscPosCommands.LineFeed);
            buffer.AddRange(EscPosCommands.Text("*** DUPLICATE COPY ***"));
            buffer.AddRange(EscPosCommands.LineFeed);
            buffer.AddRange(EscPosCommands.DoubleLineSeparator(LineWidth));
            buffer.AddRange(EscPosCommands.LineFeed);
            buffer.AddRange(EscPosCommands.LineFeed);
        }
    }
}
