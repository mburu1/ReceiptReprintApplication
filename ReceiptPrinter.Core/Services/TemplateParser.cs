using ReceiptPrinter.Core.DataAccess.Interfaces;
using ReceiptPrinter.Core.Models;
using ReceiptPrinter.Core.Services.Interfaces;
using ReceiptPrinter.Logging;
using ReceiptPrinter.Logging.Interfaces;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
namespace ReceiptPrinter.Core.Services
{
    /// <summary>
    /// Implements template parsing logic.
    /// Uses regex to replace placeholders in templates.
    /// </summary>
    public class TemplateParser : ITemplateParser
    {
        private readonly IReceiptRepository _repository;
        private static readonly ILogger _logger = LogManager.GetLogger();
        public TemplateParser(IReceiptRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }
        public CompanyHeader ParseTemplate(string template)
        {
            _logger.Debug("Parsing template.");
            if (string.IsNullOrWhiteSpace(template))
            {
                _logger.Warning("Template is empty; using default header.");
                return CompanyHeader.Default();
            }
            try
            {
                var header = new CompanyHeader();
                // Assume placeholders like {CompanyName}, etc.
                // Use regex to extract
                header.CompanyName = ExtractValue(template, @"{CompanyName:(.*?)}") ?? "Sleaklady cosmetics ltd";
                header.Address1 = ExtractValue(template, @"{Address1:(.*?)}") ?? "Bestlady";
                header.PinNumber = ExtractValue(template, @"{PinNumber:(.*?)}") ?? "Pin No: P051681112N";
                header.PhoneNumber = ExtractValue(template, @"{PhoneNumber:(.*?)}") ?? "0740470002-HQ";
                header.Country = ExtractValue(template, @"{Country:(.*?)}") ?? "Kenya";
                if (!header.IsValid())
                {
                    _logger.Warning("Parsed header is invalid; using default.");
                    return CompanyHeader.Default();
                }
                _logger.Info("Template parsed successfully.");
                return header;
            }
            catch (Exception ex)
            {
                _logger.Error("Error parsing template", ex);
                return CompanyHeader.Default();
            }
        }
        private string ExtractValue(string template, string pattern)
        {
            var match = Regex.Match(template, pattern);
            return match.Success ? match.Groups[1].Value.Trim() : null;
        }
        public async Task<ReceiptTemplate> GetTemplateAsync(int storeId)
        {
            return await _repository.GetReceiptTemplateAsync(storeId);
        }
        public string FormatReceipt(ReceiptData data, ReceiptTemplate template)
        {
            _logger.Debug("Formatting receipt.");
            if (data == null || template == null)
            {
                _logger.Warning("Data or template is null; cannot format.");
                return string.Empty;
            }
            // Simple replacement; extend as needed
            var formatted = template.TemplateSale
                .Replace("{TransactionNumber}", data.TransactionNumber.ToString())
                .Replace("{Date}", data.TransactionTime.ToString("d/MM/yyyy h:mm:ss tt"))
                .Replace("{CashierName}", data.CashierName)
                .Replace("{Register}", data.RegisterNumber)
                .Replace("{GrandTotal}", data.GrandTotal.ToString("F2"));
            // Add items; this is basic - enhance for full layout
            foreach (var item in data.Items)
            {
                formatted += $"\n{item.ItemLookupCode} {item.FormattedDescription} {item.Quantity} {item.Price:F2} {item.Total:F2}";
            }
            _logger.Info("Receipt formatted.");
            return formatted;
        }
    }
}