using ReceiptPrinter.Core.DataAccess.Interfaces;
using ReceiptPrinter.Core.Models;
using ReceiptPrinter.Logging;
using ReceiptPrinter.Logging.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Configuration;

namespace ReceiptPrinter.Core.DataAccess
{
    public class ReceiptRepository : IReceiptRepository
    {
        private readonly string _connectionString;
        private static readonly ILogger _logger = LogManager.GetLogger();
        private bool _disposed = false;

        public ReceiptRepository()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["receiptreprint.Properties.Settings.KINOTIConnectionString"].ConnectionString;
        }

        // Helper to try get ordinal safely
        private int? GetOrdinalSafe(IDataRecord reader, string name)
        {
            try
            {
                return reader.GetOrdinal(name);
            }
            catch (IndexOutOfRangeException)
            {
                return null;
            }
        }

        public async Task<ReceiptData> GetTransactionDataAsync(int transactionNumber, int? storeId = null)
        {
            _logger.Debug($"Fetching transaction data for TransactionNumber: {transactionNumber}, StoreID: {storeId ?? (int?)null}");
            try
            {
                var parameters = new List<SqlParameter> { new SqlParameter("@TransactionNumber", transactionNumber) };
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var cmd = new SqlCommand("[dbo].[RECEIPTPRINT]", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddRange(parameters.ToArray());
                        using (var reader = await cmd.ExecuteReaderAsync(CommandBehavior.Default))
                        {
                            if (!reader.HasRows)
                            {
                                _logger.Info("Stored procedure returned no rows.");
                                return null;
                            }

                            // Read first row (could be header row or first item row)
                            if (!await reader.ReadAsync())
                            {
                                _logger.Info("No data after ReadAsync()");
                                return null;
                            }

                            // safe ordinals
                            int? ordTime = GetOrdinalSafe(reader, "Time");
                            int? ordName = GetOrdinalSafe(reader, "Name");
                            int? ordRegister = GetOrdinalSafe(reader, "Register");
                            int? ordGrandTotal = GetOrdinalSafe(reader, "GrandTotal");
                            int? ordStoreId = GetOrdinalSafe(reader, "StoreID");
                            int? ordItemLookup = GetOrdinalSafe(reader, "ItemLookupCode");
                            int? ordDescription = GetOrdinalSafe(reader, "Description");
                            int? ordQuantity = GetOrdinalSafe(reader, "quantity");
                            int? ordPrice = GetOrdinalSafe(reader, "Price");
                            int? ordSalesTax = GetOrdinalSafe(reader, "SalesTax");

                            // Read header values (prefer explicit header fields if present)
                            DateTime transactionTime = ordTime.HasValue && !reader.IsDBNull(ordTime.Value)
                                ? reader.GetDateTime(ordTime.Value)
                                : DateTime.MinValue;

                            string cashierName = ordName.HasValue && !reader.IsDBNull(ordName.Value)
                                ? reader.GetString(ordName.Value)
                                : string.Empty;

                            string registerNumber = ordRegister.HasValue && !reader.IsDBNull(ordRegister.Value)
                                ? reader[ordRegister.Value].ToString()
                                : string.Empty;

                            decimal grandTotal = ordGrandTotal.HasValue && !reader.IsDBNull(ordGrandTotal.Value)
                                ? Convert.ToDecimal(reader.GetValue(ordGrandTotal.Value))
                                : 0m;

                            int storeIdFromRow = 0;
                            if (ordStoreId.HasValue && !reader.IsDBNull(ordStoreId.Value))
                            {
                                // reader.GetInt32 may fail if underlying type differs; use Convert.ToInt32 for safety
                                storeIdFromRow = Convert.ToInt32(reader.GetValue(ordStoreId.Value));
                            }

                            // We'll build items list. Two possible shapes:
                            // A) First resultset is items (each row has ItemLookupCode) -> current row is an item
                            // B) First resultset is header only -> current row is header; items are in next resultset
                            var items = new List<TransactionEntry>();

                            if (ordItemLookup.HasValue)
                            {
                                // Shape A: current row contains item columns -> add it as first item
                                if (!reader.IsDBNull(ordItemLookup.Value))
                                {
                                    // read item values from current row (guard each column)
                                    var itemLookupCode = ordItemLookup.HasValue && !reader.IsDBNull(ordItemLookup.Value)
                                        ? reader.GetString(ordItemLookup.Value)
                                        : string.Empty;

                                    var description = ordDescription.HasValue && !reader.IsDBNull(ordDescription.Value)
                                        ? reader.GetString(ordDescription.Value)
                                        : string.Empty;

                                    var quantity = ordQuantity.HasValue && !reader.IsDBNull(ordQuantity.Value)
                                        ? Convert.ToInt32(reader.GetValue(ordQuantity.Value))
                                        : 0;

                                    var price = ordPrice.HasValue && !reader.IsDBNull(ordPrice.Value)
                                        ? Convert.ToDecimal(reader.GetValue(ordPrice.Value))
                                        : 0m;

                                    var salesTax = ordSalesTax.HasValue && !reader.IsDBNull(ordSalesTax.Value)
                                        ? Convert.ToDecimal(reader.GetValue(ordSalesTax.Value))
                                        : 0m;

                                    items.Add(new TransactionEntry(itemLookupCode, description, quantity, price, salesTax));
                                }

                                // read remaining rows in this resultset (other items)
                                while (await reader.ReadAsync())
                                {
                                    var itemLookupCode = ordItemLookup.HasValue && !reader.IsDBNull(ordItemLookup.Value)
                                        ? reader.GetString(ordItemLookup.Value)
                                        : string.Empty;

                                    var description = ordDescription.HasValue && !reader.IsDBNull(ordDescription.Value)
                                        ? reader.GetString(ordDescription.Value)
                                        : string.Empty;

                                    var quantity = ordQuantity.HasValue && !reader.IsDBNull(ordQuantity.Value)
                                        ? Convert.ToInt32(reader.GetValue(ordQuantity.Value))
                                        : 0;

                                    var price = ordPrice.HasValue && !reader.IsDBNull(ordPrice.Value)
                                        ? Convert.ToDecimal(reader.GetValue(ordPrice.Value))
                                        : 0m;

                                    var salesTax = ordSalesTax.HasValue && !reader.IsDBNull(ordSalesTax.Value)
                                        ? Convert.ToDecimal(reader.GetValue(ordSalesTax.Value))
                                        : 0m;

                                    items.Add(new TransactionEntry(itemLookupCode, description, quantity, price, salesTax));
                                }

                                // safe: also check if there is a second resultset with additional items; unlikely but handle it
                                if (await reader.NextResultAsync())
                                {
                                    while (await reader.ReadAsync())
                                    {
                                        var altItemLookupIdx = GetOrdinalSafe(reader, "ItemLookupCode");
                                        var altDescIdx = GetOrdinalSafe(reader, "Description");
                                        var altQtyIdx = GetOrdinalSafe(reader, "quantity");
                                        var altPriceIdx = GetOrdinalSafe(reader, "Price");
                                        var altTaxIdx = GetOrdinalSafe(reader, "SalesTax");

                                        var itemLookupCode = altItemLookupIdx.HasValue && !reader.IsDBNull(altItemLookupIdx.Value)
                                            ? reader.GetString(altItemLookupIdx.Value)
                                            : string.Empty;

                                        var description = altDescIdx.HasValue && !reader.IsDBNull(altDescIdx.Value)
                                            ? reader.GetString(altDescIdx.Value)
                                            : string.Empty;

                                        var quantity = altQtyIdx.HasValue && !reader.IsDBNull(altQtyIdx.Value)
                                            ? Convert.ToInt32(reader.GetValue(altQtyIdx.Value))
                                            : 0;

                                        var price = altPriceIdx.HasValue && !reader.IsDBNull(altPriceIdx.Value)
                                            ? Convert.ToDecimal(reader.GetValue(altPriceIdx.Value))
                                            : 0m;

                                        var salesTax = altTaxIdx.HasValue && !reader.IsDBNull(altTaxIdx.Value)
                                            ? Convert.ToDecimal(reader.GetValue(altTaxIdx.Value))
                                            : 0m;

                                        items.Add(new TransactionEntry(itemLookupCode, description, quantity, price, salesTax));
                                    }
                                }
                            }
                            else
                            {
                                // Shape B: First resultset is header; current row contains header values already read above.
                                // Move to next resultset which should contain items.
                                if (await reader.NextResultAsync())
                                {
                                    while (await reader.ReadAsync())
                                    {
                                        var itemLookupIdx = GetOrdinalSafe(reader, "ItemLookupCode");
                                        var descIdx = GetOrdinalSafe(reader, "Description");
                                        var qtyIdx = GetOrdinalSafe(reader, "quantity");
                                        var priceIdx = GetOrdinalSafe(reader, "Price");
                                        var taxIdx = GetOrdinalSafe(reader, "SalesTax");

                                        var itemLookupCode = itemLookupIdx.HasValue && !reader.IsDBNull(itemLookupIdx.Value)
                                            ? reader.GetString(itemLookupIdx.Value)
                                            : string.Empty;

                                        var description = descIdx.HasValue && !reader.IsDBNull(descIdx.Value)
                                            ? reader.GetString(descIdx.Value)
                                            : string.Empty;

                                        var quantity = qtyIdx.HasValue && !reader.IsDBNull(qtyIdx.Value)
                                            ? Convert.ToInt32(reader.GetValue(qtyIdx.Value))
                                            : 0;

                                        var price = priceIdx.HasValue && !reader.IsDBNull(priceIdx.Value)
                                            ? Convert.ToDecimal(reader.GetValue(priceIdx.Value))
                                            : 0m;

                                        var salesTax = taxIdx.HasValue && !reader.IsDBNull(taxIdx.Value)
                                            ? Convert.ToDecimal(reader.GetValue(taxIdx.Value))
                                            : 0m;

                                        items.Add(new TransactionEntry(itemLookupCode, description, quantity, price, salesTax));
                                    }
                                }
                                else
                                {
                                    _logger.Warning("Expected items in second result set but no resultset found.");
                                }
                            }

                            // Determine final StoreID: prefer storeIdFromRow (if returned) else query DB
                            int finalStoreId = storeIdFromRow;
                            if (finalStoreId == 0)
                            {
                                // query transaction table for store id fallback
                                using (var txCmd = new SqlCommand("SELECT StoreID FROM dbo.[Transaction] WHERE TransactionNumber = @txn", connection))
                                {
                                    txCmd.Parameters.Add(new SqlParameter("@txn", transactionNumber));
                                    var obj = await txCmd.ExecuteScalarAsync();
                                    if (obj != null && obj != DBNull.Value)
                                    {
                                        finalStoreId = Convert.ToInt32(obj);
                                    }
                                }
                            }

                            // Create ReceiptData using new ctor (transactionNumber, transactionTime, cashierName, registerNumber, storeId, grandTotal, companyInfo)
                            var receiptData = new ReceiptData(
                                transactionNumber,
                                transactionTime,
                                cashierName,
                                registerNumber,
                                finalStoreId,
                                grandTotal,
                                null // CompanyInfo will be filled by service/template parser
                            );

                            // Attach items
                            if (items.Count > 0)
                            {
                                receiptData.Items.AddRange(items);
                            }

                            return receiptData;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error fetching transaction data for TransactionNumber: {transactionNumber}", ex);
                throw;
            }
        }

        public async Task<ReceiptTemplate> GetReceiptTemplateAsync(int storeId)
        {
            _logger.Debug($"Fetching receipt template for StoreID: {storeId}");
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    // Use fallback behaviour in SQL to return store template or default (store 0)
                    using (var cmd = new SqlCommand(@"
SELECT TOP (1) ID, Title, TemplateSale, StoreID
FROM [dbo].[Receipt]
WHERE StoreID = @StoreID OR StoreID = 0
ORDER BY CASE WHEN StoreID = @StoreID THEN 0 ELSE 1 END;
", connection))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.Add(new SqlParameter("@StoreID", storeId));
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return new ReceiptTemplate
                                {
                                    ID = reader.GetInt32(reader.GetOrdinal("ID")),
                                    Title = reader.IsDBNull(reader.GetOrdinal("Title")) ? string.Empty : reader.GetString(reader.GetOrdinal("Title")),
                                    TemplateSale = reader.IsDBNull(reader.GetOrdinal("TemplateSale")) ? string.Empty : reader.GetString(reader.GetOrdinal("TemplateSale")),
                                    StoreID = reader.GetInt32(reader.GetOrdinal("StoreID"))
                                };
                            }
                            _logger.Warning($"No template found for StoreID: {storeId}");
                            return null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error fetching receipt template for StoreID: {storeId}", ex);
                throw new DataException("Failed to retrieve receipt template.", ex);
            }
        }

        public async Task<List<StoreInfo>> GetStoresAsync()
        {
            _logger.Debug("Fetching all stores");
            try
            {
                var stores = new List<StoreInfo>();
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var cmd = new SqlCommand("SELECT ID, Name FROM [dbo].[Store] ORDER BY Name", connection))
                    {
                        cmd.CommandType = CommandType.Text;
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                stores.Add(new StoreInfo
                                {
                                    ID = reader.GetInt32(reader.GetOrdinal("ID")),
                                    Name = reader.GetString(reader.GetOrdinal("Name"))
                                });
                            }
                        }
                    }
                }
                _logger.Info($"Retrieved {stores.Count} stores");
                return stores;
            }
            catch (Exception ex)
            {
                _logger.Error("Error fetching stores", ex);
                throw new DataException("Failed to retrieve stores.", ex);
            }
        }

        public Task<ReceiptData> AddReceiptAsync(ReceiptData receiptData)
        {
            throw new NotImplementedException("AddReceiptAsync method not implemented.");
        }

        public Task<bool> UpdateReceiptAsync(ReceiptData receiptData)
        {
            throw new NotImplementedException("UpdateReceiptAsync method not implemented.");
        }

        public Task<bool> DeleteReceiptAsync(int transactionNumber)
        {
            throw new NotImplementedException("DeleteReceiptAsync method not implemented.");
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                // Dispose of any resources here if needed
                _disposed = true;
            }
        }
    }
}
