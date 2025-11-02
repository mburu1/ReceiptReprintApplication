// ReceiptPrinter.Printing/Behaviors/AllowEmptyPrintBehavior.cs
using ReceiptPrinter.Core.Models;
using ReceiptPrinter.UI.Presenters;
using System.Windows.Forms;

namespace ReceiptPrinter.Printing.Behaviors
{
    /// <summary>
    /// Allows printing empty/test receipts when no receipt is loaded
    /// </summary>
    public class AllowEmptyPrintBehavior : IPrintBehavior
    {
        public bool CanEnablePrintButton(ReceiptData receiptData)
        {
            // Always enable print button
            return true;
        }

        public void OnPrintClicked(MainFormPresenter presenter)
        {
            if (presenter.HasReceipt)
            {
                presenter.PrintReceipt();
            }
            else
            {
                // Ask user if they want to print empty receipt
                var result = MessageBox.Show(
                    "No receipt has been generated.\n\nDo you want to print a blank test receipt?",
                    "Print Empty Receipt?",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    presenter.PrintEmptyReceipt();
                }
            }
        }
    }
}