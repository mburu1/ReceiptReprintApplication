using ReceiptPrinter.Core.Utilities;
using ReceiptPrinter.Logging;
using ReceiptPrinter.Logging.Interfaces;
using System;
using System.Windows.Forms;

namespace ReceiptPrinter.UI.Helpers
{
    /// <summary>
    /// Helper class for validating form inputs.
    /// Centralizes UI validation logic.
    /// </summary>
    public static class FormValidator
    {
        private static readonly ILogger _logger = LogManager.GetLogger();

        /// <summary>
        /// Validates the transaction number input.
        /// </summary>
        /// <param name="txtTransactionNumber">The TextBox control.</param>
        /// <returns>True if valid; otherwise, false.</returns>
        public static bool ValidateTransactionNumber(TextBox txtTransactionNumber)
        {
            if (int.TryParse(txtTransactionNumber.Text, out int number) && ValidationHelper.IsValidTransactionNumber(number))
            {
                return true;
            }

            _logger.Warning("Invalid transaction number input.");
            UIMessageHelper.ShowError("Please enter a valid transaction number (positive integer).");
            txtTransactionNumber.Focus();
            return false;
        }

        /// <summary>
        /// Validates the printer selection.
        /// </summary>
        /// <param name="cmbPrinters">The ComboBox control.</param>
        /// <returns>True if valid; otherwise, false.</returns>
        public static bool ValidatePrinterSelection(ComboBox cmbPrinters)
        {
            if (!string.IsNullOrEmpty(cmbPrinters.SelectedItem as string))
            {
                return true;
            }

            _logger.Warning("No printer selected.");
            UIMessageHelper.ShowError("Please select a printer.");
            cmbPrinters.Focus();
            return false;
        }

        /// <summary>
        /// Validates the font size input.
        /// </summary>
        /// <param name="numFontSize">The NumericUpDown control.</param>
        /// <returns>True if valid; otherwise, false.</returns>
        public static bool ValidateFontSize(NumericUpDown numFontSize)
        {
            int size = (int)numFontSize.Value;
            if (ValidationHelper.IsValidFontSize(size))
            {
                return true;
            }

            _logger.Warning("Invalid font size.");
            UIMessageHelper.ShowError("Font size must be between 6 and 20.");
            numFontSize.Focus();
            return false;
        }

        /// <summary>
        /// Validates the store selection based on checkbox state.
        /// </summary>
        /// <param name="chkAllStores">The CheckBox for all stores.</param>
        /// <param name="cmbStores">The ComboBox for stores.</param>
        /// <returns>True if valid; otherwise, false.</returns>
        public static bool ValidateStoreSelection(CheckBox chkAllStores, ComboBox cmbStores)
        {
            if (chkAllStores.Checked || cmbStores.SelectedItem != null)
            {
                return true;
            }

            _logger.Warning("No store selected.");
            UIMessageHelper.ShowError("Please select a store or check 'All Stores'.");
            cmbStores.Focus();
            return false;
        }
    }
}