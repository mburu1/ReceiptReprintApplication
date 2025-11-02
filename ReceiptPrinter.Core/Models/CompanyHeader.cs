namespace ReceiptPrinter.Core.Models
{
    /// <summary>
    /// Represents the company header information used in receipt formatting.
    /// Serves as a value object in the domain.
    /// </summary>
    public class CompanyHeader
    {
        /// <summary>
        /// Name of the company.
        /// </summary>
        public string CompanyName { get; set; }

        /// <summary>
        /// First line of the address.
        /// </summary>
        public string Address1 { get; set; }

        /// <summary>
        /// PIN number for tax purposes.
        /// </summary>
        public string PinNumber { get; set; }

        /// <summary>
        /// Phone number with branch identifier.
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Country of operation.
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// Creates a default hardcoded header as fallback.
        /// </summary>
        /// <returns>A default CompanyHeader instance.</returns>
        public static CompanyHeader Default()
        {
            return new CompanyHeader
            {
                CompanyName = "Sleaklady cosmetics ltd",
                Address1 = "Bestlady",
                PinNumber = "Pin No: P051681112N",
                PhoneNumber = "0740470002-HQ",
                Country = "Kenya"
            };
        }

        /// <summary>
        /// Validates that all required fields are populated.
        /// </summary>
        /// <returns>True if valid; otherwise, false.</returns>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(CompanyName) &&
                   !string.IsNullOrEmpty(Address1) &&
                   !string.IsNullOrEmpty(PinNumber) &&
                   !string.IsNullOrEmpty(PhoneNumber) &&
                   !string.IsNullOrEmpty(Country);
        }
    }
}