using System;

namespace ReceiptPrinter.Core.Models
{
    /// <summary>
    /// Represents a single item entry in a transaction.
    /// Immutable value object in the domain model.
    /// </summary>
    public class TransactionEntry
    {
        public string ItemLookupCode { get; }
        public string Description { get; }
        public decimal Quantity { get; }
        public decimal Price { get; }
        public decimal SalesTax { get; }
        public bool IsDuplicate { get; private set; }

        public decimal Subtotal => Quantity * Price;
        public decimal Total => Subtotal + SalesTax;
        public string FormattedDescription => IsDuplicate ? $"{Description} (Duplicate)" : Description;

        public TransactionEntry(
            string itemLookupCode,
            string description,
            decimal quantity,
            decimal price,
            decimal salesTax)
        {
            if (string.IsNullOrWhiteSpace(itemLookupCode))
                throw new ArgumentException("Item lookup code is required.", nameof(itemLookupCode));

            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Description is required.", nameof(description));

            if (quantity <= 0)
                throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");

            if (price < 0)
                throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative.");

            if (salesTax < 0)
                throw new ArgumentOutOfRangeException(nameof(salesTax), "Sales tax cannot be negative.");

            ItemLookupCode = itemLookupCode;
            Description = description;
            Quantity = quantity;
            Price = price;
            SalesTax = salesTax;
            IsDuplicate = false;
        }

        public void MarkAsDuplicate()
        {
            IsDuplicate = true;
        }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(ItemLookupCode) &&
                   !string.IsNullOrWhiteSpace(Description) &&
                   Quantity > 0 &&
                   Price >= 0 &&
                   SalesTax >= 0;
        }
    }
}