using System;
using System.Collections.Generic;
using System.Linq;

namespace ReceiptPrinter.Core.Models
{
    /// <summary>
    /// Represents the overall data for a receipt, aggregating transaction details,
    /// items, and company information. Serves as the core domain entity for reprinting.
    /// </summary>
    public class ReceiptData
    {
        // --------------------------------------------------------------------
        // Public properties – keep the same names everywhere
        // --------------------------------------------------------------------
        public int TransactionNumber { get; set; }
        public DateTime TransactionTime { get; set; }
        public string CashierName { get; set; }
        public string RegisterNumber { get; set; }
        /// <summary>
        /// Store that originally created the transaction (important for template lookup).
        /// </summary>
        public int StoreID { get; set; }
        public List<TransactionEntry> Items { get; set; } = new List<TransactionEntry>();
        public decimal GrandTotal { get; set; }
        public CompanyHeader CompanyInfo { get; set; }

        // --------------------------------------------------------------------
        // Constructors
        // --------------------------------------------------------------------
        /// <summary>
        /// Parameter-less ctor – useful for JSON deserialization or creating an empty receipt.
        /// </summary>
        public ReceiptData() { }

        /// <summary>
        /// Full ctor – matches the property names exactly.
        /// </summary>
        public ReceiptData(
            int transactionNumber,
            DateTime transactionTime,
            string cashierName,
            string registerNumber,
            int storeId,
            decimal grandTotal,
            CompanyHeader companyInfo = null)
        {
            TransactionNumber = transactionNumber;
            TransactionTime = transactionTime;
            CashierName = cashierName ?? string.Empty;
            RegisterNumber = registerNumber ?? string.Empty;
            StoreID = storeId;
            GrandTotal = grandTotal;
            CompanyInfo = companyInfo ?? CompanyHeader.Default();
            Items = new List<TransactionEntry>(); // Ensure never null
        }

        // --------------------------------------------------------------------
        // Helper calculations – safe against null Items
        // --------------------------------------------------------------------
        public decimal CalculateSubtotal()
            => Items?.Sum(i => i.Subtotal) ?? 0m;

        public decimal CalculateSalesTax()
            => Items?.Sum(i => i.SalesTax) ?? 0m;

        // --------------------------------------------------------------------
        // Determines which store id to use for template lookup
        // --------------------------------------------------------------------
        public int GetEffectiveStoreId(int? uiStoreId = null)
        {
            if (uiStoreId.HasValue) return uiStoreId.Value;
            return StoreID != 0 ? StoreID : 0;
        }

        // --------------------------------------------------------------------
        // Validation – relaxed for empty receipts
        // --------------------------------------------------------------------
        public bool IsValid(bool strict = true)
        {
            if (strict)
            {
                return TransactionNumber > 0 &&
                       !string.IsNullOrWhiteSpace(CashierName) &&
                       !string.IsNullOrWhiteSpace(RegisterNumber) &&
                       Items != null && Items.Count > 0 &&
                       GrandTotal > 0 &&
                       CompanyInfo != null;
            }
            // Non-strict: allow test/empty receipts
            return TransactionNumber >= 0;
        }

        // --------------------------------------------------------------------
        // NEW: Static factory for empty test receipt (used by PrintEmptyReceipt)
        // --------------------------------------------------------------------
        public static ReceiptData CreateEmptyTestReceipt()
        {
            return new ReceiptData(
                transactionNumber: 0,
                transactionTime: DateTime.Now,
                cashierName: "TEST",
                registerNumber: "00",
                storeId: 0,
                grandTotal: 0m,
                companyInfo: CompanyHeader.Default()
            );
        }
    }
}