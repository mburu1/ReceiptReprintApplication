using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ReceiptPrinter.Core.Models;

namespace ReceiptPrinter.Core.Services.Interfaces
{
    /// <summary>
    /// Defines the contract for receipt-related business operations.
    /// Acts as the application service in DDD, coordinating domain logic.
    /// Uses Task-based Asynchronous Pattern (TAP) for I/O-bound operations.
    /// </summary>
    public interface IReceiptService : IDisposable
    {
        /// <summary>
        /// Retrieves and processes receipt data for a transaction asynchronously.
        /// Includes template parsing, duplicate detection, and validation.
        /// </summary>
        /// <param name="transactionNumber">The transaction number.</param>
        /// <param name="storeId">Optional store ID.</param>
        /// <returns>A Task that represents the asynchronous operation, containing Processed ReceiptData or null if not found.</returns>
        Task<ReceiptData> GetReceiptDataAsync(int transactionNumber, int? storeId = null);

        /// <summary>
        /// Validates a transaction number.
        /// (Can remain synchronous as validation is a CPU-bound operation).
        /// </summary>
        /// <param name="transactionNumber">The number to validate.</param>
        /// <returns>True if valid.</returns>
        bool ValidateTransactionNumber(int transactionNumber);

        /// <summary>
        /// Retrieves all stores for filtering asynchronously.
        /// </summary>
        /// <returns>A Task that represents the asynchronous operation, containing List of StoreInfo.</returns>
        Task<List<StoreInfo>> GetAllStoresAsync();
    }
}