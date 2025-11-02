using System;
using ReceiptPrinter.Core.Models;

namespace ReceiptPrinter.Core.Utilities
{
    /// <summary>
    /// Utility class for validating domain objects and inputs.
    /// Centralizes validation logic for reuse across services.
    /// </summary>
    public static class ValidationHelper
    {
        /// <summary>
        /// Validates a transaction number.
        /// </summary>
        /// <param name="transactionNumber">The number to validate.</param>
        /// <returns>True if valid (positive integer); otherwise, false.</returns>
        public static bool IsValidTransactionNumber(int transactionNumber)
        {
            return transactionNumber > 0;
        }

        /// <summary>
        /// Validates a store ID.
        /// </summary>
        /// <param name="storeId">The ID to validate.</param>
        /// <returns>True if valid (positive integer); otherwise, false.</returns>
        public static bool IsValidStoreId(int storeId)
        {
            return storeId > 0;
        }

        /// <summary>
        /// Validates a ReceiptData object.
        /// </summary>
        /// <param name="receiptData">The ReceiptData to validate.</param>
        /// <param name="errorMessage">Out parameter for error details.</param>
        /// <returns>True if valid; otherwise, false.</returns>
        public static bool ValidateReceiptData(ReceiptData receiptData, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (receiptData == null)
            {
                errorMessage = "Receipt data is null.";
                return false;
            }

            if (!IsValidTransactionNumber(receiptData.TransactionNumber))
            {
                errorMessage = "Invalid transaction number.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(receiptData.CashierName))
            {
                errorMessage = "Cashier name is missing.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(receiptData.RegisterNumber))
            {
                errorMessage = "Register number is missing.";
                return false;
            }

            if (receiptData.Items == null || receiptData.Items.Count == 0)
            {
                errorMessage = "No items in receipt.";
                return false;
            }

            foreach (var item in receiptData.Items)
            {
                if (!item.IsValid())
                {
                    errorMessage = $"Invalid item: {item.ItemLookupCode}";
                    return false;
                }
            }

            if (receiptData.GrandTotal <= 0)
            {
                errorMessage = "Grand total must be positive.";
                return false;
            }

            if (receiptData.CompanyInfo == null || !receiptData.CompanyInfo.IsValid())
            {
                errorMessage = "Invalid company header.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates a font size for printing.
        /// </summary>
        /// <param name="fontSize">The font size to validate.</param>
        /// <returns>True if between 6 and 20; otherwise, false.</returns>
        public static bool IsValidFontSize(int fontSize)
        {
            return fontSize >= 6 && fontSize <= 20;
        }
    }
}