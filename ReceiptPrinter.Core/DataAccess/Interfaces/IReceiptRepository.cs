using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ReceiptPrinter.Core.Models;

namespace ReceiptPrinter.Core.DataAccess.Interfaces
{
    /// <summary>
    /// Defines the contract for data access operations related to receipts.
    /// Follows Repository pattern for separation of concerns in DDD.
    /// </summary>
    public interface IReceiptRepository : IDisposable
    {
        /// <summary>
        /// Retrieves transaction data for a given transaction number and optional store ID.
        /// </summary>
        /// <param name="transactionNumber">The transaction number to fetch.</param>
        /// <param name="storeId">Optional store ID for filtering.</param>
        /// <returns>The ReceiptData object.</returns>
        Task<ReceiptData> GetTransactionDataAsync(int transactionNumber, int? storeId = null);

        /// <summary>
        /// Retrieves the receipt template for a given store ID.
        /// </summary>
        /// <param name="storeId">The store ID.</param>
        /// <returns>The ReceiptTemplate object.</returns>
        Task<ReceiptTemplate> GetReceiptTemplateAsync(int storeId);

        /// <summary>
        /// Retrieves a list of all stores.
        /// </summary>
        /// <returns>List of StoreInfo objects.</returns>
        Task<List<StoreInfo>> GetStoresAsync();

        /// <summary>
        /// Adds a new receipt to the data store.
        /// </summary>
        /// <param name="receiptData">The receipt data to add.</param>
        /// <returns>The newly added receipt.</returns>
        Task<ReceiptData> AddReceiptAsync(ReceiptData receiptData);

        /// <summary>
        /// Updates an existing receipt in the data store.
        /// </summary>
        /// <param name="receiptData">The receipt data to update.</param>
        /// <returns>True if update was successful; otherwise, false.</returns>
        Task<bool> UpdateReceiptAsync(ReceiptData receiptData);

        /// <summary>
        /// Deletes a receipt by transaction number.
        /// </summary>
        /// <param name="transactionNumber">The transaction number of the receipt to delete.</param>
        /// <returns>True if deletion was successful; otherwise, false.</returns>
        Task<bool> DeleteReceiptAsync(int transactionNumber);
    }
}
