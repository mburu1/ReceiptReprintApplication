using System;
using System.Globalization;

namespace ReceiptPrinter.Core.Utilities
{
    /// <summary>
    /// Utility class for formatting DateTime objects according to receipt specifications.
    /// Ensures consistent formatting across the domain.
    /// </summary>
    public static class DateTimeFormatter
    {
        private const string ReceiptDateFormat = "d/MM/yyyy h:mm:ss tt";

        /// <summary>
        /// Formats a DateTime to the receipt-specific string format.
        /// Example: "9/22/2025 2:36:15 PM"
        /// </summary>
        /// <param name="dateTime">The DateTime to format.</param>
        /// <returns>Formatted string.</returns>
        public static string FormatForReceipt(DateTime dateTime)
        {
            return dateTime.ToString(ReceiptDateFormat, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Parses a string back to DateTime using the receipt format.
        /// </summary>
        /// <param name="dateString">The string to parse.</param>
        /// <returns>Parsed DateTime.</returns>
        /// <exception cref="FormatException">Thrown if parsing fails.</exception>
        public static DateTime ParseFromReceipt(string dateString)
        {
            if (DateTime.TryParseExact(dateString, ReceiptDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
            {
                return result;
            }
            throw new FormatException($"Invalid date format: {dateString}. Expected: {ReceiptDateFormat}");
        }
    }
}