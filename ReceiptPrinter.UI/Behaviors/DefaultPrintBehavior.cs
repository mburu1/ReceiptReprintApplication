using ReceiptPrinter.Core.Models;
using ReceiptPrinter.Printing.Behaviors;
using ReceiptPrinter.UI.Presenters;
using System.Windows.Forms;

namespace ReceiptPrinter.Printing.Behaviors
{
    /// <summary>
    /// Default print behavior - allows printing only when receipt exists
    /// </summary>
    public class DefaultPrintBehavior : IPrintBehavior
    {
        public bool CanEnablePrintButton(ReceiptData receiptData)
        {
            // Enable print button only if receipt data exists
            return receiptData != null;
        }

        public void OnPrintClicked(MainFormPresenter presenter)
        {
            if (presenter.HasReceipt)
            {
                // Print the current receipt
                presenter.PrintReceipt();
            }
            else
            {
                // Show error - no receipt to print
                MessageBox.Show(
                    "No receipt found. Please generate a receipt first before printing.",
                    "Cannot Print",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
    }
}