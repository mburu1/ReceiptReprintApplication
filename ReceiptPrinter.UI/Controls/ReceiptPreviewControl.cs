// ReceiptPrinter.UI.Controls/ReceiptPreviewControl.cs
using ReceiptPrinter.Core.Models;
using ReceiptPrinter.Logging;
using ReceiptPrinter.Logging.Interfaces;
using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ReceiptPrinter.UI.Controls
{
    /// <summary>
    /// Custom UserControl for previewing receipts.
    /// Renders formatted text in a RichTextBox for pre-print visualization.
    /// </summary>
    public partial class ReceiptPreviewControl : UserControl
    {
        private static readonly ILogger _logger = LogManager.GetLogger();

        public ReceiptPreviewControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Updates the preview with the given ReceiptData.
        /// </summary>
        /// <param name="data">The ReceiptData to display.</param>
        public void UpdatePreview(ReceiptData data)
        {
            if (data == null)
            {
                _logger.Warning("Null ReceiptData for preview.");
                rtbPreview.Clear();
                return;
            }

            _logger.Debug("Updating receipt preview.");

            try
            {
                rtbPreview.Clear();
                rtbPreview.Font = new Font("Courier New", 10); // Fixed-width for alignment

                // Build formatted string
                var sb = new StringBuilder();

                // Header
                sb.AppendLine("             Duplicate Receipt              ".Substring(0, 42));
                sb.AppendLine(new string('═', 42));
                sb.AppendLine(data.CompanyInfo.CompanyName);
                sb.AppendLine(data.CompanyInfo.Address1);
                sb.AppendLine(data.CompanyInfo.PinNumber);
                sb.AppendLine(data.CompanyInfo.PhoneNumber);
                sb.AppendLine(data.CompanyInfo.Country);
                sb.AppendLine("                                 Sales Receipt slip");

                // Transaction Info
                sb.AppendLine($"Transaction No: {data.TransactionNumber}");
                sb.AppendLine($"Date: {data.TransactionTime.ToString("d/MM/yyyy h:mm:ss tt")}");
                sb.AppendLine($"Cashier Name: {data.CashierName}");
                sb.AppendLine($"Register: {data.RegisterNumber}");

                // Items
                sb.AppendLine(new string('─', 42));
                sb.AppendLine("Item Code Description         Qty Price Total");
                sb.AppendLine(new string('─', 42));

                foreach (var item in data.Items)
                {
                    string desc = item.FormattedDescription;
                    // Note: Ensure desc.Length is checked before using Substring to avoid IndexOutOfRangeException
                    int descLength = Math.Min(desc.Length, 20);

                    if (desc.Length > 20)
                    {
                        sb.AppendLine($"{item.ItemLookupCode,-8} {desc.Substring(0, 20),-20} {item.Quantity,3} {item.Price,5:F0} {item.Total,5:F0}");
                        sb.AppendLine($"        {desc.Substring(20).Trim()}");
                    }
                    else
                    {
                        sb.AppendLine($"{item.ItemLookupCode,-8} {desc,-20} {item.Quantity,3} {item.Price,5:F0} {item.Total,5:F0}");
                    }
                }

                sb.AppendLine(new string('─', 42));

                // Totals
                sb.AppendLine($"                    Sub Total: {data.CalculateSubtotal():F2}");
                sb.AppendLine($"                    Sales Tax: {data.CalculateSalesTax():F2}");
                sb.AppendLine($"                    Grand Total: {data.GrandTotal:F2}");

                // Footer
                sb.AppendLine(new string('═', 42));
                sb.AppendLine("          Thank you for your business          ");
                sb.AppendLine(new string('═', 42));

                rtbPreview.Text = sb.ToString();
                _logger.Info("Preview updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.Error("Error updating preview", ex);
                rtbPreview.Text = "Error generating preview.";
            }
        }

        /// <summary>
        /// Clears the preview. Renamed from ClearPreview() to match conventional naming.
        /// </summary>
        public void Clear() // Renamed from ClearPreview()
        {
            rtbPreview.Clear();
        }
    }
}