using ReceiptPrinter.Logging;
using ReceiptPrinter.Logging.Interfaces;
using System.Windows.Forms;

namespace ReceiptPrinter.UI.Helpers
{
    /// <summary>
    /// Helper class for displaying UI messages.
    /// Centralizes message box operations.
    /// </summary>
    public static class UIMessageHelper
    {
        private static readonly ILogger _logger = LogManager.GetLogger();

        /// <summary>
        /// Shows an information message.
        /// </summary>
        /// <param name="message">The message text.</param>
        public static void ShowInfo(string message)
        {
            _logger.Info($"Showing info: {message}");
            MessageBox.Show(message, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Shows an error message.
        /// </summary>
        /// <param name="message">The message text.</param>
        public static void ShowError(string message)
        {
            _logger.Error($"Showing error: {message}");
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// Shows a warning message.
        /// </summary>
        /// <param name="message">The message text.</param>
        public static void ShowWarning(string message)
        {
            _logger.Warning($"Showing warning: {message}");
            MessageBox.Show(message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        /// <summary>
        /// Shows a yes/no question.
        /// </summary>
        /// <param name="message">The question text.</param>
        /// <returns>True if yes; otherwise, false.</returns>
        public static bool AskYesNo(string message)
        {
            _logger.Debug($"Asking yes/no: {message}");
            return MessageBox.Show(message, "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }
    }
}