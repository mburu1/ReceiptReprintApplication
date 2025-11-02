namespace ReceiptPrinter.Core.Models
{
    /// <summary>
    /// Represents store information for filtering transactions.
    /// </summary>
    public class StoreInfo
    {
        /// <summary>
        /// Unique identifier for the store.
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Name of the store.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Validates the store info.
        /// </summary>
        /// <returns>True if valid; otherwise, false.</returns>
        public bool IsValid()
        {
            return ID > 0 && !string.IsNullOrWhiteSpace(Name);
        }

        /// <summary>
        /// Overrides ToString for UI binding (e.g., ComboBox display).
        /// Format: "StoreName (ID)"
        /// </summary>
        public override string ToString()
        {
            return $"{Name} ({ID})";
        }
    }
}