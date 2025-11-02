using ReceiptPrinter.Core.Models;
using ReceiptPrinter.Logging;
using ReceiptPrinter.Logging.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace ReceiptPrinter.Core.Services
{
    /// <summary>
    /// Detects and marks duplicate items in a transaction based on ItemLookupCode.
    /// Pure domain service in DDD.
    /// </summary>
    public class DuplicateDetector
    {
        private static readonly ILogger _logger = LogManager.GetLogger();

        /// <summary>
        /// Processes the list of items and marks duplicates.
        /// </summary>
        /// <param name="items">List of TransactionEntry.</param>
        public void DetectAndMarkDuplicates(List<TransactionEntry> items)
        {
            if (items == null || items.Count == 0)
            {
                _logger.Warning("No items to detect duplicates.");
                return;
            }

            _logger.Debug($"Detecting duplicates in {items.Count} items.");

            var codeGroups = items
                .GroupBy(i => i.ItemLookupCode)
                .Where(g => g.Count() > 1);

            foreach (var group in codeGroups)
            {
                bool isFirst = true;
                foreach (var item in group)
                {
                    if (!isFirst)
                    {
                        item.MarkAsDuplicate();
                        _logger.Debug($"Marked duplicate: {item.ItemLookupCode}");
                    }
                    isFirst = false;
                }
            }

            _logger.Info("Duplicate detection completed.");
        }
    }
}