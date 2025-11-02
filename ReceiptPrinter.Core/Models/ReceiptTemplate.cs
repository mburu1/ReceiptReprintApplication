namespace ReceiptPrinter.Core.Models
{
    /// <summary>
    /// Represents a receipt template stored in the database.
    /// Used for parsing company-specific formatting.
    /// </summary>
    public class ReceiptTemplate
    {
        /// <summary>
        /// Unique identifier for the template.
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Title of the template.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Sale template content with placeholders.
        /// </summary>
        public string TemplateSale { get; set; }

        /// <summary>
        /// Associated store ID.
        /// </summary>
        public int StoreID { get; set; }

        /// <summary>
        /// Validates the template for required fields.
        /// </summary>
        /// <returns>True if valid; otherwise, false.</returns>
        public bool IsValid()
        {
            return ID > 0 &&
                   !string.IsNullOrEmpty(Title) &&
                   !string.IsNullOrEmpty(TemplateSale) &&
                   StoreID > 0;
        }
    }
}