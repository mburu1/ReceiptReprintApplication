using ReceiptPrinter.Core.Models;
using System.Threading.Tasks;

namespace ReceiptPrinter.Core.Services.Interfaces
{
    /// <summary>
    /// Defines the contract for parsing receipt templates.
    /// Separates template parsing concern in DDD.
    /// </summary>
    public interface ITemplateParser
    {
        /// <summary>
        /// Parses a template string into a CompanyHeader.
        /// </summary>
        /// <param name="template">The template content.</param>
        /// <returns>Parsed CompanyHeader.</returns>
        CompanyHeader ParseTemplate(string template);

        /// <summary>
        /// Retrieves the template for a store.
        /// </summary>
        /// <param name="storeId">The store ID.</param>
        /// <returns>ReceiptTemplate or null.</returns>
        Task<ReceiptTemplate> GetTemplateAsync(int storeId);

        /// <summary>
        /// Formats the receipt string using data and template.
        /// </summary>
        /// <param name="data">The ReceiptData.</param>
        /// <param name="template">The template.</param>
        /// <returns>Formatted receipt string.</returns>
        string FormatReceipt(ReceiptData data, ReceiptTemplate template);
    }
}