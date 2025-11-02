using ReceiptPrinter.Core.Models;
using ReceiptPrinter.Core.Utilities;
using ReceiptPrinter.Logging;
using ReceiptPrinter.Logging.Interfaces;
using ReceiptPrinter.Printing.Interfaces;
using ReceiptPrinter.Printing.Models;
using System;
using System.Drawing;
using System.Drawing.Printing;
using PrinterSettings = ReceiptPrinter.Printing.Models.PrinterSettings; // Alias for your custom type

namespace ReceiptPrinter.Printing
{
    /// <summary>
    /// Implements printing using Windows GDI/PrintDocument.
    /// Draws receipt layout on the graphics context.
    /// </summary>
    public class WindowsDriverPrinter : IPrinter
    {
        private readonly PrinterSettings _settings; // Now unambiguous
        private bool _disposed = false;
        private static readonly ILogger _logger = LogManager.GetLogger();
        private const float PaperWidth = 3.15f; // 80mm in inches
        private const int MinRollHeightHundredths = 300; // Minimum 3 inches
        private const int MarginHundredths = 100; // Extra 1 inch margin

        public WindowsDriverPrinter(PrinterSettings settings) // Now unambiguous
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public bool Print(PrintJob job)
        {
            _logger.Info($"Initiating Windows Driver print to {_settings.PrinterName}");
            if (job == null || !job.IsValid())
            {
                _logger.Warning("Invalid PrintJob.");
                return false;
            }
            try
            {
                using (var pd = new PrintDocument())
                {
                    pd.PrinterSettings.PrinterName = _settings.PrinterName;
                    pd.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);

                    // Determine printer DPI
                    PrinterResolution res = pd.DefaultPageSettings.PrinterResolution;
                    int dpiX = res.X > 0 ? res.X : 600; // Default to 600 for bizhub
                    int dpiY = res.Y > 0 ? res.Y : dpiX;

                    // Compute required height in pixels
                    float pageWidthPx = 0f; // Will set after PaperSize
                    float requiredPx;
                    PaperSize selectedSize = null;

                    // Find matching PaperSize from supported list
                    string targetName = _settings.PaperSizeName ?? "A4";
                    foreach (PaperSize ps in pd.PrinterSettings.PaperSizes)
                    {
                        if (string.Equals(ps.PaperName, targetName, StringComparison.OrdinalIgnoreCase))
                        {
                            selectedSize = ps;
                            break;
                        }
                    }

                    if (selectedSize == null)
                    {
                        // Fallback to A4 or default
                        foreach (PaperSize ps in pd.PrinterSettings.PaperSizes)
                        {
                            if (string.Equals(ps.PaperName, "A4", StringComparison.OrdinalIgnoreCase))
                            {
                                selectedSize = ps;
                                break;
                            }
                        }
                        if (selectedSize == null)
                        {
                            selectedSize = pd.DefaultPageSettings.PaperSize; // Ultimate fallback
                        }
                        _logger.Warning($"Paper size '{targetName}' not found; falling back to '{selectedSize.PaperName}'");
                    }

                    // If custom roll, override with computed height (in hundredths of inch)
                    bool isCustomRoll = targetName.Equals("Custom 80mm Roll", StringComparison.OrdinalIgnoreCase);
                    if (isCustomRoll)
                    {
                        int widthHundredths = (int)(3.15f * 100f); // 80mm
                        using (var measureBmp = new Bitmap(1, 1))
                        {
                            measureBmp.SetResolution(dpiX, dpiY);
                            using (var measureG = Graphics.FromImage(measureBmp))
                            {
                                pageWidthPx = widthHundredths * dpiX / 100f; // Approx px
                                requiredPx = ComputeHeight(measureG, job, pageWidthPx);
                            }
                        }
                        float requiredInches = requiredPx / dpiY;
                        int rollHeightHundredths = Math.Max(300, (int)(requiredInches * 100) + 100); // Min 3in + margin
                                                                                                     // Clamp to bizhub max ~47in (4700 hundredths)
                        rollHeightHundredths = Math.Min(rollHeightHundredths, 4700);
                        selectedSize = new PaperSize("Custom 80mm Roll", widthHundredths, rollHeightHundredths);
                    }
                    else
                    {
                        // For A4/A3, use full width; receipt will be left-aligned
                        pageWidthPx = selectedSize.Width * dpiX / 100f; // Hundredths to px
                        using (var measureBmp = new Bitmap(1, 1))
                        {
                            measureBmp.SetResolution(dpiX, dpiY);
                            using (var measureG = Graphics.FromImage(measureBmp))
                            {
                                requiredPx = ComputeHeight(measureG, job, pageWidthPx);
                            }
                        }
                        // If required > size height, warn (but proceed; printer may cut off)
                        if (requiredPx > selectedSize.Height * dpiY / 100f)
                        {
                            _logger.Warning("Receipt content may exceed paper height; consider smaller font or custom size.");
                        }
                    }

                    pd.DefaultPageSettings.PaperSize = selectedSize;
                    pd.DefaultPageSettings.Landscape = false; // Portrait for receipts

                    pd.PrintPage += (sender, e) => DrawReceipt(e.Graphics, job, e);
                    pd.PrintController = new StandardPrintController();
                    pd.Print();
                }
                _logger.Info("Print completed successfully.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error("Windows Driver print failed", ex);
                return false;
            }
        }

        private float ComputeHeight(Graphics g, PrintJob job, float pageWidthPx)
        {
            var data = job.ReceiptData ?? new ReceiptData();
            float y = 0;
            using (var font = new Font("Courier New", _settings.FontSize))
            using (var boldFont = new Font("Courier New", _settings.FontSize, FontStyle.Bold))
            {
                float charWidth = g.MeasureString("A", font).Width;
                int charsPerLine = charWidth > 0 ? (int)(pageWidthPx / charWidth) : 42;

                // Header
                y += boldFont.GetHeight(g); // Duplicate Receipt
                y += font.GetHeight(g); // =
                y += font.GetHeight(g); // Line feed

                // Company info (up to 5 lines)
                y += font.GetHeight(g) * 5;

                // Transaction Info (4 lines)
                y += font.GetHeight(g) * 4;

                // Items header (3 lines: -, header, -)
                y += font.GetHeight(g) * 3;

                if (data.Items != null)
                {
                    foreach (var item in data.Items)
                    {
                        string desc = item.Description ?? string.Empty;
                        string qty = item.Quantity.ToString("F0");
                        string price = item.Price.ToString("F2");
                        string total = (item.Price * item.Quantity).ToString("F2");
                        string leftPart = $"{item.ItemLookupCode ?? string.Empty} {Truncate(desc, 20).PadRight(20)}";
                        float leftWidth = g.MeasureString(leftPart, font).Width;
                        string rightPart = $"{qty} {price} {total}";
                        float rightWidth = g.MeasureString(rightPart, font).Width;
                        if (leftWidth + rightWidth > pageWidthPx)
                        {
                            y += font.GetHeight(g); // left
                            y += font.GetHeight(g); // right
                        }
                        else
                        {
                            y += font.GetHeight(g); // single line
                        }
                        if (desc.Length > 20)
                        {
                            string remaining = desc.Substring(20).Trim();
                            while (!string.IsNullOrEmpty(remaining))
                            {
                                string chunk = remaining.Length > 20 ? remaining.Substring(0, 20) : remaining;
                                y += font.GetHeight(g); // extra line
                                remaining = remaining.Length > 20 ? remaining.Substring(20).Trim() : string.Empty;
                            }
                        }
                    }
                }
                else
                {
                    y += font.GetHeight(g); // (no items)
                }

                // Footer (1 line -, 3 right-aligned totals, 1 bold, 3 lines =/thank you/=)
                y += font.GetHeight(g); // -
                y += font.GetHeight(g) * 3; // Sub, Tax, Grand
                y += boldFont.GetHeight(g); // Using bold for Grand, but counted above
                y += font.GetHeight(g) * 3; // =, thank you, =
                return y;
            }
        }

        private void DrawReceipt(Graphics g, PrintJob job, PrintPageEventArgs e)
        {
            var data = job.ReceiptData ?? new ReceiptData();

            // --- FIX: Use MarginBounds for all calculations ---
            RectangleF bounds = e.MarginBounds;

            using (var font = new Font("Courier New", _settings.FontSize))
            using (var boldFont = new Font("Courier New", _settings.FontSize, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.Black))
            {
                // Start drawing from the top of the margins
                float y = bounds.Top;
                float charWidth = g.MeasureString("A", font).Width;

                // Use bounds.Width for character-per-line calculation
                int charsPerLine = charWidth > 0 ? (int)(bounds.Width / charWidth) : 42;

                // --- FIX: Pass 'bounds' to all drawing helpers ---
                DrawCentered(g, "Duplicate Receipt", boldFont, brush, ref y, bounds);
                DrawLine(g, '=', charsPerLine, font, brush, ref y, bounds);
                DrawLineFeed(g, font, ref y);

                DrawLeft(g, data.CompanyInfo?.CompanyName ?? string.Empty, font, brush, ref y, bounds);
                DrawLeft(g, data.CompanyInfo?.Address1 ?? string.Empty, font, brush, ref y, bounds);
                DrawLeft(g, data.CompanyInfo?.PinNumber ?? string.Empty, font, brush, ref y, bounds);
                DrawLeft(g, data.CompanyInfo?.PhoneNumber ?? string.Empty, font, brush, ref y, bounds);
                DrawLeft(g, data.CompanyInfo?.Country ?? string.Empty, font, brush, ref y, bounds);
                DrawRight(g, "Sales Receipt Slip", font, brush, ref y, bounds); // Pass bounds

                // Transaction Info
                DrawLeft(g, $"Transaction No: {data.TransactionNumber}", font, brush, ref y, bounds);
                DrawLeft(g, $"Date: {DateTimeFormatter.FormatForReceipt(data.TransactionTime)}", font, brush, ref y, bounds);
                DrawLeft(g, $"Cashier Name: {data.CashierName ?? string.Empty}", font, brush, ref y, bounds);
                DrawLeft(g, $"Register: {data.RegisterNumber ?? string.Empty}", font, brush, ref y, bounds);

                // Items
                DrawLine(g, '-', charsPerLine, font, brush, ref y, bounds);
                DrawLeft(g, "Item Code Description Qty Price Total", font, brush, ref y, bounds);
                DrawLine(g, '-', charsPerLine, font, brush, ref y, bounds);

                if (data.Items != null)
                {
                    foreach (var item in data.Items)
                    {
                        string desc = item.Description ?? string.Empty;
                        string qty = item.Quantity.ToString("F0");
                        string price = item.Price.ToString("F2");
                        string total = (item.Price * item.Quantity).ToString("F2");
                        string leftPart = $"{item.ItemLookupCode ?? string.Empty} {Truncate(desc, 20).PadRight(20)}";
                        string rightPart = $"{qty} {price} {total}";
                        float rightWidth = g.MeasureString(rightPart, font).Width;

                        // --- FIX: Simplified and corrected item drawing logic ---
                        // This logic correctly prints both parts on the *same line*.
                        g.DrawString(leftPart, font, brush, bounds.Left, y);
                        float rightX = Math.Max(bounds.Left, bounds.Right - rightWidth);
                        g.DrawString(rightPart, font, brush, rightX, y);
                        y += font.GetHeight(g);

                        if (desc.Length > 20)
                        {
                            string remaining = desc.Substring(20).Trim();
                            while (!string.IsNullOrEmpty(remaining))
                            {
                                string chunk = remaining.Length > 20 ? remaining.Substring(0, 20) : remaining;
                                // --- FIX: Pass 'bounds' here too ---
                                DrawLeft(g, " " + chunk, font, brush, ref y, bounds);
                                remaining = remaining.Length > 20 ? remaining.Substring(20).Trim() : string.Empty;
                            }
                        }
                    }
                }
                else
                {
                    DrawLeft(g, "(no items)", font, brush, ref y, bounds);
                }
                DrawLine(g, '-', charsPerLine, font, brush, ref y, bounds);

                var subTotalText = $"Sub Total: {data.CalculateSubtotal():F2}";
                var taxText = $"Sales Tax: {data.CalculateSalesTax():F2}";
                var grandText = $"Grand Total: {data.GrandTotal:F2}";

                // --- FIX: Pass 'bounds' to all footer drawing helpers ---
                DrawRight(g, subTotalText, font, brush, ref y, bounds);
                DrawRight(g, taxText, font, brush, ref y, bounds);
                DrawRight(g, grandText, boldFont, brush, ref y, bounds);
                DrawLine(g, '=', charsPerLine, font, brush, ref y, bounds);
                DrawCentered(g, "Thank you for your business", font, brush, ref y, bounds);
                DrawLine(g, '=', charsPerLine, font, brush, ref y, bounds);

                e.HasMorePages = false;
            }
        }

        // --- FIX: Updated helper methods to use RectangleF bounds ---

        private void DrawLeft(Graphics g, string text, Font font, Brush brush, ref float y, RectangleF bounds)
        {
            if (string.IsNullOrEmpty(text)) text = string.Empty;
            // Use bounds.Left as the X coordinate
            g.DrawString(text, font, brush, bounds.Left, y);
            y += font.GetHeight(g);
        }

        private void DrawRight(Graphics g, string text, Font font, Brush brush, ref float y, RectangleF bounds)
        {
            float textWidth = g.MeasureString(text, font).Width;
            // Calculate X based on bounds.Right
            float x = Math.Max(bounds.Left, bounds.Right - textWidth);
            g.DrawString(text, font, brush, x, y);
            y += font.GetHeight(g);
        }

        private void DrawCentered(Graphics g, string text, Font font, Brush brush, ref float y, RectangleF bounds)
        {
            float width = g.MeasureString(text, font).Width;
            // Calculate X based on bounds.Left and bounds.Width
            float x = Math.Max(bounds.Left, bounds.Left + (bounds.Width - width) / 2f);
            g.DrawString(text, font, brush, x, y);
            y += font.GetHeight(g);
        }

        private void DrawLine(Graphics g, char ch, int count, Font font, Brush brush, ref float y, RectangleF bounds)
        {
            string line = new string(ch, Math.Max(0, count));
            // Use bounds.Left as the X coordinate
            g.DrawString(line, font, brush, bounds.Left, y);
            y += font.GetHeight(g);
        }

        // --- FIX: Removed DrawRightAbsolute method ---

        private void DrawLineFeed(Graphics g, Font font, ref float y)
        {
            y += font.GetHeight(g);
        }

        private static string Truncate(string input, int max)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            return input.Length <= max ? input : input.Substring(0, max);
        }

        public bool IsAvailable()
        {
            bool isAvailable = false;
            try
            {
                using (var pd = new PrintDocument())
                {
                    pd.PrinterSettings.PrinterName = _settings.PrinterName;
                    // Explicitly set Copies and collate to avoid driver sending unexpected defaults
                    try
                    {
                        pd.PrinterSettings.Copies = 1;
                        pd.PrinterSettings.Collate = false;
                    }
                    catch
                    {
                        // ignore drivers that don't allow programmatic copies setting
                    }
                    pd.PrintController = new StandardPrintController();
                    pd.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
                    // Pick a supported PaperSize:
                    PaperSize chosen = null;
                    foreach (PaperSize ps in pd.PrinterSettings.PaperSizes)
                    {
                        if (ps.PaperName.IndexOf("80", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            ps.PaperName.IndexOf("roll", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            ps.PaperName.IndexOf("a4", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            chosen = ps;
                            break;
                        }
                    }
                    if (chosen == null)
                    {
                        chosen = pd.PrinterSettings.DefaultPageSettings.PaperSize;
                    }
                    pd.DefaultPageSettings.PaperSize = chosen;
                    pd.PrinterSettings.Duplex = Duplex.Default;
                    pd.DefaultPageSettings.Landscape = false;
                    // IsValid is the proper probe for existence/availability
                    isAvailable = pd.PrinterSettings.IsValid;
                }
            }
            catch
            {
                // Optionally add logging here: _logger.Error("Printer availability check failed");
                isAvailable = false; // Explicit, though default is already false
            }
            return isAvailable;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
            }
        }
    }
}