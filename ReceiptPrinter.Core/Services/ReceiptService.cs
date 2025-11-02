using ReceiptPrinter.Core.DataAccess.Interfaces;
using ReceiptPrinter.Core.Models;
using ReceiptPrinter.Core.Services.Interfaces;
using ReceiptPrinter.Core.Utilities;
using ReceiptPrinter.Logging;
using ReceiptPrinter.Logging.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace ReceiptPrinter.Core.Services
{
    /// <summary>
    /// Implements receipt business logic.
    /// Coordinates repository, parser, detector, and validation.
    /// </summary>
    public class ReceiptService : IReceiptService, IDisposable
    {
        private readonly IReceiptRepository _repository;
        private readonly ITemplateParser _templateParser;
        private readonly DuplicateDetector _duplicateDetector;
        private bool _disposed = false;
        private static readonly ILogger _logger = LogManager.GetLogger();
        public ReceiptService(IReceiptRepository repository, ITemplateParser templateParser, DuplicateDetector duplicateDetector)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _templateParser = templateParser ?? throw new ArgumentNullException(nameof(templateParser));
            _duplicateDetector = duplicateDetector ?? throw new ArgumentNullException(nameof(duplicateDetector));
        }

        public async Task<ReceiptData> GetReceiptDataAsync(int transactionNumber, int? storeId = null)
        {
            _logger.Info($"Getting receipt data for TransactionNumber: {transactionNumber}, StoreID (UI): {storeId}");

            if (!ValidateTransactionNumber(transactionNumber))
            {
                _logger.Warning("Invalid transaction number.");
                return null;
            }

            try
            {
                // 1) Fetch transaction data first - do NOT let a UI-selected store filter this out.
                // Repository should expose a method that fetches the transaction by transactionNumber only.
                // If repository currently requires storeId, update it; below we call the transaction-only variant.
                var data = await _repository.GetTransactionDataAsync(transactionNumber /* do not pass storeId here */);

                if (data == null)
                {
                    _logger.Info("No transaction data found.");
                    return null;
                }

                // 2) Determine effective store id:
                // Prefer explicit UI override (if you want that behaviour), otherwise prefer transaction's StoreID, finally fallback to 0.
                int effectiveStoreId = storeId ?? (data.StoreID != 0 ? data.StoreID : 0);

                // 3) Duplicate detection (guard null)
                if (data.Items != null && data.Items.Count > 0)
                {
                    _duplicateDetector.DetectAndMarkDuplicates(data.Items);
                }

                // 4) Get template for effective store (template parser or repository should implement fallback logic OR use DB fallback)
                var template = await _templateParser.GetTemplateAsync(effectiveStoreId);

                data.CompanyInfo = template != null
                    ? _templateParser.ParseTemplate(template.TemplateSale)
                    : CompanyHeader.Default();

                // 5) Validate receipt data
                if (!ValidationHelper.ValidateReceiptData(data, out string error))
                {
                    _logger.Error($"Invalid receipt data: {error}");
                    return null;
                }

                _logger.Info("Receipt data retrieved and processed.");
                return data;
            }
            catch (Exception ex)
            {
                // Log and propagate or return null depending on your error handling strategy.
                _logger.Error($"Error while building receipt data for TransactionNumber {transactionNumber}", ex);
                throw; // or return null; choose consistent behaviour.
            }
        }

        public bool ValidateTransactionNumber(int transactionNumber)
        {
            return ValidationHelper.IsValidTransactionNumber(transactionNumber);
        }
        public async Task<List<StoreInfo>> GetAllStoresAsync()
        {
            return await _repository.GetStoresAsync();
        }
        public void Dispose()
        {
            if (!_disposed)
            {
                _repository?.Dispose();
                _disposed = true;
            }
        }
    }
}
