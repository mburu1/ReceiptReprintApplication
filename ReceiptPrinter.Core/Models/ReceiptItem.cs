using System;

public class ReceiptItem
{
    public string ItemLookupCode { get; set; }
    public string Description { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Total { get; set; } // Quantity * Price
    public decimal Subtotal { get; set; } // Total - SalesTax
    public decimal SalesTax { get; set; }
    public decimal GrandTotal { get; set; }
    public DateTime Time { get; set; }
    public string Register { get; set; }
    public int TransactionNumber { get; set; }
    public string CashierName { get; set; }
}